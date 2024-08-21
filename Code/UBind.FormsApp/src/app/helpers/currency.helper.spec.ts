import { TestBed } from '@angular/core/testing';
import { LocaleService } from '@app/services/locale.service';
import { CurrencyHelper } from './currency.helper';

/* global spyOn */

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

    it('should return "45" when requesting the minor units of 123.45', () => {
        let result: string = CurrencyHelper.getMinorUnitsFormatted(123.45, true);
        expect(result).toBe('45');
    });

    it('should return "00" (empty string) when requesting the minor units of 123.00', () => {
        let result: string = CurrencyHelper.getMinorUnitsFormatted(123.00, true);
        expect(result).toBe('00');
    });

    it('should return "05" (empty string) when requesting the minor units of 123.05', () => {
        let result: string = CurrencyHelper.getMinorUnitsFormatted(123.05, true);
        expect(result).toBe('05');
    });

    it('should return "$1,234" when requesting the major units of 1234.56', () => {
        let result: string = CurrencyHelper.getMajorUnitsFormatted(1234.56);
        expect(result).toBe('$1,234');
    });

    it('should return "." when requesting the unit separator for "GBP"', () => {
        let result: string = CurrencyHelper.getUnitsSeparator('GBP');
        expect(result).toBe('.');
    });

    it('should return "," when requesting the unit separator for "DM" with local "de"', async () => {
        let localeService: LocaleService = TestBed.inject(LocaleService);
        spyOn(localeService, 'getLocaleCodeFromBrowser').and.returnValue('de');
        await localeService.initialiseBrowserLocaleAndCurrency('DEM').then(() => {
            let result: string = CurrencyHelper.getUnitsSeparator('DEM', 'de');
            expect(result).toBe(',');
        });
    }, 10000);

    it('should return "100" when requesting the thousands separator be removed from "1,00"', () => {
        let result: string = CurrencyHelper.removeThousandsSeparator('1,00', 'AUD');
        expect(result).toBe('100');
    });

    it('should return "aaa" when requesting the thousands separator be removed from "aaa"', () => {
        let result: string = CurrencyHelper.removeThousandsSeparator('aaa', 'AUD');
        expect(result).toBe('aaa');
    });

});
