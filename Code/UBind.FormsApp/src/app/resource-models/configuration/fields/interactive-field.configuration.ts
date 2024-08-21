import { DataStoringFieldConfiguration } from "./data-storing-field.configuration";
import { VisibleFieldConfiguration } from "./visible-field.configuration";

/**
 * Represents a field which is interactive
 */
export interface InteractiveFieldConfiguration extends VisibleFieldConfiguration, DataStoringFieldConfiguration {
    required?: boolean;
    placeholder: string;
    disabledConditionExpression: string;
    readOnlyConditionExpression: string;
    autoTabExpression: string;
}
