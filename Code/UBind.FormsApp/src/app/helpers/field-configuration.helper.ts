import { Errors } from "@app/models/errors";
import { FieldDataType } from "@app/models/field-data-type.enum";
import { FieldSelector } from "@app/models/field-selectors.enum";
import { FieldType } from "@app/models/field-type.enum";
import { TextInputFormat } from "@app/models/text-input-format.enum";
import { CurrencyFieldConfiguration } from "@app/resource-models/configuration/fields/currency-field.configuration";
import {
    DataStoringFieldConfiguration,
} from "@app/resource-models/configuration/fields/data-storing-field.configuration";
import { FieldConfiguration } from "@app/resource-models/configuration/fields/field.configuration";
import {
    InteractiveFieldConfiguration,
} from "@app/resource-models/configuration/fields/interactive-field.configuration";
import { LineInputFieldConfiguration } from "@app/resource-models/configuration/fields/line-input-field.configuration";
import { OptionsFieldConfiguration } from "@app/resource-models/configuration/fields/options-field.configuration";
import { RepeatingFieldConfiguration } from "@app/resource-models/configuration/fields/repeating-field.configuration";
import {
    SingleLineTextFieldConfiguration,
} from "@app/resource-models/configuration/fields/single-line-text-field.configuration";
import { ToggleFieldConfiguration } from "@app/resource-models/configuration/fields/toggle-field.configuration";
import { VisibleFieldConfiguration } from "@app/resource-models/configuration/fields/visible-field.configuration";
import { WebhookFieldConfiguration } from "@app/resource-models/configuration/fields/webhook-field.configuration";

/**
 * Provides custom type guards to help us determine the type of an instance of FieldConfiguration
 */
export class FieldConfigurationHelper {

    public static isDataStoringField(field: FieldConfiguration): field is DataStoringFieldConfiguration {
        return field.$type != FieldType.Content
            && field.$type != FieldType.Iframe
            && field.$type != FieldType.Repeating
            && field.$type != FieldType.Webhook;
    }

    public static isRepeatingField(field: FieldConfiguration): field is RepeatingFieldConfiguration {
        return field.$type == FieldType.Repeating;
    }

    public static isCurrencyField(field: FieldConfiguration): field is CurrencyFieldConfiguration {
        return field.$type == FieldType.Currency;
    }

    public static isVisibleField(field: FieldConfiguration): field is VisibleFieldConfiguration {
        return field.$type != FieldType.Webhook;
    }

    public static isInteractiveField(field: FieldConfiguration): field is InteractiveFieldConfiguration {
        return field.$type != FieldType.Content
            && field.$type != FieldType.Hidden
            && field.$type != FieldType.Iframe
            && field.$type != FieldType.Repeating;
    }

    public static isToggleField(field: FieldConfiguration): field is ToggleFieldConfiguration {
        return field.$type == FieldType.Toggle || field.$type == FieldType.Checkbox;
    }

    public static isOptionsField(field: FieldConfiguration): field is OptionsFieldConfiguration {
        return (field as OptionsFieldConfiguration).optionSetKey !== undefined
            || (field as OptionsFieldConfiguration).optionsRequest !== undefined;
    }

    public static isWebhookField(field: FieldConfiguration): field is WebhookFieldConfiguration {
        return (field as WebhookFieldConfiguration).urlExpression !== undefined;
    }

    public static isSingleLineTextField(field: FieldConfiguration): field is SingleLineTextFieldConfiguration {
        return field.$type == FieldType.SingleLineText;
    }

    public static isLineInputField(field: FieldConfiguration): field is LineInputFieldConfiguration {
        return field.$type == FieldType.SingleLineText
            || field.$type == FieldType.Currency
            || field.$type == FieldType.DropDownSelect
            || field.$type == FieldType.SearchSelect
            || field.$type == FieldType.DatePicker
            || field.$type == FieldType.Password;
    }

    public static isTextInputField(field: FieldConfiguration): boolean {
        return field.$type == FieldType.SingleLineText
            || field.$type == FieldType.Currency
            || field.$type == FieldType.DatePicker
            || field.$type == FieldType.Password;
    }

    /**
     * Comparator function for sorting. 
     * Returns:
     *    0 if the two fields have equal sort value
     *    -1 if first should appear before second
     *    1 if second should appear before first
     */
    public static compareByCalculationWorkbookRow(first: FieldConfiguration, second: FieldConfiguration): number {
        let firstScore: number = 0;
        let secondScore: number = 0;
        if (FieldConfigurationHelper.isDataStoringField(first)) {
            if (first.calculationWorkbookCellLocation) {
                firstScore += first.calculationWorkbookCellLocation.sheetIndex * 100000;
                firstScore += first.calculationWorkbookCellLocation.rowIndex;
            }
        }
        if (FieldConfigurationHelper.isDataStoringField(second)) {
            if (second.calculationWorkbookCellLocation) {
                secondScore += second.calculationWorkbookCellLocation.sheetIndex * 100000;
                secondScore += second.calculationWorkbookCellLocation.rowIndex;
            }
        }
        return firstScore - secondScore;
    }

    public static getFieldSelector(fieldType: FieldType): FieldSelector {
        switch (fieldType) {
            case FieldType.Attachment:
                return FieldSelector.Attachment;
            case FieldType.Buttons:
                return FieldSelector.Buttons;
            case FieldType.Checkbox:
                return FieldSelector.Checkbox;
            case FieldType.Content:
                return FieldSelector.Content;
            case FieldType.Currency:
                return FieldSelector.Currency;
            case FieldType.DatePicker:
                return FieldSelector.DatePicker;
            case FieldType.DropDownSelect:
                return FieldSelector.DropDownSelect;
            case FieldType.Hidden:
                return FieldSelector.Hidden;
            case FieldType.Iframe:
                return FieldSelector.Iframe;
            case FieldType.Password:
                return FieldSelector.Password;
            case FieldType.Radio:
                return FieldSelector.Radio;
            case FieldType.Repeating:
                return FieldSelector.Repeating;
            case FieldType.SearchSelect:
                return FieldSelector.SearchSelect;
            case FieldType.SingleLineText:
                return FieldSelector.SingleLineText;
            case FieldType.TextArea:
                return FieldSelector.TextArea;
            case FieldType.Toggle:
                return FieldSelector.Toggle;
            case FieldType.Webhook:
                return FieldSelector.Webhook;
            case FieldType.Slider:
                return FieldSelector.Slider;
            default:
                throw Errors.General.Unexpected(`When trying to get the selector for a field with type "${fieldType}", `
                    + 'we could not find a match.');
        }
    }

    public static getTextInputFormat(fieldDataType: FieldDataType): TextInputFormat {
        switch (fieldDataType) {
            case FieldDataType.Abn:
                return TextInputFormat.Abn;
            case FieldDataType.Currency:
                return TextInputFormat.Currency;
            case FieldDataType.NumberPlate:
                return TextInputFormat.NumberPlate;
            case FieldDataType.Phone:
                return TextInputFormat.PhoneNumber;
            case FieldDataType.Time:
                return TextInputFormat.Time;
        }
        return TextInputFormat.None;
    }
}
