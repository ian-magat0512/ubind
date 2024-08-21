// <copyright file="AdditionalPropertyEntityType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Enums
{
    using System.ComponentModel;
    using UBind.Domain.Attributes;

    /// <summary>
    /// This custom type is used to determine which the additional properties are being directly mapped to. This
    /// serves as the subset of the <see cref="AdditionalPropertyDefinitionContextType"/>. Using them both will further
    /// determine the scope of the additional properties.
    /// </summary>
    public enum AdditionalPropertyEntityType
    {
        [Description("None")]
        None = 0,

        /// <summary>
        /// Customer.
        /// </summary>
        [Description("Customer")]
        Customer = 1,

        /// <summary>
        /// Quote.
        /// </summary>
        [AdditionalPropertyEntityTypeCategory(AdditionalPropertyEntityTypeCategory.Quote)]
        [Description("Quote")]
        Quote = 2,

        /// <summary>
        /// New Business Quote.
        /// </summary>
        [AdditionalPropertyEntityTypeCategory(AdditionalPropertyEntityTypeCategory.Quote)]
        [Description("New Business Quote")]
        NewBusinessQuote = 3,

        /// <summary>
        /// Adjustment Quote.
        /// </summary>
        [AdditionalPropertyEntityTypeCategory(AdditionalPropertyEntityTypeCategory.Quote)]
        [Description("Adjustment Quote")]
        AdjustmentQuote = 4,

        /// <summary>
        /// Renewal Quote.
        /// </summary>
        [AdditionalPropertyEntityTypeCategory(AdditionalPropertyEntityTypeCategory.Quote)]
        [Description("Renewal Quote")]
        RenewalQuote = 5,

        /// <summary>
        /// Cancellation Quote.
        /// </summary>
        [AdditionalPropertyEntityTypeCategory(AdditionalPropertyEntityTypeCategory.Quote)]
        [Description("Cancellation Quote")]
        CancellationQuote = 6,

        /// <summary>
        /// Quote Version.
        /// </summary>
        [AdditionalPropertyEntityTypeCategory(AdditionalPropertyEntityTypeCategory.QuoteVersion)]
        [Description("Quote Version")]
        QuoteVersion = 7,

        /// <summary>
        /// Policy.
        /// </summary>
        [AdditionalPropertyEntityTypeCategory(AdditionalPropertyEntityTypeCategory.Policy)]
        [Description("Policy")]
        Policy = 8,

        /// <summary>
        /// Policy Transaction.
        /// </summary>
        [AdditionalPropertyEntityTypeCategory(AdditionalPropertyEntityTypeCategory.PolicyTransaction)]
        [Description("Policy Transaction")]
        PolicyTransaction = 9,

        /// <summary>
        /// New Business Transaction.
        /// </summary>
        [AdditionalPropertyEntityTypeCategory(AdditionalPropertyEntityTypeCategory.PolicyTransaction)]
        [Description("New Business Policy Transaction")]
        NewBusinessPolicyTransaction = 10,

        /// <summary>
        /// Adjustment Transaction.
        /// </summary>
        [AdditionalPropertyEntityTypeCategory(AdditionalPropertyEntityTypeCategory.PolicyTransaction)]
        [Description("Adjustment Policy Transaction")]
        AdjustmentPolicyTransaction = 11,

        /// <summary>
        /// Renewal Transaction.
        /// </summary>
        [AdditionalPropertyEntityTypeCategory(AdditionalPropertyEntityTypeCategory.PolicyTransaction)]
        [Description("Renewal Policy Transaction")]
        RenewalPolicyTransaction = 12,

        /// <summary>
        /// Cancellation Transaction.
        /// </summary>
        [AdditionalPropertyEntityTypeCategory(AdditionalPropertyEntityTypeCategory.PolicyTransaction)]
        [Description("Cancellation Policy Transaction")]
        CancellationPolicyTransaction = 13,

        /// <summary>
        /// Claim.
        /// </summary>
        [AdditionalPropertyEntityTypeCategory(AdditionalPropertyEntityTypeCategory.Claim)]
        [Description("Claim")]
        Claim = 14,

        /// <summary>
        /// Claim Version.
        /// </summary>
        [AdditionalPropertyEntityTypeCategory(AdditionalPropertyEntityTypeCategory.ClaimVersion)]
        [Description("Claim Version")]
        ClaimVersion = 15,

        /// <summary>
        /// Tenant.
        /// </summary>
        [Description("Tenant")]
        Tenant = 16,

        /// <summary>
        /// Organisation.
        /// </summary>
        [Description("Organisation")]
        Organisation = 17,

        /// <summary>
        /// Invoice.
        /// </summary>
        [Description("Invoice")]
        Invoice = 18,

        /// <summary>
        /// Credit Note.
        /// </summary>
        [Description("Credit Note")]
        CreditNote = 19,

        /// <summary>
        /// Payment.
        /// </summary>
        [Description("Payment")]
        Payment = 20,

        /// <summary>
        /// Refund.
        /// </summary>
        [Description("Refund")]
        Refund = 21,

        /// <summary>
        /// Product.
        /// </summary>
        [Description("Product")]
        Product = 22,

        /// <summary>
        /// Portal.
        /// </summary>
        [Description("Portal")]
        Portal = 23,

        /// <summary>
        /// User.
        /// </summary>
        [Description("User")]
        User = 24,
    }
}
