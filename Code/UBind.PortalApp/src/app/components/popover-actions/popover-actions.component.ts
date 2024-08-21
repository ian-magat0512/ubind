import { Component, Input } from '@angular/core';
import { ActionButtonPopover } from '@app/models/action-button-popover';
import { IconLibrary } from '@app/models/icon-library.enum';
import { PopoverController } from '@ionic/angular';

/**
 * Popover actions component
 */
@Component({
    templateUrl: './popover-actions.component.html',
    selector: 'app-popover-actions',
})
export class PopoverActionsComponent {
    @Input() public popOver: PopoverController;
    @Input() public actions: Array<ActionButtonPopover> = [];

    public iconLibrary: typeof IconLibrary = IconLibrary;

    public executeAction(action: ActionButtonPopover): void {
        this.popOver.dismiss({ action: action });
    }
}
