import { Component, OnInit } from '@angular/core';
import { ActionButtonPopover } from '@app/models/action-button-popover';
import { NavParams, PopoverController } from '@ionic/angular';

/**
 * Generic drop down menu that can have dynamic menut items passed in.
 */
@Component({
    selector: 'app-popover-view',
    templateUrl: './popover-view.component.html',
    styles: [],
})
export class PopoverViewComponent implements OnInit {
    public actions: Array<ActionButtonPopover> = [];

    public constructor(
        private navParams: NavParams,
        public popOverCtrl: PopoverController,
    ) {
    }

    public ngOnInit(): void {
        this.actions = this.navParams.get('actions') || [];
    }
}
