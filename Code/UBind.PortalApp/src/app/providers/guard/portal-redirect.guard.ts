import { Injectable } from "@angular/core";
import {
    ActivatedRouteSnapshot, CanActivate, NavigationExtras, Router, RouterStateSnapshot, UrlTree,
} from "@angular/router";
import { UrlHelper } from "@app/helpers";
import { RouteHelper } from "@app/helpers/route.helper";
import { AccountApiService } from "@app/services/api/account-api.service";
import { AppConfigService } from "@app/services/app-config.service";
import { AuthenticationService } from "@app/services/authentication.service";
import { ErrorHandlerService } from "@app/services/error-handler.service";
import { NavProxyService } from "@app/services/nav-proxy.service";
import { Location } from '@angular/common';
import { ParentFrameMessageService } from "@app/services/parent-frame-message.service";

/**
 * This route guard is used to redirect the user to the a whole different portal
 * if they login to the wrong one.
 */
@Injectable({
    providedIn: 'root',
})
export class PortalRedirectGuard implements CanActivate {

    public constructor(
        private router: Router,
        private routeHelper: RouteHelper,
        private authenticationService: AuthenticationService,
        private appConfigService: AppConfigService,
        private navProxyService: NavProxyService,
        private errorHandlerService: ErrorHandlerService,
        private accountApiService: AccountApiService,
        private location: Location,
        private parentFrameMessageService: ParentFrameMessageService,
    ) {
    }

    public async canActivate(
        routeSnapshot: ActivatedRouteSnapshot,
        state: RouterStateSnapshot,
    ): Promise<boolean | UrlTree> {
        if (!this.authenticationService.isAuthenticated()) {
            return true;
        }
        if (this.authenticationService.isMasterUser()) {
            return true;
        }
        const userPortalId: string = this.authenticationService.userPortalId;
        if (!userPortalId) {
            // the user is logged in but doesn't have a portal id, let's logout
            const didRedirect: boolean = await this.authenticationService.logout();
            if (didRedirect) {
                return false;
            } else {
                // redirect to login page
                const extras: NavigationExtras = this.navProxyService.addEnvironmentToQueryParam();
                return this.router.createUrlTree(
                    this.navProxyService.injectCommandsWithTenantOrgPortalAndPath(['login']), extras);
            }
        }
        const portalIsNotUsersPortal: boolean
            = userPortalId != this.appConfigService.currentConfig.portal.portalId;
        if (portalIsNotUsersPortal) {
            // the user is logged into the wrong portal, let's redirect them
            let didRedirect: boolean = false;
            try {
                didRedirect = await this.redirectToUsersPortal(state.url);
            } catch (error) {
                this.authenticationService.removeUserData();
                this.errorHandlerService.handleError(error);

                // we had an error so let's logout
                const extras: NavigationExtras = this.navProxyService.addEnvironmentToQueryParam();
                return this.router.createUrlTree(
                    this.navProxyService.injectCommandsWithTenantOrgPortalAndPath(['logout']), extras);
            }
            return !didRedirect;
        }
        return true;
    }

    private async redirectToUsersPortal(existingUrlPath: string): Promise<boolean> {
        const existingProtocol: string = this.getWindowLocation().protocol;
        const existingHost: string = this.getWindowLocation().host;
        const existingFullPath: string = this.location.prepareExternalUrl(existingUrlPath);
        const existingUrl: string
            = existingProtocol + '//' + existingHost + existingFullPath;
        const existingPortalBaseUrl: string = UrlHelper.getPortalBaseUrl(existingUrl);
        /* tslint:disable-next-line */
        return new Promise((resolve: (value: boolean) => void, reject: (reason?: any) => void) => {
            this.accountApiService.getPortalUrl().subscribe((usersPortalBaseUrl: string) => {
                let isIframed: boolean = this.isIframed();
                let parentUrl: string = this.parentFrameMessageService.getParentUrl();
                if ((!isIframed && usersPortalBaseUrl != existingPortalBaseUrl)
                    || (isIframed && parentUrl != usersPortalBaseUrl)) {
                    const cleanedExistingUrlPath: string = UrlHelper.stripInjectorQueryParams(existingUrlPath);
                    const portalUrlWithPath: string = this.routeHelper.addPathAsQueryParamToPortalBaseUrl(
                        usersPortalBaseUrl,
                        cleanedExistingUrlPath);
                    if (isIframed) {
                        // strip out the injector query params
                        let cleanedUrl: string = UrlHelper.stripInjectorQueryParams(portalUrlWithPath);
                        console.log('Asking injector to redirect to: ' + cleanedUrl);

                        // We should send a message to the parent frame so that
                        // the ubind injector can redirect us to the correct portal location
                        this.parentFrameMessageService.sendRedirectMessage(cleanedUrl);
                    } else {
                        // if we're not iframed, do a hard redirect to the new portal url
                        console.log('Redirecting to: ' + portalUrlWithPath);
                        window.location.href = portalUrlWithPath;
                    }
                    resolve(true);
                } else {
                    // the redirect wasn't necessary
                    resolve(false);
                }
            }, (error: any) => {
                reject(error);
            });
        });
    }

    private getWindowLocation(): any {
        return window.location;
    }

    private isIframed(): boolean {
        return window.parent !== window.self;
    }
}
