import { InputFieldMaskConfiguration } from "@app/resource-models/configuration/fields/input-field-mask-configuration";
import { ConditionalInputFieldMaskConfiguration } from
    "@app/resource-models/configuration/fields/conditional-input-field-mask-configuration";
import { FieldConfiguration } from "./field.configuration";

/**
 * Represents the input mask configuration of the field.
 */
export interface InputMaskConfiguration extends FieldConfiguration {
    inputMask: InputFieldMaskConfiguration;
    inputMaskList: Array<ConditionalInputFieldMaskConfiguration>;
}
