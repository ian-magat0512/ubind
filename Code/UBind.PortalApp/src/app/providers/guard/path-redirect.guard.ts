import { Injectable } from "@angular/core";
import {
    ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot, UrlSegment, UrlTree,
} from "@angular/router";
import { RoutePathsHelper } from "@app/helpers/route-paths.helper";
import { RouteHelper } from "@app/helpers/route.helper";
import { UrlHelper } from "@app/helpers/url.helper";

/**
 * This route guard is used to redirect the user to the correct path when they are using a portal or organisation.
 * There is a specific structure to URLs in uBind portals, typically:
 * /portal/{tenantAlias}/{organisationAlias}/{portalAlias}/path/quote/list
 * OR
 * /portal/{tenantAlias}/path/quote/list
 * OR
 * /portal/{tenantAlias}/{organisationAlias}/path/quote/list
 * 
 * So in summary, it starts with tenant, optionally organisation, then portal, then 'path', then the rest of the path.
 * 
 * We introduced the path segment 'path' to delimit between the organisation and portal, and the rest of the path.
 * This was necessary so that we could detect by location the tenant, organisation and portal, without having to
 * restrict organisation and portal names to ones which didn't match any of the top level routes.
 */
@Injectable({
    providedIn: 'root',
})
export class PathRedirectGuard implements CanActivate {

    private pathSegment: UrlSegment = new UrlSegment('path', {});

    public constructor(
        private router: Router,
        private routeHelper: RouteHelper,
    ) { }

    public canActivate(
        routeSnapshot: ActivatedRouteSnapshot,
        state: RouterStateSnapshot,
    ): boolean | UrlTree {
        let shouldRedirect: boolean = false;
        let urlTree: UrlTree = this.router.parseUrl(state.url);
        let urlSegments: Array<UrlSegment> = urlTree.root.children.primary.segments;
        const pathSegmentIndex: number
            = urlSegments.findIndex((urlSegment: UrlSegment) => urlSegment.path === 'path');
        if (pathSegmentIndex == -1) {
            // There is no segment with the path 'path', so let's add it in the right place
            // so that we can actually have a customer portal with the alias 'customer' which is a common
            // use case, we want to prioritise that over a path to the customer page.
            const urlStringSegments: Array<string> = urlSegments.map((urlSegment: UrlSegment) => urlSegment.path);
            const hasPortalAliasForCustomerPortal: boolean
            = urlStringSegments.length >= 3 && urlStringSegments[2] == 'customer';
            if (!hasPortalAliasForCustomerPortal) {
                urlSegments = this.getUrlSegmentsWhenPathDoesNotExist(urlSegments, urlStringSegments);
                shouldRedirect = true;
            } else {
                if (urlSegments.length == 3) {
                    urlSegments.push(this.pathSegment);
                } else {
                    urlSegments = this.getUrlSegmentsWhenCustomerPathExistsWithSucceedingSegments(urlSegments);
                    shouldRedirect = true;
                }
            }
            urlTree.root.children.primary.segments = urlSegments;
        }

        // if path was passed as a query param, replace the path with the query param path
        const pathQueryParam: string = urlTree.queryParamMap.get('path');
        if (pathQueryParam) {
            urlTree = this.routeHelper.replacePathSegmentWithPathQueryParam(urlTree);
            shouldRedirect = true;
        }

        if (shouldRedirect) {
            return urlTree;
        }

        return true;
    }

    private getUrlSegmentsWhenPathDoesNotExist(
        urlSegments: Array<UrlSegment>,
        urlStringSegments: Array<string>): Array<UrlSegment> {
        const pageRouteIndex: number = UrlHelper.getIndexOfFirstSegmentMatchingPageRoute(urlStringSegments);
        if (pageRouteIndex > 0) {
            urlSegments.splice(pageRouteIndex, 0, this.pathSegment);
        } else {
            urlSegments.push(this.pathSegment);
        }
        return urlSegments;
    }

    private getUrlSegmentsWhenCustomerPathExistsWithSucceedingSegments(
        urlSegments: Array<UrlSegment>): Array<UrlSegment> {
        let nextSegment: UrlSegment = urlSegments[3];
        let isNextSegmentIsInRoutePaths: boolean = RoutePathsHelper.isInRoutePaths(nextSegment.path);
        if (isNextSegmentIsInRoutePaths) {
            urlSegments.splice(3, 0, this.pathSegment);
        } else {
            urlSegments.splice(2, 0, this.pathSegment);
        }
        return urlSegments;
    }
}
