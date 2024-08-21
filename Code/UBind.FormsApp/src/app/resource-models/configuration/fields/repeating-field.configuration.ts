import { RepeatingFieldDisplayMode } from "@app/components/fields/repeating/repeating-field-display-mode.enum";
import { InteractiveFieldConfiguration } from "./interactive-field.configuration";

/**
 * Represents the configuration for a field which allows a defined repeating question set
 * to render itself one or more times, so as to collect a list of data objects.
 */
export interface RepeatingFieldConfiguration extends InteractiveFieldConfiguration {
    /**
     * Gets or sets a value which represents the minimum number of instances of the repeating question set that
     * will be rendered.
     */
    minimumQuantityExpression: string;

    /**
     * Gets or sets a value which represents the maximum number of instances of the repeating question set that
     * will be allowed to be rendered.
     */
    maximumQuantityExpression: string;

    /**
     * Gets or sets the name of the question set which is to be repeated.
     */
    questionSetNameToRepeat: string;

    /**
     * Gets or sets the key of the question set which is to be repeated.
     */
    questionSetKeyToRepeat: string;

    /**
     * Gets or sets a value indicating whether to render the repeating instance heading inside each repeating
     * instance. Defaults to true.
     */
    displayRepeatingInstanceHeading?: boolean;

    /**
     * Gets or sets the heading to be displayed inside each repeating instance.
     * This can contain expressions, e.g. "Claim %{ getRepeatingIndex(fieldPath) + 1 }%".
     */
    repeatingInstanceHeading: string;

    /**
     *  Gets or sets the name of the repeating instance which is used as the heading for each repeating instance
     *  and also appears on the button label.
     *  e.g. "Claim 1" and "Add another Claim".
     */
    repeatingInstanceName: string;

    /**
     *  Gets or sets the heading level to use within each repeating instance, e.g. 4 = "h4", 3 = "h3".
     */
    repeatingInstanceHeadingLevel: number;

    /**
     * Gets or sets the label for the button to add a repeating instance.
     * If AddFirstRepeatingInstanceButtonLabel is not defined, this label will be used for the first button,
     * otherwise it will only be used for subsequent buttons.
     */
    addRepeatingInstanceButtonLabel: string;

    /**
     * Gets or sets the label for the button to add the first repeating instance, which is displayed when there
     * are no repeating instances yet. If not set, it will just use the AddRepeatingInstanceButtonLabel.
     */
    addFirstRepeatingInstanceButtonLabel: string;

    /**
     * Gets or sets the label for the button to remove a repeating instance.
     */
    removeRepeatingInstanceButtonLabel: string;

    /**
     * Gets or sets a value which specifies whether you want to display all or one instance at a time.
     */
    displayMode: RepeatingFieldDisplayMode;

    /**
     * Gets or sets an expression which evaluates to a RepeatingFieldDisplayMode.
     */
    displayModeExpression: string;
}
