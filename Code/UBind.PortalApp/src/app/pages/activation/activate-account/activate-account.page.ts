import { Component, OnInit, OnDestroy } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { LoadingController } from '@ionic/angular';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { InvitationApiService } from '@app/services/api/invitation-api.service';
import { AuthenticationService } from '@app/services/authentication.service';
import { ParentFrameMessageService } from '@app/services/parent-frame-message.service';
import { RegularExpressions } from '@app/helpers';
import { scrollbarStyle } from '@assets/scrollbar';
import { EntryPage } from '../../login/entry.page';
import { AppConfigService } from '@app/services/app-config.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { finalize } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';
import { ProblemDetails } from '@app/models/problem-details';
import { UserAuthorisationModel } from '@app/resource-models/authentication-response.resource-model';
import { PortalApiService } from '@app/services/api/portal-api.service';
import { Title } from '@angular/platform-browser';
import { UserPasswordResourceModel } from '@app/resource-models/user/user-password.resource-model';
import { EventService } from '@app/services/event.service';
import { ErrorHandlerService } from '@app/services/error-handler.service';

/**
 * Activates a user account using an invitation token.
 */
@Component({
    selector: 'app-activate-account',
    templateUrl: './activate-account.page.html',
    styleUrls: [
        '../../../../assets/css/scrollbar-div.css',
        './activate-account.page.scss',
    ],
    styles: [
        scrollbarStyle,
    ],
})
export class ActivateAccountPage extends EntryPage implements OnInit, OnDestroy {

    private activateAccountForm: FormGroup;
    public errorDisplay: string = '';
    public formHasErrors: boolean = false;
    public isLoading: boolean;
    public invitationValid?: boolean = null;
    private userId: string;
    private invitationId: string;

    public constructor(
        private routeHelper: RouteHelper,
        private sharedAlertService: SharedAlertService,
        private authService: AuthenticationService,
        private formBuilder: FormBuilder,
        private invitationService: InvitationApiService,
        private loadCtrl: LoadingController,
        private navProxy: NavProxyService,
        private message: ParentFrameMessageService,
        appConfigService: AppConfigService,
        public layoutManager: LayoutManagerService,
        portalApiService: PortalApiService,
        titleService: Title,
        private eventService: EventService,
        private errorHandlerService: ErrorHandlerService,
    ) {
        super(appConfigService, portalApiService, titleService);
        this.activateAccountForm = this.formBuilder.group(
            {
                password: ['',
                    Validators.compose([
                        Validators.required,
                        Validators.pattern(RegularExpressions.strongPassword),
                    ])],
                confirmPassword: ['', Validators.compose([Validators.required])],
            },
            { validator: this.matchingPasswords('password', 'confirmPassword') },
        );
        this.invokeAppLoadCallback();
    }

    public ngOnInit(): void {
        this.userId = this.routeHelper.getParam('userId');
        this.invitationId = this.routeHelper.getParam('invitationId');
        if (this.authService.isAuthenticated()) {
            if (this.authService.userId == this.userId) {
                this.sharedAlertService.showToast(`Account already activated`);
                return this.navProxy.redirectToHome();
            } else {
                this.authService.logout();
            }
        }
        this.validateInvitation();
    }

    private async validateInvitation(): Promise<void> {
        const loading: HTMLIonLoadingElement = await this.loadCtrl.create({ message: 'Validating...' });
        await loading.present().then(() => {
            this.subscriptions.push(
                this.invitationService.validateActivation(this.userId, this.invitationId, this.portalTenantId)
                    .pipe(finalize(() => {
                        loading.dismiss();
                        this.isLoading = false;
                    }))
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
                        },
                    ),
            );
        });
    }

    public async userDidTapSetPassword(value: any): Promise<void> {
        if (this.activateAccountForm.valid) {
            this.formHasErrors = false;
            const loader: HTMLIonLoadingElement = await this.loadCtrl.create({ message: 'Please wait...' });
            await loader.present().then(() => {
                const userPasswordModel: UserPasswordResourceModel = {
                    tenant: this.portalTenantId,
                    userId: this.userId,
                    invitationId: this.invitationId,
                    clearTextPassword: value.password,
                    organisation: this.portalOrganisationId,
                };

                this.invitationService.setPassword(userPasswordModel)
                    .pipe(finalize(() => loader.dismiss()))
                    .subscribe(
                        (response: UserAuthorisationModel) => {
                            try {
                                this.authService.processAuthorisationModel(response);
                                this.authService.setupSessionExpiry();
                                this.eventService.userLoggedIn(response.userId);
                                this.sharedAlertService.showToast(`Your account has been successfully activated`);
                                this.navProxy.redirectToHome();
                            } catch (err) {
                                this.errorHandlerService.handleErrorResponse(err);
                                this.navProxy.navigate(['login']);
                            }
                        },
                        (err: HttpErrorResponse) => {
                            this.errorHandlerService.handleErrorResponse(err);
                            this.navProxy.navigate(['login']);
                        });
            });
        } else {
            this.formHasErrors = true;
        }
    }

    public userDidTapCancel(): any {
        this.navProxy.navigate(['login']);
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
