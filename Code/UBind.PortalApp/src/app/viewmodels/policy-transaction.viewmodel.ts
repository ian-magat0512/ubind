import { LocalDateHelper, PolicyHelper } from '@app/helpers';
import { PolicyTransactionResourceModel } from '@app/resource-models/policy.resource-model';
import { EntityViewModel } from './entity.viewmodel';
import { GroupedEntityViewModel } from './grouped-entity.viewmodel';
import { SortedEntityViewModel, SortDirection } from './sorted-entity.viewmodel';

/**
 * Export policy transaction view model class.
 * TODO: Write a better class header: view model of policy transaction.
 */
export class PolicyTransactionViewModel implements EntityViewModel, GroupedEntityViewModel, SortedEntityViewModel {
    public constructor(policyTransaction: PolicyTransactionResourceModel) {
        this.id = this.transactionId = policyTransaction.transactionId;
        this.policyId = policyTransaction.policyId;
        this.policyNumber = policyTransaction.policyNumber;
        this.quoteId = policyTransaction.quoteId;
        this.eventTypeSummary = policyTransaction.eventTypeSummary;
        this.transactionStatus = policyTransaction.transactionStatus;
        this.createdDate = LocalDateHelper.toLocalDate(policyTransaction.createdDateTime);
        this.createdDateTime = policyTransaction.createdDateTime;
        this.createdTime = LocalDateHelper.convertToLocalAndGetTimeOnly(policyTransaction.createdDateTime);
        this.effectiveDate = LocalDateHelper.toLocalDate(policyTransaction.effectiveDateTime);
        this.effectiveDateTime = policyTransaction.effectiveDateTime;
        this.cancellationEffectiveDate = LocalDateHelper.toLocalDate(policyTransaction.cancellationEffectiveDateTime);
        this.cancellationEffectoveDateTime = policyTransaction.cancellationEffectiveDateTime;
        this.iconName = this.getIconNameForTransactionEventSummary(this.eventTypeSummary);
        this.alternateEventTypeSummary = this.getAlternateEventTypeSummary(this.eventTypeSummary);
        this.groupByValue = this.createdDate;
        this.sortByValue = policyTransaction.createdDateTime;
        this.sortDirection = SortDirection.Descending;
    }

    public id: string;
    public transactionId: string;
    public policyId: string;
    public policyNumber: string;
    public quoteId: string;
    public eventTypeSummary: string;
    public transactionStatus: string;
    public effectiveDate: string;
    public effectiveDateTime: string;
    public cancellationEffectiveDate: string;
    public cancellationEffectoveDateTime: string;
    public createdDate: string;
    public createdTime: string;
    public createdDateTime: string;
    public iconName: string;
    public alternateEventTypeSummary: string;
    public groupByValue: string;
    public sortByValue: string;
    public sortDirection: SortDirection;
    public deleteFromList: boolean = false;

    public isCancellation(): boolean {
        return this.eventTypeSummary == PolicyHelper.constants.Labels.Status.Cancelled;
    }

    public setGroupByValue(
        policyTransactionList: Array<PolicyTransactionViewModel>,
        groupBy: string,
    ): Array<PolicyTransactionViewModel> {
        switch (groupBy) {
            case "Effective Date":
                policyTransactionList.forEach((item: PolicyTransactionViewModel) => {
                    item.groupByValue = item.effectiveDate;
                });
                break;
            case "Cancellation Date":
                policyTransactionList.forEach((item: PolicyTransactionViewModel) => {
                    item.groupByValue = item.cancellationEffectiveDate;
                });
                break;
            default:
                policyTransactionList.forEach((item: PolicyTransactionViewModel) => {
                    item.groupByValue = item.createdDate;
                });
        }
        return policyTransactionList;
    }

    public setSortOptions(
        policyTransactionList: Array<PolicyTransactionViewModel>,
        sortBy: string,
        sortDirection: SortDirection,
    ): Array<PolicyTransactionViewModel> {
        sortDirection = sortDirection ? sortDirection : SortDirection.Descending;

        switch (sortBy) {
            case "Effective Date":
                policyTransactionList.forEach((item: PolicyTransactionViewModel) => {
                    item.sortByValue = item.effectiveDateTime;
                    item.sortDirection = sortDirection;
                });
                break;
            case "Cancellation Date":
                policyTransactionList.forEach((item: PolicyTransactionViewModel) => {
                    item.sortByValue = item.cancellationEffectoveDateTime;
                    item.sortDirection = sortDirection;
                });
                break;
            case "Policy Number":
                policyTransactionList.forEach((item: PolicyTransactionViewModel) => {
                    item.sortByValue = item.policyNumber;
                    item.sortDirection = sortDirection;
                });
                break;
            default:
                policyTransactionList.forEach((item: PolicyTransactionViewModel) => {
                    item.sortByValue = item.createdDateTime;
                    item.sortDirection = sortDirection;
                });
                break;
        }
        return policyTransactionList;
    }

    private getIconNameForTransactionEventSummary(eventSummary: string): string {
        switch (eventSummary) {
            case PolicyHelper.constants.Labels.Status.Cancelled:
                return 'shield-ban';
            case PolicyHelper.constants.Labels.Status.Purchased:
                return 'shield-add';
            case PolicyHelper.constants.Labels.Status.Renewed:
                return 'shield-refresh';
            case PolicyHelper.constants.Labels.Status.Adjusted:
                return 'shield-pen';
        }
    }

    private getAlternateEventTypeSummary(eventSummary: string): string {
        if (eventSummary == PolicyHelper.constants.Labels.Status.Purchased) {
            return 'issued';
        } else {
            return eventSummary.toLowerCase();
        }
    }
}
