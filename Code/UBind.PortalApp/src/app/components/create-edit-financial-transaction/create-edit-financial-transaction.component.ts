import { Injector, ElementRef, OnInit, OnDestroy, Directive } from '@angular/core';
import { EventService } from '@app/services/event.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { FormBuilder } from '@angular/forms';
import { FormHelper } from '@app/helpers/form.helper';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { finalize, takeUntil, debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';
import { FinancialTransactionApiService } from '@app/services/api/financial-transaction-api.service';
import { DetailListItemHelper } from '@app/helpers/detail-list-item.helper';
import { DetailsListItemCardType } from '@app/models/details-list/details-list-item-card-type.enum';
import {
    FinancialTransactionCreateModel, FinancialTransactionResourceModel,
} from '@app/resource-models/financial-transaction.resource-model';
import { CurrencyPipe } from '@app/pipes/currency.pipe';
import { CreateEditPage } from '@app/pages/master-create/create-edit.page';
import { FinancialTransactionType } from '@app/resource-models/financial-transaction.resource-model';
import { DetailsListFormItem } from '@app/models/details-list/details-list-form-item';
import { DetailsListFormTextItem } from '@app/models/details-list/details-list-form-text-item';
import { DetailsListItemCard } from '@app/models/details-list/details-list-item-card';
import { IconLibrary } from '@app/models/icon-library.enum';
import { DetailsListFormDateItem } from '@app/models/details-list/details-list-form-date-item';
import { DetailsListFormTimeItem } from '@app/models/details-list/details-list-form-time-item';

/**
 * Export Create/edit financial transaction page abstract class
 * This financial class is for creating and editing of transactions.
 */
@Directive({ selector: '[appCreateEditFinancialTransaction]' })
export abstract class CreateEditFinancialTransactionPage
    extends CreateEditPage<FinancialTransactionResourceModel> implements OnInit, OnDestroy {

    public transactionDateName: string;
    public transactionTimeName: string;
    public amountIcon: string = '';
    protected financialTransactionId: string;
    public abstract subjectName: any;
    public abstract buildForm(): any;
    public abstract getTransactionDateString(value: any): string;
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
        protected financialTransactionType: FinancialTransactionType,
    ) {
        super(eventService, elementRef, injector, formHelper);

        if (financialTransactionType === FinancialTransactionType.Payment) {
            this.amountIcon = 'add-circle';
            this.transactionDateName = 'paymentDate';
            this.transactionTimeName = 'paymentTime';
        } else {
            this.amountIcon = 'remove-circle';
            this.transactionDateName = 'refundDate';
            this.transactionTimeName = 'refundTime';
        }
    }

    public ngOnInit(): void {
        this.financialTransactionId = this.route.snapshot.paramMap.get('financialTransactionId');
        this.isEdit = this.financialTransactionId != null;
        super.ngOnInit();
        this.form = this.buildForm();
        this.destroyed = new Subject<void>();
        if (!this.isEdit) {
            let details: Array<DetailsListFormItem> = [];
            const icons: typeof DetailListItemHelper.detailListItemIconMap
                = DetailListItemHelper.detailListItemIconMap;
            const detailsCard: DetailsListItemCard
                = new DetailsListItemCard(DetailsListItemCardType.Details, 'Details');
            details.push(DetailsListFormTextItem.create(
                detailsCard,
                'amount',
                null)
                .withIcon(this.amountIcon, IconLibrary.IonicV4));

            const datesCard: DetailsListItemCard
                = new DetailsListItemCard(DetailsListItemCardType.Dates, 'Dates');
            details.push(DetailsListFormDateItem.create(
                datesCard,
                this.transactionDateName,
                null,
            )
                .withIcon(icons.calendar, IconLibrary.IonicV4));
            details.push(DetailsListFormTimeItem.create(
                datesCard,
                this.transactionTimeName,
                null,
            ));
            this.detailList = details;
            this.isLoading = false;
        }

        this.form.valueChanges
            .pipe(
                debounceTime(2000),
                distinctUntilChanged(),
            )
            .subscribe((field: any) => {
                if (!field.amount?.ValidationErrors) {
                    let amount: any = field.amount;
                    if (field.amount.startsWith('$')) {
                        amount = field.amount.substring(1);
                    }
                    this.form.patchValue({ amount: this.convertToCurrency(amount) }, { emitEvent: false });
                }
            });
    }

    private convertToCurrency(amount: any): any {
        try {
            return this.currencyFormatPipe.transform(amount.replace(',', ''));
        } catch (ex) {
            console.log(ex);
            return amount;
        }
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    public load(): void {
    }

    public async create(value: any): Promise<void> {
        const transactionDateTimeString: string = this.getTransactionDateString(value);

        const transactionDateTime: Date = new Date(transactionDateTimeString);

        let amount: string = value.amount;
        if (value.amount.startsWith('$')) {
            amount = value.amount.substring(1);
        }
        const transactionModel: FinancialTransactionCreateModel = {
            customerId: this.routeHelper.getParam('customerId'),
            amount: +amount.split(',').join(''),
            transactionDateTime,
            type: this.financialTransactionType,
        };

        this.sharedLoaderService.presentWait().then(() => {
            this.financialTransactionApiService.createFinancialTransaction(transactionModel)
                .pipe(
                    finalize(() => this.sharedLoaderService.dismiss()),
                    takeUntil(this.destroyed),
                )
                .subscribe((res: FinancialTransactionResourceModel) => {
                    this.sharedAlertService.showToast(`${this.subjectName} ${res} was created`);
                    this.returnToPrevious();
                });
        });
    }

    public async update(value: any): Promise<void> {
    }

    public returnToPrevious(): void {
        this.navProxy.navigateBack(['customer', this.routeHelper.getParam('customerId'), 'Transactions']);
    }
}
