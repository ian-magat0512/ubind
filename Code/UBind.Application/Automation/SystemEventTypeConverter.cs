// <copyright file="SystemEventTypeConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using UBind.Domain.Events;

    /// <summary>
    /// Converter SystemEventType input to a proper SystemEventType enum value. This allows backward compatibility.
    /// </summary>
    public class SystemEventTypeConverter : DeserializationConverter
    {
        /// <summary>
        /// This is the old system event types due to renames, to work with an updated codebase.
        /// Remark: Removing the mapping would remove the backward compatibility.
        /// </summary>
        private Dictionary<string, SystemEventType?> backwardCompatibilityMapping = new Dictionary<string, SystemEventType?>
        {
            { "quoteInitialized", SystemEventType.QuoteCreated },
            { "formDataUpdated", SystemEventType.QuoteFormDataUpdated },
            { "customerAssigned", SystemEventType.QuoteCustomerAssociated },
            { "calculationResultCreated", SystemEventType.QuoteCalculationResultCreated },
            { "enquiryMade", SystemEventType.QuoteEnquiryMade },
            { "quoteNumberAssigned", SystemEventType.QuoteReferenceAssigned },
            { "quoteMigrated", SystemEventType.QuoteImported },
            { "quoteDiscard", SystemEventType.QuoteDiscarded },
            { "workflowStepAssigned", SystemEventType.QuoteWorkflowStepChanged },
            { "quoteWorkflowStepAssigned", SystemEventType.QuoteWorkflowStepChanged },
            { "quoteRollback", SystemEventType.QuoteRolledBack },
            { "fileAttached", SystemEventType.QuoteFileAttached },
            { "policyDataPatched", SystemEventType.PolicyFormDataUpdated },
            { "claimInitialized", SystemEventType.ClaimCreated },
            { "associateClaimWithPolicy", SystemEventType.ClaimPolicyAssociationCreated },
            { "quoteDocumentGenerated", SystemEventType.DocumentAttachedToQuote },
            { "quoteVersionDocumentGenerated", SystemEventType.DocumentAttachedToQuoteVersion },
            { "policyDocumentGenerated", SystemEventType.DocumentAttachedToPolicy },
            { "customerOpenedExpiredQuote", SystemEventType.CustomerExpiredQuoteOpened },
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemEventTypeConverter"/> class.
        /// </summary>
        public SystemEventTypeConverter()
        {
        }

        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(SystemEventType) == objectType;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = reader.Value.ToString();

            this.backwardCompatibilityMapping.TryGetValue(value, out SystemEventType? mappedSystemEventType);

            if (mappedSystemEventType == null)
            {
                Enum.TryParse(value, true, out SystemEventType currentSystemEventType);

                return currentSystemEventType;
            }

            return mappedSystemEventType;
        }
    }
}
