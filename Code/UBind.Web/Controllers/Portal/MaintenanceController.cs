// <copyright file="MaintenanceController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Humanizer;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Application.Authorisation;
    using UBind.Application.Commands.Entity;
    using UBind.Application.Commands.LuceneIndex;
    using UBind.Application.Commands.Migration;
    using UBind.Application.Commands.Policy;
    using UBind.Application.Commands.Quote;
    using UBind.Application.Commands.QuoteAggregate;
    using UBind.Application.Commands.Release;
    using UBind.Application.Commands.TokenSession;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Queries.Policy;
    using UBind.Application.Queries.Principal;
    using UBind.Application.Queries.ProductRelease;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.Services;
    using UBind.Web.Extensions;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels.Policy;
    using UBind.Web.ResourceModels.Quote;

    /// <summary>
    /// Provides for utilities and maintenance operations on aggregates of the system.
    /// </summary>
    [MustBeLoggedIn]
    [Route("/api/v1/maintenance")]
    [Produces("application/json")]
    public class MaintenanceController : Controller
    {
        private readonly IAuthorisationService authorisationService;
        private readonly IQuoteAggregateResolverService quoteAggregateResolverService;
        private readonly IQuoteEventRepository aggregateEventRepository;
        private readonly ICachingResolver cachingResolver;
        private readonly ICqrsMediator mediator;
        private readonly ILogger<MaintenanceController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaintenanceController"/> class.
        /// </summary>
        /// <param name="authorisationService">The authorisation service.</param>
        /// <param name="quoteAggregateResolverService">The quote aggregate resolver service.</param>
        /// <param name="aggregateEventRepository">The quote aggrgate event repository.</param>
        /// <param name="cachingResolver">The tenant, product, organisation and portal resolver.</param>
        /// <param name="mediator">The mediator.</param>
        public MaintenanceController(
            IAuthorisationService authorisationService,
            IQuoteAggregateResolverService quoteAggregateResolverService,
            IQuoteEventRepository aggregateEventRepository,
            ICachingResolver cachingResolver,
            ICqrsMediator mediator,
            ILogger<MaintenanceController> logger)
        {
            this.quoteAggregateResolverService = quoteAggregateResolverService;
            this.aggregateEventRepository = aggregateEventRepository;
            this.authorisationService = authorisationService;
            this.cachingResolver = cachingResolver;
            this.mediator = mediator;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves a collection of aggregate events for the given quote/policy/aggregate Id.
        /// </summary>
        /// <param name="recordId">The ID of the quote/policy/aggregate.</param>
        /// <param name="tenant">The tenant ID or alias. If not supplied the tenant of the logged in user is used.</param>
        /// <returns>Ok.</returns>
        [HttpGet]
        [MustBeLoggedIn]
        [Route("quote-aggregate/{recordId}/event")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustBeLoggedIn]
        [ProducesResponseType(typeof(QuoteEventSummarySetModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetQuoteAggregateEvents(Guid recordId, [FromQuery] string tenant = null)
        {
            tenant = tenant ?? this.User.GetTenantId().ToString();
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            var aggregateId = this.quoteAggregateResolverService.GetQuoteAggregateIdForQuoteIdOrPolicyId(recordId);
            if (!this.authorisationService.IsUserFromMasterTenant(this.User))
            {
                await this.authorisationService.ThrowIfUserCannotModifyPolicyOrQuote(this.User, aggregateId);
            }

            var summaries = this.aggregateEventRepository.GetEventSummaries(aggregateId);
            if (!summaries.Any())
            {
                return Errors.General.NotFound("quote aggregate", aggregateId).ToProblemJsonResult();
            }

            return this.Ok(new QuoteEventSummarySetModel(aggregateId, summaries));
        }

        /// <summary>
        /// Rolls back a quote aggregate's state to a given sequence number given the quote/policy/aggregate Id.
        /// </summary>
        /// <param name="recordId">The ID of the aggregate/policy/quote to roll back.</param>
        /// <param name="sequenceNumber">The sequence number of the aggregate event to roll back the aggregates state to.</param>
        /// <param name="tenant">The tenant ID or alias. If not supplied the tenant of the logged in user is used.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [MustBeLoggedIn]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ValidateModel]
        [Route("quote-aggregate/{recordId}/event/{sequenceNumber}/roll-back")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> RollbackQuoteAggregate(Guid recordId, int sequenceNumber, [FromQuery] string tenant = null)
        {
            tenant = tenant ?? this.User.GetTenantId().ToString();
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            var aggregateId = this.quoteAggregateResolverService.GetQuoteAggregateIdForQuoteIdOrPolicyId(recordId);
            if (!this.authorisationService.IsUserFromMasterTenant(this.User))
            {
                await this.authorisationService.ThrowIfUserCannotModifyPolicyOrQuote(this.User, aggregateId);
            }

            await this.mediator.Send(new RollbackQuoteAggregateCommand(tenantModel.Id, aggregateId, sequenceNumber));
            return this.Ok($"Aggregate with id {aggregateId} has been rolled back to sequence {sequenceNumber}.");
        }

        /// <summary>
        /// Replay all events for an aggregate. This is typically used to regenerate read models.
        /// </summary>
        /// <param name="aggregateEntityType">The entity type.</param>
        /// <param name="recordId">The ID of the aggregate/policy/quote to replay.</param>
        /// <param name="tenant">The tenant ID or alias. If not supplied the tenant of the logged in user is used.</param>
        /// <param name="dispatchToAllObservers">If true, all the events will be dispatched to all event observers.</param>
        /// <param name="dispatchToReadModelWriters">If true, all the events will be dispatched to the read model writers.</param>
        /// <param name="dispatchToSystemEventEmitters">If true, all the events will be dispatched to the system event
        /// emitters. This will cause automations that have event triggers listening to re-trigger.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [MustBeLoggedIn]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ValidateModel]
        [Route("{aggregateEntityType}/{recordId}/event/replay-all")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> ReplayAllEventsForAggregateEntity(
            [FromRoute] AggregateEntityType aggregateEntityType,
            [FromRoute] Guid recordId,
            [FromQuery] string? tenant = null,
            [FromQuery] bool dispatchToAllObservers = false,
            [FromQuery] bool dispatchToReadModelWriters = true,
            [FromQuery] bool dispatchToSystemEventEmitters = false)
        {
            tenant = tenant ?? this.User.GetTenantId().ToString();
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            if (aggregateEntityType == AggregateEntityType.Quote)
            {
                recordId = this.quoteAggregateResolverService.GetQuoteAggregateIdForQuoteId(recordId);
            }
            else if (aggregateEntityType == AggregateEntityType.Policy)
            {
                recordId = this.quoteAggregateResolverService.GetQuoteAggregateIdForPolicyId(recordId);
            }
            var aggregateId = this.quoteAggregateResolverService.GetQuoteAggregateIdForQuoteIdOrPolicyId(recordId);
            if (!this.authorisationService.IsUserFromMasterTenant(this.User))
            {
                await this.authorisationService.ThrowIfUserCannotModifyPolicyOrQuote(this.User, aggregateId);
            }

            await this.mediator.Send(new ReplayAllEventsForAggregateEntityCommand(
                tenantModel.Id,
                recordId,
                aggregateEntityType,
                dispatchToAllObservers,
                dispatchToReadModelWriters,
                dispatchToSystemEventEmitters));
            return this.Ok($"All events for {aggregateEntityType} Aggregate with id {recordId} have been replayed.");
        }

        /// <summary>
        /// Replay a single event for a quote aggregate given the quote/policy/aggregate Id.
        /// </summary>
        /// <param name="aggregateEntityType">The entity type.</param>
        /// <param name="recordId">The ID of the aggregate/policy/quote.</param>
        /// <param name="sequenceNumber">The sequence number of the event to replay.</param>
        /// <param name="tenant">The tenant ID or alias. If not supplied the tenant of the logged in user is used.</param>
        /// <param name="dispatchToAllObservers">If true, the event will be dispatched to all event observers.</param>
        /// <param name="dispatchToReadModelWriters">If true, the event will be dispatched to the read model writers.</param>
        /// <param name="dispatchToSystemEventEmitters">If true, the event will be dispatched to the system event
        /// emitters. This will cause automations that have event triggers listening to re-trigger.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [MustBeLoggedIn]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ValidateModel]
        [Route("{aggregateEntityType}/{recordId}/event/{sequenceNumber}/replay")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> ReplayEventForAggregate(
            [FromRoute] AggregateEntityType aggregateEntityType,
            [FromRoute] Guid recordId,
            [FromRoute] int sequenceNumber,
            [FromQuery] string? tenant = null,
            [FromQuery] bool dispatchToAllObservers = false,
            [FromQuery] bool dispatchToReadModelWriters = false,
            [FromQuery] bool dispatchToSystemEventEmitters = true)
        {
            tenant = tenant ?? this.User.GetTenantId().ToString();
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            if (aggregateEntityType == AggregateEntityType.Quote)
            {
                recordId = this.quoteAggregateResolverService.GetQuoteAggregateIdForQuoteId(recordId);
            }
            else if (aggregateEntityType == AggregateEntityType.Policy)
            {
                recordId = this.quoteAggregateResolverService.GetQuoteAggregateIdForPolicyId(recordId);
            }

            var aggregateId = this.quoteAggregateResolverService.GetQuoteAggregateIdForQuoteIdOrPolicyId(recordId);
            if (!this.authorisationService.IsUserFromMasterTenant(this.User))
            {
                await this.authorisationService.ThrowIfUserCannotModifyPolicyOrQuote(this.User, aggregateId);
            }

            await this.mediator.Send(new ReplayAggregateEntityEventCommand(
                tenantModel.Id,
                recordId,
                aggregateEntityType,
                sequenceNumber,
                dispatchToAllObservers,
                dispatchToReadModelWriters,
                dispatchToSystemEventEmitters));
            return this.Ok($"Event {sequenceNumber} for Aggregate with id {recordId} has been replayed.");
        }

        /// <summary>
        /// Move a quote, policy or QuoteAggregate to another organisation.
        /// This does not move the customer.
        /// This API enpoint can only be executed by a user of the master tenancy. It's
        /// intended for support related operations.
        /// </summary>
        /// <param name="recordId">The ID of the aggregate/policy/quote to move.</param>
        /// <param name="tenant">The tenant ID or alias.</param>
        /// <param name="organisation">The organisation ID or alias.</param>
        /// <returns>Ok.</returns>
        [HttpPatch]
        [MustBeLoggedIn]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ValidateModel]
        [Route("/api/v1/tenant/{tenant}/aggregate/{recordId}/organisation/{organisation}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> MoveAggregateToOrganisation(Guid recordId, string tenant, string organisation)
        {
            await this.authorisationService.ThrowIfUserIsNotFromMasterTenant(this.User, "move an entity to another organisation");
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            var organisationModel = await this.cachingResolver.GetOrganisationOrThrow(tenantModel.Id, new GuidOrAlias(organisation));
            var aggregateId = this.quoteAggregateResolverService.GetQuoteAggregateIdForQuoteIdOrPolicyId(recordId);
            await this.mediator.Send(new MoveQuoteAggregateToOrganisationCommand(
                tenantModel.Id, aggregateId, organisationModel.Id));
            return this.Ok($"Aggregate {aggregateId} has been moved to the organisation \"{organisationModel.Name}\".");
        }

        /// <summary>
        /// Regenerates the quote search index for a tenant or all tenants.
        /// Note, there is a separate search index for each node, so you must run this separately against each node.
        /// </summary>
        /// <param name="tenant">The Id or Alias of the tenant. Omit this to regenerate quote indexes for all tenants.</param>
        /// <param name="environment">The environment.</param>
        /// <returns>Response.</returns>
        [HttpPost]
        [MustBeLoggedIn]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [Route("regenerateQuotesLuceneIndexes")]
        public IActionResult RegenerateQuotesLuceneIndexes(string tenant, string environment, CancellationToken cancellationToken)
        {
            this.authorisationService.ThrowIfUserIsNotFromMasterTenant(this.User, "regenerate lucene indexes");
            var isSuccess = Enum.TryParse(environment, true, out DeploymentEnvironment env);
            if (!isSuccess)
            {
                return this.NotFound($"Environment \"{environment}\" not found");
            }

            if (string.IsNullOrEmpty(tenant))
            {
                this.logger.LogInformation("Running RegenerateQuoteLuceneIndexCommand for all tenants, as a background task.");
                Task.Run(() => this.mediator.Send(new RegenerateQuoteLuceneIndexCommand(env, null), cancellationToken));
            }
            else
            {
                this.logger.LogInformation(
                    "Running RegenerateQuoteLuceneIndexCommand for tenant {0}, as a background task.",
                    tenant);
                var tenantModel = this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant)).GetAwaiter().GetResult();
                Task.Run(() => this.mediator.Send(new RegenerateQuoteLuceneIndexCommand(env, new List<Tenant> { tenantModel }), cancellationToken));
            }

            return this.Ok("Regeneration of quote indexes has begun as a background task. Check the application log for progress.");
        }

        /// <summary>
        /// Regenerates the policy search index for a tenant or all tenants.
        /// Note, there is a separate search index for each node, so you must run this separately against each node.
        /// </summary>
        /// <param name="tenant">The Id or Alias of the tenant. Omit this to regenerate policy indexes for all tenants.</param>
        /// <param name="environment">The environment.</param>
        /// <returns>Response.</returns>
        [HttpPost]
        [MustBeLoggedIn]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [Route("regeneratePoliciesLuceneIndexes")]
        public IActionResult RegeneratePoliciesLuceneIndexes(string tenant, string environment, CancellationToken cancellationToken)
        {
            this.authorisationService.ThrowIfUserIsNotFromMasterTenant(this.User, "regenerate lucene indexes");
            var isSuccess = Enum.TryParse(environment, true, out DeploymentEnvironment env);
            if (!isSuccess)
            {
                return this.NotFound($"Environment \"{environment}\" not found");
            }

            if (string.IsNullOrEmpty(tenant))
            {
                this.logger.LogInformation("Running RegeneratePolicyLuceneIndexCommand for all tenants, as a background task.");
                Task.Run(() => this.mediator.Send(new RegeneratePolicyLuceneIndexCommand(env, null), cancellationToken));
            }
            else
            {
                this.logger.LogInformation(
                    "Running RegeneratePolicyLuceneIndexCommand for tenant {0}, as a background task.",
                    tenant);
                var tenantModel = this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant)).GetAwaiter().GetResult();
                Task.Run(() => this.mediator.Send(new RegeneratePolicyLuceneIndexCommand(env, new List<Tenant> { tenantModel }), cancellationToken));
            }

            return this.Ok("Regeneration of policy indexes has begun as a background task. Check the application log for progress.");
        }

        /// <summary>
        /// Expire all user sessions.
        /// Forces all users to re-login after a release.
        /// </summary>
        /// <returns>Ok.</returns>
        [HttpDelete]
        [MustBeLoggedIn]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [Route("/api/v1/maintenance/user/session")]
        public IActionResult ExpireAllUserSessions()
        {
            this.authorisationService.ThrowIfUserIsNotFromMasterTenant(this.User, "Expire all user sessions");
            var command = new ExpireAllUserSessionsCommand();
            Task.Run(() => this.mediator.Send(command), default);
            return this.Ok("All user sessions have been expired.");
        }

        /// <summary>
        /// Update invalid FileContents using the FileContentId from the DocumentFiles table.
        /// This can be used when we can't open the downloaded file from Documents page because the FileContents are invalid/corrupt
        /// but we're still able to open the document/attachment via the Messages > Attachment page.
        /// </summary>
        /// <param name="policyId">PolicyId from Documents, e.g. api/v1/Production/policy/3e10832d-c8ea-4cf8-ab52-856ef55d5ce1/transaction/...</param>
        /// <param name="emailId">The EmailId when downloading from Messages page, e.g. /api/v1/email/dc35e906-41fd-4be6-a9b2-91fbd0fe8138/attachment/...</param>
        /// <returns>Response that invalid document file contents have been updated.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [MustBeLoggedIn]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [Route("updateInvalidDocumentFileContents")]
        public async Task<IActionResult> UpdateInvalidDocumentFileContents(Guid policyId, Guid emailId)
        {
            await this.authorisationService.ThrowIfUserIsNotFromMasterTenant(this.User, "update invalid document file contents");
            await this.mediator.Send(new UpdateInvalidDocumentFileContentsCommand(
                policyId, emailId));
            return this.Ok("Updated invalid document file contents, you should now be able open the downloaded document.");
        }

        /// <summary>
        /// Fixes invalid quote question set attachments based on provided attachment names.
        /// </summary>
        /// <remarks>
        /// The method accepts an array of attachment names in the request body and uses them to fix
        /// serialized data related to quote question set attachments.
        /// </remarks>
        /// <param name="attachmentNames">An array of attachment names to identify and fix invalid attachments.</param>
        /// <returns>A response indicating whether the operation was successful.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [MustBeLoggedIn]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [Route("fix-invalid-quote-question-set-attachments")]
        public async Task<IActionResult> FixInvalidQuoteQuestionSetAttachments([FromBody] string[] attachmentNames)
        {
            await this.authorisationService.ThrowIfUserIsNotFromMasterTenant(this.User, "fix invalid question set attachments");
            await this.mediator.Send(new FixInvalidQuestionSetAttachmentsCommand(attachmentNames));
            return this.Ok("A background job has been queued to fix the attachments.");
        }

        /// <summary>
        /// Invalidates releases in the global release cache, which will cause the releases to be reloaded from the database on next use.
        /// </summary>
        /// <param name="tenant">The tenant ID or alias. Leave empty to delete releases from all tenants.</param>
        /// <param name="product">The product ID or alias. Leave this empty to delete releases for all products.</param>
        /// <param name="environment">The environment. Leave this empty to delete releases in all environments.</param>
        /// <param name="productRelease">The product release ID or number. Provide a value for this if you need to delete a specific release only.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPost]
        [MustBeLoggedIn]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [Route("invalidate-release-cache")]
        public async Task<IActionResult> InvalidateReleaseCache(
            [FromQuery] string? tenant,
            [FromQuery] string? product,
            [FromQuery] DeploymentEnvironment? environment,
            [FromQuery] string? productRelease)
        {
            await this.authorisationService.ThrowIfUserIsNotFromMasterTenant(this.User, "invalidate cached releases");
            Guid? tenantId = tenant != null ? await this.cachingResolver.GetTenantIdOrThrow(new GuidOrAlias(tenant)) : null;
            Guid? productId = null;
            if (product != null && !tenantId.HasValue)
            {
                throw new ArgumentException(
                    "Product was specified but tenant was not specified. If you speficy a product you must also specify a tenant.");
            }
            else if (product != null && tenantId.HasValue)
            {
                productId = await this.cachingResolver.GetProductIdOrThrow(tenantId.Value, new GuidOrAlias(product));
            }

            Guid? productReleaseId = null;
            if (productRelease != null && !productId.HasValue)
            {
                throw new ArgumentException(
                    "Product release was specified but product was not specified. If you speficy a product release you must also specify a product.");
            }
            else if (productRelease != null && productId.HasValue)
            {
                productReleaseId = await this.cachingResolver.GetProductReleaseIdOrThrow(
                    tenantId.Value,
                    productId.Value,
                    new GuidOrAlias(productRelease));
            }

            int numberCleared = await this.mediator.Send(new InvalidateReleaseCacheCommand(tenantId, productId, environment, productReleaseId));
            return this.Ok($"{numberCleared} releases were removed from the global release cache.");
        }

        /// <summary>
        /// Patches a policy date inside the aggregate event
        /// </summary>
        /// <param name="policyId">The Id of the policy</param>
        /// <param name="dateName">The name of the date to be updated. Expected values are
        /// ("inceptionDate", "expiryDate", "effectiveDate")</param>
        /// <param name="dateUpdateModel">The payload that represents the date</param>
        /// <returns></returns>
        [HttpPatch]
        [MustBeLoggedIn]
        [Route("/api/v1/policy/{policyId}/{dateName}")]
        public async Task<IActionResult> UpdateAggregatePolicyDate(
            Guid policyId,
            PolicyDateType dateName,
            [FromBody] PolicyDateUpdateModel dateUpdateModel)
        {
            // If the user has no EndorseQuotes permission and is not from the master tenant, throw an error.
            var hasEndorseQuotesPermission = await this.mediator.Send(
                new PrincipalHasPermissionQuery(this.User, Permission.EndorseQuotes));
            var isUserFromMasterTenant = this.authorisationService.IsUserFromMasterTenant(this.User);
            if (!hasEndorseQuotesPermission && !isUserFromMasterTenant)
            {
                throw new ErrorException(Errors.General.NotAuthorized("update policy date"));
            }

            await this.authorisationService.ThrowIfUserCannotModifyPolicy(this.User, policyId);
            var tenantId = this.User.GetTenantId();
            var userId = this.User.GetId().Value;

            // If the dateName provided is not valid, throw an error
            // Valid values are "inceptionDate", "expiryDate", "effectiveDate"
            if (dateName == PolicyDateType.Invalid)
            {
                var errorData = new JObject()
                {
                    { "tenantId", tenantId },
                    { "policyId", policyId },
                };

                throw new ErrorException(Errors.Policy.DatePatching.InvalidDateName(dateName.Humanize(), errorData));
            }

            var policy = await this.mediator.Send(new GetPolicySummaryByIdQuery(tenantId, policyId));
            if (policy == null)
            {
                throw new ErrorException(Errors.Policy.NotFound(policyId));
            }

            var productReleaseId = await this.mediator.Send(
            new GetProductReleaseIdQuery(
                tenantId,
                policy.ProductId,
                policy.Environment,
                policy.QuoteType,
                policyId));
            LocalDate localDate;
            LocalTime? localTime = null;

            try
            {
                localDate = new LocalDate(dateUpdateModel.Year, dateUpdateModel.Month, dateUpdateModel.Day);
            }
            catch (Exception ex)
            {
                var errorData = new JObject()
                {
                    { "tenantId", tenantId },
                    { "policyId", policyId },
                };
                throw new ErrorException(
                    Errors.Policy.DatePatching.InvalidDate(policyId.ToString(), dateName.Humanize(), errorData), ex);
            }

            if (dateUpdateModel.Hour.HasValue)
            {
                try
                {
                    localTime = new LocalTime(
                        dateUpdateModel.Hour.Value,
                        dateUpdateModel.Minute ?? 0,
                        dateUpdateModel.Second ?? 0);
                }
                catch (Exception ex)
                {
                    var errorData = new JObject()
                    {
                        { "tenantId", tenantId },
                        { "policyId", policyId },
                    };
                    throw new ErrorException(
                        Errors.Policy.DatePatching.InvalidTime(policyId.ToString(), dateName.Humanize(), errorData), ex);
                }
            }

            var command = new UpdatePolicyDateCommand(
                tenantId,
                policyId,
                productReleaseId,
                userId,
                dateName,
                localDate,
                localTime);

            var newDate = await this.mediator.Send(command);
            var newDateAet = newDate.ToTargetTimeZone(DateTimeZone.Utc, Timezones.AET);
            return this.Ok($"The {dateName.Humanize()} of Policy '{policyId}' has been successfully updated " +
                $"to {newDateAet:MMMM, dd, yyyy hh:mm:ss tt} (AET).");
        }
    }
}
