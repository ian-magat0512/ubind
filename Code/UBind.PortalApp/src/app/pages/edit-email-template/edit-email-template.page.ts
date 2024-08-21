import { Component, Injector, ElementRef, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { EmailTemplateApiService } from '@app/services/api/email-template-api.service';
import { Subject } from 'rxjs';
import { EmailTemplateSetting } from '@app/models';
import {
    EmailTemplateSettingUpdateModel,
} from '@app/resource-models/email-update-template-setting.resource-model';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { EventService } from '@app/services/event.service';
import { FormHelper } from '@app/helpers/form.helper';
import { finalize, takeUntil } from 'rxjs/operators';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { HttpErrorResponse } from '@angular/common/http';
import { CreateEditPage } from '@app/pages/master-create/create-edit.page';
import { EmailTemplateSettingViewModel } from '@app/viewmodels/email-template.viewmodel';
import { AppConfigService } from '@app/services/app-config.service';
import { AppConfig } from '@app/models/app-config';
import { TenantService } from '@app/services/tenant.service';

/**
 * Edit Email Template Page
 * Provides implementation for editing email templates for both product and tenant.
 */
@Component({
    selector: 'app-edit-email-template',
    templateUrl: './edit-email-template.page.html',
    styleUrls: ['./edit-email-template.page.scss'],
})
export class EditEmailTemplatePage
    extends CreateEditPage<EmailTemplateSettingViewModel> implements OnInit, OnDestroy {
    public isEdit: boolean = true;
    public subjectName: string = "Email Template";
    public ownerId: string = '';
    private pathTenantAlias: string;
    private performingUserTenantId: string;
    private emailTemplateId: string;
    public defaultGuid: string = '00000000-0000-0000-0000-000000000000';
    private emailTemplateSetting: EmailTemplateSetting;

    public constructor(
        public tenantService: TenantService,
        protected sharedLoaderService: SharedLoaderService,
        protected sharedAlertService: SharedAlertService,
        protected formBuilder: FormBuilder,
        protected emailTemplateApiService: EmailTemplateApiService,
        private routeHelper: RouteHelper,
        public navProxy: NavProxyService,
        public layoutManager: LayoutManagerService,
        private appConfigService: AppConfigService,
        eventService: EventService,
        elementRef: ElementRef,
        injector: Injector,
        formHelper: FormHelper,
    ) {
        super(eventService, elementRef, injector, formHelper);
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            if (!this.appConfigService.isMasterPortal()) {
                this.performingUserTenantId = appConfig.portal.tenantId;
            }
        });

        this.form = this.buildForm();
    }

    public ngOnInit(): void {
        super.ngOnInit();
        this.destroyed = new Subject<void>();
        this.emailTemplateId = this.routeHelper.getParam('emailTemplateId');
        this.pathTenantAlias = this.routeHelper.getParam('tenantAlias');
        this.load();
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    protected buildForm(): FormGroup {
        return this.formBuilder.group({
            id: [this.defaultGuid],
            name: [''],
            ownerId: [''],
            subject: [''],
            fromAddress: [''],
            toAddress: [''],
            cc: [''],
            bcc: [''],
            htmlBody: [''],
            plainTextBody: [''],
            smtpServerHost: [''],
            smtpServerPort: [''],
            createdDateTime: [''],
            disabled: [false],
        });
    }

    public async load(): Promise<void> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', this.routeHelper.getContextTenantAlias());
        this.emailTemplateApiService.getById(this.emailTemplateId, params)
            .pipe(
                finalize(() => this.isLoading = false),
                takeUntil(this.destroyed))
            .subscribe(
                (emailTemplateSetting: EmailTemplateSetting) => {
                    this.emailTemplateSetting = emailTemplateSetting;
                    this.model = new EmailTemplateSettingViewModel(emailTemplateSetting);
                    this.title = 'Edit ' + this.model.name.toLowerCase() + ' email template';

                    this.form.setValue(
                        {
                            id: this.model.id,
                            name: this.model.name,
                            ownerId: this.ownerId,
                            subject: this.model.subject,
                            fromAddress: this.model.fromAddress,
                            toAddress: this.model.toAddress,
                            cc: this.model.cc,
                            bcc: this.model.bcc,
                            htmlBody: this.model.htmlBody,
                            plainTextBody: this.model.plainTextBody,
                            smtpServerHost: this.model.smtpServerHost,
                            smtpServerPort: this.model.smtpServerPort,
                            createdDateTime: this.model.createdDateTime,
                            disabled: false,
                        });

                    this.detailList = EmailTemplateSettingViewModel.createDetailsListForEdit();
                },
                (err: HttpErrorResponse) => {
                    this.errorMessage = 'There was a problem loading the email template';
                    throw err;
                },
            );
    }

    public returnToPrevious(): void {
        this.navProxy.navigateBackN(3, true, { queryParams: { segment: 'Settings' } });
    }

    public create(value: any): void {
    }

    public async update(value: any): Promise<void> {
        this.emailTemplateSetting.name = value.name;
        this.emailTemplateSetting.ownerId = value.ownerId;
        this.emailTemplateSetting.subject = value.subject;
        this.emailTemplateSetting.fromAddress = value.fromAddress;
        this.emailTemplateSetting.toAddress = value.toAddress;
        this.emailTemplateSetting.cc = value.cc;
        this.emailTemplateSetting.bcc = value.bcc;
        this.emailTemplateSetting.htmlBody = value.htmlBody;
        this.emailTemplateSetting.plainTextBody = value.plainTextBody;
        this.emailTemplateSetting.smtpServerHost = value.smtpServerHost;
        this.emailTemplateSetting.smtpServerPort = value.smtpServerPort;
        this.emailTemplateSetting.disabled = value.disabled;

        await this.sharedLoaderService.presentWait();
        let requestModel: EmailTemplateSettingUpdateModel =
            new EmailTemplateSettingUpdateModel(this.emailTemplateSetting);
        this.emailTemplateApiService.updateEmailTemplateDetails(value.id, requestModel)
            .pipe(
                finalize(() => this.sharedLoaderService.dismiss()),
                takeUntil(this.destroyed))
            .subscribe(
                (res: EmailTemplateSetting) => {
                    this.sharedAlertService.showToast('Template details has been successfully updated');
                    this.returnToPrevious();
                });
    }
}
