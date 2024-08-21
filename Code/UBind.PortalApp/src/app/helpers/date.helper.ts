import * as moment from 'moment-timezone';
import { DateComponentParts } from '@app/models/date-component-parts';

/**
 * Helper class for dates.
 */
export class DateHelper {

    /**
     * Format date into sequence of day, month name abbreviation and full year
     * example: 01 Apr 2022
     * @param date 
     */
    public static formatDDMMMYYYY(date: string): string {
        const convertedDate: Date = new Date(date);
        return moment(convertedDate).format('DD MMM YYYY');
    }

    /**
     * Format date into sequence of day, full month name and full year
     * example: 01 April 2022
     * @param date 
     */
    public static formatDDMMMMYYYY(date: string): string {
        const convertedDate: Date = new Date(date);
        return moment(convertedDate).format('DD MMMM YYYY');
    }

    /**
     * Format date into sequence of year, month and day
     * example: 2022-04-01
     * @param date 
     */
    public static formatYYYYMMDD(date: Date): string {
        return moment(date).format('YYYY-MM-DD');
    }

    public static checkDateIfItsInTheFuture(date: string): boolean {

        let newDate: string = date.split('/').join('');
        newDate = newDate.split('-').join('');

        const targetDate: number = +(newDate);
        const nowDate: number = +(moment(new Date()).format('YYYYMMDD'));

        if (targetDate > nowDate) {
            return true;
        }

        return false;
    }

    public static formBuilderDate(date: string): string {
        const convertedDate: Date = new Date(Date.parse(date));
        return DateHelper.formatYYYYMMDD(convertedDate);
    }

    public static displayFormatDate(date: string): string {
        const convertedDate: Date = new Date(Date.parse(date));
        return `${(convertedDate.getDate() < 10 ? '0' : '') + convertedDate.getDate()}` +
            `${convertedDate.toLocaleString('en-us', { month: 'short' })} ${convertedDate.getFullYear()}`;
    }

    public static dateToString(date: Date): string {
        const convertedDate: string = date.toString().substr(4, 12);
        return `${convertedDate.substr(4, 2)} ${convertedDate.substr(0, 3)} ${convertedDate.substr(7)}`;
    }

    public static dateToYYYMMDDWithDashes(date: Date): string {
        return date.getFullYear()
            + "-"
            + ((date.getMonth() < 10 ? "0" : "") + (date.getMonth() + 1))
            + "-"
            + ((date.getDate() < 10 ? "0" : "") + date.getDate());
    }

    public static daysToSeconds(days: number): number {
        return days * 24 * 60 * 60;
    }

    public static secondToDays(seconds: number): number {
        return seconds / 24 / 60 / 60;
    }

    public static getDateToday(): string {
        const nowDate: Date = new Date();
        return DateHelper.dateToString(nowDate);
    }

    public static getFutureDateIsoString(
        yearsToAdd: number,
        monthsToAdd: number,
        daysToAdd: number,
    ): string {
        const nowDate: Date = new Date();
        const year: number = nowDate.getFullYear() + yearsToAdd;
        const month: number = nowDate.getMonth() + monthsToAdd;
        const day: number = nowDate.getDate() + daysToAdd;

        return new Date(year, month, day).toISOString();
    }

    public static isDateToday(date: string): boolean {
        const nowDate: Date = new Date();
        const targetDate: Date = new Date(date);
        const nowDateString: string = DateHelper.dateToString(nowDate);
        const targetDateString: string = DateHelper.dateToString(targetDate);
        if (nowDateString === targetDateString) {
            return true;
        }
        return false;
    }

    public static dateToDateWithPartition(date: Date): DateComponentParts {
        const localDate: string = date.toString().substr(4, 12);
        const dateWithPartition: DateComponentParts = new DateComponentParts(
            localDate.substr(0, 3).trim(),
            localDate.substr(4, 2).trim(),
            localDate.substr(7).trim(),
            date.getMonth(),
        );
        return dateWithPartition;
    }

    private static getMonthName(monthNumber: number): string {
        switch (monthNumber) {
            case 1: {
                return 'January';
            }
            case 2: {
                return 'February';
            }
            case 3: {
                return 'March';
            }
            case 4: {
                return 'April';
            }
            case 5: {
                return 'May';
            }
            case 6: {
                return 'June';
            }
            case 7: {
                return 'July';
            }
            case 8: {
                return 'August';
            }
            case 9: {
                return 'September';
            }
            case 10: {
                return 'October';
            }
            case 11: {
                return 'November';
            }
            case 12: {
                return 'December';
            }
            default: {
                throw new Error(`Invalid month : ${monthNumber}`);
            }
        }
    }

    public static getMMMYYYYfromFullMonthYear(date: string): string {
        let yearIndex: number = date.length - 4;
        return `${date.substring(0, 3)} ${date.substring(yearIndex)}`;
    }

    public static getTimeZoneId(): string {
        return Intl.DateTimeFormat().resolvedOptions().timeZone;
    }
}
