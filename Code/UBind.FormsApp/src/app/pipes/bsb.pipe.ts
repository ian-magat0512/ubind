import { Pipe, PipeTransform } from '@angular/core';

/**
 * Export bsp pipe class.
 * TODO: Write a better class header: BSB pipe functions.
 */
@Pipe({
    name: 'bsb',
})

export class BsbPipe implements PipeTransform {

    public transform(value: string): string {
        let result: string = value;
        let validRegExp: RegExp = /^(\d[ -]*?){6}$/;
        if (validRegExp.test(value)) {
            result = value.replace(/[ -]/g, '');
            return result.replace(/\B(?=(\d{3})+(?!\d))/g, '-');
        }
        return result;
    }

    public restore(value: string): string {
        return value.replace(/-/g, '');
    }

}
