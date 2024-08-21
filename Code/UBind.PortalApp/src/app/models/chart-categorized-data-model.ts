import { ChartDataCategory } from "./chart-data-category.enum";

/**
 * model for representing a data model for donut charts.
 */
export interface ChartCategorizedDataModel {
    category: ChartDataCategory;
    value: number;
}
