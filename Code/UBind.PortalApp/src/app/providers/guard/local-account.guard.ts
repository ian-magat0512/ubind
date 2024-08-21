import { Injectable } from "@angular/core";
import {
    ActivatedRouteSnapshot, CanActivate, NavigationExtras, Router, RouterStateSnapshot, UrlTree,
} from "@angular/router";
import { AppConfig } from "@app/models/app-config";
import { AuthenticationMethodType } from "@app/models/authentication-method-type.enum";
import { Errors } from "@app/models/errors";
import {
    PortalLocalAccountLoginMethodResourceModel,
    PortalLoginMethodResourceModel,
    PortalSamlLoginMethodResourceModel,
} from "@app/resource-models/portal/portal-login-method.resource-model";
import { PortalApiService } from "@app/services/api/portal-api.service";
import { AppConfigService } from "@app/services/app-config.service";
import { NavProxyService } from "@app/services/nav-proxy.service";
import { ParentFrameMessageService } from "@app/services/parent-frame-message.service";
import { SharedAlertService } from "@app/services/shared-alert.service";
import { PageRouteIndentifier } from "@app/helpers/page-route-identifier.helper";

/**
 * Guard for pages which are only available when local account authentication is enabled
 * e.g. password reset, create account, activate account
 */
 @Injectable({
     providedIn: "root",
 })
export class LocalAccountGuard implements CanActivate {

    private appConfig: AppConfig;
    private portalId: string;
    private tenantId: string;
    private apiBaseUrl: string;

    public constructor(
        private appConfigService: AppConfigService,
        private portalApiService: PortalApiService,
        private navProxy: NavProxyService,
        private router: Router,
        private sharedAlertService: SharedAlertService,
        private parentFrameMessageService: ParentFrameMessageService,
    ) {
        this.appConfigService.appConfigSubject.subscribe(
            (appConfig: AppConfig) => {
                this.appConfig = appConfig;
                this.portalId = appConfig.portal.portalId;
                this.tenantId = appConfig.portal.tenantId;
                this.apiBaseUrl = appConfig.portal.api.baseUrl;
            },
        );
    }

    public async canActivate(
        routeSnapshot: ActivatedRouteSnapshot,
        state: RouterStateSnapshot,
    ): Promise<boolean | UrlTree> {
        const routeId: PageRouteIndentifier = routeSnapshot.data
            ? routeSnapshot.data.routeIdentifier
            : null;

        if (!routeId) {
            // if it doesn't have an identifier, then it's not a page that we need to check for local account
            // authentication
            return true;
        }

        if (this.appConfigService.isMasterPortal() && routeId === PageRouteIndentifier.CreateAccountPage) {
            // you cannot self create an account for the master portal
            return this.getLoginPageUrlTree(this.appConfig.portal.environment);
        }

        return this.portalApiService.getLoginMethods(this.portalId, this.tenantId)
            .toPromise()
            .then((loginMethods: Array<PortalLoginMethodResourceModel>) => {
                // if the only login method is a SAML method, then redirect them to the saml sign on service url
                if (loginMethods.length === 1 && loginMethods[0].typeName === AuthenticationMethodType.Saml) {
                    let samlMethod: PortalSamlLoginMethodResourceModel
                        = <PortalSamlLoginMethodResourceModel>loginMethods[0];
                    const ssoInitiationUrl: string = `${this.apiBaseUrl}tenant/${this.tenantId}/saml/`
                        + `${samlMethod.authenticationMethodId}/initiate-single-sign-on`;
                    this.redirectToUrl(ssoInitiationUrl);
                    return false;
                }

                // there should really only be zero or one local account login methods, but to be safe we'll check
                // for an array
                const localAccountAuthenticationMethod: PortalLocalAccountLoginMethodResourceModel
                    = <PortalLocalAccountLoginMethodResourceModel>loginMethods
                        .find((loginMethod: PortalLoginMethodResourceModel) => {
                            return loginMethod.typeName == AuthenticationMethodType.LocalAccount;
                        });

                if (!localAccountAuthenticationMethod
                    && (routeId == PageRouteIndentifier.CreateAccountPage
                        || routeId == PageRouteIndentifier.RequestPasswordResetPage
                        || routeId == PageRouteIndentifier.ResetPasswordPage
                        || routeId == PageRouteIndentifier.PasswordExpiredPage)
                ) {
                    // if there is no local account authentication method, then redirect them to the login page
                    this.sharedAlertService.showToast(`You must manage your account with your identity provider.`);
                    return this.getLoginPageUrlTree(this.appConfig.portal.environment);
                }

                if (routeId == PageRouteIndentifier.CreateAccountPage) {
                    // determine if there is any way to self register
                    const allowSelfRegistration: boolean
                        = loginMethods.some((loginMethod: PortalLoginMethodResourceModel) => {
                            return loginMethod.typeName == AuthenticationMethodType.LocalAccount
                                && (<PortalLocalAccountLoginMethodResourceModel>loginMethod).allowSelfRegistration;
                        });
                    if (allowSelfRegistration) {
                        return true;
                    } else {
                        this.sharedAlertService.showToast(`The ability to register your own account in the portal is `
                            + `not currently enabled. Please contact support or your administrator if you would like `
                            + `an account to be created for you.`);
                        return this.getLoginPageUrlTree(this.appConfig.portal.environment);
                    }
                }

                // if we get here, then it's probably a misconfiguration issue and someone has added this route guard
                // to a non-account management type page.
                throw Errors.General.Unexpected(
                    "LocalAccountGuard was specified for a page which is not an account management page.");
            });
    }

    private getLoginPageUrlTree(environment: string): UrlTree {
        const extras: NavigationExtras = { queryParams: { environment: environment } };
        return this.router.createUrlTree(this.navProxy.injectCommandsWithTenantOrgPortalAndPath(['login']), extras);
    }

    private redirectToUrl(url: string): void {
        if (window.self !== window.top) {
            this.parentFrameMessageService.sendRedirectMessage(url);
        } else {
            window.location.href = url;
        }
    }
}
