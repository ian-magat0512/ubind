import { Component, Injector, ElementRef, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AlertController, LoadingController } from '@ionic/angular';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { scrollbarStyle } from '@assets/scrollbar';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { EventService } from '@app/services/event.service';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { FormHelper } from '@app/helpers/form.helper';
import { TenantService } from '@app/services/tenant.service';
import { ProductService } from '@app/services/product.service';
import { AppConfigService } from '@app/services/app-config.service';
import { AppConfig } from '@app/models/app-config';
import { DkimSettingsApiService } from '@app/services/api/dkim-settings-api.service';
import { DkimSettingsService } from '@app/services/dkim-settings.service';
import { DetailsListFormItem } from '@app/models/details-list/details-list-form-item';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { DkimSettingsResourceModel } from '@app/resource-models/dkim-settings.resource-model';
import { DkimTestEmailResourceModel } from '@app/resource-models/dkim-test-email.resource-model';

/**
 * Export DKIM send Test Email
 * This class is used to manage sending DKIM test email.
 */
@Component({
    selector: 'app-dkim-send-test-email.page',
    templateUrl: './dkim-send-test-email.page.html',
    styleUrls: ['./dkim-send-test-email.page.scss',
        '../../../../assets/css/form-toolbar.scss',
        '../../../../assets/css/edit-form.scss',
    ],
    styles: [scrollbarStyle],
})
export class SendDkimTestEmailPage extends DetailPage implements OnInit {
    public form: FormGroup;
    public title: string = "Send DKIM Test Email";
    public isLoading: boolean = false;
    private tenantId: string;
    private contextTenantAlias: string;
    private organisationId: string;
    public dkimSettingsId: string;
    public senderEmailAddress: string;
    public detailList: Array<DetailsListFormItem>;

    public constructor(
        protected alertCtrl: AlertController,
        protected eventService: EventService,
        public navProxy: NavProxyService,
        protected sharedLoaderService: SharedLoaderService,
        protected dkimSettingsApiService: DkimSettingsApiService,
        protected dkimSettingsService: DkimSettingsService,
        private routeHelper: RouteHelper,
        private formBuilder: FormBuilder,
        public layoutManager: LayoutManagerService,
        protected tenantService: TenantService,
        protected productService: ProductService,
        elementRef: ElementRef,
        injector: Injector,
        public formHelper: FormHelper,
        protected appConfigService: AppConfigService,
        protected loadCtrl: LoadingController,
    ) {
        super(eventService, elementRef, injector);
        appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.tenantId = appConfig.portal.tenantId;
        });
        this.buildForm();
    }

    public async ngOnInit(): Promise<void> {
        this.contextTenantAlias = this.routeHelper.getContextTenantAlias();
        this.dkimSettingsId = this.routeHelper.getParam('dkimSettingsId');
        this.organisationId = this.routeHelper.getParam('organisationId');
        this.senderEmailAddress = await this.getSenderEmailAddress();
    }

    public async send(): Promise<void> {
        this.form.get('recipientEmailAddress').markAsTouched();
        if (this.form.invalid) {
            return;
        }
        const model: DkimTestEmailResourceModel = {
            tenant: this.contextTenantAlias,
            dkimSettingsId: this.dkimSettingsId,
            organisationId: this.organisationId,
            recipientEmailAddress: this.form.get('recipientEmailAddress').value,
            senderEmailAddress: this.senderEmailAddress,
        };
        await this.dkimSettingsService.sendDkimTestEmail(model);
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.pop();
        this.navProxy.navigateBack(pathSegments, true);
    }

    protected buildForm(): void {
        this.form = this.formBuilder.group({
            recipientEmailAddress: ['', [Validators.required, Validators.email]],
        });
    }

    public async close(): Promise<void> {
        this.returnToDetailpage();
    }

    private async getSenderEmailAddress(): Promise<string> {
        this.isLoading = true;
        let dkimSettings: DkimSettingsResourceModel = await this.dkimSettingsApiService.getDkimSettingsById(
            this.dkimSettingsId,
            this.organisationId,
            this.routeHelper.getContextTenantAlias()).toPromise();
        if (dkimSettings == null || dkimSettings.applicableDomainNameList.length <= 0) {
            return "";
        }
        let applicableDomain: string = dkimSettings.applicableDomainNameList[0];
        const domainName: string = applicableDomain.startsWith("*.") ? `mail.${applicableDomain.substring(2)}`
            : applicableDomain;
        let senderEmailAddress: string = `no-reply@${domainName}`;
        this.isLoading = false;
        return senderEmailAddress;
    }

    public returnToDetailpage(): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.pop();
        this.navProxy.navigate(pathSegments);
    }
}
