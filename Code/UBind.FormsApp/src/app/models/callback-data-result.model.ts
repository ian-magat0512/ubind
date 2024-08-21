import { ApplicationStatus } from "./application-status.enum";

export interface CallbackDataResult {
    state:                  ApplicationStatus;
    quoteId:                string;
    quoteReference:         string;
    policyId:               string;
    policyNumber:           string;
    customerId:             string;
    customerAccountEmail:   string;
    productId:              string;
    productAlias:           string;
    organisationId:         string;
    organisationAlias:      string;
    tenantId:               string;
    tenantAlias:            string;
    environment:            string;
    workflowStep:           string;
    priceSummary:           PriceSummary;
}

export interface PriceSummary {
    currencyCode:           string;
    premium?:                number;
    terrorismPremium?:       number;
    emergencyServicesLevy?:  number;
    stampDuty?:              number;
    serviceFees?:            number;
    paymentFees?:            number;
    goodsAndServicesTax?:    number;
    totalPayable?:           number;
}
