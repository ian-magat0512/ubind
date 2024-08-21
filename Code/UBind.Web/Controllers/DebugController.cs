// <copyright file="DebugController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading.Tasks;
    using Hangfire;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Nito.AsyncEx;
    using NodaTime;
    using Sentry;
    using UBind.Application;
    using UBind.Application.Authorisation;
    using UBind.Application.Commands.Customer;
    using UBind.Application.Commands.Sentry;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.FlexCel;
    using UBind.Application.MicrosoftGraph;
    using UBind.Application.Releases;
    using UBind.Application.Services.Email;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Authentication;
    using UBind.Domain.Entities;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReferenceNumbers;
    using UBind.Domain.Services;
    using UBind.Domain.Upgrade;
    using UBind.Web.Exceptions;
    using UBind.Web.Extensions;
    using UBind.Web.Filters;

    /// <summary>
    /// Controller for testing and debugging.
    /// </summary>
    [ApiController]
    [ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
    [Produces(ContentTypes.Json)]
    public class DebugController : Controller
    {
        private readonly ICachingResolver cachingResolver;
        private readonly ISubscriptionService subscriptionService;
        private readonly IPolicyNumberRepository policyNumberRepository;
        private readonly IInvoiceNumberRepository invoiceNumberRepository;
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly IEmailAddressBlockingEventRepository emailBlockingEventRepository;
        private readonly ICustomerService customerService;
        private readonly IGlobalReleaseCache globalReleaseCache;
        private readonly IClock clock;
        private readonly IOrganisationReadModelRepository organisationReadModelRepository;
        private readonly IErrorNotificationService errorNotificationService;
        private readonly ISpreadsheetPoolService spreadsheetPoolService;
        private readonly IAuthorisationService authorisationService;
        private readonly ICqrsMediator mediator;
        private readonly IHub sentryHub;
        private readonly IMediator queryMediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="DebugController"/> class.
        /// </summary>
        /// <param name="subscriptionService">The One Drive subscription service.</param>
        /// <param name="policyNumberRepository">Policy number repository.</param>
        /// <param name="invoiceNumberRepository">Invoice number repository.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        /// <param name="backgroundJobClient">Service for scheduling background jobs.</param>
        /// <param name="emailBlockingEventRepository">The login attempt service.</param>
        /// <param name="customerService">The service used for creating customers.</param>
        /// <param name="organisationReadModelRepository">The organisation read model repository.</param>
        /// <param name="spreadsheetPoolService">The service for working with spreadsheet pools.</param>
        /// <param name="globalReleaseCache">The global release cache.</param>
        /// <param name="clock">A clock.</param>
        /// <param name="errorNotificationService">The service used for error notification.</param>
        /// <param name="authorisationService">The authorisation service.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="sentryHub">The sentry.</param>
        public DebugController(
            ISubscriptionService subscriptionService,
            IPolicyNumberRepository policyNumberRepository,
            IInvoiceNumberRepository invoiceNumberRepository,
            ICachingResolver cachingResolver,
            IBackgroundJobClient backgroundJobClient,
            IEmailAddressBlockingEventRepository emailBlockingEventRepository,
            ICustomerService customerService,
            IOrganisationReadModelRepository organisationReadModelRepository,
            ISpreadsheetPoolService spreadsheetPoolService,
            IGlobalReleaseCache globalReleaseCache,
            IClock clock,
            IErrorNotificationService errorNotificationService,
            IAuthorisationService authorisationService,
            ICqrsMediator mediator,
            IHub sentryHub,
            IMediator queryMediator)
        {
            this.subscriptionService = subscriptionService;
            this.policyNumberRepository = policyNumberRepository;
            this.invoiceNumberRepository = invoiceNumberRepository;
            this.cachingResolver = cachingResolver;
            this.backgroundJobClient = backgroundJobClient;
            this.emailBlockingEventRepository = emailBlockingEventRepository;
            this.customerService = customerService;
            this.organisationReadModelRepository = organisationReadModelRepository;
            this.globalReleaseCache = globalReleaseCache;
            this.clock = clock;
            this.spreadsheetPoolService = spreadsheetPoolService;
            this.errorNotificationService = errorNotificationService;
            this.authorisationService = authorisationService;
            this.mediator = mediator;
            this.sentryHub = sentryHub;
            this.queryMediator = queryMediator;
        }

        /// <summary>
        /// For testing.
        /// </summary>
        /// <returns>Yeah.</returns>
        [HttpGet]
        [Route("admin")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 100)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult Test()
        {
            return this.Ok("Yeah");
        }

        /// <summary>
        /// Trigger subscriptions.
        /// </summary>
        /// <returns>An awaitable task.</returns>
        [HttpPost]
        [Route("subscribe")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 10)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> Subscribe()
        {
            await this.authorisationService.ThrowIfUserIsNotFromMasterTenant(this.User, "trigger subscription");
            var url = this.Url.Action("Listen", "Notification", null, this.Request.HttpContext.Request.Scheme);
            await this.subscriptionService.SubscribeToNotifications(url);
            return this.Ok("Subscription added.");
        }

        /// <summary>
        /// Purge all policy numbers for a given tenant, product and environment.
        /// </summary>
        /// <param name="tenantAlias">The alias for the tenant to purge numbers for.</param>
        /// <param name="productAlias">The alias of the product to purge numbers for.</param>
        /// <param name="environment">The environment to purge numbers for.</param>
        /// <returns>Ok.</returns>
        [HttpDelete]
        [Route("/api/{tenantAlias}/{productAlias}/{environment}/purgePolicyNumbers")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 10)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> PurgePolicyNumbers(string tenantAlias, string productAlias, DeploymentEnvironment environment)
        {
            await this.authorisationService.ThrowIfUserIsNotFromMasterTenant(this.User, "purge policy numbers");
            var product = await this.cachingResolver.GetProductByAliasOThrow(tenantAlias, productAlias);
            this.policyNumberRepository.PurgeForProduct(product.TenantId, product.Id, environment);
            return this.Ok($"{{ \"Purge\": \"Policy numbers purged for {productAlias} in {environment}.\" }}");
        }

        /// <summary>
        /// Purge all invoice numbers for a given tenant, product and environment.
        /// </summary>
        /// <param name="tenantAlias">The alias for the tenant to purge numbers for.</param>
        /// <param name="productAlias">The alias of the product to purge numbers for.</param>
        /// <param name="environment">The environment to purge numbers for.</param>
        /// <returns>Ok.</returns>
        [HttpDelete]
        [Route("/api/{tenantAlias}/{productAlias}/{environment}/purgeInvoiceNumbers")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 10)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> PurgeInvoiceNumbers(string tenantAlias, string productAlias, DeploymentEnvironment environment)
        {
            await this.authorisationService.ThrowIfUserIsNotFromMasterTenant(this.User, "purge invoice numbers");
            var product = await this.cachingResolver.GetProductByAliasOThrow(tenantAlias, productAlias);
            this.invoiceNumberRepository.PurgeForProduct(product.TenantId, product.Id, environment);
            return this.Ok($"{{ \"Purge\": \"Invoice numbers purged for {productAlias} in {environment}.\" }}");
        }

        /// <summary>
        /// For triggering migration of One Drive content for release 2.0.
        /// </summary>
        /// <returns>No content.</returns>
        [HttpPost]
        [Route("migrate/OneDrive")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 1)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult MigrateOneDrive()
        {
            this.authorisationService.ThrowIfUserIsNotFromMasterTenant(this.User, "Migrate one drive");
            this.backgroundJobClient.Enqueue<OneDriveRefactorService>(
                service => service.UpdateOneDrive(Release2ProductMappings.ProductMappings));
            return this.NoContent();
        }

        /// <summary>
        /// Unblock an email from being able to be used to login to a given tenant.
        /// </summary>
        /// <param name="tenantAlias">The Alias of the tenant.</param>
        /// <param name="emailAddress">The email.</param>
        /// <param name="organisationAlias">The alias of the organisation.</param>
        /// <returns>Response.</returns>
        [HttpPost]
        [Route("unlockuser")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ValidateModel]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> UnlockLoginAttempt(
            [Required] string tenantAlias, [Required] string emailAddress, string organisationAlias)
        {
            await this.authorisationService.ThrowIfUserIsNotFromMasterTenant(this.User, "unlock user");
            var tenant = await this.cachingResolver.GetTenantByAliasOrThrow(tenantAlias);

            var organisation = this.organisationReadModelRepository.GetByAlias(tenant.Id, organisationAlias);

            Guid? organisationId = null;
            if (organisation != null)
            {
                organisationId = organisation.Id;
                if (organisation == null || organisation.TenantId != tenant.Id)
                {
                    return Errors.Organisation.NotFound(organisation.Id).ToProblemJsonResult();
                }
            }
            else
            {
                organisationId = tenant.Details.DefaultOrganisationId;
            }

            var @event = EmailAddressBlockingEvent.EmailAddressUnblocked(
                tenant.Id,
                organisationId.Value,
                emailAddress,
                EmailAddressUnblockedReason.EmailAddressUnblockedByAdministrator,
                this.clock.Now());
            this.emailBlockingEventRepository.Insert(@event);
            this.emailBlockingEventRepository.SaveChanges();
            return this.Ok("Unlock user success");
        }

        /// <summary>
        /// Tests the validation test model.
        /// </summary>
        /// <param name="model">the validation test model.</param>
        /// <returns>Response.</returns>
        [HttpPost]
        [Route("api/v1/debug/validationTest")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 1)]
        [ValidateModel]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult ValidationTest([FromBody][Required] ValidationTestModel model)
        {
            this.authorisationService.ThrowIfUserIsNotFromMasterTenant(this.User, "validate test");
            return this.NoContent();
        }

        /// <summary>
        /// Simulates a bad request.
        /// </summary>
        /// <returns>Resposne.</returns>
        [HttpPost]
        [Route("api/v1/debug/badRequest")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 1)]
        public IActionResult ValidationTest()
        {
            this.authorisationService.ThrowIfUserIsNotFromMasterTenant(this.User, "validate test");
            throw new BadRequestException("Badness :(");
        }

        /// <summary>
        /// Simulates an unhandled error.
        /// </summary>
        /// <returns>Response.</returns>
        [HttpPost]
        [Route("api/v1/debug/unhandledError")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 1)]
        public IActionResult ServerError()
        {
            this.authorisationService.ThrowIfUserIsNotFromMasterTenant(this.User, "test server error");
            throw new Exception();
        }

        /// <summary>
        /// Simulates the throwing of an error exception email.
        /// </summary>
        /// <param name="teantAlias">The tenant alias.</param>
        /// <param name="environment">The environemnt.</param>
        /// <returns>Response.</returns>
        [HttpPost]
        [Route("api/v1/debug/emailErrorException")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 1)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> EmailErrorException(string teantAlias, string environment)
        {
            await this.authorisationService.ThrowIfUserIsNotFromMasterTenant(this.User, "test email error");
            var parseEnvironment = Enum.TryParse<DeploymentEnvironment>(
                environment, true, out DeploymentEnvironment env);
            if (!parseEnvironment)
            {
                return Errors.General.NotFound("environment", environment).ToProblemJsonResult();
            }

            var exception = new ErrorException(Errors.General.Unexpected("Email error exception endpoint."));

            this.errorNotificationService.SendGeneralErrorEmail(
                teantAlias,
                environment,
                exception);

            throw exception;
        }

        /// <summary>
        /// Simulates the throwing of an ErrorException.
        /// </summary>
        /// <returns>Response.</returns>
        [HttpPost]
        [Route("api/v1/debug/errorException")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 1)]
        public IActionResult ThrowErrorException()
        {
            this.authorisationService.ThrowIfUserIsNotFromMasterTenant(this.User, "test throw error");
            var error = Errors.General.Unexpected("error exception endpoint.");
            throw new ErrorException(error);
        }

        /// <summary>
        /// Creates 100 customers to prepare for testing infinite scroll.
        /// </summary>
        /// <param name="tenant">The string tenant ID or Alias.</param>
        /// <param name="environment">The environment.</param>
        /// <returns>Response.</returns>
        [HttpPost]
        [Route("api/v1/debug/{tenant}/{environment}/prepInfiniteScrollTest")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 1)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> PrepInfiniteScrollTest(string tenant, string environment)
        {
            await this.authorisationService.ThrowIfUserIsNotFromMasterTenant(this.User, "create test data for infinite scroll");

            var parseEnvironment = Enum.TryParse<DeploymentEnvironment>(
                environment, true, out DeploymentEnvironment env);
            if (!parseEnvironment)
            {
                return Errors.General.NotFound("environment", environment).ToProblemJsonResult();
            }

            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            for (int i = 0; i < 100; ++i)
            {
                var personCommonProperties = new PersonCommonProperties();
                personCommonProperties.TenantId = tenantModel.Id;
                personCommonProperties.FullName = "Customer" + i;
                personCommonProperties.PreferredName = "Customer" + i;
                personCommonProperties.Email = "customer" + i + "@test.com";
                personCommonProperties.AlternativeEmail = "customer" + i + "@alt.com";
                personCommonProperties.MobilePhoneNumber = "0400123123";
                personCommonProperties.HomePhoneNumber = "0312341234";
                personCommonProperties.WorkPhoneNumber = "0311112222";
                var personDetails = new PersonalDetails(tenantModel.Id, personCommonProperties);
                personDetails.LoginEmail = "customer" + i + "@test.com";

                await this.mediator.Send(
                    new CreateCustomerCommand(
                        personCommonProperties.TenantId,
                        env,
                        personDetails,
                        this.User.GetId(),
                        null));
            }

            return this.Ok();
        }

        /// <summary>
        /// Returns spreadsheet memory usage information.
        /// </summary>
        /// <returns>spreadsheet memory usage information, in JSON format.</returns>
        [HttpGet]
        [Route("api/v1/debug/spreadsheet-memory-usage")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 10)]
        [ProducesResponseType(typeof(JObject), StatusCodes.Status200OK)]
        public async Task<JsonResult> GetSpreadsheetMemoryUsageInformation()
        {
            await this.authorisationService.ThrowIfUserIsNotFromMasterTenant(this.User, "get spreadsheet memory usage information");
            return this.Json(this.spreadsheetPoolService.GetMemoryUsageInformation());
        }

        /// <summary>
        /// Returns global release cache information.
        /// </summary>
        /// <returns>.</returns>
        [HttpGet]
        [Route("api/v1/debug/global-release-cache")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 10)]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        public async Task<JsonResult> GetGlobalReleaseCacheState()
        {
            await this.authorisationService.ThrowIfUserIsNotFromMasterTenant(this.User, "get global release cache");
            return this.Json(
            this.globalReleaseCache.GetCacheState()
                .OrderBy(kvp => kvp.Key.TenantId)
                .ThenBy(kvp => kvp.Key.ProductId)
                .ThenBy(kvp => kvp.Key.Environment)
                .Select(kvp => $"{kvp.Key.ToString()}: {kvp.Value}"));
        }

        /// <summary>
        /// Clears the pool.
        /// </summary>
        /// <param name="tenantId">The tenant ID or Alias.</param>
        /// <param name="productId">The product ID or Alias.</param>
        /// <param name="environment">The environment.</param>
        /// <returns>No content.</returns>
        [HttpGet]
        [Route("api/v1/debug/clearWorkbookPool")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ClearWorkbookPool(string tenantId, string productId, DeploymentEnvironment environment)
        {
            await this.authorisationService.ThrowIfUserIsNotFromMasterTenant(this.User, "clear workbook pool");
            Product productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenantId), new GuidOrAlias(productId));
            var productContext = new ProductContext(productModel.TenantId, productModel.Id, environment);
            this.spreadsheetPoolService.RemoveSpreadsheetPools(productContext, WebFormAppType.Quote);
            this.spreadsheetPoolService.RemoveSpreadsheetPools(productContext, WebFormAppType.Claim);
            return this.NoContent();
        }

        /// <summary>
        /// Send a exception to the sentry.
        /// </summary>
        /// <param name="json">The Json to capture for sentry.</param>
        /// <param name="environment">The environment.</param>
        /// <returns>No content.</returns>
        [HttpPost]
        [Route("api/v1/debug/{environment}/sentry")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Sentry([Required] string json, string environment)
        {
            var parseEnvironment = Enum.TryParse(environment, true, out DeploymentEnvironment env);
            if (!parseEnvironment)
            {
                return Errors.General.NotFound("environment", environment).ToProblemJsonResult();
            }

            await this.mediator.Send(new CaptureSentryExceptionCommand(new Exception(json), env));
            return this.Ok($"Sentry Event Id: {this.sentryHub.LastEventId}");
        }

        /// <summary>
        /// Trigger the throwing of System.OperationCanceledException exception.
        /// </summary>
        [HttpGet]
        [Route("api/v1/debug/{environment}/sentry/cancel-operation")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public IActionResult CancelOperation(string environment = "development")
        {
            throw new System.Threading.Tasks.TaskCanceledException("test");
        }

        /// <summary>
        /// Trigger the throwing of ErrorException exception that contains the code `automation.configuration.should.have.distinct.automation.trigger.alias`.
        /// </summary>
        [HttpGet]
        [Route("api/v1/debug/{environment}/sentry/throw-automation-error-code")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public IActionResult ThrowAutomationErrorCode(string environment = "development")
        {
            throw new ErrorException(
                    Errors.Automation.HasDuplicateTrggerAlias(string.Empty, string.Empty));
        }

        /// <summary>
        /// Sample validation model.
        /// </summary>
        public class ValidationTestModel
        {
            /// <summary>
            /// Gets or sets price with validation.
            /// </summary>
            [Range(0, 999.99, ErrorMessage = "Price must be between zero and $999.99")]
            public decimal Price { get; set; }

            /// <summary>
            /// Gets or sets property for testing - no validation.
            /// </summary>
            [Required]
            public string Foo { get; set; }

            /// <summary>
            /// Gets or sets another property with validation.
            /// </summary>
            [StringLength(6, MinimumLength = 3, ErrorMessage = "Bar must be between 3 and 6 characters in length.")]
            public string Bar { get; set; }
        }
    }
}
