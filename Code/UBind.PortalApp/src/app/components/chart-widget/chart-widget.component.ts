import {
    Component, OnInit, Input, OnChanges, OnDestroy, HostListener,
} from '@angular/core';
import { ClaimPeriodicSummaryModel } from '@app/resource-models/claim.resource-model';
import { PolicyTransactionPeriodicSummaryModel } from '@app/resource-models/policy.resource-model';
import { QuotePeriodicSummaryModel } from '@app/resource-models/quote.resource-model';
import { Permission } from '@app/helpers/permissions.helper';
import { scrollbarStyle } from '@assets/scrollbar';
import { ProductSelection } from '@app/models/product-selection';
import { AuthenticationService } from '@app/services/authentication.service';
import { QuoteApiService } from '@app/services/api/quote-api.service';
import { PolicyTransactionApiService } from '@app/services/api/policy-transaction-api.service';
import { ClaimApiService } from '@app/services/api/claim-api.service';
import * as moment from 'moment';
import 'moment-timezone';
import { takeUntil } from 'rxjs/operators';
import { Subject } from 'rxjs';
import { EventService } from '@app/services/event.service';
import { QuoteType } from '@app/models/quote-type.enum';
import { ChartCategorizedDataModel } from '@app/models/chart-categorized-data-model';
import { ChartPeriodicDataModel } from '@app/models/chart-periodic-data-model';
import { ChartDaySelectionModel } from '@app/models/chart-day-selection-model';
import { ChartPeriodSelectionModel } from '@app/models/chart-period-selection-model';
import { ChartSamplePeriodLength } from '@app/models/chart-sample-period-length.enum';
import { ChartDataCategory } from '@app/models/chart-data-category.enum';
import { StartupJobApiService } from '@app/services/api/startup-job-api.service';
import { StartupJobStatus } from '@app/models/startupjob-status.enum';

const quotesPeriodic: string = 'Quotes Periodic';
const quotesTrend: string = 'Quotes Trend';
const quotesConversionRate: string = 'Quotes Conversion Rate';
const policiesPeriodic: string = 'Policies Periodic';
const policiesTrend: string = 'Policies Trend';
const protectionPeriodic: string = 'Protections Periodic';
const protectionTrend: string = 'Protections Trend';
const claimsProcessedPeriodic: string = 'Claims Processed Periodic';
const claimsProcessedTrend: string = 'Claims Processed Trend';
const claimsPaidPeriodic: string = 'Claims Paid Periodic';
const claimsPaidTrend: string = 'Claims Paid  Trend';
const claimsPaidRatio: string = 'Claims Paid Ratio';
const quoteColorDark: string = '#00695c';
const quoteTitleColor: string = '#00695c';
const quoteColorLight: string = '#26a69a';
const policiesColorDark: string = '#c2185b';
const policiesTitleColor: string = '#c2185b';
const policiesColorLight: string = '#f06292';
const claimsColorDark: string = '#7b1fa2';
const claimsTitleColor: string = '#7b1fa2';
const claimsColorLight: string = '#ba68c8';
const columnChart: string = 'column';
const lineChart: string = 'line';
const donutChart: string = 'donut';
const defaultTextColor: string = "#ffffff";
const customPeriodInMinutes: number = 7 * 24 * 60;

/**
 * Export chart widget component class
 * This chart widget components is to
 * display the quote, policy and claim charts.
 */
@Component({
    selector: 'app-chart-widget',
    templateUrl: './chart-widget.component.html',
    styleUrls: ['./chart-widget.component.scss'],
    styles: [
        scrollbarStyle,
    ],
})
export class ChartWidgetComponent implements OnInit, OnDestroy, OnChanges {

    @Input()
    public quoteManagementEnabled: boolean = false;

    @Input()
    public policyManagementEnabled: boolean = false;

    @Input()
    public claimManagementEnabled: boolean = false;

    @Input()
    public productFilter: Array<ProductSelection> = new Array<ProductSelection>();

    public destroyed: Subject<void>;
    public windowWidth: number;
    public isDashboardUpgradeComplete: boolean = true;

    public quotesManagementCharts: any = [
        {
            type: columnChart,
            title: quotesPeriodic,
            vAxesLeftTitle: 'Number Created',
            vAxesRightTitle: 'Total Premium',
            backGroundColors: [
                {
                    default: quoteColorDark,
                    override: `var(--quotes-periodic-chart-dark-background-color,${quoteColorDark})`,
                },
                {
                    default: quoteColorLight,
                    override: `var(--quotes-periodic-chart-light-background-color,${quoteColorLight})`,
                }],
            textColors: [
                {
                    default: defaultTextColor,
                    override: `var(--quotes-periodic-chart-dark-text-color,${defaultTextColor})`,
                },
                {
                    default: defaultTextColor,
                    override: `var(--quotes-periodic-chart-light-text-color,${defaultTextColor})`,
                }],
            titleColor: {
                default: quoteTitleColor,
                override: `var(--quotes-periodic-chart-title-color,${quoteTitleColor})`,
            },
            unit: {
                type: 'currency',
                postSymbol: '',
                prefixSymbol: '$',
            },
            data: [],
        },
        {
            type: lineChart,
            title: quotesTrend,
            vAxesLeftTitle: 'Number Created',
            vAxesRightTitle: 'Total Premium',
            backGroundColors: [
                {
                    default: quoteColorDark,
                    override: `var(--quotes-trend-chart-dark-background-color,${quoteColorDark})`,
                },
                {
                    default: quoteColorLight,
                    override: `var(--quotes-trend-chart-light-background-color,${quoteColorLight})`,
                }],
            textColors: [
                {
                    default: defaultTextColor,
                    override: `var(--quotes-trend-chart-dark-text-color,${defaultTextColor})`,
                },
                {
                    default: defaultTextColor,
                    override: `var(--quotes-trend-chart-light-text-color,${defaultTextColor})`,
                }],
            titleColor: {
                default: quoteTitleColor,
                override: `var(--quotes-trend-chart-title-color,${quoteTitleColor})`,
            },
            unit: {
                type: 'currency',
                postSymbol: '',
                prefixSymbol: '$',
            },
            data: [],
        },
        {
            type: donutChart,
            title: quotesConversionRate,
            completedTitle: 'Converted Quotes',
            incompleteTitle: 'Abandoned Quotes',
            backGroundColors: [
                {
                    default: quoteColorDark,
                    override: `var(--quotes-conversion-rate-chart-dark-background-color,${quoteColorDark})`,
                },
                {
                    default: quoteColorLight,
                    override: `var(--quotes-conversion-rate-chart-light-background-color,${quoteColorLight})`,
                }],
            textColors: [
                {
                    default: defaultTextColor,
                    override: `var(--quotes-conversion-rate-chart-dark-text-color,${defaultTextColor})`,
                },
                {
                    default: defaultTextColor,
                    override: `var(--quotes-conversion-rate-chart-light-text-color,${defaultTextColor})`,
                }],
            titleColor: {
                default: quoteTitleColor,
                override: `var(--quotes-conversion-chart-title-color,${quoteTitleColor})`,
            },
            data: [],
        },
    ];

    public policiesManagementCharts: any = [
        {
            type: columnChart,
            title: this.getPoliciesPeriodicTitle(),
            vAxesLeftTitle: 'Number Bound',
            vAxesRightTitle: 'Total Premium',
            backGroundColors: [
                {
                    default: policiesColorDark,
                    override: `var(--policy-periodic-chart-dark-background-color,${policiesColorDark})`,
                },
                {
                    default: policiesColorLight,
                    override: `var(--policy-periodic-chart-light-background-color,${policiesColorLight})`,
                }],
            textColors: [
                {
                    default: defaultTextColor,
                    override: `var(--policy-periodic-chart-dark-text-color,${defaultTextColor})`,
                },
                {
                    default: defaultTextColor,
                    override: `var(--policy-periodic-chart-light-text-color,${defaultTextColor})`,
                }],
            titleColor: {
                default: policiesTitleColor,
                override: `var(--policy-periodic-chart-title-color,${policiesTitleColor})`,
            },
            unit: {
                type: 'currency',
                postSymbol: '',
                prefixSymbol: '$',
            },
            data: [],
        },
        {
            type: lineChart,
            title: this.getPoliciesTrendTitle(),
            vAxesLeftTitle: 'Number Bound',
            vAxesRightTitle: 'Total Premium',
            backGroundColors: [
                {
                    default: policiesColorDark,
                    override: `var(--policy-trend-chart-dark-background-color,${policiesColorDark})`,
                },
                {
                    default: policiesColorLight,
                    override: `var(--policy-trend-chart-light-background-color,${policiesColorLight})`,
                }],
            textColors: [
                {
                    default: defaultTextColor,
                    override: `var(--policy-trend-chart-dark-text-color,${defaultTextColor})`,
                },
                {
                    default: defaultTextColor,
                    override: `var(--policy-trend-chart-light-text-color,${defaultTextColor})`,
                }],
            titleColor: {
                default: policiesTitleColor,
                override: `var(--policy-trend-chart-title-color,${policiesTitleColor})`,
            },
            unit: {
                type: 'currency',
                postSymbol: '',
                prefixSymbol: '$',
            },
            data: [],
        },
    ];

    public claimsManagementCharts: any = [
        {
            type: columnChart,
            title: claimsProcessedPeriodic,
            vAxesLeftTitle: 'Number Processed',
            vAxesRightTitle: 'Average Processing Time',
            backGroundColors: [
                {
                    default: claimsColorDark,
                    override: `var(--claims-processed-periodic-chart-dark-background-color,${claimsColorDark})`,
                },
                {
                    default: claimsColorLight,
                    override: `var(--claims-processed-periodic-chart-light-background-color,${claimsColorLight})`,
                }],
            textColors: [
                {
                    default: defaultTextColor,
                    override: `var(--claims-processed-periodic-chart-dark-text-color,${defaultTextColor})`,
                },
                {
                    default: defaultTextColor,
                    override: `var(--claims-processed-periodic-chart-light-text-color,${defaultTextColor})`,
                }],
            titleColor: {
                default: claimsTitleColor,
                override: `var(--claims-processed-periodic-chart-title-color,${claimsTitleColor})`,
            },
            unit: {
                type: 'time',
                postSymbol: 'd',
                prefixSymbol: '',
            },
            data: [],
        },
        {
            type: lineChart,
            title: claimsProcessedTrend,
            vAxesLeftTitle: 'Number Processed',
            vAxesRightTitle: 'Average Processing Time',
            backGroundColors: [
                {
                    default: claimsColorDark,
                    override: `var(--claims-processed-trend-chart-dark-background-color,${claimsColorDark})`,
                },
                {
                    default: claimsColorLight,
                    override: `var(--claims-processed-trend-chart-light-background-color,${claimsColorLight})`,
                }],
            textColors: [
                {
                    default: defaultTextColor,
                    override: `var(--claims-processed-trend-chart-dark-text-color,${defaultTextColor})`,
                },
                {
                    default: defaultTextColor,
                    override: `$var(--claims-processed-trend-chart-light-text-color,${defaultTextColor})`,
                }],
            titleColor: {
                default: claimsTitleColor,
                override: `var(--claims-processed-trend-chart-title-color,${claimsTitleColor})`,
            },
            unit: {
                type: 'time',
                postSymbol: 'd',
                prefixSymbol: '',
            },
            data: [],
        },
        {
            type: columnChart,
            title: claimsPaidPeriodic,
            vAxesLeftTitle: 'Number Paid',
            vAxesRightTitle: 'Average Amount Paid',
            backGroundColors: [
                {
                    default: claimsColorDark,
                    override: `var(--claims-paid-periodic-trend-chart-dark-background-color,${claimsColorDark})`,
                },
                {
                    default: claimsColorLight,
                    override: `var(--claims-paid-periodic-trend-chart-light-background-color,${claimsColorLight})`,
                }],
            textColors: [
                {
                    default: defaultTextColor,
                    override: `var(--claims-paid-periodic-trend-chart-dark-text-color,${defaultTextColor})`,
                },
                {
                    default: defaultTextColor,
                    override: `var(--claims-paid-periodic-trend-chart-light-text-color,${defaultTextColor})`,
                }],
            titleColor: {
                default: claimsTitleColor,
                override: `var(--claims-paid-periodic-trend-chart-title-color,${claimsTitleColor})`,
            },
            unit: {
                type: 'currency',
                postSymbol: '',
                prefixSymbol: '$',
            },
            data: [],
        },
        {
            type: lineChart,
            title: claimsPaidTrend,
            vAxesLeftTitle: 'Number Paid',
            vAxesRightTitle: 'Average Amount Paid',
            backGroundColors: [
                {
                    default: claimsColorDark,
                    override: `var(--claims-paid-trend-chart-dark-background-color,${claimsColorDark})`,
                },
                {
                    default: claimsColorLight,
                    override: `var(--claims-paid-trend-chart-light-background-color,${claimsColorLight})`,
                }],
            textColors: [
                {
                    default: defaultTextColor,
                    override: `var(--claims-paid-trend-chart-dark-text-color,${defaultTextColor})`,
                },
                {
                    default: defaultTextColor,
                    override: `var(--claims-paid-trend-chart-light-text-color,${defaultTextColor})`,
                }],
            titleColor: {
                default: claimsTitleColor,
                override: `var(--claims-paid-trend-chart-title-color,${claimsTitleColor})`,
            },
            unit: {
                type: 'currency',
                postSymbol: '',
                prefixSymbol: '$',
            },
            data: [],
        },
        {
            type: donutChart,
            title: claimsPaidRatio,
            completedTitle: 'Claims Paid',
            incompleteTitle: 'Claims Not Paid',
            backGroundColors: [
                {
                    default: claimsColorDark,
                    override: `var(--claims-not-paid-ratio-chart-dark-background-color,${claimsColorDark})`,
                },
                {
                    default: claimsColorLight,
                    override: `var(--claims-not-paid-ratio-chart-light-background-color,${claimsColorLight})`,
                }],
            textColors: [
                {
                    default: defaultTextColor,
                    override: `var(--claims-not-paid-ratio-chart-dark-text-color,${defaultTextColor})`,
                },
                {
                    default: defaultTextColor,
                    override: `var(--claims-not-paid-ratio-chart-light-text-color,${defaultTextColor})`,
                }],
            titleColor: {
                default: claimsTitleColor,
                override: `var(--claims-not-paid-ratio-chart-title-color,${claimsTitleColor})`,
            },
            data: [],
        },
    ];

    private startupJobsForDashboard: Array<string> = [
        "SetTotalPayableOnExistingQuotes_20230608",
        "SetTotalPayableOnExistingPolicyTransaction_20230630",
        "SetClaimReadModelDates_20230711",
    ];

    public permission: typeof Permission = Permission;

    public constructor(
        private authService: AuthenticationService,
        private quoteApiService: QuoteApiService,
        private startupJobApiService: StartupJobApiService,
        private policyTransactionService: PolicyTransactionApiService,
        private claimApiService: ClaimApiService,
        private eventService: EventService,
    ) {
    }

    @HostListener('window:resize', ['$event'])
    public onResize(event: any): void {
        this.windowWidth = event.target.innerWidth;
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.policiesManagementCharts.forEach((item: any) => {
            item.vAxesRightTitle = this.checkReplaceTitlePremiumForMutual(item.vAxesRightTitle);
        });

        this.quotesManagementCharts.forEach((item: any) => {
            item.vAxesRightTitle = this.checkReplaceTitlePremiumForMutual(item.vAxesRightTitle);
        });
        this.windowWidth = window.innerWidth;
        this.loadDashboardStartupJobsStatus();
    }

    public ngOnDestroy(): void {
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    public ngOnChanges(simpleChanges: any): void {
        // only change if its due to filter change instead of initialization
        if (Object.prototype.hasOwnProperty.call(simpleChanges, 'productFilter')
            && !simpleChanges.productFilter.firstChange) {
            this.eventService.dashboardProductFilterChanged();
        }
    }

    public updateColumnChartData(
        eventData: ChartPeriodSelectionModel,
        resourceType: string,
        chartTitle: string): void {
        let selectedRange: string = eventData.range;
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        switch (eventData.dataGrouping) {
            case ChartSamplePeriodLength.Day:
                params.set('samplePeriodLength', eventData.dataGrouping.toString());
                if (selectedRange.indexOf('week') < 0) {
                    let monthValue: string = selectedRange.substring(0, selectedRange.indexOf(' '));
                    let yearValue: string = selectedRange.substring(selectedRange.indexOf(' ') + 1);
                    let fromDateTime: moment.Moment = moment(`${yearValue}-01-01`)
                        .month(monthValue)
                        .startOf('month');
                    let toDateTime: moment.Moment = moment(`${yearValue}-01-01`)
                        .month(monthValue)
                        .endOf('month')
                        .endOf('day');
                    this.setDateTimeParams(params, fromDateTime, toDateTime);
                } else {
                    let noOfWeeks: number =
                        parseInt(selectedRange.substring(0, selectedRange.indexOf(' ')), 10);
                    let fromDateTime: moment.Moment = moment(new Date())
                        .subtract(noOfWeeks * 7, 'days')
                        .startOf('day');
                    let toDateTime: moment.Moment = moment(new Date())
                        .endOf('day');
                    this.setDateTimeParams(params, fromDateTime, toDateTime);
                }
                break;
            case ChartSamplePeriodLength.Month:
                params.set('samplePeriodLength', eventData.dataGrouping.toString());
                if (selectedRange.indexOf('months') < 0) {
                    let yearValue: string = selectedRange;
                    let fromDateTime: moment.Moment = moment(`${yearValue}-01-01`)
                        .startOf('day');
                    let toDateTime: moment.Moment = moment(`${yearValue}-12-31`)
                        .endOf('day');
                    this.setDateTimeParams(params, fromDateTime, toDateTime);
                } else {
                    let noOfMonths: number =
                        parseInt(selectedRange.substring(0, selectedRange.indexOf(' ')), 10) - 1;
                    let fromDateTime: moment.Moment = moment(new Date())
                        .subtract(noOfMonths, 'months')
                        .startOf('month');
                    let toDateTime: moment.Moment = moment(new Date())
                        .endOf('month')
                        .endOf('day');
                    this.setDateTimeParams(params, fromDateTime, toDateTime);
                }
                break;
            case ChartSamplePeriodLength.Quarter:
                params.set('samplePeriodLength', eventData.dataGrouping.toString());
                if (selectedRange.indexOf('year') < 0) {
                    let yearValue: string = selectedRange;
                    let fromDateTime: moment.Moment = moment(`${yearValue}-01-01`)
                        .startOf('day');
                    let toDateTime: moment.Moment = moment(`${yearValue}-12-31`)
                        .endOf('day');
                    this.setDateTimeParams(params, fromDateTime, toDateTime);
                } else {
                    let noOfYears: number =
                        parseInt(selectedRange.substring(0, selectedRange.indexOf(' ')), 10);
                    let noOfQuarters: number = (noOfYears * 4) - 1;
                    let fromDateTime: moment.Moment = moment(new Date())
                        .startOf('quarter')
                        .subtract(noOfQuarters, 'quarters');
                    let toDateTime: moment.Moment = moment(new Date())
                        .endOf('quarter')
                        .endOf('day');
                    this.setDateTimeParams(params, fromDateTime, toDateTime);
                }
                break;
            case ChartSamplePeriodLength.Year:
                params.set('samplePeriodLength', eventData.dataGrouping.toString());
                if (selectedRange.indexOf('year') < 0) {
                    let toDateTime: moment.Moment = moment(new Date());
                    this.setDateTimeParams(params, null, toDateTime);
                } else {
                    let noOfYears: number =
                        parseInt(selectedRange.substring(0, selectedRange.indexOf(' ')), 10) - 1;
                    let fromDateTime: moment.Moment = moment(new Date())
                        .startOf('year')
                        .subtract(noOfYears, 'years');
                    let toDateTime: moment.Moment = moment(new Date())
                        .endOf('year')
                        .endOf('day');
                    this.setDateTimeParams(params, fromDateTime, toDateTime);
                }
                break;
            default:
                break;
        }
        this.updateChartData(resourceType, chartTitle, params);
    }

    public updateLineChartData(
        eventData: ChartDaySelectionModel,
        resourceType: string,
        chartTitle: string,
    ): void {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set("samplePeriodLength", ChartSamplePeriodLength.Custom.toString());
        params.set("customSamplePeriodMinutes", `${customPeriodInMinutes}`);
        let fromDateTime: moment.Moment = moment(new Date())
            .subtract(eventData.valueInDays, 'days');
        let toDateTime: moment.Moment = moment(new Date());
        this.setDateTimeParams(params, fromDateTime, toDateTime);
        this.updateChartData(resourceType, chartTitle, params);
    }

    public updateDonutChartData(
        eventData: ChartDaySelectionModel,
        resourceType: string,
        chartTitle: string,
    ): void {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set("samplePeriodLength", ChartSamplePeriodLength.All.toString());
        let totalDays: number = eventData.valueInDays;
        let fromDateTime: moment.Moment = moment(new Date())
            .subtract(totalDays, 'days');
        let toDateTime: moment.Moment = moment(new Date());
        this.setDateTimeParams(params, fromDateTime, toDateTime);
        this.updateChartData(resourceType, chartTitle, params);
    }

    public async updateChartData(
        resourceType: string = '',
        chartTitle: string = null,
        params: Map<string, string | Array<string>> = null): Promise<void> {
        params = this.getOptionalParams(params);
        if (resourceType == 'quote' && this.quoteManagementEnabled) {
            await this.updateQuoteData(params, chartTitle);
        } else if (resourceType == 'policy' && this.policyManagementEnabled) {
            await this.updatePolicyData(params, chartTitle);
        } else if (resourceType == 'claim' && this.claimManagementEnabled) {
            await this.updateClaimData(params, chartTitle);
        } else {
            let promises: Array<Promise<void>> = new Array<Promise<void>>();
            if (this.policyManagementEnabled) {
                let chartPromises: Array<Promise<void>> = this.policiesManagementCharts.map(
                    (chartData: any) => {
                        let requestParams: Map<string, string | Array<string>> =
                            this.getChartParamDefaults(params, chartData.type);
                        return this.updatePolicyData(requestParams, chartData.title);
                    });
                promises.push(...chartPromises);
            }

            if (this.quoteManagementEnabled) {
                let chartPromises: Array<Promise<void>> = this.quotesManagementCharts.map(
                    (chartData: any) => {
                        let requestParams: Map<string, string | Array<string>> =
                            this.getChartParamDefaults(params, chartData.type);
                        return this.updateQuoteData(requestParams, chartData.title);
                    });
                promises.push(...chartPromises);
            }

            if (this.claimManagementEnabled) {
                let chartPromises: Array<Promise<void>> = this.claimsManagementCharts.map(
                    (chartData: any) => {
                        let requestParams: Map<string, string | Array<string>> =
                            this.getChartParamDefaults(params, chartData.type);
                        return this.updateClaimData(requestParams, chartData.title);
                    });
                promises.push(...chartPromises);
            }

            Promise.all(promises);
        }
    }

    public getXLColumnWidth(): string {
        if (this.windowWidth >= 1200 && this.windowWidth <= 1270) {
            return "12";
        } else if (this.windowWidth >= 1271 && this.windowWidth <= 1500) {
            return "6";
        } else {
            return "4";
        }
    }

    public getLGColumnWidth(): string {
        if (this.windowWidth >= 991 && this.windowWidth <= 1199) {
            return "12";
        } else {
            return "6";
        }
    }

    private getChartParamDefaults(
        params: Map<string, string | Array<string>>,
        chartType: string = null): Map<string, string | Array<string>> {
        let defaultParams: Map<string, string | Array<string>> =
            new Map<string, string | Array<string>>();
        params.forEach(
            (value: string | Array<string>, key: string) => {
                defaultParams.set(key, value);
            });
        defaultParams.delete("customSamplePeriodMinutes");
        defaultParams.delete("policyTransactionType");
        let fromDateTime: moment.Moment;
        let toDateTime: moment.Moment;
        switch (chartType) {
            case columnChart:
                fromDateTime = moment(new Date()).subtract(2, 'months').startOf('month');
                toDateTime = moment(new Date()).endOf('month').endOf('day');
                defaultParams.set('samplePeriodLength', ChartSamplePeriodLength.Month.toString());
                break;
            case lineChart:
                fromDateTime = moment(new Date()).subtract(30, 'days');
                toDateTime = moment(new Date());
                defaultParams.set("samplePeriodLength", ChartSamplePeriodLength.Custom.toString());
                defaultParams.set("customSamplePeriodMinutes", `${customPeriodInMinutes}`);
                break;
            case donutChart:
                fromDateTime = moment(new Date()).subtract(30, 'days');
                toDateTime = moment(new Date());
                defaultParams.set('samplePeriodLength', ChartSamplePeriodLength.All.toString());
                break;
            default:
                break;
        }
        this.setDateTimeParams(defaultParams, fromDateTime, toDateTime);
        return defaultParams;
    }

    private getOptionalParams(
        params: Map<string, string | Array<string>> = null): Map<string, string | Array<string>> {
        if (!params) {
            params = new Map<string, string | Array<string>>();
        }
        const selectedProducts: Array<string> = this.productFilter.filter((d: ProductSelection) =>
            d.isChecked === true).map((d: ProductSelection) => d.alias);
        if (selectedProducts.length > 0 && this.productFilter.length != selectedProducts.length) {
            params.set('product', selectedProducts);
        }
        let timezoneId: string = moment.tz.guess();
        if (timezoneId !== undefined) {
            params.set('timezone', timezoneId);
        }
        return params;
    }

    private setDateTimeParams(
        params: Map<string, string | Array<string>>,
        fromDateTime: moment.Moment | null,
        toDateTime: moment.Moment): void {
        if (fromDateTime) {
            params.set("fromDateTime", this.getIsoDateTimeString(fromDateTime));
        }
        params.set("toDateTime", this.getIsoDateTimeString(toDateTime, true));
    }

    private getIsoDateTimeString(dateTime: moment.Moment, isLastNanosecond: boolean = false): string {
        // since moment does NOT support beyond milliseconds we need to add 0000 or 9999 to the end of the date time.
        return dateTime.format('YYYY-MM-DDTHH:mm:ss.SSS')
            + (isLastNanosecond ? '9999' : '0000');
    }

    private setIncludePropertyParams(
        params: Map<string, string | Array<string>>,
        chartTitle: string = null): void {
        switch (chartTitle) {
            case quotesConversionRate:
                params.set('includeProperty', ['convertedCount', 'abandonedCount']);
                break;
            case claimsProcessedPeriodic:
            case claimsProcessedTrend:
                params.set('includeProperty', ['processedCount', 'averageProcessingTime']);
                break;
            case claimsPaidPeriodic:
            case claimsPaidTrend:
                params.set('includeProperty', ['settledCount', 'averageSettlementAmount']);
                break;
            case claimsPaidRatio:
                params.set('includeProperty', ['settledCount', 'declinedCount']);
                break;
            default:
                params.set('includeProperty', ['createdCount', 'createdTotalPremium']);
                break;
        }
    }

    private async updateQuoteData(
        params: Map<string, string | Array<string>>,
        chartTitle: string = null): Promise<void> {
        this.setIncludePropertyParams(params, chartTitle);
        params.set("policyTransactionType", new Array(QuoteType[QuoteType.NewBusiness], QuoteType[QuoteType.Renewal]));
        await this.quoteApiService.getPeriodicSummaries(params)
            .pipe(takeUntil(this.destroyed))
            .toPromise()
            .then((quoteData: QuotePeriodicSummaryModel | Array<QuotePeriodicSummaryModel>) => {
                if (!quoteData) {
                    return;
                }
                if (chartTitle) {
                    let chartData: any;
                    if (chartTitle === quotesPeriodic || chartTitle === quotesTrend) {
                        chartData = this.mapQuotesToPeriodicChart(quoteData as Array<QuotePeriodicSummaryModel>);
                    } else {
                        chartData = this.mapQuotesToConversionChart(quoteData as QuotePeriodicSummaryModel);
                    }
                    let chartIndex: number = this.quotesManagementCharts
                        .findIndex((x: any) => x.title == chartTitle);
                    this.quotesManagementCharts[chartIndex].data = chartData;
                }
            });
    }

    private async updatePolicyData(
        params: Map<string, string | Array<string>>,
        chartTitle: string = null): Promise<void> {
        this.setIncludePropertyParams(params, chartTitle);
        params.set("policyTransactionType", new Array(QuoteType[QuoteType.NewBusiness], QuoteType[QuoteType.Renewal]));
        await this.policyTransactionService.getPeriodicSummaries(params)
            .pipe(takeUntil(this.destroyed))
            .toPromise()
            .then(
                (policyTransactions:
                    PolicyTransactionPeriodicSummaryModel | Array<PolicyTransactionPeriodicSummaryModel>) => {
                    if (!policyTransactions) {
                        return;
                    }
                    if (chartTitle) {
                        let index: number = this.policiesManagementCharts.findIndex((x: any) => x.title == chartTitle);
                        this.policiesManagementCharts[index].data =
                            this.mapPolicyTransactionsToPeriodicChart(
                                policyTransactions as Array<PolicyTransactionPeriodicSummaryModel>);
                    }
                });
    }

    private async updateClaimData(
        params: Map<string, string | Array<string>>,
        chartTitle: string = null): Promise<void> {
        this.setIncludePropertyParams(params, chartTitle);
        await this.claimApiService.getPeriodicSummaries(params)
            .pipe(takeUntil(this.destroyed))
            .toPromise()
            .then((claims: ClaimPeriodicSummaryModel | Array<ClaimPeriodicSummaryModel>) => {
                if (!claims) {
                    return;
                }
                if (chartTitle) {
                    let chartData: any = [];
                    switch (chartTitle) {
                        case claimsProcessedPeriodic:
                        case claimsProcessedTrend:
                            chartData = this.mapClaimsForProcessedChart(claims as Array<ClaimPeriodicSummaryModel>);
                            break;
                        case claimsPaidPeriodic:
                        case claimsPaidTrend:
                            chartData = this.mapClaimsForSettledChart(claims as Array<ClaimPeriodicSummaryModel>);
                            break;
                        default:
                            chartData = this.mapClaimsForPaidRatioChart(claims as ClaimPeriodicSummaryModel);
                            break;
                    }

                    let index: number = this.claimsManagementCharts.findIndex((x: any) => x.title == chartTitle);
                    this.claimsManagementCharts[index].data = chartData;
                    if (chartTitle == claimsProcessedTrend) {
                        if (this.claimsManagementCharts[index].unit.type == 'time'
                            && !this.claimsManagementCharts[index].vAxesRightTitle.includes('(')) {
                            this.claimsManagementCharts[index].vAxesRightTitle =
                                this.claimsManagementCharts[index].vAxesRightTitle +
                                ` (${this.claimsManagementCharts[index].unit.postSymbol.toUpperCase()})`;
                        }
                    }
                }
            });
    }

    private getPoliciesTrendTitle(): string {
        let title: string = policiesTrend;
        if (this.authService.isMutualTenant()) {
            title = protectionTrend;
        }

        return title;
    }

    private getPoliciesPeriodicTitle(): string {
        let title: string = policiesPeriodic;
        if (this.authService.isMutualTenant()) {
            title = protectionPeriodic;
        }

        return title;
    }

    private checkReplaceTitlePremiumForMutual(title: string): string {
        if (this.authService.isMutualTenant() && title) {
            title = title.replace('Premium', 'Contribution');
        }

        return title;
    }

    private mapQuotesToPeriodicChart(data: Array<QuotePeriodicSummaryModel>): any {
        if (!data) return [];
        let returnData: Array<ChartPeriodicDataModel> = data.map((q: QuotePeriodicSummaryModel) => ({
            period: q.label,
            count: q.createdCount,
            amount: q.createdTotalPremium,
        }));
        return returnData;
    }

    private mapQuotesToConversionChart(data: QuotePeriodicSummaryModel): any {
        if (!data) return null;
        let returnData: Array<ChartCategorizedDataModel> = [];
        returnData.push({
            category: ChartDataCategory.Completed,
            value: data.convertedCount,
        });
        returnData.push({
            category: ChartDataCategory.Incomplete,
            value: data.abandonedCount,
        });

        return returnData;
    }

    private mapPolicyTransactionsToPeriodicChart(data: Array<PolicyTransactionPeriodicSummaryModel>): any {
        if (!data) return [];
        let returnData: Array<ChartPeriodicDataModel> = data.map((q: PolicyTransactionPeriodicSummaryModel) => ({
            period: q.label,
            count: q.createdCount,
            amount: q.createdTotalPremium,
        }));
        return returnData;
    }

    private mapClaimsForPaidRatioChart(data: ClaimPeriodicSummaryModel): any {
        if (!data) return null;
        let returnData: Array<ChartCategorizedDataModel> = [];
        returnData.push({
            category: ChartDataCategory.Completed,
            value: data.settledCount,
        });
        returnData.push({
            category: ChartDataCategory.Incomplete,
            value: data.declinedCount,
        });
        return returnData;
    }

    private mapClaimsForProcessedChart(data: Array<ClaimPeriodicSummaryModel>): any {
        if (!data) return [];
        let returnData: Array<ChartPeriodicDataModel> = data.map((q: ClaimPeriodicSummaryModel) => ({
            period: q.label,
            count: q.processedCount,
            amount: q.averageProcessingTime,
        }));
        return returnData;
    }

    private mapClaimsForSettledChart(data: Array<ClaimPeriodicSummaryModel>): any {
        if (!data) return [];
        let returnData: Array<ChartPeriodicDataModel> = data.map((q: ClaimPeriodicSummaryModel) => ({
            period: q.label,
            count: q.settledCount,
            amount: q.averageSettlementAmount,
        }));
        return returnData;
    }

    private loadDashboardStartupJobsStatus(): void {
        this.startupJobsForDashboard.forEach((jobName: string) => {
            this.startupJobApiService.getStartupJobStatus(jobName)
                .pipe(takeUntil(this.destroyed))
                .subscribe((jobStatus: StartupJobStatus) => {
                    let isJobNotComplete: boolean = jobStatus !== StartupJobStatus.Complete;
                    if (isJobNotComplete) {
                        this.isDashboardUpgradeComplete = false;
                    }
                });
        });
    }
}
