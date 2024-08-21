import { CurrencyHelper } from './currency.helper';

describe('CurrencyHelper', () => {
    it('should convert 123.45 to $123.45 when currencyCode is AUD', () => {
        let result: string = CurrencyHelper.format(123.45, 'AUD');
        expect(result).toBe('$123.45');
    });

    it('should convert 123.45 to $123.45 when currencyCode is not set', () => {
        let result: string = CurrencyHelper.format(123.45);
        expect(result).toBe('$123.45');
    });

    it('should convert 123.45 to K123.45 when currencyCode is PGK', () => {
        let result: string = CurrencyHelper.format(123.45, 'PGK');
        expect(result).toBe('K123.45');
    });

    it('should convert 123,456.78 to $123,456.78 when currencyCode is USD', () => {
        let result: string = CurrencyHelper.format(123456.78, 'USD');
        expect(result).toBe('$123,456.78');
    });

    it('should convert 123,456.78 to £123,456.78 when currencyCode is GBP', () => {
        let result: string = CurrencyHelper.format(123456.78, 'GBP');
        expect(result).toBe('£123,456.78');
    });

    it('should consider $123.45 to be currency', () => {
        let result: boolean = CurrencyHelper.isCurrency('$123.45');
        expect(result).toBe(true);
    });

    it('should consider K123.45 to be currency', () => {
        let result: boolean = CurrencyHelper.isCurrency('K123.45');
        expect(result).toBe(true);
    });

    it('should consider AUD123.45 to be currency', () => {
        let result: boolean = CurrencyHelper.isCurrency('AUD123.45');
        expect(result).toBe(true);
    });

    it('should consider \'USD 123.45\' to be currency', () => {
        let result: boolean = CurrencyHelper.isCurrency('USD 123.45');
        expect(result).toBe(true);
    });

    it('should consider AUD123,456.78 to be currency', () => {
        let result: boolean = CurrencyHelper.isCurrency('AUD123,456.78');
        expect(result).toBe(true);
    });

    it('should consider 123,456.78 to NOT be currency', () => {
        let result: boolean = CurrencyHelper.isCurrency('123,456.78');
        expect(result).toBe(false);
    });

    it('should consider 456.78 to NOT be currency', () => {
        let result: boolean = CurrencyHelper.isCurrency('456.78');
        expect(result).toBe(false);
    });

    it('should parse \'USD 123.45\' to the number 123.45', () => {
        let result: number = CurrencyHelper.parse('USD 123.45');
        expect(result).toBe(123.45);
    });

    it('should parse \'AUD123.45\' to the number 123.45', () => {
        let result: number = CurrencyHelper.parse('AUD123.45');
        expect(result).toBe(123.45);
    });

    it('should parse \'K123.45\' to the number 123.45', () => {
        let result: number = CurrencyHelper.parse('K123.45');
        expect(result).toBe(123.45);
    });

    it('should parse \'$123.45\' to the number 123.45', () => {
        let result: number = CurrencyHelper.parse('$123.45');
        expect(result).toBe(123.45);
    });

    it('should parse AUD123,456.78 to the number 123456.78', () => {
        let result: number = CurrencyHelper.parse('AUD123,456.78');
        expect(result).toBe(123456.78);
    });

    it('should parse -AUD123,456.78 to the number -123456.78', () => {
        let result: number = CurrencyHelper.parse('-AUD123,456.78');
        expect(result).toBe(-123456.78);
    });

    it('should parse USD-123,456.78 to the number -123456.78', () => {
        let result: number = CurrencyHelper.parse('USD-123,456.78');
        expect(result).toBe(-123456.78);
    });

    it('should parse -K123.45 to the number -123.45', () => {
        let result: number = CurrencyHelper.parse('-K123.45');
        expect(result).toBe(-123.45);
    });

    it('should parse $ -123.45 to the number -123.45', () => {
        let result: number = CurrencyHelper.parse('$ -123.45');
        expect(result).toBe(-123.45);
    });
});
