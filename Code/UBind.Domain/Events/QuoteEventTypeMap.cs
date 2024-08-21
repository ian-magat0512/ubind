// <copyright file="QuoteEventTypeMap.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Events
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Aggregates.Quote;

    /// <summary>
    /// For mapping quote aggregate events to event types.
    /// </summary>
    public static class QuoteEventTypeMap
    {
        private static readonly Dictionary<Type, List<ApplicationEventType>> TypeMap =
            new Dictionary<Type, List<ApplicationEventType>>
            {
                {
                    typeof(QuoteAggregate.ApplyNewIdEvent),
                    new List<ApplicationEventType>()
                    {
                        ApplicationEventType.NewTenantAndProductIdsSet,
                    }
                },
                {
                    typeof(QuoteAggregate.QuoteActualisedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.QuoteActualised,
                        }
                },
                {
                    typeof(QuoteAggregate.QuoteInitializedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.QuoteCreated,
                            ApplicationEventType.NewBusinessQuoteCreated,
                        }
                },
                {
                    typeof(QuoteAggregate.FormDataUpdatedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.FormUpdated,
                        }
                },
                {
                    typeof(QuoteAggregate.FormDataPatchedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.FormUpdated,
                        }
                },
                {
                    typeof(QuoteAggregate.CustomerAssignedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.AssignedToCustomer,
                        }
                },
                {
                    typeof(QuoteAggregate.QuoteCustomerAssociationInvitationCreatedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.QuoteCustomerAssociationInvitationCreatedEvent,
                        }
                },
                {
                    typeof(QuoteAggregate.OwnershipAssignedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.AssignedToOwner,
                        }
                },
                {
                    typeof(QuoteAggregate.OwnershipUnassignedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.UnassignedToOwner,
                        }
                },
                {
                    typeof(QuoteAggregate.CalculationResultCreatedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.Calculated,
                        }
                },
                {
                    typeof(QuoteAggregate.CustomerDetailsUpdatedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.CustomerDetailsUpdated,
                        }
                },
                {
                    typeof(QuoteAggregate.QuoteExpiryTimestampSetEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.QuoteExpiryTimeSet,
                        }
                },
                {
                    typeof(QuoteAggregate.QuoteSubmittedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.Submitted,
                        }
                },
                {
                    typeof(QuoteAggregate.EnquiryMadeEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.Enquired,
                        }
                },
                {
                    typeof(QuoteAggregate.QuoteSavedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.Saved,
                        }
                },
                {
                    typeof(QuoteAggregate.PolicyIssuedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.PolicyIssued,
                        }
                },
                {
                    typeof(QuoteAggregate.InvoiceIssuedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.InvoiceIssued,
                        }
                },
                {
                    typeof(QuoteAggregate.PaymentMadeEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.PaymentMade,
                        }
                },
                {
                    typeof(QuoteAggregate.PaymentFailedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.PaymentAttemptFailed,
                        }
                },
                {
                    typeof(QuoteAggregate.FundingProposalCreatedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.FundingProposalCreated,
                        }
                },
                {
                    typeof(QuoteAggregate.FundingProposalCreationFailedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.FundingProposalCreationFailed,
                        }
                },
                {
                    typeof(QuoteAggregate.FundingProposalAcceptedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.FundingProposalAccepted,
                        }
                },
                {
                    typeof(QuoteAggregate.FundingProposalAcceptanceFailedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.FundingProposalAcceptanceFailed,
                        }
                },
                {
                    typeof(QuoteAggregate.QuoteNumberAssignedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.QuoteNumberAssigned,
                        }
                },
                {
                    typeof(QuoteAggregate.QuoteVersionCreatedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.QuoteVersionCreated,
                        }
                },
                {
                    typeof(QuoteAggregate.FileAttachedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.FileAttached,
                        }
                },
                {
                    typeof(QuoteAggregate.PolicyCancelledEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.PolicyCancelled,
                        }
                },
                {
                    typeof(QuoteAggregate.PolicyNumberUpdatedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.PolicyNumberUpdated,
                        }
                },
                {
                    typeof(QuoteAggregate.QuoteDocumentGeneratedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.QuoteDocumentGenerated,
                        }
                },
                {
                    typeof(QuoteAggregate.QuoteVersionDocumentGeneratedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.QuoteVersionDocumentGenerated,
                        }
                },
                {
                    typeof(QuoteAggregate.PolicyDocumentGeneratedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.PolicyDocumentGenerated,
                        }
                },
                {
                    typeof(QuoteAggregate.QuoteMigratedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.QuoteImported,
                        }
                },
                {
                    typeof(QuoteAggregate.QuoteImportedEvent),
                    new List<ApplicationEventType>()
                    {
                        ApplicationEventType.QuoteImported,
                    }
                },
                {
                    typeof(QuoteAggregate.PolicyImportedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.PolicyImported,
                        }
                },
                {
                    typeof(QuoteAggregate.PolicyIssuedWithoutQuoteEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.PolicyIssued,
                        }
                },
                {
                    typeof(QuoteAggregate.AggregateCreationFromPolicyEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.AggregateCreationFromPolicy,
                        }
                },
                {
                    typeof(QuoteAggregate.SetPolicyTransactionsEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.SetPolicyTransaction,
                        }
                },
                {
                    typeof(QuoteAggregate.PolicyAdjustedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.PolicyAdjusted,
                        }
                },
                {
                    typeof(QuoteAggregate.PolicyDeletedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.PolicyDeleted,
                        }
                },
                {
                    typeof(QuoteAggregate.PolicyDataPatchedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.PolicyFormDataUpdated,
                        }
                },
                {
                    typeof(QuoteAggregate.QuoteDiscardEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.QuoteDiscarded,
                        }
                },
                {
                    typeof(QuoteAggregate.WorkflowStepAssignedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.WorkflowStepChanged,
                        }
                },
                {
                    typeof(QuoteAggregate.QuoteEmailGeneratedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.QuoteEmailGeneratedEvent,
                        }
                },
                {
                    typeof(QuoteAggregate.PolicyEmailGeneratedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.PolicyEmailGeneratedEvent,
                        }
                },
                {
                    typeof(QuoteAggregate.QuoteEmailSentEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.QuoteEmailSentEvent,
                        }
                },
                {
                    typeof(QuoteAggregate.AdjustmentQuoteCreatedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.AdjustmentQuoteCreatedEvent,
                        }
                },
                {
                        typeof(QuoteAggregate.PolicyStateChangedEvent),
                        new List<ApplicationEventType>()
                },
                {
                    typeof(QuoteAggregate.CancellationQuoteCreatedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.CancellationQuoteCreatedEvent,
                        }
                },
                {
                    typeof(QuoteAggregate.RenewalQuoteCreatedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.RenewalQuoteCreatedEvent,
                        }
                },
                {
                    typeof(QuoteAggregate.PolicyRenewedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.PolicyRenewed,
                        }
                },
                {
                    typeof(QuoteAggregate.QuoteStateChangedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.QuoteStateChanged,
                        }
                },
                {
                    typeof(QuoteAggregate.QuoteBoundEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.QuoteBound,
                        }
                },
                {
                    typeof(QuoteAggregate.QuoteRollbackEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.QuoteRolledBack,
                        }
                },
                {
                    typeof(QuoteAggregate.CreditNoteIssuedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.CreditNoteIssued,
                        }
                },
                {
                    typeof(AdditionalPropertyValueInitializedEvent<QuoteAggregate, IQuoteEventObserver>),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.DefaultAdditionalPropertyValueAdded,
                        }
                },
                {
                    typeof(AdditionalPropertyValueUpdatedEvent<QuoteAggregate, IQuoteEventObserver>),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.UpdateAdditionalPropertyValue,
                        }
                },
                {
                    typeof(QuoteAggregate.QuoteTitleAssignedEvent),
                    new List<ApplicationEventType>()
                        {
                            ApplicationEventType.QuoteTitleAssignedEvent,
                        }
                },
                {
                        typeof(QuoteAggregate.QuoteTransferredToAnotherOrganisationEvent),
                        new List<ApplicationEventType>()
                        {
                            ApplicationEventType.QuoteOrganisationTransferred,
                        }
                },
                {
                        typeof(QuoteAggregate.MigrateQuotesAndPolicyTransactionsToNewProductReleaseEvent),
                        new List<ApplicationEventType>()
                },
                {
                    typeof(QuoteAggregate.MigrateUnassociatedEntitiesToProductReleaseEvent),
                    new List<ApplicationEventType>()
                },
            };

        /// <summary>
        /// Get the application event type for a given quote event.
        /// </summary>
        /// <param name="quoteEvent">The quote event.</param>
        /// <returns>The corresponding application events.</returns>
        public static List<ApplicationEventType> Map(IEvent<QuoteAggregate, Guid> quoteEvent)
        {
            if (TypeMap.TryGetValue(quoteEvent.GetType(), out List<ApplicationEventType> type))
            {
                return type;
            }

            throw new ArgumentException($"No entry was found in the map when trying to" +
                $" get the system event to be created for the aggregate event" +
                $" {quoteEvent.GetType()}. If no system event should be created" +
                $" for this aggregate event then you must still create an entry" +
                $" in the map which returns an empty list.");
        }
    }
}
