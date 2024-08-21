import { LocalDateHelper } from '@app/helpers';
import { PolicyResourceModel } from '@app/resource-models/policy.resource-model';
import { SegmentableEntityViewModel } from './segmentable-entity.viewmodel';
import { GroupedEntityViewModel } from './grouped-entity.viewmodel';
import { SortedEntityViewModel, SortDirection } from './sorted-entity.viewmodel';
import { DateHelper } from '@app/helpers/date.helper';
import { SortAndFilterBy } from '@app/models/sort-filter-by.enum';
import { PolicyStatus } from '@app/models';

/**
 * Export policy view model class.
 * TODO: Write a better class header: view model of policy.
 */
export class PolicyViewModel implements SegmentableEntityViewModel, GroupedEntityViewModel, SortedEntityViewModel {
    public constructor(policy: PolicyResourceModel) {
        this.id = policy.id;
        this.policyTitle = policy.policyTitle;
        this.quoteId = policy.quoteId;
        this.policyNumber = policy.policyNumber;
        this.productId = policy.productId;
        this.productName = policy.productName;
        this.totalPayable = policy.totalPayable;
        this.status = policy.status;
        this.segment = policy.status == PolicyStatus.Active
            || policy.status == PolicyStatus.Issued
            ? PolicyStatus.Current.toLowerCase()
            : policy.status.toLowerCase();
        this.expiryDate = LocalDateHelper.toLocalDate(policy.expiryDateTime);
        this.expiryTime = LocalDateHelper.convertToLocalAndGetTimeOnly(policy.expiryDateTime);
        this.expiryDateTime = policy.expiryDateTime;
        this.customerName = policy.customer ? policy.customer.displayName : '';
        this.isTestData = policy.isTestData;
        this.createdDate = LocalDateHelper.toLocalDate(policy.createdDateTime);
        this.policyIssuedTime = LocalDateHelper.convertToLocalAndGetTimeOnly(policy.issuedDateTime);
        this.policyIssuedDate = LocalDateHelper.toLocalDate(policy.issuedDateTime);
        this.policyIssuedDateTime = policy.issuedDateTime;
        this.lastModifiedDate = LocalDateHelper.toLocalDate(policy.lastModifiedDateTime);
        this.lastModifiedTime = LocalDateHelper.convertToLocalAndGetTimeOnly(policy.lastModifiedDateTime);
        this.lastModifiedDateTime = policy.lastModifiedDateTime;
        this.latestRenewalEffectiveDate = LocalDateHelper.toLocalDate(policy.latestRenewalEffectiveDateTime);
        this.latestRenewalEffectiveTime =
            LocalDateHelper.convertToLocalAndGetTimeOnly(policy.latestRenewalEffectiveDateTime);
        this.latestRenewalEffectiveDateTime = policy.latestRenewalEffectiveDateTime;
        this.isForRenewal = policy.isForRenewal;
        this.groupByValue = this.policyIssuedDate;
        this.sortByValue = policy.issuedDateTime;
        this.sortDirection = SortDirection.Descending;
        this.cancellationEffectiveDateTime = policy.cancellationEffectiveDateTime;
        this.cancellationEffectiveDate = policy.cancellationEffectiveDateTime ?
            LocalDateHelper.toLocalDate(policy.cancellationEffectiveDateTime) : '';
        this.cancellationEffectiveTime = policy.cancellationEffectiveDateTime ?
            LocalDateHelper.convertToLocalAndGetTimeOnly(policy.cancellationEffectiveDateTime) : '';
        this.inceptionDate = LocalDateHelper.toLocalDate(policy.inceptionDateTime);
        this.inceptionDateTime = policy.inceptionDateTime;
        let isExpired: boolean = Date.parse(DateHelper.getDateToday()) >= Date.parse(this.expiryDate);
        this.createdDateTime = policy.createdDateTime;
        this.expiryText = isExpired ? "Expired on" : "Expires on";
    }

    public id: string;
    public segment: string;
    public createdDate: string;
    public createdDateTime: string;
    public policyIssuedTime: string;
    public policyIssuedDate: string;
    public policyIssuedDateTime: string;
    public lastModifiedDate: string;
    public lastModifiedTime: string;
    public lastModifiedDateTime: string;
    public policyId: string;
    public quoteId: string;
    public policyTitle: string;
    public policyNumber: string;
    public productId: string;
    public productName: string;
    public totalPayable: string;
    public status: string;
    public expiryDate: string;
    public expiryDateTime: string;
    public expiryTime: string;
    public customerName: string;
    public isTestData: boolean;
    public isForRenewal: boolean;
    public groupByValue: string;
    public sortByValue: string;
    public sortDirection: SortDirection;
    public deleteFromList: boolean = false;
    public cancellationEffectiveDateTime: string;
    public cancellationEffectiveDate: string;
    public cancellationEffectiveTime: string;
    public inceptionDate: string;
    public inceptionDateTime: string;
    public expiryText: string;
    public latestRenewalEffectiveDate: string;
    public latestRenewalEffectiveTime: string;
    public latestRenewalEffectiveDateTime: string;

    public setGroupByValue(
        policyList: Array<PolicyViewModel>,
        groupBy: string,
    ): Array<PolicyViewModel> {
        switch (groupBy) {
            case SortAndFilterBy.InceptionDate:
                policyList.forEach((item: PolicyViewModel) => {
                    item.groupByValue = item.inceptionDate;
                });
                break;
            case SortAndFilterBy.ExpiryDate:
                policyList.forEach((item: PolicyViewModel) => {
                    item.groupByValue = item.expiryDate;
                });
                break;
            case SortAndFilterBy.LatestRenewalDate:
                policyList.forEach((item: PolicyViewModel) => {
                    item.groupByValue = item.latestRenewalEffectiveDate;
                });
                break;
            case SortAndFilterBy.CancellationDate:
                policyList.forEach((item: PolicyViewModel) => {
                    item.groupByValue = item.cancellationEffectiveDateTime
                        ? item.cancellationEffectiveDateTime
                        : item.policyIssuedDate;
                });
                break;
            case SortAndFilterBy.LastModifiedDate:
                policyList.forEach((item: PolicyViewModel) => {
                    item.groupByValue = item.lastModifiedDate;
                });
                break;
                // Policy Issued Date
            default:
                policyList.forEach((item: PolicyViewModel) => {
                    item.groupByValue = item.policyIssuedDate;
                });
        }
        return policyList;
    }

    public setSortOptions(
        policyList: Array<PolicyViewModel>,
        sortBy: string,
        sortDirection: SortDirection,
    ): Array<PolicyViewModel> {
        sortDirection = sortDirection == null ? SortDirection.Descending : sortDirection;

        switch (sortBy) {
            case SortAndFilterBy.InceptionDate:
                policyList.forEach((item: PolicyViewModel) => {
                    item.sortByValue = item.inceptionDateTime;
                    item.sortDirection = sortDirection;
                });
                break;
            case SortAndFilterBy.ExpiryDate:
                policyList.forEach((item: PolicyViewModel) => {
                    item.sortByValue = item.expiryDateTime;
                    item.sortDirection = sortDirection;
                });
                break;
            case SortAndFilterBy.LatestRenewalDate:
                policyList.forEach((item: PolicyViewModel) => {
                    item.sortByValue = item.latestRenewalEffectiveDateTime;
                    item.sortDirection = sortDirection;
                });
                break;
            case SortAndFilterBy.CancellationDate:
                policyList.forEach((item: PolicyViewModel) => {
                    item.sortByValue = item.cancellationEffectiveDateTime;
                    item.sortDirection = sortDirection;
                });
                break;
            case SortAndFilterBy.LastModifiedDate:
                policyList.forEach((item: PolicyViewModel) => {
                    item.sortByValue = item.lastModifiedDateTime;
                    item.sortDirection = sortDirection;
                });
                break;
            case SortAndFilterBy.CustomerName:
                policyList.forEach((item: PolicyViewModel) => {
                    item.sortByValue = item.customerName;
                    item.sortDirection = sortDirection;
                });
                break;
            case SortAndFilterBy.PolicyNumber:
                policyList.forEach((item: PolicyViewModel) => {
                    item.sortByValue = item.policyNumber;
                    item.sortDirection = sortDirection;
                });
                break;
                // Policy Issued Date
            default:
                policyList.forEach((item: PolicyViewModel) => {
                    item.sortByValue = item.policyIssuedDateTime;
                    item.sortDirection = sortDirection;
                });
                break;
        }

        return policyList;
    }
}
