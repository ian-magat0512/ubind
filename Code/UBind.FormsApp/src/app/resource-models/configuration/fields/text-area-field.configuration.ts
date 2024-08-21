import { KeyboardInputMode } from "@app/models/keyboard-input-mode.enum";
import { InteractiveFieldConfiguration } from "./interactive-field.configuration";

/**
 * Represents the configuration for a field which can have multiple lines of text.
 */
export interface TextAreaFieldConfiguration extends InteractiveFieldConfiguration {
    keyboardInputMode: KeyboardInputMode;
}
