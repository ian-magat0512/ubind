// <copyright file="WorkflowRole.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component.Form
{
    using System.ComponentModel;

    /// <summary>
    /// Workflow roles are set on a field to denote a special purpose that fields
    /// value will be used for during the workflow or lifeycle of the form.
    /// This usually means that the field is used to fulfill a data requirement
    /// for an API call, e.g. to set the policy inception data on an calculation
    /// request, or a customer's name for an API call that creates a customer
    /// object.
    /// </summary>
    public enum WorkflowRole
    {
        /// <summary>
        /// The field does not have a workflow role.
        /// </summary>
        [Description("none")]
        None,

        /// <summary>
        /// The requested start date of the policy, input by a user.
        /// </summary>
        [Description("policyStartDateInput")]
        PolicyStartDateInput,

        /// <summary>
        /// The inception date of the policy.
        /// </summary>
        [Description("policyInceptionDate")]
        PolicyInceptionDate,

        /// <summary>
        /// The expiry date of the policy.
        /// </summary>
        [Description("policyExpiryDate")]
        PolicyExpiryDate,

        /// <summary>
        /// The adjustment date of the policy.
        /// </summary>
        [Description("policyAdjustmentDate")]
        PolicyAdjustmentDate,

        /// <summary>
        /// The cancellation date of the policy.
        /// </summary>
        [Description("policyCancellationDate")]
        PolicyCancellationDate,

        /// <summary>
        /// The full name of the customer.
        /// </summary>
        [Description("customerName")]
        CustomerName,

        /// <summary>
        /// The email address of the customer.
        /// </summary>
        [Description("customerEmail")]
        CustomerEmail,

        /// <summary>
        /// The landline phone number of the customer.
        /// </summary>
        [Description("customerPhone")]
        CustomerPhone,

        /// <summary>
        /// The landline phone number of the customer.
        /// </summary>
        [Description("customerWorkPhone")]
        CustomerWorkPhone,

        /// <summary>
        /// The mobile phone number of the customer.
        /// </summary>
        [Description("customerMobile")]
        CustomerMobile,

        /// <summary>
        /// The credit card number.
        /// </summary>
        [Description("creditCardNumber")]
        CreditCardNumber,

        /// <summary>
        /// The name on the credit card.
        /// </summary>
        [Description("creditCardName")]
        CreditCardName,

        /// <summary>
        /// The expiry date of the credit card.
        /// </summary>
        [Description("creditCardExpiry")]
        CreditCardExpiry,

        /// <summary>
        /// The CCV or CVV number of the credit card.
        /// </summary>
        [Description("creditCardCCV")]
        CreditCardCcv,

        /// <summary>
        /// The name of the bank account to be used for payment.
        /// </summary>
        [Description("bankAccountName")]
        BankAccountName,

        /// <summary>
        /// The BSB number of the bank account to be used for payment.
        /// </summary>
        [Description("bankAccountBSB")]
        BankAccountBsb,

        /// <summary>
        /// The bank account number.
        /// </summary>
        [Description("bankAccountNumber")]
        BankAccountNumber,

        /// <summary>
        /// The payment method to be used.
        /// </summary>
        [Description("paymentMethod")]
        PaymentMethod,

        /// <summary>
        /// The field to be set to true when premium finance is known to be accepted.
        /// </summary>
        [Description("premiumFinanceAcceptanceConfirmation")]
        PremiumFinanceAcceptanceConfirmation,

        /// <summary>
        /// The ID of the saved quote to load.
        /// </summary>
        [Description("loadQuoteID")]
        LoadQuoteId,

        /// <summary>
        /// The password which was saved against the quote, to be used when loading the quote.
        /// </summary>
        [Description("loadPassword")]
        LoadPassword,

        /// <summary>
        /// The password to use when saving the quote.
        /// </summary>
        [Description("savePassword")]
        SavePassword,

        /// <summary>
        /// The Id of the saved payment method to be used for settlement.
        /// </summary>
        [Description("savedPaymentMethodId")]
        SavedPaymentMethodId,
    }
}
