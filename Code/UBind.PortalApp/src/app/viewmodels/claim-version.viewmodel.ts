import { ClaimVersionListResourceModel } from '@app/resource-models/claim.resource-model';
import { LocalDateHelper } from '@app/helpers';
import { GroupedEntityViewModel } from './grouped-entity.viewmodel';
import { SortedEntityViewModel, SortDirection } from './sorted-entity.viewmodel';
import { EntityViewModel } from './entity.viewmodel';

/**
 * Claim version view model.
 */
export class ClaimVersionViewModel implements
    EntityViewModel, GroupedEntityViewModel, SortedEntityViewModel {
    public constructor(claimVersion: ClaimVersionListResourceModel) {
        this.id = this.claimVersionId = claimVersion.claimVersionId;
        this.claimReference = claimVersion.claimReference;
        this.versionNumber = claimVersion.versionNumber;

        this.createdDate = LocalDateHelper.toLocalDate(claimVersion.createdDateTime);
        this.createdTime = LocalDateHelper.convertToLocalAndGetTimeOnly(claimVersion.createdDateTime);
        this.lastModifiedDate = LocalDateHelper.toLocalDate(claimVersion.lastModifiedDateTime);
        this.lastModifiedTime = LocalDateHelper.convertToLocalAndGetTimeOnly(claimVersion.lastModifiedDateTime);
        this.groupByValue = this.createdDate;
        this.sortByValue = claimVersion.claimReference;
        this.sortDirection = SortDirection.Descending;
    }

    public id: string;
    public claimVersionId: string;
    public isTestData: boolean;
    public productName: string;
    public claimReference: string;
    public versionNumber: string;
    public status: string;
    public createdDate: string;
    public createdTime: string;
    public lastModifiedDate: string;
    public lastModifiedTime: string;
    public claimNumber: string;
    public groupByValue: string;
    public sortByValue: string;
    public sortDirection: SortDirection;
    public deleteFromList: boolean = false;

    public setGroupByValue(
        claimVersionList: Array<ClaimVersionViewModel>,
        groupBy: string,
    ): Array<ClaimVersionViewModel> {
        return claimVersionList;
    }

    public setSortOptions(
        claimVersionList: Array<ClaimVersionViewModel>,
        sortBy: string,
        sortDirection: SortDirection,
    ): Array<ClaimVersionViewModel> {
        sortDirection = sortDirection == null ? SortDirection.Descending : sortDirection;

        if (sortBy === "Claim Reference") {
            claimVersionList.forEach((item: ClaimVersionViewModel) => {
                item.sortByValue = item.claimReference;
                item.sortDirection = sortDirection;
            });
        } else {
            claimVersionList.forEach((item: ClaimVersionViewModel) => {
                item.sortByValue = item.createdDate;
                item.sortDirection = sortDirection;
            });
        }
        return claimVersionList;
    }
}
