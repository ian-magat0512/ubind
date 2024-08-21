import { Component, ElementRef, Injector, OnDestroy, OnInit } from '@angular/core';
import { ToastController } from '@ionic/angular';
import { ActivatedRoute } from '@angular/router';
import { Permission, RegularExpressions } from '@app/helpers';
import { FormGroup, FormBuilder, Validators, ValidatorFn, AbstractControl } from '@angular/forms';
import { PolicyApiService } from '@app/services/api/policy-api.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { Subject, Subscription } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { EventService } from '@app/services/event.service';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { PolicyResourceModel } from '@app/resource-models/policy.resource-model';
import { PolicyNumberAssignmentMethod } from '@app/models/policy-number-assignment-method.enum';
import { DeploymentEnvironment } from '@app/models/deployment-environment.enum';
import { NumberPoolApiService } from '@app/services/api/number-pool-api.service';
import { AppConfigService } from '@app/services/app-config.service';
import { AppConfig } from '@app/models/app-config';

/**
 * Export update policy number page component class
 * This class manage updaing the number of policies
 */
@Component({
    selector: 'app-update-policy-number',
    templateUrl: './update-policy-number.page.html',
    styleUrls: ['./update-policy-number.page.scss',
        '../../../../assets/css/form-toolbar.scss',
    ],
})
export class UpdatePolicyNumberPage extends DetailPage implements OnInit, OnDestroy {
    public policyNumber: string = null;
    // eslint-disable-next-line @typescript-eslint/naming-convention
    public Permission: typeof Permission = Permission;
    public isDesktop: boolean = false;
    public searchIsOn: boolean = false;
    // eslint-disable-next-line @typescript-eslint/naming-convention
    public PolicyNumberAssignmentMethod: typeof PolicyNumberAssignmentMethod = PolicyNumberAssignmentMethod;
    public isLoading: boolean = false;
    public policyNumberUpdateForm: FormGroup;
    public assignmentErrorText: string = 'The policy number is already assigned';
    private policyId: string;
    private arePolicyNumbersAvailable: boolean;
    public environment: DeploymentEnvironment;
    private performingUserTenantId: string;

    protected destroyed: Subject<void>;

    public constructor(
        private navProxy: NavProxyService,
        private toastCtrl: ToastController,
        private route: ActivatedRoute,
        private policyApiService: PolicyApiService,
        private formBuilder: FormBuilder,
        private sharedAlertService: SharedAlertService,
        public layoutManager: LayoutManagerService,
        private elementRef: ElementRef,
        public injector: Injector,
        protected eventService: EventService,
        protected sharedLoaderService: SharedLoaderService,
        private numberPoolApiService: NumberPoolApiService,
        private appConfigService: AppConfigService,
    ) {
        super(eventService, elementRef, injector);
        this.policyId = this.route.snapshot.paramMap.get('policyId');
        this.policyNumber = this.route.snapshot.paramMap.get('policyNumber') || '';
        this.buildPolicyUpdateForm();
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            if (!this.appConfigService.isMasterPortal()) {
                this.performingUserTenantId = appConfig.portal.tenantId;
            }
        });
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.load();
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
        this.policyNumberUpdateForm.reset();
        this.buildPolicyUpdateForm();
    }

    public updatePolicyNumber(formValues: any): void {
        if (!this.policyNumberUpdateForm.dirty) {
            this.policyNumberUpdateForm.markAsDirty();
        }

        if (this.policyNumberUpdateForm.get('assignmentType').value != this.PolicyNumberAssignmentMethod.Custom) {
            this.policyNumberUpdateForm.get('policyNumber').setErrors(null);
        }

        if (this.policyNumberUpdateForm.valid) {
            this.assignPolicyNumber(formValues);
        }
    }

    public backToPolicy(): void {
        this.navProxy.navigateBack(['policy', this.policyId], true, null);
    }

    private async load(): Promise<void> {
        try {
            this.isLoading = true;
            const env: string = this.route.snapshot.queryParamMap.get('environment');
            this.environment = DeploymentEnvironment[env];
            const policyDetail: PolicyResourceModel = await this.policyApiService
                .getById(this.policyId).toPromise();
            this.arePolicyNumbersAvailable = await this.numberPoolApiService
                .hasAvailableNumbers(
                    this.performingUserTenantId,
                    policyDetail.productId,
                    'policy',
                    this.environment,
                )
                .toPromise();
        } catch (error) {
            this.errorMessage = 'There was an error loading the policy details';
            throw error;
        } finally {
            this.isLoading = false;
        }

    }

    private async assignPolicyNumber(formValues: any): Promise<void> {
        formValues.policyNumber =
            this.policyNumberUpdateForm.get('assignmentType').value == this.PolicyNumberAssignmentMethod.Automatic ?
                '' : formValues.policyNumber;
        await this.sharedLoaderService.presentWithDelay();
        const isCurrentPolicyNumberSameAsOld: boolean
            = this.policyNumber == formValues.policyNumber;
        const isCustomType: boolean = this.policyNumberUpdateForm &&
            this.policyNumberUpdateForm.get('assignmentType').value == this.PolicyNumberAssignmentMethod.Custom;
        if (isCurrentPolicyNumberSameAsOld && isCustomType) {
            this.policyNumberUpdateForm.get('policyNumber').setErrors({ sameNumberError: true });
            this.sharedLoaderService.dismiss();
            return;
        }
        let subs: Subscription = this.policyApiService.updatePolicyNumber(
            this.policyId,
            formValues.policyNumber,
            formValues.isReusePreviousPolicyNumber)
            .pipe(takeUntil(this.destroyed))
            .subscribe((policy: PolicyResourceModel) => {
                this.eventService.getEntityUpdatedSubject('Policy').next(policy);
                this.presentUpdateSuccessToast(policy.policyNumber);
            }, async (error: any) => {
                const isErrorEmpty: boolean = error == null && error.error == null;
                this.policyNumberUpdateForm.get('policyNumber').setErrors({ responseError: true });
                this.sharedLoaderService.dismiss();
                if (!isErrorEmpty) {
                    if (error.error.code == 'policy.issuance.policy.number.not.unique') {
                        this.assignmentErrorText = `The policy number ${formValues.policyNumber} `
                            + `is already assigned to a different policy.`;
                    } else if (error.error.code == 'number.pool.none.available') {
                        await this.sharedAlertService.showWithOk(
                            error.error.title,
                            error.error.detail);
                    } else {
                        throw error;
                    }
                }
            }, () => {
                this.sharedLoaderService.dismiss();
                subs.unsubscribe();
            });
    }

    private async presentUpdateSuccessToast(policyNumber: string): Promise<void> {
        const toastMessage: string = `Policy number updated to ${policyNumber}`;
        const toast: HTMLIonToastElement = await this.toastCtrl.create(
            {
                id: this.policyId,
                message: toastMessage,
                position: 'bottom',
                duration: 3000,
            });

        return toast.present().then(() => {
            this.backToPolicy();
        });
    }

    private buildPolicyUpdateForm(): void {
        this.policyNumberUpdateForm = this.formBuilder.group({
            policyNumber: [
                this.policyNumber,
                [
                    Validators.required,
                    this.samePolicyNumberValidator(),
                    Validators.pattern(RegularExpressions.policyNumber),
                ],
            ],
            assignmentType: [this.PolicyNumberAssignmentMethod.Custom, [Validators.required]],
            isReusePreviousPolicyNumber: [false, []],
        });
    }

    private samePolicyNumberValidator(): ValidatorFn {
        return (control: AbstractControl): { [key: string]: any } | null => {
            const isCurrentPolicyNumberSameAsOld: boolean = control.value == this.policyNumber;
            const isCustomType: boolean = this.policyNumberUpdateForm &&
                this.policyNumberUpdateForm.get('assignmentType').value == this.PolicyNumberAssignmentMethod.Custom;
            return (isCurrentPolicyNumberSameAsOld && isCustomType && control.dirty) ?
                ({ sameNumberError: { value: true } }) : null;
        };
    }
}
