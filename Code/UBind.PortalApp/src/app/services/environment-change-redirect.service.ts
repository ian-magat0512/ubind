import { NavProxyService } from "./nav-proxy.service";
import { Injectable } from "@angular/core";
import { NavigationExtras } from "@angular/router";
import { EventService } from "./event.service";
import { RouteHelper } from "../helpers/route.helper";
import { DeploymentEnvironment } from "../models/deployment-environment.enum";
import { EnvironmentChange } from "@app/models/environment-change";
import { StringHelper } from "@app/helpers";
import { TypedRouteData } from "@app/routing/typed-route-data";

/**
 * Reads the route data and where "environmentChangeRedirectTo" is set, 
 * when the environment changes, it triggers the redirect.
 */
@Injectable({ providedIn: 'root' })
export class EnvironmentChangeRedirectService {

    public constructor(
        private navProxy: NavProxyService,
        private eventService: EventService,
        private routeHelper: RouteHelper,
        private stringHelper: StringHelper,
    ) {
        this.onEnvironmentChangeRedirect();
    }

    private onEnvironmentChangeRedirect(): void {
        this.eventService.environmentChangedSubject$.subscribe((environmentChange: EnvironmentChange) => {
            if (environmentChange && environmentChange.newEnvironment != environmentChange.oldEnvironment) {
                let pathSegments: Array<string>;
                const queryParams: any = { ...this.routeHelper.ActivatedRouteSnapShot.queryParams };

                if (this.stringHelper.equalsIgnoreCase(
                    environmentChange.newEnvironment,
                    DeploymentEnvironment.Production,
                )) {
                    if (queryParams && queryParams.environment) {
                        delete queryParams['environment'];
                    }
                } else {
                    queryParams.environment = environmentChange.newEnvironment;
                }

                const routeData: TypedRouteData = this.routeHelper.ActivatedRouteSnapShot.data;
                if (routeData.onEnvironmentChangeRedirectTo) {
                    pathSegments = this.routeHelper.getModulePathSegments();
                    const newSegments: Array<string> = routeData.onEnvironmentChangeRedirectTo.split('/');
                    pathSegments = pathSegments.concat(newSegments);
                } else {
                    pathSegments = this.routeHelper.getPathSegments();
                }

                let extras: NavigationExtras = { queryParams: queryParams };
                this.navProxy.navigateRoot(pathSegments, extras, true);
            }
        });
    }
}
