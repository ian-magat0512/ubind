import { Pipe, PipeTransform } from '@angular/core';
import { StringHelper } from '@app/helpers';

/**
 * Export beautify pipe class
 * This class for transforming of beautify.
 */
@Pipe({ name: 'beautify' })
export class BeautifyPipe implements PipeTransform {

    public transform(value: string): string {
        return StringHelper.beautify(value);
    }
}
