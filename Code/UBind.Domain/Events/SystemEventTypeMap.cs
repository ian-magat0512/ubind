// <copyright file="SystemEventTypeMap.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Events
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Aggregates.Portal;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.User;

    /// <summary>
    /// For mapping quote aggregate events to system event types.
    /// Note: Aggregate can now be mapped with multiple System Events.
    /// </summary>
    public static class SystemEventTypeMap
    {
        private static readonly Dictionary<Type, List<SystemEventType>> TypeMap = new Dictionary<Type, List<SystemEventType>>
        {
            { typeof(QuoteAggregate.ApplyNewIdEvent), new List<SystemEventType> { SystemEventType.QuoteApplyNewIdMigration, } },
            { typeof(QuoteAggregate.QuoteActualisedEvent), new List<SystemEventType> { SystemEventType.QuoteActualised, } },
            { typeof(QuoteAggregate.QuoteInitializedEvent), new List<SystemEventType> { SystemEventType.QuoteCreated, SystemEventType.NewBusinessQuoteCreated, } },
            { typeof(QuoteAggregate.FormDataUpdatedEvent), new List<SystemEventType> { SystemEventType.QuoteFormDataUpdated, } },
            { typeof(QuoteAggregate.FormDataPatchedEvent), new List<SystemEventType> { SystemEventType.QuoteFormDataUpdated, } },
            { typeof(QuoteAggregate.CustomerAssignedEvent), new List<SystemEventType> { SystemEventType.QuoteCustomerAssociated, } },
            { typeof(QuoteAggregate.OwnershipAssignedEvent), new List<SystemEventType> { SystemEventType.AssignedToOwner, } },
            { typeof(QuoteAggregate.OwnershipUnassignedEvent), new List<SystemEventType> { SystemEventType.UnassignedToOwner, } },
            { typeof(QuoteAggregate.CalculationResultCreatedEvent), new List<SystemEventType> { SystemEventType.QuoteCalculationResultCreated, } },
            { typeof(QuoteAggregate.CustomerDetailsUpdatedEvent), new List<SystemEventType> { SystemEventType.CustomerDetailsUpdated, } },
            { typeof(QuoteAggregate.QuoteSubmittedEvent), new List<SystemEventType> { SystemEventType.QuoteSubmitted, } },
            { typeof(QuoteAggregate.EnquiryMadeEvent), new List<SystemEventType> { SystemEventType.QuoteEnquiryMade, } },
            { typeof(QuoteAggregate.QuoteSavedEvent), new List<SystemEventType> { SystemEventType.QuoteSaved, } },
            { typeof(QuoteAggregate.InvoiceIssuedEvent), new List<SystemEventType> { SystemEventType.InvoiceIssued, } },
            { typeof(QuoteAggregate.PaymentMadeEvent), new List<SystemEventType> { SystemEventType.PaymentMade, } },
            { typeof(QuoteAggregate.PaymentFailedEvent), new List<SystemEventType> { SystemEventType.PaymentFailed, } },
            { typeof(QuoteAggregate.FundingProposalCreatedEvent), new List<SystemEventType> { SystemEventType.FundingProposalCreated, } },
            { typeof(QuoteAggregate.FundingProposalCreationFailedEvent), new List<SystemEventType> { SystemEventType.FundingProposalCreationFailed, } },
            { typeof(QuoteAggregate.FundingProposalAcceptedEvent), new List<SystemEventType> { SystemEventType.FundingProposalAccepted, } },
            { typeof(QuoteAggregate.FundingProposalAcceptanceFailedEvent), new List<SystemEventType> { SystemEventType.FundingProposalAcceptanceFailed, } },
            { typeof(QuoteAggregate.QuoteNumberAssignedEvent), new List<SystemEventType> { SystemEventType.QuoteReferenceAssigned, } },
            { typeof(QuoteAggregate.FileAttachedEvent), new List<SystemEventType> { SystemEventType.QuoteFileAttached, } },
            { typeof(QuoteAggregate.QuoteDocumentGeneratedEvent), new List<SystemEventType> { SystemEventType.DocumentAttachedToQuote, } },
            { typeof(QuoteAggregate.QuoteVersionDocumentGeneratedEvent), new List<SystemEventType> { SystemEventType.DocumentAttachedToQuoteVersion, } },
            { typeof(QuoteAggregate.PolicyDocumentGeneratedEvent), new List<SystemEventType> { SystemEventType.DocumentAttachedToPolicy, } },
            { typeof(QuoteAggregate.QuoteMigratedEvent), new List<SystemEventType> { SystemEventType.QuoteImported, } },
            { typeof(QuoteAggregate.QuoteImportedEvent), new List<SystemEventType> { SystemEventType.QuoteImported, } },
            { typeof(QuoteAggregate.PolicyImportedEvent), new List<SystemEventType> { SystemEventType.PolicyImported, } },
            { typeof(QuoteAggregate.PolicyIssuedWithoutQuoteEvent), new List<SystemEventType> { SystemEventType.PolicyIssued, } },
            { typeof(QuoteAggregate.PolicyDataPatchedEvent), new List<SystemEventType> { SystemEventType.PolicyFormDataUpdated, } },
            { typeof(QuoteAggregate.QuoteDiscardEvent), new List<SystemEventType> { SystemEventType.QuoteDiscarded, } },
            { typeof(QuoteAggregate.PolicyDeletedEvent), new List<SystemEventType> { SystemEventType.PolicyDeleted, } },
            { typeof(QuoteAggregate.WorkflowStepAssignedEvent), new List<SystemEventType> { SystemEventType.QuoteWorkflowStepChanged, } },
            { typeof(QuoteAggregate.AdjustmentQuoteCreatedEvent), new List<SystemEventType> { SystemEventType.AdjustmentQuoteCreated, } },
            { typeof(QuoteAggregate.RenewalQuoteCreatedEvent), new List<SystemEventType> { SystemEventType.RenewalQuoteCreated, } },
            { typeof(QuoteAggregate.QuoteBoundEvent), new List<SystemEventType> { SystemEventType.QuoteBound, } },
            { typeof(QuoteAggregate.QuoteRollbackEvent), new List<SystemEventType> { SystemEventType.QuoteRolledBack, } },
            { typeof(QuoteAggregate.CreditNoteIssuedEvent), new List<SystemEventType> { SystemEventType.CreditNoteIssued, } },
            { typeof(QuoteAggregate.CancellationQuoteCreatedEvent), new List<SystemEventType> { SystemEventType.CancellationQuoteCreated, } },
            { typeof(QuoteAggregate.QuoteCustomerAssociationInvitationCreatedEvent), new List<SystemEventType> { SystemEventType.QuoteCustomerAssociationInvitationCreated, } },
            { typeof(QuoteAggregate.QuoteExpiryTimestampSetEvent), new List<SystemEventType> { SystemEventType.QuoteExpiryTimestampSet, } },
            { typeof(QuoteAggregate.QuoteStateChangedEvent), new List<SystemEventType> { SystemEventType.QuoteStateChanged, } },
            { typeof(QuoteAggregate.QuoteVersionCreatedEvent), new List<SystemEventType> { SystemEventType.QuoteVersionCreated, } },
            { typeof(QuoteAggregate.PolicyIssuedEvent), new List<SystemEventType> { SystemEventType.PolicyIssued, } },
            { typeof(QuoteAggregate.PolicyAdjustedEvent), new List<SystemEventType> { SystemEventType.PolicyAdjusted, } },
            { typeof(QuoteAggregate.PolicyRenewedEvent), new List<SystemEventType> { SystemEventType.PolicyRenewed, } },
            { typeof(QuoteAggregate.PolicyCancelledEvent), new List<SystemEventType> { SystemEventType.PolicyCancelled, } },
            { typeof(QuoteAggregate.PolicyStateChangedEvent), new List<SystemEventType> { SystemEventType.PolicyStateChanged, } },
            { typeof(QuoteAggregate.PolicyNumberUpdatedEvent), new List<SystemEventType> { SystemEventType.PolicyNumberUpdated, SystemEventType.PolicyModified } },
            { typeof(ClaimAggregate.ApplyNewIdEvent), new List<SystemEventType> { SystemEventType.ClaimApplyNewIdMigration, } },
            { typeof(ClaimAggregate.ClaimStateChangedEvent), new List<SystemEventType> { SystemEventType.ClaimStateChanged, } },
            { typeof(ClaimAggregate.ClaimVersionCreatedEvent), new List<SystemEventType> { SystemEventType.ClaimVersionCreated, } },
            { typeof(ClaimAggregate.ClaimActualisedEvent), new List<SystemEventType> { SystemEventType.ClaimActualised, } },
            { typeof(ClaimAggregate.ClaimInitializedEvent), new List<SystemEventType> { SystemEventType.ClaimCreated, } },
            { typeof(ClaimAggregate.ClaimStatusUpdatedEvent), new List<SystemEventType> { SystemEventType.ClaimStatusUpdated, } },
            { typeof(ClaimAggregate.ClaimNumberUpdatedEvent), new List<SystemEventType> { SystemEventType.ClaimNumberUpdated, } },
            { typeof(ClaimAggregate.ClaimFormDataUpdatedEvent), new List<SystemEventType> { SystemEventType.ClaimFormDataUpdated, } },
            { typeof(ClaimAggregate.ClaimCalculationResultCreatedEvent), new List<SystemEventType> { SystemEventType.ClaimCalculationResultCreated, } },
            { typeof(ClaimAggregate.ClaimImportedEvent), new List<SystemEventType> { SystemEventType.ClaimImported, } },
            { typeof(ClaimAggregate.ClaimUpdateImportedEvent), new List<SystemEventType> { SystemEventType.ClaimUpdateImported, } },
            { typeof(ClaimAggregate.ClaimDescriptionUpdatedEvent), new List<SystemEventType> { SystemEventType.ClaimDescriptionUpdated, } },
            { typeof(ClaimAggregate.ClaimIncidentDateUpdatedEvent), new List<SystemEventType> { SystemEventType.ClaimIncidentDateUpdated, } },
            { typeof(ClaimAggregate.ClaimWorkflowStepAssignedEvent), new List<SystemEventType> { SystemEventType.ClaimWorkflowStepChanged, } },
            { typeof(ClaimAggregate.ClaimEnquiryMadeEvent), new List<SystemEventType> { SystemEventType.ClaimEnquiryMade, } },
            { typeof(ClaimAggregate.ClaimAmountUpdatedEvent), new List<SystemEventType> { SystemEventType.ClaimAmountUpdated, } },
            { typeof(ClaimAggregate.ClaimFileAttachedEvent), new List<SystemEventType> { SystemEventType.ClaimFileAttached, } },
            { typeof(ClaimAggregate.AssociateClaimWithPolicyEvent), new List<SystemEventType> { SystemEventType.ClaimPolicyAssociationCreated, } },
            { typeof(ClaimAggregate.DisassociateClaimWithPolicyEvent), new List<SystemEventType> { SystemEventType.ClaimDisassociatedWithPolicy, } },
            { typeof(ClaimAggregate.ClaimDeletedEvent), new List<SystemEventType> { SystemEventType.ClaimDeleted, } },
            { typeof(ClaimAggregate.ClaimVersionFileAttachedEvent), new List<SystemEventType> { SystemEventType.ClaimVersionFileAttached, } },
            { typeof(ClaimAggregate.ClaimOrganisationMigratedEvent), new List<SystemEventType> { SystemEventType.ClaimOrganisationMigratedEvent, } },
            { typeof(QuoteAggregate.QuoteTransferredToAnotherOrganisationEvent), new List<SystemEventType> { SystemEventType.QuoteOrganisationAssociationUpdated, } },
            { typeof(ClaimAggregate.ClaimTransferredToAnotherOrganisationEvent), new List<SystemEventType> { SystemEventType.ClaimOrganisationAssociationUpdated, } },
            { typeof(ClaimAggregate.OwnershipAssignedEvent), new List<SystemEventType> { SystemEventType.AssignedToOwner, } },
            { typeof(ClaimAggregate.OwnershipUnassignedEvent), new List<SystemEventType> { SystemEventType.UnassignedToOwner, } },
            { typeof(ClaimAggregate.CustomerAssignedEvent), new List<SystemEventType> { SystemEventType.QuoteCustomerAssociated, } },
            { typeof(PortalAggregate.PortalCreatedEvent), new List<SystemEventType> { SystemEventType.PortalCreated, } },
            { typeof(PortalAggregate.PortalUpdatedEvent), new List<SystemEventType> { SystemEventType.PortalUpdated, } },
            { typeof(PortalAggregate.UpdatePortalStylesEvent), new List<SystemEventType> { SystemEventType.PortalUpdated, } },
            { typeof(PortalAggregate.SetPortalLocationEvent), new List<SystemEventType> { SystemEventType.PortalUpdated, } },
            { typeof(PortalAggregate.DisablePortalEvent), new List<SystemEventType> { SystemEventType.PortalDisabled, } },
            { typeof(PortalAggregate.EnablePortalEvent), new List<SystemEventType> { SystemEventType.PortalEnabled, } },
            { typeof(PortalAggregate.DeletePortalEvent), new List<SystemEventType> { SystemEventType.PortalDeleted, } },
            { typeof(Organisation.OrganisationInitializedEvent), new List<SystemEventType> {SystemEventType.OrganisationCreated, } },
            { typeof(Organisation.OrganisationNameUpdatedEvent), new List<SystemEventType> {SystemEventType.OrganisationUpdated, SystemEventType.OrganisationModified } },
            { typeof(Organisation.OrganisationAliasUpdatedEvent), new List<SystemEventType> {SystemEventType.OrganisationUpdated, SystemEventType.OrganisationModified } },
            { typeof(Organisation.ManagingOrganisationUpdatedEvent), new List<SystemEventType> {SystemEventType.OrganisationUpdated } },
            { typeof(Organisation.OrganisationTenantUpdatedEvent), new List<SystemEventType> {SystemEventType.OrganisationUpdated, SystemEventType.OrganisationModified } },
            { typeof(Organisation.SetDefaultOrganisationEvent), new List<SystemEventType> {SystemEventType.OrganisationUpdated, } },
            { typeof(Organisation.SetOrganisationDefaultPortalEvent), new List<SystemEventType> {SystemEventType.OrganisationUpdated } },
            { typeof(Organisation.OrganisationDisabledEvent), new List<SystemEventType> {SystemEventType.OrganisationDisabled, SystemEventType.OrganisationModified } },
            { typeof(Organisation.OrganisationActivatedEvent), new List<SystemEventType> {SystemEventType.OrganisationEnabled, SystemEventType.OrganisationModified } },
            { typeof(Organisation.OrganisationDeletedEvent), new List<SystemEventType> {SystemEventType.OrganisationDeleted, SystemEventType.OrganisationModified } },
            { typeof(Organisation.OrganisationAuthenticationMethodAddedEvent), new List<SystemEventType> { SystemEventType.OrganisationModified } },
            { typeof(Organisation.OrganisationAuthenticationMethodUpdatedEvent), new List<SystemEventType> { SystemEventType.OrganisationModified } },
            { typeof(Organisation.OrganisationAuthenticationMethodDeletedEvent), new List<SystemEventType> { SystemEventType.OrganisationModified } },
            { typeof(Organisation.OrganisationAuthenticationMethodDisabledEvent), new List<SystemEventType> { SystemEventType.OrganisationModified } },
            { typeof(Organisation.OrganisationAuthenticationMethodEnabledEvent), new List<SystemEventType> { SystemEventType.OrganisationModified } },
            { typeof(Organisation.OrganisationIdentityLinkedEvent), new List<SystemEventType> { SystemEventType.OrganisationModified } },
            { typeof(Organisation.OrganisationIdentityUnlinkedEvent), new List<SystemEventType> { SystemEventType.OrganisationModified } },
            { typeof(Organisation.OrganisationLinkedIdentityUpdatedEvent), new List<SystemEventType> { SystemEventType.OrganisationModified } },
            { typeof(UserAggregate.ActivationInvitationCreatedEvent), new List<SystemEventType> { SystemEventType.UserAccountActivationInvitationCreated, SystemEventType.UserModified } },
            { typeof(UserAggregate.UserActivatedEvent), new List<SystemEventType> { SystemEventType.UserAccountActivated, SystemEventType.UserModified } },
            { typeof(UserAggregate.PasswordChangedEvent), new List<SystemEventType> { SystemEventType.UserPasswordUpdated } },
            { typeof(UserAggregate.PasswordResetInvitationCreatedEvent), new List<SystemEventType> { SystemEventType.UserPasswordResetRequestMade } },
            { typeof(UserAggregate.UserInitializedEvent), new List<SystemEventType> { SystemEventType.UserCreated } },
            { typeof(UserAggregate.UserBlockedEvent), new List<SystemEventType> { SystemEventType.UserDisabled, SystemEventType.UserModified } },
            { typeof(UserAggregate.UserUnblockedEvent), new List<SystemEventType> { SystemEventType.UserEnabled, SystemEventType.UserModified } },
            { typeof(UserAggregate.UserTransferredToAnotherOrganisationEvent), new List<SystemEventType> { SystemEventType.UserOrganisationAssociationUpdated } },
            { typeof(UserAggregate.UserImportedEvent), new List<SystemEventType> { SystemEventType.UserModified } },
            { typeof(UserAggregate.LoginEmailSetEvent), new List<SystemEventType> { SystemEventType.UserModified } },
            { typeof(UserAggregate.RoleAddedEvent), new List<SystemEventType> { SystemEventType.UserModified, SystemEventType.UserRoleAdded} },
            { typeof(UserAggregate.UserTypeUpdatedEvent), new List<SystemEventType> { SystemEventType.UserModified } },
            { typeof(UserAggregate.RoleAssignedEvent), new List<SystemEventType> { SystemEventType.UserModified, SystemEventType.UserRoleAssigned } },
            { typeof(UserAggregate.RoleRetractedEvent), new List<SystemEventType> { SystemEventType.UserModified, SystemEventType.UserRoleRetracted } },
            { typeof(UserAggregate.UserModifiedTimeUpdatedEvent), new List<SystemEventType> { SystemEventType.UserModified } },
            { typeof(UserAggregate.ProfilePictureAssignedToUserEvent), new List<SystemEventType> { SystemEventType.UserEdited, SystemEventType.UserModified } },
            { typeof(CustomerAggregate.CustomerInitializedEvent), new List<SystemEventType> { SystemEventType.CustomerCreated } },
            { typeof(CustomerAggregate.CustomerImportedEvent), new List<SystemEventType> { SystemEventType.CustomerModified } },
            { typeof(CustomerAggregate.CustomerOrganisationMigratedEvent), new List<SystemEventType> { SystemEventType.CustomerModified } },
            { typeof(CustomerAggregate.CustomerModifiedTimeUpdatedEvent), new List<SystemEventType> { SystemEventType.CustomerModified } },
            { typeof(CustomerAggregate.OwnershipAssignedEvent), new List<SystemEventType> { SystemEventType.CustomerAgentAssigned, SystemEventType.CustomerModified } },
            { typeof(CustomerAggregate.OwnershipUnassignedEvent), new List<SystemEventType> { SystemEventType.CustomerAgentUnassigned, SystemEventType.CustomerModified } },
            { typeof(CustomerAggregate.CustomerTransferredToAnotherOrganisationEvent), new List<SystemEventType> { SystemEventType.CustomerOrganisationAssociationUpdated } },
            { typeof(CustomerAggregate.PortalChangedEvent), new List<SystemEventType> { SystemEventType.CustomerModified } },
            { typeof(CustomerAggregate.CustomerSetPrimaryPersonEvent), new List<SystemEventType> { SystemEventType.CustomerModified } },
            { typeof(CustomerAggregate.CustomerDeletedEvent), new List<SystemEventType> { SystemEventType.CustomerDeleted, SystemEventType.CustomerModified } },
            { typeof(CustomerAggregate.CustomerUndeletedEvent), new List<SystemEventType> {SystemEventType.CustomerUndeleted, SystemEventType.CustomerModified } },
        };

        /// <summary>
        /// Get the system event type for a given quote event.
        /// </summary>
        /// <typeparam name="T">The type of the event.</typeparam>
        /// <param name="aggregateEvent">The aggregate event.</param>
        /// <returns>The corresponding application event.</returns>
        public static List<SystemEventType> Map<T>(T aggregateEvent)
            where T : class
        {
            if (TypeMap.TryGetValue(aggregateEvent.GetType(), out List<SystemEventType> type))
            {
                return type;
            }

            throw new ArgumentException($"Cannot map unknown system event type: {aggregateEvent.GetType()}");
        }
    }
}
