import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router, UrlTree } from '@angular/router';
import { StringHelper } from '@app/helpers/string.helper';
import { DeploymentEnvironment } from '@app/models/deployment-environment.enum';
import { PermissionService } from '@app/services/permission.service';
import { AppConfigService } from '@app/services/app-config.service';

/**
 * Ensures that a user who does not have the ability to access multiple environents (e.g. a customer)
 * can only access the environment they are registered under, and forces a redirect if they try to.
 */
@Injectable({ providedIn: 'root' })
export class EnvironmentGuard implements CanActivate {

    public constructor(
        private router: Router,
        private permissionService: PermissionService,
        private appConfigService: AppConfigService,
    ) {
    }

    public canActivate(
        route: ActivatedRouteSnapshot,
        state: RouterStateSnapshot,
    ): boolean | UrlTree {
        let urlTree: UrlTree = this.router.parseUrl(state.url);
        let urlEnvironment: string = urlTree.queryParamMap.get('environment');
        let environment: DeploymentEnvironment = urlEnvironment
            ? <DeploymentEnvironment>StringHelper.camelCase(urlEnvironment)
            : DeploymentEnvironment.Production;
        if (!this.permissionService.canAccessEnvironment(environment)) {
            const allowedDataEnvironments: Array<DeploymentEnvironment>
                = this.permissionService.getAvailableEnvironments();
            environment = allowedDataEnvironments[0];
            if (environment) {
                if (environment == DeploymentEnvironment.Production && urlTree.queryParamMap.has('environment')) {
                    delete urlTree.queryParams['environment'];
                } else {
                    urlTree.queryParams['environment'] = environment;
                }
                this.appConfigService.changeEnvironment(environment, true);
                return urlTree;
            }
        } else if (urlEnvironment && urlEnvironment.toLowerCase() == DeploymentEnvironment.Production.toLowerCase()) {
            // remove environment from the url since you don't need it if it's production:
            delete urlTree.queryParams['environment'];
            return urlTree;
        }
        return true;
    }
}
