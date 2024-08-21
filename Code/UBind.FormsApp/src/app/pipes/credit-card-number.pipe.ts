import { Pipe, PipeTransform } from '@angular/core';

/**
 * Export credit card number pipe class.
 * TODO: Write a better class header: Credit card number pipe functions.
 */
@Pipe({
    name: 'creditCardNumber',
})

export class CreditCardNumberPipe implements PipeTransform {

    public transform(value: string): string {
        let sanitized: string = value.replace(/[^0-9]+/g, '');
        let validRegExp: RegExp = /^\d{16}$/;
        if (validRegExp.test(sanitized)) {
            return sanitized.replace(/\B(?=(\d{4})+(?!\d))/g, '-');
        }
        return value;
    }

    public restore(value: string): string {
        return value.replace(/[ -]/g, '');
    }

}
