import { Component, Input, OnDestroy, OnInit } from "@angular/core";
import { Permission } from "@app/helpers/permissions.helper";
import { UserTypePathHelper } from "@app/helpers/user-type-path.helper";
import { DeploymentEnvironment } from "@app/models/deployment-environment.enum";
import { EnvironmentChange } from "@app/models/environment-change";
import { Errors } from "@app/models/errors";
import { IconLibrary } from "@app/models/icon-library.enum";
import { MenuItem } from "@app/models/menu-item";
import { AppConfigService } from "@app/services/app-config.service";
import { AuthenticationService } from "@app/services/authentication.service";
import { EventService, UserId } from "@app/services/event.service";
import { LayoutManagerService } from "@app/services/layout-manager.service";
import { MenuItemService } from "@app/services/menu-item.service";
import { NavProxyService } from "@app/services/nav-proxy.service";
import { PermissionService } from "@app/services/permission.service";
import { SharedLoaderService } from "@app/services/shared-loader.service";
import { UserService } from "@app/services/user.service";
import { scrollbarStyle } from "@assets/scrollbar";
import { Subject } from "rxjs";
import { takeUntil } from "rxjs/operators";

/**
 * The renders the main menu
 */
@Component({
    selector: 'app-main-menu',
    templateUrl: './main-menu.component.html',
    styleUrls: ['./main-menu.component.scss'],
    styles: [
        scrollbarStyle,
    ],
})
export class MainMenuComponent implements OnInit, OnDestroy {
    @Input() public slide: boolean = false;

    public appPages: Array<MenuItem>;
    public isAccordionExpanded: boolean = false;
    public availableEnvironments: Array<string> = new Array<string>();
    public environment: DeploymentEnvironment;
    public canChangeEnvironment: boolean = false;
    public permission: typeof Permission = Permission;
    public canViewMyAccount: boolean = false;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    protected destroyed: Subject<void>;

    public constructor(
        public layoutManager: LayoutManagerService,
        public authenticationService: AuthenticationService,
        private userPath: UserTypePathHelper,
        public userService: UserService,
        private appConfigService: AppConfigService,
        private eventService: EventService,
        public navProxy: NavProxyService,
        private sharedLoaderService: SharedLoaderService,
        private permissionService: PermissionService,
        private menuItemService: MenuItemService,
    ) { }

    private initializeMenuItems(): void {
        this.appPages = new Array<MenuItem>();
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.eventService.environmentChangedSubject$.pipe(takeUntil(this.destroyed))
            .subscribe((ec: EnvironmentChange) => {
                this.environment = ec.newEnvironment;
            });

        this.initializeMenuItems();
        if (this.authenticationService.isAuthenticated()) {
            this.manageMenuItems();
        }

        // when we login, the permissions change so we need re-draw
        this.eventService.userLoginSubject$.pipe(takeUntil(this.destroyed)).subscribe((userId: UserId) => {
            this.initializeMenuItems();
            if (this.authenticationService.isAuthenticated()) {
                this.manageMenuItems();
            }
        });
        if (this.authenticationService.isAuthenticated()) {
            this.manageMenuItems();
        }

        this.eventService.featureSettingChangedSubject$.pipe(takeUntil(this.destroyed)).subscribe(() => {
            this.initializeMenuItems();
            if (this.authenticationService.isAuthenticated()) {
                this.manageMenuItems();
            }
        });
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    public userDidTapMyAccount(): void {
        this.navProxy.navigate([this.userPath.account]);
    }

    private manageMenuItems(): void {
        if (!this.authenticationService.isAuthenticated()) {
            return;
        }
        this.appPages = this.menuItemService.getMenuItems();
        this.environment = this.appConfigService.getEnvironment();
        this.canChangeEnvironment = this.permissionService.canAccessOtherEnvironments()
            && this.authenticationService.isAgent();
        this.availableEnvironments = this.permissionService.getAvailableEnvironments();
    }

    public userDidTapEnvironment(): void {
        this.isAccordionExpanded = !this.isAccordionExpanded;
    }

    public environmentChange(environment: DeploymentEnvironment): void {
        this.isAccordionExpanded = false;
        this.appConfigService.changeEnvironment(environment);
    }

    public userDidTapMenuItems(menuItem: MenuItem): void {
        if (menuItem.permissions
            && (this.userService
                && !this.permissionService.hasOneOfPermissions(menuItem.permissions))) {
            throw Errors.User.AccessDenied(menuItem.title);
        } else {
            this.navProxy.navigate(menuItem.navigate.commands, menuItem.navigate.extras);
        }
    }

    public async userDidTapSignout(): Promise<void> {
        await this.sharedLoaderService.present("Signing out...");
        await this.authenticationService.logout();
        this.navProxy.navigateRoot(["login"]);
        this.sharedLoaderService.dismiss();
    }
}
