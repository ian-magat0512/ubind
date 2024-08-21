import { Injectable } from "@angular/core";
import { LoginRedirectService } from "./login-redirect.service";
import { PortalSamlLoginMethodResourceModel } from "@app/resource-models/portal/portal-login-method.resource-model";
import { AppConfigService } from "./app-config.service";
import { AppConfig } from "@app/models/app-config";

/**
 * Provides services for working with alternate authentication methods such as SAML
 */
@Injectable({
    providedIn: 'root',
})
export class AuthenticationMethodService {
    private apiBaseUrl: string;
    private tenantId: string;
    private portalId: string;

    public constructor(
        private loginRedirectService: LoginRedirectService,
        private appConfigService: AppConfigService,
    ) {
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.tenantId = appConfig.portal.tenantId;
            this.apiBaseUrl = appConfig.portal.api.baseUrl;
            this.portalId = appConfig.portal.portalId;
        });
    }

    public generateSamlSsoInitiationUrl(loginMethod: PortalSamlLoginMethodResourceModel ): string {
        let ssoInitiationUrl: string = `${this.apiBaseUrl}tenant/${this.tenantId}/saml/`
            + `${loginMethod.authenticationMethodId}/initiate-single-sign-on`;
        if (this.portalId) {
            ssoInitiationUrl = `${ssoInitiationUrl}?portalId=${this.portalId}`;
        }
        let redirectPath: string = this.loginRedirectService.getRedirectPath();
        if (redirectPath) {
            // make sure the redirect path doesn't contain the tenant, organisation, or partal aliases
            const pathSegmentPos: number = redirectPath.indexOf('/path/');
            if (pathSegmentPos !== -1) {
                redirectPath = redirectPath.substring(pathSegmentPos + 6);
            }
            ssoInitiationUrl = ssoInitiationUrl.indexOf('?') === -1
                ? `${ssoInitiationUrl}?path=${redirectPath}`
                : `${ssoInitiationUrl}&path=${redirectPath}`;
        }
        return ssoInitiationUrl;
    }
}
