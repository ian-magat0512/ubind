// <copyright file="ExportController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers.Quoter
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application.Authorisation;
    using UBind.Application.Export;
    using UBind.Application.Queries.Policy;
    using UBind.Application.Queries.User;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel;
    using UBind.Web.Extensions;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels;
    using UBind.Web.ResourceModels.Quote;

    /// <summary>
    /// Controller for managing periodic exports of data.
    /// </summary>
    [Route("api/v1/{environment}/export")]
    public class ExportController : Controller
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IQuoteReadModelRepository quoteReadModelRepository;
        private readonly ICqrsMediator mediator;
        private readonly IAuthorisationService authorisationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportController"/> class.
        /// </summary>
        /// <param name="quoteReadModelRepository">Quote read model repository.</param>
        /// <param name="authorisationService">The authorisation service.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        public ExportController(
            IQuoteReadModelRepository quoteReadModelRepository,
            IAuthorisationService authorisationService,
            ICqrsMediator mediator,
            ICachingResolver cachingResolver)
        {
            Contract.Assert(quoteReadModelRepository != null);

            this.cachingResolver = cachingResolver;
            this.quoteReadModelRepository = quoteReadModelRepository;
            this.authorisationService = authorisationService;
            this.mediator = mediator;
        }

        /// <summary>
        /// export quote into csv file.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant.</param>
        /// <param name="product">The ID or Alias of the product to serve the app for.</param>
        /// <param name="environment">The environment to use.</param>
        /// <param name="model">Export model.</param>
        /// <returns>An awaitable task.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 20, Type = RateLimitPeriodType.Seconds, Limit = 10)]
        [ValidateModel]
        [MustHavePermission(Permission.ExportQuotes)]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ExportQuote(string tenant, string product, string environment, ExportModel model)
        {
            var isSuccess = Enum.TryParse(environment, true, out DeploymentEnvironment env);
            if (!isSuccess)
            {
                return Errors.General.NotFound("environment", environment).ToProblemJsonResult();
            }

            if (string.IsNullOrEmpty(product))
            {
                return Errors.General.NotFound("product", product).ToProblemJsonResult();
            }

            if (model.ExportType != ExportType.Application)
            {
                throw new NotSupportedException("Only application exports are supported.");
            }

            var filter = new EntityListFilters();

            if (!string.IsNullOrEmpty(model.From))
            {
                filter.WithDateIsAfterFilter(model.From.ToLocalDateFromIso8601());
            }

            if (!string.IsNullOrEmpty(model.To))
            {
                filter.WithDateIsBeforeFilter(model.To.ToLocalDateFromIso8601());
            }

            var productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));

            IEnumerable<IQuoteReadModelSummary> quotes
                = model.FilterBy == FilterBy.None ? this.GetAllQuotes(productModel.TenantId, productModel.Id, env, filter)
                : model.FilterBy == FilterBy.HasPolicy ? this.GetQuotesWithPolicy(productModel.TenantId, productModel.Id, env, filter)
                : model.FilterBy == FilterBy.HasInvoice ? this.GetQuotesWithInvoice(productModel.TenantId, productModel.Id, env, filter)
                : model.FilterBy == FilterBy.HasPayment ? this.GetQuotesWithPayment(productModel.TenantId, productModel.Id, env, filter)
                : model.FilterBy == FilterBy.HasSubmission ? this.GetQuotesWithSubmission(productModel.TenantId, productModel.Id, env, filter)
                : throw new NotSupportedException($"The following export filter is not yet supported: {model.FilterBy}.");

            IEnumerable<QuoteExportModel> quoteExport = quotes.Select(q => new QuoteExportModel(q));

            // TODO: Is there a better export format we should be using?
            // Where is the spec for this?
            if (model.OutputFormat == OutputFormat.JSON)
            {
                return this.Ok(quoteExport);
            }
            else if (model.OutputFormat == OutputFormat.CSV)
            {
                var fileContents = this.ToCsv(quoteExport);
                return this.File(fileContents, "text/csv", "export.csv");
            }
            else
            {
                throw new NotSupportedException("Only Json or CSV output is supported.");
            }
        }

        /// <summary>
        /// export policy into csv file.
        /// </summary>
        /// <param name="format">The download format to be return.</param>
        /// <param name="options">Optional filter parameters for obtaining the list to export.</param>
        /// <returns>Blob file.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 20, Type = RateLimitPeriodType.Seconds, Limit = 10)]
        [MustBeLoggedIn]
        [MustHavePermission(Permission.ExportPolicies)]
        [Route("policy")]
        [ProducesResponseType(typeof(IEnumerable<IPolicyReadModelSummary>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPolicyExport(OutputFormat format, [FromQuery] PolicyQueryOptionsModel options)
        {
            await this.authorisationService.CheckAndStandardiseOptions(this.User, options);
            var tenant = options.Tenant;
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            var organisationModel = await this.cachingResolver.GetOrganisationOrNull(tenantModel.Id, new GuidOrAlias(options.Organisation));
            options.Organisation =
                (await this.mediator.Send(new OrganisationIdForFilterQuery(organisationModel?.Id, this.User)))?.ToString();
            await this.authorisationService.ThrowIfUserNotInOrganisationOrDefaultOrganisation(
                this.User,
                organisationModel?.Id,
                tenantModel?.Id);

            PolicyReadModelFilters filters = await options.ToFilters(tenantModel.Id, this.cachingResolver);
            filters.PageSize = 100000;
            filters.Page = 1;
            await this.authorisationService.ApplyViewPolicyRestrictionsToFilters(this.User, filters);
            IEnumerable<IPolicyReadModelSummary> policyReadModels = await this.mediator.Send(
                new PolicyExportReadModelSummariesQuery(tenantModel.Id, filters));
            if (format.Equals(OutputFormat.CSV))
            {
                var fileContents = this.ToCsv(policyReadModels);
                return this.File(fileContents, "text/csv", "export.csv");
            }
            else
            {
                return this.Ok(policyReadModels);
            }
        }

        private IEnumerable<IQuoteReadModelSummary> GetAllQuotes(
            Guid tenantId, Guid productId, DeploymentEnvironment environment, EntityListFilters filter)
        {
            return this.quoteReadModelRepository.GetQuotesByTenantAndProduct(tenantId, productId, environment, filter);
        }

        private IEnumerable<IQuoteReadModelSummary> GetQuotesWithSubmission(
            Guid tenantId, Guid productId, DeploymentEnvironment environment, EntityListFilters filter)
        {
            return this.quoteReadModelRepository
                .GetQuotesByTenantAndProduct(tenantId, productId, environment, filter).Where(q => q.IsSubmitted);
        }

        private IEnumerable<IQuoteReadModelSummary> GetQuotesWithPolicy(
            Guid tenantId, Guid productId, DeploymentEnvironment environment, EntityListFilters filter)
        {
            return this.quoteReadModelRepository
                .GetQuotesByTenantAndProduct(tenantId, productId, environment, filter).Where(q => q.PolicyIssued);
        }

        private IEnumerable<IQuoteReadModelSummary> GetQuotesWithInvoice(
            Guid tenantId, Guid productId, DeploymentEnvironment environment, EntityListFilters filter)
        {
            return this.quoteReadModelRepository
                .GetQuotesByTenantAndProduct(tenantId, productId, environment, filter).Where(q => q.IsInvoiced);
        }

        private IEnumerable<IQuoteReadModelSummary> GetQuotesWithPayment(
            Guid tenantId, Guid productId, DeploymentEnvironment environment, EntityListFilters filter)
        {
            return this.quoteReadModelRepository
                .GetQuotesByTenantAndProduct(tenantId, productId, environment, filter)
                .Where(q => q.IsPaidFor);
        }

        private byte[] ToCsv(IEnumerable<QuoteExportModel> quotes)
        {
            var table = JsonTabulator.TabulateCsv(quotes);
            return new UTF8Encoding().GetBytes(table);
        }

        private byte[] ToCsv(IEnumerable<IPolicyReadModelSummary> policyReadModelSummaries)
        {
            var table = JsonTabulator.TabulateCsv(policyReadModelSummaries);
            return new UTF8Encoding().GetBytes(table);
        }
    }
}
