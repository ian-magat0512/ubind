import { TextInputFormat } from "@app/models/text-input-format.enum";
import { InteractiveFieldConfiguration } from "./interactive-field.configuration";
import { LineInputFieldConfiguration } from "./line-input-field.configuration";

/**
 * Represents the configuration for a field which takes a password.
 */
export interface PasswordFieldConfiguration extends InteractiveFieldConfiguration, LineInputFieldConfiguration {
    /**
     * Gets or sets the text input format for this field.
     * This is determined from the field's data type if not passed.
     */
    textInputFormat: TextInputFormat;
}
