import { Component, ViewChild, ElementRef, OnInit } from '@angular/core';
import { AuthenticationService } from '@app/services/authentication.service';
import { PolicyApiService } from '@app/services/api/policy-api.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { LoginRedirectService } from '@app/services/login-redirect.service';
import { SharedToastService } from '@app/services/shared-toast.service';
import { UrlHelper } from '@app/helpers';
import { TenantApiService } from '@app/services/api/tenant-api.service';
import { TenantResourceModel } from '@app/resource-models/tenant.resource-model';
import { AppConfigService } from '@app/services/app-config.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { ActivatedRouteSnapshot } from '@angular/router';

/**
 * Export logout page component class
 * TODO: Write a better class header: action of user for logout.
 */
@Component({
    selector: 'app-logout',
    template: '',
})
export class LogOutPage implements OnInit {
    @ViewChild('focusElement', { read: ElementRef, static: true }) public focusElement: any;

    public constructor(
        private authenticationService: AuthenticationService,
        private loginRedirectService: LoginRedirectService,
        public navProxy: NavProxyService,
        public policyApiService: PolicyApiService,
        private sharedToastService: SharedToastService,
        private tenantApiService: TenantApiService,
        private appConfigService: AppConfigService,
        private routeHelper: RouteHelper,
    ) {
    }

    public async ngOnInit(): Promise<void> {
        let oldAlias: string = UrlHelper.getTenantAliasFromUrl();
        let logoutMessage: string = `Your account has been logged out`;
        let urlChangeMessageSent: boolean = false;
        this.authenticationService.removeUserData();

        // check if there is change in tenant alias.
        if (this.authenticationService.isAuthenticated()) {
            await this.tenantApiService.getTenantAlias(this.authenticationService.tenantId)
                .toPromise()
                .then((tenant: TenantResourceModel) => {
                    let currentAlias: string = this.appConfigService.currentConfig.portal.tenantAlias = tenant.alias;
                    if (oldAlias != tenant.alias) {
                        let message: string = `URL Tenant Alias changed from '${oldAlias}' to '${currentAlias}', `;
                        message += logoutMessage;
                        this.presentToast(message);
                        urlChangeMessageSent = true;
                        this.loginRedirectService.clear();
                    } else {
                        this.setRedirectionParams();
                    }
                });

            await this.authenticationService.logout();
            if (!urlChangeMessageSent) {
                this.presentToast(logoutMessage);
            }
        }

        this.navProxy.navigateRoot(['login'], null, null, this.appConfigService.currentConfig.portal.tenantAlias);
    }

    public async presentToast(_message: string): Promise<void> {
        const toast: HTMLIonToastElement = await this.sharedToastService.create({
            message: _message,
            duration: 3000,
        });

        return await toast.present();
    }

    private setRedirectionParams(): void {
        const url: string = this.routeHelper.PreviousUrl;
        const snapshot: ActivatedRouteSnapshot = this.routeHelper.PreviousActivatedRouteSnapShot;

        if (url) {
            this.loginRedirectService.pathSegments =
                this.stripQueryFromUrl(url).split('/').filter((segment: string) => segment);
        }
        if (snapshot) {
            this.loginRedirectService.queryParams = snapshot.queryParams;
        }
    }

    /**
     * Remove's the query from the url, e.g.:
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
