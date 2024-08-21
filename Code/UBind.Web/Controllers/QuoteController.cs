// <copyright file="QuoteController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application;
    using UBind.Application.Authorisation;
    using UBind.Application.Commands.Policy;
    using UBind.Application.Commands.Quote;
    using UBind.Application.Dashboard.Model;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Helpers;
    using UBind.Application.Queries.ProductConfiguration;
    using UBind.Application.Queries.ProductRelease;
    using UBind.Application.Queries.Quote;
    using UBind.Application.Queries.QuoteVersions;
    using UBind.Application.Services;
    using UBind.Application.Services.Search;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.CustomPipelines.BindPolicy;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.Search;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using UBind.Domain.ValueTypes;
    using UBind.Web.Filters;
    using UBind.Web.Mapping;
    using UBind.Web.ResourceModels;
    using UBind.Web.ResourceModels.Policy;
    using UBind.Web.ResourceModels.Quote;

    /// <summary>
    /// Controller for handling portal-related quote requests.
    /// </summary>
    [Produces("application/json")]
    [Route("/api/v1/quote")]
    public class QuoteController : Controller
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IQuoteService quoteService;
        private readonly IApplicationQuoteService applicationQuoteService;
        private readonly IConfigurationService configurationService;
        private readonly ISearchableEntityService<IQuoteSearchResultItemReadModel, QuoteReadModelFilters> quoteSearchIndexService;
        private readonly IProductConfigurationProvider productConfigurationProvider;
        private readonly IFormDataPrettifier formDataPrettifier;
        private readonly Application.User.IUserService userService;
        private readonly IClock clock;
        private readonly ICqrsMediator mediator;
        private readonly IAuthorisationService authorisationService;
        private readonly IAdditionalPropertyValueService additionalPropertyValueService;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteController"/> class.
        /// </summary>
        /// <param name="quoteService">The service for handling quote-related functionalities.</param>
        /// <param name="applicationQuoteService">The application quote service.</param>
        /// <param name="configurationService">The service for obtaining product configuration.</param>
        /// <param name="quoteSearchIndexService">The service for searching quote indexes.</param>
        /// <param name="productConfigurationProvider">The product configuration provider.</param>
        /// <param name="formDataPrettifier">The form data prettifier.</param>
        /// <param name="userService">The user service of <see cref="Application.User"/>.</param>
        /// <param name="authorisationService">The authorisation service.</param>
        /// <param name="clock">The clock to retrieve current timestamp.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        /// <param name="mediator">The mediator object.</param>
        /// <param name="additionalPropertyValueService">Additional property value service.</param>
        public QuoteController(
            IQuoteService quoteService,
            IApplicationQuoteService applicationQuoteService,
            IConfigurationService configurationService,
            ISearchableEntityService<IQuoteSearchResultItemReadModel, QuoteReadModelFilters> quoteSearchIndexService,
            IProductConfigurationProvider productConfigurationProvider,
            IFormDataPrettifier formDataPrettifier,
            Application.User.IUserService userService,
            IAuthorisationService authorisationService,
            IClock clock,
            ICachingResolver cachingResolver,
            ICqrsMediator mediator,
            IAdditionalPropertyValueService additionalPropertyValueService)
        {
            this.cachingResolver = cachingResolver;
            this.quoteService = quoteService;
            this.applicationQuoteService = applicationQuoteService;
            this.configurationService = configurationService;
            this.quoteSearchIndexService = quoteSearchIndexService;
            this.productConfigurationProvider = productConfigurationProvider;
            this.formDataPrettifier = formDataPrettifier;
            this.userService = userService;
            this.authorisationService = authorisationService;
            this.clock = clock;
            this.additionalPropertyValueService = additionalPropertyValueService;
            this.mediator = mediator;
        }

        /// <summary>
        /// Gets the list of all the quotes periodic summary for dashboard.
        /// </summary>
        /// <param name="options">
        /// The required and optional filter options to be used on the request.
        /// </param>
        /// <returns>The latest collection of quotes summary available in the system.</returns>
        [HttpGet]
        [Route("periodic-summary")]
        [MustHaveUserType(UserType.Client)]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [MustHaveOneOfPermissions(Permission.ViewQuotes, Permission.ViewAllQuotes, Permission.ViewAllQuotesFromAllOrganisations)]
        [RequiresFeature(Feature.QuoteManagement)]
        [ProducesResponseType(typeof(IEnumerable<QuotePeriodicSummaryModel>), StatusCodes.Status200OK)]
        [SingleValueQueryParameter(
            nameof(BasePeriodicSummaryQueryOptionsModel.Environment),
            nameof(BasePeriodicSummaryQueryOptionsModel.SamplePeriodLength),
            nameof(BasePeriodicSummaryQueryOptionsModel.FromDateTime),
            nameof(BasePeriodicSummaryQueryOptionsModel.ToDateTime),
            nameof(BasePeriodicSummaryQueryOptionsModel.CustomSamplePeriodMinutes),
            nameof(BasePeriodicSummaryQueryOptionsModel.Timezone))]
        public async Task<IActionResult> GetPeriodicSummary([FromQuery] QuotePeriodicSummaryQueryOptionsModel options)
        {
            var tenantId = this.User.GetTenantId();
            options.ValidateQueryOptions();
            await this.authorisationService.CheckAndStandardiseOptions(this.User, options);
            QuoteReadModelFilters filters = await options.ToFilters(tenantId, this.cachingResolver);
            await this.authorisationService.ApplyViewQuoteRestrictionsToFilters(this.User, filters);
            await this.authorisationService.ApplyViewQuoteRestrictionsToFiltersForRideProtect(this.User, filters);
            var quoteSummaries = await this.mediator.Send(new GetQuotePeriodicSummariesQuery(tenantId, filters, options));
            if (options.SamplePeriodLength == SamplePeriodLength.All && quoteSummaries.Any())
            {
                return this.Ok(quoteSummaries.First());
            }

            return this.Ok(quoteSummaries);
        }

        /// <summary>
        /// Gets the list of all the quotes in the system.
        /// </summary>
        /// <param name="options">
        /// The additional filter options to be used on the request, otherwise all quotes are returned.
        /// </param>
        /// <returns>The latest collection of quotes available in the system.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(Permission.ViewQuotes, Permission.ViewAllQuotes, Permission.ViewAllQuotesFromAllOrganisations)]
        [RequiresFeature(Feature.QuoteManagement)]
        [ProducesResponseType(typeof(IEnumerable<QuoteSetModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetList([FromQuery] QuoteQueryOptionsModel options)
        {
            var tenantId = this.User.GetTenantId();
            await this.authorisationService.CheckAndStandardiseOptions(this.User, options, true);
            options.ExcludedStatuses = options.ExcludedStatuses ?? new List<string>() { StandardQuoteStates.Nascent };
            QuoteReadModelFilters filters = await options.ToFilters(this.User.GetTenantId(), this.cachingResolver);
            await this.authorisationService.ApplyViewQuoteRestrictionsToFilters(this.User, filters);
            await this.authorisationService.ApplyViewQuoteRestrictionsToFiltersForRideProtect(this.User, filters);
            var results = await this.mediator.Send(new SearchQuotesIndexQuery(tenantId, filters.Environment.Value, filters));
            var quoteSetModels = results.Select(q => new QuoteSetModel(q, this.clock.Now()));
            return this.Ok(quoteSetModels);
        }

        /// <summary>
        /// Gets the count of all matching quotes.
        /// </summary>
        /// <param name="options">
        /// The additional filter options to be used on the request, otherwise the count of all quotes are returned.
        /// </param>
        /// <returns>The number of quotes.</returns>
        [HttpGet]
        [Route("count")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(Permission.ViewQuotes, Permission.ViewAllQuotes, Permission.ViewAllQuotesFromAllOrganisations)]
        [RequiresFeature(Feature.QuoteManagement)]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCount([FromQuery] QuoteQueryOptionsModel options)
        {
            var tenantId = this.User.GetTenantId();
            await this.authorisationService.CheckAndStandardiseOptions(this.User, options);
            options.ExcludedStatuses = options.ExcludedStatuses ?? new List<string>() { StandardQuoteStates.Nascent };
            QuoteReadModelFilters filters = await options.ToFilters(tenantId, this.cachingResolver);
            await this.authorisationService.ApplyViewQuoteRestrictionsToFilters(this.User, filters);
            var count = await this.mediator.Send(new GetQuoteCountQuery(tenantId, filters));
            return this.Ok(count);
        }

        /// <summary>
        /// Gets a quote's summary information.
        /// </summary>
        /// <param name="quoteId">The ID of the quote for retrieval.</param>
        /// <param name="environment">The environment the caller is requesting for.</param>
        /// <returns>A quote.</returns>
        [HttpGet]
        [Route("{quoteId}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(Permission.ViewQuotes, Permission.ViewAllQuotes, Permission.ViewAllQuotesFromAllOrganisations)]
        [RequiresFeature(Feature.QuoteManagement)]
        [ProducesResponseType(typeof(QuoteSetModel), StatusCodes.Status200OK)]
        public IActionResult GetQuoteSummary(Guid quoteId, [FromQuery] string environment)
        {
            var quoteSummary = this.quoteService.GetQuoteSummary(this.User.GetTenantId(), quoteId);
            this.authorisationService.ThrowIfUserCannotViewQuote(this.User, quoteSummary);
            EnvironmentHelper.ThrowIfEnvironmentDoesNotMatchIfPassed(quoteSummary.Environment, environment, "quote");
            return this.Ok(new QuoteSetModel(quoteSummary));
        }

        /// <summary>
        /// Gets the details of a quote.
        /// </summary>
        /// <param name="quoteId">The ID of the quote for retrieval.</param>
        /// <param name="environment">The environment the caller is requesting for.</param>
        /// <returns>A quote.</returns>
        [HttpGet("{quoteId}/detail")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(Permission.ViewQuotes, Permission.ViewAllQuotes, Permission.ViewAllQuotesFromAllOrganisations)]
        [RequiresFeature(Feature.QuoteManagement)]
        [ProducesResponseType(typeof(QuoteDetailModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetQuoteDetails(Guid quoteId, [FromQuery] string environment)
        {
            IQuoteReadModelDetails quoteDetails =
                this.quoteService.GetQuoteDetails(this.User.GetTenantId(), quoteId);
            await this.authorisationService.ThrowIfUserCannotViewQuote(this.User, quoteDetails);
            EnvironmentHelper.ThrowIfEnvironmentDoesNotMatchIfPassed(quoteDetails.Environment, environment, "quote");
            var releaseContext = await this.mediator.Send(new GetReleaseContextForReleaseOrDefaultReleaseQuery(
                quoteDetails.TenantId,
                quoteDetails.ProductId,
                quoteDetails.Environment,
                quoteDetails.ProductReleaseId));
            var displayableFields =
                await this.configurationService.GetDisplayableFieldsAsync(releaseContext);
            var formDataSchema = this.productConfigurationProvider.GetFormDataSchema(
                releaseContext,
                WebFormAppType.Quote);
            var entityType = AdditionalPropertyEntityTypeConverter.FromQuoteType(quoteDetails.QuoteType);
            var additionalPropertyValuesDto = await this.additionalPropertyValueService.GetAdditionalPropertyValuesByEntityTypeAndEntityId(
                quoteDetails.TenantId, entityType, quoteId);
            var triggerConfig = await this.mediator.Send(new GetProductComponentTriggerConfigurationQuery(
                releaseContext,
                WebFormAppType.Quote));
            QuoteDetailModel model = new QuoteDetailModel(
                this.formDataPrettifier, quoteDetails, displayableFields, formDataSchema, additionalPropertyValuesDto, triggerConfig);
            return this.Ok(model);
        }

        /// <summary>
        /// Gets the number of a quote.
        /// </summary>
        /// <param name="quoteId">The ID of the quote for retrieval.</param>
        /// <returns>A quote.</returns>
        [HttpGet("{quoteId}/quoteNumber")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(Permission.ViewQuotes, Permission.ViewAllQuotes, Permission.ViewAllQuotesFromAllOrganisations)]
        [RequiresFeature(Feature.QuoteManagement)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult GetQuoteNumber(Guid quoteId)
        {
            var quoteNumber = this.quoteService.GetQuoteNumber(this.User.GetTenantId(), quoteId);
            return this.Ok(quoteNumber);
        }

        /// <summary>
        /// Gets the form data of a quote for renewal or update.
        /// </summary>
        /// <param name="quoteId">The Id of the quote for retrieval.</param>
        /// <param name="environment">The Deployment environment.</param>
        /// <returns>A quote.</returns>
        [HttpGet("{quoteId}/form-data")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(Permission.ViewQuotes, Permission.ViewAllQuotes, Permission.ViewAllQuotesFromAllOrganisations)]
        [RequiresFeature(Feature.QuoteManagement)]
        [ProducesResponseType(typeof(QuoteFormDataModel), StatusCodes.Status200OK)]
        public IActionResult GetFormData(Guid quoteId, [FromQuery] string environment)
        {
            var quote = this.quoteService.GetQuoteDetails(this.User.GetTenantId(), quoteId);
            this.authorisationService.ThrowIfUserCannotViewQuote(this.User, quote);
            EnvironmentHelper.ThrowIfEnvironmentDoesNotMatchIfPassed(quote.Environment, environment, "quote");
            var formDataModel = new QuoteFormDataModel(quote);
            return this.Ok(JsonConvert.SerializeObject(formDataModel));
        }

        /// <summary>
        /// Handles requests for discarding quotes.
        /// </summary>
        /// <param name="quoteId">The ID of the parent quote.</param>
        /// <returns>Ok.</returns>
        [HttpPut("{quoteId}/discard")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [MustHaveOneOfPermissions(Permission.ManageQuotes, Permission.ManageAllQuotes, Permission.ManageAllQuotesForAllOrganisations)]
        [RequiresFeature(Feature.QuoteManagement)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DiscardQuote(Guid quoteId)
        {
            await this.authorisationService.ThrowIfUserCannotModifyQuote(this.User, quoteId);
            await this.applicationQuoteService.DiscardQuote(this.User.GetTenantId(), quoteId, this.User.GetTenantId());
            return this.Ok();
        }

        /// <summary>
        /// Handles requests for expiring quotes via settings.
        /// </summary>
        /// <param name="quoteId">The ID of the quote from the aggregate you will set expiry quote to.</param>
        /// <returns>Ok.</returns>
        [HttpPut]
        [Route("{quoteId}/expire")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [MustHaveOneOfPermissions(Permission.ManageQuotes, Permission.ManageAllQuotes, Permission.ManageAllQuotesForAllOrganisations)]
        [RequiresFeature(Feature.QuoteManagement)]
        [ProducesResponseType(typeof(QuoteSetModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> ExpireQuote(Guid quoteId)
        {
            await this.authorisationService.ThrowIfUserCannotModifyQuote(this.User, quoteId);
            var quote = await this.applicationQuoteService.ExpireNow(this.User.GetTenantId(), quoteId);
            return this.Ok(this.GetQuoteSetModel(quote, quoteId));
        }

        /// <summary>
        /// Set the expiry of the quote.
        /// </summary>
        /// <param name="quoteId">The ID of the quote from the aggregate you will set expiry quote to.</param>
        /// <param name="dateTime">The date time that is going to be the expiry of the quote.</param>
        /// <returns>Ok.</returns>
        [HttpPut]
        [Route("{quoteId}/set-expiry")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [MustHaveOneOfPermissions(Permission.ManageQuotes, Permission.ManageAllQuotes, Permission.ManageAllQuotesForAllOrganisations)]
        [RequiresFeature(Feature.QuoteManagement)]
        [ProducesResponseType(typeof(QuoteSetModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> SetExpiryQuote(
            Guid quoteId, [ModelBinder(BinderType = typeof(InstantModelBinder))] Instant dateTime)
        {
            await this.authorisationService.ThrowIfUserCannotModifyQuote(this.User, quoteId);
            var quote = await this.applicationQuoteService.SetExpiry(this.User.GetTenantId(), quoteId, dateTime);
            return this.Ok(this.GetQuoteSetModel(quote, quoteId));
        }

        /// <summary>
        /// Handles requests for creating new incomplete quote from the previous quote.
        /// </summary>
        /// <param name="quoteId">The ID of the quote from the aggregate which you will replicate from.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <returns>Ok.</returns>
        [HttpPut]
        [Route("{quoteId}/clone")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [MustHaveOneOfPermissions(Permission.ManageQuotes, Permission.ManageAllQuotes, Permission.ManageAllQuotesForAllOrganisations)]
        [RequiresFeature(Feature.QuoteManagement)]
        [ProducesResponseType(typeof(CloneQuoteModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> CloneQuote(Guid quoteId, DeploymentEnvironment environment)
        {
            await this.authorisationService.ThrowIfUserCannotModifyQuote(this.User, quoteId);
            var quote = await this.mediator.Send(new CloneQuoteFromExpiredQuoteCommand(
                    this.User.GetTenantId(),
                    quoteId,
                    environment));

            return this.Ok(new CloneQuoteModel(quote));
        }

        /// <summary>
        /// Gets the quote versions related to a specific quote.
        /// </summary>
        /// <param name="quoteId">The ID of the quote the versions are related to.</param>
        /// <returns>The collection of quote versions for the quote ID or null if none.</returns>
        [HttpGet("{quoteId}/version")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHavePermission(Permission.ViewQuoteVersions)]
        [RequiresFeature(Feature.QuoteManagement)]
        [ProducesResponseType(typeof(IEnumerable<QuoteVersionListModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetQuoteVersionsListAsync(Guid quoteId)
        {
            await this.authorisationService.ThrowIfUserCannotViewQuote(this.User, quoteId);

            var query = new GetQuoteVersionsQuery(this.User.GetTenantId(), quoteId);
            var quoteVersionList = await this.mediator.Send(query);
            var models = quoteVersionList
                .Select(qv => new QuoteVersionListModel(qv))
                .OrderByDescending(qv => qv.CreatedDateTime);
            return this.Ok(models);
        }

        /// <summary>
        /// Create/Update search index documents from latest DB.
        /// </summary>
        /// <param name="environment">The environment.</param>
        /// <returns>Action Result.</returns>
        [HttpPost]
        [Route("processQuoteIndexUpdates")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [MustHavePermission(Permission.ManageMasterAdminUsers)]
        [RequiresFeature(Feature.QuoteManagement)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ProcessQuoteIndexUpdates(string environment, CancellationToken cancellationToken)
        {
            DeploymentEnvironment env = EnvironmentHelper.ParseEnvironmentOrThrow(environment);
            var activeProducts = await this.cachingResolver.GetActiveProducts();
            this.quoteSearchIndexService.ProcessIndexUpdates(env, activeProducts, cancellationToken);
            return this.Ok();
        }

        /// <summary>
        /// Creates an empty application based on the values submitted and returns a model containing the configurations.
        /// TODO: If there is a form.model.json file defined in the configuration load that as the initial data.
        /// </summary>
        /// <param name="createQuoteModel">The data model for creating a quote.</param>
        /// <returns>The configuration for the application.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ValidateModel]
        [RequiresFeature(Feature.QuoteManagement)]
        [ProducesResponseType(typeof(QuoteCreateResultModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Create([FromBody] QuoteCreateModel createQuoteModel)
        {
            string? tenantIdOrAlias = !string.IsNullOrEmpty(createQuoteModel.TenantId)
                ? createQuoteModel.TenantId
                : !string.IsNullOrEmpty(createQuoteModel.Tenant)
                    ? createQuoteModel.Tenant
                    : this.User.GetTenantIdOrNull()?.ToString() ?? null;
            Tenant tenant = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenantIdOrAlias));

            NewQuoteReadModel quote;
            if (createQuoteModel.QuoteType == QuoteType.NewBusiness)
            {
                string quoteModelProductIdOrAlias;
                var quoteModelOrganisationIdOrAlias = string.IsNullOrEmpty(createQuoteModel.Organisation) ? createQuoteModel.OrganisationId : createQuoteModel.Organisation;
                var organisation = !string.IsNullOrEmpty(quoteModelOrganisationIdOrAlias)
                    ? quoteModelOrganisationIdOrAlias
                    : this.User.GetOrganisationId() != default
                        ? this.User.GetOrganisationId().ToString()
                        : tenant.Details.DefaultOrganisationId.ToString();
                var organisationModel = await this.cachingResolver.GetOrganisationOrThrow(tenant.Id, new GuidOrAlias(organisation));
                quoteModelProductIdOrAlias = string.IsNullOrEmpty(createQuoteModel.Product) ? createQuoteModel.ProductId : createQuoteModel.Product;

                Product product =
                    await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenantIdOrAlias), new GuidOrAlias(quoteModelProductIdOrAlias));

                var ownerIdRequest = new DetermineOwnerIdForNewQuoteQuery(
                    tenant.Id,
                    this.User,
                    createQuoteModel.CustomerId);
                Guid? ownerUserId = await this.mediator.Send(ownerIdRequest);

                var quoteModelPortalIdOrAlias = string.IsNullOrEmpty(createQuoteModel.Portal) ? createQuoteModel.PortalId : createQuoteModel.Portal;
                var portalModel = await this.cachingResolver.GetPortalOrNull(tenant.Id, new GuidOrAlias(quoteModelPortalIdOrAlias));
                quote = await this.mediator.Send(new CreateNewBusinessQuoteCommand(
                    tenant.Id,
                    organisationModel.Id,
                    portalModel?.Id,
                    product.Id,
                    createQuoteModel.Environment,
                    createQuoteModel.IsTestData,
                    createQuoteModel.CustomerId,
                    ownerUserId,
                    createQuoteModel.FormData,
                    productRelease: createQuoteModel.ProductRelease));
            }
            else
            {
                if (!createQuoteModel.PolicyId.HasValue)
                {
                    throw new ErrorException(
                        Errors.Quote.CannotCreateQuoteOfTypeWithoutPolicy(createQuoteModel.QuoteType));
                }

                switch (createQuoteModel.QuoteType)
                {
                    case QuoteType.Renewal:
                        quote = await this.mediator.Send(new CreateRenewalQuoteCommand(
                            tenant.Id,
                            createQuoteModel.PolicyId.Value,
                            createQuoteModel.DiscardExistingQuote,
                            initialQuoteState: StandardQuoteStates.Incomplete,
                            productRelease: createQuoteModel.ProductRelease));
                        break;
                    case QuoteType.Adjustment:
                        quote = await this.mediator.Send(new CreateAdjustmentQuoteCommand(
                            tenant.Id,
                            createQuoteModel.PolicyId.Value,
                            createQuoteModel.DiscardExistingQuote,
                            initialQuoteState: StandardQuoteStates.Incomplete,
                            productRelease: createQuoteModel.ProductRelease));
                        break;
                    case QuoteType.Cancellation:
                        quote = await this.mediator.Send(new CreateCancellationQuoteCommand(
                            tenant.Id,
                            createQuoteModel.PolicyId.Value,
                            createQuoteModel.DiscardExistingQuote,
                            initialQuoteState: StandardQuoteStates.Incomplete,
                            productRelease: createQuoteModel.ProductRelease));
                        break;
                    default:
                        throw new ErrorException(
                            Errors.General.ModelValidationFailed($"Invalid quote type \"${createQuoteModel.QuoteType}\"."));
                }
            }

            if (!this.User.Identity.IsAuthenticated)
            {
                return this.Ok(new QuoteCreateResultModel(quote));
            }

            var loggedUser = this.userService.GetUserDetail(tenant.Id, this.User.GetId().Value);
            var userResourceModel = loggedUser != null ? new UserResourceModel(loggedUser) : null;
            return this.Ok(new QuoteCreateResultModel(quote, userResourceModel));
        }

        /// <summary>
        /// Associate an existing quote with a new customer.
        /// </summary>
        /// <param name="quoteId">The quote read model Id.</param>
        /// <param name="customerId">The customer read model Id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPost]
        [MustHaveOneOfPermissions(
            Permission.ManageQuotes,
            Permission.ManageAllQuotes,
            Permission.ManageAllQuotesForAllOrganisations)]
        [MustHaveOneOfPermissions(
            Permission.ManageCustomers,
            Permission.ManageAllCustomers)]
        [Route("{quoteId}/associate-with-customer/{customerId}")]
        public async Task<IActionResult> QuoteAssociateWithCustomer(Guid quoteId, Guid customerId)
        {
            await this.authorisationService.ThrowIfUserCannotModifyQuote(this.User, quoteId);
            var quoteSummary = this.quoteService.GetQuoteSummary(this.User.GetTenantId(), quoteId);
            if (quoteSummary.PolicyIssued && quoteSummary.PolicyId != null)
            {
                await this.authorisationService.ThrowIfUserCannotModifyPolicy(this.User, quoteSummary.PolicyId.Value);
                var command = new AssociatePolicyWithCustomerCommand(this.User.GetTenantId(), quoteSummary.PolicyId.Value, customerId);
                await this.mediator.Send(command);
            }
            else
            {
                var command = new AssociateQuoteWithCustomerCommand(this.User.GetTenantId(), quoteId, customerId);
                await this.mediator.Send(command);
            }
            return this.Ok();
        }

        /// <summary>
        /// Handles issue policy requests.
        /// </summary>
        /// <param name="quoteId">The ID of the quote the policy is for.</param>
        /// <param name="model">The model passed through the body.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [Route("{quoteId}/issue-policy")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [MustHaveOneOfPermissions(
            Permission.ManagePolicies,
            Permission.ManageAllPolicies,
            Permission.ManageAllPoliciesForAllOrganisations)]
        [MustHaveOneOfPermissions(
            Permission.ManageQuotes,
            Permission.ManageAllQuotes,
            Permission.ManageAllQuotesForAllOrganisations)]
        [ProducesResponseType(typeof(PolicyIssuanceModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> IssuePolicy(Guid quoteId, [FromBody] IssuePolicyModel model, [FromQuery] DeploymentEnvironment? environment)
        {
            var quoteDetails = this.quoteService.GetQuoteDetails(this.User.GetTenantId(), quoteId);
            var releaseContext = await this.mediator.Send(new GetReleaseContextForReleaseOrDefaultReleaseQuery(
                quoteDetails.TenantId,
                quoteDetails.ProductId,
                quoteDetails.Environment,
                quoteDetails.ProductReleaseId));
            var bindCommand = BindPolicyCommand.CreateForBindingWithQuote(
                releaseContext,
                quoteId,
                externalPolicyNumber: model.PolicyNumber,
                environment: environment);
            (NewQuoteReadModel, PolicyReadModel) result = await this.mediator.Send(bindCommand);
            var responseModel = new PolicyIssuanceModel(result.Item1, result.Item2);
            return this.Ok(responseModel);
        }

        private async Task<QuoteSetModel> GetQuoteSetModel(QuoteAggregate quoteAggregate, Guid quoteId)
        {
            var quote = quoteAggregate.GetQuoteOrThrow(quoteId);
            var product = await this.cachingResolver.GetProductOrNull(quoteAggregate.TenantId, quoteAggregate.ProductId);

            return new QuoteSetModel(
                quote,
                this.clock.Now(),
                product.Details.Alias);
        }
    }
}
