import { InputFieldMaskConfiguration } from "./input-field-mask-configuration";

/**
 * This class is used to hold the input mask list configuration of the field. 
 * The first item in the "inputMaskList" whose "useWhenExpression" evaluates to
 * true should have its "inputMask" settings applied to the input field.
 */
export interface ConditionalInputFieldMaskConfiguration {
    inputMask: InputFieldMaskConfiguration;
    useWhenExpression: string;
}
