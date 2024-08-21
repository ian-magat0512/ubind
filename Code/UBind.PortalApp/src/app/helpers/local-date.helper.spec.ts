import { } from 'jasmine';
import { LocalDateHelper } from './local-date.helper';

describe('LocalDateHelper.fromUtcIsoStringToLocalDateTimeIsoString', () => {
    it('should convert iso string to local date acceptable format', () => {
        console.log(`using language: ${navigator.language}`);
        let isoString: string = "2011-05-05T00:00:00Z";
        let formBuilderDateFormat: string
            = LocalDateHelper.fromUtcIsoStringToLocalDateTimeIsoString(isoString, 'en-AU');
        let date: Date = new Date(isoString);
        let testResult: string =
            date.getFullYear()
            + "-"
            + ((date.getMonth() < 10 ? "0" : "") + (date.getMonth() + 1))
            + "-"
            + ((date.getDate() < 10 ? "0" : "") + date.getDate())
            + "T"
            + ((date.getHours() < 10 ? "0" : "") + date.getHours())
            + ":"
            + ((date.getMinutes() < 10 ? "0" : "") + date.getMinutes());

        expect(formBuilderDateFormat).toBe(testResult);
    });

    it('should throw an error if the input is null or empty', () => {
        expect(() => LocalDateHelper.fromUtcIsoStringToLocalDateTimeIsoString("")).toThrow(
            new Error('ISO Format is invalid.'),
        );
        expect(() => LocalDateHelper.fromUtcIsoStringToLocalDateTimeIsoString(null)).toThrow(
            new Error('ISO Format is invalid.'),
        );
    });

    it('should throw an error if the input is not in valid ISO format', () => {
        expect(() => LocalDateHelper.fromUtcIsoStringToLocalDateTimeIsoString("2030-12-35T12:00:00Z")).toThrow(
            new Error('ISO Format is invalid.'),
        );
        expect(() => LocalDateHelper.fromUtcIsoStringToLocalDateTimeIsoString("2021-03-25T15w:59:00Z")).toThrow(
            new Error('ISO Format is invalid.'),
        );
    });

    it('toLocalDateFormattedRelativeToNow should show only the time part if date is today', () => {
        console.log(`using language: ${navigator.language}`);
        // Arrange
        const fakeTodayDate: string = '2020-12-01T12:00:00Z';
        const fakeTargetDate: string = '2020-12-01T10:12:33Z';
        // Act
        const result: string = LocalDateHelper.toLocalDateFormattedRelativeToNow(
            fakeTargetDate,
            new Date(fakeTodayDate),
        );

        // Assert
        expect(result.substring(1).toUpperCase()).toEqual(':12 PM');
    });

    it('toLocalDateFormattedRelativeToNow should show only the day and month part if date is same year', () => {
        // Arrange
        const fakeTodayDate: string = '2020-07-01T12:00:00Z';
        const fakeTargetDate: string = '2020-12-01T10:12:33Z';
        // Act
        const result: string = LocalDateHelper.toLocalDateFormattedRelativeToNow(
            fakeTargetDate,
            new Date(fakeTodayDate),
        );

        // Assert
        expect(result).toEqual('01 Dec');
    });

    it('toLocalDateFormattedRelativeToNow should show day, month and year if date has a different year', () => {
        // Arrange
        const fakeTodayDate: any = '2020-07-01T12:00:00Z';
        const fakeTargetDate: any = '2019-12-01T10:12:33Z';
        // Act
        const result: string = LocalDateHelper.toLocalDateFormattedRelativeToNow(
            fakeTargetDate,
            new Date(fakeTodayDate),
        );

        // Assert
        expect(result).toEqual('01/12/2019');

        // Arrange
        const fakeTargetDate2: any = '2019-01-01T10:12:33Z';
        // Act
        const result2: string = LocalDateHelper.toLocalDateFormattedRelativeToNow(
            fakeTargetDate2,
            new Date(fakeTodayDate),
        );
        // Assert
        expect(result2).toEqual('01/01/2019');
    });
});
