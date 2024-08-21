import { Component, HostListener, Input, Output, EventEmitter } from '@angular/core';
import { loaderAnimation } from '@assets/animations';
import { trigger, state, transition, animate, style } from '@angular/animations';
import { BaseChartComponent } from '../base-chart.component';
import 'moment-timezone';
import { EventService } from '@app/services/event.service';
import { ChartColorModel } from '@app/models/chart-color.model';
import { ChartUnitModel } from '@app/models/chart-unit.model';
import { ChartPeriodicDataModel } from '@app/models/chart-periodic-data-model';
import { ChartDaySelectionModel } from '@app/models/chart-day-selection-model';
import { ChartOptionsModel } from '@app/models/chart-options-model';

declare const google: any;
declare const window: any;
const periodInDays: number = 7;

/**
 * Export line chart components class
 * This class component is all the line chart functions
 * Drawing of charts, formatter and period changes.
 */
@Component({
    selector: 'app-line-chart',
    templateUrl: './line-chart.component.html',
    styleUrls: ['./line-chart.component.scss'],
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
export class LineChartComponent extends BaseChartComponent {

    @Input()
    public data: Array<ChartPeriodicDataModel> = [];
    @Input()
    public vAxesLeftTitle: string = 'Number Generated';
    @Input()
    public vAxesRightTitle: string = 'Total Premium';
    @Input()
    public backGroundColors: Array<ChartColorModel> = [
        {
            default: "#00897b",
            override: "#00897b",
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
            default: "#00897b",
            override: "#00897b",
        };
    @Input()
    public unit: ChartUnitModel =
        {
            type: 'currency',
            postSymbol: '',
            prefixSymbol: '$',
        };
    @Output()
    public selectedPeriodChangeEvent: EventEmitter<ChartDaySelectionModel>
        = new EventEmitter<ChartDaySelectionModel>();
    public ranges: Array<ChartOptionsModel> = [
        { label: 'Last 30 days', value: 30 },
        { label: 'Last 60 days', value: 60 },
        { label: 'Last 90 days', value: 90 },
        { label: 'Last 6 months', value: 180 },
        { label: 'Last 12 months', value: 360 },
        { label: 'Last 24 months', value: 720 },
    ];
    public selectedRange: ChartOptionsModel = this.ranges[0];
    public formattedData: Array<any> = [];
    private chartColors: Array<string> = ["#00897b", "#26a69a"];
    private maxValueRightAxis: number = 0;
    private maxValueLeftAxis: number = 0;
    private perDayVAxesLeftTitle: string = '';
    private perDayVAxesRightTitle: string = '';

    public constructor(
        private eventService: EventService,
    ) {
        super();
    }

    public init(): void {
        if (this.title === '') {
            this.title = 'Line Trend';
        }

        this.eventService.dashboardProductFilterChangedSubject$.subscribe(() => {
            this.requestChartUpdate();
        });
        this.perDayVAxesLeftTitle = this.vAxesLeftTitle + ' Per Day';
        this.perDayVAxesRightTitle = this.vAxesRightTitle + ' Per Day';
        this.updateData();
    }

    public updateData(): void {
        if (this.data.length < 1) {
            this.formattedData = [];
            return;
        }
        this.maxValueRightAxis = Math.max(...this.data.map((o: ChartPeriodicDataModel) => o.amount / periodInDays));
        this.maxValueLeftAxis = Math.max(...this.data.map((o: ChartPeriodicDataModel) => o.count / periodInDays));
        this.formattedData = this.data.map((item: ChartPeriodicDataModel) =>
            [
                item.period,
                item.count / periodInDays,
                this.createToolTip(this.perDayVAxesLeftTitle, item.count / periodInDays, item.period, '', false),
                // if amount is negative, we set it to -0.01 to reduce Bézier curve
                // effect of google line charts.
                item.amount < 0 ? -0.01 : item.amount / periodInDays,
                this.createToolTip(
                    this.perDayVAxesRightTitle,
                    item.amount / periodInDays,
                    item.period,
                    this.unit.prefixSymbol,
                    false),
            ]);
    }

    public userDidChangeRange(event: any): void {
        this.selectedRange = event.detail.value;
        this.requestChartUpdate();
    }

    public drawChart = (): void => {
        if (!this.target || !this.chartColorReference) {
            // loader currently running, skip drawing for now...
            setTimeout(() => {
                this.drawChart();
            }, 100);
            return;
        }

        let data: any = new google.visualization.DataTable();
        data.addColumn('string', 'Period');
        data.addColumn('number', this.perDayVAxesLeftTitle);
        data.addColumn({ type: 'string', role: 'tooltip', p: { html: true } });
        data.addColumn('number', this.perDayVAxesRightTitle);
        data.addColumn({ type: 'string', role: 'tooltip', p: { html: true } });
        data.addRows(this.formattedData);
        this.chartColors = this.getBackgroundColors(this.backGroundColors);
        let options: any = {
            curveType: 'function',
            legend: { position: 'none' },
            series: {
                0: { targetAxisIndex: 0 },
                1: { targetAxisIndex: 1 },
            },
            vAxes: {
                0: {
                    title: this.perDayVAxesLeftTitle,
                    format: this.unit.type == 'time'
                        ? 'decimal'
                        : 'short',
                    viewWindowMode: 'explicit',
                    viewWindow: { min: 0 },
                    gridlines: { color: 'gray' },
                },
                1: {
                    title: this.perDayVAxesRightTitle,
                    format: this.unit.type == 'time'
                        ? 'decimal'
                        : 'short',
                    ticks: this.getTicksForVerticalRightAxis(
                        this.maxValueRightAxis,
                        this.unit.prefixSymbol),
                    viewWindowMode: 'explicit',
                    viewWindow: { min: 0 },
                    gridlines: { color: 'transparent' },
                },
            },
            colors: this.chartColors,
            hAxis: { textPosition: 'none' },
            chartArea: {
                height: 175,
            },
            tooltip: {
                isHtml: true,
            },
        };

        // for empty data, we use axis range of -0.1 to 1 to show zero values
        if (this.maxValueLeftAxis == 0) {
            for (let i: number = 0; i <= 1; i++) {
                options.vAxes[i].viewWindow = { min: -0.01, max: 1 };
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
            window.charts[this.title] = new google.visualization.LineChart(this.target.nativeElement);
        } else {
            window.charts[this.title].clearChart();
            window.charts[this.title] = new google.visualization.LineChart(this.target.nativeElement);
        }

        window.charts[this.title].draw(data, options);
        this.canReDraw = true;
    };

    @HostListener('window:resize', ['$event'])
    public onResize(event: any): void {
        try {
            this.drawChart();
        } catch { }
    }

    private requestChartUpdate(): void {
        this.selectedPeriodChangeEvent
            .emit({ valueInDays: this.selectedRange.value });
        this.isLoading = true;
    }
}
