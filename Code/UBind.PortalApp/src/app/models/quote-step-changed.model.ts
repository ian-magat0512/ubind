/**
 * Model for notifications when a quote step change
 */
export interface QuoteStepChangedModel {
    quoteId: string;
    previousStep: string;
    destinationStep: string;
}
