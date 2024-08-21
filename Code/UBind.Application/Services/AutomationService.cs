// <copyright file="AutomationService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Security.Claims;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json.Linq;
    using StackExchange.Profiling;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Data;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Helper;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.File;
    using UBind.Application.Automation.Triggers;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Releases;
    using UBind.Application.SystemEvents;
    using UBind.Domain;
    using UBind.Domain.Events;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;

    /// <inheritdoc/>
    public class AutomationService : IAutomationService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ICachingResolver cachingResolver;
        private readonly IAutomationConfigurationProvider configurationProvider;
        private readonly ISystemEventService systemEventService;
        private readonly IAutomationPortalPageTriggerService portalPageTriggerService;
        private readonly IReleaseQueryService releaseQueryService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationService"/> class.
        /// </summary>
        /// <param name="configurationProvider">The automations configuration provider.</param>
        /// <param name="systemEventService">The system event service.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        public AutomationService(
            IAutomationConfigurationProvider configurationProvider,
            ISystemEventService systemEventService,
            IServiceProvider serviceProvider,
            ICachingResolver cachingResolver,
            IAutomationPortalPageTriggerService portalPageTriggerService,
            IReleaseQueryService releaseQueryService)
        {
            this.serviceProvider = serviceProvider;
            this.cachingResolver = cachingResolver;
            this.configurationProvider = configurationProvider;
            this.systemEventService = systemEventService;
            this.portalPageTriggerService = portalPageTriggerService;
            this.releaseQueryService = releaseQueryService;
        }

        /// <inheritdoc/>
        public async Task<AutomationData> TriggerHttpAutomation(
            ReleaseContext releaseContext,
            Guid organisationId,
            TriggerRequest triggerRequest,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (MiniProfiler.Current.Step(nameof(AutomationService) + "." + nameof(this.TriggerHttpAutomation)))
            {
                var httpVerb = triggerRequest.HttpVerb;
                var isHttpFormatValid = this.ValidateHttpVerbFormat(httpVerb);
                if (!isHttpFormatValid)
                {
                    JObject errorData = new JObject();
                    errorData.Add("httpVerb", httpVerb);
                    throw new ErrorException(Errors.Automation.HttpRequest.InvalidHttpVerbFormat(httpVerb, errorData));
                }

                var config = await this.configurationProvider.GetAutomationConfigurationOrThrow(
                        releaseContext.TenantId,
                        releaseContext.ProductId,
                        releaseContext.Environment,
                        releaseContext.ProductReleaseId);

                if (config == null)
                {
                    var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(releaseContext.TenantId);
                    var productAlias = await this.cachingResolver.GetProductAliasOrThrowAsync(releaseContext.TenantId, releaseContext.ProductId);
                    var errorData = GenericErrorDataHelper.GetGeneralErrorDetails(releaseContext.TenantId, releaseContext.ProductId, releaseContext.Environment);
                    throw new ErrorException(Errors.Automation.AutomationConfigurationUnavailable(tenantAlias, productAlias, releaseContext.Environment, errorData));
                }

                var data = AutomationData.CreateFromHttpRequest(
                    releaseContext.TenantId,
                    organisationId,
                    releaseContext.ProductId,
                    releaseContext.ProductReleaseId,
                    releaseContext.Environment,
                    triggerRequest,
                    this.serviceProvider);
                var triggerData = data.Trigger as HttpTriggerData;
                var (automation, trigger) = await config.GetClosestMatchingHttpTrigger(data);
                if (automation == null)
                {
                    var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(releaseContext.TenantId);
                    var productAlias = await this.cachingResolver.GetProductAliasOrThrowAsync(
                        releaseContext.TenantId,
                        releaseContext.ProductId);
                    var errorData = GenericErrorDataHelper.GetGeneralErrorDetails(
                        releaseContext.TenantId,
                        releaseContext.ProductId,
                        releaseContext.Environment);
                    errorData.Add(ErrorDataKey.TriggerAlias, data.Trigger.TriggerAlias);
                    errorData.Add("endpointUrl", triggerData.HttpRequest.Url);
                    errorData.Add("httpMethod", triggerData.HttpRequest.HttpVerb.ToString());

                    throw new ErrorException(
                        Errors.Automation.AutomationNotFound(
                            tenantAlias,
                            productAlias,
                            releaseContext.Environment,
                            errorData));
                }

                // perform request validation
                var providerContext = new ProviderContext(data);
                await trigger.Endpoint.ValidateAndThrowIfFail(providerContext);

                // detect path parameters
                triggerData.HttpRequest.DetectPathParameters(trigger.Endpoint.Path);

                await this.ExecuteWithRequestIntent(automation, data, trigger, cancellationToken);
                return data;
            }
        }

        /// <inheritdoc/>
        [DisplayName("Trigger System Event Automation | TENANT: {1}, ORGANISATION: {3}, PRODUCT: {5}, ENVIRONMENT: {6}, AUTOMATION: {8}, EVENT: {9}")]
        public async Task TriggerSystemEventAutomation(
            Guid tenantId,
            string tenantAlias,
            Guid organisationId,
            string? organisationAlias,
            Guid productId,
            string productAlias,
            DeploymentEnvironment environment,
            SystemEvent systemEvent,
            string automationAlias,
            string eventType,
            Guid? productReleaseId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            AutomationData automationData = AutomationData.CreateFromSystemEvent(
                systemEvent,
                productReleaseId,
                this.serviceProvider);

            Automation? automation = await this.GetAutomation(tenantId, productId, environment, productReleaseId, automationAlias);

            if (automation == null)
            {
                if (tenantAlias.IsNullOrEmpty())
                {
                    tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(tenantId);
                }

                if (productAlias.IsNullOrEmpty())
                {
                    productAlias = await this.cachingResolver.GetProductAliasOrThrowAsync(tenantId, productId);
                }

                JObject errorData = this.GetSystemEventErrorData(tenantId, productId, environment, automationAlias, automationData);
                throw new ErrorException(
                    Errors.Automation.AutomationNotFound(
                        tenantAlias, productAlias, environment, errorData));
            }

            var resolveMatchingTrigger = await automation.Triggers.SelectAsync(async t => await t.DoesMatch(automationData) ? t : null);
            var matchingTrigger = resolveMatchingTrigger.FirstOrDefault(t => t != null);
            if (matchingTrigger == null)
            {
                if (tenantAlias.IsNullOrEmpty())
                {
                    tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(tenantId);
                }

                if (productAlias.IsNullOrEmpty())
                {
                    productAlias = await this.cachingResolver.GetProductAliasOrThrowAsync(tenantId, productId);
                }

                JObject errorData = this.GetSystemEventErrorData(tenantId, productId, environment, automationAlias, automationData);
                throw new ErrorException(
                    Errors.Automation.AutomationTriggerNotFound(
                        tenantAlias, productAlias, environment, errorData));
            }

            this.SetAutomationDataContext(automationData, systemEvent);

            await this.ExecuteWithRequestIntent(automation, automationData, matchingTrigger, cancellationToken);
            if (automationData.Error != null)
            {
                throw new ErrorException(automationData.Error);
            }
        }

        public async Task TriggerPeriodicAutomation(
            Guid tenantId,
            Guid organisationId,
            Guid productId,
            DeploymentEnvironment environment,
            string automationAlias,
            CancellationToken cancellationToken)
        {
            var productReleaseId = this.releaseQueryService.GetDefaultProductReleaseIdOrNull(tenantId, productId, environment);
            if (productReleaseId == null)
            {
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();
            using (MiniProfiler.Current.Step(nameof(AutomationService) + "." + nameof(this.TriggerPeriodicAutomation)))
            {
                Automation? automation = await this.GetAutomation(tenantId, productId, environment, productReleaseId, automationAlias);
                if (automation != null)
                {
                    var data = AutomationData.CreateFromPeriodicTrigger(
                        tenantId,
                        organisationId,
                        productId,
                        productReleaseId.Value,
                        environment,
                        this.serviceProvider);

                    await this.ExecuteWithRequestIntent(automation, data, null, cancellationToken);
                }
            }
        }

        public async Task<(FileInfo file, string successMessage)> TriggerPortalPageAutomation(
            Guid tenantId,
            Guid organisationId,
            Guid? productId,
            DeploymentEnvironment environment,
            string automationAlias,
            string triggerAlias,
            EntityType entityType,
            PageType pageType,
            string tab,
            ClaimsPrincipal user,
            EntityListFilters filters,
            Guid entityId,
            CancellationToken cancellationToken)
        {
            Guid? productReleaseId = productId.HasValue
                ? this.releaseQueryService.GetDefaultProductReleaseIdOrNull(tenantId, productId.Value, environment)
                : null;
            cancellationToken.ThrowIfCancellationRequested();
            using (MiniProfiler.Current.Step($"{nameof(AutomationService)}.{nameof(this.TriggerPortalPageAutomation)}"))
            {
                if (productId == null)
                {
                    // for now we have to throw an error, until we support defining automations outside a product.
                    throw new ErrorException(Errors.Automation.PortalPageTrigger.ProductIdRequired(automationAlias, triggerAlias));
                }

                var automation = await this.GetAutomation(tenantId, productId.Value, environment, null, automationAlias);
                if (automation != null)
                {
                    var automationData = AutomationData.CreateFromPortalPageTriggerRequest(
                        tenantId,
                        organisationId,
                        productId,
                        productReleaseId,
                        environment,
                        triggerAlias,
                        this.serviceProvider,
                        entityType,
                        pageType,
                        tab);

                    automationData.Automation[AutomationData.ProductId] = productId;

                    var resolveMatchingTrigger = await automation.Triggers.SelectAsync(async t => await t.DoesMatch(automationData) ? t : null);
                    var matchingTrigger = resolveMatchingTrigger.FirstOrDefault(t => t != null);
                    if (matchingTrigger is PortalPageTrigger trigger)
                    {
                        filters.Page = null;
                        var portalPageData = new PortalPageData(
                            tenantId, organisationId, productId.Value, environment, user.GetId(), entityType, pageType, tab, filters, entityId);

                        this.SetAutomationDataContext(automationData, portalPageData);
                        await this.ExecuteWithRequestIntent(automation, automationData, trigger, cancellationToken);
                        var triggerData = automationData.Trigger as PortalPageTriggerData;
                        return (triggerData.DownloadFile, triggerData.SuccessSnackbarText);
                    }
                }
            }

            throw new ErrorException(Errors.Automation.PortalPageTrigger.NotFound(automationAlias, triggerAlias));
        }

        /// <inheritdoc/>
        public void SetAutomationDataContext(AutomationData automationData, SystemEvent systemEvent)
        {
            automationData.ContextManager.SetContextFromSystemEvent(systemEvent);
            automationData.ContextManager.SetContextFromEventRelationships(systemEvent.Relationships);
        }

        private void SetAutomationDataContext(AutomationData automationData, PortalPageData portalPageData)
        {
            if (portalPageData.PageType == PageType.List)
            {
                automationData.ContextManager.SetContextFromPortalPageTrigger(portalPageData);
            }
            else if (portalPageData.PageType == PageType.Display)
            {
                var entityDetails = this.portalPageTriggerService.GetEntityDisplay(portalPageData);
                automationData.ContextManager.SetContextFromPortalPageTrigger(entityDetails);
            }
        }

        private async Task<Automation?> GetAutomation(
            Guid tenantId,
            Guid productId,
            DeploymentEnvironment environment,
            Guid? productReleaseId,
            string alias)
        {
            var config =
                await this.configurationProvider.GetAutomationConfigurationOrNull(
                    tenantId,
                    productId,
                    environment,
                    productReleaseId);
            var automation = config?.Automations.FirstOrDefault(x => x.Alias == alias);
            return automation;
        }

        private JObject GetSystemEventErrorData(Guid tenantId, Guid productId, DeploymentEnvironment environment, string alias, AutomationData automationData)
        {
            EventTriggerData triggerData = automationData.Trigger as EventTriggerData;
            var triggerEvent = triggerData.EventType.Equals(SystemEventType.Custom.ToString().ToCamelCase())
                ? triggerData.CustomEventAlias : triggerData.EventType;

            JObject errorData = GenericErrorDataHelper.GetGeneralErrorDetails(tenantId, productId, environment);
            errorData.Add(ErrorDataKey.Automation, alias);
            errorData.Add(ErrorDataKey.TriggerAlias, automationData.Trigger.TriggerAlias);
            errorData.Add("event", triggerEvent);
            return errorData;
        }

        private bool ValidateHttpVerbFormat(string httpVerb)
        {
            var verbValidator = new Regex(@"^([a-zA-Z]+(\-[a-zA-Z]+)*)$", RegexOptions.IgnoreCase);
            return verbValidator.IsMatch(httpVerb);
        }

        private async Task ExecuteWithRequestIntent(
            Automation automation,
            AutomationData automationData,
            Trigger trigger,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!automation.IsReadOnly())
            {
                // since it's read-only by default, we need to create a new scope with read-write intent
                using (var serviceScope = this.serviceProvider.CreateScope())
                {
                    var httpContextAccessor = this.serviceProvider.GetRequiredService<IHttpContextAccessor>();
                    if (httpContextAccessor?.HttpContext != null)
                    {
                        var httpContext = httpContextAccessor.HttpContext;
                        httpContext.Items[nameof(RequestIntent)] = RequestIntent.ReadWrite;
                    }
                    else
                    {
                        var requestContext = serviceScope.ServiceProvider.GetRequiredService<ICqrsRequestContext>();
                        requestContext.RequestIntent = RequestIntent.ReadWrite;
                    }

                    await automation.Execute(automationData, trigger, cancellationToken);
                }
            }
            else
            {
                await automation.Execute(automationData, trigger, cancellationToken);
            }
        }
    }
}
