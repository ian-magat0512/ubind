import {
    Component,
    EventEmitter,
    HostListener,
    Input,
    Output,
    ViewChild,
    ElementRef,
} from '@angular/core';
import { loaderAnimation } from '@assets/animations';
import { trigger, state, transition, animate, style } from '@angular/animations';
import { BaseChartComponent } from '../base-chart.component';
import * as moment from 'moment';
import 'moment-timezone';
import { EventService } from '@app/services/event.service';
import { ChartColorModel } from '@app/models/chart-color.model';
import { ChartUnitModel } from '@app/models/chart-unit.model';
import { ChartPeriodicDataModel } from '@app/models/chart-periodic-data-model';
import { ChartPeriodSelectionModel } from '@app/models/chart-period-selection-model';
import { ChartSamplePeriodLength } from '@app/models/chart-sample-period-length.enum';
import { ChartOptionsModel } from '@app/models/chart-options-model';
import { DateHelper } from '@app/helpers/date.helper';

declare const google: any;
declare const window: any;

/**
 * Export column chart component class
 * This class component is all the column chart functions
 * Drawing of charts, formatter and period changes.
 */
@Component({
    selector: 'app-column-chart',
    templateUrl: './column-chart.component.html',
    styleUrls: ['./column-chart.component.scss'],
    animations: [
        trigger('chartAnimation', [
            state('visible', style({ opacity: 100 })),
            state('hidden', style({ opacity: 0 })),
            transition('hidden => visible', animate('200ms ease-in-out')),
            transition('visible => hidden', animate('200ms ease-in-out')),
        ]),
        loaderAnimation,
    ],
})
export class ColumnChartComponent extends BaseChartComponent {
    @Input()
    public data: Array<ChartPeriodicDataModel> = [];
    @Input()
    public vAxesLeftTitle: string = 'Number Generated';
    @Input()
    public vAxesRightTitle: string = 'Total Premium';
    @Input()
    public backGroundColors: Array<ChartColorModel> = [
        {
            default: "#00695c",
            override: "#00695c",
        },
        {
            default: "#26a69a",
            override: "#26a69a",
        }];
    @Input()
    public textColors: Array<ChartColorModel> = [
        {
            default: "#ffffff",
            override: "#ffffff",
        },
        {
            default: "#ffffff",
            override: "#ffffff",
        }];
    @Input()
    public titleColor: ChartColorModel =
        {
            default: "#00695c",
            override: "#00695c",
        };
    @Input()
    public unit: ChartUnitModel =
        {
            type: 'currency',
            postSymbol: '',
            prefixSymbol: '$',
        };
    @Output()
    public selectedPeriodChangeEvent: EventEmitter<ChartPeriodSelectionModel>
        = new EventEmitter<ChartPeriodSelectionModel>();

    @ViewChild('currency', { read: ElementRef, static: true }) public currencyElement: ElementRef;

    public dataGroupings: Array<ChartOptionsModel> = [
        { label: 'Daily', value: ChartSamplePeriodLength.Day },
        { label: 'Monthly', value: ChartSamplePeriodLength.Month },
        { label: 'Quarterly', value: ChartSamplePeriodLength.Quarter },
        { label: 'Yearly', value: ChartSamplePeriodLength.Year },
    ];
    public ranges: Array<string> = [];
    public selectedDataGrouping: ChartOptionsModel = this.dataGroupings[1];
    public selectedRange: string;
    public vAxisLeftValue: number = 0;
    public vAxisRightValue: number = 0;
    private formattedData: Array<any> = [];
    private availableRanges: Map<string, Array<string>> = new Map<string, Array<string>>();
    private chartColors: Array<string> = ["#00695c", "#26a69a"];
    private maxValueRightAxis: number = 0;

    public constructor(private eventService: EventService) {
        super();
    }

    public init(): void {
        if (this.title === '') {
            this.title = 'Column Periodic';
        }

        this.eventService.dashboardProductFilterChangedSubject$.subscribe(() => {
            this.requestChartUpdate();
        });
        this.populateAvailableRanges();
        this.resetSelectOptions();
    }

    public updateData(): void {
        if (this.data.length < 1) {
            this.vAxisLeftValue = 0;
            this.vAxisRightValue = 0;
            this.formattedData = [];
            return;
        }

        this.maxValueRightAxis = Math.max(...this.data.map((o: ChartPeriodicDataModel) => o.amount));
        let isShortened: boolean = this.selectedDataGrouping.value == ChartSamplePeriodLength.Month;
        this.formattedData = this.data.map((item: ChartPeriodicDataModel) => [
            isShortened
                ? DateHelper.getMMMYYYYfromFullMonthYear(item.period)
                : item.period,
            item.count,
            this.createToolTip(this.vAxesLeftTitle, item.count, item.period, '', true),
            item.amount,
            this.createToolTip(this.vAxesRightTitle, item.amount, item.period, this.unit.prefixSymbol, false),
        ]);
        this.vAxisLeftValue = this.dFormatter(this.data
            .reduce((a: any, c: ChartPeriodicDataModel) => a + c.count, 0));
        const totalAmount: number = this.data
            .reduce((a: number, c: ChartPeriodicDataModel) => a + c.amount, 0);
        const formattedAmount: number = Number(totalAmount.toFixed(2));
        this.vAxisRightValue = this.unit.type === 'time'
            ? formattedAmount + `${this.unit.postSymbol}`
            : this.dFormatter(formattedAmount);
    }

    public userDidChangeDataGrouping(event: any): void {
        this.selectedDataGrouping = event.detail.value;
        this.updateRangeOptions();
        setTimeout(() => {
            // added a delay as change does not reflect in UI when set.
            this.selectedRange = this.ranges[0];
        }, 10);
    }

    public userDidChangeRange(event: any): void {
        this.selectedRange = event.detail.value;
        if (this.selectedRange === undefined) {
            return;
        }
        this.requestChartUpdate();
    }

    public dFormatter(num: number): any {
        let formatted: any = this.getFormattedValue(num, this.unit.prefixSymbol);
        let shortenedAmount: number = Number(formatted.shortenedAmount);
        if (shortenedAmount == num) {
            return num;
        }
        if (shortenedAmount > 99) {
            return shortenedAmount.toFixed(0) + formatted.symbol;
        } else if (shortenedAmount > 9) {
            return shortenedAmount.toFixed(1) + formatted.symbol;
        }
        return shortenedAmount.toFixed(2) + formatted.symbol;
    }

    public drawChart = (): void => {
        if (!this.target || !this.chartColorReference) {
            // loader currently running, skip drawing for now...            
            setTimeout(() => {
                this.drawChart();
            }, 250);
            return;
        }

        let data: any = new google.visualization.DataTable();
        data.addColumn('string', 'Period');
        data.addColumn('number', this.vAxesLeftTitle);
        data.addColumn({ type: 'string', role: 'tooltip', p: { html: true } });
        data.addColumn('number', this.vAxesRightTitle);
        data.addColumn({ type: 'string', role: 'tooltip', p: { html: true } });
        this.chartColors = this.getBackgroundColors(this.backGroundColors);
        if (this.data.length > 0) {
            data.addRows(this.formattedData);
        }

        let options: any = {
            bar: { groupWidth: "95%" },
            legend: { position: "none" },
            series: {
                0: { targetAxisIndex: 0 },
                1: { targetAxisIndex: 1 },
            },
            colors: this.chartColors,
            vAxes: {
                0: {
                    viewWindowMode: 'explicit',
                    viewWindow: { min: 0 },
                    gridlines: { color: 'gray' },
                },
                1: {
                    format: this.unit.type == 'time'
                        ? 'decimal'
                        : 'short',
                    viewWindowMode: 'explicit',
                    viewWindow: { min: 0 },
                    ticks: this.getTicksForVerticalRightAxis(
                        this.maxValueRightAxis,
                        this.unit.prefixSymbol),
                    gridlines: { color: 'transparent' },
                },
            },
            hAxes: {
                0:{
                    slantedText: false,
                },
            },
            tooltip: {
                isHtml: true,
            },
        };

        // for empty data, we use axis range of 0 to 1 to show zero values
        if (this.vAxisLeftValue == 0 && this.vAxisRightValue == 0) {
            for (let i: number = 0; i <= 1; i++) {
                options.vAxes[i].viewWindow = { min: 0, max: 1 };
            }
            options.vAxes[0].gridlines = { color: 'gray', count: 4 };
        }

        if (this.unit.type === 'currency') {
            let formatter: any = new google.visualization.NumberFormat({
                prefix: this.unit.prefixSymbol,
                fractionDigits: 2,
            });
            formatter.format(data, 2);
        }
        if (window.charts[this.title] === undefined || window.charts[this.title] === null) {
            window.charts[this.title] = new google.visualization.ColumnChart(this.target.nativeElement);
        } else {
            window.charts[this.title].clearChart();
            window.charts[this.title] = new google.visualization.ColumnChart(this.target.nativeElement);
        }
        window.charts[this.title].draw(data, options);
        if (this.currencyElement) {
            this.currencyElement.nativeElement.style.setProperty("color", this.textColors[1], "important");
        }
        this.canReDraw = true;
    };

    @HostListener('window:resize', ['$event'])
    public onResize(event: any): void {
        try {
            this.drawChart();
        } catch {
        }
    }

    private requestChartUpdate(): void {
        const eventData: ChartPeriodSelectionModel =
        {
            dataGrouping: this.selectedDataGrouping.value,
            range: this.selectedRange,
        };
        this.selectedPeriodChangeEvent.emit(eventData);
        this.vAxisLeftValue = 0;
        this.vAxisRightValue = 0;
        this.isLoading = true;
    }

    private resetSelectOptions(): void {
        this.selectedDataGrouping = this.dataGroupings[1];
        this.selectedRange = '3 months';
        this.updateRangeOptions();
    }

    private updateRangeOptions(): void {
        this.ranges = [];
        this.ranges.push(...this.availableRanges.get(this.selectedDataGrouping.label));
    }

    private populateAvailableRanges(): void {
        let currentMoment: moment.Moment = moment(new Date());
        let currentYear: string = currentMoment.format('YYYY');
        let currentMonth: number = parseInt(currentMoment.format('M'), 10) - 1;
        let yearValues: Array<string> = ['2 years', '3 years', '5 years', '10 years', 'All'];
        this.availableRanges.set(this.dataGroupings[0].label, this.getDailyValues(currentYear, currentMonth));
        this.availableRanges.set(this.dataGroupings[1].label, this.getMonthlyValues(currentYear, currentMonth));
        this.availableRanges.set(this.dataGroupings[2].label, this.getQuarterlyValues(currentYear, currentMonth));
        this.availableRanges.set(this.dataGroupings[3].label, yearValues);
    }

    private getDailyValues(currentYear: string, currentMonth: number): Array<string> {
        let dailyValues: Array<string> = [];
        for (let week: number = 1; week <= 4; week++) {
            dailyValues.push(week + (week == 1 ? ' week' : ' weeks'));
        }

        for (let month: number = 0; month < 12; month++) {
            let monthValue: string = moment(`${currentYear}-01-01`)
                .month(currentMonth - month)
                .format('MMM YYYY');
            dailyValues.push(monthValue);
        }
        return dailyValues;
    }

    private getMonthlyValues(currentYear: string, currentMonth: number): Array<string> {
        let monthValues: Array<string> = [];
        let months: Array<string> = ['3 months', '6 months', '12 months', '24 months'];
        monthValues.push(...months);
        let yearsToInclude: number = 5;
        if (currentMonth >= 2) {
            monthValues.push(currentYear);
            yearsToInclude -= 1;
        }
        for (let year: number = 1; year <= yearsToInclude; year++) {
            let yearValue: string = (parseInt(currentYear, 10) - year).toString();
            monthValues.push(yearValue);
        }
        return monthValues;
    }

    private getQuarterlyValues(currentYear: string, currentMonth: number):
        Array<string> {
        let quarterlyValues: Array<string> = [];
        let years: Array<string> = ['1 year', '2 years', '3 years', '5 years'];
        quarterlyValues.push(...years);
        let yearsToInclude: number = 5;
        if (currentMonth >= 6) {
            quarterlyValues.push(currentYear);
            yearsToInclude -= 1;
        }
        for (let year: number = 1; year <= yearsToInclude; year++) {
            let yearValue: string = (parseInt(currentYear, 10) - year).toString();
            quarterlyValues.push(yearValue);
        }
        return quarterlyValues;
    }
}
