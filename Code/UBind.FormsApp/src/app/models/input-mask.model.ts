/**
 * This class is used for input mask configuration.
 */
export class InputMask {
    public type: string;
    public mask: string;
    public thousandSeparator: string = "";
    public preDecimalDigitCountLimit: string = "";
    public allowNegativeNumberValue: boolean = false;
    public hidePrefixWhenInputValueIsEmpty: boolean;
    public hideSuffixWhenInputValueIsEmpty: boolean;
    public specialCharacters: Array<string>;
    public removeNonInputCharactersFromValue: boolean = false;
    public removeOnlySpecificNonInputCharactersFromValue: any;
    public padDateAndTimeValuesWithLeadingZeros: boolean;
    public typeOverPlaceholderPattern: boolean = false;
    public displayPatternAsPlaceholder: boolean = false;
    public placeholderPatternInputCharacter: string = "";
    public placeholderText: string;
    public placeholder: string;
    public prefix: string;
    public suffix: string;
    public includePrefixInValue: boolean;
    public includeSuffixInValue: boolean;
    public isNumericDataType: boolean = false;
}
