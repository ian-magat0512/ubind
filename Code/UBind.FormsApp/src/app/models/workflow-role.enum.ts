export enum WorkflowRole {

    /**
     * None
     */
    None = 'none',

    /**
     * The requested start date of the policy, input by a user.
     */
    PolicyStartDateInput = 'policyStartDateInput',

    /**
     * The inception date of the policy.
     */
    PolicyInceptionDate = 'policyInceptionDate',

    /**
     * The expiry date of the policy.
     */
    PolicyExpiryDate = 'policyExpiryDate',

    /**
     * The adjustment date of the policy.
     */
    PolicyAdjustmentDate = 'policyAdjustmentDate',

    /**
     * The cancellation date of the policy.
     */
    PolicyCancellationDate = 'policyCancellationDate',

    /**
     * The full name of the customer.
     */
    CustomerName = 'customerName',

    /**
     * The email address of the customer.
     */
    CustomerEmail = 'customerEmail',

    /**
     * The landline phone number of the customer.
     */
    CustomerPhone = 'customerPhone',

    /**
     * The landline work phone number of the customer.
     */
    CustomerWorkPhone = 'customerWorkPhone',

    /**
     * The mobile phone number of the customer.
     */
    CustomerMobile = 'customerMobile',

    /**
     * The credit card number.
     */
    CreditCardNumber = 'creditCardNumber',

    /**
     * The name on the credit card.
     */
    CreditCardName = 'creditCardName',

    /**
     * The expiry date of the credit card.
     */
    CreditCardExpiry = 'creditCardExpiry',

    /**
     * The CCV or CVV number of the credit card.
     */
    CreditCardCcv = 'creditCardCCV',

    /**
     * The name of the bank account to be used for payment.
     */
    BankAccountName = 'bankAccountName',

    /**
     * The BSB number of the bank account to be used for payment.
     */
    BankAccountBsb = 'bankAccountBSB',

    /**
     * The bank account number.
     */
    BankAccountNumber = 'bankAccountNumber',

    /**
     * The payment method to be used.
     */
    PaymentMethod = 'paymentMethod',

    /**
     * The field to be set to true when premium finance is known to be accepted.
     */
    PremiumFinanceAcceptanceConfirmation = 'premiumFinanceAcceptanceConfirmation',

    /**
     * The ID of the saved quote to load.
     */
    LoadQuoteId = 'loadQuoteID',

    /**
     * The ID of the saved payment method to use on settlement.
     */
    SavedPaymentMethodId = 'savedPaymentMethodId'
}
