import { Component, Injector, ElementRef, OnDestroy, OnInit } from '@angular/core';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { FormBuilder, Validators, AbstractControl, FormArray, FormControl, FormGroup } from '@angular/forms';
import { ProductApiService } from '@app/services/api/product-api.service';
import { ProductResourceModel } from '@app/resource-models/product.resource-model';
import { ReportApiService } from '@app/services/api/report-api.service';
import { AlertController } from '@ionic/angular';
import { Router } from '@angular/router';
import { ReportCreateModel, ReportResourceModel } from '@app/resource-models/report.resource-model';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { EventService } from '@app/services/event.service';
import { FormHelper } from '@app/helpers/form.helper';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { finalize, takeUntil } from 'rxjs/operators';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { ReportDetailViewModel } from '@app/viewmodels/report-detail.viewmodel';
import { CreateEditPage } from '@app/pages/master-create/create-edit.page';
import { Subject, Subscription } from 'rxjs';
import { AppConfigService } from '@app/services/app-config.service';
import { AppConfig } from '@app/models/app-config';
import { FormValidatorHelper } from '@app/helpers/form-validator.helper';
import { ValidationMessages } from '@app/models/validation-messages';

/**
 * Export create/edit report page component class.
 * This class manage creation and editing of reports.
 */
@Component({
    selector: 'app-create-edit-report',
    templateUrl: './create-edit-report.page.html',
    styleUrls: [
        './create-edit-report.page.scss',
        '../../../../assets/css/form-toolbar.scss',
    ],
})
export class CreateEditReportPage extends CreateEditPage<ReportResourceModel> implements OnInit, OnDestroy {
    public isEdit: boolean;
    public subjectName: string = "Report";

    public filenameInitialContent: string = 'filename-{{Report.FromDate}}-{{Report.ToDate}}.csv';
    public bodyInitialContent: string = `TransactionType,ProductName,CreationDate,CreationTime,` +
        `InceptionDate,InceptionTime,AdjustmentDate,` +
        `AdjustmentTime,PolicyNumber,QuoteReference,InvoiceNumber,CreditNoteNumber,` +
        `CustomerFullName,CustomerEmail,RatingState,BasePremium,Esl,` +
        `PremiumGst,StampDutyAct,StampDutyNsw,StampDutyNt,StampDutyQld,` +
        `StampDutySa,StampDutyTas,StampDutyVic,StampDutyWa,` +
        `StampDutyTotal,TotalPremium,Commission,CommissionGst,BrokerFee,BrokerFeeGst,` +
        `UnderwriterFee,UnderwriterFeeGst,TotalGst,TotalPayable \n` +
        `{% for PolicyTransaction in PolicyTransactions %}"{{PolicyTransaction.TransactionType}}",` +
        `"{{PolicyTransaction.ProductName}}",` +
        `"{{PolicyTransaction.CreationDate}}","{{PolicyTransaction.CreationTime}}",` +
        `"{{PolicyTransaction.InceptionDate}}",` +
        `"{{PolicyTransaction.InceptionTime}}","{{PolicyTransaction.AdjustmentDate}}",` +
        `"{{PolicyTransaction.AdjustmentTime}}",` +
        `"{{PolicyTransaction.PolicyNumber}}","{{PolicyTransaction.QuoteReference}}",` +
        `"{{PolicyTransaction.InvoiceNumber}}","{{PolicyTransaction.CreditNoteNumber}}",` +
        `"{{PolicyTransaction.CustomerFullName}}","{{PolicyTransaction.CustomerEmail}}",` +
        `"{{PolicyTransaction.RatingState}}",` +
        `"{{PolicyTransaction.BasePremium}}","{{PolicyTransaction.Esl}}","{{PolicyTransaction.PremiumGst}}",` +
        `"{{PolicyTransaction.StampDutyAct}}","{{PolicyTransaction.StampDutyNsw}}",` +
        `"{{PolicyTransaction.StampDutyNt}}","{{PolicyTransaction.StampDutyQld}}",` +
        `"{{PolicyTransaction.StampDutySa}}","{{PolicyTransaction.StampDutyTas}}",` +
        `"{{PolicyTransaction.StampDutyVic}}","{{PolicyTransaction.StampDutyWa}}",` +
        `"{{PolicyTransaction.StampDutyTotal}}","{{PolicyTransaction.TotalPremium}}",` +
        `"{{PolicyTransaction.Commission}}","{{PolicyTransaction.CommissionGst}}",` +
        `"{{PolicyTransaction.BrokerFee}}","{{PolicyTransaction.BrokerFeeGst}}",` +
        `"{{PolicyTransaction.UnderwriterFee}}","{{PolicyTransaction.UnderwriterFeeGst}}",` +
        `"{{PolicyTransaction.TotalGst}}","{{PolicyTransaction.TotalPayable}}" \n` +
        `{% endfor %} `;

    public products: Array<ProductResourceModel> = [];
    public sourceData: any = [
        { id: 1, name: 'New Business Transactions', value: 'New Business' },
        { id: 2, name: 'Renewal Transactions', value: 'Renewal' },
        { id: 3, name: 'Adjustment Transactions', value: 'Adjustment' },
        { id: 4, name: 'Cancellation Transactions', value: 'Cancellation' },
        { id: 5, name: 'Quotes', value: 'Quote' },
        { id: 6, name: 'System Emails', value: 'System Email' },
        { id: 7, name: 'Product Emails', value: 'Product Email' },
        { id: 8, name: 'Claims', value: 'Claim' },
    ];

    public tenantId: string;
    protected tenantAlias: string;
    public reportId: string;

    public constructor(
        public navProxy: NavProxyService,
        protected formBuilder: FormBuilder,
        private productApiService: ProductApiService,
        protected reportApiService: ReportApiService,
        protected alertCtrl: AlertController,
        protected eventService: EventService,
        private routeHelper: RouteHelper,
        protected router: Router,
        public layoutManager: LayoutManagerService,
        elementRef: ElementRef,
        injector: Injector,
        public sharedLoaderService: SharedLoaderService,
        public sharedAlertService: SharedAlertService,
        public formHelper: FormHelper,
        appConfigService: AppConfigService,
    ) {
        super(eventService, elementRef, injector, formHelper);
        this.form = this.buildForm();
        appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.tenantId = appConfig.portal.tenantId;
        });
    }

    public ngOnInit(): void {
        this.reportId = this.routeHelper.getParam('reportId');
        this.isEdit = this.reportId != null;
        super.ngOnInit();
        this.destroyed = new Subject<void>();
        this.tenantAlias = this.routeHelper.getParam('portalTenantAlias');
        if (this.isEdit) {
            this.load();
        } else {
            this.queryProducts();
        }
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    public load(): void {
        this.isLoading = true;
        this.reportApiService.getById(this.reportId)
            .pipe(takeUntil(this.destroyed))
            .subscribe(
                (report: ReportResourceModel) => {
                    this.model = report;
                    this.form.controls.name.setValue(report.name);
                    this.form.controls.description.setValue(report.description);
                    this.form.controls.mimeType.setValue(report.mimeType);
                    this.form.controls.filename.setValue(report.filename);
                    this.form.controls.textBody.setValue(report.body);
                    this.queryProducts(this.model.products, this.model.sourceData.split(','));
                });
    }

    private queryProducts(
        selectedProducts: Array<ProductResourceModel> = null,
        selectedSourceData: Array<string> = null,
    ): void {
        this.productApiService.getProductsByTenantId(this.tenantId)
            .pipe(
                takeUntil(this.destroyed),
                finalize(() => this.isLoading = false),
            )
            .subscribe((p: Array<ProductResourceModel>) => {
                this.products = p;
                this.addProducts(selectedProducts);
                this.addSourceData(selectedSourceData);

                this.detailList = ReportDetailViewModel.createDetailsListForEdit();
            });
    }

    protected buildForm(): FormGroup {
        return this.formBuilder.group({
            name: ['', FormValidatorHelper.alphaNumericValidator(true, ValidationMessages.errorKey.Name)],
            description: ['', FormValidatorHelper.alphaNumericValidator(false, ValidationMessages.errorKey.Alias)],
            products: this.formBuilder.array([], [this.checkboxValidation]),
            sourceData: this.formBuilder.array([], [this.checkboxValidation]),
            mimeType: ['text/csv', [Validators.required, this.mimeTypeValidation]],
            filename: [this.filenameInitialContent, [Validators.required, this.liquidTemplateValidation]],
            textBody: [this.bodyInitialContent, [Validators.required, this.liquidTemplateValidation]],
            disabled: false,
        });
    }

    public async save(value: any): Promise<void> {
        if (!this.form.valid) {
            return;
        }

        const selectedSourceData: any = value.sourceData
            .map((v: any, i: any) => v ? this.sourceData[i].id : null)
            .filter((v: any) => v !== null);

        const selectedProducts: any = value.products
            .map((v: any, i: any) => v ? this.products[i].id : null)
            .filter((v: any) => v !== null);

        const report: ReportCreateModel = {
            id: this.isEdit ? this.model.id : '',
            tenantId: this.tenantId,
            name: value.name,
            createdDateTime: value.createdDateTime,
            description: value.description,
            productIds: selectedProducts,
            sourceData: this.sourceData.filter((v: any, i: number) =>
                selectedSourceData.indexOf(i + 1) > -1).map((ss: any) => ss.value).join(','),
            mimeType: value.mimeType,
            filename: value.filename,
            body: value.textBody,
            isDeleted: false,
        };

        await this.sharedLoaderService.presentWithDelay();

        if (this.isEdit) {
            this.update(report);
        } else {
            this.create(report);
        }
    }

    public update(report: ReportCreateModel): void {
        const subscription: Subscription = this.reportApiService.update(report.id, report)
            .pipe(finalize(() => {
                this.sharedLoaderService.dismiss();
                subscription.unsubscribe();
            }))
            .subscribe(
                (newReport: ReportResourceModel) => {
                    this.eventService.getEntityUpdatedSubject('Report').next(newReport);
                    this.sharedAlertService.showToast(`Report details for ${newReport.name} were saved`);
                    this.navProxy.navigateBack(['report', newReport.id]);
                });
    }

    public create(report: ReportCreateModel): void {
        this.reportApiService.create(report)
            .pipe(
                takeUntil(this.destroyed),
                finalize(() => this.sharedLoaderService.dismiss()))
            .subscribe(
                (newReport: ReportResourceModel) => {
                    this.eventService.getEntityCreatedSubject('Report').next(newReport);
                    this.form.controls['name'].reset();
                    this.form.controls['description'].reset();
                    this.sharedAlertService.showToast(`Report ${newReport.name} was created`);
                    this.navProxy.navigateBack(['report', newReport.id]);
                });
    }

    public returnToPrevious(): void {
        this.navProxy.navigateBackOne();
    }

    private addProducts(selectedProducts: Array<ProductResourceModel> = null): void {
        const items: Array<any> = [];
        this.products.map((p: ProductResourceModel, i: number) => {
            let isChecked: boolean = i === 0;
            if (selectedProducts != null) {
                isChecked = selectedProducts.map((p: ProductResourceModel) => p.id).indexOf(p.id) > -1;
            }

            (this.form.get('products') as FormArray)
                .push(new FormControl(isChecked));
            items.push({
                label: p.name,
                value: isChecked.toString(),
            });
        });

        this.fieldOptions.push({ name: "products", options: items, type: "checkbox" });

        // i guess this is the worst implementation of this. 
        // but a massive refactor is already in the works on another ticket.
        this.fieldOptions.push({ name: "textBody", options: [], type: "textarea" });
    }

    private addSourceData(selectedSourceData: Array<string> = null): void {
        const items: Array<any> = [];
        this.sourceData.map((s: any, i: number) => {
            let isChecked: boolean = i == 0;
            if (selectedSourceData != null) {
                isChecked = selectedSourceData.indexOf(s.value) > -1;
            }
            (this.form.get('sourceData') as FormArray).push(new FormControl(isChecked));
            items.push({
                label: s.name,
                value: isChecked.toString(),
            });
        });

        this.fieldOptions.push({ name: "sourceData", options: items, type: "checkbox" });
    }

    protected mimeTypeValidation(control: AbstractControl): { [key: string]: any } {
        const val: string = control.value;

        if (val === '') {
            return null;
        }

        const mimeTypes: Array<string> = ['audio/aac', 'application/x-abiword', 'application/x-freearc',
            'video/x-msvideo', 'application/vnd.amazon.ebook', 'application/octet-stream',
            'image/bmp', 'application/x-bzip', 'application/x-bzip2', 'application/x-csh',
            'text/css', 'text/csv', 'application/msword',
            'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
            'application/vnd.ms-fontobject', 'application/epub+zip', 'application/gzip', 'image/gif',
            'text/html', 'image/vnd.microsoft.icon', 'text/calendar', 'application/java-archive',
            'image/jpeg', 'text/javascript', 'application/json', 'application/ld+json',
            'audio/midi audio/x-midi', 'audio/mpeg', 'video/mpeg', 'application/vnd.apple.installer+xml',
            'application/vnd.oasis.opendocument.presentation', 'application/vnd.oasis.opendocument.spreadsheet',
            'application/vnd.oasis.opendocument.text', 'audio/ogg', 'video/ogg', 'application/ogg',
            'font/otf', 'image/png', 'application/pdf', 'appliction/php', 'application/vnd.ms-powerpoint',
            'application/vnd.openxmlformats-officedocument.presentationml.presentation',
            'application/x-rar-compressed', 'application/rtf', 'application/x-sh', 'image/svg+xml',
            'application/x-shockwave-flash', 'application/x-tar', 'image/tiff', 'video/mp2t', 'font/ttf',
            'text/plain', 'application/vnd.visio', 'audio/wav', 'audio/webm', 'video/webm', 'image/webp',
            'font/woff', 'font/woff2', 'application/xhtml+xml', 'application/vnd.ms-excel',
            'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', 'application/xml ',
            'text/xml', 'application/vnd.mozilla.xul+xml', 'application/zip', 'video/3gpp',
            'audio/3gpp', 'video/3gpp2', 'audio/3gpp2', 'application/x-7z-compressed',
        ];

        if (mimeTypes.indexOf(val) === -1) {
            return { invalidMimeType: 'Please enter a valid mime-type' };
        }

        return null;
    }

    protected liquidTemplateValidation(control: AbstractControl): { [key: string]: any } {
        const val: string = control.value;

        if (val == '') {
            return null;
        }

        if (val.match(/\{\{(.*?)\}\}/g) === null) {
            return { invalidLiquidTemplate: 'Liquid template is required' };
        }

        return null;
    }

    protected checkboxValidation(control: AbstractControl): { [key: string]: any } {
        const val: any = control.value;

        if (val.indexOf(true) === -1) {
            return { checkbox: 'You must select atleast one from these selection/s' };
        }
        return null;
    }
}
