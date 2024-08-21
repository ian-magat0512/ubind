import { Injectable } from "@angular/core";
import {
    ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot, UrlTree,
} from "@angular/router";
import { AppConfigService } from "@app/services/app-config.service";

/**
 * This route guard is used to populate null organisation alias placeholders in the URL.
 */
@Injectable({
    providedIn: 'root',
})
export class PopulateNullOrganisationAliasGuard implements CanActivate {
    public constructor(
        private router: Router,
        private appConfigService: AppConfigService,
    ) {
    }
    public async canActivate(
        routeSnapshot: ActivatedRouteSnapshot,
        state: RouterStateSnapshot,
    ): Promise<boolean | UrlTree> {
        const organisationAliasFromRoute: string = routeSnapshot.params?.portalOrganisationAlias ?? null;

        // A 'null' organisation alias is used as a placeholder in the URL 
        // when an embed portal has a defined portal alias but no organisation alias is given.
        // E.g. An embed portal may be setup like this: 
        //      <div class="ubind-portal" id="ubind-portal"
        //           data-tenant="carl" data-portal="customer" data-environment="staging"></div>
        //      Then, the iframe URL will look like: /portal/carl/null/customer
        //      This guard will redirect to the correct URL: /portal/carl/{portalOrganisationAlias}/customer
        if (organisationAliasFromRoute === 'null') {
            const portalOrganisationAlias: string = this.appConfigService.currentConfig.portal.organisationAlias;
            const newPath: string = state.url.replace(
                routeSnapshot.params.portalOrganisationAlias,
                portalOrganisationAlias);
            return this.router.parseUrl(newPath);
        }
        return true;
    }
}
