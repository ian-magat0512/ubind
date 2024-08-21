/**
 * This helper class provides translation of error code.
 */
export class ErrorCodeTranslationHelper {

    // Product feature

    public static isNewBusinessDisabled(errorCode: string): boolean {
        return errorCode === 'quote.creation.new.business.quote.type.disabled';
    }

    public static isAdjustmentDisabled(errorCode: string): boolean {
        return errorCode === 'quote.creation.adjustment.quote.type.disabled';
    }

    public static isRenewalDisabled(errorCode: string): boolean {
        return errorCode === 'quote.creation.renewal.quote.type.disabled';
    }

    public static isCancellationDisabled(errorCode: string): boolean {
        return errorCode === 'quote.creation.cancellation.quote.type.disabled';
    }

    // Adjustment

    public static isExpiredCancellationQuoteExistsWhenAdjusting(errorCode: string): boolean {
        return errorCode === 'policy.expired.cancellation.quote.exists.when.adjusting';
    }

    public static isExpiredAdjustmentQuoteAlreadyExists(errorCode: string): boolean {
        return errorCode === 'policy.expired.adjustment.quote.already.exists';
    }

    public static isAdjustmentQuoteAlreadyExists(errorCode: string): boolean {
        return errorCode === 'policy.adjustment.quote.already.exists';
    }

    public static isExpiredRenewalQuoteExistsWhenAdjusting(errorCode: string): boolean {
        return errorCode === 'policy.expired.renewal.quote.exists.when.adjusting';
    }

    public static isRenewalQuoteExistsWhenAdjusting(errorCode: string): boolean {
        return errorCode === 'policy.renewal.quote.exists.when.adjusting';
    }

    public static isCancellationQuoteExistsWhenAdjusting(errorCode: string): boolean {
        return errorCode === 'policy.cancellation.quote.exists.when.adjusting';
    }

    // Renewal

    public static isExpiredCancellationQuoteExistsWhenRenewing(errorCode: string): boolean {
        return errorCode === 'policy.expired.cancellation.quote.exists.when.renewing';
    }

    public static isExpiredRenewalQuoteAlreadyExists(errorCode: string): boolean {
        return errorCode === 'policy.expired.renewal.quote.already.exists';
    }

    public static isRenewalQuoteAlreadyExists(errorCode: string): boolean {
        return errorCode === 'policy.renewal.quote.already.exists';
    }

    public static isExpiredAdjustmentQuoteExistsWhenRenewing(errorCode: string): boolean {
        return errorCode === 'policy.expired.adjustment.quote.exists.when.renewing';
    }

    public static isAdjustmentQuoteExistsWhenRenewing(errorCode: string): boolean {
        return errorCode === 'policy.adjustment.quote.exists.when.renewing';
    }

    public static isCancellationQuoteExistsWhenRenewing(errorCode: string): boolean {
        return errorCode === 'policy.cancellation.quote.exists.when.renewing';
    }

    // cancellation

    public static isExpiredCancellationQuoteExistsWhenCancelling(errorCode: string): boolean {
        return errorCode === 'policy.expired.cancellation.quote.already.exists';
    }

    public static isCancellationQuoteExistsWhenCancelling(errorCode: string): boolean {
        return errorCode === 'policy.cancellation.quote.already.exists';
    }

    public static handleExpiredAdjustmentQuoteExistsWhenCancelling(errorCode: string): boolean {
        return errorCode === 'policy.expired.adjustment.quote.exists.when.cancelling';
    }

    public static isAdjustmentQuoteExistsWhenCancelling(errorCode: string): boolean {
        return errorCode === 'policy.adjustment.quote.exists.when.cancelling';
    }

    public static isExpiredRenewalQuoteExistsWhenCancelling(errorCode: string): boolean {
        return errorCode === 'policy.expired.renewal.quote.already.exists.when.cancelling';
    }

    public static isRenewalQuoteExistsWhenCancelling(errorCode: string): boolean {
        return errorCode === 'policy.renewal.quote.already.exists.when.cancelling';
    }

    public static isUserPasswordExpired(errorCode: string): boolean {
        return errorCode === 'user.password.expired';
    }

    public static isUserLoginAccountLocked(errorCode: string): boolean {
        return errorCode === 'user.login.account.locked';
    }

    public static isProductReleaseNotFound(errorCode: string): boolean {
        return errorCode === 'release.not.found';
    }


    // Data Table Definition
    public static isDatatableCsvColumnHeaderNameNotFound(errorCode: string): boolean {
        return errorCode === 'data.table.csv.data.column.alias.not.found';
    }

    public static isDatatableCsvDataRequiredColumnNotFound(errorCode: string): boolean {
        return errorCode === 'data.table.csv.data.required.column.not.found';
    }

    public static isDatatableCsvDataColumnValueNotUnique(errorCode: string): boolean {
        return errorCode === 'data.table.csv.data.column.value.not.unique';
    }
}
