import { OnInit, Input, ViewChild, ElementRef, OnChanges, AfterViewChecked, Directive } from '@angular/core';
import { IonicHelper } from '@app/helpers/ionic.helper';
import { ChartColorModel } from '@app/models/chart-color.model';

declare const google: any;
declare const window: any;

/**
 * Export base chart component class
 * This class component is all the base chart functions
 * Drawing of charts, formatter and period changes.
 */
@Directive({ selector: '[appBaseChart]' })
export abstract class BaseChartComponent implements OnInit, OnChanges, AfterViewChecked {

    @Input()
    public title: string = '';

    @Input()
    public data: any = {};

    @ViewChild('target', { static: true }) public target: ElementRef;
    @ViewChild('chartColorReference', { read: ElementRef, static: true }) public chartColorReference: ElementRef;

    public canReDraw: boolean = false;
    public isLoading: boolean = false;

    public didChecked: boolean = false;
    public setAriaLabel: any = IonicHelper.setAriaLabel;
    private noBackgroundColor: string = 'rgba(0, 0, 0, 0)';

    public constructor() { }

    public ngOnInit(): void {
        if (window.charts === undefined) {
            window.charts = [];
        }

        google.charts.load('current', { 'packages': ['corechart'] });
        google.charts.setOnLoadCallback(this.drawChart);
        this.init();
        this.isLoading = true;
    }

    public ngOnChanges(simpleChanges: any): void {
        if (Object.prototype.hasOwnProperty.call(simpleChanges, 'data')
            && !simpleChanges.data.firstChange) {
            this.updateData();
            this.isLoading = false;
            if (this.canReDraw) this.drawChart();
        }
    }

    public ngAfterViewChecked(): void {
        if (this.didChecked === false) {
            setTimeout(() => window.dispatchEvent(new Event('resize')), 1000);
            this.didChecked = true;
        }
    }

    public getChartVisibilityState(): string {
        return this.isLoading ? 'hidden' : 'visible';
    }

    public abstract init(): void;

    public abstract drawChart(): void;

    public abstract updateData(): void;

    public getBackgroundColors(backGroundColors: Array<ChartColorModel>): Array<string> {
        let colors: Array<string> = [];

        backGroundColors.forEach((c: ChartColorModel) => {
            this.chartColorReference.nativeElement.style.backgroundColor = c.override;
            let override: string = window.getComputedStyle(this.chartColorReference.nativeElement).backgroundColor;
            if (!override || override == this.noBackgroundColor) {
                colors.push(c.default);
            } else {
                colors.push(override);
            }
        });

        return colors;
    }

    protected createToolTip(
        title: string,
        amount: number,
        period: string,
        currencyPrefix: string,
        isWholeNumber: boolean): string {
        let signPrefix: string = amount < 0 ? '-' : '';
        amount = Math.abs(amount);
        let formattedValue: string = `${amount}`;
        if (!isWholeNumber && amount != undefined) {
            formattedValue = amount.toLocaleString(undefined, {
                style: 'decimal',
                minimumFractionDigits: 2,
                maximumFractionDigits: 2,
            });
        }
        return `<div class="chart-tooltip"><b>${period}</b>
        </br>${title}: <b>${signPrefix}${currencyPrefix}${formattedValue}</b></div>`;
    }

    protected getFormattedValue(amount: number, prefix: string): any {
        const formats: Array<any> = [
            { symbol: 'T', factor: 1e12 },
            { symbol: 'B', factor: 1e9 },
            { symbol: 'M', factor: 1e6 },
            { symbol: 'k', factor: 1e3 },
            { symbol: '', factor: 1 },
        ];
        let selectedFormat: any = formats.find((format: any) => amount >= format.factor);
        if (!selectedFormat) {
            selectedFormat = formats[formats.length - 1]; // Use the last format if none match
        }
        const { symbol, factor }: any = selectedFormat;
        const shortenedAmount: number = amount / factor;

        let formattedValue: string = `${shortenedAmount}`;
        if (factor == 1) {
            // make sure it doesn't have excess zeros
            formattedValue = shortenedAmount.toLocaleString(undefined, {
                style: 'decimal',
                minimumFractionDigits: 0,
                maximumFractionDigits: 1,
            });
        }
        return {
            formatted: `${ prefix }${ formattedValue }${ symbol }`,
            raw: amount,
            symbol,
            shortenedAmount,
            factor,
        };
    }

    protected getCeilingAmount(amount: number): number {
        const formattedObj: any = this.getFormattedValue(amount, '');
        if (formattedObj.shortenedAmount < 10) {
            return Math.ceil(formattedObj.shortenedAmount) * formattedObj.factor;
        }

        // if the value is greater than 10, round it up to the nearest 10s
        const remainder: number = formattedObj.shortenedAmount % 10;
        let ceilingAmount: number = formattedObj.raw;
        if (remainder > 0) {
            ceilingAmount = (formattedObj.shortenedAmount + (10 - remainder)) * formattedObj.factor;
        }
        return ceilingAmount;
    }

    /** This is to get a properly formatted ticks for the vertical axis (i.e. 100k instead of 100000.00).
     * We need to pass in the max value of the right axis (amount),
     * and the prefix symbol for the currency.
     * Once plotted, the left axis automatically adjusts its ticks to the ticks of the right axis
     * */
    protected getTicksForVerticalRightAxis(maxValue: number, prefix: string = ''): Array<any> {
        let ticks: Array<any> = [];
        if (maxValue == 0) {
            for (let i: number = 0; i <= 1; i++) {
                ticks.push({
                    v: i,
                    f: this.getFormattedValue(i, prefix).formatted,
                });
            }
            return ticks;
        }

        // To use a vertical left axis of either 4 or 3 ticks
        // on a 6 ticks vertical right axis (including zero):
        // generate a tick for every 1/4 of the max value up to the max value
        // add one more interval on top of max value, this will result in:
        // either 4 (left ticks): 6 (right ticks) or 3 (left ticks): 6 (right ticks)
        // thus getting left and right ticks to align
        const interval: number = this.getCeilingAmount(maxValue) / 4;
        for (let i: number = 0; i < 6; i++) {
            ticks.push({
                v: i * interval,
                f: this.getFormattedValue(i * interval, prefix).formatted,
            });
        }
        return ticks;
    }
}
