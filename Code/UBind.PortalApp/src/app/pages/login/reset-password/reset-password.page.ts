import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormGroup, FormBuilder, Validators, AbstractControl } from '@angular/forms';
import { LoadingController } from '@ionic/angular';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { AppConfigService } from '@app/services/app-config.service';
import { InvitationApiService } from '@app/services/api/invitation-api.service';
import { ParentFrameMessageService } from '@app/services/parent-frame-message.service';
import { RegularExpressions } from '@app/helpers';
import { RouteHelper } from '@app/helpers/route.helper';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { EntryPage } from '../entry.page';
import { finalize } from 'rxjs/operators';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { HttpErrorResponse } from '@angular/common/http';
import { ProblemDetails } from '@app/models/problem-details';
import { PortalApiService } from '@app/services/api/portal-api.service';
import { Title } from '@angular/platform-browser';
import { UserPasswordResourceModel } from '@app/resource-models/user/user-password.resource-model';
import { UserAuthorisationModel } from '@app/resource-models/authentication-response.resource-model';
import { AuthenticationService } from '@app/services/authentication.service';
import { EventService } from '@app/services/event.service';
import { ErrorHandlerService } from '@app/services/error-handler.service';

/**
 * Export reset password page component class
 * This class manage reseting of password.
 */
@Component({
    selector: 'app-reset-password',
    templateUrl: './reset-password.page.html',
    styleUrls: [
        './reset-password.page.scss',
    ],
})
export class ResetPasswordPage extends EntryPage implements OnInit, OnDestroy {

    public resetPasswordForm: FormGroup;
    public errorDisplay: string = '';
    public formHasError: boolean = false;
    public invitationValid?: boolean = null;

    public constructor(
        private sharedAlertService: SharedAlertService,
        private authService: AuthenticationService,
        private formBuilder: FormBuilder,
        private invitationService: InvitationApiService,
        private loadCtrl: LoadingController,
        public navProxy: NavProxyService,
        private message: ParentFrameMessageService,
        protected routeHelper: RouteHelper,
        public layoutManager: LayoutManagerService,
        appConfigService: AppConfigService,
        portalApiService: PortalApiService,
        titleService: Title,
        private eventService: EventService,
        private errorHandlerService: ErrorHandlerService,
    ) {
        super(appConfigService, portalApiService, titleService);
        this.resetPasswordForm = this.formBuilder.group(
            {
                password: [
                    '',
                    Validators.compose([
                        Validators.required,
                        Validators.pattern(RegularExpressions.strongPassword),
                    ]),
                ],
                confirmPassword: ['', Validators.compose([Validators.required])],
            },
            {
                validator: this.matchingPasswords('password', 'confirmPassword'),
            },
        );
        this.invokeAppLoadCallback();
    }

    public ngOnInit(): void {
        this.validateInvitation();
    }

    public ngOnDestroy(): void {
        super.ngOnDestroy();
    }

    private async validateInvitation(): Promise<void> {
        const userId: string = this.routeHelper.getParam('userId');
        const invitationId: string = this.getInvitationId();
        const loading: HTMLIonLoadingElement = await this.loadCtrl.create({ message: 'Validating...' });

        await loading.present();
        this.subscriptions.push(
            this.invitationService.validatePasswordReset(userId, invitationId, this.portalTenantId)
                .pipe(finalize(() => loading.dismiss()))
                .subscribe(
                    () => this.invitationValid = true,
                    (err: HttpErrorResponse) => {
                        this.invitationValid = false;
                        if (ProblemDetails.isProblemDetailsResponse(err)) {
                            let problemDetails: ProblemDetails = ProblemDetails.fromJson(err.error);
                            this.sharedAlertService.showErrorWithCustomButtons(
                                problemDetails,
                                [{
                                    text: 'OK',
                                    handler: (): any => {
                                        this.navProxy.navigateForward(['login']);
                                    },
                                }],
                                false,
                            );
                        } else {
                            throw err;
                        }
                    }));
    }

    public async userDidTapResetPassword(value: any): Promise<void> {
        if (this.resetPasswordForm.valid) {
            this.formHasError = false;
            const loader: HTMLIonLoadingElement = await this.loadCtrl.create({ message: 'Please wait...' });
            const invitationId: string = this.getInvitationId();

            loader.present().then(() => {
                const userPasswordModel: UserPasswordResourceModel = {
                    tenant: this.portalTenantId,
                    invitationId: invitationId,
                    userId: this.routeHelper.getParam('userId'),
                    clearTextPassword: value.password,
                    organisation: this.portalOrganisationId,
                };
                this.invitationService.resetPassword(userPasswordModel)
                    .pipe(finalize(() => loader.dismiss()))
                    .subscribe((response: UserAuthorisationModel) => {
                        try {
                            this.authService.processAuthorisationModel(response);
                            this.authService.setupSessionExpiry();
                            this.eventService.userLoggedIn(response.userId);
                            this.sharedAlertService.showToast(`You've successfully set a new password for your `
                                + `account. Please keep it securely inside a password vault`);
                            this.navProxy.redirectToHome();
                        } catch (err) {
                            this.errorHandlerService.handleErrorResponse(err);
                            this.navProxy.navigate(['login']);
                        }
                    });
            });
        } else {
            this.formHasError = true;
        }
    }

    public userDidTapCancel(): void {
        this.navProxy.navigateRoot(['login']);
    }

    // Parameter 'invitation' is checked for backward compatibility
    private getInvitationId(): string {
        return this.routeHelper.getParam('invitationId')
            ?? this.routeHelper.getParam('invitation');
    }

    private matchingPasswords(passwordKey: string, confirmPasswordKey: string): any {
        return (group: FormGroup): { [key: string]: any } => {
            const password: AbstractControl = group.controls[passwordKey];
            const confirmPassword: AbstractControl = group.controls[confirmPasswordKey];

            if (password.value !== confirmPassword.value) {
                return {
                    mismatchedPasswords: true,
                };
            }
        };
    }

    private invokeAppLoadCallback(): void {
        const payload: any = {
            'status': 'success',
            'message': {},
        };

        this.message.sendMessage('appLoad', payload);
    }
}
