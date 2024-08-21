import { Component, OnInit } from '@angular/core';
import { ActionButtonPopover } from '@app/models/action-button-popover';
import { NavParams, PopoverController } from '@ionic/angular';

/**
 * Drop down menu for a financial transaction (e.g. payment or refund)
 */
@Component({
    templateUrl: './popover-transaction.component.html',
})
export class PopoverTransactionComponent implements OnInit {
    public shouldShowPopOverCreatePayment: boolean = false;
    public shouldShowPopOverCreateRefund: boolean = false;

    public actions: Array<ActionButtonPopover> = [];

    public constructor(public popOverCtrl: PopoverController, private navParams: NavParams) {
        this.shouldShowPopOverCreatePayment = this.navParams.get('shouldShowPopOverCreatePayment');
        this.shouldShowPopOverCreateRefund = this.navParams.get('shouldShowPopOverCreateRefund');
    }

    public close(action: string): void {
        this.popOverCtrl.dismiss({ action: <ActionButtonPopover>{ actionName: action } });
    }

    public ngOnInit(): void {
        this.actions = this.navParams.get('actions') || [];
    }
}
