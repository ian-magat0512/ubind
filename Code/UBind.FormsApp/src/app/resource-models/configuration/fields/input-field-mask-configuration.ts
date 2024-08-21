import { InputMaskType } from "@app/models/input-mask-type.enum";

/**
 * Represents the Input Masking of a field.
 */
export class InputFieldMaskConfiguration {
    public type: InputMaskType;
    public pattern: string;
    public decimalPrecision: number;
    public preDecimalDigitCountLimit: number;
    public thousandSeparator: string;
    public allowNegativeNumberValue: boolean;
    public prefix: string = "";
    public hidePrefixWhenInputValueIsEmpty: boolean;
    public suffix: string;
    public hideSuffixWhenInputValueIsEmpty: boolean;
    public includePrefixInValue: boolean;
    public includeSuffixInValue: boolean;
    public removeNonInputCharactersFromValue: boolean;
    public removeOnlySpecificNonInputCharactersFromValue: string;
    public padDateAndTimeValuesWithLeadingZeros: boolean;
    public displayPatternAsPlaceholder: boolean;
    public typeOverPlaceholderPattern: boolean;
    public placeholderPatternInputCharacter: string;
}
