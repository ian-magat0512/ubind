// <copyright file="AutomationEventTriggerService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Humanizer;
    using UBind.Application.Automation.Data;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Events;
    using UBind.Domain.Processing;
    using UBind.Domain.ReadModel;

    /// <inheritdoc/>
    public class AutomationEventTriggerService : IAutomationEventTriggerService
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IServiceProvider serviceProvider;
        private readonly IAutomationConfigurationProvider automationConfigurationProvider;
        private readonly IJobClient backgroundJobClient;
        private readonly IQuoteReadModelRepository quoteReadModelRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationEventTriggerService"/> class.
        /// </summary>
        /// <param name="automationConfigurationProvider">The automation configuration provider.</param>
        /// <param name="backgroundJobClient">A client for queing integration jobs.</param>
        /// <param name="serviceProvider">The service provider.</param>
        public AutomationEventTriggerService(
            IAutomationConfigurationProvider automationConfigurationProvider,
            IJobClient backgroundJobClient,
            IServiceProvider serviceProvider,
            ICachingResolver cachingResolver,
            IQuoteReadModelRepository quoteReadModelRepository)
        {
            this.cachingResolver = cachingResolver;
            this.serviceProvider = serviceProvider;
            this.automationConfigurationProvider = automationConfigurationProvider;
            this.backgroundJobClient = backgroundJobClient;
            this.quoteReadModelRepository = quoteReadModelRepository;
        }

        /// <inheritdoc/>
        public async Task HandleSystemEvent(SystemEvent systemEvent, CancellationToken cancellationToken)
        {
            // TEMPORARY - wait before processing the event to ensure that the quote aggregate has been persisted
            // This is not the correct solution, but it's quick fix. Full fix will be in UB-10052.
            await Task.Delay(2000, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            var environment = systemEvent.Environment;
            var productIdForIag = await this.GetHardcodeProductIdForIag(systemEvent);
            if (productIdForIag.HasValue)
            {
                systemEvent.SetProductIdForIag(productIdForIag);
                environment = DeploymentEnvironment.Development;
            }

            // TODO: currently we can only handle system events which specify a product Id and have an environment,
            // because currently you can only create automations against a product. In future you will be able to
            // create automations against an organisation or tenant, and so there will not be a need to specify a
            // product and environment for those.
            if (environment == DeploymentEnvironment.None || !systemEvent.ProductId.HasValue)
            {
                return;
            }

            var productReleaseId = this.GetProductReleaseId(systemEvent);
            AutomationsConfiguration? config = await this.automationConfigurationProvider.GetAutomationConfigurationOrNull(
                systemEvent.TenantId,
                systemEvent.ProductId.Value,
                environment,
                productReleaseId);
            if (config == null)
            {
                return;
            }

            var automationData = AutomationData.CreateFromSystemEvent(systemEvent, productReleaseId, this.serviceProvider);
            var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(systemEvent.TenantId);
            var productAlias = systemEvent.ProductId.HasValue
                ? await this.cachingResolver.GetProductAliasOrThrowAsync(systemEvent.TenantId, systemEvent.ProductId.Value)
                : string.Empty;
            var organisation = await this.cachingResolver.GetOrganisationOrNull(systemEvent.TenantId, systemEvent.OrganisationId);
            var organisationAlias = organisation?.Alias;
            foreach (Automation automation in config.Automations)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (await automation.DoesMatch(automationData))
                {
                    // trigger background job.
                    var jobId = this.backgroundJobClient.Enqueue<IAutomationService>(
                        c => c.TriggerSystemEventAutomation(
                            systemEvent.TenantId,
                            tenantAlias,
                            systemEvent.OrganisationId,
                            organisationAlias,
                            systemEvent.ProductId.Value,
                            productAlias,
                            environment,
                            systemEvent,
                            automation.Alias,
                            systemEvent.EventType.Humanize(),
                            productReleaseId,
                            CancellationToken.None),
                        systemEvent.TenantId,
                        systemEvent.ProductId.Value,
                        environment);
                }
            }
        }

        /// <summary>
        /// For IAG, we need to add some hardcode to the system event.
        /// Currently we can only handle system events which specify a product Id
        /// and have an environment. For IAG, user, organisation and tenant events will
        /// not have a product Id so we need to use a special product "iag-automations"
        /// which will contain automations of such events.
        /// </summary>
        /// <param name="systemEvent">The system event whose automation should be emitted.</param>
        /// <returns>System event with changes.</returns>
        private async Task<Guid?> GetHardcodeProductIdForIag(SystemEvent systemEvent)
        {
            Guid tenantId = systemEvent.TenantId;
            Guid? currentProductId = systemEvent.ProductId;
            if (currentProductId.GetValueOrDefault(default) != default)
            {
                // use the current product id if it has a value
                return null;
            }

            var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(tenantId);
            if (!tenantAlias.Equals("iag", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            var product = await this.cachingResolver.GetProductByAliasOrNull(tenantId, "iag-automations");
            return product != null ? product.Id : null;
        }

        private Guid? GetProductReleaseId(SystemEvent systemEvent)
        {
            foreach (var relationship in systemEvent.Relationships)
            {
                if (relationship.Type == RelationshipType.QuoteEvent)
                {
                    var quoteId = relationship.FromEntityId;
                    return this.quoteReadModelRepository.GetProductReleaseId(relationship.TenantId, quoteId);
                }
            }

            return null;
        }
    }
}
