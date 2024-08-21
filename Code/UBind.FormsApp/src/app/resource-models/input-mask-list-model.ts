import { InputFieldMaskConfiguration } from "@app/resource-models/configuration/fields/input-field-mask-configuration";

/** This class is used to store the input mask list after evaluating the show when expression.
 * When show when expression value changed the evaluatedShowWhenExpression will be updated.
 * We can then filter input mask list to show only when the evaluated show when expression is true and
 * set the input field mask to the first record of input mask list.
 */
export class InputMaskListModel {
    public inputMaskConfiguration: InputFieldMaskConfiguration;
    public evaluatedShowWhenExpression: boolean;
}
