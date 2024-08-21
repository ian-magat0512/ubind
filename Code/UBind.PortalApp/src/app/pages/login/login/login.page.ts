import { Component, ViewChild, ElementRef, OnDestroy, AfterViewChecked, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthenticationService } from '@app/services/authentication.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { AppConfigService } from '@app/services/app-config.service';
import { scrollbarStyle } from '@assets/scrollbar';
import { SubscriptionLike } from 'rxjs';
import { Result } from '@app/helpers/result';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { EntryPage } from '../entry.page';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { AccountLoginCredentialsResourceModel } from '@app/resource-models/account-login-credentials.resource-model';
import { LoginRedirectService } from '@app/services/login-redirect.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { PortalApiService } from '@app/services/api/portal-api.service';
import { Title } from '@angular/platform-browser';
import { ProblemDetails } from '@app/models/problem-details';
import { ErrorCodeTranslationHelper } from '@app/helpers/error-code-translation.helper';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { finalize, takeUntil } from 'rxjs/operators';
import {
    PortalLocalAccountLoginMethodResourceModel,
    PortalLoginMethodResourceModel,
} from '@app/resource-models/portal/portal-login-method.resource-model';
import { AuthenticationMethodType } from '@app/models/authentication-method-type.enum';
import { Errors } from '@app/models/errors';
import { ParentFrameMessageService } from '@app/services/parent-frame-message.service';
import { AppConfig } from '@app/models/app-config';
import { TypeHelper } from '@app/helpers/type.helper';
import { AuthenticationMethodService } from '@app/services/authentication-method.service';
import { RedirectService } from '@app/services/redirect.service';
import { ExternalRedirectService } from '@app/services/external-redirect.service';
import { HangfireCommons } from '@app/models/hangfire-commons.enum';
/**
 * The page for logging into the portal.
 */
@Component({
    selector: 'app-login',
    templateUrl: './login.page.html',
    styleUrls: [
        './login.page.scss',
        '../../../../assets/css/scrollbar-div.css',
    ],
    styles: [scrollbarStyle],
})
export class LoginPage extends EntryPage implements OnInit, OnDestroy, AfterViewChecked {
    @ViewChild('focusElement', { read: ElementRef, static: true }) public focusElement: any;

    public title: string = '';
    public authForm: FormGroup;
    public errorDisplay: string = '';
    public emailBlockErrorDisplay: string = '';
    public formHasError: boolean = false;
    public hasSubmitButtonBeenClicked: boolean;
    public redirectUrlMap: Map<string, any> = new Map<string, any>();
    protected subscriptions: Array<SubscriptionLike> = new Array<SubscriptionLike>();
    public allowSelfRegistration: boolean = false;
    public loginMethods: Array<PortalLoginMethodResourceModel>;
    // eslint-disable-next-line @typescript-eslint/naming-convention
    public AuthenticationMethodType: typeof AuthenticationMethodType = AuthenticationMethodType;
    private apiBaseUrl: string;
    private tenantId: string;


    public constructor(
        private authenticationService: AuthenticationService,
        private formBuilder: FormBuilder,
        private sharedLoaderService: SharedLoaderService,
        appConfigService: AppConfigService,
        public navProxy: NavProxyService,
        private sharedAlertService: SharedAlertService,
        public layoutManager: LayoutManagerService,
        protected activatedRoute: ActivatedRoute,
        private loginRedirectService: LoginRedirectService,
        private routeHelper: RouteHelper,
        portalApiService: PortalApiService,
        titleService: Title,
        private location: Location,
        private errorHandlerService: ErrorHandlerService,
        private parentFrameMessageService: ParentFrameMessageService,
        private authenticationMethodService: AuthenticationMethodService,
        private redirectService: RedirectService,
        private externalRedirectService: ExternalRedirectService,
    ) {
        super(appConfigService, portalApiService, titleService);
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.tenantId = appConfig.portal.tenantId;
            this.apiBaseUrl = appConfig.portal.api.baseUrl;
        });
        this.generateFormGroup();
    }

    public ngOnInit(): void {
        super.ngOnInit();
        let organisationAlias: string = this.routeHelper.getParam('portalOrganisationAlias');
        if (organisationAlias) {
            this.organisationName = this.appConfigService.organisationName;
        }
        if (!this.organisationName) {
            this.navProxy.navigate(['login', 'not-found'], { skipLocationChange: true });
        }
        this.loadLoginMethods();
    }

    public ngOnDestroy(): void {
        super.ngOnDestroy();
    }

    private async loadLoginMethods(): Promise<void> {
        this.isLoading = true;
        if (!this.appConfigService.isMasterPortal() && this.portalId) {
            this.portalApiService.getLoginMethods(this.portalId, this.routeHelper.getContextTenantAlias())
                .pipe(
                    takeUntil(this.destroyed),
                    finalize(() => this.isLoading = false),
                ).subscribe((loginMethods: Array<PortalLoginMethodResourceModel>) => {
                    this.loginMethods = loginMethods;

                    // if the only login method is a SAML method, then redirect them to the saml sign on service url
                    if (this.loginMethods.length === 1
                        && TypeHelper.isPortalSamlLoginMethodResourceModel(loginMethods[0])
                    ) {
                        const ssoInitiationUrl: string =
                            this.authenticationMethodService.generateSamlSsoInitiationUrl(loginMethods[0]);
                        this.redirectService.redirectToUrl(ssoInitiationUrl);
                        return;
                    }

                    // determine if there is any way to self register
                    this.allowSelfRegistration
                        = this.loginMethods.some((loginMethod: PortalLoginMethodResourceModel) => {
                            return loginMethod.typeName == AuthenticationMethodType.LocalAccount
                                && (<PortalLocalAccountLoginMethodResourceModel>loginMethod).allowSelfRegistration;
                        });
                });
        } else {
            // for the master portal
            this.loginMethods = new Array<PortalLoginMethodResourceModel>();
            this.loginMethods.push({
                name: 'Local',
                typeName: AuthenticationMethodType.LocalAccount,
                sortOrder: 0,
                authenticationMethodId: null,
                includeSignInButtonOnPortalLoginPage: true,
                signInButtonBackgroundColor: null,
                signInButtonIconUrl: null,
                signInButtonLabel: 'Sign In',
            });
            this.isLoading = false;
        }
    }

    public ngAfterViewChecked(): void {
        if (this.focusElement && this.authForm.controls["email"].untouched) {
            let inputFocusElement: HTMLIonInputElement = (<HTMLIonInputElement> this.focusElement.nativeElement);
            if (!inputFocusElement.classList.toString().includes("has-focus")) {
                inputFocusElement.setFocus();
            }
        }
    }

    public async userDidTapLocalAuthLogin(value: any): Promise<void> {
        this.hasSubmitButtonBeenClicked = true;

        this.emailBlockErrorDisplay = '';
        this.errorDisplay = '';

        if (this.authForm.valid) {
            try {
                this.formHasError = false;
                await this.sharedLoaderService.present('Signing in...');
                const credentials: AccountLoginCredentialsResourceModel = {
                    tenant: this.portalTenantId,
                    organisation: this.portalOrganisationId,
                    emailAddress: value.email,
                    plaintextPassword: value.password,
                };

                let loginResult: Result<void, any> = await this.authenticationService.login(credentials);
                this.sharedLoaderService.dismiss();
                if (loginResult.isSuccess) {
                    const redirectToUrl: string | null =
                        this.externalRedirectService.decodeBase64Url(
                            this.routeHelper.getParam(HangfireCommons.DashboardLocationFlag));
                    if (redirectToUrl == HangfireCommons.DashboardLocationValue) {
                        this.externalRedirectService.goToHangfireDashboard();
                    } else if (redirectToUrl != null && redirectToUrl != '') {
                        this.externalRedirectService.goToExternalUrl(redirectToUrl);
                    }
                    this.loginRedirectService.redirect();
                } else if (loginResult.isFailure) {
                    const appError: ProblemDetails = ProblemDetails.isProblemDetailsResponse(loginResult.error)
                        ? ProblemDetails.fromJson(loginResult.error.error)
                        : ProblemDetails.isProblemDetails(loginResult.error)
                            ? loginResult.error
                            : null;
                    if (appError) {
                        if (ErrorCodeTranslationHelper.isUserLoginAccountLocked(appError.Code)) {
                            await this.sharedAlertService.showWithOk(
                                'Email address locked',
                                'Due to 6 or more consecutive failed sign-in attempts this'
                                + '  email address has been blocked from further sign-in attempts.'
                                + ' For assistance please contact an administrator.',
                            );
                            this.emailBlockErrorDisplay = 'Email address blocked due to 6 or '
                                + 'more consecutive failed sign-in attempts';
                        } else if (ErrorCodeTranslationHelper.isUserPasswordExpired(appError.Code)) {
                            this.navProxy.navigate(
                                ['login', 'password-expired'],
                                { queryParams: { emailAddress: credentials.emailAddress } },
                            );
                        } else {
                            this.errorDisplay = appError.Detail;
                        }
                    } else if (ProblemDetails.isProblemDetails(loginResult.error)) {
                        this.errorHandlerService.handleError(loginResult.error);
                    } else {
                        console.error(loginResult.error);
                        this.errorDisplay = 'There was a problem signing you in. Please try again later.';
                    }
                } else {
                    this.errorDisplay = 'There was a problem signing you in. Please try again later.';
                }
                this.hasSubmitButtonBeenClicked = false;
            } catch (error) {
                this.hasSubmitButtonBeenClicked = false;
                throw error;
            } finally {
                this.sharedLoaderService.dismiss();
            }
        } else {
            this.formHasError = true;
        }
    }

    public userDidTapLoginMethod(loginMethod: PortalLoginMethodResourceModel): void {
        // if the login method is a SAML method, then redirect them to the saml sign on service url
        const typeName: string = loginMethod.typeName;
        if (TypeHelper.isPortalSamlLoginMethodResourceModel(loginMethod)) {
            const ssoInitiationUrl: string
                = this.authenticationMethodService.generateSamlSsoInitiationUrl(loginMethod);
            this.redirectService.redirectToUrl(ssoInitiationUrl);
            return;
        } else {
            throw Errors.General.NotImplemented("There is currently no implementation for logging in using the "
                + typeName + " authentication method.");
        }
    }

    public userDidTapResetPassword(): void {
        this.navProxy.navigate(['login', 'request-password']);
    }

    public userDidTapCreateAccount(): void {
        this.navProxy.navigate(['create-account']);
    }

    private generateFormGroup(): void {
        this.authForm = this.formBuilder.group({
            email: ['', [Validators.required, Validators.email]],
            password: ['', Validators.compose([Validators.required, Validators.minLength(8)])],
        });
    }
}
