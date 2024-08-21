// <copyright file="AfterQuoteCalculationExtensionPointTrigger.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Triggers.ExtensionPointTrigger
{
    using System;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Data;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Domain;
    using UBind.Domain.Configuration;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Product;

    /// <summary>
    /// Defines an after quote calculation extension point trigger with an object input.
    /// </summary>
    public class AfterQuoteCalculationExtensionPointTrigger : ExtensionPointTrigger
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
        public AfterQuoteCalculationExtensionPointTrigger(
            string name,
            string alias,
            string description,
            ExtensionPointType extensionPointType,
            IObjectProvider? returnInputData,
            IObjectProvider? returnCalculationResult,
            IProvider<Data<bool>>? runCondition)
            : base(name, alias, description, extensionPointType, runCondition)
        {
            this.ReturnCalculationResult = returnCalculationResult;
            this.ReturnInputData = returnInputData;
        }

        public IObjectProvider? ReturnCalculationResult { get; set; }

        public IObjectProvider? ReturnInputData { get; set; }

        public static AutomationData CreateAutomationData(
            ReleaseContext releaseContext,
            Domain.Aggregates.Quote.Quote? quote,
            Guid? organisationId,
            IProductConfiguration productConfig,
            IFormDataPrettifier formDataPrettifier,
            IServiceProvider serviceProvider,
            ExtensionPointType extensionPointType,
            JObject sourceInputData,
            JObject sourceCalculationResult)
        {
            var triggerData = new AfterQuoteCalculationExtensionPointTriggerData(extensionPointType, sourceInputData, sourceCalculationResult);
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
                automationData.ContextManager.SetContextEntity(quote, productConfig, formDataPrettifier);
            }

            return automationData;
        }

        /// <inheritdoc/>
        public override Task<bool> DoesMatch(AutomationData dataContext)
        {
            if (!(dataContext.Trigger is AfterQuoteCalculationExtensionPointTriggerData trigger))
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public override async Task GenerateCompletionResponse(IProviderContext providerContext)
        {
            try
            {
                var automationData = providerContext.AutomationData;
                if (!(automationData.Trigger is AfterQuoteCalculationExtensionPointTriggerData trigger))
                {
                    return;
                }

                var calculationResultResolved = (await this.ReturnCalculationResult.ResolveValueIfNotNull(providerContext))?.DataValue;
                trigger.ReturnCalculationResult = (JObject?)calculationResultResolved;

                var inputDataResolved = (await this.ReturnInputData.ResolveValueIfNotNull(providerContext))?.DataValue;
                trigger.ReturnInputData = (JObject?)inputDataResolved;
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