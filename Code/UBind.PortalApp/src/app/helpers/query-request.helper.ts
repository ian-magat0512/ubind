import { SortOption } from "@app/components/filter/sort-option";
import { ProductFilter } from "@app/models/product-filter";
import { FilterSelection } from "../viewmodels/filter-selection.viewmodel";
import { MapHelper } from "./map.helper";

/**
 * Export query request helper class
 * Manage the filter query requests.
 */
export class QueryRequestHelper {

    /**
     * Helper method for creating key-value pairs to be used for querying the API service.
     * @param searchKeys the search keywords to be applied to a service request, if any
     * @param filterSelections the filters to be applied to a service request, if any
     * @param currentTab the currently selected status on call of this method
     */
    public static getFilterQueryParameters(
        filterSelections?: Array<FilterSelection>,
        currentTab?: string,
    ): Map<string, string | Array<string>> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (filterSelections?.length > 0) {
            filterSelections.forEach((element: FilterSelection) => {
                if (element.propertyName.includes('afterDateTime') || element.propertyName.includes('beforeDateTime')) {
                    const dateValue: string = element.value.replace('> ', '').replace('< ', '');
                    const actualDate: Date = new Date(dateValue + 'UTC');
                    params.set(element.propertyName, actualDate.toISOString().substr(0, 10));
                } else {
                    if (!params.has(element.propertyName)) {
                        params.set(element.propertyName, element.value);
                    } else {
                        const existingValue: any = params.get(element.propertyName);

                        if (Array.isArray(existingValue)) {
                            existingValue.push(element.value);
                        } else {
                            params.set(element.propertyName, [existingValue, element.value]);
                        }
                    }
                }
            });
        }
        return params;
    }

    /**
     * Helper method for declaring date values to be used by
     *  the filter component in a list page. 
     * @param filterSelections filters currently applied to the list, if any
     */
    public static constructDateFilters(filterSelections?: Array<FilterSelection>): Array<any> {
        const model: Array<any> = [];
        for (const filter in filterSelections) {
            if (filterSelections[filter].propertyName.includes('afterDateTime')) {
                model['after'] = filterSelections[filter].value;
            }
        }
        for (const filter in filterSelections) {
            if (filterSelections[filter].propertyName.includes('beforeDateTime')) {
                model['before'] = filterSelections[filter].value;
            }
        }
        return model;
    }

    /**
     * Helper method for setting filter by date options to be used by Filter Page in a list page.
     * @param filters the currently applied filters to the list, if any,
     */
    public static constructFilterByDate(filters?: Array<FilterSelection>): string {
        for (const filter in filters) {
            if (filters[filter].propertyName.includes('dateFilteringPropertyName')) {
                return filters[filter].value;
            }
        }
    }

    /**
     * Helper method for setting sort options to be used by Filter Page in a list page.
     * @param filters the currently applied filters to the list, if any,
     */
    public static constructSortFilters(filters?: any): SortOption {
        const model: SortOption = { sortBy: [], sortOrder: [] };
        for (const filter in filters) {
            if (filters[filter].propertyName.includes('sortBy')) {
                model.sortBy.push(filters[filter].value);
            }
            if (filters[filter].propertyName.includes('sortOrder')) {
                model.sortOrder.push(filters[filter].value);
            }
        }
        return model;
    }

    /**
     * Helper method to determine if Include Test Data option is true or false.
     * @param filters the currently applied filters to the list, if any,
     */
    public static getTestDataFilter(filters?: any): boolean {
        let isTestDataIncluded: boolean = false;
        for (const filter in filters) {
            if (filters[filter].propertyName.includes('includeTestData')) {
                isTestDataIncluded = true;
            }
        }
        return isTestDataIncluded;
    }

    /**
     * Helper method for declaring status values to be used by
     *  the filter component in a list page.
     * @param keys the different statuses of a given list page
     * @param filterSelections the currently applied filters to the list, if any
     */
    public static constructStringFilters(keys: Array<string>, filterSelections: Array<FilterSelection>): Array<any> {
        const model: Array<any> = [];
        for (let key of keys) {
            model.push({
                status: key,
                value: filterSelections.some((x: FilterSelection) => x.value === key),
            });
        }
        return model;
    }

    /**
     * Helper method for declaring status values to be used by
     *  the filter component in a list page.
     * @param filters the different products of a given list page
     * @param filterSelections the currently applied filters to the list, if any
     */
    public static constructProductFilters(
        filters: Array<ProductFilter>,
        filterSelections: Array<FilterSelection>,
    ): Array<ProductFilter> {
        const model: Array<ProductFilter> = [];
        for (let filter of filters) {
            model.push({
                id: filter.id,
                name: filter.name,
                value: filterSelections.some((x: FilterSelection) => x.value === filter.id),
            });
        }
        return model;
    }

    /**
     * Helper method for getting the query parameters from status filter selections.
     * @param filterSelections the filters to be applied to a service request, if any
     */
    public static getStatusFilterSelectionQueryParameters(
        filterSelections?: Array<FilterSelection>,
    ): Map<string, string | Array<string>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (filterSelections && filterSelections.length > 0) {
            const statusFilterSelection: Array<FilterSelection> = filterSelections.filter(
                (filterSelection: FilterSelection) => filterSelection.propertyName == 'status',
            );
            if (statusFilterSelection.length > 0) {
                statusFilterSelection.forEach((filterSelection: FilterSelection) => {
                    MapHelper.add(params, 'status', filterSelection.value);
                });
            }
        }
        return params;
    }
}
