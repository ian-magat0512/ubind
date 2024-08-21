/**
 * Model for notifications when a quote changes state
 */
export interface QuoteStateChangedModel {
    quoteId: string;
    previousQuoteState: string;
    newQuoteState: string;
}
