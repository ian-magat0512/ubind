import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, ActivatedRouteSnapshot, ActivationStart, NavigationExtras, Router } from '@angular/router';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { AppConfigService } from '@app/services/app-config.service';
import { AuthenticationService } from '@app/services/authentication.service';
import { scrollbarStyle } from '@assets/scrollbar';
import { Subject } from 'rxjs';
import { contentAnimation } from '@assets/animations';
import { Permission, StringHelper } from '@app/helpers';
import { OrganisationResourceModel } from '@app/resource-models/organisation/organisation.resource-model';
import { filter, map, takeUntil } from 'rxjs/operators';
import { CommonListUserPage } from '@app/pages/user/list-user-common/list-user-common.page';
import { UserApiService } from '@app/services/api/user-api.service';
import { UserService } from '@app/services/user.service';
import { AdditionalPropertyDefinitionService } from '@app/services/additional-property-definition.service';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { PermissionService } from '@app/services/permission.service';
import { IconLibrary } from '@app/models/icon-library.enum';
import { RouteHelper } from '@app/helpers/route.helper';

/**
 * List user page component class for organisation.
 */
@Component({
    selector: 'app-list-organisation-user',
    templateUrl: './list-organisation-user.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-segment.css',
    ],
    styles: [scrollbarStyle],
})
export class ListOrganisationUserPage extends CommonListUserPage implements OnInit {
    public iconLibrary: typeof IconLibrary = IconLibrary;
    private organisationId: string;
    private destroy$: any = new Subject();
    private contextTenantAlias: string;

    public constructor(
        public userApiService: UserApiService,
        private route: ActivatedRoute,
        private router: Router,
        protected navProxy: NavProxyService,
        protected authService: AuthenticationService,
        protected appConfigService: AppConfigService,
        protected stringHelper: StringHelper,
        protected userService: UserService,
        protected additionalPropertyDefinitionService: AdditionalPropertyDefinitionService,
        protected sharedAlertService: SharedAlertService,
        protected permissionService: PermissionService,
        protected routeHelper: RouteHelper,
    ) {
        super(
            appConfigService,
            navProxy,
            authService,
            stringHelper,
            additionalPropertyDefinitionService,
            userService,
            sharedAlertService,
            permissionService,
            routeHelper);
        this.loadUsersForListUserPage();
    }

    private loadUsersForListUserPage(): void {
        // Used to force loading when navigating from organisation users to tenant users
        this.router.events
            .pipe(
                filter((event: any) => event instanceof ActivationStart),
                map((event: any) => (<ActivationStart>event).snapshot))
            .subscribe(
                (snapshot: ActivatedRouteSnapshot) => {
                    if (snapshot.firstChild != null
                        && snapshot.firstChild.data.masterComponent?.name == this.constructor.name
                    ) {
                        this.title = 'Users';
                        this.listComponent.load();
                    }
                });
    }

    public ngOnInit(): void {
        this.organisationId = this.route.snapshot.paramMap.get('organisationId') || undefined;
        this.contextTenantAlias = this.routeHelper.getContextTenantAlias();
        this.setPageOrganisationTitle();
        const userOrganisationId: string = this.authService.userOrganisationId;
        if (userOrganisationId) {
            const sameOrganisation: boolean = this.organisationId == userOrganisationId;
            if (sameOrganisation) {
                this.canCreateUser = this.permissionService.hasManageUserPermission();
            } else {
                this.canCreateUser = this.permissionService.hasPermission(Permission.ManageOrganisations)
                    && this.permissionService.hasManageUserPermission();
            }
        }

        this.initialiseAdditionalActionButtons();
    }

    // Overriding the super class method
    public didSelectCreate(): void {
        this.listComponent.selectedId = null;
        if (this.organisationId) {
            let params: NavigationExtras = {
                queryParams: {
                    organisationId: this.organisationId,
                },
            };
            const urlSegments: Array<string> = this.routeHelper.getPathSegmentsUpUntil('user');
            urlSegments.push('create');
            this.navProxy.navigateForward(urlSegments, true, params);
        } else {
            throw new Error(`Parameter 'organisationId' cannot be found.`);
        }
    }

    private setPageOrganisationTitle(): void {
        this.userApiService
            .getOrganisationForUser(this.authService.userId, this.organisationId, this.contextTenantAlias)
            .pipe(takeUntil(this.destroy$))
            .subscribe((organisation: OrganisationResourceModel) => this.title = `${organisation.name} Users`);
    }
}
