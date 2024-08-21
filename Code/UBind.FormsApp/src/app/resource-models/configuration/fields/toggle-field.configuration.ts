import { InteractiveFieldConfiguration } from "./interactive-field.configuration";

/**
 * Represents the configuration for a field which can be toggled on or off.
 */
export interface ToggleFieldConfiguration extends InteractiveFieldConfiguration {
    icon: string;
}
