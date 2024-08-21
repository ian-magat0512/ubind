import { DateHelper } from "./date.helper";

describe('DateHelper.formatDDMMMYYYY', () => {

    it('returns the correct sequence of day, month name abbreviation and full year format', () => {
        const date: Date = new Date('April 1, 2022');
        const formattedDate: string = DateHelper.formatDDMMMYYYY(date.toUTCString());
        expect(formattedDate).toBe('01 Apr 2022');
    });
});

describe('DateHelper.formatDDMMMMYYYY', () => {

    it('returns the correct sequence of day, full month name and full year format', () => {
        const date: Date = new Date('April 1, 2022');
        const formattedDate: string = DateHelper.formatDDMMMMYYYY(date.toUTCString());
        expect(formattedDate).toBe('01 April 2022');
    });
});

describe('DateHelper.formatYYYYMMDD', () => {

    it('returns the correct sequence of year, month and day format', () => {
        const date: Date = new Date('April 1, 2022');
        const formattedDate: string = DateHelper.formatYYYYMMDD(date);
        expect(formattedDate).toBe('2022-04-01');
    });
});

describe('DateHelper.getMMMYYYYfromFullMonthYear ', () => {
    const fullMonthYearCases: Array<any> = [
        { input: 'January 2024', expectedResult: 'Jan 2024' },
        { input: 'February 2024', expectedResult: 'Feb 2024' },
        { input: 'March 2024', expectedResult: 'Mar 2024' },
        { input: 'April 2024', expectedResult: 'Apr 2024' },
        { input: 'May 2024', expectedResult: 'May 2024' },
        { input: 'Jun 2024', expectedResult: 'Jun 2024' },
        { input: 'July 2024', expectedResult: 'Jul 2024' },
        { input: 'August 2024', expectedResult: 'Aug 2024' },
        { input: 'September 2024', expectedResult: 'Sep 2024' },
        { input: 'October 2024', expectedResult: 'Oct 2024' },
        { input: 'November 2024', expectedResult: 'Nov 2024' },
        { input: 'December 2024', expectedResult: 'Dec 2024' },

    ];

    fullMonthYearCases.forEach((testCase: any) => {
        it(`returns the shortened month, year format ${testCase.expectedResult}` +
            `from full month, year format ${testCase.input}`, () => {
            // Act
            const formattedDate: string = DateHelper.getMMMYYYYfromFullMonthYear(testCase.input);

            // Assert
            expect(formattedDate).toBe(testCase.expectedResult);
        });
    });
});


