import { InputMaskConfiguration } from "./input-mask-configuration";
import { LineInputFieldConfiguration } from "./line-input-field.configuration";
import { OptionsFieldConfiguration } from "./options-field.configuration";

/**
 * Represents the configuration for a field which allows someone to input a search select.
 */
export interface SearchSelectFieldConfiguration extends
    OptionsFieldConfiguration,
    LineInputFieldConfiguration,
    InputMaskConfiguration {
}
