import { Component, Injector, ElementRef } from '@angular/core';
import { scrollbarStyle } from '@assets/scrollbar';
import { EventService } from '@app/services/event.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { FormHelper } from '@app/helpers/form.helper';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { ActivatedRoute } from '@angular/router';
import { FinancialTransactionType } from '../../../resource-models/financial-transaction.resource-model';
import { FinancialTransactionApiService } from '@app/services/api/financial-transaction-api.service';
import { CurrencyPipe } from '@app/pipes/currency.pipe';
import {
    CreateEditFinancialTransactionPage,
} from '@app/components/create-edit-financial-transaction/create-edit-financial-transaction.component';
import { RegularExpressions } from '@app/helpers';
import moment from 'moment';
import { checkRefundFutureDateValidator } from '@app/directives/check-refund-future-date.directive';
import { CustomValidators } from '@app/helpers/custom-validators';

/**
 * Export create/edit customer refund page component class
 * This class manage creating and editing of customer refunds.
 */
@Component({
    selector: 'app-create-customer-refund',
    templateUrl: './create-edit-customer-refund.page.html',
    styleUrls: [
        '../../../../assets/css/form-toolbar.scss',
    ],
    styles: [
        scrollbarStyle,
    ],
})

export class CreateEditCustomerRefundPage extends CreateEditFinancialTransactionPage {
    public isEdit: boolean;

    public subjectName: string = 'Refund';

    public constructor(
        protected formBuilder: FormBuilder,
        protected financialTransactionApiService: FinancialTransactionApiService,
        protected route: ActivatedRoute,
        protected navProxy: NavProxyService,
        public formHelper: FormHelper,
        protected sharedLoaderService: SharedLoaderService,
        protected routeHelper: RouteHelper,
        protected sharedAlertService: SharedAlertService,
        protected eventService: EventService,
        protected currencyFormatPipe: CurrencyPipe,
        elementRef: ElementRef,
        injector: Injector,
    ) {
        super(
            formBuilder,
            financialTransactionApiService,
            route,
            navProxy,
            formHelper,
            sharedLoaderService,
            routeHelper,
            sharedAlertService,
            eventService,
            currencyFormatPipe,
            elementRef,
            injector,
            FinancialTransactionType.Refund,
        );
    }

    public buildForm(): FormGroup {
        const currentDateString: string = moment(new Date()).format('YYYY-MM-DD');
        const defaultTransactionTime: string = moment(new Date()).format('HH:mm');

        return new FormGroup({
            'amount': new FormControl('', [Validators.required, CustomValidators.isNumberZeroValued,
                CustomValidators.isNumberPositiveValued, Validators.pattern(RegularExpressions.currencyWithSign)]),
            'refundDate': new FormControl(currentDateString, [Validators.required,
                CustomValidators.isDateNotInFuture]),
            'refundTime': new FormControl(defaultTransactionTime, [Validators.required]),
        }, { validators: checkRefundFutureDateValidator });
    }

    public getTransactionDateString(value: any): string {
        return value.refundDate.substring(0, 10) + 'T' + value.refundTime;
    }
}
