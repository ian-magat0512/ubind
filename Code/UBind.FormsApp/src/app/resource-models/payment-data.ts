/**
 * Data which is used to calculate fees and surcharges for the given payment type.
 */
export interface PaymentData {
    /**
     * Gets or sets the Card BIN, which is the Bank Identification Number, which is the first
     * four to six numbers of the credit card.
     */
    cardBin?: string;

    /**
     * Gets or sets the payment card number lenght.
     */
    cardNumberLength?: number;

    /**
     * Gets or sets the payment method, one of "CARD" or "DIRECT_DEBIT".
     */
    paymentMethod?: string;

    /**
     * gets or sets a value indicating whether this is a once of or recurring payment.
     */
    singlePayment?: boolean;
}
