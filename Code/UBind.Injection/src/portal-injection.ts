import { InjectionContext } from './injection-context';
import { Injection } from './injection';
import { PortalEmbedOptions } from './models/portal-embed-options';
import { UrlHelper } from './helpers/url.helper';

/**
 * Represents the embdding of a portal into a webpage.
 */
export class PortalInjection extends Injection {
    public constructor(
        containingElement: HTMLElement,
        private embedOptions: PortalEmbedOptions,
        parentUrl: string,
        injectionContext: InjectionContext) {
        super(
            containingElement,
            containingElement,
            embedOptions.tenant,
            embedOptions.portal,
            embedOptions.organisation,
            embedOptions.environment,
            parentUrl,
            injectionContext);
        this.init();
    }

    protected buildIframeUrl(): string {
        // When using the https://embed.ubind.io/embed-portal.html? 
        // the path parameter is retrieved from the url in this.embedOptions.path as the location path,
        // but if coming from a embedded web page, we use this.embedOptions.path as the location path.
        const locationPath: string = this.injectionContext
            .findGetParameter("path", this.embedOptions.path)
            || this.embedOptions.path;
        let locationPathNormalized: string = this.normalizePath(locationPath);

        const organisationPart: string = this.isAliasSpecified(this.organisation) ? `${this.organisation}` :
            this.isAliasSpecified(this.portal) ? `null` : '';
        const portalPart: string = this.isAliasSpecified(this.portal) ? `${this.portal}` : '';

        // TODO: review this code and remove the passing of a referrer and parent url as this can be doctored by
        // people and is not secure.
        // URL Guide:
        //     w/ organisation & portal alias URL formatting: /portal/{tenant-alias}/{organisation-alias}/{portal-alias}
        //     w/o organisation or portal alias URL formatting: /portal/{tenant-alias}/
        let tenantOrganisationPortalAliases: Array<string> = [this.tenant, organisationPart, portalPart]
            .filter((alias: string) => alias.trim() !== '');
        let iframeUrl: string = `${this.injectionContext.uBindAppHost}/portal/`
            + `${tenantOrganisationPortalAliases.join("/")}/path`
            + `${this.embedOptions.path ? locationPathNormalized : ''}`;

        // Add query parameters to the iframe URL
        iframeUrl = UrlHelper.appendNewQueryStringParameterToUrl(iframeUrl, "frameId", this.getIframeId());
        iframeUrl = UrlHelper.appendNewQueryStringParameterToUrl(iframeUrl, "portal", this.portal);
        iframeUrl =
            UrlHelper.appendNewQueryStringParameterToUrl(
                iframeUrl,
                "data-parent-url",
                encodeURI(this.parentUrl));
        iframeUrl = UrlHelper.appendNewQueryStringParameterToUrl(iframeUrl, "referrer", location.host);
        if (this.environment) {
            iframeUrl = UrlHelper.appendNewQueryStringParameterToUrl(iframeUrl, "environment", this.environment);
        }
        if (this.organisation) {
            iframeUrl = UrlHelper.appendNewQueryStringParameterToUrl(iframeUrl, "organisation", this.organisation);
        }
        return iframeUrl;
    }

    public getIframeId(): string {
        const organisationPart: string = this.isAliasSpecified(this.organisation) ? `---${this.organisation}` : '';
        const portalPart: string = this.isAliasSpecified(this.portal) ? `---${this.portal}` : '';
        return `ubind-portal-iframe---${this.tenant}---${this.environment}${organisationPart}${portalPart}`;
    }

    public getIframeTitle(): string {
        return `${this.tenant} Portal`;
    }

    public handleAppLoadEvent(data: any): void {
        const payload: any = data.payload;
        if (payload.status == 'success') {
            this.loaded = true;

            if (this.embedOptions.fullScreen) {
                this.makePortalFullScreen();
            }

            this.revealIframe();
        }
    }

    public updateUrlPathQueryParameter(payload: any): void {
        if (!payload.path) {
            return;
        }
        const { pathname, search }: Location = window.location;
        let newPath: string = "";

        // gather params from payload path.
        let newParams: Map<string, string> = UrlHelper.gatherQueryParamsFromIncompletePath(payload.path);
        let customParams: Map<string, string> = UrlHelper.gatherQueryParamsFromIncompletePath(search);
        customParams = UrlHelper.getQueryParamsNotUsedByThePortal(customParams);

        // append custom params to the new params
        customParams.forEach((value: string, key: string) => {
            newParams.set(key, value);
        });

        // the params will be appended to the current URL query strings.
        newParams.forEach((value: string, key: string) => {
            if (value && value != '&') {
                newPath = UrlHelper.getNewQueryPath(newPath, key, value);
            }
        });
        history.replaceState(null, "", pathname.concat(newPath));
    }

    private makePortalFullScreen(): void {
        let cssBlock: string = `
            * {
                box-sizing: border-box;
            }

            html, body {
                margin: 0 !important;
                padding: 0 !important;
                height: 100%;
                overflow: hidden;
            }
        
            #${this.rootElement.id} {
                position: fixed;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                z-index: 99999;
            }
        `;
        let styleElement: HTMLStyleElement = document.createElement('style');
        styleElement.innerHTML = cssBlock;
        document.head.appendChild(styleElement);
        let iframeContainer: HTMLDivElement = this.iframeContainer;
        iframeContainer.style.height = '100%';
    }

    private isAliasSpecified(alias: string): boolean {
        return (!!alias);
    }

    private normalizePath(path: string): string {
        let pathDecoded: string = decodeURIComponent(path);
        let normalizedPath: string = this.normalizePartialUrl(pathDecoded);
        return (normalizedPath.startsWith('/') ? '' : '/') + normalizedPath;
    }

    private normalizePartialUrl(partialUrl: string): string {
        const hasQueryStart: boolean = partialUrl.includes('?');
        const parts: Array<string> = partialUrl.split('&');
        if (parts.length > 1) {
            return parts[0] + (hasQueryStart ? '&' : '?') + parts.slice(1).join('&');
        } else {
            return partialUrl;
        }
    }
}
