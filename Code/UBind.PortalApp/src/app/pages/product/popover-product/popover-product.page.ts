import { Component, OnInit } from '@angular/core';
import { NavParams, PopoverController } from '@ionic/angular';
import { Permission } from '@app/helpers/permissions.helper';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { AuthenticationService } from '@app/services/authentication.service';
import { ActionButtonPopover } from '@app/models/action-button-popover';
import { IconLibrary } from '@app/models/icon-library.enum';
import { DeploymentEnvironment } from '@app/models/deployment-environment.enum';

export enum PopoverProductAction {
    DisableProduct = 'disableProduct',
    EnableProduct = 'enableProduct',
    DeleteProduct = 'deleteProduct',
    SynchronizeProductQuoteAssets = 'synchronizeProductQuoteAssets',
    SynchronizeProductClaimAssets = 'synchronizeProductClaimAssets',
    Edit = 'edit',
    CreateRelease = 'createRelease',
    MoveUnassociatedQuotes = 'moveUnassociatedQuotes',
}

/**
 * Export popover product page component class.
 * This class manage Product popover function.
 */
@Component({
    selector: 'app-popover-product',
    templateUrl: './popover-product.page.html',
})
export class PopoverProductPage implements OnInit {
    public productIsDisabled: boolean = false;
    public permission: typeof Permission = Permission;
    public actions: Array<ActionButtonPopover> = [];
    public segment: string;
    public canCreateRelease: boolean = false;
    public iconLibrary: typeof IconLibrary = IconLibrary;
    protected deploymentEnvironment: typeof DeploymentEnvironment = DeploymentEnvironment;

    public constructor(
        private navParams: NavParams,
        public popOverCtrl: PopoverController,
        private sharedAlertService: SharedAlertService,
        private authService: AuthenticationService,
    ) {
    }

    public ngOnInit(): void {
        this.productIsDisabled = this.navParams.get('isDisabled');
        this.actions = this.navParams.get('actions') || [];
        this.segment = this.navParams.get('segment');
        this.canCreateRelease = this.navParams.get('canCreateRelease');
    }

    public onDisableProduct(): void {
        this.sharedAlertService.showWithActionHandler({
            header: 'Disable Product',
            message: 'By disabling this product it will no longer '
                + 'be accessible to users and customers. '
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
                        this.popOverCtrl.dismiss(
                            { action: <ActionButtonPopover>{ actionName: PopoverProductAction.DisableProduct } });
                    },
                },
            ],
        });
    }

    public onEnableProduct(): void {
        this.popOverCtrl.dismiss({ action: <ActionButtonPopover>{ actionName: PopoverProductAction.EnableProduct } });
    }

    public onDeleteProduct(): void {
        this.sharedAlertService.showWithActionHandler({
            header: 'Delete Product',
            message: `By deleting this product all associated quotes, `
                + `${this.authService.isMutualTenant() ? 'protections' : 'policies'} `
                + `and other records are permanently removed from the platform. `
                + `Are you sure you wish to proceed?`,
            buttons: [
                {
                    text: 'No',
                    handler: (): any => {
                        this.popOverCtrl.dismiss(null);
                    },
                }, {
                    text: 'Yes',
                    handler: (): any => {
                        this.popOverCtrl.dismiss(
                            { action: <ActionButtonPopover>{ actionName: PopoverProductAction.DeleteProduct } });
                    },
                },
            ],
        });
    }

    public onSynchronizeProductQuoteAssets(): void {
        this.popOverCtrl.dismiss(
            { action: <ActionButtonPopover>{ actionName: PopoverProductAction.SynchronizeProductQuoteAssets } });
    }

    public onSynchronizeProductClaimAssets(): void {
        this.popOverCtrl.dismiss(
            { action: <ActionButtonPopover>{ actionName: PopoverProductAction.SynchronizeProductClaimAssets } });
    }

    public onEdit(): void {
        this.popOverCtrl.dismiss({ action: <ActionButtonPopover>{ actionName: PopoverProductAction.Edit } });
    }

    public onCreateRelease(): void {
        this.popOverCtrl.dismiss({ action: <ActionButtonPopover>{ actionName: PopoverProductAction.CreateRelease } });
    }

    public onMoveUnassociatedQuotes(environment: DeploymentEnvironment): void {
        this.popOverCtrl.dismiss({
            action: <ActionButtonPopover>{ actionName: PopoverProductAction.MoveUnassociatedQuotes },
            environment: environment,
        });
    }
}
