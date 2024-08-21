import { QuoteType } from "@app/models/quote-type.enum";

/**
 * What comes back when you load a quote
 */
export interface QuoteApplicationModel {
    quoteId: string;
    policyId?: string;
    quoteReference: string;
    quoteVersion?: number;
    formDataId?: string;
    calculationResultId?: string;
    priceBreakdown: any;
    refundBreakdonw: any;
    formModel: any;
    calculationResult: any;
    customerId?: string;
    customerHasAccount?: boolean;
    policyNumber?: string;
    quoteType: QuoteType;
    workflowStep: string;
    quoteState: string;
    amountPayable: string;
    isTestData: boolean;
    currentUser: any;
    premiumFundingProposal: any;
    hadCustomerOnCreation: boolean;
    productReleaseId: string;
}
