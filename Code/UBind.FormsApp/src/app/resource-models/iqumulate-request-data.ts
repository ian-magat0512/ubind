/**
 * The data model to be sent to iqumulate to seed the funding request iframe.
 */
export interface IQumulateRequestData {
    general: IQumulateRequestGeneralData;
    introducer: IQumulateRequestIntroducerData;
    client: IQumulateRequestClientData;
    policies: Array<IQumulateRequestPolicyData>;
    host: IQumulateRequestHostData;
}

/**
 * General data
 */
export interface IQumulateRequestGeneralData {
    region?: IQumulateRegion;
    firstInstalmentDate?: string; // documentation provided this spelling
    paymentFrequency?: IQumulatePaymentFrequency;
    numberOfInstalments?: number;
    commissionRate?: number;
    settlementDays?: number;
    paymentMethod: IQumulatePaymentMethod;
}

/**
 * Client data
 */
export interface IQumulateRequestClientData {
    legalName: string;
    tradingName?: string;
    entityType?: string; // ABR Entity Type of the client
    abn?: string;
    industry?: string;
    streetAddress: IQumulateAddress;
    postalAddress?: IQumulateAddress;
    mobileNumber?: string;
    telephoneNumber?: string;
    faxNumber?: string;
    email: string;
    title?: string;
    firstName: string;
    lastName?: string;
    introducerClientReference?: string;
    borrowers: Array<IQumulateBorrower>;
}

/**
 * Introducer data
 */
export interface IQumulateRequestIntroducerData {
    affinitySchemeCode: string;
    introducerContactEmail?: string;
}

/**
 * Policies data
 */
export interface IQumulateRequestPolicyData {
    policyNumber: string;
    invoiceNumber?: string;
    policyClassCode: string;
    policyUnderwriterCode: string;
    policyInceptionDate?: string;
    policyExpiryDate: string;
    policyAmount: string;
    // eslint-disable-next-line @typescript-eslint/naming-convention
    DEFTReferenceNumber?: string;
}

/**
 * Host data
 */
export interface IQumulateRequestHostData {
    host: string;
    origin: string;
    protocol: string;
}

/**
 * borrower data
 */
export interface IQumulateBorrower {
    firstName: string;
    lastName: string;
    // eslint-disable-next-line @typescript-eslint/naming-convention
    DOB: string;
    driverLicence?: string;
}

/**
 * An address which is compatible with the IQumulate API
 */
export interface IQumulateAddress {
    streetLine1: string;
    streetLine2?: string;
    suburb: string;
    state: string;
    postCode: string;
}

export enum IQumulatePaymentFrequency {
    Monthly = 'M',
    Quarterly = 'Q'
}

export enum IQumulateRegion {
    Australia = 'AU',
    NewZealand = 'NZ',
    None = ''
}

export enum IQumulatePaymentMethod {
    CreditCardOnly = 'CC',
    CreditDebitCard = 'CCDD',
    Either = 'either'
}
