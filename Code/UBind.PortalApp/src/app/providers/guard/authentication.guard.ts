import { Injectable } from '@angular/core';
import {
    CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router, UrlTree, NavigationExtras,
} from '@angular/router';
import { AuthenticationService } from '@app/services/authentication.service';
import { LoginRedirectService } from '../../services/login-redirect.service';
import { AppConfigService } from '@app/services/app-config.service';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { Errors } from '@app/models/errors';
import { PortalUserType } from '@app/models/portal-user-type.enum';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { ExternalRedirectService } from '@app/services/external-redirect.service';

/**
 * The authentication guard will redirect a user to the login page if they are not logged in.
 */
@Injectable({ providedIn: 'root' })
export class AuthenticationGuard implements CanActivate {
    public constructor(
        private authService: AuthenticationService,
        private loginRedirectService: LoginRedirectService,
        private appConfigService: AppConfigService,
        private errorHandlerService: ErrorHandlerService,
        private navProxy: NavProxyService,
        private router: Router,
        private userPath: UserTypePathHelper,
        private externalRedirectService: ExternalRedirectService,
    ) {
    }

    public async canActivate(
        next: ActivatedRouteSnapshot,
        state: RouterStateSnapshot,
    ): Promise<boolean | UrlTree> {
        const url: string = state.url;
        // check for a jwt in the url and if it's there log the user in
        const jwt: string = this.getJwtFromUrl();
        if (jwt) {
            await this.authService.loginWithJwt(jwt);
        }

        const authenticated: boolean = this.authService.isAuthenticated();
        if (authenticated) {
            if (this.stripQueryFromUrl(url).endsWith('/login')) {
                // since we are already logged in and someone is going to the login page, we'll clear any existing
                // redirect path and redirect to the home page
                this.loginRedirectService.clear();
                return this.router.createUrlTree(
                    this.navProxy.injectCommandsWithTenantOrgPortalAndPath([this.userPath.home]));
            } else if (this.authService.userPortalId == this.appConfigService.currentConfig.portal.portalId) {
                // the user's portal id matches the current portal id, so they are allowed to access the portal
                return true;
            } else if (this.isUserTryingToAccessAnotherOrganisationsPortal()) {
                this.errorHandlerService.handleError(Errors.User.CannotAccessAnotherOrganisation(
                    this.appConfigService.currentConfig.portal.organisationAlias));
                return false;
            } else if (this.isAgentTryingToAccessCustomerPortal()) {
                this.errorHandlerService.handleError(Errors.User.AgentCannotAccessCustomerPortal());
                return false;
            } else {
                return true;
            }
        } else if (this.stripQueryFromUrl(url).endsWith('/login')) {
            return true;
        } else {
            // store the path we were trying to reach and then redirect to the login page
            this.loginRedirectService.pathSegments =
                this.stripQueryFromUrl(url).split('/').filter((segment: string) => segment);
            this.loginRedirectService.queryParams = next.queryParams;
            const extras: NavigationExtras = this.navProxy.addEnvironmentToQueryParam();
            return this.router.createUrlTree(
                this.navProxy.injectCommandsWithTenantOrgPortalAndPath(['login']), extras);
        }
    }

    /**
     * Removes the query from the url, e.g.:
     * http://domain.com/path?queryParam=value => http://domain.com/path
     */
    private stripQueryFromUrl(url: string): string {
        const questionMarkIndex: number = url.indexOf('?');
        if (questionMarkIndex > 0) {
            return url.substring(0, questionMarkIndex);
        }
        return url;
    }

    private isUserTryingToAccessAnotherOrganisationsPortal(): boolean {
        const portalOrganisationId: string = this.appConfigService.currentConfig.portal.organisationId;
        return this.authService.userOrganisationId != portalOrganisationId
            && !this.appConfigService.currentConfig.portal.isDefaultOrganisation;
    }

    private isAgentTryingToAccessCustomerPortal(): boolean {
        return this.authService.isAgent()
            && this.appConfigService.currentConfig.portal.portalUserType == PortalUserType.Customer;
    }

    private getJwtFromUrl(): string {
        const urlFragment: string = window.location.hash;
        if (urlFragment.startsWith('#token=')) {
            const urlSafeJwt: string = urlFragment.substring(7);
            const decodedJwt: string = decodeURIComponent(urlSafeJwt);
            return decodedJwt;
        }
        return null;
    }
}
