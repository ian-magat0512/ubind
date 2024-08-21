import { formatDate } from "@angular/common";
import { Entity } from '@app/models/entity';

/**
 * Export list filter class
 * Get the date filters and sorting of dates.
 */
export class ListFilter<T extends Entity> {
    public filterStrings: Array<any> = [];
    /**
     * Gets the date filters to be displayed in the page
     */
    public getDateFilters(): Array<any> {
        const model: Array<any> = [];
        for (const item in this.filterStrings) {
            if (this.filterStrings[item].type.includes('After')) {
                model['after'] = this.filterStrings[item].value;
            }
        }
        for (const item in this.filterStrings) {
            if (this.filterStrings[item].type.includes('Before')) {
                model['before'] = this.filterStrings[item].value;
            }
        }
        return model;
    }

    public sortByDate(data: Array<T>): Array<any> {
        return data.sort((a: T, b: T) => {
            return +new Date(b.createdDateTime) - +new Date(a.createdDateTime);
        });
    }

    public groupByDate(data: Array<T>): Array<any> {
        return Array.from(new Set(data
            .map((item: T) => formatDate(new Date(item.createdDateTime), 'yyyy MMM dd', 'en-AU'))));
    }
}
