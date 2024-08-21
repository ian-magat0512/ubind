import { Pipe, PipeTransform } from '@angular/core';
import { FormatTextInputPipe } from './format-text-input-pipe';

/**
 * Cleans and formats number plates entered into a text field
 */
@Pipe({
    name: 'number-plate',
})
export class NumberPlatePipe extends FormatTextInputPipe implements PipeTransform {

    public clean(value: string): string {
        return value.replace(/[ -]/g, '').toUpperCase();
    }

    public transform(value: string): string {
        let validRegExp1: RegExp = /^[A-Z]{3}[A-Z0-9]{3}$/;
        if (validRegExp1.test(value)) {
            return value.replace(/\B(?=(.{3})+(?!.))/g, '-');
        }
        let validRegExp2: RegExp = /^[a-zA-Z0-9]{1,7}$/;
        if (validRegExp2.test(value)) {
            return value;
        }
        return value;
    }

    public restore(value: string): string {
        return value.replace(/-/g, '');
    }
}
