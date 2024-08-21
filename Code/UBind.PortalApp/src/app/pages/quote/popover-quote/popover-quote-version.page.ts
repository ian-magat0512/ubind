import { Component } from "@angular/core";
import { PopoverController } from "@ionic/angular";
import { Permission } from "@app/helpers";
import { ActionButtonPopover } from "@app/models/action-button-popover";
import { IconLibrary } from "@app/models/icon-library.enum";

/**
 * Export popover quote version page component class.
 * This class manage quote popover page function.
 */
@Component({
    selector: 'app-popover-quote-version',
    templateUrl: './popover-quote-version.page.html',
})
export class PopoverQuoteVersionPage {
    public permission: typeof Permission = Permission;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        private popCtrl: PopoverController,
    ) { }

    public doAction(action: any): void {
        this.popCtrl.dismiss({ action: <ActionButtonPopover>{ actionName: action } });
    }
}
