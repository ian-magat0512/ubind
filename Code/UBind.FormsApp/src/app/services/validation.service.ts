import { Injectable } from '@angular/core';
import { FormControl, AbstractControl } from '@angular/forms';
import { ExpressionMethodService } from '@app/expressions/expression-method.service';
import { RegularExpressions } from '@app/helpers/regular-expressions';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { Expression } from '@app/expressions/expression';
import { Field } from '@app/components/fields/field';
import { TextWithExpressions } from '@app/expressions/text-with-expressions';
import { FieldDataType } from '@app/models/field-data-type.enum';
import { ErrorHandlerService } from './error-handler.service';
import { AnyHelper } from '@app/helpers/any.helper';
import { LocaleService } from './locale.service';
import { PhoneNumber, parsePhoneNumberFromString,isValidNumber  } from 'libphonenumber-js';

export type FormControlValidatorFunction = (control: FormControl) => { [key: string]: any } | null;

/**
 * Provides validation functions.
 */
@Injectable()
export class ValidationService {

    protected validatorsPrefix: string = 'ValidationService';

    // temporarily place ccv validation
    protected ccvRegex: any;

    public constructor(
        private expressionDependencies: ExpressionDependencies,
        private expressionMethodService: ExpressionMethodService,
        private errorHandlerService: ErrorHandlerService,
        private localeService: LocaleService,
    ) {
    }

    /* Special */

    /**
     * returns an error if the field is required but the value is not provided.
     * @param requiredExpressionSource 
     * @param hiddenExpressionSource this is ignored, because the hidden state is now taken directly from the field.
     */
    public required(requiredExpressionSource: any, hiddenExpressionSource: any): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            try {
                let field: Field = control['field'];
                if (field == null || field.isHidden() || !requiredExpressionSource) {
                    // we consider it valid if it's not ready or able to be validated
                    return null;
                }
                let expression: Expression = field.getOrCreateRequiredValidationExpression(requiredExpressionSource);
                let required: any = expression.latestResult;
                let fieldValue: any = control && control.value != null ?
                    (typeof (control.value) == 'string' ? control.value.trim() : control.value) : '';
                let result: any = required == true && fieldValue === '' ? { required: true } : null;
                return result;
            } catch (e) {
                this.errorHandlerService.handleError(e);
                return { required: true };
            }
        }; // .bind(this);
    }

    public customExpression(expressionSource: any, errorMessage: string): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            try {
                let field: Field = control['field'];
                if (!field) {
                    // we consider it valid if it's not ready or able to be validated
                    return null;
                }

                let expression: Expression = field.getOrCreateCustomValidationExpression(
                    expressionSource,
                    errorMessage);
                let valid: any = expression.latestResult;
                if (!valid && errorMessage) {
                    errorMessage = new TextWithExpressions(
                        errorMessage,
                        this.expressionDependencies,
                        `custom expression error message for field ${field.fieldPath}`,
                        field.getFixedExpressionArguments(),
                        field.getObservableExpressionArguments(),
                        field.scope).evaluateAndDispose();
                }

                if (errorMessage == null) {
                    errorMessage = "";
                }

                return valid == true ? null : { customExpression: { message: errorMessage } };
            } catch (e) {
                this.errorHandlerService.handleError(e);
                return { customExpression: { message: errorMessage } };
            }
        }; // .bind(this);
    }

    public acceptedDeclaration(): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            return control.value == 'Yes' ? null : { acceptedDeclaration: true };
        };
    }

    /* Types of values */

    public isName(): FormControlValidatorFunction {
        let expressionMethods: ExpressionMethodService = this.expressionMethodService;
        return (control: FormControl): { [key: string]: any } | null => {
            if (AnyHelper.hasNoValue(control.value)) return null;
            return expressionMethods.isName(control.value) ? null : { isName: true };
        };
    }

    public isFullName(): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            if (AnyHelper.hasNoValue(control.value)) return null;
            let regExp: RegExp = /^[a-zA-Z,.'-]{2,30} [a-zA-Z ,.'-]{1,30}[^\s]+$/i;
            return regExp.test(control.value) ? null : { isFullName: true };
        };
    }

    public isNumber(): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            let value: string = '' + control.value;
            if (!value || value == '') return null;
            let numRegEx: RegExp = /[\s]+/;
            let conditions: boolean = !isNaN(<any>value) &&
                !isNaN(parseInt(value, 10)) && !numRegEx.test(value);
            return conditions ? null : { isNumber: true };
        };
    }

    public isWholeNumber(): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            let value: string = '' + control.value;
            if (!value || value == '') return null;
            let regExp: RegExp = /^[0-9]+$/i;
            return regExp.test(value) ? null : { isWholeNumber: true };
        };
    }

    public isDecimalNumber(): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            let value: string = '' + control.value;
            if (!value || value == '') return null;
            return !isNaN(parseFloat(value)) ? null : { isWholeNumber: true };
        };
    }

    public isCurrency(): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            let value: string = '' + control.value;
            if (!value || value == '') return null;
            return !isNaN(<any>value.replace(/[,]/g, '')) ? null : { isCurrency: true };
        };
    }

    public isPercent(): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            if (AnyHelper.hasNoValue(control.value)) return null;
            let regExp: RegExp = /^(\-[0-9]|[0-9]|\.[0-9])\d*(\.\d+)?$/i;
            let value: string = '' + control.value;
            return regExp.test(value.trim()) ? null : { isPercent: true };
        };
    }

    public isDate(): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            if (AnyHelper.hasNoValue(control.value)) return null;
            // eslint-disable-next-line no-useless-escape
            let regExp: RegExp = /^(\d){2}[\/](\d){2}[\/](\d){4}$/;
            if (regExp.test(control.value)) {
                let splitValues: any = control.value.split('/');
                splitValues[1]--; // correct month value
                let dateObject: Date = new Date(splitValues[2], splitValues[1], splitValues[0]);
                let dateString: string = dateObject.toString();
                if (dateString != 'Invalid Date' &&
                    dateString != 'NaN' &&
                    dateObject.getDate() == splitValues[0] &&
                    dateObject.getMonth() == splitValues[1] &&
                    dateObject.getFullYear() == splitValues[2]) {
                    return null;
                }
            }
            return { isDate: true };
        };
    }

    public isTime(): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            if (AnyHelper.hasNoValue(control.value)) return null;
            let regExp: RegExp = /^(\d){2}:(\d){2}[ ]?(AM|PM|Am|Pm|am|pm)?$/;
            if (regExp.test(control.value)) {
                let hours: number = parseInt(control.value.substring(0, 2), 10);
                let minutes: number = parseInt(control.value.split(':')[1].substring(0, 2), 10);
                let amPm: string = control.value.indexOf('AM') != -1 ? 'AM' :
                    control.value.indexOf('PM') != -1 ? 'PM' : null;
                if (hours >= 24 ||
                    hours < 0 ||
                    minutes >= 60 ||
                    minutes < 0 ||
                    (amPm && (hours > 12 || hours == 0))) {
                    return { isTime: true };
                }
                return null;
            }
            return { isTime: true };
        };
    }

    public isPhoneNumber(): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            if (AnyHelper.hasNoValue(control.value)) return null;
            // eslint-disable-next-line no-useless-escape
            let clean: any = control.value.replace(/[\ \(\)\-]/g, '');
            // eslint-disable-next-line no-useless-escape
            let regExp1: RegExp = /^([\+]?61|0)[2|4|3|7|8][\d]{8}$/;
            // eslint-disable-next-line no-useless-escape
            let regExp2: RegExp = /^1[3|8]00[\d]{6}$/;
            // eslint-disable-next-line no-useless-escape
            let regExp3: RegExp = /^13[\d]{4}$/;
            // eslint-disable-next-line no-useless-escape
            let regExp4: RegExp = /^([\+]?61|0)4[\d]{8}$/;
            let isPhoneNumberValid: boolean = this.validatePhoneNumber(clean);

            return (
                regExp1.test(clean) ||
                regExp2.test(clean) ||
                regExp3.test(clean) ||
                regExp4.test(clean) ||
                isPhoneNumberValid
            ) ? null : { isPhoneNumber: true };
        };
    }

    private validatePhoneNumber(phoneNumber: string): boolean {
        if (!isValidNumber(phoneNumber)) {
            return false;
        }

        const phoneNumberUtil: PhoneNumber = parsePhoneNumberFromString(phoneNumber);
        return phoneNumberUtil ? phoneNumberUtil.isValid() : false;
    }

    public isMobileNumber(): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            if (AnyHelper.hasNoValue(control.value)) return null;
            // eslint-disable-next-line no-useless-escape
            let clean: any = control.value.replace(/[\ \(\)\-]/g, '');
            // eslint-disable-next-line no-useless-escape
            let regExp: RegExp = /^([\+]?61|0)4[\d]{8}$/;
            return regExp.test(clean) ? null : { isMobileNumber: true };
        };
    }

    public isLandlineNumber(): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            if (AnyHelper.hasNoValue(control.value)) return null;
            // eslint-disable-next-line no-useless-escape
            let clean: any = control.value.replace(/[\ \(\)\-]/g, '');
            let regExp: RegExp = /^(\+61|0)[2|3|7|8][\d]{8}$/;
            return regExp.test(clean) ? null : { isLandlineNumber: true };
        };
    }

    public isEmail(): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            if (AnyHelper.hasNoValue(control.value)) return null;
            // eslint-disable-next-line no-useless-escape, max-len
            let regExp: RegExp = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
            return regExp.test(control.value) ? null : { isEmail: true };
        };
    }

    public isURL(): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            if (AnyHelper.hasNoValue(control.value)) return null;
            // eslint-disable-next-line max-len
            let regExp: RegExp = /^(?:(?:(?:https?|ftp):)?\/\/)?(?:\S+(?::\S*)?@)?(?:(?!(?:10|127)(?:\.\d{1,3}){3})(?!(?:169\.254|192\.168)(?:\.\d{1,3}){2})(?!172\.(?:1[6-9]|2\d|3[0-1])(?:\.\d{1,3}){2})(?:[1-9]\d?|1\d\d|2[01]\d|22[0-3])(?:\.(?:1?\d{1,2}|2[0-4]\d|25[0-5])){2}(?:\.(?:[1-9]\d?|1\d\d|2[0-4]\d|25[0-4]))|(?:(?:[a-z\u00a1-\uffff0-9]-*)*[a-z\u00a1-\uffff0-9]+)(?:\.(?:[a-z\u00a1-\uffff0-9]-*)*[a-z\u00a1-\uffff0-9]+)*(?:\.(?:[a-z\u00a1-\uffff]{2,})).?)(?::\d{2,5})?(?:[/?#]\S*)?/i;
            return regExp.test(control.value) ? null : { isURL: true };
        };
    }

    public isCreditCardNumber(_this: this = this): FormControlValidatorFunction {
        return (control: FormControl): any => {
            if (AnyHelper.hasNoValue(control.value)) return null;
            let val: any = control.value;
            let sanitized: any = val.replace(/[^0-9]+/g, '');
            // problem with chrome
            let creditCardType: string = _this.expressionMethodService.getCreditCardType(sanitized);

            _this.ccvRegex = _this.expressionMethodService.getCCVValidation(creditCardType);

            const hasLeadingOrTrailingSpaces: boolean = control.value.trim() !== control.value;

            if (creditCardType == null || hasLeadingOrTrailingSpaces) {
                return { isCreditCardNumber: true };
            }
            return null;
            // Note the check digit test doesn't work for some cards. 
            // Until this is resolved we'll let the back end handle incorrect card numbers.
            /*
              var sum = 0;
              for (var i = 0; i < val.length; i++) {
                  var intVal = parseInt(val.substr(i, 1));
                  if (i % 2 == 0) {
                      intVal *= 2;
                      if (intVal > 9) {
                          intVal = 1 + (intVal % 10);
                      }
                  }
                  sum += intVal;
              }
              if (sum % 10 == 0) {
                  return null;
              } else {
                  return { isCreditCardNumber: true };
              }
            */
        };
    }

    public isAccountNumber(): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            if (AnyHelper.hasNoValue(control.value)) return null;
            // eslint-disable-next-line no-useless-escape
            let clean: any = control.value.replace(/[\ \-]/g, '');
            let regExp: RegExp = /[0-9]{6,12}$/;
            return regExp.test(clean) ? null : { isAccountNumber: true };
        };
    }

    public isCCV(_this: this = this): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            if (AnyHelper.hasNoValue(control.value)) return null;

            let regExp: any;

            if (_this.ccvRegex != null) {
                regExp = _this.ccvRegex;
            } else {
                regExp = /^(\d){3,4}$/;
            }

            return regExp.test(control.value) ? null : { isCCV: true };
        };
    }

    public isBSB(): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            if (AnyHelper.hasNoValue(control.value)) return null;
            let regExp: RegExp = /^(\d){3}(-?)(\d){3}$/;
            return regExp.test(control.value) ? null : { isBSB: true };
        };
    }

    public isACN(): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            if (AnyHelper.hasNoValue(control.value)) return null;
            let regExp: RegExp = /^(\d *?){9}$/;
            return regExp.test(control.value) ? null : { isACN: true };
        };
    }

    public isABN(): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            if (AnyHelper.hasNoValue(control.value)) return null;
            let regExp: RegExp = /^(\d *?){11}$/;
            return regExp.test(control.value) ? null : { isABN: true };
        };
    }

    public isNumberPlate(): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            if (AnyHelper.hasNoValue(control.value)) return null;
            let regExp1: RegExp = /^[A-Z]{3}-[A-Z0-9]{3}$/;
            let regExp2: RegExp = /^[a-zA-Z0-9]{1,9}$/;
            return (regExp1.test(control.value) ||
                regExp2.test(control.value)) ? null : { isNumberPlate: true };
        };
    }

    public isPostcode(): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            if (AnyHelper.hasNoValue(control.value)) return null;
            let regExp: RegExp = /^\d{4}$/;
            return regExp.test(control.value) ? null : { isPostcode: true };
        };
    }

    /**
     * @deprecated Please use isPaymentCardExpiryDate() instead 
     * because this method not appropriately named to indicate it's use for a payment card
     */
    public isExpiryDate(_this: this = this): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            if (_this.expressionMethodService.isExpiryDate(control.value)) {
                return null;
            }

            return { isExpiryDate: true };
        };
    }

    public isPaymentCardExpiryDate(_this: this = this): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            if (_this.expressionMethodService.isPaymentCardExpiryDate(control.value)) {
                return null;
            }

            return { paymentCardExpiryDate: true };
        };
    }

    /* Comparisons */

    public isEqualTo(val: any): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            let field: Field = control['field'];
            let formattedValue: string = this.getFormattedValue(field, val);
            return (AnyHelper.hasNoValue(control.value) ||
                control.value == val) ? null : { isEqualTo: { requiredValue: formattedValue } };
        };
    }

    public notEqualTo(val: any): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            let field: Field = control['field'];
            let formattedValue: string = this.getFormattedValue(field, val);
            return (AnyHelper.hasNoValue(control.value) ||
                control.value != val) ? null : { notEqualTo: { requiredValue: formattedValue } };
        };
    }

    private isCurrencyField(field: Field): boolean {
        return field && !field.isHidden()
            && field.dataStoringFieldConfiguration
            && field.dataStoringFieldConfiguration.dataType == FieldDataType.Currency;
    }

    private getFormattedValue(field: Field, val: any): string {
        return this.isCurrencyField(field)
            ? this.expressionDependencies.expressionMethodService.currencyAsString(
                val, null, field.dataStoringFieldConfiguration.currencyCode)
            : val;
    }

    public minValue(val: any): FormControlValidatorFunction {
        return (control: any): { [key: string]: any } | null => {
            let field: Field = control['field'];
            let formattedValue: string = this.getFormattedValue(field, val);
            return AnyHelper.hasNoValue(control.value) || control.value >= val
                ? null
                : { minValue: { requiredValue: formattedValue } };
        };
    }

    public maxValue(val: any): FormControlValidatorFunction {
        return (control: any): { [key: string]: any } | null => {
            let field: Field = control['field'];
            let formattedValue: string = this.getFormattedValue(field, val);
            return (AnyHelper.hasNoValue(control.value) ||
                control.value <= val) ? null : { maxValue: { requiredValue: formattedValue } };
        };
    }

    public minLength(val: any): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            if (AnyHelper.hasNoValue(control.value)) return null;
            let ret: boolean = false;
            if (typeof control.value == 'string') {
                if (control.value.length >= val) {
                    ret = true;
                }
            }
            return ret ? null : { minLength: { requiredValue: val } };
        };
    }

    public maxLength(val: any): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            if (AnyHelper.hasNoValue(control.value)) return null;
            let ret: boolean = false;
            if (typeof control.value == 'string') {
                if (control.value.length <= val) {
                    ret = true;
                }
            }
            return ret ? null : { maxLength: { requiredValue: val } };
        };
    }

    public minAge(val: any, _this: this = this): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            if (AnyHelper.hasNoValue(control.value)) return null;
            let ret: boolean = false;
            if (typeof control.value == 'string') {
                if (_this.expressionMethodService.isDate(control.value)) {
                    let age: number = _this.expressionMethodService.getAgeFromDateOfBirth(
                        _this.expressionMethodService.date(control.value));
                    if (age >= val) {
                        ret = true;
                    }
                }
            }
            return ret ? null : { minAge: { requiredValue: val } };
        };
    }

    public maxAge(val: any, _this: this = this): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            if (AnyHelper.hasNoValue(control.value)) return null;
            let ret: boolean = false;
            if (typeof control.value == 'string') {
                if (_this.expressionMethodService.isDate(control.value)) {
                    let age: number = _this.expressionMethodService.getAgeFromDateOfBirth(
                        _this.expressionMethodService.date(control.value));
                    if (age <= val) {
                        ret = true;
                    }
                }
            }
            return ret ? null : { maxAge: { requiredValue: val } };
        };
    }

    public greaterThan(val: any): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            let field: Field = control['field'];
            let formattedValue: string = this.getFormattedValue(field, val);
            return (AnyHelper.hasNoValue(control.value) ||
                control.value > val) ? null : { greaterThan: { requiredValue: formattedValue } };
        };
    }

    public greaterThanOrEqual(val: any): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            let field: Field = control['field'];
            let formattedValue: string = this.getFormattedValue(field, val);
            return (AnyHelper.hasNoValue(control.value) ||
                control.value >= val) ? null : { greaterThanOrEqual: { requiredValue: formattedValue } };
        };
    }

    public lessThan(val: any): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            let field: Field = control['field'];
            let formattedValue: string = this.getFormattedValue(field, val);
            return (AnyHelper.hasNoValue(control.value) ||
                control.value < val) ? null : { lessThan: { requiredValue: formattedValue } };
        };
    }

    public lessThanOrEqual(val: any): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            let field: Field = control['field'];
            let formattedValue: string = this.getFormattedValue(field, val);
            return (AnyHelper.hasNoValue(control.value) ||
                control.value <= val) ? null : { lessThanOrEqual: { requiredValue: formattedValue } };
        };
    }

    public noDecimals(val: any): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            if (AnyHelper.hasNoValue(control.value)) return null;
            if (control.value && control.value.indexOf('.') != -1) {
                let decimals: any = control.value.split('.')[1];
                if (decimals.length > 0) {
                    return { noDecimals: true };
                }
            }
            return null;
        };
    }

    public minDecimals(val: any): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            if (AnyHelper.hasNoValue(control.value)) return null;
            if (control.value && control.value.indexOf('.') != -1) {
                let decimals: any = control.value.split('.')[1];
                if (decimals.length >= val) {
                    return null;
                }
            }
            return { minDecimals: { requiredValue: val } };
        };
    }

    public maxDecimals(val: any): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            if (AnyHelper.hasNoValue(control.value)) return null;
            if (control.value && control.value.indexOf('.') != -1) {
                let decimals: any = control.value.split('.')[1];
                if (decimals.length > val) {
                    return { maxDecimals: { requiredValue: val } };
                }
            }
            return null;
        };
    }

    public matchRegExp(val: any, message: string): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            if (AnyHelper.hasNoValue(control.value)) return null;
            let regExp: RegExp = new RegExp(val);
            return regExp.test(control.value) ? null : { matchRegExp: { message: message } };
        };
    }

    /* Files */

    public notUploading(val: any, _this: this = this): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            if (control['uploading'] == true) {
                return { notUploading: {} };
            } else {
                return null;
            }
        };
    }

    public isMimeType(val: any, _this: this = this): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            if (AnyHelper.hasNoValue(control.value)) return null;
            let fileProperties: any = _this.expressionMethodService.fileProperties(control.value);
            let allowedMimeTypes: any = val.split('|');
            if (fileProperties && fileProperties.mimeType && allowedMimeTypes.indexOf(fileProperties.mimeType) != -1) {
                return null;
            }
            return { isMimeType: {} };
        };
    }

    public maxFileSize(val: any, _this: this = this): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            if (AnyHelper.hasNoValue(control.value)) return null;
            let fileProperties: any = _this.expressionMethodService.fileProperties(control.value);
            if (fileProperties && fileProperties.fileSizeBytes) {
                let multipliers: any = {
                    // eslint-disable-next-line @typescript-eslint/naming-convention
                    'KB': 1024,
                    // eslint-disable-next-line @typescript-eslint/naming-convention
                    'MB': 1024 * 1024,
                    // eslint-disable-next-line @typescript-eslint/naming-convention
                    'GB': 1024 * 1024 * 1024,
                    // eslint-disable-next-line @typescript-eslint/naming-convention
                    'TB': 1024 * 1024 * 1024 * 1024,
                };
                let regExp: RegExp = /^([0-9]+)(KB|MB|GB|TB)$/;
                if (regExp.test(val)) {
                    let result: RegExpExecArray = regExp.exec(val);
                    let maxFileSizeNumber: number = parseInt(result[1], 10);
                    let maxFileSizeMultiplier: string = result[2];
                    let maxFileSizeBytes: number = maxFileSizeNumber * multipliers[maxFileSizeMultiplier];
                    if (fileProperties.fileSizeBytes <= maxFileSizeBytes) {
                        return null;
                    }
                }
            }
            return { maxFileSize: { requiredValue: val } };
        };
    }

    public minImageWidth(val: any, _this: this = this): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            if (AnyHelper.hasNoValue(control.value)) return null;
            let fileProperties: any = _this.expressionMethodService.fileProperties(control.value);

            if (fileProperties && fileProperties.imageWidth && fileProperties.imageWidth >= val) {
                return null;
            }
            return { minImageWidth: { requiredValue: val } };
        };
    }

    public minImageHeight(val: any, _this: any = this): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            if (AnyHelper.hasNoValue(control.value)) return null;
            let fileProperties: any = _this.expressionMethodService.fileProperties(control.value);
            if (fileProperties && fileProperties.imageHeight && fileProperties.imageHeight >= val) {
                return null;
            }
            return { minImageHeight: { requiredValue: val } };
        };
    }

    public maxImageWidth(val: any, _this: this = this): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            if (AnyHelper.hasNoValue(control.value)) return null;
            let fileProperties: any = _this.expressionMethodService.fileProperties(control.value);
            if (fileProperties && fileProperties.imageWidth && fileProperties.imageWidth <= val) {
                return null;
            }
            return { maxImageWidth: { requiredValue: val } };
        };
    }

    public maxImageHeight(val: any, _this: this = this): FormControlValidatorFunction {
        return (control: FormControl): { [key: string]: any } | null => {
            if (AnyHelper.hasNoValue(control.value)) return null;
            let fileProperties: any = _this.expressionMethodService.fileProperties(control.value);
            if (fileProperties && fileProperties.imageHeight && fileProperties.imageHeight <= val) {
                return null;
            }
            return { maxImageHeight: { requiredValue: val } };
        };
    }

    public elementTagValidator(control: AbstractControl): { [key: string]: boolean } | null {
        const regExp: RegExp = new RegExp(RegularExpressions.htmlElementTag);
        if (control.value != undefined && regExp.test(control.value)) {
            return { 'isElementTag': true };
        }
        return null;
    }
}
