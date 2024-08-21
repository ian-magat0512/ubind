import { CalculationState, TriggerState } from "./calculation-result-state";
import { SourceRatingSummaryItem } from "./source-rating-summary-item";
import { TriggerDisplayConfig } from "./trigger-display-config";

/**
 * Common properties for a calculation result (e.g. for a quote OR claim).
 */
export interface CalculationResult {
    oldStateDeprecated: string;
    calculationState: CalculationState;
    triggerState: TriggerState;
    triggers: object;
    trigger: TriggerDisplayConfig;
    payment: Payment;
    funding: object;
    amountPayable: string;
    ratingSummaryItems: Array<SourceRatingSummaryItem>;
    risks: Array<object>;
}

export interface Payment {
    outputVersion?:          number;
    priceComponents?:        PriceComponents;
    instalments?:            Instalments;
    premiumFundingProposal?: PremiumFundingProposal;
    amountPayable?:          string;
    refundComponents?:       Components;
    payableComponents?:      Components;
}

export interface Instalments {
    instalmentsPerYear: string | number;
    instalmentAmount:   string;
}

export interface Components {
    currencyCode:             string;
    basePremium:              string;
    terrorismPremium:         string;
    emergencyServicesLevyNSW: string;
    emergencyServicesLevyTAS: string;
    // eslint-disable-next-line @typescript-eslint/naming-convention
    ESL:                      string;
    premiumGST:               string;
    stampDutyACT:             string;
    stampDutyNSW:             string;
    stampDutyNT:              string;
    stampDutyQLD:             string;
    stampDutySA:              string;
    stampDutyTAS:             string;
    stampDutyVIC:             string;
    stampDutyWA:              string;
    commission:               string;
    commissionGST:            string;
    brokerFee:                string;
    brokerFeeGST:             string;
    underwriterFee:           string;
    underwriterFeeGST:        string;
    roadsideAssistanceFee:    string;
    roadsideAssistanceFeeGST: string;
    policyFee:                string;
    policyFeeGST:             string;
    partnerFee:               string;
    partnerFeeGST:            string;
    administrationFee:        string;
    administrationFeeGST:     string;
    establishmentFee:         string;
    establishmentFeeGST:      string;
    totalGST:                 string;
    interest:                 string;
    interestGST:              string;
    merchantFees:             string;
    merchantFeesGST:          string;
    transactionCosts:         string;
    transactionCostsGST:      string;
    stampDutyTotal:           string;
    totalPremium:             string;
    totalPayable:             string;
}

export interface PremiumFundingProposal {
    amountFunded:            number;
    interestRate:            number;
    paymentFrequency:        string;
    numberOfInstalments:     number;
    applicationFee:          number;
    initialInstalmentAmount: number;
    regularInstalmentAmount: number;
}

export interface PriceComponents {
    basePremium:       string;
    // eslint-disable-next-line @typescript-eslint/naming-convention
    ESL:               string;
    premiumGST:        string;
    stampDutyACT:      string;
    stampDutyNSW:      string;
    stampDutyNT:       string;
    stampDutyQLD:      string;
    stampDutySA:       string;
    stampDutyTAS:      string;
    stampDutyVIC:      string;
    stampDutyWA:       string;
    stampDutyTotal:    string;
    totalPremium:      string;
    commission:        string;
    commissionGST:     string;
    brokerFee:         string;
    brokerFeeGST:      string;
    underwriterFee:    string;
    underwriterFeeGST: string;
    interest:          string;
    merchantFees:      string;
    transactionCosts:  string;
    totalGST:          string;
    totalPayable:      string;
}
