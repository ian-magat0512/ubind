// <copyright file="QuoteVersionController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers.Portal
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application;
    using UBind.Application.Authorisation;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Helpers;
    using UBind.Application.Queries.ProductRelease;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.Product;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels.Quote;

    /// <summary>
    /// Controller for managing quote version requests.
    /// </summary>
    [Route("api/v1/quote-version")]
    [ApiController]
    [RequiresFeature(Feature.QuoteManagement)]
    public class QuoteVersionController : Controller
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IQuoteService quoteService;
        private readonly IFeatureSettingService settingService;
        private readonly IConfigurationService configurationService;
        private readonly IProductConfigurationProvider productConfigurationProvider;
        private readonly IFormDataPrettifier formDataPrettifier;
        private readonly IAuthorisationService authorisationService;
        private readonly IAdditionalPropertyValueService additionalPropertyValueService;
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteVersionController"/> class.
        /// </summary>
        /// <param name="quoteService">The quote service.</param>
        /// <param name="settingService">The Setting service.</param>
        /// <param name="configurationService">The Configuration service.</param>
        /// <param name="productConfigurationProvider">The product configuration provider.</param>
        /// <param name="formDataPrettifier">The form data prettifier.</param>
        /// <param name="authorisationService">The authorisation service.</param>
        /// <param name="additionalPropertyValueService">Additional property value service.</param>
        /// <param name="cachingResolver">The caching resolver.</param>
        public QuoteVersionController(
            IQuoteService quoteService,
            IFeatureSettingService settingService,
            IConfigurationService configurationService,
            IProductConfigurationProvider productConfigurationProvider,
            IFormDataPrettifier formDataPrettifier,
            IAuthorisationService authorisationService,
            IAdditionalPropertyValueService additionalPropertyValueService,
            ICachingResolver cachingResolver,
            ICqrsMediator mediator)
        {
            this.cachingResolver = cachingResolver;
            this.quoteService = quoteService;
            this.settingService = settingService;
            this.configurationService = configurationService;
            this.productConfigurationProvider = productConfigurationProvider;
            this.formDataPrettifier = formDataPrettifier;
            this.authorisationService = authorisationService;
            this.additionalPropertyValueService = additionalPropertyValueService;
            this.mediator = mediator;
        }

        /// <summary>
        /// Gets the detail of quote version related to a specific quote.
        /// </summary>
        /// <param name="quoteVersionId">The ID of the quote version.</param>
        /// <param name="environment">The environment the caller is requesting for.</param>
        /// <returns>A quote with values from the specified quote version id.</returns>
        [HttpGet]
        [Route("{quoteVersionId}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHavePermission(Permission.ViewQuoteVersions)]
        [ProducesResponseType(typeof(QuoteVersionModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetQuoteVersion(Guid quoteVersionId, [FromQuery] string environment)
        {
            var quoteVersionDetail = this.quoteService.GetQuoteVersionDetail(this.User.GetTenantId(), quoteVersionId);
            var quoteReadModelDetails = this.quoteService.GetQuoteDetails(this.User.GetTenantId(), quoteVersionDetail.QuoteId);
            await this.authorisationService.ThrowIfUserCannotViewQuote(this.User, quoteReadModelDetails);
            EnvironmentHelper.ThrowIfEnvironmentDoesNotMatchIfPassed(quoteReadModelDetails.Environment, environment, "quote");

            var product = await this.cachingResolver.GetProductOrThrow(quoteReadModelDetails.TenantId, quoteReadModelDetails.ProductId);
            var model = new QuoteVersionModel(quoteVersionDetail, product);
            return this.Ok(model);
        }

        /// <summary>
        /// Gets the details of a quote version.
        /// </summary>
        /// <param name="quoteVersionId">The ID of the quote version for retrieval.</param>
        /// <param name="environment">The environment the caller is requesting for.</param>
        /// <returns>A quote.</returns>
        [HttpGet("{quoteVersionId}/detail")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHavePermission(Permission.ViewQuoteVersions)]
        [ProducesResponseType(typeof(QuoteVersionDetailModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetQuoteVersionDetail(Guid quoteVersionId, [FromQuery] string environment)
        {
            var quoteVersionDetails = this.quoteService.GetQuoteVersionDetail(this.User.GetTenantId(), quoteVersionId);
            var quoteDetails = this.quoteService.GetQuoteDetails(this.User.GetTenantId(), quoteVersionDetails.QuoteId);
            await this.authorisationService.ThrowIfUserCannotViewQuote(this.User, quoteDetails);
            EnvironmentHelper.ThrowIfEnvironmentDoesNotMatchIfPassed(quoteDetails.Environment, environment, "quote");

            var releaseContext = await this.mediator.Send(new GetReleaseContextForReleaseOrDefaultReleaseQuery(
                quoteDetails.TenantId,
                quoteDetails.ProductId,
                quoteDetails.Environment,
                quoteDetails.ProductReleaseId));
            var displayableFields =
                await this.configurationService.GetDisplayableFieldsAsync(releaseContext);
            var additionalPropertyValueDtos = await this.additionalPropertyValueService
            .GetAdditionalPropertyValuesByEntityTypeAndEntityId(
                quoteDetails.TenantId,
                AdditionalPropertyEntityType.QuoteVersion,
                quoteVersionId);
            var formDataSchema = this.productConfigurationProvider.GetFormDataSchema(
                releaseContext,
                WebFormAppType.Quote);
            var model = new QuoteVersionDetailModel(
                this.formDataPrettifier,
                quoteDetails,
                quoteVersionDetails,
                displayableFields,
                formDataSchema,
                additionalPropertyValueDtos);
            return this.Ok(model);
        }
    }
}
