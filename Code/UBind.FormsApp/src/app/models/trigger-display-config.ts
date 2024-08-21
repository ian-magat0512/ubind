/**
 * Configuration for how a trigger should display itself within the forms app.
 */
export interface TriggerDisplayConfig {
    name: string;
    type: string;
    header: string;
    title: string;
    message: string;
    displayPrice: boolean;

    /**
     * A description of the trigger, for the reviewer or underwriter.
     */
    reviewerExplanation: string;
}
