import { Injectable } from '@angular/core';
import {
    Router, NavigationExtras, UrlTree,
} from '@angular/router';
import { NavController } from '@ionic/angular';
import { AuthenticationService } from '@app/services/authentication.service';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { AppConfigService } from './app-config.service';
import { AppConfig } from '@app/models/app-config';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { DeploymentEnvironment } from '@app/models/deployment-environment.enum';
import { StringHelper } from '@app/helpers/string.helper';
import { EntityType } from '@app/models/entity-type.enum';
import { UrlHelper } from '@app/helpers';
import { PermissionService } from './permission.service';
import { NavigationOptions } from '@ionic/angular/providers/nav-controller';

/**
 * Export NavProxy service class.
 * This class manage navigation of proxy of the tenants.
 */
@Injectable({ providedIn: 'root' })
export class NavProxyService {

    private portalTenantAlias: string;

    public constructor(
        private router: Router,
        errorHandlerService: ErrorHandlerService,
        private navCtrl: NavController,
        private routeHelper: RouteHelper,
        private appConfigService: AppConfigService,
        private userPath: UserTypePathHelper,
        private authenticationService: AuthenticationService,
        private permissionService: PermissionService,
        private stringHelper: StringHelper,
    ) {
        errorHandlerService.navigateSubject.subscribe((commands: Array<any>) => this.navigate(commands));
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.portalTenantAlias = appConfig.portal.tenantAlias;
        });
    }

    public injectCommandsWithTenantOrgPortalAndPath(commands: Array<any>, overrideTenantAlias?: string): Array<any> {
        let pathSegmentIndex: number = commands.findIndex((command: any) => command === 'path');
        if (pathSegmentIndex == -1) {
            commands.splice(0, 0, 'path');
            pathSegmentIndex = 0;

            let portalAliasFromUrl: string = UrlHelper.getPortalAliasFromUrl();
            if (portalAliasFromUrl) {
                commands.splice(0, 0, portalAliasFromUrl);
            }

            let organisationAliasFromUrl: string = UrlHelper.getOrganisationAliasFromUrl();
            if (organisationAliasFromUrl) {
                commands.splice(0, 0, organisationAliasFromUrl);
            }

            commands.splice(0, 0, this.portalTenantAlias);
        }
        if (overrideTenantAlias) {
            commands[0] = this.portalTenantAlias;
        }
        return commands;
    }

    public async navigate(commands: Array<any>, extras?: NavigationExtras): Promise<boolean> {
        let cleanCommand: any = [];
        commands.forEach((com: any) => {
            if (com.indexOf('?') > -1) {
                cleanCommand.push(com.split('?')[0]);
            } else {
                cleanCommand.push(com);
            }
        });
        commands = this.injectCommandsWithTenantOrgPortalAndPath(cleanCommand);
        extras = this.addEnvironmentToQueryParam(extras);
        return this.navCtrl.navigateRoot(commands, extras);
    }

    public async navigateRoot(
        commands: Array<any>,
        extras?: NavigationExtras,
        isRedirect?: boolean,
        overrideTenantAlias?: string,
    ): Promise<boolean> {
        commands = this.injectCommandsWithTenantOrgPortalAndPath(commands, overrideTenantAlias);
        if (!isRedirect) {
            extras = this.addEnvironmentToQueryParam(extras);
        }
        return this.navCtrl.navigateRoot(commands, extras);
    }

    public async navigateForward(
        commands: Array<any>,
        animated: boolean = true,
        extras?: NavigationExtras): Promise<boolean> {
        commands = this.injectCommandsWithTenantOrgPortalAndPath(commands);
        extras = this.addEnvironmentToQueryParam(extras);

        let options: NavigationOptions = extras;
        options.animated = animated;
        options.animationDirection = 'forward';

        return this.navCtrl.navigateRoot(commands, options);
    }

    public async navigateBack(
        commands: Array<any>,
        animated: boolean = true,
        extras?: NavigationExtras,
    ): Promise<boolean> {
        commands = this.injectCommandsWithTenantOrgPortalAndPath(commands);
        extras = this.addEnvironmentToQueryParam(extras);
        return this.navCtrl.navigateBack(commands, extras);
    }

    public async navigateBackOne(animated: boolean = true, extras?: NavigationExtras): Promise<void> {
        this.navigateBackN(1, animated, extras);
    }

    public async navigateBackTwo(animated: boolean = true, extras?: NavigationExtras): Promise<void> {
        this.navigateBackN(2, animated, extras);
    }

    public async navigateBackN(
        numberOfSegmentsToPop: number,
        animated: boolean = true,
        extras?: NavigationExtras,
    ): Promise<boolean> {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        for (let i: number = 0; i < numberOfSegmentsToPop; i++) {
            pathSegments.pop();
        }
        extras = this.addEnvironmentToQueryParam(extras);
        return this.navCtrl.navigateBack(pathSegments, extras);
    }

    public createUrlTree(commands: Array<any>, extras?: NavigationExtras): UrlTree {
        commands = this.injectCommandsWithTenantOrgPortalAndPath(commands);
        extras = this.addEnvironmentToQueryParam(extras);
        return this.router.createUrlTree(commands, extras);
    }

    public redirectToHome(extras?: NavigationExtras): void {
        this.navigate([this.userPath.home], extras);
    }

    public addEnvironmentToQueryParam(extras?: NavigationExtras): NavigationExtras {
        let environment: string = this.appConfigService.getEnvironment();
        if (!extras) {
            extras = { queryParams: { environment: environment } };
        }

        if (this.stringHelper.equalsIgnoreCase(environment, DeploymentEnvironment.Production)) {
            if (extras && extras.queryParams && extras.queryParams.environment) {
                extras = JSON.parse(JSON.stringify(extras));
                delete extras.queryParams['environment'];
            }

            return extras;
        }

        extras = JSON.parse(JSON.stringify(extras));
        if (!extras.queryParams) {
            extras.queryParams = { environment: environment };
        } else {
            extras.queryParams.environment = environment;
        }

        return extras;
    }

    public gotoTenant(tenantAlias: string, segment?: string): void {
        if (segment) {
            this.navigate(["tenant", tenantAlias], { queryParams: { segment: segment } });
        } else {
            this.navigate(["tenant", tenantAlias]);
        }
    }

    public goToCustomer(customerId: string): void {
        if (this.authenticationService.isAgent() &&
            this.permissionService.hasViewCustomerPermission() &&
            customerId) {
            this.navigate([this.userPath.customer, customerId]);
        } else {
            this.navigate([this.userPath.account]);
        }
    }

    public async goToCustomerList(): Promise<void> {
        await this.navigate(["customer", "list"]);
    }

    public goToUser(userId: string): void {
        if (this.canNavigateToUser() && userId) {
            this.navigate([this.userPath.user, userId]);
        } else {
            this.navigate([this.userPath.account]);
        }
    }

    public goToEntityMessageList(emailId: string, type: string): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.push('message');
        if (type) {
            pathSegments.push(type);
        }
        pathSegments.push(emailId);
        this.navigateForward(pathSegments);
    }

    public goToPolicy(policyId: string): void {
        if (this.permissionService.hasViewPolicyPermission() && policyId) {
            let url: Array<string> = [this.userPath.policy, policyId];
            this.navigate(url, null);
        }
    }

    public goToOrganisation(organisationId: string): void {
        if (!organisationId || !this.permissionService.hasViewOrganisationPermission()) {
            console.warn('Cannot navigate to organisation without an id or without permission');
            return;
        }
        if (this.routeHelper.pathContainsSegment('tenant')) {
            // we are in the master portal under tenants, let's do a relative navigation
            const pathSegments: Array<string> = this.routeHelper.getPathSegmentsUpUntil('tenant');
            pathSegments.push(this.routeHelper.getContextTenantAlias(), 'organisation', organisationId);
            this.navigate(pathSegments);
        } else {
            let url: Array<string> = [this.userPath.organisation, organisationId];
            this.navigate(url, null);
        }
    }

    public goToPolicyTransaction(param: { policyId: string; policyTransactionId: string }): void {
        if (this.permissionService.hasViewPolicyPermission() && param.policyId) {
            let url: Array<string> = [
                this.userPath.policy, param.policyId,
                this.userPath.policyTransaction,
                param.policyTransactionId,
            ];
            this.navigate(url, null);
        }
    }

    public goToQuote(quoteId: string): void {
        if (this.permissionService.hasViewQuotePermission() && quoteId) {
            this.navigate([this.userPath.quote, quoteId]);
        }
    }

    public goToClaim(claimId: string): void {
        if (this.permissionService.hasViewClaimPermission() && claimId) {
            this.navigate([this.userPath.claim, claimId]);
        }
    }

    public goToOwner(ownerUserId: string, ownerOrganisationId?: string): void {
        if (this.canNavigateToUser() && ownerUserId) {
            let commands: Array<any> = ['user', ownerUserId];

            // If an ownerOrganisationId is provided, we have to check if it is in the same organisation with
            // the performing user. If it is, then we should not add the organisation to the route parameters
            // because it would throw an error if the performing user does not have a ViewAllUsersForAllOrganisations
            // permission
            if (ownerOrganisationId && ownerOrganisationId != this.authenticationService.userOrganisationId) {
                commands = ['organisation', ownerOrganisationId, ...commands];
            }

            this.navigate(commands, null);
        } else {
            this.navigate([this.userPath.account]);
        }
    }

    public async goToAdditionalPropertyValues(entityType: EntityType): Promise<void> {
        const parentSegment: string = 'additional-property-values';
        const segments: Array<string> = this.routeHelper.getPathSegmentsAndAppend(parentSegment, entityType);
        await this.navigate(segments);
    }

    public goToPortal(portalId: string, organisationId: string = null): void {
        if (!portalId || !this.permissionService.hasViewPortal()) {
            // we can't navigate to a portal without an id or without permission
            console.warn('Cannot navigate to portal without an id or without permission');
            return;
        }
        if (organisationId == null && this.routeHelper.pathContainsSegment('organisation')) {
            organisationId = this.routeHelper.getParam('organisationId');
        }
        if (this.routeHelper.pathContainsSegment('tenant') && organisationId != null) {
            // we are in the master portal under tenants, let's do a relative navigation
            const pathSegments: Array<string> = this.routeHelper.getPathSegmentsUpUntil('organisation');
            pathSegments.push(organisationId, 'portal', portalId);
            this.navigate(pathSegments);
        } else if (organisationId) {
            this.navigate(['organisation', organisationId, 'portal', portalId]);
        } else {
            this.navigate(['portal', portalId]);
        }
    }

    /**
     * * Updates or clears the URL segment query parameter
     */
    public updateSegmentQueryStringParameter(segment: string, segmenValue: string | null): void {
        let navigationExtras: NavigationExtras;

        if (segment) {
            navigationExtras = {
                queryParams: { segment: segmenValue },
                queryParamsHandling: 'merge', // preserve existing query params
            };
        } else {
            navigationExtras = {
                queryParams: { segment: null },
                queryParamsHandling: 'merge', // preserve existing query params
            };
        }

        this.router.navigate([], navigationExtras);
    }

    public goToProductRelease(productAlias: string, productReleaseId: string): void {
        if (this.permissionService.hasManageProductPermission() && productReleaseId) {
            this.navigate(['product', productAlias, 'release', productReleaseId]);
        }
    }

    private canNavigateToUser(): boolean {
        return this.authenticationService.isAgent() && this.permissionService.hasViewUserPermission();
    }
}
