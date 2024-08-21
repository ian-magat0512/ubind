import { Component, ChangeDetectorRef, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { AppConfigService } from '@app/services/app-config.service';
import { UserApiService } from '@app/services/api/user-api.service';
import { AuthenticationService } from '@app/services/authentication.service';
import { LoadDataService } from '@app/services/load-data.service';
import { scrollbarStyle } from '@assets/scrollbar';
import { SubscriptionLike } from 'rxjs';
import { contentAnimation } from '@assets/animations';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { CommonListUserPage } from '../list-user-common/list-user-common.page';
import { StringHelper } from '@app/helpers';
import { AdditionalPropertyDefinitionService } from '@app/services/additional-property-definition.service';
import { UserService } from '@app/services/user.service';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { PermissionService } from '@app/services/permission.service';
import { IconLibrary } from '@app/models/icon-library.enum';
import { RouteHelper } from '@app/helpers/route.helper';
import { UserType } from '@app/models/user-type.enum';

/**
 * Export list user page component class.
 * This class manage of displaying of user in the list.
 */
@Component({
    selector: 'app-list-user',
    templateUrl: './list-user.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-segment.css',
    ],
    styles: [scrollbarStyle],
})
export class ListUserPage extends CommonListUserPage implements OnInit {
    public navigationTimeout: any;
    public userIdChanged$: SubscriptionLike;
    public profilePictureBaseUrl: string;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        public layoutManager: LayoutManagerService,
        public navProxy: NavProxyService,
        public userApiService: UserApiService,
        protected appConfigService: AppConfigService,
        protected changeDetectorRef: ChangeDetectorRef,
        protected route: ActivatedRoute,
        protected authService: AuthenticationService,
        protected loadDataService: LoadDataService,
        protected stringHelper: StringHelper,
        protected additionalPropertyDefinitionService: AdditionalPropertyDefinitionService,
        protected userService: UserService,
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
    }

    // Overriding the super class method
    public getDefaultHttpParams(): Map<string, string | Array<string>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (this.authService.isMasterUser()) {
            params.set('userTypes', [UserType.Master]);
        } else if (this.authService.isAgent()) {
            params.set('userTypes', [UserType.Client]);
        }

        params.set('organisation', this.authService.userOrganisationId);
        return params;
    }
}
