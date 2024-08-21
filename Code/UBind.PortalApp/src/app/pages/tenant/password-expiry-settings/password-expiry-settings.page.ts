import { Component, Injector, ElementRef, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { SystemAlertApiService } from '@app/services/api/system-alert-api.service';
import { TenantApiService } from '@app/services/api/tenant-api.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { EventService } from '@app/services/event.service';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { finalize, takeUntil } from 'rxjs/operators';
import { TenantService } from '@app/services/tenant.service';
import { CustomValidators } from '@app/helpers/custom-validators';
import { Subject } from 'rxjs';
import { TenantPasswordExpirySettingResourceModel }
    from '@app/resource-models/tenant-password-expiry-settings.resource-model';
import { TenantResourceModel } from '@app/resource-models/tenant.resource-model';

/**
 * Export tenant password expiry settings page component class.
 * This class manage setting up the tenant password expiry.
 */
@Component({
    selector: 'app-password-expiry-settings',
    templateUrl: './password-expiry-settings.page.html',
    styleUrls: [
        './password-expiry-settings.page.scss',
        '../../../../assets/css/form-toolbar.scss',
    ],
})
export class TenantPasswordExpirySettingsPage extends DetailPage implements OnInit, OnDestroy {
    public passwordExpirySettingsForm: FormGroup;
    public formHasError: boolean = false;
    public model: TenantPasswordExpirySettingResourceModel;
    public tenantAlias: string;
    public tenant: TenantResourceModel;
    public isPasswordExpiryEnabled: boolean;

    public constructor(
        public navProxy: NavProxyService,
        protected sharedLoaderService: SharedLoaderService,
        protected sharedAlertService: SharedAlertService,
        protected systemAlertApiService: SystemAlertApiService,
        private routeHelper: RouteHelper,
        private formBuilder: FormBuilder,
        private tenantApiService: TenantApiService,
        public layoutManager: LayoutManagerService,
        public tenantService: TenantService,
        eventService: EventService,
        elementRef: ElementRef,
        injector: Injector,
    ) {
        super(eventService, elementRef, injector);
        this.buildForm();
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.tenantAlias = this.routeHelper.getParam('tenantAlias') || null;
        this.loadModel();
    }

    public ngOnDestroy(): void {
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    public async loadModel(): Promise<void> {
        this.tenant = await this.tenantService.getTenantFromAlias(this.tenantAlias);
        this.tenantApiService.getPasswordExpirySettings(this.tenant.id)
            .pipe(
                finalize(() => this.isLoading = false),
                takeUntil(this.destroyed),
            )
            .subscribe((dt: TenantPasswordExpirySettingResourceModel) => {
                this.model = dt;
                this.isPasswordExpiryEnabled = this.model.passwordExpiryEnabled;

                this.passwordExpirySettingsForm.setValue({
                    maxPasswordAgeDays: this.model.passwordExpiryEnabled ? this.model.maxPasswordAgeDays : 90,
                    passwordExpiryEnabled: this.model.passwordExpiryEnabled,
                });
            }, (err: any) => {
                this.errorMessage = 'There was a problem loading the password expiry settings';
                throw err;
            });
    }

    protected buildForm(): void {
        this.passwordExpirySettingsForm = this.formBuilder.group({
            maxPasswordAgeDays: ['', [CustomValidators.positiveNumber, CustomValidators.greaterThanZero]],
            passwordExpiryEnabled: [''],
        });
    }

    public onBlur(value: any): void {
        if (this.validateForm(value)) {
            return;
        }
    }

    public async userDidTapCloseButton(value: any): Promise<void> {
        if (!this.isLoading && this.model.passwordExpiryEnabled != value.passwordExpiryEnabled
            || (this.model.passwordExpiryEnabled && this.model.maxPasswordAgeDays != value.maxPasswordAgeDays)) {
            this.sharedAlertService.showWithActionHandler({
                header: 'Unsaved Changes',
                subHeader: 'You have unsaved changes, are you sure you wish to '
                    + 'close the current view without saving them?',
                buttons: [
                    {
                        text: 'No',
                        handler: (): any => {
                            return;
                        },
                    },
                    {
                        text: 'Yes',
                        handler: (): any => {
                            this.returnToSettings();
                        },
                    },
                ],
            });
        } else {
            this.returnToSettings();
        }
    }

    public userDidTapSaveButton(value: any): void {
        if (!this.validateForm(value) && !this.isLoading && this.model) {
            this.updateTenantSessionSettings(value);
        }
    }

    public toggleIsPasswordExpiryEnabled(): void {
        this.isPasswordExpiryEnabled = !this.isPasswordExpiryEnabled;
    }

    private async updateTenantSessionSettings(value: any): Promise<void> {
        const model: TenantPasswordExpirySettingResourceModel = {
            maxPasswordAgeDays: value.passwordExpiryEnabled ? value.maxPasswordAgeDays : 0,
            passwordExpiryEnabled: value.passwordExpiryEnabled,
        };

        await this.sharedLoaderService.presentWait();
        this.tenantApiService.updatePasswordExpirySettings(this.tenant.id, model)
            .pipe(
                finalize(() => this.sharedLoaderService.dismiss()),
                takeUntil(this.destroyed),
            )
            .subscribe((res: any) => {
                this.sharedAlertService.showToast(`The password expiry settings for ${this.tenant.name} were saved`);
                this.returnToSettings();
            });
    }

    private returnToSettings(): void {
        this.navProxy.navigateBack(['tenant', this.tenantAlias], true, { queryParams: { segment: "Settings" } });
    }

    private validateForm(value: any): boolean {
        if (!this.passwordExpirySettingsForm.valid
            || (value.passwordExpiryEnabled && !value.maxPasswordAgeDays)) {
            this.formHasError = true;
            return true;
        }

        this.formHasError = false;
        return false;
    }
}
