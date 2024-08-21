import { Component } from '@angular/core';
import { NavParams, PopoverController } from '@ionic/angular';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { ActionButtonPopover } from '@app/models/action-button-popover';

/**
 * Export popover portal page component class.
 * This class manage portal popover function.
 */
@Component({
    selector: 'app-popover-sso-configuration',
    templateUrl: './popover-sso-configuration.html',
})
export class PopoverSsoConfigurationPage {
    public isDisabled: boolean = false;
    public actions: Array<ActionButtonPopover> = [];

    public constructor(
        private navParams: NavParams,
        private sharedAlertService: SharedAlertService,
        public popOverCtrl: PopoverController) {
    }

    public ionViewWillEnter(): void {
        this.isDisabled = this.navParams.get('isDisabled');
        this.actions = this.navParams.get('actions') || [];
    }

    public onDisable(): void {
        this.sharedAlertService.showWithActionHandler({
            header: 'Disable SSO Configuration',
            message: 'By disabling this SSO Configuration, users relying upon it will no longer be able to log in.'
                + 'Are you sure you wish to proceed?',
            buttons: [
                {
                    text: 'No',
                    handler: (): any => {
                        this.popOverCtrl.dismiss(null);
                    },
                }, {
                    text: 'Yes',
                    handler: (): any => {
                        this.popOverCtrl.dismiss({ action: <ActionButtonPopover>{ actionName: 'disable' } });
                    },
                },
            ],
        });
    }

    public onEnablePortal(): void {
        this.popOverCtrl.dismiss({ action: <ActionButtonPopover>{ actionName: 'enable' } });
    }

    public onDelete(): void {
        this.sharedAlertService.showWithActionHandler({
            header: 'Delete SSO Configuration',
            message: 'By disabling this SSO Configuration, users relying upon it will no longer be able to log in.'
                + 'Are you sure you wish to proceed?',
            buttons: [
                {
                    text: 'No',
                    handler: (): any => {
                        this.popOverCtrl.dismiss(null);
                    },
                }, {
                    text: 'Yes',
                    handler: (): any => {
                        this.popOverCtrl.dismiss({ action: <ActionButtonPopover>{ actionName: 'delete' } });
                    },
                },
            ],
        });
    }

    public onEdit(): void {
        this.popOverCtrl.dismiss({ action: <ActionButtonPopover>{ actionName: 'edit' } });
    }

    public onShowSamlMetadata(): void {
        this.popOverCtrl.dismiss({ action: <ActionButtonPopover>{ actionName: 'showSamlMetadata' } });
    }
}
