/**
 * This file contains the Data transfer objects for 
 * Creating/Editing Financial Transactions such as Payments and Refunds, as well as
 * the enum for the types of financial transactions.
 */
export interface FinancialTransactionCreateModel {
    customerId: string;
    amount: number;
    transactionDateTime: any;
    type: FinancialTransactionType;
}

/**
 * Financial transaction resource model
 */
export interface FinancialTransactionResourceModel extends FinancialTransactionCreateModel {
    id: string;
}

export enum FinancialTransactionType {
    Payment = 0,
    Refund = 1,
}
