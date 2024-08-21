import { Component } from '@angular/core';
import { Permission } from '@app/helpers';
import { IconLibrary } from '@app/models/icon-library.enum';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { PopoverController } from '@ionic/angular';

/**
 * Component class for data table popover menu.
 */
@Component({
    selector: 'app-popover-data-table',
    templateUrl: './popover-data-table.component.html',
})
export class PopoverDataTableComponent {

    public permission: typeof Permission = Permission;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        public popOverCtrl: PopoverController,
        private sharedAlertService: SharedAlertService,
    ) { }

    public download(): void {
        this.popOverCtrl.dismiss({ 'action': 'download' });
    }

    public delete(): void {
        this.sharedAlertService.showWithActionHandler({
            header: 'Delete Data Table',
            message: 'By deleting this data table the associated database table '
                + 'will be permanently removed from the platform. Are you sure you wish to proceed?',
            buttons: [
                {
                    text: 'No',
                    handler: (): any => {
                        this.popOverCtrl.dismiss(null);
                    },
                }, {
                    text: 'Yes',
                    handler: (): any => {
                        this.popOverCtrl.dismiss({ 'action': 'delete' });
                    },
                },
            ],
        });
    }

    public edit(): void {
        this.popOverCtrl.dismiss({ 'action': 'edit' });
    }
}
