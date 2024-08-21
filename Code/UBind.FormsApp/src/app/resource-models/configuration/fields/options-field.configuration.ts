import { InteractiveFieldConfiguration } from "./interactive-field.configuration";

/**
 * Represents the configuration for fields which allows a user to select from options.
 */
export interface OptionsFieldConfiguration extends InteractiveFieldConfiguration {
    optionSetName?: string;
    optionSetKey?: string;
    optionsRequest?: OptionsRequest;
    selectedOptionRequest?: SelectedOptionRequest;
    searchTextExpression?: string;
    noOptionsFoundText?: string;
    hideAllOptionsConditionExpression?: string;
    optionHideConditionExpression?: string;

    /**
     * @deprecated please use searchTextExpression instead.
     */
    filterOptionsByExpression?: string;

    /**
     * @deprecated please use hideAllOptionsConditionExpression instead.
     */
    hideOptionsConditionExpression?: string;
}

/**
 * Represents configuration associated with an options field, which provides a way to configure loading 
 * options from an external data source.
 */
export interface OptionsRequest {
    autoTrigger?: boolean;
    triggerExpression: string;
    conditionExpression: string;
    urlExpression: string;
    httpVerb: string;
    payloadExpression: string;
    debounceTimeMilliseconds: number;
    allowCachingWithMaxAgeSeconds?: number;
}

/**
 * Configuration for a request to load a single option from the API on page load to present the data
 * for a selected option. You need this to get the label for the selected option, and any additional
 * properties.
 */
export interface SelectedOptionRequest {
    urlExpression: string;
    httpVerb: string;
    payloadExpression: string;
    allowCachingWithMaxAgeSeconds?: number;
}
