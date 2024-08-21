/**
 * Information about questions which can be used for rendering, display or other reasons.
 */
export interface QuestionMetadata {
    dataType: string;
    displayable: boolean;
    canChangeWhenApproved: boolean;
    private: boolean;
    resetForNewQuotes: boolean;
    resetForNewRenewalQuotes: boolean;
    resetForNewAdjustmentQuotes: boolean;
    resetForNewCancellationQuotes: boolean;
    resetForNewPurchaseQuotes: boolean;
    tags: Array<string>;
    currencyCode?: string;
    summaryLabel?: string;
    summaryPositionExpression?: string;
    name: string;
    /**
     * @deprecated please just use summary label and embed expressions using %{ }% syntax.
     */
    summaryLabelExpression?: string;
}
