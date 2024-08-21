import { ClaimResourceModel } from '@app/resource-models/claim.resource-model';
import { LocalDateHelper, ClaimHelper } from '@app/helpers';
import { SegmentableEntityViewModel } from './segmentable-entity.viewmodel';
import { GroupedEntityViewModel } from './grouped-entity.viewmodel';
import { SortedEntityViewModel, SortDirection } from './sorted-entity.viewmodel';
import { SortAndFilterBy } from '@app/models/sort-filter-by.enum';

/**
 * Export claim view model class.
 * TODO: Write a better class header: claim view models.
 */
export class ClaimViewModel implements SegmentableEntityViewModel, GroupedEntityViewModel, SortedEntityViewModel {
    public constructor(claim: ClaimResourceModel) {
        this.id = claim.id;
        this.productName = claim.productName;
        this.fullName = claim.customerDetails && claim.customerDetails.displayName;
        this.claimReference = claim.claimReference;
        this.status = claim.status;
        this.isTestData = claim.isTestData;
        this.segment = claim.status && ClaimHelper.getTab(claim.status).toLowerCase();
        this.claimNumber = claim.claimNumber;
        this.createdDate = claim.createdDateTime && LocalDateHelper.toLocalDate(claim.createdDateTime);
        this.createdDateTime = claim.createdDateTime;
        this.lastModifiedDate = claim.lastModifiedDateTime && LocalDateHelper.toLocalDate(claim.lastModifiedDateTime);
        this.lastModifiedDateTime = claim.lastModifiedDateTime;
        this.groupByValue = this.createdDate;
        this.sortByValue = claim.createdDateTime;
        this.sortDirection = SortDirection.Descending;
    }

    public id: string;
    public fullName: string;
    public isTestData: boolean;
    public segment: string;
    public productName: string;
    public claimReference: string;
    public status: string;
    public createdDate: string;
    public createdDateTime: string;
    public claimNumber: string;
    public lastModifiedDate: string;
    public lastModifiedDateTime: string;
    public groupByValue: string;
    public sortByValue: string;
    public sortDirection: SortDirection;
    public deleteFromList: boolean = false;

    public setGroupByValue(
        claimList: Array<ClaimViewModel>,
        groupBy: string,
    ): Array<ClaimViewModel> {
        if (groupBy === "Last Modified Date") {
            claimList.forEach((item: ClaimViewModel) => {
                item.groupByValue = item.lastModifiedDate;
            });
        } else {
            claimList.forEach((item: ClaimViewModel) => {
                item.groupByValue = item.createdDate;
            });
        }
        return claimList;
    }

    public setSortOptions(
        claimList: Array<ClaimViewModel>,
        sortBy: string,
        sortDirection: SortDirection,
    ): Array<ClaimViewModel> {
        sortDirection = sortDirection == null ? SortDirection.Descending : sortDirection;

        switch (sortBy) {
            case SortAndFilterBy.LastModifiedDate:
                claimList.forEach((item: ClaimViewModel) => {
                    item.sortByValue = item.lastModifiedDateTime;
                    item.sortDirection = sortDirection;
                });
                break;
            case SortAndFilterBy.CustomerName:
                claimList.forEach((item: ClaimViewModel) => {
                    item.sortByValue = item.fullName;
                    item.sortDirection = sortDirection;
                });
                break;
            case SortAndFilterBy.ClaimNumber:
                // Claims with no Claim Number will be filtered from the list
                claimList = claimList.filter((x: ClaimViewModel) =>
                    x.claimNumber != "" && x.claimNumber != null);
                claimList.forEach((item: ClaimViewModel) => {
                    item.sortByValue = item.claimNumber;
                    item.sortDirection = sortDirection;
                });
                break;
                // Created Date
            default:
                claimList.forEach((item: ClaimViewModel) => {
                    item.sortByValue = item.createdDateTime;
                    item.sortDirection = sortDirection;
                });
                break;
        }
        return claimList;
    }
}
