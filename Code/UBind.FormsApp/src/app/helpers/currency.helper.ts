import { CurrencyPipe } from "@angular/common";
import { Locale } from "./locale";
import * as numberToWord from 'number-to-cyrillic';
import * as pluralize from 'pluralize';
import { Currency } from "@app/models/currency.model";

/**
 * Export currency helper class.
 * Provides the ability to parse, format currency.
 * @dynamic
 */
export class CurrencyHelper {
    private currencies: Map<string, Currency>;
    public constructor() {
        this.currencies = new Map([
            ["AUD", new Currency("dollar", "cent")],
            ["USD", new Currency("dollar", "cent")],
            ["BSD", new Currency("dollar", "cent")],
            ["BBD", new Currency("dollar", "cent")],
            ["BMD", new Currency("dollar", "cent")],
            ["BND", new Currency("dollar", "cent")],
            ["CAD", new Currency("dollar", "cent")],
            ["KYD", new Currency("dollar", "cent")],
            ["XCD", new Currency("dollar", "cent")],
            ["SVC", new Currency("dollar", "cent")],
            ["FJD", new Currency("dollar", "cent")],
            ["GYD", new Currency("dollar", "cent")],
            ["HKD", new Currency("dollar", "cent")],
            ["LRD", new Currency("dollar", "cent")],
            ["NAD", new Currency("dollar", "cent")],
            ["NZD", new Currency("dollar", "cent")],
            ["SGD", new Currency("dollar", "cent")],
            ["SBD", new Currency("dollar", "cent")],
            ["SRD", new Currency("dollar", "cent")],
            ["TVD", new Currency("dollar", "cent")],
            ["ARS", new Currency("pesos", "centavo")],
            ["CLP", new Currency("pesos", "centavo")],
            ["COP", new Currency("pesos", "centavo")],
            ["KHR", new Currency("riel", "cent")],
            ["EUR", new Currency("euro", "cent")],
            ["GBP", new Currency("pound", "penny")],
            ["GGP", new Currency("pound", "penny")],
            ["FKP", new Currency("pound", "penny")],
            ["GIP", new Currency("pound", "penny")],
            ["IMP", new Currency("pound", "penny")],
            ["JEP", new Currency("pound", "penny")],
            ["SHP", new Currency("pound", "penny")],
            ["PGK", new Currency("kina", "toea")],
        ]);
        pluralize.addIrregularRule('kina', 'kina');
        pluralize.addIrregularRule('penny', 'pence');
        pluralize.addIrregularRule('toea', 'toea');
    }

    public static commaSeparatedToNumber(input: string): number {
        return Number(input.replace(/[^0-9-.]/g, ''));
    }

    public static format(
        value: number,
        currencyCode: string = 'AUD',
        display: string = 'symbol-narrow',
        digitsInfo?: string,
        locale?: string,
    ): string {
        if (!locale) {
            locale = (new Locale()).getLocale();
        }
        if (!currencyCode) {
            currencyCode = 'AUD';
        }
        let currencyPipe: CurrencyPipe = new CurrencyPipe(locale);
        let formatted: string = currencyPipe.transform(value, currencyCode, display, digitsInfo, locale);
        if (formatted && display == 'symbol-narrow') {
            if (formatted.indexOf('PGK') != -1) {
                formatted = formatted.replace('PGK', 'K');
            }
        }
        return formatted;
    }

    public static isCurrency(currencyString: string): boolean {
        // return currencyString.replace(/\s/, '').match(/^(\$|-\$)(\d+|\d{1,3}(,\d{3})*)(\.\d{1,2})?$/) ? true : false;
        return currencyString.match(/^([^\d])+(\d+|\d{1,3}(,\d{3})*)(\.\d{1,2})?$/) ? true : false;
    }

    public static parse(currencyString: string): number {
        if (typeof currencyString == 'number') {
            // It's already a number so no need to parse it.
            return currencyString;
        }

        // TODO: we need a better way to do this, which takes into account locale
        // since some countries use comma instead of space
        // this needs to be revisited properly.
        let stripped: string = currencyString.replace(/[^\d.-]/g, '');

        // TODO for currency we need to not use float because of artithmetic errors
        // 0.1 + 0.2 = 0.30000000000000004 :( that is not correct.
        // https://frontstuff.io/how-to-handle-monetary-values-in-javascript
        let parsed: number = parseFloat(stripped);
        return parsed;
    }

    /**
     * Converts an amount to a formatted currency value and returns the minor units
     * @param value 
     */
    public static getMinorUnitsFormatted(
        value: number,
        padIfZero: boolean = true,
    ): string {
        let minorValue: number = value % 1;
        if (minorValue == 0.0) {
            return padIfZero ? '00' : '';
        }
        let formatted: string = Math.round(minorValue * 100).toString();
        return formatted.padStart(2, '0');
    }

    /**
     * Converts an amount to a formatted currency value and returns the major units
     * @param value 
     */
    public static getMajorUnitsFormatted(
        value: number,
        currencyCode: string = 'AUD',
        locale?: string,
        display: string = 'symbol-narrow',
    ): string {
        let digitsInfo: string = '1.0-0';
        let majorValue: number = Math.floor(value);
        return CurrencyHelper.format(majorValue, currencyCode, display, digitsInfo, locale);
    }

    public static getUnitsSeparator(currencyCode: string, locale?: string): string {
        let testValue: number = 1.1;
        let formattedTestValue: string = CurrencyHelper.format(testValue, currencyCode, null, null, locale);
        let commaPosition: number = formattedTestValue.lastIndexOf(',');
        return commaPosition != -1 ? ',' : '.';
    }

    public static getThousandsSeparator(currencyCode: string, locale?: string): string {
        let testValue: number = 1000.1;
        let formattedTestValue: string = CurrencyHelper.format(testValue, currencyCode, null, null, locale);
        let commaPosition: number = formattedTestValue.lastIndexOf(',');
        let dotPosition: number = formattedTestValue.lastIndexOf('.');
        if (commaPosition == -1 || dotPosition == -1) {
            return '';
        }
        return commaPosition < dotPosition ? ',' : '.';
    }

    /**
     * Removes the thousands separator from a currency string.
     */
    public static removeThousandsSeparator(input: string, currencyCode: string, locale?: string): string {
        input = `${input}`;
        let thousandsSeparator: string = CurrencyHelper.getThousandsSeparator(currencyCode, locale);
        let regex: RegExp = new RegExp(thousandsSeparator, 'g');
        return input.replace(regex, '');
    }

    public convertNumberToWords(value: any, currencyCode: string, includeMinorUnits: boolean,
        localeCode: string): string {
        const englishLanguage: string = 'en';
        const locale: Locale = new Locale();
        if (!localeCode) {
            localeCode = locale.getLocale();
        }

        const currencyName: Currency = this.currencies.get(currencyCode.toUpperCase());
        if (!currencyName) {
            throw new Error(`The currency '${currencyCode}' is not supported`);
        }

        const languageCode: string = locale.getLanguageCode(localeCode);
        if (languageCode != englishLanguage) {
            throw new Error(`The locale '${localeCode}' is not supported.`
                + ' Only English is currently supported when converting numbers to words.');
        }

        const word: any = numberToWord.convert(value,
            {
                currency: false,
                language: languageCode,
                capitalize: true,
            });
        const isWholeNumber: boolean = value % 1 == 0;
        const convertedInteger: string = word.convertedInteger;
        const integerCurrency: string =  currencyName.integerCurrency;
        const convertedFractional: string = word.convertedFractional.replace(/- /gi, "");
        const fractionalCurrency: string = currencyName.fractionalCurrency;
        const convertedWords: string = includeMinorUnits && !isWholeNumber ?
            `${convertedInteger} ` + pluralize(integerCurrency, word.integer) +
        ` and ${convertedFractional} ` + pluralize(fractionalCurrency, word.fractional) :
            `${convertedInteger} ` + pluralize(integerCurrency, word.integer);
        return convertedWords;
    }
}
