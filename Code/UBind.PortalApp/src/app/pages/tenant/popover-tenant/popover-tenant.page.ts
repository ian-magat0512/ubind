import { Component, OnInit } from '@angular/core';
import { NavParams, PopoverController } from '@ionic/angular';
import { Permission } from '@app/helpers/permissions.helper';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { ActionButtonPopover } from '@app/models/action-button-popover';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Export popover tenant page component class.
 * This class manage tenant popover page functions.
 */
@Component({
    selector: 'app-popover-tenant',
    templateUrl: './popover-tenant.page.html',
})
export class PopoverTenantPage implements OnInit {
    public tenantIsDisabled: boolean = false;
    public permission: typeof Permission = Permission;
    public segment: string;
    public canEditTenant: boolean = false;
    public canCreateProduct: boolean = false;
    public canCreateOrganisation: boolean = false;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        private navParams: NavParams,
        private sharedAlertService: SharedAlertService,
        private popCtrl: PopoverController,
    ) { }

    public ngOnInit(): void {
        this.tenantIsDisabled = this.navParams.get('status');
        this.segment = this.navParams.get('segment');
        this.canEditTenant = this.navParams.get('canEditTenant');
        this.canCreateProduct = this.navParams.get('canCreateProduct');
        this.canCreateOrganisation = this.navParams.get('canCreateOrganisation');
    }

    public onDisableTenant(): void {
        this.sharedAlertService.showWithActionHandler({
            header: 'Disable Tenant',
            message: 'By disabling this tenant the associated products and '
                + 'user accounts will no longer be accessible. Are you sure you wish to proceed?',
            buttons: [
                {
                    text: 'No',
                    handler: (): any => {
                        this.popCtrl.dismiss(null);
                    },
                }, {
                    text: 'Yes',
                    handler: (): any => {
                        this.popCtrl.dismiss({ action: <ActionButtonPopover>{ actionName: 'disableTenant' } });
                    },
                },
            ],
        });
    }

    public onEnableTenant(): void {
        this.popCtrl.dismiss({ action: <ActionButtonPopover>{ actionName: 'enableTenant' } });
    }

    public onDeleteTenant(): void {
        this.sharedAlertService.showWithActionHandler({
            header: 'Delete Tenant',
            message: 'By deleting this tenant the associated users, products and '
                + 'other records are permanently removed from the platform. Are you sure you wish to proceed?',
            buttons: [
                {
                    text: 'No',
                    handler: (): any => {
                        this.popCtrl.dismiss(null);
                    },
                }, {
                    text: 'Yes',
                    handler: (): any => {
                        this.popCtrl.dismiss({ action: <ActionButtonPopover>{ actionName: 'deleteTenant' } });
                    },
                },
            ],
        });
    }

    public onEdit(): void {
        this.popCtrl.dismiss({ action: <ActionButtonPopover>{ actionName: 'edit' } });
    }

    public onCreateProduct(): void {
        this.popCtrl.dismiss({ action: <ActionButtonPopover>{ actionName: 'createProduct' } });
    }

    public onCreateOrganisation(): void {
        this.popCtrl.dismiss({ action: <ActionButtonPopover>{ actionName: 'createOrganisation' } });
    }
}
