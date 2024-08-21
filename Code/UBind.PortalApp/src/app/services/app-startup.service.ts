import { Injectable } from "@angular/core";
import { UrlHelper } from "@app/helpers/url.helper";
import { AppConfig } from "@app/models/app-config";
import { BehaviorSubject } from "rxjs";
import { AppConfigService } from "./app-config.service";
import { EventService } from "./event.service";
import { ParentFrameMessageService } from "./parent-frame-message.service";
import { ProblemDetails } from '@app/models/problem-details';
import { HttpClient, HttpErrorResponse, HttpParams } from "@angular/common/http";
import { FeatureSettingResourceModel } from "@app/resource-models/feature-setting.resource-model";
import { PortalAppContextModel } from "@app/models/portal-app-context.model";

/**
 * Performs application initialisation.
 */
@Injectable({ providedIn: 'root' })
export class AppStartupService {

    public isIframe: boolean = window.self !== window.top;
    private originalConfig: AppConfig;

    public constructor(
        private appConfigService: AppConfigService,
        private httpClient: HttpClient,
        private messageService: ParentFrameMessageService,
        private eventService: EventService,
    ) {
    }

    public async load(): Promise<boolean> {
        await this.loadJsonConfigFile();
        let tenantAlias: string
            = this.appConfigService.currentConfig.portal.tenantAlias
            = UrlHelper.getTenantAliasFromUrl();
        let portalAlias: string = this.appConfigService.currentConfig.portal.alias =
            UrlHelper.getPortalAliasFromUrl();
        this.appConfigService.currentConfig.portal.tenantName = '';
        let organisationAlias: string
            = this.appConfigService.currentConfig.portal.organisationAlias
            = UrlHelper.getOrganisationAliasFromUrl();
        this.appConfigService.currentConfig.portal.organisationName = '';

        if (tenantAlias) {
            await this.loadPortalAppContext(tenantAlias, organisationAlias, portalAlias);
            if (tenantAlias != 'ubind' && tenantAlias != 'master') {
                await this.loadFeatureSettings(
                    this.appConfigService.currentConfig.portal.tenantId,
                    this.appConfigService.currentConfig.portal.portalId);
            }
            let environment: any = this.appConfigService.getEnvironment();
            this.appConfigService.setEnvironment(environment);
            this.loadIframeResizer();
            this.messageService.sendMessage('appLoad', { status: 'success', message: {} });
        } else {
            this.appConfigService.initialisationErrorMessage
                = `We couldn't load the portal because no tenant alias was specified.`;
            console.error(this.appConfigService.initialisationErrorMessage);
            this.messageService.sendDisplayMessage(this.appConfigService.initialisationErrorMessage);
        }

        return Promise.resolve(true);
    }

    private async loadJsonConfigFile(): Promise<void> {
        return this.httpClient
            .get<AppConfig>('./assets/appConfig.json')
            .toPromise()
            .then((res: AppConfig) => {
                let apiOrigin: string = location.origin;
                res.portal.auth0.audience = apiOrigin;
                res.portal.api.accountUrl = `${apiOrigin}/api/v1/`;
                res.portal.api.baseUrl = `${apiOrigin}/api/v1/`;
                res.portal.baseUrl = `${apiOrigin}/portal/`;
                res.formsApp.baseUrl = `${apiOrigin}/`;
                this.originalConfig = res;
                this.appConfigService.currentConfig = this.originalConfig;
                this.appConfigService.appConfigSubject
                    = new BehaviorSubject<AppConfig>(this.originalConfig);
            });
    }

    public async loadPortalAppContext(
        tenantAlias: string,
        organisationAlias?: string,
        portalAlias?: string,
    ): Promise<void> {
        let portalAppContext: PortalAppContextModel
            = await this.getPortalAppContext(tenantAlias, organisationAlias, portalAlias);
        this.applyPortalAppContext(portalAppContext);
    }

    private async getPortalAppContext(
        tenantAlias: string,
        organisationAlias?: string,
        portalAlias?: string,
    ): Promise<PortalAppContextModel> {
        let options: HttpParams = new HttpParams();
        options = options.append('tenant', tenantAlias);
        if (portalAlias) {
            options = options.append('portal', portalAlias);
        }

        // A 'null' organisation alias is used as a placeholder in the URL when an emmbed portal
        // has a defined portal alias but no organisation alias is given.
        // See 'PopulateNullOrganisationAliasGuard' for more information.
        if (organisationAlias && organisationAlias != 'null') {
            options = options.append('organisation', organisationAlias);
        }
        return this.httpClient
            .get<PortalAppContextModel>(
                `${this.appConfigService.currentConfig.portal.api.baseUrl}app-context/portal`,
                { params: options })
            .toPromise().catch((error: HttpErrorResponse) => {
                if (ProblemDetails.isProblemDetailsResponse(error)) {
                    const details: ProblemDetails = ProblemDetails.fromJson(error.error);
                    this.messageService.sendDisplayMessage(details.Detail);
                } else {
                    this.messageService.sendDisplayMessage(error.message);
                }
                throw error;
            });
    }

    private applyPortalAppContext(portalAppContext: PortalAppContextModel): void {
        let portalConfig: any = this.appConfigService.currentConfig.portal;
        portalConfig.organisationId = portalAppContext.organisationId;
        portalConfig.organisationName = portalAppContext.organisationName;
        portalConfig.organisationAlias = portalAppContext.organisationAlias;
        portalConfig.tenantId = portalAppContext.tenantId;
        portalConfig.tenantName = portalAppContext.tenantName;
        portalConfig.alias = portalAppContext.portalAlias;
        portalConfig.title = portalAppContext.portalTitle;
        portalConfig.stylesheetUrl = portalAppContext.portalStylesheetUrl;
        portalConfig.styles = portalAppContext.portalStyles;
        portalConfig.isDefaultOrganisation = portalAppContext.isDefaultOrganisation;
        portalConfig.customDomain = portalAppContext.customDomain;
        portalConfig.isMutual = portalAppContext.isMutual;
        portalConfig.portalId = portalAppContext.portalId;
        portalConfig.portalAlias = portalAppContext.portalAlias;
        portalConfig.portalUserType = portalAppContext.portalUserType;
        portalConfig.isDefaultAgentPortal = portalAppContext.isDefaultAgentPortal;
        this.loadPortalStyles();
        this.appConfigService.appConfigSubject.next(this.appConfigService.currentConfig);
    }

    private async loadFeatureSettings(tenantId: string, portalId?: string): Promise<void> {
        let options: HttpParams = new HttpParams();
        options = options.append('tenantId', tenantId);
        let baseUrl: string = this.appConfigService.currentConfig.portal.api.baseUrl;
        let endpointUrl: string = portalId == null
            ? `${baseUrl}tenant/${tenantId}/feature-setting`
            : `${baseUrl}tenant/${tenantId}/portal/${portalId}/feature-setting`;

        return this.httpClient.get<Array<FeatureSettingResourceModel>>(endpointUrl, { params: options }).toPromise()
            .then((featureSettings: Array<FeatureSettingResourceModel>) => {
                this.appConfigService.setFeatureSettings(featureSettings);
            });
    }

    public onRouteChange(): void {
        this.eventService.routeChangedSubject$.subscribe(async (url: string) => {
            let urlTenantAlias: string = UrlHelper.getTenantAliasFromUrl();
            if (urlTenantAlias && this.appConfigService.currentConfig.portal.tenantAlias != urlTenantAlias) {
                // load the new tenant
                let context: PortalAppContextModel
                    = await this.getPortalAppContext(urlTenantAlias);
                this.applyPortalAppContext(context);
            }
        });
    }

    private loadPortalStyles(): void {
        let stylesheetUrl: string = this.appConfigService.currentConfig.portal.stylesheetUrl;
        if (stylesheetUrl) {
            let head: HTMLHeadElement = document.head;
            let link: HTMLLinkElement = document.createElement("link");

            link.type = "text/css";
            link.rel = "stylesheet";
            link.href = stylesheetUrl;

            head.appendChild(link);
        }

        let styles: string = this.appConfigService.currentConfig.portal.styles;
        if (styles) {
            // insert the style tag
            let head: HTMLHeadElement = document.head || document.getElementsByTagName('head')[0];
            let el: HTMLStyleElement = document.createElement('style');
            el.id = 'portal-inline-styles';
            el.appendChild(document.createTextNode(styles));
            head.appendChild(el);
        }
    }

    private loadIframeResizer(): void {
        if (this.isIframe) {
            const head: HTMLHeadElement = document.head || document.querySelector('head');
            let el: HTMLScriptElement = document.createElement('script');
            el.type = 'application/javascript';
            el.src = '/assets/iframeResizer.contentWindow.min.js';
            head.appendChild(el);

            el = document.createElement('script');
            el.type = 'application/javascript';
            el.src = '/assets/iframeResizer.js';
            head.appendChild(el);
        }
    }
}
