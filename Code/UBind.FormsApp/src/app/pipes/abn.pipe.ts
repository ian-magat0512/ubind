import { Pipe, PipeTransform } from '@angular/core';

/**
 * export ABN pipe class.
 * TODO: Write a better class header: ABN pipe functions.
 */
@Pipe({
    name: 'abn',
})

export class AbnPipe implements PipeTransform {

    public transform(value: string): string {
        let result: string = value;
        let validRegExp: RegExp = /^(\d *?){11}$/;
        if (validRegExp.test(value)) {
            result = value.replace(/ /g, '');
            return result.replace(/\B(?=(\d{3})+(?!\d))/g, ' ');
        }
        return result;
    }

    public restore(value: string): string {
        return value.replace(/ /g, '');
    }

}
