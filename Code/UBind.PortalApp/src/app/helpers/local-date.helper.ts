import { DateComponentParts } from '@app/models/date-component-parts';
import { DateHelper } from './date.helper';

/**
 * Export local date helper class
 * To locale the date formats.
 */
export class LocalDateHelper {

    /**
     * Converts the specified ISO datetime string to local date
     * @param date - the date to convert
     */
    public static toLocalDate(date: string): string {
        const convertedDate: Date = new Date(date);
        return DateHelper.dateToString(convertedDate);
    }

    /**
     * Formats date in relation to today's date with the following rules:
     * - Will show the time if date is today
     * - Will show the Day and Month if date is in the past, but within the year
     * - Will show the Day Month and Year if date is in the past, and from past years
     */
    public static toLocalDateFormattedRelativeToNow(date: string, nowDate?: Date): string {
        if (!nowDate) {
            nowDate = new Date();
        }

        const convertedDate: Date = new Date(date);

        if (nowDate === convertedDate) {
            return this.convertToLocalAndGetTimeOnly(date);
        }
        const localDateWithPartition: DateComponentParts = DateHelper.dateToDateWithPartition(convertedDate);
        const currentDateWithPartition: DateComponentParts = DateHelper.dateToDateWithPartition(nowDate);
        if (localDateWithPartition.year === currentDateWithPartition.year
            && localDateWithPartition.month === currentDateWithPartition.month
            && localDateWithPartition.day === currentDateWithPartition.day) {
            return this.convertToLocalAndGetTimeOnly(date);
        } else if (localDateWithPartition.year === currentDateWithPartition.year) {
            return `${localDateWithPartition.day} ${localDateWithPartition.month}`;
        } else {
            return `${localDateWithPartition.day}/`
                + `${localDateWithPartition.monthNumberString}/`
                + `${localDateWithPartition.year}`;
        }
    }

    /**
     * Converts the specified ISO UTC Format to Local Time ISO Format for the UI ion-datetime control to read.
     * @param date - the dateTimeISOFormat to convert
     */
    public static fromUtcIsoStringToLocalDateTimeIsoString(
        dateTimeISOFormat: string,
        locales?: string | Array<string>,
    ): string {
        let parsedIso: number = Date.parse(dateTimeISOFormat);
        if (isNaN(parsedIso)) {
            throw new Error("ISO Format is invalid.");
        }

        // converts to local time.
        const convertedDate: Date = new Date(parsedIso);

        // converts to a specific format.
        let formattedDateYYYYMMDDWithDashes: string = DateHelper.dateToYYYMMDDWithDashes(convertedDate);
        let localtimeOnlyFormat: string = convertedDate.toLocaleTimeString(
            locales,
            {
                hour: '2-digit',
                minute: '2-digit',
                hour12: false,
            },
        );
        return formattedDateYYYYMMDDWithDashes + 'T' + localtimeOnlyFormat;
    }

    public static convertToLocalAndGetTimeOnly(date: string): string {
        const isIE11: boolean = !((window as any).ActiveXObject) && 'ActiveXObject' in window;
        const convertedDate: Date = new Date(date);

        // do not use 'bestfit' here, because it will display 12pm as 0pm which is very wrong.
        const locale: any = !isIE11 ? 'en-AU' : undefined;
        const options: Intl.DateTimeFormatOptions = {
            hour12: true,
            hour: 'numeric',
            minute: '2-digit' };

        return convertedDate.toLocaleTimeString(locale, options);
    }
}
