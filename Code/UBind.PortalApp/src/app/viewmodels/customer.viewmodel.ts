import { SegmentableEntityViewModel } from "./segmentable-entity.viewmodel";
import { CustomerResourceModel } from "../resource-models/customer.resource-model";
import { LocalDateHelper } from "../helpers/local-date.helper";
import { GroupedEntityViewModel } from "./grouped-entity.viewmodel";
import { SortedEntityViewModel, SortDirection } from "./sorted-entity.viewmodel";
import { SortAndFilterBy } from "@app/models/sort-filter-by.enum";

/**
 * Export customer view model class.
 * TODO: Write a better class header: view model of customer.
 */
export class CustomerViewModel implements SegmentableEntityViewModel, GroupedEntityViewModel, SortedEntityViewModel {
    public constructor(customer: CustomerResourceModel) {
        this.id = customer.id;
        this.name = customer.fullName;
        this.isTestData = customer.isTestData;
        this.segment = customer.status !== undefined
            ? customer.status.toLowerCase() : customer['userStatus'].toLowerCase();
        this.createdTime = LocalDateHelper.convertToLocalAndGetTimeOnly(customer.createdDateTime);
        this.createdDateTime = customer.createdDateTime;
        this.lastModifiedDateTime = customer.lastModifiedDateTime;
        this.groupByValue = this.createdDate = LocalDateHelper.toLocalDate(customer.createdDateTime);
        this.sortByValue = customer.createdDateTime;
        this.sortDirection = SortDirection.Descending;
        this.profilePictureId = customer.profilePictureId;
        this.deleteFromList = customer.deleteFromList;
    }

    public id: string;
    public name: string;
    public segment: string;
    public isTestData: boolean;
    public createdDate: string;
    public createdTime: string;
    public createdDateTime: string;
    public groupByValue: string;
    public sortByValue: string;
    public sortDirection: SortDirection;
    public profilePictureId: string;
    public deleteFromList: boolean = false;
    public lastModifiedDateTime: string;

    public setGroupByValue(
        customerList: Array<CustomerViewModel>,
        groupBy: string,
    ): Array<CustomerViewModel> {
        // Customer Entity only has one group by value - createdDateTime, 
        // since it was already set as a default value, we return the list for now
        return customerList;
    }

    public setSortOptions(
        customerList: Array<CustomerViewModel>,
        sortBy: string,
        sortDirection: SortDirection,
    ): Array<CustomerViewModel> {

        sortDirection = sortDirection == null ? SortDirection.Descending : sortDirection;

        if (sortBy === SortAndFilterBy.CustomerName) {
            customerList.forEach((item: CustomerViewModel) => {
                item.sortByValue = item.name;
                item.sortDirection = sortDirection;
            });
        } else if (sortBy === SortAndFilterBy.LastModifiedDate) {
            customerList.forEach((item: CustomerViewModel) => {
                item.sortByValue = item.lastModifiedDateTime;
                item.sortDirection = sortDirection;
            });
        } else {
            customerList.forEach((item: CustomerViewModel) => {
                item.sortByValue = item.createdDateTime;
                item.sortDirection = sortDirection;
            });
        }

        return customerList;
    }
}
