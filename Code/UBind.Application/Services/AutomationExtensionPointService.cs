// <copyright file="AutomationExtensionPointService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using StackExchange.Profiling;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Data;
    using UBind.Application.Automation.Triggers.ExtensionPointTrigger;
    using UBind.Application.SystemEvents;
    using UBind.Domain;
    using UBind.Domain.Configuration;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Product;

    /// <inheritdoc/>
    public class AutomationExtensionPointService : IAutomationExtensionPointService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ICachingResolver cachingResolver;
        private readonly IAutomationConfigurationProvider configurationProvider;
        private readonly ISystemEventService systemEventService;
        private readonly IAutomationPortalPageTriggerService portalPageTriggerService;
        private readonly IFormDataPrettifier formDataPrettifier;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationExtensionPointService"/> class.
        /// </summary>
        /// <param name="configurationProvider">The automations configuration provider.</param>
        /// <param name="systemEventService">The system event service.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        public AutomationExtensionPointService(
            IAutomationConfigurationProvider configurationProvider,
            ISystemEventService systemEventService,
            IServiceProvider serviceProvider,
            ICachingResolver cachingResolver,
            IAutomationPortalPageTriggerService portalPageTriggerService,
            IFormDataPrettifier formDataPrettifier)
        {
            this.serviceProvider = serviceProvider;
            this.cachingResolver = cachingResolver;
            this.configurationProvider = configurationProvider;
            this.systemEventService = systemEventService;
            this.portalPageTriggerService = portalPageTriggerService;
            this.formDataPrettifier = formDataPrettifier;
        }

        /// <inheritdoc/>
        public async Task<JObject> TriggerBeforeQuoteCalculation(
            UBind.Domain.Aggregates.Quote.Quote? quote,
            ReleaseContext releaseContext,
            IProductConfiguration productConfiguration,
            JObject inputData,
            Guid? organisationId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (MiniProfiler.Current.Step(nameof(AutomationService) + "." + nameof(this.TriggerBeforeQuoteCalculation)))
            {
                var product = await this.cachingResolver.GetProductOrThrow(releaseContext.TenantId, releaseContext.ProductId);

                var config = await this.configurationProvider.GetAutomationConfigurationOrNull(
                        product.TenantId,
                        product.Id,
                        releaseContext.Environment,
                        releaseContext.ProductReleaseId);

                if (config == null)
                {
                    return null;
                }

                AutomationData data = BeforeQuoteCalculationExtensionPointTrigger.CreateAutomationData(
                    releaseContext,
                    quote,
                    organisationId,
                    productConfiguration,
                    this.formDataPrettifier,
                    this.serviceProvider,
                    ExtensionPointType.BeforeQuoteCalculation,
                    inputData);

                JObject outputData = inputData;
                bool createNewAutomationData = false;
                bool outputDataChanged = false;
                foreach (Automation automation in config.Automations)
                {
                    if (createNewAutomationData)
                    {
                        data = BeforeQuoteCalculationExtensionPointTrigger.CreateAutomationData(
                            releaseContext,
                            quote,
                            organisationId,
                            productConfiguration,
                            this.formDataPrettifier,
                            this.serviceProvider,
                            ExtensionPointType.BeforeQuoteCalculation,
                            outputData);
                        createNewAutomationData = false;
                    }

                    var trigger = await automation.GetFirstMatchingTrigger(data);
                    if (trigger != null)
                    {
                        // add quote context here instead of on creation of automation data.
                        data.ContextManager.SetContextEntity(quote, productConfiguration, this.formDataPrettifier);
                        createNewAutomationData = true;
                        await automation.Execute(data, trigger, cancellationToken);
                        if (data.Error != null)
                        {
                            throw new ErrorException(data.Error);
                        }

                        if (data.Trigger is BeforeQuoteCalculationExtensionPointTriggerData triggerData)
                        {
                            if (triggerData.ReturnInputData != null)
                            {
                                outputData = triggerData.ReturnInputData;
                                outputDataChanged = true;
                            }
                        }
                    }
                }

                return outputDataChanged ? outputData : null;
            }
        }

        /// <inheritdoc/>
        public async Task<AfterQuoteCalculationExtensionResultModel> TriggerAfterQuoteCalculation(
            UBind.Domain.Aggregates.Quote.Quote? quote,
            ReleaseContext releaseContext,
            IProductConfiguration productConfiguration,
            JObject sourceInputData,
            JObject sourceCalculationResult,
            Guid? organisationId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (MiniProfiler.Current.Step(nameof(AutomationService) + "." + nameof(this.TriggerAfterQuoteCalculation)))
            {
                var product = await this.cachingResolver.GetProductOrThrow(releaseContext.TenantId, releaseContext.ProductId);

                var config = await this.configurationProvider.GetAutomationConfigurationOrNull(
                        product.TenantId,
                        product.Id,
                        releaseContext.Environment,
                        releaseContext.ProductReleaseId);

                if (config == null)
                {
                    return null;
                }

                AutomationData data = AfterQuoteCalculationExtensionPointTrigger.CreateAutomationData(
                    releaseContext,
                    quote,
                    organisationId,
                    productConfiguration,
                    this.formDataPrettifier,
                    this.serviceProvider,
                    ExtensionPointType.AfterQuoteCalculation,
                    sourceInputData,
                    sourceCalculationResult);

                JObject outputData = sourceInputData;
                JObject outputCalculationResult = sourceCalculationResult;
                bool createNewAutomationData = false;
                foreach (Automation automation in config.Automations)
                {
                    if (createNewAutomationData)
                    {
                        data = AfterQuoteCalculationExtensionPointTrigger.CreateAutomationData(
                            releaseContext,
                            quote,
                            organisationId,
                            productConfiguration,
                            this.formDataPrettifier,
                            this.serviceProvider,
                            ExtensionPointType.BeforeQuoteCalculation,
                            outputData,
                            outputCalculationResult);
                        createNewAutomationData = false;
                    }

                    var trigger = await automation.GetFirstMatchingTrigger(data);
                    if (trigger != null)
                    {
                        // add quote context here instead of on creation of automation data.
                        data.ContextManager.SetContextEntity(quote, productConfiguration, this.formDataPrettifier);
                        createNewAutomationData = true;
                        await automation.Execute(data, trigger, cancellationToken);
                        if (data.Error != null)
                        {
                            throw new ErrorException(data.Error);
                        }

                        if (data.Trigger is AfterQuoteCalculationExtensionPointTriggerData triggerData)
                        {
                            if (triggerData.ReturnCalculationResult != null)
                            {
                                outputCalculationResult = triggerData.ReturnCalculationResult;
                            }

                            if (triggerData.ReturnInputData != null)
                            {
                                outputData = triggerData.ReturnInputData;
                            }
                        }
                    }
                }

                return new AfterQuoteCalculationExtensionResultModel(outputData, outputCalculationResult);
            }
        }
    }
}
