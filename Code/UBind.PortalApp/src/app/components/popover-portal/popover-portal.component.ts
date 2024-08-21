import { Component, OnInit, OnDestroy } from '@angular/core';
import { PopoverController } from '@ionic/angular';
import { Permission } from '@app/helpers/permissions.helper';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { CustomerApiService } from '@app/services/api/customer-api.service';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { CustomerResourceModel } from '@app/resource-models/customer.resource-model';
import { EventService } from '@app/services/event.service';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Export popover portal page component class.
 * This class manage portal popover function.
 */
@Component({
    selector: 'app-popover-portal',
    templateUrl: './popover-portal.component.html',
})
export class PopoverPortalComponent implements OnInit, OnDestroy {
    public customerId: string;
    public customerName: string;
    public portalId: string;
    public portalName: string;
    public hasPortal: boolean = false;
    public permission: typeof Permission = Permission;
    public destroyed: Subject<void>;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        private navProxy: NavProxyService,
        private sharedAlertService: SharedAlertService,
        private customerApiService: CustomerApiService,
        private eventService: EventService,
        private alertService: SharedAlertService,
        private loading: SharedLoaderService,
        private popoverController: PopoverController,
    ) {
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
    }

    public ngOnDestroy(): void {
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    public assignPortal(): void {
        this.navProxy.navigate(
            ["customer", this.customerId, "assign-portal"],
            {
                queryParams:
                {
                    selectedPortalId: this.portalId,
                    customerId: this.customerId,
                },
            },
        );
        this.popoverController.dismiss();
    }

    public unassignPortal(): void {
        this.sharedAlertService.showWithActionHandler({
            header: 'Un-assign customer portal',
            message: `When a customer is not assigned to a specific portal, they will automatically access their `
                + `account using the default portal. Are you sure you want to un-assign the ${this.portalName} portal `
                + `from ${this.customerName}?`,
            buttons: [
                {
                    text: 'Cancel',
                    handler: (): any => this.popoverController.dismiss(),
                }, {
                    text: 'Un-assign',
                    handler: async (): Promise<void> => {
                        await this.loading.presentWithDelay();
                        try {
                            await this.customerApiService.unassignPortalFromCustomer(this.customerId)
                                .pipe(takeUntil(this.destroyed)).toPromise();
                            this.alertService.showToast(`The portal for ${this.customerName} has been un-assigned`);

                            const customer: CustomerResourceModel =
                                await this.customerApiService.getById(this.customerId)
                                    .pipe(takeUntil(this.destroyed)).toPromise();
                            customer.deleteFromList = false;
                            this.eventService.getEntityUpdatedSubject('Customer').next(customer);
                            this.navProxy.navigate(["customer", this.customerId]);
                        } finally {
                            this.loading.dismiss();
                            this.popoverController.dismiss();
                        }
                    },
                },
            ],
        });
    }
}
