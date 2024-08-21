import { DataStoringFieldConfiguration } from "./data-storing-field.configuration";
import { VisibleFieldConfiguration } from "./visible-field.configuration";

/**
 * Represents the configuration for a field which is not visible when rendered, but still maintains a value.
 */
export interface HiddenFieldConfiguration extends VisibleFieldConfiguration, DataStoringFieldConfiguration {
}
