import { Component, OnInit } from '@angular/core';
import { NavParams, PopoverController } from '@ionic/angular';
import { Permission } from '@app/helpers/permissions.helper';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { ActionButtonPopover } from '@app/models/action-button-popover';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Export popover organisation page component class
 * TODO: Write a better class header: popover of the organisation.
 */
@Component({
    selector: 'app-popover-organisation',
    templateUrl: './popover-organisation.page.html',
})
export class PopoverOrganisationPage implements OnInit {
    public organisationIsDisabled: boolean = false;
    public organisationIsDefault: boolean = false;
    public canAddUser: boolean = false;
    public canAddPortal: boolean = false;
    public permission: typeof Permission = Permission;
    public actions: Array<ActionButtonPopover> = [];
    public segment: string;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        private navParams: NavParams,
        public popOverCtrl: PopoverController,
        private sharedAlertService: SharedAlertService,
    ) {
    }

    public ngOnInit(): void {
        this.organisationIsDisabled = this.navParams.get('isDisabled');
        this.organisationIsDefault = this.navParams.get('isDefault');
        this.canAddUser = this.navParams.get('canAddUser');
        this.canAddPortal = this.navParams.get('canAddPortal');
        this.actions = this.navParams.get('actions') || [];
        this.segment = this.navParams.get('segment');
    }

    public onDisableOrganisation(): void {
        if (this.organisationIsDefault) {
            this.sharedAlertService.showWithOk(
                'Cannot disable default organisation',
                'Before disabling this organisation you must set another organisation as the default organisation.',
            );
            return;
        }

        this.sharedAlertService.showWithActionHandler({
            header: 'Disable organisation?',
            message: 'If you disable this organisation, users that belong to this '
                + 'organisation will no longer be able to access the portal.',
            buttons: [
                {
                    text: 'Cancel',
                    handler: (): any => {
                        this.popOverCtrl.dismiss(null);
                    },
                },
                {
                    text: 'Disable',
                    handler: (): any => {
                        this.popOverCtrl.dismiss({
                            action: <ActionButtonPopover>{ actionName: 'disableOrganisation' },
                        });
                    },
                },
            ],
        });
    }

    public onEnableOrganisation(): void {
        this.popOverCtrl.dismiss({ action: <ActionButtonPopover>{ actionName: 'enableOrganisation' } });
    }

    public onOrganisationSetDefault(): void {
        this.sharedAlertService.showWithActionHandler({
            header: 'Change default organisation?',
            message: 'Changing the default organisation within a tenancy has a lot of implications, '
                + 'including breaking existing URLs to resources belonging to the previous default organisation. '
                + 'Are you sure you wish to proceed?',
            buttons: [
                {
                    text: 'Cancel',
                    handler: (): any => {
                        this.popOverCtrl.dismiss(null);
                    },
                },
                {
                    text: 'Proceed',
                    handler: (): any => {
                        this.popOverCtrl.dismiss({
                            action: <ActionButtonPopover>{ actionName: 'defaultOrganisation' },
                        });
                    },
                },
            ],
        });
    }

    public onDeleteOrganisation(): void {
        if (this.organisationIsDefault) {
            this.sharedAlertService.showWithOk(
                'Cannot delete default organisation',
                'Before deleting this organisation you must set another organisation as the default organisation.',
            );
            return;
        }

        this.sharedAlertService.showWithActionHandler({
            header: 'Delete organisation? ',
            message: 'By deleting this organisation the associated users, '
                + 'customers and other records are permanently removed from the platform. '
                + 'Are you sure you wish to proceed?',
            buttons: [
                {
                    text: 'Cancel',
                    handler: (): any => {
                        this.popOverCtrl.dismiss(null);
                    },
                },
                {
                    text: 'Delete',
                    handler: (): any => {
                        this.popOverCtrl.dismiss({ action: <ActionButtonPopover>{ actionName: 'deleteOrganisation' } });
                    },
                },
            ],
        });
    }

    public onAddUser(): void {
        this.popOverCtrl.dismiss({ action: <ActionButtonPopover>{ actionName: 'addUser' } });
    }

    public onAddPortal(): void {
        this.popOverCtrl.dismiss({ action: <ActionButtonPopover>{ actionName: 'addPortal' } });
    }

    public onEdit(): void {
        this.popOverCtrl.dismiss({ action: <ActionButtonPopover>{ actionName: 'edit' } });
    }
}
