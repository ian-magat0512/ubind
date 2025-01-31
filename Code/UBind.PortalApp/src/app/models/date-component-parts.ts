/**
 * Export date component parts class
 * TODO: Write a better class header: component of date parts functions.
 */
export class DateComponentParts {
    public month: string = '';
    public monthNumberString: string = '';
    public day: string = '';
    public year: string = '';

    public constructor(month: string, day: string, year: string, monthNumber: number) {
        this.month = month;
        this.day = day;
        this.year = year;
        this.monthNumberString = this.getMonthNumberString(monthNumber);
    }

    private getMonthNumberString(monthNumber: number): string {
        switch (monthNumber) {
            case 0: {
                return '01';
            }
            case 1: {
                return '02';
            }
            case 2: {
                return '03';
            }
            case 3: {
                return '04';
            }
            case 4: {
                return '05';
            }
            case 5: {
                return '06';
            }
            case 6: {
                return '07';
            }
            case 7: {
                return '08';
            }
            case 8: {
                return '09';
            }
            case 9: {
                return '10';
            }
            case 10: {
                return '11';
            }
            case 11: {
                return '12';
            }
            default: {
                throw new Error(`Invalid month : ${monthNumber}`);
            }
        }

    }
}
