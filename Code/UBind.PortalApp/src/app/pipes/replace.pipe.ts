import { Pipe, PipeTransform } from '@angular/core';

/**
 * Export replace pipe class.
 * TODO: Write a better class header: replace pipe function.
 */
@Pipe({ name: 'replace' })
export class ReplacePipe implements PipeTransform {

    public transform(value: string, regexValue: string, replaceValue: string): any {
        let regex: RegExp = new RegExp(regexValue, 'g');
        return value.replace(regex, replaceValue);
    }
}
