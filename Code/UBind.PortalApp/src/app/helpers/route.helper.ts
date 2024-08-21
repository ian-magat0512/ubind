import {
    ActivatedRouteSnapshot, Router,
    ActivationStart, NavigationStart, NavigationEnd,
    NavigationExtras, UrlSegment, RouterEvent, NavigationCancel, UrlTree, Params,
} from "@angular/router";
import { Injectable } from "@angular/core";
import { filter, map } from "rxjs/operators";
import { EntityType } from "../models/entity-type.enum";
import { StringHelper } from "./string.helper";
import { EventService } from "@app/services/event.service";
import { HttpParams } from "@angular/common/http";
import { Errors } from "@app/models/errors";

/**
 * Export route helper class
 * This class helps for routing.
 */
@Injectable({
    providedIn: 'root',
})
export class RouteHelper {

    private activatedRouteSnapshot: ActivatedRouteSnapshot;
    private previousActivatedRouteSnapshot: ActivatedRouteSnapshot;
    private currentNavigationId: number;
    private currentUrl: string;
    private previousUrl: string;
    public navigationInProgress: boolean = false;
    public navigationExtras: NavigationExtras;

    public constructor(
        private router: Router,
        private eventService: EventService,
    ) {
        this.updateNavigationId();
        this.updateActivatedRouteSnapshot();
        this.updateNavigationExtras();
        this.onRouteChangeNotifyAppConfigService();
        this.onNavigationUpdateNavigationInProgress();
        this.restorePreviousActivatedRouteSnapshot();
        this.onNavigationEndRemoveJwtTokenFromUrl();
    }

    private onNavigationUpdateNavigationInProgress(): void {
        this.router.events
            .pipe(
                filter((event: RouterEvent) => event instanceof NavigationStart || event instanceof NavigationEnd),
            )
            .subscribe((event: RouterEvent) => {
                if (event instanceof NavigationStart) {
                    this.navigationInProgress = true;
                } else if (event instanceof NavigationEnd) {
                    this.navigationInProgress = false;
                }
            });
    }

    private updateNavigationId(): void {
        this.router.events.pipe(
            filter((event: any) => event instanceof NavigationStart),
        )
            .subscribe((event: NavigationStart) => {
                this.currentNavigationId = event.id;
                this.previousActivatedRouteSnapshot = this.activatedRouteSnapshot;
            });
    }

    private updateActivatedRouteSnapshot(): void {
        this.router.events.pipe(
            filter((event: any) => event instanceof ActivationStart),
            map((event: any) => (<ActivationStart>event).snapshot),
        )
            .subscribe((snapshot: ActivatedRouteSnapshot) => {
                this.activatedRouteSnapshot = snapshot;
            });
    }

    private restorePreviousActivatedRouteSnapshot(): void {
        this.router.events
            .pipe(filter((event: any) => event instanceof NavigationCancel))
            .subscribe((navigationCancel: NavigationCancel) => {
                if (navigationCancel.id == this.currentNavigationId) {
                    this.activatedRouteSnapshot = this.previousActivatedRouteSnapshot;
                }
            });
    }

    private onNavigationEndRemoveJwtTokenFromUrl(): void {
        this.router.events
            .pipe(filter((event: any) => event instanceof NavigationEnd))
            .subscribe((navigationEnd: NavigationEnd) => {
                if (navigationEnd) {
                    const currentUrl: string = window.location.href;
                    if (currentUrl.indexOf('#token=') !== -1) {
                        const baseUrl: string = currentUrl.split('#token=')[0];
                        window.history.replaceState({}, '', baseUrl);
                    }
                }
            });
    }

    private updateNavigationExtras(): void {
        this.router.events.pipe(filter((event: any) => event instanceof NavigationStart))
            .subscribe((event: NavigationStart) => {
                this.navigationExtras = this.router.getCurrentNavigation().extras;
            });
    }

    private onRouteChangeNotifyAppConfigService(): void {
        this.router.events.pipe(
            filter((event: any) => event instanceof NavigationEnd),
            map((event: any) => (<NavigationEnd>event).url),
        ).subscribe((url: string) => {
            if (this.currentUrl !== url) {
                this.previousUrl = this.currentUrl;
                this.currentUrl = url;
            }
            this.eventService.routeChanged(url);
        });
    }

    public get ActivatedRouteSnapShot(): ActivatedRouteSnapshot {
        return this.activatedRouteSnapshot;
    }

    public get PreviousActivatedRouteSnapShot(): ActivatedRouteSnapshot {
        return this.previousActivatedRouteSnapshot;
    }

    public get PreviousUrl(): string {
        return this.previousUrl;
    }

    public isUrlEditPage(): boolean {
        const url: Array<UrlSegment> = this.activatedRouteSnapshot.url;
        return url.findIndex((u: UrlSegment) => u.path == "edit") > -1;
    }

    public isUrlCreatePage(): boolean {
        const url: Array<UrlSegment> = this.activatedRouteSnapshot.url;
        return url.findIndex((u: UrlSegment) => u.path == "create") > -1;
    }

    public getPathSegments(): Array<string> {
        let pathSegments: Array<string> = new Array<string>();
        this.activatedRouteSnapshot.pathFromRoot.forEach((snapshot: ActivatedRouteSnapshot) =>
            snapshot.url.forEach((urlSegment: UrlSegment) => pathSegments.push(urlSegment.path)));
        return pathSegments;
    }

    public pathContainsSegment(segment: string): boolean {
        return this.getPathSegments().indexOf(segment) > -1;
    }

    public getPathSegmentsUpUntil(targetSegment: string): Array<string> {
        let pathSegments: Array<string> = this.getPathSegments();
        let found: boolean = false;
        for (let i: number = pathSegments.length - 1; i >= 0; i--) {
            if (pathSegments[i] == targetSegment) {
                found = true;
                break;
            }
            pathSegments.pop();
        }
        if (!found) {
            throw Errors.General.Unexpected("An attempt was made to get the path segments up until '"
                + targetSegment + "' but it was not found in the path. This is an internal error. "
                + "Please get in touch with customer support.");
        }
        return pathSegments;
    }

    public getPathSegmentsAfter(targetSegment: string): Array<string> {
        let pathSegments: Array<string> = new Array<string>();
        this.activatedRouteSnapshot.pathFromRoot.forEach((snapshot: ActivatedRouteSnapshot) =>
            snapshot.url.forEach((urlSegment: UrlSegment) => pathSegments.push(urlSegment.path)));
        let result: Array<string> = new Array<string>();
        let matched: boolean = false;
        for (let pathSegment of pathSegments) {
            if (pathSegment == targetSegment) {
                matched = true;
                continue;
            }
            if (!matched) {
                continue;
            }
            result.push(pathSegment);
        }
        return result;
    }

    /**
     * Returns the path segments to the module root for the give activated route.
     * E.g. if the path is /demo/quote/list and this is the quote module, then it will return /demo/quote
     * This is typically used to build other routes relative to the module's route.
     * 
     * @param routeSnapshot
     */
    public getModulePathSegments(): Array<string> {
        return this.getModulePathSegmentsForSnapshot(this.activatedRouteSnapshot);
    }

    public getModulePathSegmentsForSnapshot(routeSnapshot: ActivatedRouteSnapshot): Array<string> {
        let childMostRrouteSnapshot: ActivatedRouteSnapshot = this.getChildmostRouteSnapshotForSnapshot(routeSnapshot);
        let pathSegments: Array<string> = new Array<string>();
        childMostRrouteSnapshot.pathFromRoot.forEach((snapshot: ActivatedRouteSnapshot) =>
            snapshot.url.forEach((urlSegment: UrlSegment) => pathSegments.push(urlSegment.path)));
        let routeConfigSegments: Array<string> = childMostRrouteSnapshot.routeConfig.path.split('/');

        // pop the number of times there is a segment within the route config
        routeConfigSegments.forEach(() => pathSegments.pop());
        return pathSegments;
    }

    /**
     * Returns the path parameter by name for the given route snapshot.
     * This is necessary because the parent/ancestor may not have all of the route path paramters, 
     * so we need to get the childmost route and then pull the params from there.
     * @param routeSnapshot
     * @param paramName
     */
    public getPathParam(paramName: string): string {
        let finalChild: ActivatedRouteSnapshot = this.getChildmostRouteSnapshot();
        return finalChild.paramMap.get(paramName);
    }

    /**
     * Returns the query parameter by name for the given route snapshot.
     * This is necessary because the parent/ancestor may not have all of the route query paramters, 
     * so we need to get the childmost route and then pull the params from there.
     * @param routeSnapshot
     * @param paramName
     */
    public getQueryParam(paramName: string): string {
        return this.getQueryParamFromLocation(paramName);
    }

    /**
     * This function gets the query param not from the ActivatedRouteSnapshot, 
     * but instead from window.location.
     * This is important because there is a bug in Angular/Ionic 
     * whereby the query params in the route do not get picked up
     */
    public getQueryParamFromLocation(paramName: string): string {
        let questionMarkPosition: number = window.location.href.indexOf('?');
        if (questionMarkPosition == -1) {
            return null;
        }
        let paramPairs: Array<string> = window.location.href.substring(questionMarkPosition + 1).split('&');
        let paramMap: Map<string, string> = new Map<string, string>();
        paramPairs.forEach((p: string) => {
            let splitPair: Array<string> = p.split('=');
            paramMap.set(splitPair[0], decodeURIComponent(splitPair[1]));
        });
        return paramMap.get(paramName);
    }

    /**
     * Returns a path or query parameter matching the paramName.
     * First path params will be checked, and then query params.
     * @param route
     * @param paramName
     */
    public getParam(paramName: string): string {
        let value: string = this.getPathParam(paramName);
        if (value == null) {
            value = this.getQueryParam(paramName);
        }
        return value;
    }

    /**
     * Returns the url segment with the appended parts. This is normally for additional property definition.
     * The supplied parameters will be added on top of the existing segments.
     * @param parentSegment
     * @param entityType
     */
    public getPathSegmentsAndAppend(parentSegment: string, entityType: EntityType): Array<string> {
        let retValue: Array<string> = this.getPathSegments();
        retValue.push(parentSegment);
        let kebabCase: string = StringHelper.toKebabCase(entityType);
        retValue.push(kebabCase);
        return retValue;
    }

    public appendPathSegment(newSegment: string): Array<string> {
        let pathSegments: Array<string> = this.getPathSegments();
        pathSegments.push(newSegment);
        return pathSegments;
    }

    public appendPathSegments(newSegments: Array<string>): Array<string> {
        let pathSegments: Array<string> = this.getPathSegments();
        pathSegments.push(...newSegments);
        return pathSegments;
    }

    private getChildmostRouteSnapshot(): ActivatedRouteSnapshot {
        return this.getChildmostRouteSnapshotForSnapshot(this.activatedRouteSnapshot);
    }

    private getChildmostRouteSnapshotForSnapshot(routeSnapshot: ActivatedRouteSnapshot): ActivatedRouteSnapshot {
        if (routeSnapshot == null) {
            throw new Error(
                "Could not access ActivatedRoute because no route has been activated yet. "
                + "If you are calling RouteHelper in app.component.ts "
                + "pleae check you are not calling it too early.");
        }
        let finalChild: ActivatedRouteSnapshot = routeSnapshot;
        while (finalChild.firstChild != null) {
            finalChild = finalChild.firstChild;
        }
        return finalChild;
    }

    /**
     * Retuns a copy of the urlTree with the query param path transposed in place of the 'path' segment.
     */
    public replacePathSegmentWithPathQueryParam(urlTree: UrlTree): UrlTree {
        const pathQueryParam: string = urlTree.queryParamMap.get('path');
        if (!pathQueryParam) {
            return urlTree;
        }

        const path: string = decodeURIComponent(pathQueryParam);
        const pathUrlTree: UrlTree = this.router.parseUrl(path);
        let urlSegments: Array<UrlSegment> = urlTree.root.children.primary.segments;
        const pathSegmentIndex: number
            = urlSegments.findIndex((urlSegment: UrlSegment) => urlSegment.path === 'path');
        if (pathSegmentIndex === -1) {
            throw new Error('Could not find path segment in url tree. Before calling this function, plase ensure '
                + ' you add the path segment to the url tree.');
        }

        // delete everything after the path segment
        const unwantedSegmentsStartIndex: number = pathSegmentIndex + 1;
        if (urlSegments.length > pathSegmentIndex + 1) {
            urlSegments.splice(unwantedSegmentsStartIndex, urlSegments.length - unwantedSegmentsStartIndex);
        }

        // append the new path segments
        if (pathUrlTree.root.children.primary) {
            urlSegments.push(...pathUrlTree.root.children.primary.segments);
        }

        // add the query params
        for (const key in pathUrlTree.queryParams) {
            if (Object.prototype.hasOwnProperty.call(pathUrlTree.queryParams, key)) {
                urlTree.queryParams[key] = pathUrlTree.queryParams[key];
            }
        }

        // remove the path query param since we've transposed it
        delete urlTree.queryParams['path'];
        return urlTree;
    }

    public addPathAsQueryParamToPortalBaseUrl(portalBaseUrl: string, fullPath: string): string {
        let urlTree: UrlTree = this.router.parseUrl(fullPath);
        let queryString: string = this.convertQueryParamsToString(urlTree.queryParams);
        let urlSegments: Array<UrlSegment> = urlTree.root.children.primary.segments;
        const pathSegmentIndex: number
            = urlSegments.findIndex((urlSegment: UrlSegment) => urlSegment.path === 'path');
        let path: string = '';
        if (pathSegmentIndex != -1) {
            urlSegments = urlSegments.slice(pathSegmentIndex + 1);
            path = urlSegments.map((urlSegment: UrlSegment) => urlSegment.path).join('/');
        }

        if (queryString) {
            path += '?' + queryString;
        }

        let params: Map<string, string> = this.gatherQueryParamsFromIncompletePath(path);

        // the params will be appended to the current URL query strings.
        params.forEach((value: string, key: string) => {
            portalBaseUrl = this.appendNewQueryStringParameterToUrl(portalBaseUrl, key, value);
        });
        return portalBaseUrl;
    }

    /* 
    * gathers the parameter from the path input
    * ex: /path?param1=value1&param2=value2 will be transformed to
    * "path=>/path" and "param1 => value1" and "param2 => value2"
    * @return then transforms it into a map<string,string>
    */
    public gatherQueryParamsFromIncompletePath(path: string): Map<string, string> {
        let newParams: Map<string, string> = new Map();
        let separatePathAndOtherParams: Array<string> = path.split('?');
        let itemIndex: number = 0;
        separatePathAndOtherParams.forEach((item: string) => {
            if (item) {
                // possibly a path.
                if (itemIndex == 0) {
                    newParams.set("path", decodeURIComponent(item));
                } else {
                    const parsedUrl: URL = new URL(origin + "?" + item);
                    parsedUrl.searchParams.forEach((value: string, key: string) => {
                        newParams.set(key, value);
                    });
                }
            }
            itemIndex++;
        });
        return newParams;
    }

    // appends new parameter to URL
    // returns the whole path with the new parameter
    private appendNewQueryStringParameterToUrl(path: string, paramName: string, paramValue: string): string {
        const parsedUrl: URL = new URL(path);
        let decode: string = decodeURIComponent(paramValue);
        parsedUrl.searchParams.set(paramName, decode);
        return parsedUrl.toString();
    }

    private convertQueryParamsToString(queryParams: Params): string {
        const params: HttpParams = new HttpParams({ fromObject: queryParams });
        return params.toString();
    }

    public getContextTenantAlias(): string {
        const pathTenantAlias: string = this.getParam('tenantAlias');
        const portalTenantAlias: string = this.getParam('portalTenantAlias');
        return pathTenantAlias || portalTenantAlias;
    }

    public hasPathSegment(segment: string): boolean {
        let pathSegments: Array<string> = this.getPathSegments();
        for (let i: number = pathSegments.length - 1; i >= 0; i--) {
            if (pathSegments[i] == segment) {
                return true;
            }
        }
        return false;
    }
}
