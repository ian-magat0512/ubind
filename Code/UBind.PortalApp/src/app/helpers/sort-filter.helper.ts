import { SortAndFilterBy, SortAndFilterByFieldName } from "@app/models/sort-filter-by.enum";

/**
 * This is a helper to manage the sort and filter for dates per entity.
 */
export class SortFilterHelper {
    // standard naming for the 'sort by' property
    public static sortBy: string = "sortBy";
    // standard naming for the 'filter by date' property
    public static dateFilteringPropertyName: string = "dateFilteringPropertyName";
    // standard naming for the 'include test data' property
    public static includeTestData: string = "includeTestData";

    public static defaultSortAndFilterDates: Array<string> = [
        SortAndFilterBy.CreatedDate,
        SortAndFilterBy.LastModifiedDate];

    public static getEntitySortAndFiltersMap(additional?: Map<string, string>): Map<string, string> {
        let entityDefaultSortAndFilters: Map<string, string> = new Map<string, string>();
        entityDefaultSortAndFilters.set(
            SortAndFilterBy.CreatedDate,
            SortAndFilterByFieldName.CreatedDate,
        );

        if (additional) {
            let mergedEntitySortAndFilters: Map<string, string> =
                new Map([
                    ...Array.from(entityDefaultSortAndFilters.entries()),
                    ...Array.from(additional.entries()),
                ]);
            return mergedEntitySortAndFilters;
        }

        return entityDefaultSortAndFilters;
    }

    public static getEntitySortAndFilter(
        additionalSortFilter?: Array<string>,
        isDefaultSortFilter: boolean = false,
    ): Array<string> {
        let sortFilters: Array<string> = this.defaultSortAndFilterDates;
        if (additionalSortFilter) {
            sortFilters = isDefaultSortFilter
                ? [...additionalSortFilter, ...sortFilters]
                : sortFilters.concat(additionalSortFilter);
        }

        return sortFilters;
    }

    public static setSortAndFilterByParam(
        params: Map<string, string | Array<string>>,
        selectedType: string,
        selectedField: string,
        sortAndFilter: Map<string, string>,
    ): Map<string, string | Array<string>> {
        if (sortAndFilter) {
            let value: string = sortAndFilter.get(selectedField);
            return params.set(selectedType, value);
        }

        return params;
    }

    public static setQuoteSortAndFilterByParam(
        params: Map<string, string | Array<string>>,
        selectedFieldType: string,
        selectedType: string,
    ): Map<string, string | Array<string>> {
        switch (selectedFieldType) {
            case SortAndFilterBy.CreatedDate:
                return params.set(selectedType, SortAndFilterByFieldName.CreatedDate);
            case SortAndFilterBy.LastModifiedDate:
                return params.set(selectedType, SortAndFilterByFieldName.LastModifiedDate);
            case SortAndFilterBy.ExpiryDate:
                return params.set(selectedType, SortAndFilterByFieldName.ExpiryDate);
            case SortAndFilterBy.CustomerName:
                return params.set(selectedType, SortAndFilterByFieldName.CustomerFullName);
            default: return params;
        }
    }

    public static determineSortAndFilterPropertyIsDate(propertyName: string): boolean {
        return propertyName?.includes('Date');
    }
}
