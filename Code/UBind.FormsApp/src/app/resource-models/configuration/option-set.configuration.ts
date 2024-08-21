import { OptionConfiguration } from "./option.configuration";

/**
 * Represents the configuration of an option set.
 */
export interface OptionSetConfiguration {
    name: string;
    key: string;
    options: Array<OptionConfiguration>;
}
