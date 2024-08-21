import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { SubscriptionLike } from 'rxjs';
import { contentAnimation } from '@assets/animations';
import { AccountApiService } from '@app/services/api/account-api.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { scrollbarStyle } from '@assets/scrollbar';
import { Permission } from '@app/helpers/permissions.helper';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { DetailAccountPage } from '../detail-account/detail-account.page';
import { EventService } from '@app/services/event.service';
import { AuthenticationService } from '@app/services/authentication.service';
import { UserApiService } from '@app/services/api/user-api.service';
import { AdditionalPropertyDefinitionService } from '@app/services/additional-property-definition.service';
import { PermissionService } from '@app/services/permission.service';
import { SharedPopoverService } from '@app/services/shared-popover.service';
import { UserService } from '@app/services/user.service';

/**
 * Export detail customer account page component class
 * TODO: Write a better class header: details of the customer account page.
 */
@Component({
    selector: 'app-detail-customer-account',
    templateUrl: './detail-customer-account.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-detail.css',
    ],
    styles: [
        scrollbarStyle,
    ],
})
export class DetailCustomerAccountPage extends DetailAccountPage implements OnInit, OnDestroy {
    public title: string;
    public returnValue: any;
    public permission: typeof Permission = Permission;
    public subscription: SubscriptionLike;

    public constructor(
        route: ActivatedRoute,
        accountApiService: AccountApiService,
        userApiService: UserApiService,
        eventService: EventService,
        navProxy: NavProxyService,
        layoutManager: LayoutManagerService,
        userPath: UserTypePathHelper,
        authenticationService: AuthenticationService,
        protected additionalPropertyDefinitionService: AdditionalPropertyDefinitionService,
        permissionService: PermissionService,
        protected sharedPopoverService: SharedPopoverService,
        protected userService: UserService,
    ) {
        super(
            navProxy,
            route,
            accountApiService,
            userApiService,
            eventService,
            layoutManager,
            userPath,
            authenticationService,
            additionalPropertyDefinitionService,
            permissionService,
            sharedPopoverService,
            userService,
        );
    }

    public ngOnInit(): void {
        super.ngOnInit();
    }

    public ngOnDestroy(): void {
        super.ngOnDestroy();
    }
}
