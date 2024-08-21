
/**
 * Represents the configuration for an option of an option set.
 */
export interface OptionConfiguration {
    label: string;
    value: string;
    icon: string;
    cssClass?: string;
    hideConditionExpression?: string;
    disabledConditionExpression?: string;
    searchableText?: string;
    properties?: object;
}
