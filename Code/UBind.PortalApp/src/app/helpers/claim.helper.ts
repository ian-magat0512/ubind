import { SortOption } from '@app/components/filter/sort-option';
import { SortAndFilterBy } from '@app/models/sort-filter-by.enum';
import { SortDirection } from '@app/viewmodels/sorted-entity.viewmodel';
import { SortFilterHelper } from './sort-filter.helper';

/**
 * Export claim helper class.
 * This class is manage the status, segment of the claims
 */
export class ClaimHelper {
    public static dashboardDisplayableStatuses: Array<string> = [
        'Incomplete',
        'Notified',
        'Acknowledged',
        'Review',
        'Assessment',
        'Approved',
        'Withdrawn',
        'Declined',
        'Complete'];
    public static segmentsList: Array<string> = [
        'Incomplete',
        'Notified',
        'Acknowledged',
        'Review',
        'Assessment',
        'Settlement',
        'Complete'];
    public static segmentsListLowerCase: Array<string> = [
        'incomplete',
        'notified',
        'acknowledged',
        'review',
        'assessment',
        'settlement',
        'complete'];
    public static sortOptions: SortOption = {
        sortBy: SortFilterHelper.getEntitySortAndFilter([
            SortAndFilterBy.CustomerName,
            SortAndFilterBy.ClaimNumber,
        ]),
        sortOrder: [
            SortDirection.Descending,
            SortDirection.Ascending,
        ],
    };

    public static status: any = {
        Nascent: 'Nascent',
        Incomplete: 'Incomplete',
        Notified: 'Notified',
        Acknowledged: 'Acknowledged',
        Review: 'Review',
        Assessment: 'Assessment',
        Settlement: 'Settlement',
        Withdrawn: 'Withdrawn',
        Approved: 'Approved',
        Declined: 'Declined',
        Complete: 'Complete',
        Settled: 'Settled',
        Inactive: ['Withdrawn', 'Complete', 'Declined'],
        Active: ['Incomplete', 'Notified', 'Acknowledged', 'Review', 'Assessment', 'Approved'],
        Label: {
            Active: 'Active',
            Inactive: 'Inactive',
        },
    };

    public static getTab(status: string, isCustomer: boolean = false): string {
        let tab: string = isCustomer ? this.status.Label.Active : this.status.Incomplete;
        if (isCustomer) {
            if (status) {
                switch (status.toLowerCase()) {
                    case this.status.Withdrawn.toLowerCase():
                    case this.status.Complete.toLowerCase():
                    case this.status.Declined.toLowerCase():
                    case this.status.Settled.toLowerCase():
                        tab = this.status.Label.Inactive;
                        break;
                    case this.status.Incomplete.toLowerCase():
                    case this.status.Notified.toLowerCase():
                    case this.status.Acknowledged.toLowerCase():
                    case this.status.Review.toLowerCase():
                    case this.status.Assessment.toLowerCase():
                    case this.status.Approved.toLowerCase():
                        tab = this.status.Label.Active;
                        break;
                    default: break;
                }
            }

        } else {
            if (status) {
                switch (status.toLowerCase()) {
                    case this.status.Incomplete.toLowerCase():
                        tab = this.status.Incomplete;
                        break;
                    case this.status.Notified.toLowerCase():
                        tab = this.status.Notified;
                        break;
                    case this.status.Acknowledged.toLowerCase():
                        tab = this.status.Acknowledged;
                        break;
                    case this.status.Review.toLowerCase():
                        tab = this.status.Review;
                        break;
                    case this.status.Assessment.toLowerCase():
                        tab = this.status.Assessment;
                        break;
                    case this.status.Approved.toLowerCase():
                        tab = this.status.Settlement;
                        break;
                    case this.status.Settled.toLowerCase():
                    case this.status.Complete.toLowerCase():
                    case this.status.Declined.toLowerCase():
                    case this.status.Withdrawn.toLowerCase():
                        tab = this.status.Complete;
                        break;
                    default: break;
                }
            }
        }

        return tab;
    }

    public static canShowUpdateButton(claimStatus: string): boolean {
        return (this.status.Active.find((x: string) => x.toLowerCase() == claimStatus.toLowerCase()) != undefined);
    }
}
