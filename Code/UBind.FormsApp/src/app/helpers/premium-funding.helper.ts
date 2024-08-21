/**
 * This helper class provides logic for retrieving premium funding PDF download link.
 */
export class PremiumFundingHelper {
    private static baseUrl: string = 'https://api.premiumfunding.net.au';

    public static getPremiumFundingContractPdfUrl(contractId: string, pdfKey: string): string {
        return `${this.baseUrl}/contract/${contractId}/PDF/${pdfKey}`;
    }
}
