import { formatNumber } from '@angular/common';
import { Locale } from './locale';

/**
 * Providers tools for working with Numbers
 */
export class NumberHelper {

    /**
     * Formats a number according to the users local, e.g. by adding in commas for thousands separators.
     * @param number the number to format.
     */
    public static format(
        number: number,
        digitsInfo?: string,
        locale?: string,
    ): string {
        if (!locale) {
            locale = (new Locale()).getLocale();
        }
        if (!digitsInfo) {
            digitsInfo = '1.0-99';
        }
        return formatNumber(number, locale, digitsInfo);
    }

    public static isNumber(value: any): boolean {
        return !isNaN(value) && value !== '' && value !== null && value !== undefined;
    }

    public static roundPrecisionError(value: any): number {
        let numberValue: number = <number>value;
        if (typeof value !== 'number') {
            numberValue = parseFloat(value);
        }
        return parseFloat(numberValue.toPrecision(12));
    }
}
