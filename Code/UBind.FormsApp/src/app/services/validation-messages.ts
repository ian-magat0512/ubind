/* eslint-disable prefer-arrow/prefer-arrow-functions */
export function validationMessages(): any {
    return [
        // special
        { name: 'required', message: required },
        { name: 'notValidSelection', message: notValidSelection },
        { name: 'customExpression', message: customExpression },
        { name: 'acceptedDeclaration', message: acceptedDeclaration },
        // types of values
        { name: 'isName', message: isName },
        { name: 'isFullName', message: isFullName },
        { name: 'isNumber', message: isNumber },
        { name: 'isWholeNumber', message: isWholeNumber },
        { name: 'isCurrency', message: isCurrency },
        { name: 'isPercent', message: isPercent },
        { name: 'isDate', message: isDate },
        { name: 'isTime', message: isTime },
        { name: 'isPhoneNumber', message: isPhoneNumber },
        { name: 'isMobileNumber', message: isMobileNumber },
        { name: 'isLandlineNumber', message: isLandlineNumber },
        { name: 'isEmail', message: isEmail },
        { name: 'isURL', message: isURL },
        { name: 'isCreditCardNumber', message: isCreditCardNumber },
        { name: 'isAccountNumber', message: isAccountNumber },
        { name: 'isCCV', message: isCCV },
        { name: 'isBSB', message: isBSB },
        { name: 'isABN', message: isABN },
        { name: 'isACN', message: isACN },
        { name: 'isNumberPlate', message: isNumberPlate },
        { name: 'isPostcode', message: isPostcode },
        { name: 'isExpiryDate', message: isExpiryDate },
        { name: 'isPaymentCardExpiryDate', message: isPaymentCardExpiryDate },
        { name: 'isElementTag', message: isElementTag },
        // comparisons
        { name: 'isEqualTo', message: isEqualTo },
        { name: 'notEqualTo', message: notEqualTo },
        { name: 'maxValue', message: maxValue },
        { name: 'minValue', message: minValue },
        { name: 'maxLength', message: maxLength },
        { name: 'minLength', message: minLength },
        { name: 'maxAge', message: maxAge },
        { name: 'minAge', message: minAge },
        { name: 'greaterThan', message: greaterThan },
        { name: 'greaterThanOrEqual', message: greaterThanOrEqual },
        { name: 'lessThan', message: lessThan },
        { name: 'lessThanOrEqual', message: lessThanOrEqual },
        { name: 'noDecimals', message: noDecimals },
        { name: 'minDecimals', message: minDecimals },
        { name: 'maxDecimals', message: maxDecimals },
        { name: 'matchRegExp', message: matchRegExp },
        // files
        { name: 'notUploading', message: notUploading },
        { name: 'isMimeType', message: isMimeType },
        { name: 'maxFileSize', message: maxFileSize },
        { name: 'minImageWidth', message: minImageWidth },
        { name: 'minImageHeight', message: minImageHeight },
        { name: 'maxImageWidth', message: maxImageWidth },
        { name: 'maxImageHeight', message: maxImageHeight },
    ];
}

export function required(err: any, field: any): string {
    return `You must answer this question`;
}
export function notValidSelection(err: any, field: any): string {
    return `Not a valid selection`;
}
export function customExpression(err: any, field: any): string {
    return `${err.message}`;
}
export function acceptedDeclaration(err: any, field: any): string {
    return `You must understand and accept this declaration in order to proceed`;
}
export function isName(err: any, field: any): string {
    return `This field may only contain letters, spaces, hyphens, apostrophes, commas and period characters`;
}
export function isFullName(err: any, field: any): string {
    return `Not a valid full name`;
}
export function isNumber(err: any, field: any): string {
    return `Not a valid number`;
}
export function isWholeNumber(err: any, field: any): string {
    return `Must be a whole number`;
}
export function isCurrency(err: any, field: any): string {
    return `Not a valid currency value`;
}
export function isPercent(err: any, field: any): string {
    return `Not a valid percentage value`;
}
export function isDate(err: any, field: any): string {
    return `Not a valid date`;
}
export function isTime(err: any, field: any): string {
    return `Not a valid time value`;
}
export function isPhoneNumber(err: any, field: any): string {
    return `Not a valid phone number`;
}
export function isMobileNumber(err: any, field: any): string {
    return `Not a valid mobile number`;
}
export function isLandlineNumber(err: any, field: any): string {
    return `Not a valid landline number`;
}
export function isEmail(err: any, field: any): string {
    return `Not a valid email address`;
}
export function isURL(err: any, field: any): string {
    return `Not a valid web address`;
}
export function isCreditCardNumber(err: any, field: any): string {
    return `Not a valid credit card number`;
}
export function isAccountNumber(err: any, field: any): string {
    return `Not a valid account number`;
}
export function isCCV(err: any, field: any): string {
    return `Not a valid CCV;`;
}
export function isBSB(err: any, field: any): string {
    return `Not a valid BSB`;
}
export function isABN(err: any, field: any): string {
    return `Not a valid ABN`;
}
export function isACN(err: any, field: any): string {
    return `Not a valid ACN`;
}
export function isNumberPlate(err: any, field: any): string {
    return `Not a valid Australian number plate`;
}
export function isPostcode(err: any, field: any): string {
    return `Postcode does not match selected state`;
}
export function isExpiryDate(err: any, field: any): string {
    return `Not a valid expiry date`;
}
export function isPaymentCardExpiryDate(err: any, field: any): string {
    return `Not a valid expiry date`;
}
export function isEqualTo(err: any, field: any): string {
    return `The value must be ${err.requiredValue}`;
}
export function notEqualTo(err: any, field: any): string {
    return `The value must not be ${err.requiredValue}`;
}
export function maxValue(err: any, field: any): string {
    return `Maximum value is ${err.requiredValue}`;
}
export function minValue(err: any, field: any): string {
    return `Minimum value is ${err.requiredValue}`;
}
export function maxLength(err: any, field: any): string {
    return `Must not contain more than ${err.requiredValue} characters`;
}
export function minLength(err: any, field: any): string {
    return `Must contain at least ${err.requiredValue} characters`;
}
export function maxAge(err: any, field: any): string {
    return `Maximum age ${err.requiredValue} years`;
}
export function minAge(err: any, field: any): string {
    return `Minimum age ${err.requiredValue} years`;
}
export function greaterThan(err: any, field: any): string {
    return `Must be greater than ${err.requiredValue}`;
}
export function greaterThanOrEqual(err: any, field: any): string {
    return `Must be greater than or equal to ${err.requiredValue}`;
}
export function lessThan(err: any, field: any): string {
    return `Must be less than ${err.requiredValue}`;
}
export function lessThanOrEqual(err: any, field: any): string {
    return `Must be less than or equal to ${err.requiredValue}`;
}
export function noDecimals(err: any, field: any): string {
    return `Must not contain a decimal value`;
}
export function minDecimals(err: any, field: any): string {
    return `Must contain at least ${err.requiredValue} decimal places`;
}
export function maxDecimals(err: any, field: any): string {
    return `Must not contain more than ${err.requiredValue} decimal places`;
}
export function matchRegExp(err: any, field: any): string {
    return `${err.message}`;
}
export function notUploading(err: any, field: any): string {
    return `File is still uploading`;
}
export function isMimeType(err: any, field: any): string {
    return `File type not allowed`;
}
export function maxFileSize(err: any, field: any): string {
    return `File size must not exceed ${err.requiredValue}`;
}
export function minImageWidth(err: any, field: any): string {
    return `Image must be at least ${err.requiredValue}px wide`;
}
export function minImageHeight(err: any, field: any): string {
    return `Image must be at least ${err.requiredValue}px wide`;
}
export function maxImageWidth(err: any, field: any): string {
    return `Image must not be more than ${err.requiredValue}px wide`;
}
export function maxImageHeight(err: any, field: any): string {
    return `Image must not be more than ${err.requiredValue}px wide`;
}
export function isElementTag(err: any, field: any): string {
    return `Input is invalid`;
}
