import { Injectable } from "@angular/core";

/** This service handles external redirects. 
 * Any call to a URL that is not part of the defined SPA routes should be
 * defined on this service moving forward.
 */
@Injectable({ providedIn: 'root' })
export class ExternalRedirectService {
    private readonly hangfireDashboardUrl: string = '/hangfire';

    /** Contains the external links that are permitted
     * for redirection. Add more items to allow navigation on other 
     * external links in the future. */
    private whitelist: Array<string> = ['/hangfire'];

    /** Since the hangfire dashboard is not part of the SPA, we need to treat 
     *  it like it's a totally different page outside of the SPA.
     */
    public goToHangfireDashboard(): void {
        this.goToExternalUrl(this.hangfireDashboardUrl);
    }

    public goToExternalUrl(externalUrl: string): void {
        if (externalUrl === null || externalUrl === '') {
            return;
        }

        const foundUrl: Array<string> = this.whitelist.filter((element: string) => {
            return externalUrl.startsWith(element);
        });
        if ((foundUrl ?? '') === '') {
            return;
        }
        window.location.href = externalUrl;
    }

    public decodeBase64Url(base64Url: string): string {
        if (base64Url == null) {
            return "";
        }

        if (base64Url.length == 0) {
            return base64Url;
        }

        try {
            return window.atob(base64Url);
        } catch (error) {
            return base64Url;
        }

    }
}
