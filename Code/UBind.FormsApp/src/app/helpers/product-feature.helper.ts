/**
 * This helper class provides logic to check if product feature is disabled or not. 
 */
export class ProductFeatureHelper {

    public static isNewBusinessQuoteDisabled(errorCode: string): boolean {
        return  errorCode === 'quote.creation.new.business.quote.type.disabled';
    }

    public static isAdjustmentQuoteDisabled(errorCode: string): boolean {
        return errorCode === 'quote.creation.adjustment.quote.type.disabled';
    }

    public static isRenewalQuoteDisabled(errorCode: string): boolean {
        return errorCode === 'quote.creation.renewal.quote.type.disabled';
    }

    public static isCancellationQuoteDisabled(errorCode: string): boolean {
        return errorCode === 'quote.creation.cancellation.quote.type.disabled';
    }

    public static isProductFeatureDisabled(errorCode: string): boolean {
        return this.isNewBusinessQuoteDisabled(errorCode) ||
         this.isAdjustmentQuoteDisabled(errorCode) ||
         this.isRenewalQuoteDisabled(errorCode) ||
         this.isCancellationQuoteDisabled(errorCode);
    }
}
