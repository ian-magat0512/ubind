import { BooleanHelper } from './boolean.helper';

describe('BooleanHelper', () => {

    it('should return false for string "None"', () => {
        let result: boolean = BooleanHelper.fromAny('None');
        expect(result).toBe(false);
    });

    it('should return true for string "Ok"', () => {
        let result: boolean = BooleanHelper.fromAny('Ok');
        expect(result).toBe(true);
    });

    it('should return true for string "Yes"', () => {
        let result: boolean = BooleanHelper.fromAny('Yes');
        expect(result).toBe(true);
    });

    it('should return true for string "True"', () => {
        let result: boolean = BooleanHelper.fromAny('True');
        expect(result).toBe(true);
    });

    it('should return false for number 0', () => {
        let result: boolean = BooleanHelper.fromAny(0);
        expect(result).toBe(false);
    });

    it('should return true for number 1', () => {
        let result: boolean = BooleanHelper.fromAny(1);
        expect(result).toBe(true);
    });

    it('should return true for number 1092384', () => {
        let result: boolean = BooleanHelper.fromAny(1092384);
        expect(result).toBe(true);
    });

    it('should return false for number -1', () => {
        let result: boolean = BooleanHelper.fromAny(-1);
        expect(result).toBe(false);
    });

    it('should return false for number -1.1', () => {
        let result: boolean = BooleanHelper.fromAny(-1.1);
        expect(result).toBe(false);
    });

    it('should return true for number 1.1', () => {
        let result: boolean = BooleanHelper.fromAny(1.1);
        expect(result).toBe(true);
    });

    it('should return true for boolean true', () => {
        let result: boolean = BooleanHelper.fromAny(true);
        expect(result).toBe(true);
    });

    it('should return false for boolean false', () => {
        let result: boolean = BooleanHelper.fromAny(false);
        expect(result).toBe(false);
    });

});
