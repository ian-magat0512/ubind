import { EntityViewModel } from "./entity.viewmodel";
import { ReleaseType } from "../models";
import { ReleaseResourceModel } from '../resource-models/release.resource-model';
import { LocalDateHelper } from "../helpers";
import { GroupedEntityViewModel } from "./grouped-entity.viewmodel";
import { SortedEntityViewModel, SortDirection } from "./sorted-entity.viewmodel";
import { SortAndFilterBy } from "@app/models/sort-filter-by.enum";

/**
 * Export release view model class.
 * TODO: Write a better class header: view model of release.
 */
export class ReleaseViewModel implements EntityViewModel, GroupedEntityViewModel, SortedEntityViewModel {
    public constructor(release: ReleaseResourceModel) {
        this.id = release.id;
        this.tenantId = release.tenantId;
        this.productId = release.productId;
        this.productName = release.productName;
        this.number = release.number;
        this.createdDate = LocalDateHelper.toLocalDate(release.createdDateTime);
        this.createdTime = LocalDateHelper.convertToLocalAndGetTimeOnly(release.createdDateTime);
        this.createdDateTime = release.createdDateTime;
        this.description = release.description;
        this.type = release.type;
        this.groupByValue = LocalDateHelper.toLocalDate(release.createdDateTime);
        this.sortByValue = release.createdDateTime;
        this.sortDirection = SortDirection.Descending;
        this.status = release.status;
    }

    public id: string;
    public tenantId: string;
    public productId: string;
    public productName: string;
    public number: number;
    public createdDate: any;
    public createdTime: any;
    public createdDateTime: string;
    public description: string;
    public errors: Array<string>;
    public type: ReleaseType;
    public groupByValue: string;
    public sortByValue: string;
    public sortDirection: SortDirection;
    public deleteFromList: boolean = false;
    public status: string;

    public setGroupByValue(
        releaseList: Array<ReleaseViewModel>,
        groupBy: string,
    ): Array<ReleaseViewModel> {
        // Release Entity only has one group by value - createdDateTime, 
        // since it was already set as a default value, we return the list for now
        return releaseList;
    }

    public setSortOptions(
        releaseList: Array<ReleaseViewModel>,
        sortBy: string,
        sortDirection: SortDirection,
    ): Array<ReleaseViewModel> {
        sortDirection = sortDirection ? sortDirection : SortDirection.Descending;

        switch (sortBy) {
            case SortAndFilterBy.ReleaseNumber:
                releaseList.forEach((item: ReleaseViewModel) => {
                    item.sortByValue = item.number.toString();
                    item.sortDirection = sortDirection;
                });
                break;
            default:
                releaseList.forEach((item: ReleaseViewModel) => {
                    item.sortByValue = item.createdDateTime;
                    item.sortDirection = sortDirection;
                });
                break;
        }
        return releaseList;
    }
}
