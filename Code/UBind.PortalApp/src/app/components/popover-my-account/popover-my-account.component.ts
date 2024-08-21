import { Component, OnInit } from '@angular/core';
import { PopoverController, NavParams } from '@ionic/angular';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { AppConfigService } from '@app/services/app-config.service';
import {
    PopoverEnvironmentSelectionComponent,
} from '@app/components/popover-environment-selection/popover-environment-selection.component';
import { Permission } from '@app/helpers';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { SharedPopoverService } from '@app/services/shared-popover.service';
import { PermissionService } from '@app/services/permission.service';
import { AuthenticationService } from '@app/services/authentication.service';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Renders the menu which appears when you click on your profile pic, to manage your account
 */
@Component({
    templateUrl: './popover-my-account.component.html',
    styleUrls: [
        './popover-my-account.component.scss',
    ],
})

export class PopoverMyAccountComponent implements OnInit {

    public showEnvironmentSelection: boolean;
    public permission: typeof Permission = Permission;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        protected navParams: NavParams,
        private appConfigService: AppConfigService,
        public popoverController: PopoverController,
        private navProxy: NavProxyService,
        private userPath: UserTypePathHelper,
        private sharedPopoverService: SharedPopoverService,
        private permissionService: PermissionService,
        private sharedLoaderService: SharedLoaderService,
        private authenticationService: AuthenticationService,
    ) {
    }

    public ngOnInit(): void {
        this.showEnvironmentSelection = this.permissionService.canAccessOtherEnvironments()
            && this.authenticationService.isAgent();
    }

    public async userDidTapMyAccount(ev: any): Promise<void> {
        this.navProxy.navigate([this.userPath.account]);
        this.popoverController.dismiss();
    }

    public async userDidTapSignout(ev: any): Promise<void> {
        await this.sharedLoaderService.present("Signing out...");
        await this.authenticationService.logout();
        this.navProxy.navigateRoot(["login"]);
        this.sharedLoaderService.dismiss();
        await this.popoverController.dismiss();
    }

    public async clickEnvironmentSelection(event: any): Promise<void> {
        const popoverDismissAction = (data: any): void => {
            if (data?.data) {
                this.appConfigService.changeEnvironment(data.data.environment);
                setTimeout(() => {
                    this.popoverController.dismiss();
                },0);
            }
        };

        await this.sharedPopoverService.show(
            {
                component: PopoverEnvironmentSelectionComponent,
                showBackdrop: false,
                cssClass: 'custom-popover top-popover-positioning',
                event: event,
            },
            'footer environment selection popover',
            popoverDismissAction,
        );
    }
}
