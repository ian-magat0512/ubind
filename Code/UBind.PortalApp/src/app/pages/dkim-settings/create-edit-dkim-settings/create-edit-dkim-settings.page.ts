import { Component, Injector, ElementRef, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AlertController } from '@ionic/angular';
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
import { CreateEditPage } from '@app/pages/master-create/create-edit.page';
import { finalize } from 'rxjs/operators';
import { DkimSettingsApiService } from '@app/services/api/dkim-settings-api.service';
import { DkimSettingsResourceModel, DkimSettingsUpsertModel } from '@app/resource-models/dkim-settings.resource-model';
import { DkimSettingsDetailViewModel } from '@app/viewmodels/dkim-settings-detail.viewmodel';
import { DkimSettingsService } from '@app/services/dkim-settings.service';
import { DetailsListFormItem } from '@app/models/details-list/details-list-form-item';
import { StringHelper } from '@app/helpers';

/**
 * Export create/edit DKIM settings page component class.
 * This class manage creation and editing of DKIM settings page.
 */
@Component({
    selector: 'app-create-edit-dkim-settings',
    templateUrl: './create-edit-dkim-settings.page.html',
    styleUrls: [
        '../../../../assets/css/scrollbar-form.css',
        '../../../../assets/css/form-toolbar.scss',
    ],
    styles: [scrollbarStyle],
})
export class CreateEditDkimSettingPage
    extends CreateEditPage<DkimSettingsResourceModel> implements OnInit {
    public isEdit: boolean;
    public subjectName: string = "DKIM Configuration";
    private tenantId: string;
    private organisationId: string;
    public dkimSettingsId: string;
    public domainNamePreviousValue: string;

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
    ) {
        super(eventService, elementRef, injector, formHelper);
        appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.tenantId = appConfig.portal.tenantId;
        });
    }

    public ngOnInit(): void {
        this.dkimSettingsId = this.routeHelper.getParam('dkimSettingsId');
        this.organisationId = this.routeHelper.getParam('organisationId');

        this.isEdit = this.dkimSettingsId != null;
        super.ngOnInit();
        this.isLoading = this.isEdit;
        if (this.isEdit) {
            this.detailList = DkimSettingsDetailViewModel.createDetailsListForCreateAndEdit();
            this.form = this.buildForm();
            this.load();
        } else {
            this.detailList = DkimSettingsDetailViewModel.createDetailsListForCreateAndEdit();
            this.form = this.buildForm();
        }

        this.setApplicableDomainNameValidation();
        this.setAgentOrUserIdentifier();
    }

    protected buildForm(): FormGroup {
        let controls: any = [];
        this.detailList.forEach((item: DetailsListFormItem) => {
            controls[item.Alias] = item.FormControl;
        });
        const form: FormGroup = this.formBuilder.group(controls);
        return form;
    }

    public load(): void {
        this.isLoading = true;
        let dkimSettings: any = {};
        this.dkimSettingsApiService.getDkimSettingsById(
            this.dkimSettingsId,
            this.organisationId,
            this.routeHelper.getContextTenantAlias())
            .pipe(finalize(() => this.isLoading = false))
            .subscribe((dkimSettingsResourceModel: DkimSettingsResourceModel) => {
                this.model = dkimSettingsResourceModel;
                dkimSettings = {
                    domainName: this.model.domainName,
                    privateKey: this.model.privateKey,
                    dnsSelector: this.model.dnsSelector,
                    agentOrUserIdentifier: this.model.agentOrUserIdentifier,
                    applicableDomainNames: this.model.applicableDomainNameList.join("\n"),
                };
                this.form.setValue(dkimSettings);
            });
    }

    public setTitle(): void {
        let prefix: string = this.isEdit ? "Edit" : "Create";
        this.title = `${prefix} ${this.subjectName}`;
    }
    private setAgentOrUserIdentifier(): void {
        this.form.controls["domainName"].valueChanges.subscribe((value: string) => {
            const domainNameChanged: boolean = this.domainNamePreviousValue != value;
            this.checkApplicableDomainNameValidation();
            if (domainNameChanged && !this.form.controls["agentOrUserIdentifier"].value) {
                const agentOrUserIdentifier: string = `@ubind.${this.form.controls["domainName"].value}`;
                this.form.controls["agentOrUserIdentifier"].setValue(agentOrUserIdentifier);
            }
            this.domainNamePreviousValue = value;
        });
    }

    private setApplicableDomainNameValidation(): void {
        this.form.controls["applicableDomainNames"].valueChanges.subscribe(() => {
            this.checkApplicableDomainNameValidation();
        });

        this.form.controls["domainName"].valueChanges.subscribe(() => {
            this.checkApplicableDomainNameValidation();
        });
    }

    private checkApplicableDomainNameValidation(): void {
        let domainName: string = this.form.controls["domainName"].value;
        let subDomain: string = this.form.controls["applicableDomainNames"].value;

        const subDomains: Array<string> = subDomain.split("\n");
        const isEmpty: boolean = StringHelper.isNullOrEmpty(subDomain);
        let isApplicableDomainInvalid: boolean;
        subDomains.forEach((subdomainName: string) => {
            let isValidSubDomain: boolean = subdomainName && !subdomainName.includes(" ")
                && subdomainName.endsWith(domainName);

            if (!isValidSubDomain) {
                isApplicableDomainInvalid = true;
                return;
            }
        });
        this.form.controls["applicableDomainNames"].
            setErrors(isEmpty ? { required: true } : isApplicableDomainInvalid ? { invalidSubDomain: true } : null);
    }

    public async close(): Promise<void> {
        if (this.isEdit) {
            if (this.form.dirty) {
                if (!await this.formHelper.confirmExitWithUnsavedChanges()) {
                    return;
                }
            }
            let pathSegments: Array<string> = this.routeHelper.getPathSegments();
            pathSegments.pop();
            this.navProxy.navigateForward(pathSegments);
        } else {
            super.close();
        }
    }

    public async create(value: any): Promise<void> {
        const model: DkimSettingsUpsertModel = {
            tenant: this.routeHelper.getContextTenantAlias(),
            organisationId: this.organisationId,
            domainName: value.domainName,
            privateKey: value.privateKey,
            dnsSelector: value.dnsSelector,
            agentOrUserIdentifier: value.agentOrUserIdentifier,
            applicableDomainNameList: value.applicableDomainNames ? value.applicableDomainNames.split('\n') : [],
        };

        const result: DkimSettingsResourceModel = await this.dkimSettingsService.create(model);
        const pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.pop();
        pathSegments.push(result.id);
        this.navProxy.navigateForward(pathSegments);
    }

    public async update(value: any): Promise<void> {
        const model: DkimSettingsUpsertModel = {
            tenant: this.routeHelper.getContextTenantAlias(),
            id: this.dkimSettingsId,
            domainName: value.domainName,
            privateKey: value.privateKey,
            dnsSelector: value.dnsSelector,
            agentOrUserIdentifier: value.agentOrUserIdentifier,
            applicableDomainNameList: value.applicableDomainNames ? value.applicableDomainNames.split('\n') : [],
        };

        await this.dkimSettingsService.update(this.dkimSettingsId, this.organisationId, model);
        const pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.pop();
        this.navProxy.navigateForward(pathSegments);
    }

    public returnToPrevious(): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        if (pathSegments[pathSegments.length - 1] == 'edit') {
            pathSegments.pop();
            this.navProxy.navigateForward(pathSegments);
        } else if (pathSegments[pathSegments.length - 1] == 'create') {
            pathSegments.pop();
            if (this.dkimSettingsId) {
                pathSegments.push(this.dkimSettingsId);
                this.navProxy.navigateForward(pathSegments);
            } else {
                pathSegments.push('list');
                this.navProxy.navigateBack(pathSegments, true);
            }
        }
    }
}
