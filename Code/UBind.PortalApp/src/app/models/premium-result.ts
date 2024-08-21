/**
 * Represents a premium breakdown
 */
export interface PremiumResult {
    basePremium: number;
    terrorismPremium: number;
    esl: number;
    eslNsw: number;
    eslTas: number;
    premiumGst: number;
    stampDuty: number;
    stampDutyTotal: number;
    stampDutyAct: number;
    stampDutyNsw: number;
    stampDutyNt: number;
    stampDutyQld: number;
    stampDutySa: number;
    stampDutyTas: number;
    stampDutyVic: number;
    stampDutyWa: number;
    totalPremium: number;
    commission: number;
    commissionGst: number;
    brokerFee: number;
    brokerFeeGst: number;
    underwriterFee: number;
    underwriterFeeGst: number;
    roadsideAssistanceFee: number;
    roadsideAssistanceFeeGst: number;
    policyFee: number;
    policyFeeGst: number;
    partnerFee: number;
    partnerFeeGst: number;
    administrationFee: number;
    administrationFeeGst: number;
    establishmentFee: number;
    establishmentFeeGst: number;
    interest: number;
    interestGst: number;
    merchantFees: number;
    merchantFeesGst: number;
    transactionCosts: number;
    transactionCostsGst: number;
    serviceFees: number;
    totalGST: number;
    totalPayable: number;
    currencyCode: string;
}
