// <copyright file="ApplicationEventType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    /// <summary>
    /// Represents the type of an application event.
    /// WARNING: Do NOT insert new enum values as it will cause issues with existing application events.
    /// Please add new values at the end.
    /// </summary>
    public enum ApplicationEventType
    {
        /// <summary>
        /// Default value.
        /// </summary>
        None = 0,

        /// <summary>
        /// Raised when an application's ownership has been updated.
        /// </summary>
        OwnershipDetailsUpdated,

        /// <summary>
        /// Raised when an application's form data has been updated.
        /// </summary>
        FormUpdated,

        /// <summary>
        /// Raised when a quote has been assigned to a customer.
        /// </summary>
        AssignedToCustomer,

        /// <summary>
        /// Raised when a quote invitation has been generated and sent to the customer user.
        /// </summary>
        QuoteCustomerAssociationInvitationCreatedEvent,

        /// <summary>
        /// Raised when a quote has been assigned to an owner.
        /// </summary>
        AssignedToOwner,

        /// <summary>
        /// Raised when a calculation result has been created.
        /// </summary>
        Calculated,

        /// <summary>
        /// Raised when customer details have been updated.
        /// </summary>
        CustomerDetailsUpdated,

        /// <summary>
        /// Raised when the application has been submitted.
        /// </summary>
        Submitted,

        /// <summary>
        /// Raised when the customer has raised an enquiry / requested a callback.
        /// </summary>
        Enquired,

        /// <summary>
        /// Raised when the customer has requested the application to be saved.
        /// </summary>
        Saved,

        /// <summary>
        /// Raised when the a policy has been issued from import.
        /// </summary>
        PolicyImported,

        /// <summary>
        /// Raised when the a policy has been issued for an application.
        /// </summary>
        PolicyIssued,

        /// <summary>
        /// Raised when an invoice has been issued for an application.
        /// </summary>
        InvoiceIssued,

        /// <summary>
        /// Raised when a payment for an application has been successfully made.
        /// </summary>
        PaymentMade,

        /// <summary>
        /// Raised when a payment attempt for an application has failed.
        /// </summary>
        PaymentAttemptFailed,

        /// <summary>
        /// Raised when a funding proposal for an application has been successfully created.
        /// </summary>
        FundingProposalCreated,

        /// <summary>
        /// Raised when an attempt to create a funding proposal for an application has failed.
        /// </summary>
        FundingProposalCreationFailed,

        /// <summary>
        /// Raised when a funding proposal for an application has been successfully accepted.
        /// </summary>
        FundingProposalAccepted,

        /// <summary>
        /// Raised when an attempt to accept a funding proposal for an application has failed.
        /// </summary>
        FundingProposalAcceptanceFailed,

        /// <summary>
        /// Raised when a quote has been bound.
        /// </summary>
        QuoteBound,

        /// <summary>
        /// Raised when a quote has been rolled back to an earlier state.
        /// </summary>
        QuoteRolledBack,

        /// <summary>
        /// Raised when a quote has been discarded.
        /// </summary>
        QuoteDiscarded,

        /// <summary>
        /// Raised when an quote has been assigned a quote number.
        /// </summary>
        QuoteNumberAssigned,

        /// <summary>
        /// Raised when an quote has been issued for an application.
        /// </summary>
        QuoteVersionCreated,

        /// <summary>
        /// Raised when file attachment has succeeded.
        /// </summary>
        FileAttached,

        /// <summary>
        /// Raised when a policy is adjusted.
        /// </summary>
        PolicyAdjusted,

        /// <summary>
        /// Raised when a policy is renewed.
        /// </summary>
        PolicyRenewed,

        /// <summary>
        /// Raised when a policy is cancelled.
        /// </summary>
        PolicyCancelled,

        /// <summary>
        /// Raised when a policy form data is updated.
        /// </summary>
        PolicyFormDataUpdated,

        /// <summary>
        /// Raised when a policy has been deleted.
        /// </summary>
        PolicyDeleted,

        /// <summary>
        /// Raised when a quote is created.
        /// </summary>
        QuoteCreated,

        /// <summary>
        /// Raised when a document has been generated for the quote.
        /// </summary>
        QuoteDocumentGenerated,

        /// <summary>
        /// Raised when a document has been generated for the policy.
        /// </summary>
        PolicyDocumentGenerated,

        /// <summary>
        /// Raised when a document has been generated for the quote version.
        /// </summary>
        QuoteVersionDocumentGenerated,

        /// <summary>
        /// Raised when a quote is imported (from old application).
        /// </summary>
        QuoteImported,

        /// <summary>
        /// Raised when a quote is assigned with a parent quote ID.
        /// </summary>
        ParentQuoteAssigned,

        /// <summary>
        /// Raised when a quote workflow step changed.
        /// </summary>
        WorkflowStepChanged,

        /// <summary>
        /// Raised when email has been generated.
        /// </summary>
        QuoteEmailGeneratedEvent,

        /// <summary>
        /// Raised when email has been sent.
        /// </summary>
        QuoteEmailSentEvent,

        /// <summary>
        /// Raised when Quote renewal/adjustment.
        /// </summary>
        AdjustmentQuoteInitializedEvent,

        /// <summary>
        /// Raised when Quote adjustment Creation.
        /// </summary>
        AdjustmentQuoteCreatedEvent,

        /// <summary>
        /// Raised when Quote cancelled.
        /// </summary>
        CancellationQuoteCreatedEvent,

        /// <summary>
        /// Raised when Quote adjustment Creation.
        /// </summary>
        RenewalQuoteCreatedEvent,

        /// <summary>
        /// Raised when the quote's state has changed.
        /// </summary>
        QuoteStateChanged,

        /// <summary>
        /// Raised when email has been generated.
        /// </summary>
        PolicyEmailGeneratedEvent,

        /// <summary>
        /// Raised when an operation is being replayed manually
        /// </summary>
        ReplayEvent,

        /// <summary>
        /// Raised when a credit note has been issued.
        /// </summary>
        CreditNoteIssued,

        /// <summary>
        /// Raised when a quote has been assigned an expiry time.
        /// </summary>
        QuoteExpiryTimeSet,

        /// <summary>
        /// Raised when a quote has been assigned an expiry time in bulk (in product settings quote expiry enabled).
        /// </summary>
        QuoteExpiryTimeSetInBulk,

        /// <summary>
        /// Raised when a quote is actualised.
        /// </summary>
        QuoteActualised,

        /// <summary>
        /// Raised when a migrating new tenant/product ids for the quote.
        /// </summary>
        NewTenantAndProductIdsSet,

        /// <summary>
        /// Raised when a quote is migrated to an organisation.
        /// </summary>
        QuoteOrganisationMigratedEvent,

        /// <summary>
        /// Raised when the default additional property value is added.
        /// </summary>
        DefaultAdditionalPropertyValueAdded,

        /// <summary>
        /// Raised when the existing additional property value is updated.
        /// </summary>
        UpdateAdditionalPropertyValue,

        /// <summary>
        /// Raised when a title is assigned to a quote.
        /// </summary>
        QuoteTitleAssignedEvent,

        /// <summary>
        /// Raised when a quote has been unassigned to an owner.
        /// </summary>
        UnassignedToOwner,

        /// <summary>
        /// Raised when a quote is transferred to another organisation.
        /// </summary>
        QuoteOrganisationTransferred,

        /// <summary>
        /// Raised when a policy is missing its quote aggregate
        /// This event will fix the data anomaly
        /// </summary>
        AggregateCreationFromPolicy,

        /// <summary>
        /// Raised only when correcting data from aggregates,
        /// and setting up initial policy transaction records.
        /// </summary>
        SetPolicyTransaction,

        /// <summary>
        /// Raised when a New Business Quote is Created.
        /// </summary>
        NewBusinessQuoteCreated,

        /// <summary>
        /// Raised only when correcting the policy state changed.
        /// </summary>
        PolicyStateChangedEvent,

        /// <summary>
        /// Raised only when customer opened expired quote.
        /// </summary>
        CustomerOpenedExpiredQuote,

        /// <summary>
        /// Raised when a policy number is updated.
        /// </summary>
        PolicyNumberUpdated,
    }
}
