import { ChartSamplePeriodLength } from "./chart-sample-period-length.enum";

/**
 * model for representing an event data emitted in column charts.
 */
export interface ChartPeriodSelectionModel {
    dataGrouping: ChartSamplePeriodLength;
    range: string;
}
