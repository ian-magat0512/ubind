/**
 * Represets the configuration of a trigger.
 */
export interface TriggerConfiguration {
    /**
     * Gets or sets the Name of the trigger.
     */
    name: string;

    /**
     * Gets or sets the key of the trigger.
     */
    key: string;

    /**
     * Gets or sets the type of the trigger. Standard trigger types include:
     * - Review
     * - Endorsement
     * - Decline
     * - Error.
     */
    type: string;

    /**
     * Gets or sets a summary for the trigger to be displayed to the customer if this trigger is activated.
     */
    customerSummary: string;

    /**
     * Gets or sets a summary for the trigger to be displayed to the agent who is filling out the form on behalf
     * of the customer. If not set, the CustomerSummary is used.
     */
    agentSummary: string;

    /**
     * Gets or sets the detailed message to be displayed to the customer when this trigger is activated.
     */
    customerMessage: string;

    /**
     * Gets or sets the detailed message to be displayed to the agent when this trigger is activated,
     * when the agent is filling out the form on behalf of the customer. If this is not set, the
     * CustomerMessage is used.
     */
    agentMessage: string;

    /**
     * Gets or sets a title to be displayed above the message to the customer when this trigger is activated.
     * If the price is set to be displayed, this title will precede the price, so it will be become the label
     * for the price.
     * If not set, the title or price label will not change when this trigger is actived.
     */
    customerTitle: string;

    /**
     * Gets or sets a title to be displayed above the message to the agent when this trigger is activated and
     * the agent is filling out the form on behalf of the customer.
     * If the price is set to be displayed, this title will precede the price, so it will be become the label
     * for the price.
     * If not set, the CustomerTitle will be used.
     */
    agentTitle: string;

    /**
     * Gets or sets a value indicating whether the price should be displayed when this trigger is activated.
     */
    displayPrice: boolean;

    /**
     * Gets or sets an explanation of the cause of the trigger to be shown to a person reviewing the trigger.
     */
    reviewerExplanation: string;
}
