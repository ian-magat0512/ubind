import { Pipe, PipeTransform } from '@angular/core';

/**
 * Export time pipe class.
 * TODO: Write a better class header: Time pipe functions.
 */
@Pipe({
    name: 'time',
})

export class TimePipe implements PipeTransform {

    public transform(value: string): string {
        value = value.trim();
        if (!value) return '';

        let regExp: RegExp = /^(\d){2}:(\d){2}[ ]?(AM|PM)?$/i;
        if (regExp.test(value)) {
            let hours: number = parseInt(value.substring(0, 2), 10);
            let minutes: number = parseInt(value.split(':')[1].substring(0, 2), 10);

            let amPm: string = value.toLowerCase().indexOf('am') > -1
                ? 'AM'
                : value.toLowerCase().indexOf('pm') > -1
                    ? 'PM'
                    : null;

            if (hours >= 24 ||
                hours < 0 ||
                minutes >= 60 ||
                minutes < 0 ||
                (amPm && (hours > 12 || hours == 0))) {

                return value;
            }

            if (!amPm) {
                if (hours > 11) {
                    hours -= 12;
                    amPm = 'PM';
                } else {
                    amPm = 'AM';
                }
                if (hours == 0) {
                    hours = 12;
                }
            }

            let hoursStr: string = hours < 10 ? '0' + hours : '' + hours;
            let minutesStr: string = minutes < 10 ? '0' + minutes : '' + minutes;

            return hoursStr + ':' + minutesStr + ' ' + amPm;
        }
        return value;
    }

    public restore(value: string): string {
        return value;
    }
}
