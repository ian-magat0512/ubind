
/**
 * Represents a view model for an entity that when presented in a list, can be
 * grouped together by matching values
 */
export interface GroupedEntityViewModel {
    groupByValue: string;
    setGroupByValue(viewModelList: Array<any>, groupBy: string): Array<any>;
}
