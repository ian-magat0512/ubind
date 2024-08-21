// <copyright file="SystemEventType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Events
{
    using System;
    using NodaTime;

    /// <summary>
    /// The System Event Type selection, implementation of numbering schema below.
    /// </summary>
    /// <remarks>
    /// --- Guidelines ---
    /// 1st number tells what type of event.
    /// 2nd to 4rd number tells what subtype of that event that is.
    /// last 4 numbers determine what specific event it is.
    /// Here are some guidelines on how the numberings came to be:
    /// 0 = default.
    /// 1xxxxxxx = aggregate events
    /// 1001xxxx = quote aggregate events
    /// 1002xxxx = policy aggregate events
    /// 1003xxxx = claim aggregate events
    /// 1004xxxx = user aggregate events
    /// 1005xxxx = customer aggregate events
    /// 1006xxxx = portal aggregate events
    /// 1007xxxx = organisation aggregate events
    /// 1008xxxx = tenant events
    /// ... ( new kinds of aggregate events needs to be documented on this comment as well )
    /// 1999xxxx = other aggregate events
    /// 2xxxxxxx = other platform events
    /// 2001xxxx = quote related platform events
    /// 2002xxxx = policy related platform events
    /// 2003xxxx = claim related platform events
    /// 2004xxxx = user related platform events
    /// 2005xxxx = customer related platform events
    /// 2006xxxx = portal related platform events
    /// 2007xxxx = organisation related platform events
    /// 2008xxxx = tenant related platform events
    /// ... ( new kinds of platform events needs to be documented on this comment as well )
    /// 2999xxxx = other platform events.
    /// </remarks>
    public enum SystemEventType
    {
        Custom = 0,

        QuoteStateChanged = 10010001,

        QuoteVersionCreated = 10010002,

        [PersistFor(PeriodUnits.Hours, 1)]
        QuoteCreated = 10010003,

        [PersistFor(PeriodUnits.Hours, 1)]
        QuoteFormDataUpdated = 10010004,

        QuoteCustomerAssociated = 10010005,

        AssignedToOwner = 10010006,

        [PersistFor(PeriodUnits.Hours, 1)]
        QuoteCalculationResultCreated = 10010007,

        CustomerDetailsUpdated = 10010008,

        QuoteSubmitted = 10010009,

        Enquired = 10010010,

        Saved = 10010011,

        InvoiceIssued = 10010012,

        PaymentMade = 10010013,

        PaymentFailed = 10010014,

        [PersistFor(PeriodUnits.Hours, 1)]
        FundingProposalCreated = 10010015,

        [PersistFor(PeriodUnits.Hours, 1)]
        FundingProposalCreationFailed = 10010016,

        FundingProposalAccepted = 10010017,

        FundingProposalAcceptanceFailed = 10010018,

        QuoteReferenceAssigned = 10010019,

        [PersistFor(PeriodUnits.Hours, 1)]
        QuoteFileAttached = 10010020,

        DocumentAttachedToQuote = 10010021,

        DocumentAttachedToQuoteVersion = 10010022,

        QuoteImported = 10010023,

        QuoteDiscarded = 10010024,

        [PersistFor(PeriodUnits.Hours, 1)]
        QuoteWorkflowStepChanged = 10010025,

        [Obsolete]
        QuoteEmailGenerated = 10010026,

        [Obsolete]
        QuoteEmailSent = 10010027,

        AdjustmentQuoteCreated = 10010028,

        RenewalQuoteCreated = 10010029,

        QuoteBound = 10010030,

        QuoteRolledBack = 10010031,

        CreditNoteIssued = 10010032,

        CancellationQuoteCreated = 10010033,

        QuoteCustomerAssociationInvitationCreated = 10010034,

        QuoteExpiryTimestampSet = 10010035,

        QuoteExpiryTimeSetInBulk = 10010036,

        QuoteActualised = 10010037,

        QuoteApplyNewIdMigration = 10010038,

        QuoteOrganisationMigrated = 10010039,

        UnassignedToOwner = 10010040,

        QuoteOrganisationAssociationUpdated = 10010041,

        QuoteEnquiryMade = 10010042,

        QuoteSaved = 10010043,

        NewBusinessQuoteCreated = 10010044,

        PolicyIssued = 10020001,

        PolicyAdjusted = 10020002,

        PolicyRenewed = 10020003,

        PolicyCancelled = 10020004,

        PolicyImported = 10020005,

        PolicyFormDataUpdated = 10020006,

        [Obsolete]
        PolicyEmailGenerated = 10020007,

        DocumentAttachedToPolicy = 10020008,

        PolicyStateChanged = 10020009,

        PolicyNumberUpdated = 10020010,

        PolicyModified = 10020011,

        PolicyDeleted = 10020012,

        ClaimStateChanged = 10030001,

        ClaimVersionCreated = 10030002,

        ClaimCreated = 10030003,

        ClaimAmountUpdated = 10030004,

        ClaimStatusUpdated = 10030005,

        ClaimNumberUpdated = 10030006,

        [PersistFor(PeriodUnits.Hours, 1)]
        ClaimFormDataUpdated = 10030007,

        ClaimCalculationResultCreated = 10030008,

        ClaimImported = 10030009,

        ClaimUpdateImported = 10030010,

        ClaimDescriptionUpdated = 10030011,

        ClaimIncidentDateUpdated = 10030012,

        [PersistFor(PeriodUnits.Hours, 1)]
        ClaimWorkflowStepChanged = 10030013,

        ClaimEnquiryMade = 10030014,

        ClaimFileAttached = 10030015,

        ClaimPolicyAssociationCreated = 10030016,

        ClaimActualised = 100300017,

        ClaimDisassociatedWithPolicy = 100300018,

        ClaimDeleted = 100300019,

        [Obsolete]
        QuoteFormDataPatched = 100100018,

        ClaimVersionFileAttached = 10030020,

        ClaimApplyNewIdMigration = 10030021,

        ClaimOrganisationMigratedEvent = 10030022,

        ClaimOrganisationAssociationUpdated = 10030023,

        UserAccountActivated = 10040001,

        UserAccountActivationInvitationCreated = 10040002,

        UserCreated = 10040003,

        UserDeleted = 10040004,

        UserDisabled = 10040005,

        UserEnabled = 10040006,

        UserOrganisationAssociationUpdated = 10040007,

        UserPasswordResetRequestMade = 10040008,

        UserPasswordUpdated = 10040009,

        UserRoleAdded = 10040010,

        UserRoleAssigned = 10040011,

        UserRoleRetracted = 10040012,

        CustomerAgentAssigned = 10050001,

        CustomerAgentUnassigned = 10050002,

        CustomerCreated = 10050003,

        CustomerDeleted = 10050004,

        CustomerOrganisationAssociationUpdated = 10050005,

        CustomerUndeleted = 10050006,

        PortalCreated = 10060001,

        PortalUpdated = 10060002,

        PortalDisabled = 10060003,

        PortalEnabled = 10060004,

        PortalDeleted = 10060005,

        OrganisationCreated = 10070001,

        OrganisationUpdated = 10070002,

        OrganisationDisabled = 10070003,

        OrganisationEnabled = 10070004,

        OrganisationDeleted = 10070005,

        TenantDisabled = 10080001,

        CustomerExpiredQuoteOpened = 20010001,

        UserEdited = 20040001,

        [PersistFor(PeriodUnits.Years, 1)]
        UserEmailAddressBlocked = 20040002,

        [PersistFor(PeriodUnits.Years, 1)]
        UserLoggedOut = 20040003,

        [PersistFor(PeriodUnits.Years, 1)]
        UserLoginAttemptFailed = 20040004,

        [PersistFor(PeriodUnits.Years, 1)]
        UserLoginAttemptSucceeded = 20040005,

        UserModified = 20040006,

        [PersistFor(PeriodUnits.Hours, 1)]
        UserSessionInvalidated = 20040007,

        CustomerEdited = 20050001,

        [PersistFor(PeriodUnits.Hours, 1)]
        CustomerModified = 20050002,

        OrganisationModified = 20070001,

        TenantModified = 20080001,
    }
}
