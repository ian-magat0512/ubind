import { Component, OnInit, OnDestroy } from '@angular/core';
import { NavParams, PopoverController } from '@ionic/angular';
import { AuthenticationService } from '@app/services/authentication.service';
import { SubscriptionLike } from 'rxjs';
import { EventService } from '@app/services/event.service';
import { Permission } from '@app/helpers';
import { ActionButtonPopover } from '@app/models/action-button-popover';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Popover for list policy page used in exports.
 */
@Component({
    selector: 'app-popover-list-policy',
    templateUrl: './popover-list-policy.page.html',
})
export class PopoverListPolicyPage implements OnInit, OnDestroy {
    public export: boolean = false;
    public isMutual: boolean;
    public permission: typeof Permission = Permission;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    protected subscriptions: Array<SubscriptionLike> = new Array<SubscriptionLike>();
    public constructor(
        private popOverCtrl: PopoverController,
        navParams: NavParams,
        private authService: AuthenticationService,
        protected eventService: EventService,
    ) {
        this.export = navParams.get('export') || false;
    }

    public async ngOnInit(): Promise<void> {
        this.isMutual = this.authService.isMutualTenant();
    }

    public ngOnDestroy(): void {
        this.subscriptions.forEach((s: SubscriptionLike) => s.unsubscribe());
    }

    public canExportPolicy(): boolean {
        return this.export;
    }

    public close(action: string): void {
        this.popOverCtrl.dismiss({ action: <ActionButtonPopover>{ actionName: action } });
    }
}
