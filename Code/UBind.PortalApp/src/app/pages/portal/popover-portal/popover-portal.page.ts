import { Component, OnInit } from '@angular/core';
import { NavParams, PopoverController } from '@ionic/angular';
import { Permission } from '@app/helpers/permissions.helper';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { ActionButtonPopover } from '@app/models/action-button-popover';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Export popover portal page component class.
 * This class manage portal popover function.
 */
@Component({
    selector: 'app-popover-portal',
    templateUrl: './popover-portal.page.html',
})
export class PopoverPortalPage implements OnInit {
    public isPortalDisabled: boolean = false;
    public segment: string;
    public permission: typeof Permission = Permission;
    public actions: Array<ActionButtonPopover> = [];
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        private navParams: NavParams,
        private sharedAlertService: SharedAlertService,
        public popOverCtrl: PopoverController,
    ) {
    }

    public ngOnInit(): void {
        this.isPortalDisabled = this.navParams.get('isPortalDisabled');
        this.segment = this.navParams.get('segment');
        this.actions = this.navParams.get('actions') || [];
    }

    public onDisablePortal(): void {
        this.sharedAlertService.showWithActionHandler({
            header: 'Disable Portal',
            message: 'By disabling this portal it will be temporarily inaccessible to all users. '
                + 'Are you sure you wish to proceed?',
            buttons: [
                {
                    text: 'No',
                    handler: (): any => {
                        this.popOverCtrl.dismiss(null);
                    },
                },
                {
                    text: 'Yes',
                    handler: (): any => {
                        this.popOverCtrl.dismiss({ action: <ActionButtonPopover>{ actionName: 'disablePortal' } });
                    },
                },
            ],
        });
    }

    public onEnablePortal(): void {
        this.popOverCtrl.dismiss({ action: <ActionButtonPopover>{ actionName: 'enablePortal' } });
    }

    public onDeletePortal(): void {
        this.sharedAlertService.showWithActionHandler({
            header: 'Delete Portal',
            message: 'By deleting this portal it will be permanently inaccessible to all users. '
                + 'Are you sure you wish to proceed?',
            buttons: [
                {
                    text: 'No',
                    handler: (): any => {
                        this.popOverCtrl.dismiss(null);
                    },
                },
                {
                    text: 'Yes',
                    handler: (): any => {
                        this.popOverCtrl.dismiss({ action: <ActionButtonPopover>{ actionName: 'deletePortal' } });
                    },
                },
            ],
        });
    }

    public onEdit(): void {
        this.popOverCtrl.dismiss({ action: <ActionButtonPopover>{ actionName: 'edit' } });
    }
}
