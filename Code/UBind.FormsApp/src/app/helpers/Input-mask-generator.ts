import { FieldDataType } from "@app/models/field-data-type.enum";
import { InputMask } from "@app/models/input-mask.model";
import { InputFieldMaskConfiguration } from "@app/resource-models/configuration/fields/input-field-mask-configuration";
import { InputMaskType } from "@app/models/input-mask-type.enum";
import { AnyHelper } from "./any.helper";
import { StringHelper } from "./string.helper";

/**
 * This class is used for generating the input mask settings needed by the ngx-mask library
 * (and our input fields) for applying an input mask. It uses our source configuration and the field type
 * to generate the necessary properties.
 */
export class InputMaskGenerator {

    public static defaultMaskingCharacters: Array<string> = ['-', '/', '(', ')', ':',
        ' ', '+', ',', '.', '@', '[', ']', '"', '\''];

    public static generateInputMask(maskSetting: InputFieldMaskConfiguration, fieldDataType: FieldDataType): InputMask {
        if(!maskSetting) {
            return;
        }

        let inputMask: InputMask = new InputMask();
        inputMask.type = maskSetting.type;
        if(maskSetting.type == InputMaskType.Numeric) {
            inputMask.mask = this.getNumericMasking(maskSetting);
            inputMask.preDecimalDigitCountLimit = this.getPreDecimalDigitCountLimit(maskSetting);
            inputMask.thousandSeparator = maskSetting.thousandSeparator;
            inputMask.allowNegativeNumberValue = maskSetting.allowNegativeNumberValue;
        } else if(maskSetting.type == InputMaskType.Pattern) {
            inputMask.mask = maskSetting.pattern;
            inputMask.padDateAndTimeValuesWithLeadingZeros = maskSetting.padDateAndTimeValuesWithLeadingZeros;
            inputMask.placeholder = maskSetting.displayPatternAsPlaceholder
                ? this.replaceInputCharacterWithUnderScore(maskSetting.pattern)
                : "";
            inputMask.placeholderText = inputMask.placeholder;
            inputMask.typeOverPlaceholderPattern = maskSetting.typeOverPlaceholderPattern &&
            maskSetting.displayPatternAsPlaceholder;

            inputMask.placeholderPatternInputCharacter =
            maskSetting.displayPatternAsPlaceholder && maskSetting.placeholderPatternInputCharacter
                ? maskSetting.placeholderPatternInputCharacter
                : "";
        }

        inputMask.isNumericDataType = fieldDataType == FieldDataType.Number ||
        fieldDataType == FieldDataType.Percent ||
        fieldDataType == FieldDataType.Currency;

        inputMask.removeNonInputCharactersFromValue = inputMask.isNumericDataType
            ? true
            : maskSetting.removeNonInputCharactersFromValue;

        inputMask.prefix = maskSetting.prefix ??  "";
        inputMask.suffix = maskSetting.suffix ?? "";
        inputMask.includePrefixInValue = maskSetting.includePrefixInValue;
        inputMask.includeSuffixInValue = maskSetting.includeSuffixInValue;
        inputMask.displayPatternAsPlaceholder = maskSetting.displayPatternAsPlaceholder;
        inputMask.hidePrefixWhenInputValueIsEmpty = maskSetting.hidePrefixWhenInputValueIsEmpty;
        inputMask.specialCharacters = this.getSpecialCharacters(maskSetting);
        return inputMask;
    }

    private static getNumericMasking(inputFieldMaskSetting: InputFieldMaskConfiguration): string {
        return AnyHelper.hasNoValue(inputFieldMaskSetting.decimalPrecision) ?
            `separator` : `separator.${inputFieldMaskSetting.decimalPrecision}`;
    }

    public static getPreDecimalDigitCountLimit(inputMask: InputFieldMaskConfiguration): string {
        return inputMask.preDecimalDigitCountLimit ? `1${"0".repeat(inputMask.preDecimalDigitCountLimit -1)}` : "";
    }

    public static replaceInputCharacterWithUnderScore(input: string): string {
        return input.replace(/[^a-zA-Z()-/]/g, "_");
    }

    public static formatStringToDecimalWithLimitedPlaces(
        rawNumberString: string,
        maximumDecimalPlaces: number,
        thousandSeparatorSymbol: string): string {
        let regexPattern: any = new RegExp(thousandSeparatorSymbol, 'g');
        rawNumberString = rawNumberString.replace(regexPattern, '').replace(/\D/g, ".");

        if (!rawNumberString.includes('.')) {
            return rawNumberString;
        }

        let decimalLimitedNumber: string = this.truncateToSpecifiedDecimalPlaces(rawNumberString, maximumDecimalPlaces);
        let decimalPart: string = decimalLimitedNumber.split('.')[1];

        if (Number(decimalPart) === 0) {
            decimalLimitedNumber = decimalLimitedNumber.substring(0, decimalLimitedNumber.indexOf('.'));
        }

        return decimalLimitedNumber;
    }

    public static truncateToSpecifiedDecimalPlaces(
        inputNumberAsString: string,
        maximumDecimalPlaces: number): string {
        if (!inputNumberAsString.includes('.')) {
            return inputNumberAsString;
        }
        let decimalPosition: number = inputNumberAsString.indexOf('.');
        let truncationPosition: number = decimalPosition + maximumDecimalPlaces + 1;
        return inputNumberAsString.substring(0, truncationPosition);
    }

    public static removeNonInputCharacter(input: string, thousandSeparator: string, prefix: string) {
        if (input == undefined || StringHelper.isNullOrEmpty(input)) {
            return input;
        }

        const specialCharacterRegEx: RegExp =  /[\\/() \[:+@\]-]/g;
        input = input.replace(specialCharacterRegEx, '');
        input = input.replace(prefix, '');
        const output: string = input.replace(new RegExp(thousandSeparator, 'g'), '');
        return  output;
    }

    public static getSpecialCharacters(inputMask: InputFieldMaskConfiguration): Array<string> {
        if (!inputMask.pattern) {
            return this.defaultMaskingCharacters;
        }
        const nonInputCharacters: Array<string> = this.getNonInputCharacter(inputMask.pattern);
        const specialCharacters: Array<string> = this.hasNonDefaultMaskingCharacter(nonInputCharacters)
            && nonInputCharacters.length > 0 ? Array.from(nonInputCharacters) : this.defaultMaskingCharacters;
        return specialCharacters;
    }

    public static hasNonDefaultMaskingCharacter(nonInputCharacters: Array<string>): boolean {
        for (let nonInputCharacter of nonInputCharacters) {
            if(this.defaultMaskingCharacters.includes(nonInputCharacter)) {
                return true;
            }
        }
        return false;
    }

    public static getNonInputCharacter(inputPattern: string): Array<string> {
        if (!inputPattern) {
            return this.defaultMaskingCharacters;
        }

        const inputMaskCharacters: Set<string> = new Set(inputPattern.replace(/[^a-zA-Z0-9]/g, ""));
        for (let inputMaskCharacter of inputMaskCharacters) {
            inputPattern = StringHelper.replaceAll(inputPattern, inputMaskCharacter, "");
        }
        const uniqueNonInputCharacter: Set<string> = new Set(inputPattern);
        return Array.from(uniqueNonInputCharacter);
    }

}
