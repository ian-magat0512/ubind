import { AbstractControl, ValidationErrors, ValidatorFn } from "@angular/forms";
import moment from "moment";
import { DateHelper } from "@app/helpers/date.helper";

export const checkRefundFutureDateValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {

    let transactionDate: any = control.get('refundDate').value;
    let transactionTime: any = control.get('refundTime').value;

    const transactionDateTimeString: string = transactionDate.substring(0, 10) + 'T' + transactionTime;

    const isDateToday: boolean = DateHelper.isDateToday(transactionDateTimeString);

    if (isDateToday) {
        const currentTime: number = +moment(new Date()).format('HHmm');
        const targetTime: number = +transactionTime.replace(':', '');

        if (targetTime > currentTime) {
            control.get('refundTime').setErrors({ futureDateInvalid: true });
        } else {
            control.get('refundTime').setErrors(null);
        }
    } else {
        control.get('refundTime').setErrors(null);
    }
    return;
};
