import * as moment from 'moment-timezone';

/**
 * Export Aest Date helper class
 * Date formatter or helper class.
 */
export class AestDateHelper {
    /**
     * Converts the timezone string into formatted AU date string
     * @param dateTz - the date timezone
     */
    public static toFormattedDateString(dateTz: string): string {
        return moment.tz(dateTz, 'Australia/Melbourne').format('DD MMM YYYY');
    }

    /**
     * Converts the specified ISO datetime string to AU time
     * @param date - the date to convert
     */
    public static convertToAEST(date: string): string {
        const convertedDate: Date = new Date(date);
        const utcTime: number = convertedDate.getTime() + convertedDate.getTimezoneOffset() * 60000;
        const auDateTime: Date = new Date(utcTime + 3600000 * 10);
        const auDate: string = auDateTime.toString().substr(4, 12);
        return `${auDate.substr(4, 2)} ${auDate.substr(0, 3)} ${auDate.substr(7)}`;
    }

    public static convertToAESTandGetTimeOnly(date: string): string {
        const convertedDate: Date = new Date(date);
        const utcTime: number = convertedDate.getTime() + convertedDate.getTimezoneOffset() * 60000;
        const auDateTime: Date = new Date(utcTime + 3600000 * 10);
        let hours: number = auDateTime.getHours();
        const minutes: string = (auDateTime.getMinutes() + '').length === 1
            ? ('0' + auDateTime.getMinutes()) : (auDateTime.getMinutes() + '');
        let meridiem: string = 'AM';

        if (hours > 12) {
            hours = hours - 12;
            meridiem = 'PM';
        }

        return hours + ':' + minutes + ' ' + meridiem;
    }
}
