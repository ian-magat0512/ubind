import { Pipe, PipeTransform } from '@angular/core';

/**
 * Export currency pipe class.
 * TODO: Write a better class header: currency pipe functions.
 */
@Pipe({
    name: 'currency',
})

export class CurrencyPipe implements PipeTransform {

    private decimalSeparator: string = '.';
    private thousandSeparator: string = ',';
    private paddingZeros: string = '000000';
    private fractionSize: number = 2;

    public transform(value: string): string {
        let integer: string;
        let fraction: string;
        let newValue: string = value.trim().replace(/[,]/g, '');

        const numVal: number = +newValue;
        if (isNaN(numVal) || numVal == 0) {
            return value;
        }

        if (newValue.indexOf(this.decimalSeparator) == -1) {
            return this.transformWithThousandSeparator(newValue);
        } else {
            integer = newValue.split(this.decimalSeparator)[0] || '0';
            fraction = newValue.split(this.decimalSeparator)[1];

            if (+fraction === 0) {
                return this.transformWithThousandSeparator(integer);
            } else if (fraction.length < this.fractionSize) {
                fraction = fraction + this.paddingZeros.substring(0, this.fractionSize - fraction.length);
            }
            integer = this.transformWithThousandSeparator(integer);
            return integer + this.decimalSeparator + fraction;
        }
    }

    public restore(value: string): string {
        if (value) {
            const clean: any = value.split(',').join('');
            if (!isNaN(clean)) {
                let integer: string;
                let fraction: string;
                if (value.indexOf(this.decimalSeparator) == -1) {
                    integer = clean;
                } else {
                    integer = clean.split(this.decimalSeparator)[0] || '0';
                    fraction = clean.split(this.decimalSeparator)[1];
                }
                if (fraction && +fraction > 0) {
                    return integer + this.decimalSeparator + fraction;
                } else {
                    return integer;
                }
            } else {
                return value;
            }
        } else {
            return value;
        }
    }

    private transformWithThousandSeparator(integer: any): any {
        return integer.replace(/\B(?=(\d{3})+(?!\d))/g, this.thousandSeparator);
    }
}
