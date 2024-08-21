import { Component, Injector, ElementRef, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { AlertController, LoadingController } from '@ionic/angular';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { SystemAlertApiService } from '@app/services/api/system-alert-api.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { DetailPage } from '../master-detail/detail.page';
import { EventService } from '@app/services/event.service';
import { FormHelper } from '@app/helpers/form.helper';
import { AuthenticationService } from '@app/services/authentication.service';
import { CustomValidators } from '@app/helpers/custom-validators';

/**
 * Export system alert page component class.
 * TODO: Write a better class header: system alert functions.
 */
@Component({
    selector: 'app-system-alert',
    templateUrl: './system-alert.page.html',
    styleUrls: [
        '../../../assets/css/form-toolbar.scss',
        '../../../assets/css/edit-form.scss',
    ],
})
export class SystemAlertPage extends DetailPage implements OnInit {
    public systemAlertForm: FormGroup;
    public formHasError: boolean = false;
    public model: any;
    public headTitle: string = 'Create';
    public tenantAlias: string;
    public productAlias: string;
    public systemAlertId: string;
    public isMutual: boolean;

    public constructor(
        public navProxy: NavProxyService,
        protected loadCtrl: LoadingController,
        protected alertCtrl: AlertController,
        protected systemAlertApiService: SystemAlertApiService,
        public authService: AuthenticationService,
        private routeHelper: RouteHelper,
        private formBuilder: FormBuilder,
        public layoutManager: LayoutManagerService,
        eventService: EventService,
        elementRef: ElementRef,
        injector: Injector,
        private formHelper: FormHelper,
    ) {
        super(eventService, elementRef, injector);
        this.buildForm();
    }

    public ngOnInit(): void {
        this.systemAlertId = this.routeHelper.getParam('systemAlertId');
        this.tenantAlias =
            this.routeHelper.getParam('tenantAlias') || this.routeHelper.getParam('portalTenantAlias') || null;
        this.isMutual = this.authService.isMutualTenant();
        this.productAlias = this.routeHelper.getParam('productAlias') || null;
        const typeId: string = this.routeHelper.getParam('typeId') || null;
        if (this.systemAlertId) {
            this.loadModel(this.systemAlertId);
            this.headTitle = 'Edit';
        } else {
            this.model = {
                tenantId: this.tenantAlias,
                productId: this.productAlias,
                systemAlertType: { id: typeId },
                systemAlertTypeId: typeId,
            };
            this.headTitle = 'Create';
        }
    }

    public async loadModel(systemAlertId: string): Promise<void> {
        const loader: HTMLIonLoadingElement = await this.loadCtrl.create({
            message: 'Please wait..',
        });
        loader.present().then(() => {
            this.systemAlertApiService.getSystemAlertById(this.tenantAlias, systemAlertId).subscribe(
                (dt: any) => {
                    this.model = dt;
                    this.model.systemAlertType.name = this.isMutual ?
                        (this.model.systemAlertType.name + '').replace('Policy', 'Protection') :
                        this.model.systemAlertType.name;
                    loader.dismiss();
                    this.systemAlertForm.setValue({
                        warningThreshold: this.model.warningThreshold, criticalThreshold: this.model.criticalThreshold,
                        disabled: this.model.disabled,
                    });
                },
                (err: any) => {
                    loader.dismiss();
                    throw err;
                },
            );
        });
    }

    public async userDidTapCloseButton(): Promise<void> {
        if (this.systemAlertForm.dirty) {
            if (!await this.formHelper.confirmExitWithUnsavedChanges()) {
                return;
            }
        }
        this.goBack();
    }

    public userDidTapSaveButton(value: any): void {
        if (!this.systemAlertForm.valid) {
            this.formHasError = true;
            return;
        }
        this.formHasError = false;
        if (this.model && this.model.id) {
            this.updateSystemAlert(value);
        }
    }

    protected buildForm(): void {
        this.systemAlertForm = this.formBuilder.group({
            warningThreshold: ['', [CustomValidators.wholeNumber, Validators.min(0), Validators.max(9999)]],
            criticalThreshold: ['', [CustomValidators.wholeNumber, Validators.min(0), Validators.max(9999)]],
            disabled: false,
        });
    }

    private async updateSystemAlert(value: any): Promise<void> {
        const model: any = {
            id: this.model.id,
            tenantId: this.model.tenantId,
            warningThreshold: value.warningThreshold,
            criticalThreshold: value.criticalThreshold,
            disabled: this.model.disabled,
            systemAlertType: this.model.systemAlertType,
            systemAlertTypeId: this.model.systemAlertTypeId,
        };

        const loading: HTMLIonLoadingElement = await this.loadCtrl.create({
            message: 'Please wait...',
            cssClass: (this.layoutManager.splitPaneEnabled) ? 'detail-loader' : '',
            showBackdrop: (!this.layoutManager.splitPaneEnabled),
        });

        loading.present().then(() => {
            this.systemAlertApiService.update(this.tenantAlias, this.model.id, model).subscribe(
                (res: Response) => {
                    loading.dismiss();
                    this.goBack();
                },
                (err: HttpErrorResponse) => {
                    loading.dismiss();
                    throw err;
                },
            );
        });
    }

    private goBack(): void {
        this.navProxy.navigateBackN(3, true, { queryParams: { segment: 'Settings' } });
    }
}
