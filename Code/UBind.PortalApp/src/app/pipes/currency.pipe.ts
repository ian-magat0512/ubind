import { Injectable, Pipe, PipeTransform } from '@angular/core';
import { CurrencyHelper } from '@app/helpers/currency.helper';
import { Locale } from '@app/helpers/locale';

/**
 * Similar to the standard CurrencyPipe in Angular, however it provides additional
 * narrow symbols for certain currencies, e.g. PGK => K
 */
@Injectable({ providedIn: 'root' })
@Pipe({ name: 'currency' })
export class CurrencyPipe implements PipeTransform {

    public constructor(private locale: Locale) {
    }

    public transform(
        value: any,
        currencyCode?: string,
        display?: 'code' | 'symbol' | 'symbol-narrow' | string,
        digitsInfo?: string,
        locale?: string,
    ): string | null {
        locale = locale ? locale : this.locale.getLocale();
        return CurrencyHelper.format(value, currencyCode, display, digitsInfo, locale);
    }
}
