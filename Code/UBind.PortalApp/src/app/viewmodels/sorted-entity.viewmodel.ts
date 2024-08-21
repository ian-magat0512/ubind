export enum SortDirection {
    Ascending = "Ascending",
    Descending = "Descending"
}

/**
 * Represents an entity that can by sorted
 */
export interface SortedEntityViewModel {
    sortByValue: string;
    sortDirection: SortDirection;
    setSortOptions(
        list: Array<any>,
        sortBy: string,
        sortDirection: SortDirection): Array<any>;
}
