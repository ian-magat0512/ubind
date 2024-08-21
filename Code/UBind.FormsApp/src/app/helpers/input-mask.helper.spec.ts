import { InputMaskGenerator } from "@app/helpers/Input-mask-generator";
import { InputMask } from "@app/models/input-mask.model";
import { InputFieldMaskConfiguration } from "@app/resource-models/configuration/fields/input-field-mask-configuration";
import { FieldDataType } from "@app/models/field-data-type.enum";
import { InputMaskType } from "@app/models/input-mask-type.enum";

describe('InputMaskGenerator', () => {
    it('should return numeric input mask when property type is set to numeric', () => {
        let inputFieldMaskSetting: InputFieldMaskConfiguration = new InputFieldMaskConfiguration();
        inputFieldMaskSetting.type = InputMaskType.Numeric;

        let inputMask: InputMask = InputMaskGenerator.generateInputMask(inputFieldMaskSetting, FieldDataType.Number);
        expect(inputMask.type).toBe('numeric');
    });

    it('should return pattern input mask when property type is set to pattern', () => {
        let inputFieldMaskSetting: InputFieldMaskConfiguration = new InputFieldMaskConfiguration();
        inputFieldMaskSetting.type = InputMaskType.Pattern;
        inputFieldMaskSetting.pattern = "(00)0000-0000";

        let inputMask: InputMask = InputMaskGenerator.generateInputMask(inputFieldMaskSetting, FieldDataType.Text);
        expect(inputMask.type).toBe('pattern');
    });

    it('should return input mask with separator and decimal precision when decimal precision is set.', () => {
        let inputFieldMaskSetting: InputFieldMaskConfiguration = new InputFieldMaskConfiguration();
        inputFieldMaskSetting.type = InputMaskType.Numeric;
        inputFieldMaskSetting.decimalPrecision = 2;

        let inputMask: InputMask = InputMaskGenerator.generateInputMask(inputFieldMaskSetting, FieldDataType.Number);
        expect(inputMask.mask).toBe('separator.2');
    });

    it('should return input mask with pre decimal digit count limit' +
     'when preDecimalDigitCountLimit property is set.', () => {
        let inputFieldMaskSetting: InputFieldMaskConfiguration = new InputFieldMaskConfiguration();
        inputFieldMaskSetting.type = InputMaskType.Numeric;
        inputFieldMaskSetting.preDecimalDigitCountLimit = 5;

        let inputMask: InputMask = InputMaskGenerator.generateInputMask(inputFieldMaskSetting, FieldDataType.Number);
        expect(inputMask.preDecimalDigitCountLimit).toBe('10000');
    });

    it('should return input mask with thousand separator' +
    'when thousand separator is set', () => {
        let inputFieldMaskSetting: InputFieldMaskConfiguration = new InputFieldMaskConfiguration();
        inputFieldMaskSetting.type = InputMaskType.Numeric;
        inputFieldMaskSetting.thousandSeparator = ",";

        let inputMask: InputMask = InputMaskGenerator.generateInputMask(inputFieldMaskSetting, FieldDataType.Number);
        expect(inputMask.thousandSeparator).toBe(',');
    });

    it('should return input mask with allow negative value set to true when' +
    'allow negative value is set', () => {
        let inputFieldMaskSetting: InputFieldMaskConfiguration = new InputFieldMaskConfiguration();
        inputFieldMaskSetting.type = InputMaskType.Numeric;
        inputFieldMaskSetting.allowNegativeNumberValue = true;

        let inputMask: InputMask = InputMaskGenerator.generateInputMask(inputFieldMaskSetting, FieldDataType.Number);
        expect(inputMask.allowNegativeNumberValue).toBe(true);
    });

    it('should return input mask with prefix ' +
    'when prefix property is set.', () => {
        let inputFieldMaskSetting: InputFieldMaskConfiguration = new InputFieldMaskConfiguration();
        inputFieldMaskSetting.type = InputMaskType.Numeric;
        inputFieldMaskSetting.prefix = 'USD';

        let inputMask: InputMask = InputMaskGenerator.generateInputMask(inputFieldMaskSetting, FieldDataType.Number);
        expect(inputMask.prefix).toBe('USD');
    });

    it('should return input mask with suffix ' +
    'when suffix property is set.', () => {
        let inputFieldMaskSetting: InputFieldMaskConfiguration = new InputFieldMaskConfiguration();
        inputFieldMaskSetting.type = InputMaskType.Numeric;
        inputFieldMaskSetting.suffix = '%';

        let inputMask: InputMask = InputMaskGenerator.generateInputMask(inputFieldMaskSetting, FieldDataType.Number);
        expect(inputMask.suffix).toBe('%');
    });

    it('should return input mask with includePrefixInValue ' +
    'when includePrefixInValue property is set.', () => {
        let inputFieldMaskSetting: InputFieldMaskConfiguration = new InputFieldMaskConfiguration();
        inputFieldMaskSetting.type = InputMaskType.Numeric;
        inputFieldMaskSetting.includePrefixInValue = true;
        inputFieldMaskSetting.prefix = 'USD';

        let inputMask: InputMask = InputMaskGenerator.generateInputMask(inputFieldMaskSetting, FieldDataType.Number);
        expect(inputMask.includePrefixInValue).toBe(true);
        expect(inputMask.prefix).toBe('USD');
    });

    it('should return input mask with includeSuffixInValue ' +
    'when includeSuffixInValue property is set.', () => {
        let inputFieldMaskSetting: InputFieldMaskConfiguration = new InputFieldMaskConfiguration();
        inputFieldMaskSetting.type = InputMaskType.Numeric;
        inputFieldMaskSetting.includeSuffixInValue = true;
        inputFieldMaskSetting.suffix = '%';

        let inputMask: InputMask = InputMaskGenerator.generateInputMask(inputFieldMaskSetting, FieldDataType.Number);
        expect(inputMask.includeSuffixInValue).toBe(true);
        expect(inputMask.suffix).toBe('%');
    });

    it('should return input mask with displayPatternAsPlaceholder ' +
    'when displayPatternAsPlaceholder property is set.', () => {
        let inputFieldMaskSetting: InputFieldMaskConfiguration = new InputFieldMaskConfiguration();
        inputFieldMaskSetting.type = InputMaskType.Pattern;
        inputFieldMaskSetting.displayPatternAsPlaceholder = true;
        inputFieldMaskSetting.pattern = '(00)0000-0000';

        let inputMask: InputMask = InputMaskGenerator.generateInputMask(inputFieldMaskSetting, FieldDataType.Text);
        expect(inputMask.displayPatternAsPlaceholder).toBe(true);
        expect(inputMask.placeholderText).toBe('(__)____-____');
    });

    it('should return input mask with typeOverPlaceholderPattern ' +
    'when typeOverPlaceholderPattern property is set.', () => {
        let inputFieldMaskSetting: InputFieldMaskConfiguration = new InputFieldMaskConfiguration();
        inputFieldMaskSetting.type = InputMaskType.Pattern;
        inputFieldMaskSetting.displayPatternAsPlaceholder = true;
        inputFieldMaskSetting.typeOverPlaceholderPattern = true;
        inputFieldMaskSetting.pattern = '(00)0000-0000';

        let inputMask: InputMask = InputMaskGenerator.generateInputMask(inputFieldMaskSetting, FieldDataType.Text);
        expect(inputMask.typeOverPlaceholderPattern).toBe(true);
    });


    it('should return input mask with placeholderPatternInputCharacter ' +
    'when placeholderPatternInputCharacter property is set.', () => {
        let inputFieldMaskSetting: InputFieldMaskConfiguration = new InputFieldMaskConfiguration();
        inputFieldMaskSetting.type = InputMaskType.Pattern;
        inputFieldMaskSetting.displayPatternAsPlaceholder = true;
        inputFieldMaskSetting.placeholderPatternInputCharacter = 'X';
        inputFieldMaskSetting.pattern = '(00)0000-0000';

        let inputMask: InputMask = InputMaskGenerator.generateInputMask(inputFieldMaskSetting, FieldDataType.Text);
        expect(inputMask.placeholderPatternInputCharacter).toBe('X');
    });

});
