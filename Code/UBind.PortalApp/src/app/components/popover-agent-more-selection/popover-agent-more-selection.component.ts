import { Component, OnDestroy, OnInit } from '@angular/core';
import { CustomerResourceModel } from '@app/resource-models/customer.resource-model';
import { Permission } from '@app/helpers';
import { CustomerApiService } from '@app/services/api/customer-api.service';
import { AuthenticationService } from '@app/services/authentication.service';
import { EventService } from '@app/services/event.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { PopoverController } from '@ionic/angular';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

/**
 * Export customer detail view model class.
 */
@Component({
    selector: 'app-popover-agent-more-selection',
    templateUrl: './popover-agent-more-selection.component.html',
    styleUrls: ['./popover-agent-more-selection.component.scss'],
})
export class PopoverAgentMoreSelectionComponent implements OnInit, OnDestroy {

    public customerId: string;
    public customerFullName: string;
    public hasAgent: boolean;
    public destroyed: Subject<void>;

    public constructor(
        private navProxy: NavProxyService,
        private popoverController: PopoverController,
        private customerService: CustomerApiService,
        private sharedLoaderService: SharedLoaderService,
        private alertService: SharedAlertService,
        private eventService: EventService,
        private authService: AuthenticationService,
    ) { }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
    }

    public ngOnDestroy(): void {
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    public assignAgent(hasAgent: boolean): void {
        this.navProxy.navigate(
            ["customer", this.customerId, "assign-agent"],
            { queryParams: { hasAgent: hasAgent } },
        );
        this.popoverController.dismiss();
    }

    public unAssignAgent(): void {

        this.alertService.showWithActionHandler({
            header: `Un-assign customer agent`,
            message: `Once un-assigned, the current agent of this customer `
                + `may no longer be able to manage this customer. `
                + `Are you sure you want to un-assign the customer agent?`,
            buttons: [
                {
                    text: 'Cancel',
                    handler: (): void => {
                        this.popoverController.dismiss();
                    },
                }, {
                    text: 'Un-assign',
                    handler: async (): Promise<void> => {
                        this.sharedLoaderService.presentWithDelay();

                        try {
                            const customer: CustomerResourceModel = await this.customerService.getById(this.customerId)
                                .pipe(takeUntil(this.destroyed))
                                .toPromise();
                            await this.customerService.assignOwner(this.customerId, null)
                                .pipe(takeUntil(this.destroyed))
                                .toPromise();

                            const displayName: string = customer.displayName ? customer.displayName : customer.fullName;
                            this.alertService.showToast(`The agent for ${displayName} has been un-assigned`);

                            if (this.authService.permissions.indexOf(Permission.ManageAllCustomers) > -1) {
                                customer.deleteFromList = false;
                                this.eventService.getEntityUpdatedSubject('Customer').next(customer);
                                this.navProxy.navigate(["customer", this.customerId]);
                            } else {
                                customer.deleteFromList = true;
                                this.eventService.getEntityUpdatedSubject('Customer').next(customer);
                                this.navProxy.goToCustomerList();
                            }
                        } finally {
                            this.sharedLoaderService.dismiss();
                            this.popoverController.dismiss();
                        }
                    },
                },
            ],
        });
    }
}
