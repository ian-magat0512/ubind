import { Component, OnInit, Renderer2 } from '@angular/core';
import { Permission } from "@app/helpers";
import { UserApiService } from '@app/services/api/user-api.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { AuthenticationService } from '@app/services/authentication.service';
import { FeatureSettingService } from '@app/services/feature-setting.service';
import { UserType } from '@app/models/user-type.enum';
import { scrollbarStyle } from '@assets/scrollbar';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { MenuItem } from '@app/models/menu-item';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { PermissionService } from '@app/services/permission.service';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Export tenant home page component class
 * TODO: Write a better class header: displaying of tenant home page.
 */
@Component({
    selector: 'app-master-home',
    templateUrl: './master-home.page.html',
    styleUrls: [
        './master-home.page.scss',
        '../../../../assets/css/scrollbar-div.css',
    ],
    styles: [scrollbarStyle],
})
export class MasterHomePage implements OnInit {

    public title: string = 'uBind Master Portal';
    private tenantAdminMenus: Array<MenuItem> = new Array<MenuItem>();
    public menuButtons: Array<MenuItem> = [];
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        public navProxy: NavProxyService,
        private renderer: Renderer2,
        private authenticationService: AuthenticationService,
        private featureSettingService: FeatureSettingService,
        public userApiService: UserApiService,
        public layoutManager: LayoutManagerService,
        protected userPath: UserTypePathHelper,
        protected permissionService: PermissionService,
    ) { }

    private setAvailableButtons(): void {
        const userRole: string = this.authenticationService.userType;
        this.tenantAdminMenus = [
            {
                identifier: "Tenants",
                title: "Tenants",
                icon: "cloud-circle",
                iconLibrary: IconLibrary.IonicV4,
                navigate: { commands: ['tenant', 'list'] },
                permissions: [Permission.ViewTenants],
            },
            {
                identifier: "Messages",
                title: "Messages",
                icon: "chatboxes",
                iconLibrary: IconLibrary.IonicV4,
                navigate: { commands: [this.userPath.message, 'list'] },
                permissions: [Permission.ViewMessages, Permission.ViewAllMessages],
            },
            {
                identifier: "Users",
                title: "Users",
                icon: "contact",
                iconLibrary: IconLibrary.IonicV4,
                navigate: { commands: ['user', 'list'] },
                permissions: [Permission.ViewUsers],
            },
            {
                identifier: "Roles",
                title: "Roles",
                icon: "shirt",
                iconLibrary: IconLibrary.IonicV4,
                navigate: { commands: ['role', 'list'] },
                permissions: [Permission.ViewRoles],
            },
            {
                identifier: "Accounts",
                title: "My Account",
                icon: "settings",
                iconLibrary: IconLibrary.IonicV4,
                navigate: { commands: ['account'] },
                permissions: [Permission.ViewMyAccount],
            },
        ].filter((p: MenuItem) => this.permissionService.hasOneOfPermissions(p.permissions));

        if (userRole == UserType.Client) {
            this.menuButtons = this.featureSettingService.removeMenuItemsForDisabledFeatures(this.tenantAdminMenus);
        } else if (userRole == UserType.Master) {
            this.menuButtons = this.tenantAdminMenus;
        }
    }

    public ngOnInit(): void {
        this.renderer.setAttribute(document.body, "data-head-title", "uBind");
        this.setAvailableButtons();
    }

    public menuButtonClicked(menuButton: MenuItem): void {
        this.navProxy.navigate(menuButton.navigate.commands, menuButton.navigate.extras);
    }
}
