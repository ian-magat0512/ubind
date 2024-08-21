// <copyright file="SystemEventConverterTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations
{
    using Newtonsoft.Json;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Triggers;
    using UBind.Domain.Events;
    using Xunit;

    public class SystemEventConverterTests
    {
        /// <summary>
        /// Tests backward compatibility.
        /// </summary>
        /// <param name="oldEventAlias">old Event Alias.</param>
        /// <param name="expectedEventType">Expected event type from alias.</param>
        [Theory]
        [InlineData("quoteInitialized", SystemEventType.QuoteCreated)]
        [InlineData("formDataUpdated", SystemEventType.QuoteFormDataUpdated)]
        [InlineData("customerAssigned", SystemEventType.QuoteCustomerAssociated)]
        [InlineData("calculationResultCreated", SystemEventType.QuoteCalculationResultCreated)]
        [InlineData("enquiryMade", SystemEventType.QuoteEnquiryMade)]
        [InlineData("quoteNumberAssigned", SystemEventType.QuoteReferenceAssigned)]
        [InlineData("quoteMigrated", SystemEventType.QuoteImported)]
        [InlineData("quoteDiscard", SystemEventType.QuoteDiscarded)]
        [InlineData("workflowStepAssigned", SystemEventType.QuoteWorkflowStepChanged)]
        [InlineData("quoteRollback", SystemEventType.QuoteRolledBack)]
        [InlineData("fileAttached", SystemEventType.QuoteFileAttached)]
        [InlineData("policyDataPatched", SystemEventType.PolicyFormDataUpdated)]
        [InlineData("claimInitialized", SystemEventType.ClaimCreated)]
        [InlineData("quoteDocumentGenerated", SystemEventType.DocumentAttachedToQuote)]
        [InlineData("quoteVersionDocumentGenerated", SystemEventType.DocumentAttachedToQuoteVersion)]
        [InlineData("policyDocumentGenerated", SystemEventType.DocumentAttachedToPolicy)]
        public void SystemEventConverter_BackwardCompatible_IfUsingOldEventAlias(string oldEventAlias, SystemEventType expectedEventType)
        {
            // Arrange
            var json = "{'name':'test', 'alias':'test', 'description':'test', 'customerEventAlias':'test','eventType':'" + oldEventAlias + "'}";

            // Action
            var trigger = JsonConvert.DeserializeObject<EventTriggerConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);

            // Assert
            Assert.Equal(trigger.EventType, expectedEventType);
        }
    }
}
