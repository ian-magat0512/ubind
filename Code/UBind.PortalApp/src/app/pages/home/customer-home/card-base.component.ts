import { Directive, Input } from '@angular/core';
import { DateHelper } from '@app/helpers/date.helper';

/**
 * Export card based component abstract class
 * TODO: Write a better class header: displaying of card based.
 */
@Directive({ selector: '[appCardBase]' })
export abstract class CardBaseComponent {

    @Input() public permission: any;
    @Input() public isLoading: boolean = true;

    public constructor() { }

    public getDisplayDatePhrase(dateToFormat: string): string {
        const stringDate: string = DateHelper.formatDDMMMMYYYY(dateToFormat);
        let displayPhrase: string = '';
        if (DateHelper.formatDDMMMYYYY(dateToFormat) == DateHelper.formatDDMMMYYYY(
            new Date().toLocaleDateString(),
        )) {
            displayPhrase = `expires today`;
        } else if (Date.parse(dateToFormat) > Date.now()) {
            displayPhrase = `will expire on ${stringDate}`;
        } else {
            displayPhrase = `expired on ${stringDate}`;
        }
        return displayPhrase;
    }
}
