import { Component, ElementRef, Injector, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormControl } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { AlertController, LoadingController } from '@ionic/angular';
import { finalize } from 'rxjs/operators';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { QuoteApiService } from '@app/services/api/quote-api.service';
import { QuoteDetailViewModel } from '@app/viewmodels';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { QuoteResourceModel } from '@app/resource-models/quote.resource-model';
import { EventService } from '@app/services/event.service';
import { FormHelper } from '@app/helpers/form.helper';
import { CreateEditPage } from '../../master-create/create-edit.page';
import moment from 'moment';

/**
 * Export set expiry quote page component class.
 * TODO: Write a better class header: setting up the expiry quote.
 */
@Component({
    selector: 'app-set-expiry-quote',
    templateUrl: './set-expiry-quote.page.html',
    styleUrls: [
        './set-expiry-quote.page.scss',
        '../../../../assets/css/form-toolbar.scss',
    ],
})
export class SetExpiryQuotePage extends CreateEditPage<QuoteDetailViewModel> implements OnInit {
    public isEdit: boolean;
    public subjectName: string;
    public title: string = "Set New Expiry Date";
    private quoteId: string;
    private expiryDate: string;
    private expiryTimeOfDay: string;

    public constructor(
        protected eventService: EventService,
        protected loadCtrl: LoadingController,
        protected alertCtrl: AlertController,
        public navProxy: NavProxyService,
        private route: ActivatedRoute,
        private formBuilder: FormBuilder,
        private sharedLoaderService: SharedLoaderService,
        private quoteApiService: QuoteApiService,
        private userPath: UserTypePathHelper,
        public layoutManager: LayoutManagerService,
        private routeHelper: RouteHelper,
        public formHelper: FormHelper,
        public elementRef: ElementRef,
        public injector: Injector,
    ) {
        super(eventService, elementRef, injector, formHelper);
        this.form = this.buildForm();
    }

    public create(value: any): void {
        throw new Error("Method not implemented.");
    }

    public load(): void {
        throw new Error("Method not implemented.");
    }

    public ngOnInit(): void {
        this.title = 'Set Expiry Quote';
        this.quoteId = this.routeHelper.getParam('quoteId');

        this.loadModel();
    }

    private async setupPageMode(): Promise<void> {
        this.detailList = this.model.createDetailsListForQuoteExpiryDateEdit();
    }

    public async loadModel(): Promise<void> {
        const loader: HTMLIonLoadingElement = await this.loadCtrl.create({
            message: 'Please wait..',
        });

        loader.present().then(() => {
            this.quoteApiService.getQuoteDetails(this.quoteId)
                .pipe(finalize(() => loader.dismiss()))
                .subscribe(
                    (dt: any) => {
                        this.model = new QuoteDetailViewModel(dt);
                        let expiryDateTime: string = this.model.getFormBuilderFormattedExpiryDateTimeStamp();
                        if (expiryDateTime) {
                            this.expiryTimeOfDay = this.expiryDate = expiryDateTime;
                            this.form.setValue({
                                expiryDate: this.expiryDate,
                                expiryTimeOfDay: this.expiryTimeOfDay,
                            });
                        }
                        this.setupPageMode();
                        this.isLoading = false;
                    });
        });
    }

    public async userDidTapCloseButton(): Promise<void> {
        if (this.form.value.expiryDate != this.expiryDate) {
            if (!await this.formHelper.confirmExitWithUnsavedChanges()) {
                return;
            }
        }
        this.returnToPrevious();
    }

    public userDidTapSaveButton(value: any): void {
        if (!this.form.valid) {
            return;
        }

        if (this.model && this.model.id) {
            this.update(value);
        }
    }

    protected buildForm(): FormGroup {
        const currentDateString: string = moment(new Date()).format('YYYY-MM-DD');
        const defaultTransactionTime: string = moment(new Date()).format('HH:mm');

        return new FormGroup({
            'expiryDate': new FormControl(currentDateString, [Validators.required]),
            'expiryTimeOfDay': new FormControl(defaultTransactionTime, [Validators.required]),
        });
    }

    public async update(value: any): Promise<void> {
        let dateTime: Date = this.convertControlValueToDateTime(value.expiryDate, value.expiryTimeOfDay);

        await this.sharedLoaderService.present("Please wait...");

        this.quoteApiService.setExpiryDateTime(this.quoteId, dateTime)
            .pipe(finalize(() => this.sharedLoaderService.dismiss()))
            .subscribe(
                (quote: QuoteResourceModel) => {
                    quote.productName = this.model.productName;
                    if (quote.customerDetails && this.model.customerDetails) {
                        quote.customerDetails.displayName = this.model.customerDetails.displayName;
                        quote.customerDetails.id = this.model.customerDetails.id;
                    }
                    this.eventService.getEntityUpdatedSubject('Quote').next(quote);
                    this.returnToPrevious();
                });
    }

    private convertControlValueToDateTime(expiryDate: string, expiryTimeOfDay: string): Date {
        let selectedExpiryDate: string = expiryDate;
        let selectedExpiryTimeOfDay: string = expiryTimeOfDay;
        let isolateDate: string = selectedExpiryDate.split('T')[0];
        let isolateTime: string = selectedExpiryTimeOfDay.split('T').length > 1 ?
            selectedExpiryTimeOfDay.split('T')[1] : selectedExpiryTimeOfDay.split('T')[0];
        let localDateTime: string = isolateDate + "T" + (isolateTime.indexOf('-') > -1 ?
            isolateTime.split('-')[0] : isolateTime);
        return new Date(localDateTime);
    }

    public returnToPrevious(): void {
        this.navProxy.navigateBack([
            this.userPath.quote,
            this.quoteId],
        true,
        {
            queryParams:
                {
                    id: this.quoteId,
                    selectedId: this.quoteId,
                    onPreview: 'quote',
                },
        });
    }
}
