import { Pipe, PipeTransform } from '@angular/core';

/**
 * Export phone number pipe class.
 * TODO: Write a better class header: phone number pipe functions.
 */
@Pipe({
    name: 'phoneNumber',
})

export class PhoneNumberPipe implements PipeTransform {

    public transform(value: string): string {
        let clean: string = value.replace(/[\ \(\)\-]/g, '');
        let validRegExp: RegExp = /^(\+61|0)[2|4|3|7|8][\d]{8}$/;
        if (validRegExp.test(clean)) {
            let formatted: string;
            if (/^(\+61)4[\d]{8}$/.test(clean)) {
                formatted = clean.replace(/^(\+61)([\d]{3})([\d]{3})([\d]{3})$/, '$1 $2 $3 $4');
            }
            if (/^(\+61)[2|3|7|8][\d]{8}$/.test(clean)) {
                formatted = clean.replace(/^(\+61)([\d]{1})([\d]{4})([\d]{4})$/, '$1 ($2) $3-$4');
            }
            if (/^04[\d]{8}$/.test(clean)) {
                formatted = clean.replace(/^([\d]{4})([\d]{3})([\d]{3})$/, '$1 $2 $3');
            }
            if (/^(0)[2|3|7|8][\d]{8}$/.test(clean)) {
                formatted = clean.replace(/^([\d]{2})([\d]{4})([\d]{4})$/, '($1) $2-$3');
            }
            return formatted;
        }
        return value;
    }

    public restore(value: string): string {
        return value.replace(/[\ \(\)\-]/g, '');
    }

}
