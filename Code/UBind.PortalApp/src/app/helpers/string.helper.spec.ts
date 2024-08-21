import { } from 'jasmine';
import { StringHelper } from './string.helper';

const stringHelper: StringHelper = new StringHelper();

describe('StringHelper.capitalizeFirstLetter', () => {
    it('should capitalize first letter', () => {
        expect(StringHelper.capitalizeFirstLetter('ubind')).toEqual('Ubind');
    });
    it('should capitalize first letter only', () => {
        expect(StringHelper.capitalizeFirstLetter('uBind')).toEqual('UBind');
    });
});

describe('stringHelper.equalsIgnoreCase', () => {
    it('should match lower and uppercase strings', () => {
        expect(stringHelper.equalsIgnoreCase('ubind', 'UBIND')).toBe(true);
    });
    it('should match Proper & lowercase strings', () => {
        expect(stringHelper.equalsIgnoreCase('Ubind', 'ubind')).toBe(true);
    });
    it('should match UPPER & camelCase strings', () => {
        expect(stringHelper.equalsIgnoreCase('UBIND', 'uBind')).toBe(true);
    });
    it('should match the same lowercase strings', () => {
        expect(stringHelper.equalsIgnoreCase('ubind', 'ubind')).toBe(true);
    });
    it('should match the same UPPERCASE strings', () => {
        expect(stringHelper.equalsIgnoreCase('UBIND', 'UBIND')).toBe(true);
    });
    it('should not match different strings', () => {
        expect(stringHelper.equalsIgnoreCase('ubind', 'notubind')).toBe(false);
    });
    it('should be able to handle undefined', () => {
        expect(stringHelper.equalsIgnoreCase('ubind', undefined)).toBe(false);
    });
    it('should be able to handle null', () => {
        expect(stringHelper.equalsIgnoreCase('ubind', null)).toBe(false);
    });
    it('should not match NULL and empty string', () => {
        expect(stringHelper.equalsIgnoreCase(null, '')).toBe(false);
    });
    it('should not match empty and NULL string', () => {
        expect(stringHelper.equalsIgnoreCase('', null)).toBe(false);
    });
    it('should not match undefined and empty string', () => {
        expect(stringHelper.equalsIgnoreCase(undefined, '')).toBe(false);
    });
    it('should match null and undefined', () => {
        expect(stringHelper.equalsIgnoreCase(null, undefined)).toBe(true);
    });
});

describe('StringHelper.camelToSentenceCase', () => {

    it('returns sentence case from camel case strings', () => {
        expect(StringHelper.camelToSentenceCase('camelCase')).toBe('Camel Case');
        expect(StringHelper.camelToSentenceCase('specialRecipe')).toBe('Special Recipe');
        expect(StringHelper.camelToSentenceCase('dinugoan')).toBe('Dinugoan');
        expect(StringHelper.camelToSentenceCase('Price%100')).toBe('Price%100');
        expect(StringHelper.camelToSentenceCase('price%100')).toBe('Price%100');
        expect(StringHelper.camelToSentenceCase('Welcome Here')).toBe('Welcome Here');
        expect(StringHelper.camelToSentenceCase('WelcomeHere')).toBe('Welcome Here');
        expect(StringHelper.camelToSentenceCase('')).toBe('');
        expect(StringHelper.camelToSentenceCase(null)).toBe('');
    });
});

describe('StringHelper.camelCase', () => {

    it('returns camel case from any string', () => {
        expect(StringHelper.camelCase('camel case')).toBe('camelCase');
        expect(StringHelper.camelCase('Camel Case')).toBe('camelCase');
        expect(StringHelper.camelCase('camel Case')).toBe('camelCase');
        expect(StringHelper.camelCase('camel some Case')).toBe('camelSomeCase');
        expect(StringHelper.camelCase('camel Case 200')).toBe('camelCase200');
        expect(StringHelper.camelCase('camel 200 ph')).toBe('camel200Ph');
        expect(StringHelper.camelCase('')).toBe('');
        expect(StringHelper.camelCase(null)).toBe('');
    });
});

describe('StringHelper.beautify', () => {

    it('Capitalizes first letter and gives proper spacing if camel case', () => {
        expect(StringHelper.beautify('beautifyMe')).toBe('Beautify Me');
        expect(StringHelper.beautify('beautify100')).toBe('Beautify 100');
        expect(StringHelper.beautify('beautify me please')).toBe('Beautify me please');
        expect(StringHelper.beautify('beautify-100')).toBe('Beautify- 100');
        expect(StringHelper.beautify('beautify-200')).toBe('Beautify- 200');
        expect(StringHelper.beautify('BeautifyMePlease200')).toBe('Beautify Me Please 200');
        expect(StringHelper.beautify('')).toBe('');
        expect(StringHelper.beautify(null)).toBe(null);
    });
});
