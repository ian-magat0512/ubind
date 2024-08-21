import { Component, ElementRef, Injector, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ToastController } from '@ionic/angular';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { RegularExpressions } from '@app/helpers';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { PermissionService } from '@app/services/permission.service';
import { ClaimApiService } from '@app/services/api/claim-api.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { Subscription } from 'rxjs';
import { IconLibrary } from '@app/models/icon-library.enum';
import { IonicLifecycleEventReplayBus } from '@app/services/ionic-lifecycle-event-replay-bus';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { EventService } from '@app/services/event.service';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { ClaimNumberAssignmentMethod } from '@app/models/claim-number-assignment-method.enum';

/**
 * Export number assign claim page component class
 * This class manage assingning of claims.
 */
@Component({
    selector: 'app-number-assign-claim',
    templateUrl: './number-assign-claim.page.html',
    styleUrls: ['./number-assign-claim.page.scss',
        '../../../../assets/css/form-toolbar.scss',
        '../../../../assets/css/edit-form.scss',
    ],
})
export class NumberAssignClaimPage extends DetailPage implements OnInit, OnDestroy {
    public navProxy: NavProxyService;
    public permission: PermissionService;
    public isDesktop: boolean = false;
    public searchIsOn: boolean = false;
    // eslint-disable-next-line @typescript-eslint/naming-convention
    public ClaimNumberAssignmentMethod: typeof ClaimNumberAssignmentMethod = ClaimNumberAssignmentMethod;
    public claimNumberAssignForm: FormGroup;
    public isLoading: boolean = false;
    public assignmentErrorText: string = 'The claim number is already assigned';

    private claimId: string = null;
    private portalBaseUrl: string = null;
    public ionicLifecycleEventReplayBus: IonicLifecycleEventReplayBus;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        private _navProxy: NavProxyService,
        private claimApiService: ClaimApiService,
        private route: ActivatedRoute,
        private toastCtrl: ToastController,
        private formBuilder: FormBuilder,
        public layoutManager: LayoutManagerService,
        private elementRef: ElementRef,
        public injector: Injector,
        protected eventService: EventService,
        private sharedLoaderService: SharedLoaderService,
    ) {
        super(eventService, elementRef, injector);
        this.ionicLifecycleEventReplayBus = new IonicLifecycleEventReplayBus(elementRef);
        this.navProxy = _navProxy;
        this.buildClaimAssignForm();
    }

    public ngOnInit(): void {
        this.claimId = this.route.snapshot.paramMap.get('claimId');
    }

    public ngOnDestroy(): void {
        this.claimNumberAssignForm.reset();
    }

    public async assignClaimNumber(formValues: any): Promise<void> {
        if (!this.claimNumberAssignForm.dirty) {
            this.claimNumberAssignForm.markAsDirty();
        }

        if (!this.claimNumberAssignForm.get('claimNumber').dirty) {
            this.claimNumberAssignForm.get('claimNumber').markAsTouched();
            this.claimNumberAssignForm.get('claimNumber').markAsDirty();
        }

        if (this.claimNumberAssignForm.get('assignmentType').value != this.ClaimNumberAssignmentMethod.Custom) {
            this.claimNumberAssignForm.get('claimNumber').setErrors(null);
        }

        if (this.claimNumberAssignForm.valid ||
            formValues.assignmentType == this.ClaimNumberAssignmentMethod.Automatic.toString()) {
            formValues.claimNumber = formValues.assignmentType == this.ClaimNumberAssignmentMethod.Automatic ?
                '' : formValues.claimNumber;
            await this.sharedLoaderService.presentWithDelay();
            let subs: Subscription = this.claimApiService.assignClaimNumber(formValues.claimNumber, this.claimId)
                .subscribe((data: Response) => {
                    this.sharedLoaderService.dismiss();
                    this.presentAssignSuccessToast();
                }, (error: any) => {
                    this.assignmentErrorText = error && error.error ? error.error : '';
                    this.claimNumberAssignForm.get('claimNumber').setErrors({ responseError: true });
                    this.sharedLoaderService.dismiss();
                }, () => {
                    this.sharedLoaderService.dismiss();
                    subs.unsubscribe();
                });
        }
    }

    public backToClaim(): void {
        this.navProxy.navigateBack(['claim', this.claimId], true, null);
    }

    private async presentAssignSuccessToast(): Promise<void> {
        const toast: HTMLIonToastElement = await this.toastCtrl.create(
            {
                id: this.claimId,
                message: `Claim number successfully assigned`,
                position: 'bottom',
                duration: 3000,
            },
        );

        return toast.present().then(() => {
            this.backToClaim();
        });
    }

    private buildClaimAssignForm(): void {
        this.claimNumberAssignForm = this.formBuilder.group({
            claimNumber: ['', [Validators.required, Validators.pattern(RegularExpressions.claimNumber)]],
            assignmentType: ['', [Validators.required]],
        });
    }
}
