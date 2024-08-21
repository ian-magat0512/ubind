import { Component } from '@angular/core';
import { ActionButtonPopover } from '@app/models/action-button-popover';
import { IconLibrary } from '@app/models/icon-library.enum';
import { PopoverController } from '@ionic/angular';

/**
 * The component for additional properties popover menu.
 */
@Component({
    selector: 'app-popover-additional-property',
    templateUrl: './popover-additional-property.component.html',
})

export class PopoverAdditionalPropertyComponent {
    public iconLibrary: typeof IconLibrary = IconLibrary;
    public constructor(private popOverCtrl: PopoverController) { }

    public delete(): void {
        this.popOverCtrl.dismiss({ action: <ActionButtonPopover>{ actionName: 'delete' } });
    }
}
