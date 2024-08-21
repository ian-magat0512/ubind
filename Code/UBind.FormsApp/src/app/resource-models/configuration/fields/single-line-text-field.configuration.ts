import { TextInputFormat } from "@app/models/text-input-format.enum";
import { InputMaskConfiguration } from "./input-mask-configuration";
import { InteractiveFieldConfiguration } from "./interactive-field.configuration";
import { LineInputFieldConfiguration } from "./line-input-field.configuration";

/**
 * Represents the configuration for a field which allows a user to input a single line of text.
 */
export interface SingleLineTextFieldConfiguration extends
    InteractiveFieldConfiguration,
    LineInputFieldConfiguration,
    InputMaskConfiguration {
    /**
     * Gets or sets a value indicating whether the information entered into the field
     * should be masked so as to stop shoulder surfing. This would typically be set to true
     * for things like credit card numbers.
     */
    sensitive?: boolean;

    /**
     * Gets or sets the text input format for this field.
     * This is determined from the field's data type if not passed.
     */
    textInputFormat: TextInputFormat;
}
