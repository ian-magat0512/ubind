import { ValidatorFn, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { RegularExpressions } from './regular-expressions';
import { CustomValidators } from './custom-validators';
import { isUri } from 'valid-url';
import { Liquid } from 'liquidjs';
import { JsonValidator } from './json-validator';
import { ValidationMessages } from '@app/models/validation-messages';
import { PhoneNumber, parsePhoneNumberFromString, isValidNumber  } from 'libphonenumber-js';

/**
 * Export form validator helper class.
 * This class is help form validations.
 */
export class FormValidatorHelper {
    public static getValidator(name: string, type: string): Array<ValidatorFn> {
        if (name.indexOf("mobile") > -1 && type == "tel") {
            return this.mobilePhoneValidator();
        }

        if (name.indexOf("mobile") < 0 && type == "tel") {
            return this.phoneValidator();
        }

        if (type == "email") {
            return this.emailValidator();
        }

    }

    public static nameValidator(isRequired: boolean = false): Array<ValidatorFn> {
        let nameValidator: ValidatorFn = this.validatorPattern(
            RegularExpressions.naming,
            ValidationMessages.errorKey.Name);
        return isRequired ? [Validators.required, nameValidator] : [nameValidator];
    }

    public static companyNameValidator(isRequired: boolean = false): Array<ValidatorFn> {
        let companyNameValidator: ValidatorFn = this.validatorPattern(
            RegularExpressions.company,
            ValidationMessages.errorKey.Company);
        return isRequired ? [Validators.required, companyNameValidator] : [companyNameValidator];
    }

    public static phoneValidator(isRequired: boolean = false): Array<ValidatorFn> {
        let validator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
            if (control.value && !this.phoneNumber(control.value)) {
                return { invalidPhone: true };
            }
            return null;
        };
        return isRequired ? [Validators.required, validator] : [validator];
    }

    private static validatePhoneNumber(phoneNumber: string): boolean {
        if (!isValidNumber(phoneNumber)) {
            return false;
        }

        const phoneNumberUtil: PhoneNumber = parsePhoneNumberFromString(phoneNumber);
        return phoneNumberUtil ? phoneNumberUtil.isValid() : false;
    }

    public static mobilePhoneValidator(isRequired: boolean = false): Array<ValidatorFn> {
        let mobilephoneValidator: ValidatorFn = this.validatorPattern(
            RegularExpressions.australianMobilePhoneNumber,
            ValidationMessages.errorKey.Mobile);
        return isRequired ? [Validators.required, mobilephoneValidator] : [mobilephoneValidator];
    }

    public static emailValidator(isRequired: boolean = false): Array<ValidatorFn> {
        return isRequired ? [Validators.required, Validators.email] : [Validators.email];
    }

    public static emailWithNameValidator(isRequired: boolean = false): Array<ValidatorFn> {
        let emailWithValidator: ValidatorFn = this.validatorPattern(
            RegularExpressions.emailWithNamePattern,
            ValidationMessages.errorKey.Email);
        return isRequired ? [Validators.required, emailWithValidator] : [emailWithValidator];
    }

    public static emailSeparatedBySemiColonValidator(isRequired: boolean = false): Array<ValidatorFn> {
        let emailSeparatedBySemiColon: ValidatorFn = this.validatorPattern(
            RegularExpressions.emailSeparatedBySemiColonPattern,
            ValidationMessages.errorKey.EmailSeparatedBySemiColon);
        return isRequired ? [Validators.required, emailSeparatedBySemiColon] : [emailSeparatedBySemiColon];
    }

    public static smtpServerHostValidator(isRequired: boolean = false): Array<ValidatorFn> {
        let smtpServerHost: ValidatorFn = this.validatorPattern(
            RegularExpressions.smtpServerHost,
            ValidationMessages.errorKey.SmtpServerHost);
        return isRequired ? [Validators.required, smtpServerHost] : [smtpServerHost];
    }

    public static smtpServerPortValidator(isRequired: boolean = false): Array<ValidatorFn> {
        let smtpServerPort: ValidatorFn = this.validatorPattern(
            RegularExpressions.smtpServerPort,
            ValidationMessages.errorKey.SmtpServerPort);
        return isRequired ? [Validators.required, smtpServerPort] : [smtpServerPort];
    }

    public static entityNameValidator(isRequired: boolean = false): Array<ValidatorFn> {
        let entityName: ValidatorFn = this.validatorPattern(
            RegularExpressions.entityName,
            ValidationMessages.errorKey.EntityName);
        return isRequired ? [Validators.required, entityName] : [entityName];
    }

    public static aliasValidator(isRequired: boolean = false): Array<ValidatorFn> {
        let alias: ValidatorFn = this.validatorPattern(
            RegularExpressions.alias,
            ValidationMessages.errorKey.Alias);
        return isRequired ? [Validators.required, alias] : [alias];
    }

    public static stylesheetUrlValidator(isRequired: boolean = false): Array<ValidatorFn> {
        let stylesheetUrl: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
            if (!control.value) {
                return null;
            }

            let absoluteUrl: string = control.value;
            if (control.value.toLowerCase().startsWith("/assets")) {
                absoluteUrl = `https://app.ubind.com.au${control.value}`;
            }

            if (!isUri(absoluteUrl)) {
                return { invalidStylesheetUrl: true };
            }
            return null;
        };

        return isRequired ? [Validators.required, stylesheetUrl] : [stylesheetUrl];
    }

    public static domainNameValidator(isRequired: boolean = false): Array<ValidatorFn> {
        let domainName: ValidatorFn = this.validatorPattern(
            RegularExpressions.domainName,
            ValidationMessages.errorKey.DomainName);
        return isRequired ? [Validators.required, domainName] : [domainName];
    }

    public static liquidTemplateValidator(isRequired: boolean = false): Array<ValidatorFn> {
        let liquidTemplate: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
            if (!this.validLiquidTemplate(control)) {
                return { invalidLiquidTemplate: true };
            }
        };
        return isRequired ? [Validators.required, liquidTemplate] : [liquidTemplate];
    }

    public static alphaNumericValidator(isRequired: boolean = false, errorKey: string): Array<ValidatorFn> {
        const startsWithAlphabeticCharacter: RegExp = /^[a-z]/i;

        let alphaNumericWithSpace: ValidatorFn = this.validatorPattern(
            RegularExpressions.alphaNumericWithSpace && startsWithAlphabeticCharacter,
            errorKey);

        return isRequired ? [Validators.required, alphaNumericWithSpace] : [alphaNumericWithSpace];
    }

    public static optionalEmailValidator(isRequired: boolean = false): Array<ValidatorFn> {
        return isRequired ? [Validators.required, CustomValidators.optionalEmail] : [CustomValidators.optionalEmail];
    }

    public static required(): Array<ValidatorFn> {
        return [Validators.required];
    }

    public static customLabel(isRequired: boolean = false): Array<ValidatorFn> {
        let customLabelValidator: ValidatorFn = this.validatorPattern(
            RegularExpressions.customLabel,
            ValidationMessages.errorKey.CustomLabel);
        return isRequired ? [Validators.required, customLabelValidator] : [customLabelValidator];
    }

    public static streetAddress(isRequired: boolean = false): Array<ValidatorFn> {
        let streetAddressValidator: ValidatorFn = this.validatorPattern(
            RegularExpressions.streetAddress,
            ValidationMessages.errorKey.StreetAddress);
        return isRequired ? [Validators.required, streetAddressValidator] : [streetAddressValidator];
    }

    public static addressSuburb(isRequired: boolean = false): Array<ValidatorFn> {
        let suburbValidator: ValidatorFn = this.validatorPattern(
            RegularExpressions.addressSuburb,
            ValidationMessages.errorKey.AddressSuburb);
        return isRequired ? [Validators.required, suburbValidator] : [suburbValidator];
    }

    public static addressPostcode(isRequired: boolean = false): Array<ValidatorFn> {
        let postcodeValidator: ValidatorFn = this.validatorPattern(
            RegularExpressions.addressPostcode,
            ValidationMessages.errorKey.AddressPostcode);
        return isRequired ? [Validators.required, postcodeValidator] : [postcodeValidator];
    }

    public static addressStateAndPostcode(state: string): ValidatorFn {
        let validator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
            if (control.value && !this.postcodeInState(control.value, state, true)) {
                return { invalidAddressStateAndPostcode: true };
            }
            return null;
        };
        return validator;
    }

    public static noWhitespace(control: AbstractControl): any {
        const isWhitespace: boolean = (control.value || '').trim().length === 0;
        return isWhitespace ? { whitespace: `This field is required.` } : null;
    }

    public static phoneNumber(value: string): boolean {
        let clean: string = value.replace(/[\ \(\)\-]/g, '');
        let regExp1: RegExp = /^([\+]?61|0)[2|4|3|7|8][\d]{8}$/;
        let regExp2: RegExp = /^1[3|8]00[\d]{6}$/;
        let regExp3: RegExp = /^13[\d]{4}$/;
        let regExp4: RegExp = /^([\+]?61|0)4[\d]{8}$/;
        let isPhoneNumberValid: boolean = this.validatePhoneNumber(clean);
        return (regExp1.test(clean) ||
            regExp2.test(clean) ||
            regExp3.test(clean) ||
            regExp4.test(clean) ||
            isPhoneNumberValid);
    }

    public static url(isRequired: boolean = false): Array<ValidatorFn> {
        let urlValidator: ValidatorFn = this.validatorPattern(
            RegularExpressions.url,
            ValidationMessages.errorKey.URL);
        return isRequired ? [Validators.required, urlValidator] : [urlValidator];
    }

    public static webUrl(isRequired: boolean = false): Array<ValidatorFn> {
        let urlValidator: ValidatorFn = this.validatorPattern(
            RegularExpressions.webUrl,
            ValidationMessages.errorKey.URL);
        return isRequired ? [Validators.required, urlValidator] : [urlValidator];
    }

    public static x509Certificate(isRequired: boolean = false): Array<ValidatorFn> {
        let x509CertificateValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
            if (control.value) {
                const beginCertificatePos: number = control.value.indexOf('-----BEGIN CERTIFICATE-----');
                if (beginCertificatePos === -1) {
                    return { missingBeginCertificate: true };
                }
                const endCertificatePos: number = control.value.indexOf('-----END CERTIFICATE-----');
                if (endCertificatePos === -1) {
                    return { missingEndCertificate: true };
                }
            }
            return null;
        };
        return isRequired ? [Validators.required, x509CertificateValidator] : [x509CertificateValidator];
    }

    public static validJson(formControl: AbstractControl): ValidationErrors | null {
        if (!formControl.value) {
            return null;
        }
        let isValid: boolean = JsonValidator.isValidJson(formControl.value);
        return isValid ? null : {
            invalidJson: true,
        };
    }

    private static validLiquidTemplate(control: AbstractControl): boolean {
        if (!control.value) {
            return null;
        } else {
            try {
                const engine: any = new Liquid();
                engine.parse(control.value);
                return true;
            } catch (e) {
                return false;
            }
        }
    }

    public static positiveNoneZeroWholeNumberValidator(isRequired: boolean = false): Array<ValidatorFn> {
        let validators: Array<ValidatorFn> = this.createPositiveNonZeroWholeNumberValidator();
        if (isRequired) {
            validators.push(Validators.required);
        }
        return validators;
    }

    public static wholeNumberValidator(isRequired: boolean = false): Array<ValidatorFn> {
        const wholeNumberValidator: ValidatorFn = this.validatorPattern(
            RegularExpressions.wholeNumber,
            ValidationMessages.errorKey.WholeNumber);
        return isRequired ? [Validators.required, wholeNumberValidator] : [wholeNumberValidator];
    }

    private static createPositiveNonZeroWholeNumberValidator(): Array<ValidatorFn> {
        const wholeNumberValidator: ValidatorFn = this.validatorPattern(
            RegularExpressions.postiveWholeNumber,
            ValidationMessages.errorKey.PositiveWholeNumber);
        const isNotZeroValidator: ValidatorFn =
            this.isNotZeroValidator(ValidationMessages.errorKey.PositiveWholeNumber);
        return [wholeNumberValidator, isNotZeroValidator];
    }

    private static isNotZeroValidator(propertyName: string): ValidatorFn {
        const validator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
            if (control.value == 0) {
                let validatorPattern: { [key: string]: any } = {};
                validatorPattern[propertyName] = true;
                return validatorPattern;
            } else {
                return null;
            }
        };

        return validator;
    }

    private static validatorPattern(regExp: RegExp, propertyName: string): ValidatorFn {
        let validator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
            if (!control.value) {
                return null;
            }

            if (regExp.test(control.value)) {
                return null;
            } else {
                let validatorPattern: { [key: string]: any } = {};
                validatorPattern[propertyName] = true;
                return validatorPattern;
            }
        };

        return validator;
    }

    private static postcodeInState(postcode: string, state: string, includePOBoxes: boolean = true): boolean {
        let intPostCode: number = parseInt(postcode, 10);
        let statePostcodes: any = {
            "NSW": [{ "start": 2000, "end": 2599 }, { "start": 2620, "end": 2899 }, { "start": 2921, "end": 2999 }],
            "ACT": [{ "start": 2600, "end": 2619 }, { "start": 2900, "end": 2920 }],
            "VIC": [{ "start": 3000, "end": 3999 }],
            "QLD": [{ "start": 4000, "end": 4999 }],
            "SA": [{ "start": 5000, "end": 5799 }],
            "WA": [{ "start": 6000, "end": 6797 }],
            "TAS": [{ "start": 7000, "end": 7799 }],
            "NT": [{ "start": 800, "end": 899 }],
        };
        let statePOBoxPostcodes: any = {
            "NSW": [{ "start": 1000, "end": 1999 }],
            "ACT": [{ "start": 200, "end": 299 }],
            "VIC": [{ "start": 8000, "end": 8999 }],
            "QLD": [{ "start": 9000, "end": 9999 }],
            "SA": [{ "start": 5800, "end": 5999 }],
            "WA": [{ "start": 6800, "end": 6999 }],
            "TAS": [{ "start": 7800, "end": 7999 }],
            "NT": [{ "start": 900, "end": 999 }],
        };
        let exceptions: any = {
            "4825": ["NT"],
            "0872": ["SA", "WA"],
            "2406": ["QLD"],
            "2611": ["NSW"],
            "2620": ["ACT"],
            "3500": ["NSW"],
            "3585": ["NSW"],
            "3586": ["NSW"],
            "3644": ["NSW"],
            "3691": ["NSW"],
            "3707": ["NSW"],
            "4380": ["NSW"],
            "4377": ["NSW"],
        };
        if (exceptions['' + intPostCode] && exceptions['' + intPostCode].indexOf(state.toUpperCase()) != -1) {
            return true;
        }
        let states: Array<string> = Object.keys(statePostcodes);
        if (states.indexOf(state) != -1) {
            for (let range of statePostcodes[state]) {
                if (range.start <= intPostCode && range.end >= intPostCode) {
                    return true;
                }
            }
            if (includePOBoxes) {
                for (let range of statePOBoxPostcodes[state]) {
                    if (range.start <= intPostCode && range.end >= intPostCode) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
}
