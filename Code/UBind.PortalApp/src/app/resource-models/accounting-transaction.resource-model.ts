import { Entity } from '../models/entity';
import { AccountingTransactionHistoryType } from '@app/models/accounting-transaction-history-type';

/**
 * Data transfer object for Accounting Transaction, i.e. Payment, Refund, Invoice, and CreditNote
 */
export interface AccountingTransactionResourceModel extends Entity {
    transactionId: string;
    transactionHistoryType: AccountingTransactionHistoryType;
    transactionDateTime: any;
    amount: number;
    displayedAmount: string;
    referenceNumber: string;
}
