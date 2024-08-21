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
import { AssignPortalEntityType } from "@app/models/assign-portal-entity-type.enum";
import { UserApiService } from '@app/services/api/user-api.service';
import { UserResourceModel } from '@app/resource-models/user/user.resource-model';
import { UserType } from '@app/models/user-type.enum';
import { RouteHelper } from '@app/helpers/route.helper';

/**
 * A popover menu for assigning a portal to a customer/user from the customer/user details card.
 */
@Component({
    selector: 'app-popover-assign-portal',
    templateUrl: './popover-assign-portal.component.html',
})
export class PopoverAssignPortalComponent implements OnInit, OnDestroy {
    public entityType: AssignPortalEntityType;
    public customerId: string;
    public customerName: string;
    public entityOrganisationId: string;
    public entityTenantId: string;
    public userId: string;
    public userName: string;
    public userType: UserType;
    public portalId: string;
    public portalName: string;
    public hasPortal: boolean = false;
    public handleUnassignPortal: () => Promise<void>;
    public handleAssignPortal: () => void;

    public permission: typeof Permission = Permission;
    public destroyed: Subject<void>;

    public constructor(
        private navProxy: NavProxyService,
        private sharedAlertService: SharedAlertService,
        private customerApiService: CustomerApiService,
        private userApiService: UserApiService,
        private eventService: EventService,
        private alertService: SharedAlertService,
        private loading: SharedLoaderService,
        private popoverController: PopoverController,
        private routeHelper: RouteHelper,
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
        this.popoverController.dismiss();
        if (this.entityType === AssignPortalEntityType.Customer) {
            this.assignPortalToCustomer();
        }
        if (this.entityType === AssignPortalEntityType.User) {
            this.assignPortalToUser();
        }
    }

    public unassignPortal(): void {
        let displayName: string = this.entityType === AssignPortalEntityType.Customer
        ? this.customerName : this.userName;
        this.sharedAlertService.showWithActionHandler({
            header: `Un-assign ${this.entityType} portal`,
            message: `When a ${this.entityType} is not assigned to a specific portal, `
                + `they will automatically access their `
                + `account using the default portal. Are you sure you want to un-assign the ${this.portalName} portal `
                + `from ${displayName}?`,
            buttons: [
                {
                    text: 'Cancel',
                    handler: (): any => this.popoverController.dismiss(),
                }, {
                    text: 'Un-assign',
                    handler: async (): Promise<void> => {
                        this.loading.presentWithDelay();
                        try {
                            if (this.entityType === AssignPortalEntityType.Customer) {
                                await this.unassignPortalFromCustomer();
                            }
                            if (this.entityType === AssignPortalEntityType.User) {
                                await this.unassignPortalFromUser();
                            }
                        } finally {
                            this.loading.dismiss();
                            this.popoverController.dismiss();
                        }
                    },
                },
            ],
        });
    }

    private assignPortalToCustomer(): void {
        this.navProxy.navigate(
            ["customer", this.customerId, "assign-portal"],
            {
                queryParams:
                    {
                        existingAssignedPortalId: this.portalId,
                        selectedEntityOrganisationId: this.entityOrganisationId,
                        selectedEntityTenantId: this.entityTenantId,
                    },
            });
    }

    private assignPortalToUser(): void {
        this.navProxy.navigate(
            this.getNavigatePathForUser(true),
            {
                queryParams:
                {
                    existingAssignedPortalId: this.portalId,
                    selectedEntityOrganisationId: this.entityOrganisationId,
                    selectedEntityTenantId: this.entityTenantId,
                    userType: this.userType,
                },
            });
    }

    private async unassignPortalFromCustomer(): Promise<void> {
        await this.customerApiService.unassignPortalFromCustomer(this.customerId)
            .pipe(takeUntil(this.destroyed)).toPromise();
        this.alertService.showToast(`The portal for ${this.customerName} has been un-assigned`);
        await this.updateCustomerEntitySubject();
        this.navProxy.navigate(["customer", this.customerId]);
    }

    private async unassignPortalFromUser(): Promise<void> {
        await this.userApiService.unassignPortalFromUser(this.userId, this.entityTenantId)
            .pipe(takeUntil(this.destroyed)).toPromise();
        this.alertService.showToast(`The portal for ${this.userName} has been un-assigned`);
        await this.updateUserEntitySubject();
        this.navProxy.navigate(this.getNavigatePathForUser());
    }

    private async updateUserEntitySubject(): Promise<void> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', this.routeHelper.getContextTenantAlias());
        let user: UserResourceModel = await this.userApiService.getById(this.userId, params)
            .pipe(takeUntil(this.destroyed)).toPromise();
        this.eventService.getEntityUpdatedSubject('User').next(user);
    }

    private async updateCustomerEntitySubject(): Promise<void> {
        const customer: CustomerResourceModel =
        await this.customerApiService.getById(this.customerId)
            .pipe(takeUntil(this.destroyed)).toPromise();
        customer.deleteFromList = false;
        this.eventService.getEntityUpdatedSubject('Customer').next(customer);
    }

    private getNavigatePathForUser(forAssignPortal?: boolean): Array<string> {
        let navigatePath: Array<string> = ["user", this.userId];
        const tenantPath: Array<string> = this.getTenantPath();
        if (forAssignPortal) {
            navigatePath = [...navigatePath, "assign-portal"];
        }
        if (this.routeHelper.hasPathSegment('organisation')) {
            navigatePath = [...tenantPath, "organisation", this.entityOrganisationId, ...navigatePath];
        }
        return navigatePath;
    }

    private getTenantPath(): Array<string> {
        let tenantPath: Array<string> = [];
        if (this.routeHelper.hasPathSegment('tenant')) {
            let tenantAlias: string = this.routeHelper.getParam('tenantAlias');
            tenantPath = ["tenant", tenantAlias];
        }
        return tenantPath;
    }
}
