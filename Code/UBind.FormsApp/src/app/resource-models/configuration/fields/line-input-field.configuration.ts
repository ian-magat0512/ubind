import { KeyboardInputMode } from "@app/models/keyboard-input-mode.enum";
import { FieldConfiguration } from "./field.configuration";

/**
 * Represents the configuration of a field which has a line of input.
 * e.g. SingleLineText, Currency, DropDownSelect, SearchSelect.
 */
export interface LineInputFieldConfiguration extends FieldConfiguration {
    /**
     * Gets or sets the icon to display on the left side of the field.
     * This is a css class or a number of css classes, e.g. "fa fa-phone".
     */
    iconLeft: string;

    /**
     * Gets or sets the icon to display on the right side of the field.
     * This is a css class or a number of css classes, e.g. "fa fa-phone".
     */
    iconRight: string;

    /**
     * Gets or sets the text to display on the left side of the field.
     */
    textLeft: string;

    /**
     * Gets or sets the text to display on the right side of the field.
     */
    textRight: string;

    /**
     * Gets or sets the keyboard input mode.
     */
    keyboardInputMode: KeyboardInputMode;
}
