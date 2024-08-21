import { Component, OnDestroy, OnInit, Injector, ElementRef, ViewChild } from "@angular/core";
import { DetailPage } from "@app/pages/master-detail/detail.page";
import { contentAnimation } from '@assets/animations';
import { EventService } from "@app/services/event.service";
import { Subject } from "rxjs";
import { LayoutManagerService } from "@app/services/layout-manager.service";
import { NavProxyService } from "@app/services/nav-proxy.service";
import { PortalResourceModel } from "@app/resource-models/portal.resource-model";
import { takeUntil } from "rxjs/operators";
import { CustomerApiService } from "@app/services/api/customer-api.service";
import { CustomerResourceModel } from "@app/resource-models/customer.resource-model";
import { IconLibrary } from "@app/models/icon-library.enum";
import  { AssignPortalComponent } from "@app/components/assign-portal/assign-portal.component";
import { AssignPortalEntityType } from "@app/models/assign-portal-entity-type.enum";
import { UserType } from "@app/models/user-type.enum";
import { RouteHelper } from "@app/helpers/route.helper";

/**
 * Assign customer's portal component.
 */
@Component({
    selector: 'app-assign-customer-portal',
    templateUrl: './assign-customer-portal.component.html',
    styleUrls: [
        './assign-customer-portal.component.scss',
        '../../../../assets/css/form-toolbar.scss',
    ],
    animations: [contentAnimation],
})
export class AssignCustomerPortalComponent extends DetailPage implements OnInit, OnDestroy {
    @ViewChild(AssignPortalComponent) public assignPortalComponent: AssignPortalComponent;

    public existingAssignedPortalId: string | null;

    private customerId: string;
    public iconLibrary: typeof IconLibrary = IconLibrary;
    public assignPortalEntityType: AssignPortalEntityType;
    public userType: UserType;
    public customerOrganisationId: string;
    public customerTenantId: string;

    public constructor(
        protected eventService: EventService,
        elementRef: ElementRef,
        public injector: Injector,
        public layoutManager: LayoutManagerService,
        private navProxy: NavProxyService,
        private customerApiService: CustomerApiService,
        private routeHelper: RouteHelper,
    ) {
        super(eventService, elementRef, injector);
        this.existingAssignedPortalId = this.routeHelper.getQueryParam('existingAssignedPortalId');
        this.customerOrganisationId =  this.routeHelper.getQueryParam('selectedEntityOrganisationId');
        this.customerTenantId =  this.routeHelper.getQueryParam('selectedEntityTenantId');
        this.customerId =  this.routeHelper.getParam('customerId');
        this.assignPortalEntityType = AssignPortalEntityType.Customer;
        this.userType = UserType.Customer;
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

    public async handleAssignPortalButtonClicked({ selectedPortal }:
        { selectedPortal: PortalResourceModel}): Promise<void> {
        await this.customerApiService.assignPortalToCustomer(this.customerId, selectedPortal.id)
            .pipe(takeUntil(this.destroyed)).toPromise();
        let customer: CustomerResourceModel = await this.customerApiService.getById(this.customerId)
            .pipe(takeUntil(this.destroyed)).toPromise();
        this.assignPortalComponent.showSuccessToast(customer.displayName);
        customer.deleteFromList = false;
        this.eventService.getEntityUpdatedSubject('Customer').next(customer);
        this.navProxy.navigateBackOne();
    }
}
