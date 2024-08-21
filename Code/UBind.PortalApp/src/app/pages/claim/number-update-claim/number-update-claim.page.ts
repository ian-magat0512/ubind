import { Component, ElementRef, Injector, OnDestroy, OnInit } from '@angular/core';
import { ToastController } from '@ionic/angular';
import { ActivatedRoute } from '@angular/router';
import { Permission, RegularExpressions } from '@app/helpers';
import { FormGroup, FormBuilder, Validators, ValidatorFn, AbstractControl } from '@angular/forms';
import { ClaimApiService } from '@app/services/api/claim-api.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { Subject, Subscription } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { EventService } from '@app/services/event.service';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { ClaimNumberAssignmentMethod } from '@app/models/claim-number-assignment-method.enum';

/**
 * Export number update claim page component class
 * This class manages the updating of the claim number
 */
@Component({
    selector: 'app-number-update-claim',
    templateUrl: './number-update-claim.page.html',
    styleUrls: ['./number-update-claim.page.scss',
        '../../../../assets/css/form-toolbar.scss',
    ],
})
export class NumberUpdateClaimPage extends DetailPage implements OnInit, OnDestroy {
    // eslint-disable-next-line @typescript-eslint/naming-convention
    public ClaimNumberAssignmentMethod: typeof ClaimNumberAssignmentMethod = ClaimNumberAssignmentMethod;
    public claimNumber: string = null;
    public permission: typeof Permission = Permission;
    public isDesktop: boolean = false;
    public searchIsOn: boolean = false;
    public isLoading: boolean = false;
    public claimNumberUpdateForm: FormGroup;
    public assignmentErrorText: string = 'The claim number is already assigned';
    private claimId: string;

    protected destroyed: Subject<void>;

    public constructor(
        private navProxy: NavProxyService,
        private toastCtrl: ToastController,
        private route: ActivatedRoute,
        private claimApiService: ClaimApiService,
        private formBuilder: FormBuilder,
        public layoutManager: LayoutManagerService,
        private elementRef: ElementRef,
        public injector: Injector,
        protected eventService: EventService,
        private sharedLoaderService: SharedLoaderService,
    ) {
        super(eventService, elementRef, injector);
        this.claimId = this.route.snapshot.paramMap.get('claimId');
        this.claimNumber = this.route.snapshot.queryParamMap.get('claimNumber') || '';
        this.buildClaimUpdateForm();
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
        this.claimNumberUpdateForm.reset();
        this.buildClaimUpdateForm();
    }

    public updateClaimNumber(): void {
        if (!this.claimNumberUpdateForm.dirty) {
            this.claimNumberUpdateForm.markAsDirty();
        }

        if (this.claimNumberUpdateForm.get('assignmentType').value != this.ClaimNumberAssignmentMethod.Custom) {
            this.claimNumberUpdateForm.get('claimNumber').setErrors(null);
        }

        if (this.claimNumberUpdateForm.valid) {
            if (this.claimNumberUpdateForm.get('assignmentType').value == this.ClaimNumberAssignmentMethod.Unassign) {
                this.unassignClaimNumber();
            } else {
                this.assignClaimNumber();
            }
        }
    }

    public backToClaim(): void {
        this.navProxy.navigateBack(['claim', this.claimId], true, null);
    }

    private async assignClaimNumber(): Promise<void> {
        if (this.claimNumberUpdateForm.valid) {
            let claimForm: any = this.claimNumberUpdateForm.value;
            claimForm.claimNumber =
                this.claimNumberUpdateForm.get('assignmentType').value == this.ClaimNumberAssignmentMethod.Automatic ?
                    '' : claimForm.claimNumber;
            await this.sharedLoaderService.presentWithDelay();
            let subs: Subscription = this.claimApiService.assignClaimNumber(
                claimForm.claimNumber,
                this.claimId,
                this.claimNumberUpdateForm.get('isReusePreviousClaimNumber').value,
            )
                .pipe(takeUntil(this.destroyed))
                .subscribe(() => {
                    this.presentUpdateSuccessToast();
                }, (error: any) => {
                    this.sharedLoaderService.dismiss();
                    this.assignmentErrorText = error && error.error ? error.error : '';
                    this.claimNumberUpdateForm.get('claimNumber').setErrors({ responseError: true });
                    throw error;
                }, () => {
                    this.sharedLoaderService.dismiss();
                    subs.unsubscribe();
                });
        }
    }

    private async unassignClaimNumber(): Promise<void> {
        if (this.claimNumberUpdateForm.valid) {
            await this.sharedLoaderService.presentWithDelay();
            let subs: Subscription = this.claimApiService.unassignClaimNumber(
                this.claimNumberUpdateForm.get('claimNumber').value,
                this.claimId,
                this.claimNumberUpdateForm.get('isReusePreviousClaimNumber').value,
            )
                .pipe(takeUntil(this.destroyed))
                .subscribe(() => {
                    this.presentUpdateSuccessToast();
                }, (error: any) => {
                    this.sharedLoaderService.dismiss();
                    this.assignmentErrorText = error && error.error ? error.error : '';
                    this.claimNumberUpdateForm.get('claimNumber').setErrors({ responseError: true });
                    throw error;
                }, () => {
                    this.sharedLoaderService.dismiss();
                    subs.unsubscribe();
                });
        }
    }

    private async presentUpdateSuccessToast(): Promise<void> {
        const toastMessage: string = (
            this.claimNumberUpdateForm.get('assignmentType').value == this.ClaimNumberAssignmentMethod.Unassign) ?
            'Claim number successfully un-assigned' : 'Claim number successfully updated';
        const toast: HTMLIonToastElement = await this.toastCtrl.create(
            {
                id: this.claimId,
                message: toastMessage,
                position: 'bottom',
                duration: 3000,
            },
        );

        return toast.present().then(() => {
            this.backToClaim();
        });
    }

    private buildClaimUpdateForm(): void {
        this.claimNumberUpdateForm = this.formBuilder.group({
            claimNumber: [
                this.claimNumber,
                [
                    Validators.required,
                    this.sameClaimNumberValidator(),
                    Validators.pattern(RegularExpressions.claimNumber),
                ],
            ],
            assignmentType: ['', [Validators.required]],
            isReusePreviousClaimNumber: [false, []],
        });
    }

    private sameClaimNumberValidator(): ValidatorFn {
        return (control: AbstractControl): { [key: string]: any } | null => {
            let isCurrentClaimNumberSameAsOld: boolean = control.value == this.claimNumber;
            let isCustomType: boolean = this.claimNumberUpdateForm &&
                this.claimNumberUpdateForm.get('assignmentType').value == this.ClaimNumberAssignmentMethod.Custom;
            return (isCurrentClaimNumberSameAsOld && isCustomType && control.dirty) ?
                ({ sameNumberError: { value: true } }) : null;
        };
    }
}
