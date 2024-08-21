// <copyright file="BeforeQuoteCalculationExtensionPointTrigger.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Triggers.ExtensionPointTrigger
{
    using System;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using StackExchange.Profiling;
    using UBind.Application.Automation.Data;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Configuration;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Product;

    /// <summary>
    /// Defines a before quote calculation extension point trigger with an object input.
    /// </summary>
    public class BeforeQuoteCalculationExtensionPointTrigger : ExtensionPointTrigger
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventTrigger"/> class.
        /// </summary>
        /// <param name="name">The name of the trigger.</param>
        /// <param name="alias">The alias of the trigger.</param>
        /// <param name="description">The trigger description.</param>
        /// <param name="extensionPointType">The extension point type.</param>
        /// <param name="runCondition">The run condition.</param>
        [JsonConstructor]
        public BeforeQuoteCalculationExtensionPointTrigger(
            string name,
            string alias,
            string description,
            ExtensionPointType extensionPointType,
            IObjectProvider? returnInputData,
            IProvider<Data<bool>> runCondition)
            : base(name, alias, description, extensionPointType, runCondition)
        {
            this.ReturnInputData = returnInputData;
        }

        public IObjectProvider? ReturnInputData { get; set; }

        public static AutomationData CreateAutomationData(
            ReleaseContext releaseContext,
            Quote? quote,
            Guid? organisationId,
            IProductConfiguration productConfiguration,
            IFormDataPrettifier formDataPrettifier,
            IServiceProvider serviceProvider,
            ExtensionPointType extensionPointType,
            JObject sourceInputData)
        {
            using (MiniProfiler.Current.Step(nameof(BeforeQuoteCalculationExtensionPointTrigger) + "." + nameof(CreateAutomationData)))
            {
                var triggerData = new BeforeQuoteCalculationExtensionPointTriggerData(extensionPointType, sourceInputData);
                var automationData = new AutomationData(
                    releaseContext.TenantId,
                    quote?.Aggregate?.OrganisationId ?? organisationId,
                    releaseContext.ProductId,
                    releaseContext.ProductReleaseId,
                    releaseContext.Environment,
                    triggerData,
                    serviceProvider);

                // set quote context.
                if (quote != null)
                {
                    automationData.ContextManager.SetContextEntity(quote, productConfiguration, formDataPrettifier);
                }

                return automationData;
            }
        }

        /// <inheritdoc/>
        public override Task<bool> DoesMatch(AutomationData dataContext)
        {
            return Task.FromResult(dataContext.Trigger is BeforeQuoteCalculationExtensionPointTriggerData);
        }

        public override async Task GenerateCompletionResponse(IProviderContext providerContext)
        {
            try
            {
                var automationData = providerContext.AutomationData;
                if (!(automationData.Trigger is BeforeQuoteCalculationExtensionPointTriggerData trigger))
                {
                    return;
                }

                var resolveReturnInputData = (await this.ReturnInputData.ResolveValueIfNotNull(providerContext))?.DataValue;
                var data = (JObject?)resolveReturnInputData;
                trigger.ReturnInputData = data;
            }
            catch (System.Exception ex) when (!(ex is ErrorException))
            {
                var errorData = await providerContext.GetDebugContext();
                throw new ErrorException(
                    Errors.Automation.HttpRequest.ExtensionPointGenerationError(
                         ex.Message, errorData), ex);
            }
        }
    }
}