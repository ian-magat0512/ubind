import { AbstractControl, ValidationErrors, ValidatorFn } from "@angular/forms";
import moment from "moment";
import { DateHelper } from "@app/helpers/date.helper";

export const checkPaymentFutureDateValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {

    let transactionDate: any = control.get('paymentDate').value;
    let transactionTime: any = control.get('paymentTime').value;

    const transactionDateTimeString: string = transactionDate.substring(0, 10) + 'T' + transactionTime;

    const isDateToday: boolean = DateHelper.isDateToday(transactionDateTimeString);

    if (isDateToday) {
        const currentTime: number = +moment(new Date()).format('HHmm');
        const targetTime: number = +transactionTime.replace(':', '');

        if (targetTime > currentTime) {
            control.get('paymentTime').setErrors({ futureDateInvalid: true });
        } else {
            control.get('paymentTime').setErrors(null);
        }
    } else {
        control.get('paymentTime').setErrors(null);
    }
    return;
};
