import { Component, OnInit } from '@angular/core';
import { NavParams, PopoverController } from '@ionic/angular';
import { DeploymentEnvironment } from '@app/models/deployment-environment.enum';
import { Permission } from '@app/helpers/permissions.helper';
import { ActionButtonPopover } from '@app/models/action-button-popover';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Export popover Release page component class.
 * This class manage release popover page functions.
 */
@Component({
    selector: 'app-popover-release',
    templateUrl: './popover-release.page.html',
})
export class PopoverReleasePage implements OnInit {
    public deployments: Array<string>;
    public forProduction: boolean;
    public forStaging: boolean;
    public permission: typeof Permission = Permission;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        private navParams: NavParams,
        private popCtrl: PopoverController,
    ) { }

    public ngOnInit(): void {
        this.deployments = this.navParams.get('deployments') || [];
        this.forProduction = this.deployments.includes(DeploymentEnvironment.Production);
        this.forStaging = this.deployments.includes(DeploymentEnvironment.Staging);
    }

    public restoreToDev(): void {
        this.popCtrl.dismiss({
            action: <ActionButtonPopover>{ actionName: 'restore' },
            environment: DeploymentEnvironment.Development,
        });
    }

    public setAsDefault(environment: DeploymentEnvironment): void {
        this.popCtrl.dismiss({
            action: <ActionButtonPopover>{ actionName: 'set' },
            environment: environment,
        });
    }

    public unset(environment: DeploymentEnvironment): void {
        this.popCtrl.dismiss({
            action: <ActionButtonPopover>{ actionName: 'unset' },
            environment: environment,
        });
    }

    public onEditRelease(): void {
        this.popCtrl.dismiss({ action: <ActionButtonPopover>{ actionName: 'edit' } });
    }

    public move(environment: string): void {
        this.popCtrl.dismiss({
            action: <ActionButtonPopover>{ actionName: 'move' },
            environment: environment,
        });
    }
}
