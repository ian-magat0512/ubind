import { AbstractControl, Validators, ValidationErrors } from '@angular/forms';
import { FormControl } from '@angular/forms';
import { RegularExpressions } from '@app/helpers/regular-expressions';
import moment from 'moment';
import { DateHelper } from './date.helper';
import { Liquid } from 'liquidjs';

/**
 * Export custom validators class
 * This class composed of the custom validation functions.
 */
export class CustomValidators {
    public static optionalEmail(control: AbstractControl): ValidationErrors {
        if (!control.value) {
            return null;
        }
        const errors: ValidationErrors = Validators.email(control);
        return errors;
    }

    public static validLiquidTemplate(control: AbstractControl): ValidationErrors {
        if (!control.value) {
            return null;
        } else {
            try {
                const engine: any = new Liquid();
                engine.parse(control.value);
                return null;
            } catch (e) {
                return {
                    invalidLiquidTemplate:
                    {
                        valid: false,
                    },
                };
            }
        }
    }

    public static emailSeparatedBySemiColonPattern(formControl: FormControl): any {
        if (!formControl.value) {
            return null;
        }
        return RegularExpressions.emailSeparatedBySemiColonPattern.test(formControl.value) ? null : {
            invalidEmailSeparatedBySemiColonPattern: {
                valid: false,
            },
        };
    }

    public static emailWithNamePattern(formControl: FormControl): any {
        if (!formControl.value) {
            return null;
        }

        return RegularExpressions.emailWithNamePattern.test(formControl.value) ? null : {
            invalidEmailWithNamePattern: {
                valid: false,
            },
        };
    }

    public static wholeNumber(formControl: FormControl): any {
        if (!formControl.value) {
            return null;
        }
        return RegularExpressions.wholeNumber.test(formControl.value) ? null : {
            inValidWholeNumber: {
                valid: false,
            },
        };
    }

    public static positiveNumber(formControl: FormControl): any {
        if (!formControl.value) {
            return null;
        }
        return RegularExpressions.positiveNumber.test(formControl.value) ? null : {
            inValidPositiveNumber: {
                valid: false,
            },
        };
    }

    public static greaterThanZero(formControl: FormControl): any {
        if (!formControl.value) {
            return null;
        }

        if (!isNaN(Number(formControl.value))) {
            const numberValue: any = Number(formControl.value);
            if (numberValue > 0) {
                return null;
            }
        }

        return {
            invalidValue: {
                valid: false,
            },
        };
    }

    public static naming(formControl: FormControl): any {
        if (!formControl.value) {
            return null;
        }
        return RegularExpressions.naming.test(formControl.value) ? null : {
            invalidNaming: {
                valid: false,
            },
        };
    }

    public static smtpServerHost(formControl: FormControl): any {
        if (!formControl.value) {
            return null;
        }
        return RegularExpressions.smtpServerHost.test(formControl.value) ? null : {
            invalidSmtpServerHost: {
                valid: false,
            },
        };
    }

    public static smtpServerPort(formControl: FormControl): any {
        if (!formControl.value) {
            return null;
        }
        return (formControl.value >= 1 && formControl.value <= 65535) ? null : {
            invalidSmtpServerPort: {
                valid: false,
            },
        };
    }

    public static isValidCurrency(formControl: FormControl): any {
        if (!formControl.value) {
            return null;
        }

        return RegularExpressions.currency.test(formControl.value) ? null : {
            invalidCurrency: {
                valid: false,
            },
        };
    }

    public static incidentDate(formControl: FormControl): any {
        if (!formControl.value) {
            return null;
        }

        return (formControl.value <= moment().format()) ? null : {
            invalidDate: {
                valid: false,
            },
        };
    }

    public static isDateNotInFuture(formControl: FormControl): any {
        if (!formControl.value) {
            return null;
        }

        const dateIsInFuture: boolean = DateHelper.checkDateIfItsInTheFuture(formControl.value);

        if (dateIsInFuture) {
            return {
                futureDateInvalid: {
                    valid: false,
                },
            };
        } else {
            return null;
        }
    }

    public static isNumberPositiveValued(formControl: FormControl): any {
        if (!formControl.value) {
            return null;
        }

        let targetValue: string = '';
        if (formControl.value.startsWith('$')) {
            targetValue = formControl.value.substring(1);
        } else if (formControl.value.startsWith('-$')) {
            targetValue = `${'-'}${formControl.value.substring(2)}`;
        }

        if (!targetValue) {
            return null;
        }
        if (!isNaN(Number(targetValue))) {
            const numberValue: any = Number(targetValue);
            if (numberValue < 0) {
                return {
                    negativeValue: {
                        valid: false,
                    },
                };
            }
        } else {
            // not a number
            return null;
        }
    }

    public static isNumberZeroValued(formControl: FormControl): any {
        if (!formControl.value) {
            return null;
        }

        let targetValue: string = '';
        if (formControl.value.startsWith('$')) {
            targetValue = formControl.value.substring(1);
        }

        if (!targetValue) {
            return null;
        }

        if (!isNaN(Number(targetValue))) {
            const numberValue: number = Number(targetValue);
            if (numberValue === 0) {
                return {
                    zeroValue: {
                        valid: false,
                    },
                };
            }
        } else {
            // not a number
            return null;
        }
    }

    public static isValidFullCurrency(formControl: FormControl): any {
        if (!formControl.value) {
            return null;
        }

        return RegularExpressions.fullCurrency.test(formControl.value) ? null : {
            invalidFullCurrency: {
                valid: false,
            },
        };
    }

    public static isValidTime(formControl: FormControl): any {
        if (!formControl.value) {
            return null;
        }

        if (formControl.value.length > 10) {
            return null;
        }

        if (RegularExpressions.timeAmPm.test(formControl.value) === true ||
            RegularExpressions.timeMilitary.test(formControl.value) === true) {
            return null;
        } else {
            return {
                invalidTime: {
                    valid: false,
                },
            };
        }
    }
}
