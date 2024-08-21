import { Injectable } from '@angular/core';
import {
    CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router, UrlTree,
} from '@angular/router';
import { AuthenticationService } from '@app/services/authentication.service';
import { LoginRedirectService } from '../../services/login-redirect.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { ExternalRedirectService } from '@app/services/external-redirect.service';
import { HangfireCommons } from '@app/models/hangfire-commons.enum';

/**
 * This is a small interface that only used for ExternalRedirectGuard class
 * Use this to contain the url and redirection flag/status
 */
interface RedirectParameter {
    url: string | null;
    redirected: boolean | null;
}

/**
 * The external redirect guard is responsible for redirecting to other url
 * e.g Hangfire
 */
@Injectable({ providedIn: 'root' })
export class ExternalRedirectGuard implements CanActivate {
    public constructor(
        private authService: AuthenticationService,
        private loginRedirectService: LoginRedirectService,
        private navProxy: NavProxyService,
        private router: Router,
        private externalRedirectService: ExternalRedirectService,
    ) {
    }

    public async canActivate(
        next: ActivatedRouteSnapshot,
        state: RouterStateSnapshot,
    ): Promise<boolean | UrlTree> {
        const url: string = state.url;
        const parameters: RedirectParameter =
            this.getUrlRedirectionParameters(next);
        const authenticated: boolean = this.authService.isAuthenticated();
        if (authenticated) {
            if (this.stripQueryFromUrl(url).endsWith('/login')
                && parameters.url != ''
                && !parameters.redirected) {
                if (parameters.url == HangfireCommons.DashboardLocationValue) {
                    this.externalRedirectService.goToHangfireDashboard();
                    this.loginRedirectService.clear();
                    return true;
                } else if (parameters.url.length > 0) {
                    this.externalRedirectService.goToExternalUrl(
                        `${parameters.url}?${HangfireCommons.RedirectionFlag}=true`);
                    this.loginRedirectService.clear();
                    return true;
                }
                this.loginRedirectService.redirect();
                return true;
            } else if (parameters.url != ''
                && parameters.redirected) {
                const externalUrl: string = this.stripQueryFromUrl(parameters.url);
                this.loginRedirectService.clear();
                this.authService.removeUserData();
                this.externalRedirectService.goToExternalUrl(`${externalUrl}`);
                return false;
            }   else {
                return true;
            }
        }   else {
            return true;
        }
    }

    /**
     * Get the url to redirect and the flag to determine if already been redirected once
     * url : can be a plain-text or base64 encoded string
     * redirected : use to avoid infinite redirection
     */
    private getUrlRedirectionParameters(
        next: ActivatedRouteSnapshot,
    ): RedirectParameter {
        const queryStringredirected: string | null =
            this.getQueryStringFromActivatedRouteSnapshot(
                HangfireCommons.RedirectionFlag,
                next) ?? 'false';
        const queryStringUrl: string | null =
            this.getQueryStringFromActivatedRouteSnapshot(
                HangfireCommons.DashboardLocationFlag,
                next) ?? '';
        const redirectionParameters: RedirectParameter = {
            url: this.externalRedirectService.decodeBase64Url(queryStringUrl),
            redirected: JSON.parse(queryStringredirected),
        };
        return redirectionParameters;
    }

    /**
     *  Get Query String by Key in validate ActivatedRouteSnapshot object instance
     */
    private getQueryStringFromActivatedRouteSnapshot(
        key: string,
        next: ActivatedRouteSnapshot,
    ): string | null {
        if (key === null) {
            return null;
        }
        return key in (next?.queryParams ?? {})
            ? next.queryParams[key] : null;
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
}

