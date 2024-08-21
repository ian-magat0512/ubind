import { EntityViewModel } from './entity.viewmodel';
import { LocalDateHelper } from '../helpers';
import { GroupedEntityViewModel } from './grouped-entity.viewmodel';
import { SortDirection, SortedEntityViewModel } from '@app/viewmodels/sorted-entity.viewmodel';
import { AccountingTransactionResourceModel } from '../resource-models/accounting-transaction.resource-model';
import { AccountingTransactionHistoryType } from '../models/accounting-transaction-history-type';

/**
 *  View model for displaying Accounting transaction history item which includes Payments, Refunds
 *  Credit Notes, and Invoices
 */
export class AccountingTransactionHistoryItemViewModel
implements EntityViewModel, GroupedEntityViewModel, SortedEntityViewModel {
    public amount: number;
    public displayedAmount: string;
    public createdDate: string;
    public transactionDate: string;
    public transactionHistoryType: AccountingTransactionHistoryType;
    public referenceNumber: string;
    public id: string;
    public title: string;
    public type: AccountingTransactionHistoryType;
    public groupByValue: string;
    public sortByValue: string;
    public sortDirection: SortDirection;
    public deleteFromList: boolean = false;
    public iconName: string;
    public createdTime: string;

    public constructor(transaction: AccountingTransactionResourceModel) {
        this.id = transaction.id;
        this.referenceNumber = transaction.referenceNumber;
        this.amount = transaction.amount;
        this.displayedAmount = transaction.displayedAmount;
        this.transactionDate = LocalDateHelper.toLocalDateFormattedRelativeToNow(transaction.transactionDateTime);
        this.type = transaction.transactionHistoryType;
        this.groupByValue = LocalDateHelper.toLocalDate(transaction.transactionDateTime);
        this.sortByValue = transaction.transactionDateTime;
        this.sortDirection = SortDirection.Descending;
        if (this.type === AccountingTransactionHistoryType.Invoice) {
            this.iconName = 'add-circle-outline';
            this.title = 'Invoice';
        } else if (this.type === AccountingTransactionHistoryType.CreditNote) {
            this.iconName = 'remove-circle-outline';
            this.title = 'Credit Note';
        } else if (this.type === AccountingTransactionHistoryType.Payment) {
            this.iconName = 'add-circle';
            this.title = 'Payment';
        } else if (this.type === AccountingTransactionHistoryType.Refund) {
            this.iconName = 'remove-circle';
            this.title = 'Refund';
        }
    }

    public setGroupByValue(
        accountingTransactionList: Array<AccountingTransactionHistoryItemViewModel>,
        groupBy: string,
    ): Array<AccountingTransactionHistoryItemViewModel> {
        // Not implemented for this entity view model, using default Group By value.
        return accountingTransactionList;
    }

    public setSortOptions(
        accountingTransactionList: Array<AccountingTransactionHistoryItemViewModel>,
        sortBy: string,
        sortDirection: SortDirection,
    ): Array<AccountingTransactionHistoryItemViewModel> {
        // Not implemented for this entity view model, using default Sort By and Sort Direction value.
        return accountingTransactionList;
    }
}
