// <copyright file="RelationshipType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    /// <summary>
    /// Defines the different types of relationships that can be created between different entities within the system.
    /// </summary>
    public enum RelationshipType
    {
        /// <summary>
        /// The message sender relationship type.
        /// </summary>
        [RelationshipTypeInformation(
            "Message Sender",
            "messageSender",
            "Sender",
            "Senders",
            "sender",
            "senders",
            "was sent by",
            "Sent Message",
            "Sent Messages",
            "sentMessage",
            "sentMessages",
            "is sender of",
            EntityType.Message,
            new EntityType[] { EntityType.Customer, EntityType.User, EntityType.Tenant })]
        MessageSender = 0,

        /// <summary>
        /// The message recipient relationship type.
        /// </summary>
        [RelationshipTypeInformation(
            "Message Recipient",
            "messageRecipient",
            "Recipient",
            "Recipients",
            "recipient",
            "recipients",
            "was sent to",
            "Recieved Message",
            "Recieved Messages",
            "recievedMessage",
            "recievedMessages",
            "is recipient of",
            EntityType.Message,
            new EntityType[] { EntityType.Customer, EntityType.User, EntityType.Tenant })]
        MessageRecipient = 1,

        /// <summary>
        /// The customer message relationship type.
        /// </summary>
        [RelationshipTypeInformation(
            "Customer Message",
            "customerMessage",
            "Related Message",
            "Related Messages",
            "message",
            "messages",
            "relates to",
            "Related Customer",
            "Related Customers",
            "customer",
            "customers",
            "relates to",
            EntityType.Customer,
            new EntityType[] { EntityType.Message })]
        CustomerMessage = 2,

        /// <summary>
        /// The quote message relationship type.
        /// </summary>
        [RelationshipTypeInformation(
            "Quote Message",
            "quoteMessage",
            "Related Message",
            "Related Messages",
            "message",
            "messages",
            "relates to",
            "Related Quote",
            "Related Quotes",
            "quote",
            "quotes",
            "relates to",
            EntityType.Quote,
            new EntityType[] { EntityType.Message })]
        QuoteMessage = 3,

        /// <summary>
        /// The quote version message relationship type.
        /// </summary>
        [RelationshipTypeInformation(
            "Quote Version Message",
            "quoteVersionMessage",
            "Related Message",
            "Related Messages",
            "message",
            "messages",
            "relates to",
            "Related Quote Version",
            "Related Quote Versions",
            "quoteVersion",
            "quoteVersions",
            "relates to",
            EntityType.QuoteVersion,
            new EntityType[] { EntityType.Message })]
        QuoteVersionMessage = 4,

        /// <summary>
        /// The policy message relationship type.
        /// </summary>
        [RelationshipTypeInformation(
            "Policy Message",
            "policyMessage",
            "Related Message",
            "Related Messages",
            "message",
            "messages",
            "relates to",
            "Related Policy",
            "Related Policies",
            "policy",
            "policies",
            "relates to",
            EntityType.Policy,
            new EntityType[] { EntityType.Message })]
        PolicyMessage = 5,

        /// <summary>
        /// The polciy transaction message relationship type.
        /// </summary>
        [RelationshipTypeInformation(
            "Policy Transaction Message",
            "policyTransactionMessage",
            "Related Message",
            "Related Messages",
            "message",
            "messages",
            "relates to",
            "Related Policy Transaction",
            "Related Policy Transactions",
            "policyTransaction",
            "policyTransactions",
            "relates to",
            EntityType.PolicyTransaction,
            new EntityType[] { EntityType.Message })]
        PolicyTransactionMessage = 6,

        /// <summary>
        /// The claim message relationship type.
        /// </summary>
        [RelationshipTypeInformation(
            "Claim Message",
            "claimMessage",
            "Related Message",
            "Related Messages",
            "message",
            "messages",
            "relates to",
            "Related Claim",
            "Related Claims",
            "claim",
            "claims",
            "relates to",
            EntityType.Claim,
            new EntityType[] { EntityType.Message })]
        ClaimMessage = 7,

        /// <summary>
        /// The claim version message relationship type.
        /// </summary>
        [RelationshipTypeInformation(
            "Claim Version Message",
            "claimVersionMessage",
            "Related Message",
            "Related Messages",
            "message",
            "messages",
            "relates to",
            "Related Claim Version",
            "Related Claim Versions",
            "claimVersion",
            "claimVersions",
            "relates to",
            EntityType.ClaimVersion,
            new EntityType[] { EntityType.Message })]
        ClaimVersionMessage,

        /// <summary>
        /// The report message relationship type.
        /// </summary>
        [RelationshipTypeInformation(
            "Report Message",
            "reportMessage",
            "Related Message",
            "Related Messages",
            "message",
            "messages",
            "relates to",
            "Related Report",
            "Related Reports",
            "report",
            "reports",
            "relates to",
            EntityType.Report,
            new EntityType[] { EntityType.Message })]
        ReportMessage,

        /// <summary>
        /// The product message relationship type.
        /// </summary>
        [RelationshipTypeInformation(
            "Product Message",
            "productMessages",
            "Related Message",
            "Related Messages",
            "message",
            "messages",
            "relates to",
            "Related Product",
            "Related Products",
            "product",
            "products",
            "relates to",
            EntityType.Product,
            new EntityType[] { EntityType.Message })]
        ProductMessage,

        /// <summary>
        /// The quote event relationship type.
        /// </summary>
        [RelationshipTypeInformation(
            "Quote Event",
            "quoteEvent",
            "Related Event",
            "Related Events",
            "event",
            "events",
            "relates to",
            "Related Quote",
            "Related Quotes",
            "quote",
            "quotes",
            "relates to",
            EntityType.Quote,
            new EntityType[] { EntityType.Event })]
        QuoteEvent,

        /// <summary>
        /// The quote version event relationship type.
        /// </summary>
        [RelationshipTypeInformation(
            "Quote Version Event",
            "quoteVersionEvent",
            "Related Event",
            "Related Events",
            "event",
            "events",
            "relates to",
            "Related Quote Version",
            "Related Quote Versions",
            "quoteVersion",
            "quoteVersions",
            "relates to",
            EntityType.QuoteVersion,
            new EntityType[] { EntityType.Event })]
        QuoteVersionEvent,

        /// <summary>
        /// The policy event relationship type.
        /// </summary>
        [RelationshipTypeInformation(
            "Policy Event",
            "policyEvent",
            "Related Event",
            "Related Events",
            "event",
            "events",
            "relates to",
            "Related Policy",
            "Related Policies",
            "policy",
            "policies",
            "relates to",
            EntityType.Policy,
            new EntityType[] { EntityType.Event })]
        PolicyEvent,

        /// <summary>
        /// The policy transaction event relationship type.
        /// </summary>
        [RelationshipTypeInformation(
            "Policy Transaction Event",
            "policyTransactionEvent",
            "Related Event",
            "Related Events",
            "event",
            "events",
            "relates to",
            "Related Policy Transaction",
            "Related Policy Transactions",
            "policyTransaction",
            "policyTransactions",
            "relates to",
            EntityType.PolicyTransaction,
            new EntityType[] { EntityType.Event })]
        PolicyTransactionEvent,

        /// <summary>
        /// The claim event relationship type.
        /// </summary>
        [RelationshipTypeInformation(
            "Claim Event",
            "claimEvent",
            "Related Event",
            "Related Events",
            "event",
            "events",
            "relates to",
            "Related Claim",
            "Related Claims",
            "claim",
            "claims",
            "relates to",
            EntityType.Claim,
            new EntityType[] { EntityType.Event })]
        ClaimEvent,

        /// <summary>
        /// The claim version event relationship type.
        /// </summary>
        [RelationshipTypeInformation(
            "Claim Version Event",
            "claimVersionEvent",
            "Related Event",
            "Related Events",
            "event",
            "events",
            "relates to",
            "Related Claim Version",
            "Related Claim Versions",
            "claimVersion",
            "claimVersions",
            "relates to",
            EntityType.ClaimVersion,
            new EntityType[] { EntityType.Event })]
        ClaimVersionEvent,

        /// <summary>
        /// The customer event relationship type.
        /// </summary>
        [RelationshipTypeInformation(
            "Customer Event",
            "customerEvent",
            "Related Event",
            "Related Events",
            "event",
            "events",
            "relates to",
            "Related Customer",
            "Related Customers",
            "customer",
            "customers",
            "relates to",
            EntityType.Customer,
            new EntityType[] { EntityType.Event })]
        CustomerEvent,

        /// <summary>
        /// The user event relationship type.
        /// </summary>
        [RelationshipTypeInformation(
            "User Event",
            "userEvent",
            "Related Event",
            "Related Events",
            "event",
            "events",
            "relates to",
            "Related User",
            "Related Users",
            "user",
            "users",
            "relates to",
            EntityType.User,
            new EntityType[] { EntityType.Event })]
        UserEvent,

        /// <summary>
        /// The performing user event relationship type.
        /// </summary>
        [RelationshipTypeInformation(
            "Event Performing User",
            "eventPerformingUser",
            "Performing User",
            "Performing Users",
            "performingUser",
            "performingUsers",
            "was performed by",
            "Performed Event",
            "Performed Events",
            "performedEvent",
            "performedEvents",
            "performed",
            EntityType.Event,
            new EntityType[] { EntityType.User })]
        EventPerformingUser,

        /// <summary>
        /// The product event relationship type.
        /// </summary>
        [RelationshipTypeInformation(
            "Product Event",
            "productEvent",
            "Related Event",
            "Related Events",
            "event",
            "events",
            "relates to",
            "Related Product",
            "Related Products",
            "product",
            "products",
            "relates to",
            EntityType.Product,
            new EntityType[] { EntityType.Event })]
        ProductEvent,

        /// <summary>
        /// The te nant event relationship type.
        /// </summary>
        [RelationshipTypeInformation(
            "Tenant Event",
            "tenantEvent",
            "Related Event",
            "Related Events",
            "event",
            "events",
            "relates to",
            "Related Tenant",
            "Related Tenants",
            "tenant",
            "tenants",
            "relates to",
            EntityType.Tenant,
            new EntityType[] { EntityType.Event })]
        TenantEvent,

        /// <summary>
        /// The organisation event relationship type.
        /// </summary>
        [RelationshipTypeInformation(
            "Organisation Event",
            "organisationEvent",
            "Related Event",
            "Related Events",
            "event",
            "events",
            "relates to",
            "Related Organisation",
            "Related Organisations",
            "organisation",
            "organisations",
            "relates to",
            EntityType.Organisation,
            new EntityType[] { EntityType.Event })]
        OrganisationEvent,

        /// <summary>
        /// The email event relationship type.
        /// </summary>
        EmailEvent,

        /// <summary>
        /// The document event relationship type.
        /// </summary>
        [RelationshipTypeInformation(
            "Document Event",
            "documentEvent",
            "Related Event",
            "Related Events",
            "event",
            "events",
            "relates to",
            "Related Document",
            "Related Documents",
            "document",
            "documents",
            "relates to",
            EntityType.Document,
            new EntityType[] { EntityType.Event })]
        DocumentEvent,

        /// <summary>
        /// The credit note event relationship type.
        /// </summary>
        CreditNoteEvent,

        /// <summary>
        /// The invoice event relationship type.
        /// </summary>
        InvoiceEvent,

        /// <summary>
        /// The payment event relationship type.
        /// </summary>
        PaymentEvent,

        /// <summary>
        /// The refund event relationship type.
        /// </summary>
        RefundEvent,

        /// <summary>
        /// The organisation message relationship type.
        /// </summary>
        [RelationshipTypeInformation(
            "Organisation Message",
            "organisationMessage",
            "Related Message",
            "Related Messages",
            "message",
            "messages",
            "relates to",
            "Related Organisation",
            "Related Organisations",
            "organisation",
            "organisations",
            "relates to",
            EntityType.Product,
            new EntityType[] { EntityType.Message })]
        OrganisationMessage,

        /// <summary>
        /// The user message relationship type.
        /// </summary>
        [RelationshipTypeInformation(
            "User Message",
            "userMessage",
            "Related Message",
            "Related Messages",
            "message",
            "messages",
            "relates to",
            "Related User",
            "Related Users",
            "user",
            "users",
            "relates to",
            EntityType.User,
            new EntityType[] { EntityType.Message })]
        UserMessage,

        /// <summary>
        /// The portal event relationship type.
        /// </summary>
        [RelationshipTypeInformation(
            "Portal Event",
            "portalEvent",
            "Related Event",
            "Related Events",
            "event",
            "events",
            "relates to",
            "Related Portal",
            "Related Portals",
            "portal",
            "portals",
            "relates to",
            EntityType.Portal,
            new EntityType[] { EntityType.Event })]
        PortalEvent,

        /// <summary>
        /// The person event relationship type.
        /// </summary>
        PersonEvent,
    }
}
