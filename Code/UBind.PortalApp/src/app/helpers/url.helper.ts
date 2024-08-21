import { RoutePathsHelper } from "./route-paths.helper";
import { StringHelper } from "./string.helper";

/**
 * Represents the components of a URL.
 */
export interface UrlComponents {
    protocol: string | null;
    domain: string | null;
    port: string | null;
    path: string;
    queryString: string | null;
}

/**
 * Tools for working with or manipulating URLs.
 */
export class UrlHelper {

    public static getQueryStringParameter(name: string, url: string = null): string {
        if (!url) url = window.location.href;
        name = name.replace(/[\[\]]/g, '\\$&');
        let regex: RegExp = new RegExp('[?&]' + name + '(=([^&#]*)|&|#|$)');
        let results: RegExpExecArray = regex.exec(url);
        if (!results) return null;
        if (!results[2]) return '';
        return decodeURIComponent(results[2].replace(/\+/g, ' '));
    }

    // validates url
    // passing urls are 
    // https://feature-ub-793.platform.ubind.io
    // *.platform.ubind.io
    // http://*.platform.ubind.io
    // http://*.ubind.io
    // https://*.ubind.io:*
    public static validateUrl(url: string): boolean {
    // https or http ? asterisk or any .example . com. au ? OR localhost : port ?                             
        let validUrlPattern: RegExp = /^(https:\/\/|http:\/\/)?(((\*|[\w\d]+(-[\w\d]+)*)\.)*([-\w\d]+)(\.\w{1,4})(\.\w{0,4})?|localhost)(\:(\d{1,5}|\*))?$/;
        let lengthCheck: RegExp = /^.{1,253}$/;

        if (validUrlPattern.test(url)
            && url.indexOf('.*.') < 0
            && url != "localhost"
            && lengthCheck.test(url)) {
            return true;
        } else {
            return false;
        }
    }

    public static getTenantAliasFromUrl(loc: Location = location): string {
        const pathSegments: Array<string> = loc.pathname.replace(/^\/|\/$/g, "").split('/');
        const tenantAlias: string = pathSegments[0] == 'portal'
            ? pathSegments[1] || null
            : pathSegments[0] || null;
        return tenantAlias?.toLowerCase() ?? null;
    }

    public static getPortalAliasFromUrl(loc: Location = location): string {
        let portalAlias: string = UrlHelper.getPortalAliasFromPath(loc);
        if (!portalAlias) {
            portalAlias = UrlHelper.getQueryStringParameter("portal");
        }
        return portalAlias;
    }

    private static getPortalAliasFromPath(loc: Location = location): string {
        const pathSegments: Array<string> = loc.pathname.replace(/^\/|\/$/g, "").split('/');
        const startIndex: number = pathSegments.indexOf('portal');
        if (startIndex == -1 || pathSegments.length < startIndex + 3) {
            return null;
        }
        if (pathSegments[startIndex + 3] == 'path' || pathSegments[startIndex + 2] == 'path') {
            return null;
        }

        // so that we can actually have a customer portal with the alias 'customer' which is a common
        // use case, we want to prioritise that over a path to the customer page.
        const endsWithPortalWithAliasCustomer: boolean
            = pathSegments.length == startIndex + 4 && pathSegments[startIndex + 3] == 'customer';
        const portalAliasIsCustomerWithPath: boolean
            = pathSegments.length > startIndex + 4
            && pathSegments[startIndex + 3] == 'customer' && pathSegments[startIndex + 4] == 'path';
        if (endsWithPortalWithAliasCustomer || portalAliasIsCustomerWithPath) {
            return 'customer';
        }

        const pageRouteIndex: number = UrlHelper.getIndexOfFirstSegmentMatchingPageRoute(pathSegments);
        if (pageRouteIndex == -1 && pathSegments.length == startIndex + 4) {
            return pathSegments[startIndex + 3];
        }
        if (pageRouteIndex <= startIndex + 3) {
            return null;
        }
        return pathSegments[startIndex + 3];
    }

    public static getOrganisationAliasFromUrl(loc: Location = location): string {
        const pathSegments: Array<string> = loc.pathname.replace(/^\/|\/$/g, "").split('/');
        const segmentOrganisationAlias: string = pathSegments[0] == 'portal'
            ? pathSegments[2] || null
            : pathSegments[1] || null;
        const paramOrganisationAlias: string = UrlHelper.getQueryStringParameter("organisation");
        if (segmentOrganisationAlias == 'path' && !paramOrganisationAlias) {
            // if we found 'path' at this position and there is no organisation alias in the query string
            // then we know they didn't specify an organisation alias
            return null;
        }
        const organisationAlias: string = paramOrganisationAlias || segmentOrganisationAlias;
        return RoutePathsHelper.isInRoutePaths(organisationAlias) ? null : organisationAlias;
    }

    /**
     * find the index of the first segment matching a page route, or the word path which is the start delimiter
     * of the page route. If no match is found then return -1
     * @param urlSegments 
     */
    public static getIndexOfFirstSegmentMatchingPageRoute(urlSegments: Array<string>): number {
        const maxDepth: number = Math.min(urlSegments.length, 6);
        const minDepth: number = urlSegments[0] == '' && urlSegments[1] == 'portal'
            ? 3
            : urlSegments[0] == 'portal' ? 2 : 1;
        for (let i: number = minDepth; i <= maxDepth; i++) {
            const urlSegment: string = urlSegments[i];
            if (urlSegment != '' && (urlSegment == 'path' || RoutePathsHelper.isInRoutePaths(urlSegment))) {
                return i;
            }
        }
        return -1;
    }

    /**
     * Retuns the base url of the portal, not including the path or query string parameters.
     * @param fullUrlPath the full url path (e.g. no protocol, domain or port.)
     */
    public static getPortalBaseUrl(fullUrlPath: string): string {
        let urlComponents: UrlComponents | null = UrlHelper.extractUrlComponents(fullUrlPath);
        if (!urlComponents) {
            return fullUrlPath;
        }
        let urlPathSegments: Array<string> = urlComponents.path.split('/');
        const pageRouteIndex: number = UrlHelper.getIndexOfFirstSegmentMatchingPageRoute(urlPathSegments);
        if (pageRouteIndex != -1) {
            urlPathSegments.splice(pageRouteIndex);
        }

        urlComponents.path = StringHelper.withoutTrailing(urlPathSegments.join('/'), '/');
        urlComponents.queryString = null;
        return UrlHelper.buildUrl(urlComponents);
    }

    public static extractUrlComponents(urlString: string): UrlComponents | null {
        // tslint:disable-next-line:max-line-length
        const urlRegex: RegExp = /^(?:(https?:)\/\/)?((?:[a-z0-9-]+\.)+[a-z0-9]+|[a-z0-9\-.:]+)?(?::(\d+))?(\/[^?#]*)(\?.*)?$/i;
        const pathMatch: RegExpMatchArray | null = urlString.match(urlRegex);
        if (pathMatch) {
            const protocol: string = pathMatch[1] || null;
            const domain: string = pathMatch[2] || null;
            const port: string = pathMatch[3] || null;
            const path: string = pathMatch[4] || '/';
            const queryString: string = pathMatch[5] || null;
            return { protocol, domain, port, path, queryString };
        } else {
            console.error('Invalid URL:', urlString);
            return null;
        }
    }

    public static buildUrl(urlComponents: UrlComponents): string {
        let url: string = '';
        if (urlComponents.protocol) {
            url += urlComponents.protocol;
        }
        if (urlComponents.domain) {
            url += `//${urlComponents.domain}`;
        }
        if (urlComponents.port) {
            url += `:${urlComponents.port}`;
        }
        if (urlComponents.path) {
            url += urlComponents.path.startsWith('/') ? urlComponents.path : '/' + urlComponents.path;
        }
        if (urlComponents.queryString) {
            url += urlComponents.queryString;
        }
        return url;
    }

    public static stripInjectorQueryParams(url: string): string {
        const injectorQueryParameters: Array<string> = ['frameId', 'portal', 'data-parent-url', 'referrer'];
        let urlComponents: UrlComponents | null = UrlHelper.extractUrlComponents(url);
        if (urlComponents.queryString) {
            const queryStringParameters: Array<string> = urlComponents.queryString.substr(1).split('&');
            const filteredQueryStringParameters: Array<string>
                = queryStringParameters.filter((queryStringParameter: string) => {
                    const key: string = queryStringParameter.split('=')[0];
                    return !injectorQueryParameters.includes(key);
                });
            urlComponents.queryString = filteredQueryStringParameters.length > 0
                ? '?' + filteredQueryStringParameters.join('&')
                : null;
        }
        return UrlHelper.buildUrl(urlComponents);
    }

    public static pathCombine(paths: Array<string>): string {
        let result: string = '';
        for (let i: number = 0; i < paths.length; i++) {
            if (i !== 0) {
                result += '/';
            }
            result += paths[i].replace(/(^\/)|(\/$)/g, '');
        }
        return result;
    }

    public static appendPathWithPathQueryParm(url: string): string {
        let urlComponents: UrlComponents | null = UrlHelper.extractUrlComponents(url);
        if (urlComponents.queryString) {
            const params: URLSearchParams = new URLSearchParams(urlComponents.queryString);
            let pathQueryParam: string = params.get('path');
            if (pathQueryParam) {
                pathQueryParam = decodeURIComponent(pathQueryParam);
                urlComponents.path = UrlHelper.pathCombine([urlComponents.path, pathQueryParam]);
                urlComponents.queryString = null;
            }
        }
        return UrlHelper.buildUrl(urlComponents);
    }

    public static extractPathUrl(url: string): string {
        url = UrlHelper.removeIframePropertiesFromQueryParameters(url);
        return url.split('/path')[1];
    }

    /**
     * This method is responsible for trimming out
     * any iframe related properties such as the frameId on initial page load.
     * @param url the current location or page in the app.
     * @returns a string to serve as a value for the 'path' query parameter. 
     */
    public static removeIframePropertiesFromQueryParameters(url: string): string {
        if (url.includes('?frameId')) {
            url = url.split('?frameId')[0];
        }

        if (url.includes('&frameId')) {
            url = url.split('&frameId')[0];
        }

        return url;
    }
}
