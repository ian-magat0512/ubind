import { Component, EventEmitter, HostListener, Input, Output } from '@angular/core';
import { loaderAnimation } from '@assets/animations';
import { trigger, state, transition, animate, style } from '@angular/animations';
import { BaseChartComponent } from '../base-chart.component';
import 'moment-timezone';
import { EventService } from '@app/services/event.service';
import { ChartColorModel } from '@app/models/chart-color.model';
import { ChartCategorizedDataModel } from '@app/models/chart-categorized-data-model';
import { ChartDaySelectionModel } from '@app/models/chart-day-selection-model';
import { ChartOptionsModel } from '@app/models/chart-options-model';
import { ChartDataCategory } from '@app/models/chart-data-category.enum';
declare const google: any;
declare const window: any;

/**
 * Export Donut chart component class
 * This class component is all the donut chart functions
 * Drawing of charts, formatter and period changes.
 */
@Component({
    selector: 'app-donut-chart',
    templateUrl: './donut-chart.component.html',
    styleUrls: ['./donut-chart.component.scss'],
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
export class DonutChartComponent extends BaseChartComponent {

    @Input()
    public data: Array<ChartCategorizedDataModel> = [];
    @Input()
    public completedTitle: string = 'Claims Paid';
    @Input()
    public incompleteTitle: string = 'Claims Not Paid';
    @Input()
    public backGroundColors: Array<ChartColorModel> = [
        {
            default: "#673ab7",
            override: "#673ab7",
        },
        {
            default: "#9575cd",
            override: "#9575cd",
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
            default: "#673ab7",
            override: "#673ab7",
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
    public completed: number = 0;
    public incomplete: number = 0;
    public formattedData: Array<any> = [];
    public hasData: boolean = false;
    private chartColors: Array<string> = ["#673ab7", "#9575cd"];

    public constructor(
        private eventService: EventService,
    ) {
        super();
    }

    public init(): void {
        if (this.title === '') {
            this.title = 'Donut Paid Ratio';
        }

        this.eventService.dashboardProductFilterChangedSubject$.subscribe(() => {
            this.requestChartUpdate();
        });
        this.updateData();
    }

    public updateData(): void {
        if (this.data.length < 1) {
            this.completed = 0;
            this.incomplete = 0;
            this.formattedData = [];
            this.hasData = false;
            return;
        }
        this.formattedData = [];
        this.completed = this.data.filter(
            (d: ChartCategorizedDataModel) => d.category == ChartDataCategory.Completed)[0].value;
        this.incomplete = this.data.filter(
            (d: ChartCategorizedDataModel) => d.category == ChartDataCategory.Incomplete)[0].value;
        const total: number = this.completed + this.incomplete;
        this.formattedData.push([
            this.completedTitle,
            this.completed,
            this.createToolTipForDonut(
                this.completedTitle, this.completed, this.completed*100/total),
        ]);
        this.formattedData.push([
            this.incompleteTitle,
            this.incomplete,
            this.createToolTipForDonut(
                this.incompleteTitle, this.incomplete, this.incomplete*100/total),
        ]);
        this.hasData = this.completed != 0 || this.incomplete != 0;
        if (this.selectedRange['value'] === 0) {
            this.drawChart();
            return;
        }
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
        data.addColumn('string', 'Type');
        data.addColumn('number', 'CountType');
        data.addColumn({ type: 'string', role: 'tooltip', p: { html: true } });
        data.addRows(this.formattedData);
        this.chartColors = this.getBackgroundColors(this.backGroundColors);
        let options: any = {
            pieHole: .4,
            colors: this.chartColors,
            legend: { position: "none" },
            chartArea: {
                height: '75%',
                width: '75%',
            },
            tooltip: {
                isHtml: true,
            },
        };

        if (window.charts[this.title] === undefined || window.charts[this.title] === null) {
            window.charts[this.title] = new google.visualization.PieChart(this.target.nativeElement);
        } else {
            window.charts[this.title].clearChart();
            window.charts[this.title] = new google.visualization.PieChart(this.target.nativeElement);
        }

        window.charts[this.title].draw(data, options);
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
        this.selectedPeriodChangeEvent
            .emit({ valueInDays: this.selectedRange.value });
        this.completed = 0;
        this.incomplete = 0;
        this.isLoading = true;
    }

    private createToolTipForDonut(
        title: string,
        amount: number,
        percentage: number): string {
        const formattedValue: string = amount.toLocaleString(undefined, { style: 'decimal' });
        const percentageFormatted: string = percentage.toFixed(1);
        return `<div class="chart-tooltip" style="min-width: 150px;">${title}
        </br><b>${formattedValue} (${percentageFormatted}%)</b></div>`;
    }
}
