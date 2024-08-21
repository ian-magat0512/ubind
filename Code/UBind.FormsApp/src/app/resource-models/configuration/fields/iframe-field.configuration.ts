import { VisibleFieldConfiguration } from "./visible-field.configuration";

/**
 * Represents the configuration for a field which will render an Iframe.
 */
export interface IframeFieldConfiguration extends VisibleFieldConfiguration {
    urlExpression: string;
}
