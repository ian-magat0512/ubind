import { LineInputFieldConfiguration } from "./line-input-field.configuration";
import { OptionsFieldConfiguration } from "./options-field.configuration";

/**
 * Represents the configuration for a field which when clicked, drops down or reveals a fixed set of options.
 */
export interface DropDownSelectFieldConfiguration extends OptionsFieldConfiguration, LineInputFieldConfiguration {
}
