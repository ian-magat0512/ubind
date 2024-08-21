import { Component, ElementRef, Injector, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ToastController } from '@ionic/angular';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { RegularExpressions } from '@app/helpers';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { PermissionService } from '@app/services/permission.service';
import { QuoteApiService } from '@app/services/api/quote-api.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { Subscription } from 'rxjs';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { IconLibrary } from '@app/models/icon-library.enum';
import { IonicLifecycleEventReplayBus } from '@app/services/ionic-lifecycle-event-replay-bus';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { EventService } from '@app/services/event.service';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { QuoteResourceModel } from '@app/resource-models/quote.resource-model';
import { PolicyNumberAssignmentMethod } from '@app/models/policy-number-assignment-method.enum';
import { PolicyIssuanceResourceModel } from '@app/resource-models/policy.resource-model';
import { NumberPoolApiService } from '@app/services/api/number-pool-api.service';
import { DeploymentEnvironment } from '@app/models/deployment-environment.enum';
import { AppConfig } from '@app/models/app-config';
import { AppConfigService } from '@app/services/app-config.service';

/**
 * Export issue policy page component class
 * This class manages the issuing of a policy.
 */
@Component({
    selector: 'app-issue-policy',
    templateUrl: './issue-policy.page.html',
    styleUrls: ['./issue-policy.page.scss',
        '../../../../assets/css/form-toolbar.scss',
        '../../../../assets/css/edit-form.scss',
    ],
})
export class IssuePolicyPage extends DetailPage implements OnInit, OnDestroy {
    public navProxy: NavProxyService;
    public permission: PermissionService;
    public isDesktop: boolean = false;
    public searchIsOn: boolean = false;
    // eslint-disable-next-line @typescript-eslint/naming-convention
    public PolicyNumberAssignmentMethod: typeof PolicyNumberAssignmentMethod = PolicyNumberAssignmentMethod;
    public issuePolicyForm: FormGroup;
    public isLoading: boolean = false;
    public assignmentErrorText: string;
    private quoteId: string = null;
    private quoteNumber: string = null;
    private arePolicyNumbersAvailable: boolean;
    public environment: DeploymentEnvironment;
    private performingUserTenantId: string;

    public ionicLifecycleEventReplayBus: IonicLifecycleEventReplayBus;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        private _navProxy: NavProxyService,
        private quoteApiService: QuoteApiService,
        private route: ActivatedRoute,
        private toastCtrl: ToastController,
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
        this.ionicLifecycleEventReplayBus = new IonicLifecycleEventReplayBus(elementRef);
        this.navProxy = _navProxy;
        this.buildIssuePolicyForm();
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            if (!this.appConfigService.isMasterPortal()) {
                this.performingUserTenantId = appConfig.portal.tenantId;
            }
        });
    }

    public ngOnInit(): void {
        this.load();
    }

    private async load(): Promise<void> {
        try {
            this.isLoading = true;
            this.quoteId = this.route.snapshot.paramMap.get('quoteId');
            const env: string = this.route.snapshot.queryParamMap.get('environment');
            const quote: QuoteResourceModel = await this.quoteApiService.getById(this.quoteId).toPromise();
            this.quoteNumber = quote.quoteNumber;
            this.environment = env ? DeploymentEnvironment[env] : 'Production';
            this.arePolicyNumbersAvailable = await this.numberPoolApiService.hasAvailableNumbers(
                this.performingUserTenantId,
                quote.productId,
                'policy',
                this.environment,
            ).toPromise();
            let defaultAssignmentType: PolicyNumberAssignmentMethod = this.arePolicyNumbersAvailable
                ? this.PolicyNumberAssignmentMethod.Automatic
                : this.PolicyNumberAssignmentMethod.Custom;
            this.issuePolicyForm = this.formBuilder.group({
                policyNumber: ['', [Validators.required,
                    Validators.pattern(RegularExpressions.policyNumber)]],
                assignmentType: [defaultAssignmentType, [Validators.required]],
            });
        } catch (error) {
            this.errorMessage = 'There was an error loading the quote details';
            throw error;
        } finally {
            this.isLoading = false;
        }
    }

    public ngOnDestroy(): void {
        this.issuePolicyForm.reset();
    }

    public async issuePolicyNumber(formValues: any): Promise<void> {
        if (!this.issuePolicyForm.dirty) {
            this.issuePolicyForm.markAsDirty();
        }

        if (!this.issuePolicyForm.get('policyNumber').dirty) {
            this.issuePolicyForm.get('policyNumber').markAsTouched();
            this.issuePolicyForm.get('policyNumber').markAsDirty();
        }

        if (this.issuePolicyForm.get('assignmentType').value != this.PolicyNumberAssignmentMethod.Custom) {
            this.issuePolicyForm.get('policyNumber').setErrors(null);
        }

        if (this.issuePolicyForm.valid ||
            formValues.assignmentType == this.PolicyNumberAssignmentMethod.Automatic.toString()) {
            formValues.policyNumber = formValues.assignmentType == this.PolicyNumberAssignmentMethod.Automatic ?
                '' : formValues.policyNumber;
            await this.sharedLoaderService.presentWait();
            let subs: Subscription = this.quoteApiService.issuePolicy(this.quoteId, formValues.policyNumber)
                .subscribe((data: PolicyIssuanceResourceModel) => {
                    this.sharedLoaderService.dismiss();
                    this.presentIssueSuccessToast(data);
                }, async (error: any) => {
                    const isErrorEmpty: boolean = error == null && error.error == null;
                    this.issuePolicyForm.get('policyNumber').setErrors({ responseError: true });
                    this.sharedLoaderService.dismiss();
                    if (!isErrorEmpty) {
                        if (error.error.code == 'policy.issuance.policy.number.not.unique') {
                            this.assignmentErrorText = `The specified policy number is already in use.`;
                        } else if (error.error.code == 'number.pool.none.available') {
                            this.issuePolicyForm.get('policyNumber').setErrors(null);
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
    }

    public backToQuote(): void {
        this.navProxy.navigateBack(['quote', this.quoteId], true, null);
    }

    public goToPolicy(policyId: string): void {
        this.navProxy.navigateForward(['policy', policyId], true, null);
    }

    private async presentIssueSuccessToast(data: PolicyIssuanceResourceModel): Promise<void> {
        const toast: HTMLIonToastElement = await this.toastCtrl.create(
            {
                id: this.quoteId,
                message: `Policy ${data.policyNumber} issued for quote ${this.quoteNumber}`,
                position: 'bottom',
                duration: 3000,
            });

        return toast.present().then(() => {
            this.goToPolicy(data.policyId);
        });
    }

    private buildIssuePolicyForm(): void {
        this.issuePolicyForm = this.formBuilder.group({
            policyNumber: ['', [Validators.required, Validators.pattern(RegularExpressions.policyNumber)]],
        });
    }
}
