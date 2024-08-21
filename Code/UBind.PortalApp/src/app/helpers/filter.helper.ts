import { FilterSelection } from '@app/viewmodels/filter-selection.viewmodel';
import { QuoteType } from '@app/models';
import { QuoteViewModel } from '@app/viewmodels/quote.viewmodel';
import { SortDirection } from '@app/viewmodels/sorted-entity.viewmodel';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Export filter helper class
 * Filter class is to help the filtering of 
 * icons and creating of filters.
 */
export class FilterHelper {

    private static chipIconMap: any = {
        date: "calendar-filter",
        search: "search",
        status: "checkbox-outline",
        sortBy: "sort-variant",
        dateFilteringPropertyName: "filter",
        sortOrderAscending: "sort-alphabetical-ascending",
        sortOrderDescending: "sort-alphabetical-descending",
        sortOrderAscendingDate: "sort-calendar-ascending",
        sortOrderDescendingDate: "sort-calendar-descending",
        testData: "tools",
        product: "cube",
    };

    public static createFilterSelection(
        propertyName: string,
        value: string,
        label: string = value,
        iconLibrary: string = IconLibrary.IonicV4,
        isDateProperty: boolean = false,
    ): FilterSelection {
        let filterSelection: FilterSelection = new FilterSelection(
            propertyName,
            value,
            this.getFilterIcon(propertyName, value, isDateProperty),
            label,
            iconLibrary,
        );
        return filterSelection;
    }

    private static getFilterIcon(propertyName: string, value: string, isDateProperty: boolean): string {
        let icon: string = null;

        switch (propertyName) {
            case "dateFilteringPropertyName":
                icon = this.chipIconMap.dateFilteringPropertyName;
                break;
            case "beforeDateTime":
                icon = this.chipIconMap.date;
                break;
            case "afterDateTime":
                icon = this.chipIconMap.date;
                break;
            case "search":
                icon = this.chipIconMap.search;
                break;
            case "quoteTypes":
                icon = QuoteViewModel.getIconNameForQuoteType(QuoteType[value]);
                break;
            case "productIds":
                icon = this.chipIconMap.product;
                break;
            case "sortBy":
                icon = this.chipIconMap.sortBy;
                break;
            case "sortOrder":
                icon = value == SortDirection.Ascending
                    ? isDateProperty
                        ? this.chipIconMap.sortOrderAscendingDate
                        : this.chipIconMap.sortOrderAscending
                    : isDateProperty
                        ? this.chipIconMap.sortOrderDescendingDate
                        : this.chipIconMap.sortOrderDescending;
                break;
            case "includeTestData":
                icon = this.chipIconMap.testData;
                break;
            default:
                icon = this.chipIconMap.status;
                break;
        }

        return icon;
    }
}
