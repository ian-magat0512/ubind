import { CurrencyPipe } from "@angular/common";
import { Locale } from "./locale";

/**
 * Tools to help with currency values, including formatting and parsing them.
 * @dynamic
 */
export class CurrencyHelper {
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
        if (display == 'symbol-narrow') {
            if (formatted.indexOf('PGK') != -1) {
                formatted = formatted.replace('PGK', 'K');
            }
        }
        return formatted;
    }

    public static isCurrency(currencyString: string): boolean {
        return currencyString.match(/^([^\d])+(\d+|\d{1,3}(,\d{3})*)(\.\d{1,2})?$/) ? true : false;
    }

    public static parse(currencyString: string): number {
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
}
