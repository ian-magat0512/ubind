import { StringHelper } from "./string.helper";

describe('StringHelper', () => {
    it('converts "A Custom String" to "aCustomString" when calling toCamelCase', () => {
        expect(StringHelper.toCamelCase('A Custom String')).toBe('aCustomString');
    });

    it('converts "Step 1" to "step1" when calling toCamelCase', () => {
        expect(StringHelper.toCamelCase('Step 1')).toBe('step1');
    });

    it('converts "Step1" to "step1" when calling toCamelCase', () => {
        expect(StringHelper.toCamelCase('Step1')).toBe('step1');
    });

    it('converts "kebab-to-camel" to "kebabToCamel" when calling toCamelCase', () => {
        expect(StringHelper.toCamelCase('kebab-to-camel')).toBe('kebabToCamel');
    });
});
