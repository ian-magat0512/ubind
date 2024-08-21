import { Injectable } from "@angular/core";
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Params, UrlTree } from "@angular/router";
import { ElevatedPermission, Permission } from "@app/helpers/permissions.helper";
import { Errors } from "@app/models/errors";
import { TypedRouteData } from "@app/routing/typed-route-data";
import { AuthenticationService } from "@app/services/authentication.service";
import { ErrorHandlerService } from "@app/services/error-handler.service";
import { NavProxyService } from "@app/services/nav-proxy.service";
import { PermissionService } from "@app/services/permission.service";
import { FeatureSetting } from "@app/models/feature-setting.enum";

/**
 * Checks that the user has the required permissions to access that page.
 */
@Injectable({ providedIn: "root" })
export class PermissionGuard implements CanActivate {

    public constructor(
        private permissionService: PermissionService,
        private navProxy: NavProxyService,
        private errorHandlerService: ErrorHandlerService,
        private authService: AuthenticationService,
    ) {
    }

    public canActivate(
        routeSnapshot: ActivatedRouteSnapshot,
        state: RouterStateSnapshot,
    ): boolean | UrlTree {
        if (!this.authService.isAuthenticated()) {
            // we won't bother checking permissions if the user is not authenticated.
            // The AuthenticationGuard will redirect the user to the login page.
            return false;
        }

        let routeData: TypedRouteData = routeSnapshot.data;
        let queryParams: Params = routeSnapshot.queryParams;
        let redirect: any = null;
        let quoteId: string = null;
        let environment: string = null;
        let policyRenewal: string = 'policy-renewal';
        let tenantAlias: string = routeSnapshot.params['tenant'];

        if (Object.prototype.hasOwnProperty.call(queryParams, "redirect")) {
            redirect = queryParams.redirect;
            if (redirect == policyRenewal) {
                if (Object.prototype.hasOwnProperty.call(queryParams, "environment")) {
                    environment = queryParams.environment;
                }
                if (Object.prototype.hasOwnProperty.call(queryParams, "id")) {
                    quoteId = queryParams.id;
                }
            }
        }

        if (routeData.mustHaveOneOfPermissions) {
            let success: boolean = this.checkIfHasOneOfPermissions(
                routeData.mustHaveOneOfPermissions,
                redirect,
                routeSnapshot.url.join('/'),
                tenantAlias,
                quoteId,
                environment);
            if (!success) {
                return false;
            }
        }

        if (routeData.mustHavePermissions) {
            let success: boolean = this.checkIfHasAllPermissions(
                routeData.mustHavePermissions,
                redirect,
                routeSnapshot.url.join('/'),
                tenantAlias,
                quoteId,
                environment);
            if (!success) {
                return false;
            }
        }

        if (routeData.mustHaveOneOfEachSetOfPermissions) {
            let success: boolean = this.checkIfHasOneOfEachSetOfPermissions(
                routeData.mustHaveOneOfEachSetOfPermissions,
                redirect,
                routeSnapshot.url.join('/'),
                tenantAlias,
                quoteId,
                environment);
            if (!success) {
                return false;
            }
        }

        return true;
    }

    private checkIfHasOneOfPermissions(
        permissions: Array<Permission>,
        redirect: any,
        url: string,
        tenantAlias: string,
        quoteId: string,
        environment: string,
    ): boolean {
        let success: boolean = this.permissionService.hasOneOfPermissions(permissions);
        if (!success) {
            if (redirect != null) {
                this.redirectToLoginPage(tenantAlias, redirect, quoteId, environment);
            } else {
                this.errorHandlerService.handleError(Errors.User.Forbidden(
                    null,
                    `to access the location "${url}" you need one of `
                    + `the following permissions: ${permissions}`));
            }
        }

        return success;
    }

    private checkIfHasAllPermissions(
        mustHaveAllPermissions: Array<Permission>,
        redirect: any,
        url: string,
        tenantAlias: string,
        quoteId: string,
        environment: string,
    ): boolean {
        let hasAllPermissions: boolean = true;
        let missingPermissions: Array<Permission> = new Array<Permission>();
        let missingFeatureSettings: Array<FeatureSetting> = new Array<FeatureSetting>();
        mustHaveAllPermissions.forEach((permission: Permission) => {
            if (!this.permissionService.hasPermission(permission)) {
                const relatedPermission: ElevatedPermission
                    = this.permissionService.retrieveRelatedPermissions(permission);
                if (relatedPermission != null) {
                    const feature: FeatureSetting = relatedPermission.requiresFeature;
                    if (!missingFeatureSettings.includes(feature)) {
                        missingFeatureSettings.push(feature);
                    }
                }
                missingPermissions.push(permission);
                hasAllPermissions = false;
            }
        });

        if (missingPermissions.length > 0) {
            if (redirect != null) {
                this.redirectToLoginPage(tenantAlias, redirect, quoteId, environment);
            } else {
                let permissionList: string = missingPermissions.join(', ');
                missingFeatureSettings = missingFeatureSettings.filter((x: FeatureSetting) => x != null);
                let featureList: string = missingFeatureSettings
                    .join(', ');
                this.errorHandlerService.handleError(Errors.User.Forbidden(
                    null,
                    `to access the location "${url}" you also need `
                    + `the following permissions: ${permissionList}`
                    + (missingFeatureSettings.length > 0
                        ? ` and the following features enabled: ${featureList}`
                        : "")));
            }
        }

        return hasAllPermissions;
    }

    private checkIfHasOneOfEachSetOfPermissions(
        permissionSets: Array<Array<Permission>>,
        redirect: any,
        url: string,
        tenantAlias: string,
        quoteId: string,
        environment: string,
    ): boolean {
        let success: boolean = this.permissionService.hasOneOfEachSetOfPermissions(permissionSets);
        if (!success) {
            if (redirect != null) {
                this.redirectToLoginPage(tenantAlias, redirect, quoteId, environment);
            } else {
                let permissionList: string = permissionSets.map((permissionSet: Array<Permission>) => {
                    return permissionSet.join(', ');
                }).join(' and ');
                this.errorHandlerService.handleError(Errors.User.Forbidden(
                    null,
                    `to access the location "${url}" you need to have one permission from each set of `
                    + `permissions: ${permissionList}`));
            }
        }

        return success;
    }

    private redirectToLoginPage(tenantAlias: string, redirect: any, quoteId: string, environment: string): void {
        this.navProxy.navigateRoot(
            ['login'],
            {
                queryParams:
                {
                    redirect: redirect,
                    quoteId: quoteId,
                    environment: environment,
                },
            });
    }
}
