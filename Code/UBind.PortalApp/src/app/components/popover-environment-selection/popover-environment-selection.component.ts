import { Component, OnInit } from '@angular/core';
import { DeploymentEnvironment } from '@app/models/deployment-environment.enum';
import { AppConfigService } from '@app/services/app-config.service';
import { PermissionService } from '@app/services/permission.service';
import { NavParams, PopoverController } from '@ionic/angular';

/**
 * A component which renders the environments that you can switch to in a drop down menu
 */
@Component({
    selector: 'app-popover-environment',
    templateUrl: './popover-environment-selection.component.html',
    styleUrls: ['./popover-environment-selection.component.scss'],
})
export class PopoverEnvironmentSelectionComponent implements OnInit {
    public deployment: string;
    public forProduction: boolean;
    public forStaging: boolean;
    public currentEnvironment: DeploymentEnvironment;
    public availableEnvironments: Array<DeploymentEnvironment> = new Array<DeploymentEnvironment>();

    public constructor(
        private navParams: NavParams,
        private popCtrl: PopoverController,
        private permissionService: PermissionService,
        private appConfigService: AppConfigService,
    ) { }

    public ngOnInit(): void {
        this.availableEnvironments = this.permissionService.getAvailableEnvironments();
        this.currentEnvironment = this.appConfigService.getEnvironment();
    }

    public select(value: any): void {
        this.popCtrl.dismiss({ 'environment': value });
    }

    public isSelected(value: any): boolean {
        return this.currentEnvironment?.toLowerCase() == value.toLowerCase();
    }
}
