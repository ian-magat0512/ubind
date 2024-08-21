import { formatDate } from '@angular/common';
import { SortOption } from '@app/components/filter/sort-option';
import { CustomerPolicyStatus, PolicyStatus, PremiumResult } from '@app/models';
import { SortAndFilterBy, SortAndFilterByFieldName } from '@app/models/sort-filter-by.enum';
import { PolicyTransactionEventNamePastTense } from '@app/models/policy-transaction-event-name-past-tense.enum';
import { PolicyTransactionEventName } from '@app/models/policy-transaction-event-name.enum';
import { CurrencyPipe } from '@app/pipes/currency.pipe';
import { SortDirection } from '@app/viewmodels/sorted-entity.viewmodel';

/**
 * Policy helper class
 *
 * This class provides utility functions for managing the status and segment of policies.
 * It includes methods for sorting, filtering, and formatting policy-related data.
 */
export class PolicyHelper {
    private static dateFormatting: string = 'dd MMMM yyyy';

    public static constants: any = {
        Labels: {
            Toast: {
                View: 'VIEW',
                Dismiss: 'DISMISS',
            },
            Status: {
                Purchased: 'Purchased',
                Adjusted: 'Adjusted',
                Renewed: 'Renewed',
                Cancelled: 'Cancelled',
                Pending: 'Pending',
                Issued: 'Issued',
                Completed: 'Completed',
            },
            StampDuty: {
                StampDuty: 'Stamp Duty',
                ACT: 'Stamp Duty ACT',
                NSW: 'Stamp Duty NSW',
                NT: 'Stamp Duty NT',
                QLD: 'Stamp Duty QLD',
                SA: 'Stamp Duty SA',
                TAS: 'Stamp Duty TAS',
                VIC: 'Stamp Duty VIC',
                WA: 'Stamp Duty WA',
            },
            Esl: {
                Esl: 'Emergency Services Levy',
                EslNsw: 'Emergency Services Levy NSW',
                EslTas: 'Emergency Services Levy TAS',
            },
        },
        Tabs: {
            Purchased: ['Details', 'Questions', 'Premium', 'Documents', 'Price'],
            Adjusted: ['Details', 'Questions', 'Premium', 'Documents', 'Price', 'Refund'],
            Renewed: ['Details', 'Questions', 'Premium', 'Documents', 'Price'],
            Cancelled: ['Details', 'Questions', 'Refund', 'Price', 'Documents'],
        },
        Formats: {
            dateMMMdd: 'MMM dd',
            ddMonthyyyy: 'dd MMMM, yyyy',
        },
        Locales: {
            AU: 'en-AU',
        },
        Transactions: {
            Titles: {
                Purchased: 'Policy Issuance',
                Adjusted: 'Policy Adjustment',
                Renewed: 'Policy Renewal',
                Cancelled: 'Policy Cancellation',
            },
        },

    };

    public static sortOptions: SortOption = {
        sortBy: [
            SortAndFilterBy.IssuedDate,
            SortAndFilterBy.InceptionDate,
            SortAndFilterBy.ExpiryDate,
            SortAndFilterBy.LatestRenewalDate,
            SortAndFilterBy.CancellationDate,
            SortAndFilterBy.LastModifiedDate,
            SortAndFilterBy.CustomerName,
            SortAndFilterBy.PolicyNumber,
        ],
        sortOrder: [
            SortDirection.Descending,
            SortDirection.Ascending],
    };

    public static filterByDates: Array<string> = [
        SortAndFilterBy.IssuedDate,
        SortAndFilterBy.InceptionDate,
        SortAndFilterBy.ExpiryDate,
        SortAndFilterBy.LatestRenewalDate,
        SortAndFilterBy.CancellationDate,
        SortAndFilterBy.LastModifiedDate,
    ];

    public static isDisplayableTab(transactionStatus: string, tab: string): boolean {
        let isDisplayable: boolean = false;
        switch (transactionStatus) {
            case this.constants.Labels.Status.Purchased: {
                isDisplayable = this.constants.Tabs.Purchased.includes(tab);
            }
                break;
            case this.constants.Labels.Status.Adjusted: {
                isDisplayable = this.constants.Tabs.Adjusted.includes(tab);
            }
                break;
            case this.constants.Labels.Status.Renewed: {
                isDisplayable = this.constants.Tabs.Renewed.includes(tab);
            }
                break;
            case this.constants.Labels.Status.Cancelled: {
                isDisplayable = this.constants.Tabs.Cancelled.includes(tab);
            }
                break;
            default: break;
        }

        return isDisplayable;
    }

    public static setSortAndFilterByParam(
        params: Map<string, string | Array<string>>,
        selectedFieldType: string,
        selectedType: string,
    ): Map<string, string | Array<string>> {
        switch (selectedFieldType) {
            case SortAndFilterBy.IssuedDate:
                return params.set(selectedType, SortAndFilterByFieldName.IssuedDate);
            case SortAndFilterBy.InceptionDate:
                return params.set(selectedType, SortAndFilterByFieldName.InceptionDate);
            case SortAndFilterBy.ExpiryDate:
                return params.set(selectedType, SortAndFilterByFieldName.ExpiryDate);
            case SortAndFilterBy.LatestRenewalDate:
                return params.set(selectedType, SortAndFilterByFieldName.LatestRenewalDate);
            case SortAndFilterBy.CancellationDate:
                return params.set(selectedType, SortAndFilterByFieldName.CancellationDate);
            case SortAndFilterBy.LastModifiedDate:
                return params.set(selectedType, SortAndFilterByFieldName.LastModifiedDate);
            case SortAndFilterBy.CustomerName:
                return params.set(selectedType, SortAndFilterByFieldName.CustomerFullName);
            case SortAndFilterBy.PolicyNumber:
                return params.set(selectedType, SortAndFilterByFieldName.PolicyNumber);
            default: return params;
        }
    }

    public static sortDate(a: any, b: any): number {
        let aDate: Date = new Date(a.createdDate);
        let bDate: Date = new Date(b.createdDate);
        if (aDate < bDate) {
            return 1;
        } else if (aDate > bDate) {
            return -1;
        } else {
            return 0;
        }
    }

    public static formatPolicyDate(dateString: string): string {
        let date: string = formatDate(dateString, this.constants.Formats.ddMonthyyyy, this.constants.Locales.AU);
        return date;
    }

    public static getStampDutyStateData(premiumResult: PremiumResult, currencyFormatPipe: CurrencyPipe): any {
        let stateData: any = {};
        let stampDutyHeaders: Array<string> = this.getParentPropertyHeaders(premiumResult, 'stampDuty');
        for (let header of stampDutyHeaders) {
            if (premiumResult[header].toString().length > 0) {
                let correctHeader: string = '';
                switch (header) {
                    case "stampDutyAct":
                        correctHeader = this.constants.Labels.StampDuty.ACT;
                        break;
                    case "stampDutyNsw":
                        correctHeader = this.constants.Labels.StampDuty.NSW;
                        break;
                    case "stampDutyNt":
                        correctHeader = this.constants.Labels.StampDuty.NT;
                        break;
                    case "stampDutyQld":
                        correctHeader = this.constants.Labels.StampDuty.QLD;
                        break;
                    case "stampDutySa":
                        correctHeader = this.constants.Labels.StampDuty.SA;
                        break;
                    case "stampDutyTas":
                        correctHeader = this.constants.Labels.StampDuty.TAS;
                        break;
                    case "stampDutyVic":
                        correctHeader = this.constants.Labels.StampDuty.VIC;
                        break;
                    case "stampDutyWa":
                        correctHeader = this.constants.Labels.StampDuty.WA;
                        break;
                    default: break;
                }

                if (correctHeader != '') {
                    stateData[correctHeader] = currencyFormatPipe.transform(
                        premiumResult[header],
                        premiumResult.currencyCode,
                    );
                }
            }
        }
        return stateData;
    }

    public static getEslData(premiumResult: PremiumResult, currencyFormatPipe: CurrencyPipe): any {
        let stateData: any = {};
        let eslHeaders: Array<string> = this.getParentPropertyHeaders(premiumResult, 'esl');
        for (let eslHeader of eslHeaders) {
            if (premiumResult[eslHeader].toString().length > 0) {
                let correctHeader: string = '';
                switch (eslHeader) {
                    case "eslNsw":
                        correctHeader = this.constants.Labels.Esl.EslNsw;
                        break;
                    case "eslTas":
                        correctHeader = this.constants.Labels.Esl.EslTas;
                        break;
                    default: break;
                }

                if (correctHeader != '') {
                    stateData[correctHeader] = currencyFormatPipe.transform(
                        premiumResult[eslHeader],
                        premiumResult.currencyCode,
                    );
                }
            }
        }
        return stateData;
    }

    public static getPageTitle(status: string): string {
        let pageTitle: string = '';
        switch (status) {
            case this.constants.Labels.Status.Purchased:
                pageTitle = this.constants.Transactions.Titles.Purchased;
                break;
            case this.constants.Labels.Status.Adjusted:
                pageTitle = this.constants.Transactions.Titles.Adjusted;
                break;
            case this.constants.Labels.Status.Renewed:
                pageTitle = this.constants.Transactions.Titles.Renewed;
                break;
            case this.constants.Labels.Status.Cancelled:
                pageTitle = this.constants.Transactions.Titles.Cancelled;
                break;
        }
        return pageTitle;
    }

    public static getEditAdditionalPropertiesPopOverTitle(eventTypeName: PolicyTransactionEventNamePastTense): string {
        let propertyType: PolicyTransactionEventName;
        switch (eventTypeName) {
            case PolicyTransactionEventNamePastTense.Adjusted:
                propertyType = PolicyTransactionEventName.Adjustment;
                break;
            case PolicyTransactionEventNamePastTense.Cancelled:
                propertyType = PolicyTransactionEventName.Cancellation;
                break;
            case PolicyTransactionEventNamePastTense.Renewed:
                propertyType = PolicyTransactionEventName.Renewal;
                break;
            default:
                propertyType = PolicyTransactionEventName.Purchase;
                break;
        }
        return `Edit ${propertyType} Properties`;
    }

    public static getTab(status: string, isCustomer: boolean = false): string {
        let tab: string = PolicyStatus.Current;
        if (isCustomer) {
            switch (status.toLowerCase()) {
                case PolicyStatus.Active.toLowerCase():
                case PolicyStatus.Issued.toLowerCase():
                    tab = CustomerPolicyStatus.Current;
                    break;
                case PolicyStatus.Cancelled.toLowerCase():
                case PolicyStatus.Expired.toLowerCase():
                    tab = CustomerPolicyStatus.Inactive;
                    break;
                default: break;
            }
        } else {
            switch (status.toLowerCase()) {
                case PolicyStatus.Active.toLowerCase():
                case PolicyStatus.Issued.toLowerCase():
                    tab = PolicyStatus.Current;
                    break;
                case PolicyStatus.Cancelled.toLowerCase():
                    tab = PolicyStatus.Cancelled;
                    break;
                case PolicyStatus.Expired.toLowerCase():
                    tab = PolicyStatus.Expired;
                    break;
                default: break;
            }
        }

        return tab;
    }

    private static getParentPropertyHeaders(premiumResult: PremiumResult, parentProperty: string): Array<string> {
        let keys: Array<string> = Object.keys(premiumResult);
        return keys.filter((item: any) => item.includes(parentProperty)).sort();
    }
}
