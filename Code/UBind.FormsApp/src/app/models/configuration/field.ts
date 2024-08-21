import { FieldConfiguration } from "@app/resource-models/configuration/fields/field.configuration";
import { OptionConfiguration } from "@app/resource-models/configuration/option.configuration";
import { FormControlValidatorFunction } from "@app/services/validation.service";

/**
 * Represents a field to be rendered on the form
 */
export interface Field {
    className: string;
    key: string;
    templateOptions: {
        fieldConfiguration: FieldConfiguration;
        options: Array<OptionConfiguration>;
        addonLeft?: {
            class?: string;
            text?: string;
        };
        addonRight?: {
            class?: string;
            text?: string;
        };
        formatTextInput?: string;
    };
    type: string;
    validators: {
        validation: Array<FormControlValidatorFunction>;
    };
    customFieldProperties: object;
}
