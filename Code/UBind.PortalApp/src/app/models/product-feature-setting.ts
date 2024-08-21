/**
 * Export Product Feature Setting class.
 * This class manage the details of Product feature settings.
 */
export class ProductFeatureSetting {
    public productId: string;
    public areNewBusinessPolicyTransactionsEnabled: boolean;
    public areRenewalPolicyTransactionsEnabled: boolean;
    public areAdjustmentPolicyTransactionsEnabled: boolean;
    public areCancellationPolicyTransactionsEnabled: boolean;
    public isClaimsEnabled: boolean;
    public mustCreateClaimAgainstPolicy: boolean;
    public allowRenewalAfterExpiry: boolean;
    public expiredPolicyRenewalPeriodSeconds: number;
    public refundPolicy: string;
    public periodWhichNoClaimsMade: string;
    public lastNumberOfYearsWhichNoClaimsMade: number;
    public allowQuotesForNewOrganisations: boolean;
    public areNewBusinessQuotesEnabled: boolean;
    public areRenewalQuotesEnabled: boolean;
    public areAdjustmentQuotesEnabled: boolean;
    public areCancellationQuotesEnabled: boolean;
}
