import { TestBed } from "@angular/core/testing";
import { LocaleService } from "./locale.service";
import { FormatWidth, getLocaleDateFormat } from '@angular/common';

/* global spyOn */

describe('LocaleService', () => {
    let localeService: LocaleService;
    let currencyCode: string;
    let locale: string;
    let actualLocale: string;
    let expectedDateFormat: string;
    let actualDateFormat: string;

    beforeEach(() => {
        localeService = TestBed.inject(LocaleService);
    });

    it('should initialise the correct browser locale', async () => {
        // Arrange
        locale = 'de-DE';
        actualLocale = 'de';
        currencyCode = 'EUR';
        spyOn(localeService, 'getLocaleCodeFromBrowser').and.returnValue(locale);
        expectedDateFormat = 'dd.MM.y';

        // Act
        await localeService.initialiseBrowserLocaleAndCurrency(currencyCode).then(() => {
            // Will throw an error if locale was not registered
            actualDateFormat = getLocaleDateFormat(actualLocale, FormatWidth.Medium);

            // Assert
            expect(localeService.getLocale()).toBe(actualLocale);
            expect(localeService.getCurrencyCode()).toBe(currencyCode);
            expect(localeService.getCurrencyLocale()).toBe(actualLocale);
            expect(localeService.getLanguageCode()).toBe(actualLocale);
            expect(localeService.getRegisteredLocales()).toContain(actualLocale);
            expect(actualDateFormat).toBe(expectedDateFormat);
        });
    }, 10000);

    it('should initialise the correct browser locale', async () => {
        // Arrange
        actualLocale = 'en-AU';
        currencyCode = 'AUD';
        spyOn(localeService, 'getLocaleCodeFromBrowser').and.returnValue(actualLocale);
        expectedDateFormat = 'd MMM y';

        // Act
        await localeService.initialiseBrowserLocaleAndCurrency(currencyCode).then(() => {
            // Will throw an error if locale was not registered
            actualDateFormat = getLocaleDateFormat(actualLocale, FormatWidth.Medium);

            // Assert
            expect(localeService.getLocale()).toBe(actualLocale);
            expect(localeService.getCurrencyCode()).toBe(currencyCode);
            expect(localeService.getCurrencyLocale()).toBe(actualLocale);
            expect(localeService.getLanguageCode()).toBe('en');
            expect(localeService.getRegisteredLocales()).toContain(actualLocale);
            expect(actualDateFormat).toBe(expectedDateFormat);
        });
    }, 10000);

    it(`should register a currency's default locale`, async () => {
        // Arrange
        currencyCode = 'PGK';
        actualLocale = 'en-PG';
        expectedDateFormat = 'd MMM y';

        // Act
        await localeService.initialiseOrGetCurrencyLocaleAsync(currencyCode).then((resolve: string) => {
            // Will throw an error if locale was not registered
            actualDateFormat = getLocaleDateFormat(actualLocale, FormatWidth.Medium);

            // Assert
            expect(resolve).toBe(actualLocale);
            expect(localeService.getRegisteredLocales()).toContain(actualLocale);
            expect(actualDateFormat).toBe(expectedDateFormat);
        });
    }, 10000);

    it('should return the registered locale on second call', async () => {
        // Arrange
        currencyCode = 'PGK';
        actualLocale = 'en-PG';
        expectedDateFormat = 'd MMM y';

        // Act
        await localeService.initialiseOrGetCurrencyLocaleAsync(currencyCode)
            .then((resolve: string) => {
                // Will throw an error if locale was not registered
                actualDateFormat = getLocaleDateFormat(actualLocale, FormatWidth.Medium);

                // Assert
                expect(resolve).toBe(actualLocale);
                expect(localeService.getRegisteredLocales()).toContain(actualLocale);
                expect(actualDateFormat).toBe(expectedDateFormat);

                // Arrange
                const addToRegisteredLocalesFx: any
                    = spyOn<any>(localeService, 'addToRegisteredLocales').and.callThrough();

                // Act
                localeService.initialiseOrGetCurrencyLocaleAsync(currencyCode)
                    .then((resolve: string) => {

                        // Assert
                        expect(resolve).toBe(actualLocale);
                        expect(localeService.getRegisteredLocales()).toContain(actualLocale);
                        expect(addToRegisteredLocalesFx).not.toHaveBeenCalled();
                        expect(actualDateFormat).toBe(expectedDateFormat);
                    });
            });
    }, 10000);

    it('should return the correct language code', () => {
        // Arrange
        locale = 'pt-BR';

        // Act
        let languageCode: string = localeService.getLanguageCode(locale);

        // Assert
        expect(languageCode).toBe('pt');
    }, 10000);
});
