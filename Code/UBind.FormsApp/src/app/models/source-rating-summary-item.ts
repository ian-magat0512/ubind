/**
 * Represents a rating summary item which has not had any processing or evaluation done on it.
 * These rating summary items will end up appearing in the sidebar in the calculation widget. They
 * allow the presentation of key inputs or calculated values back to the user.
 */
export class SourceRatingSummaryItem {

    public summaryLabel: string;
    public summaryPositionExpression: string;
    public defaultPosition: number;
    public value: string;

    public constructor(summaryLabel: string,
        value: string,
        defaultPosition: number = 0,
        summaryPositionExpression: string = null,
    ) {
        this.summaryLabel = summaryLabel;
        this.value = value;
        this.defaultPosition = defaultPosition;
        this.summaryPositionExpression = summaryPositionExpression;
    }
}
