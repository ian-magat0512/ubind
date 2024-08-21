import { Component } from '@angular/core';
import { PopoverController } from '@ionic/angular';
import { Permission } from '@app/helpers';
import { ActionButtonPopover } from '@app/models/action-button-popover';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Drop down menu for a role
 */
@Component({
    selector: 'popover-role',
    templateUrl: 'popover-role.component.html',
})

export class PopoverRoleComponent {

    public permission: typeof Permission = Permission;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(public popOverCtrl: PopoverController) { }

    public close(action: string): void {
        this.popOverCtrl.dismiss({ action: <ActionButtonPopover>{ actionName: action } });
    }
}
