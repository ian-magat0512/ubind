
/**
 * Data model for what can be returned from IQumulate requests
 */
export interface IQumulateResponseData {
    // eslint-disable-next-line @typescript-eslint/naming-convention
    General: IQumulateResponseGeneralData;
    // eslint-disable-next-line @typescript-eslint/naming-convention
    Client: any;
    // eslint-disable-next-line @typescript-eslint/naming-convention
    Policies: any;
    // eslint-disable-next-line @typescript-eslint/naming-convention
    Documents: any;
}

/**
 * General response data
 */
export interface IQumulateResponseGeneralData {
    type: string;
    region: string;
    takeInitialPayment: boolean;
    number: string;
    amountFinanced: number;
    isOverThreshold: boolean;
    flatInterestRate: number;
    totalInterestCharges: number;
    totalAmountRepayable: number;
    paymentFrequency: string;
    numberOfInstalments: number;
    firstInstalmentDate: string;
    lastInstalmentDate: string;
    initialInstalmentAmount: number;
    ongoingInstalmentAmount: number;
    applicationFee: number;
    settlementDays: number;
    responseCode: string;
    responseDescription: IQumulateResponseCode;
}

export enum IQumulateResponseCode {
    Success = "0",
    ErrorOccured = "1",
    ErrorOccuredDuringPaymentProcessing = "2",
    ForcedClose = "3"
}
