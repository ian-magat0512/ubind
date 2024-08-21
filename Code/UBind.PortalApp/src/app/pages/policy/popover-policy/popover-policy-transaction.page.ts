import { Component } from '@angular/core';
import { Permission } from '@app/helpers/permissions.helper';
import { ActionButtonPopover } from '@app/models/action-button-popover';
import { IconLibrary } from '@app/models/icon-library.enum';
import { EventService } from '@app/services/event.service';
import { PopoverController, NavParams } from '@ionic/angular';

/**
 * Export popover policy page component class.
 * TODO: Write a better class header: policy popover component function.
 */
@Component({
    selector: 'app-popover-policy-transaction',
    templateUrl: './popover-policy-transaction.page.html',
})
export class PopoverPolicyTransactionPage {
    public permission: typeof Permission = Permission;
    public editPropertiesTitle: string;
    public canEditAdditionalPropertyValues: boolean = false;
    public actions: Array<ActionButtonPopover> = [];
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        private navParams: NavParams,
        public popOverCtrl: PopoverController,
        protected eventService: EventService,
    ) {
        this.canEditAdditionalPropertyValues = this.navParams.get('canEditAdditionalPropertyValues');
        this.actions = this.navParams.get('actions') || [];
    }

    public close(action: string): void {
        this.popOverCtrl.dismiss({ action: <ActionButtonPopover>{ actionName: action } });
    }
}
