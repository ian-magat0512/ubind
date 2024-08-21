import { Pipe, PipeTransform } from '@angular/core';

/**
 * Converts a string into a valid css identifier, by altering or stripping characters.
 * CSS identifiers must only consist of characters [a-zA-Z0-9] and hyphen (-) and underscore (_).
 * They must also not start with a digit, two hypens, or a hypen followed by a digit.
 */
@Pipe({
    name: 'cssIdentifier',
})
export class CssIdentifierPipe implements PipeTransform {

    public transform(value: string): string {
        if (!value) {
            return '';
        }
        let returnValue: string = value.replace(/[\[\]]/g, '');
        returnValue = returnValue.replace(/[^a-zA-Z0-9_-]/g, '-');
        return this.fixStartsWith(returnValue);

    }

    private fixStartsWith(value: string): string {
        let returnValue: string = value;
        if (returnValue.match(/^\d/)) {
            returnValue = `-${returnValue}`;
        }
        if (returnValue.match(/^--/)) {
            returnValue = returnValue.substring(1);
        }
        if (returnValue.match(/^-\d/)) {
            returnValue = `_${returnValue}`;
        }
        if (returnValue == value) {
            return value;
        }
        // recurse once more in case the changes we made caused the css identifier to be invalid
        return this.fixStartsWith(returnValue);
    }
}
