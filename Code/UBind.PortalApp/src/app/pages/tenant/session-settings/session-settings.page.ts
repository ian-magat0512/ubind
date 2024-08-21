import { Component, Injector, ElementRef, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { SystemAlertApiService } from '@app/services/api/system-alert-api.service';
import { TenantApiService } from '@app/services/api/tenant-api.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { EventService } from '@app/services/event.service';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { finalize } from 'rxjs/operators';
import { TenantService } from '@app/services/tenant.service';
import { SessionExpiryMode } from '@app/models/session-expiry-mode.enum';
import { TenantSessionSettingResourceModel } from '@app/resource-models/tenant-session-settings.resource-model';
import { CustomValidators } from '@app/helpers/custom-validators';
import { Subscription } from 'rxjs';

/**
 * Export tenant session settings page component class.
 * This class manage setting up the tenant sessions.
 */
@Component({
    selector: 'app-session-settings',
    templateUrl: './session-settings.page.html',
    styleUrls: [
        './session-settings.page.scss',
        '../../../../assets/css/form-toolbar.scss',
        '../../../../assets/css/edit-form.scss',
    ],
})
export class TenantSessionSettingsPage extends DetailPage implements OnInit {
    public sessionSettingsForm: FormGroup;
    public formHasError: boolean = false;
    public model: TenantSessionSettingResourceModel;
    public tenantAlias: string;
    public tenantId: string;
    public selectedSessionExpiryMode: string;
    public sessionExpiryMode: any = SessionExpiryMode;

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
        this.tenantAlias = this.routeHelper.getParam('tenantAlias');
        this.buildForm();
    }

    public ngOnInit(): void {
        this.tenantAlias = this.routeHelper.getParam('tenantAlias') || null;
        this.loadModel();
    }

    public applySelection(event: any): void {
        this.selectedSessionExpiryMode = event.value;
        this.formHasError = false;
    }

    public async loadModel(): Promise<void> {
        this.tenantId = await this.tenantService.getTenantIdFromAlias(this.tenantAlias);
        let subscription: Subscription = this.tenantApiService.getSessionSettings(this.tenantId)
            .pipe(finalize(() => {
                subscription.unsubscribe();
                this.isLoading = false;
            }))
            .subscribe((dt: TenantSessionSettingResourceModel) => {
                this.model = dt;
                if (!this.model.idleTimeoutPeriodType) {
                    this.model.idleTimeoutPeriodType = "Minute";
                }
                if (!this.model.fixLengthTimeoutInPeriodType) {
                    this.model.fixLengthTimeoutInPeriodType = "Day";
                }
                this.selectedSessionExpiryMode = this.model.sessionExpiryMode;
                this.sessionSettingsForm.setValue({
                    sessionExpiryMode: this.model.sessionExpiryMode,
                    idleTimeoutPeriodType: this.model.idleTimeoutPeriodType,
                    fixLengthTimeoutInPeriodType: this.model.fixLengthTimeoutInPeriodType,
                    idleTimeout: this.model.idleTimeout,
                    fixLengthTimeout: this.model.fixLengthTimeout,
                });
            }, (err: any) => {
                this.errorMessage = 'There was a problem loading the session settings';
                throw err;
            });
    }

    public async userDidTapCloseButton(value: any): Promise<void> {
        if (!this.isLoading && (this.model.idleTimeoutPeriodType != value.idleTimeoutPeriodType
            || this.model.fixLengthTimeoutInPeriodType != value.fixLengthTimeoutInPeriodType
            || this.model.idleTimeout != value.idleTimeout
            || this.model.fixLengthTimeout != value.fixLengthTimeout)) {
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
        if (!this.sessionSettingsForm.valid) {
            this.formHasError = true;
            return;
        }

        this.formHasError = false;
        if (!this.isLoading && this.model) {
            this.updateTenantSessionSettings(value);
        }
    }

    protected buildForm(): void {
        this.sessionSettingsForm = this.formBuilder.group({
            sessionExpiryMode: [SessionExpiryMode.FixedPeriod, [Validators.required]],
            idleTimeoutPeriodType: ['Minute', [Validators.required]],
            fixLengthTimeoutInPeriodType: ['Day', [Validators.required]],
            idleTimeout: [0, [CustomValidators.wholeNumber, Validators.required]],
            fixLengthTimeout: [0, [CustomValidators.wholeNumber, Validators.required]],
        });
    }

    private async updateTenantSessionSettings(value: any): Promise<void> {
        const model: TenantSessionSettingResourceModel = {
            sessionExpiryMode: value.sessionExpiryMode,
            idleTimeoutPeriodType: value.idleTimeoutPeriodType,
            fixLengthTimeoutInPeriodType: value.fixLengthTimeoutInPeriodType,
            idleTimeout: value.idleTimeout,
            fixLengthTimeout: value.fixLengthTimeout,
        };

        await this.sharedLoaderService.presentWait();
        let subscription: Subscription = this.tenantApiService.updateSessionSettings(this.tenantId, model)
            .pipe(finalize(() => {
                subscription.unsubscribe();
                this.sharedLoaderService.dismiss();
            }))
            .subscribe((res: any) => {
                this.returnToSettings();
            });
    }

    private returnToSettings(): void {
        // TODO: Redirect to Settings Page instead of tenant details page
        this.navProxy.navigateBack(['tenant', this.tenantAlias], true, { queryParams: { segment: "Settings" } });
    }
}
