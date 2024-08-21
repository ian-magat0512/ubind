import { Component, ElementRef, Injector, OnDestroy, OnInit } from '@angular/core';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { UserApiService } from '@app/services/api/user-api.service';
import { CustomerApiService } from '@app/services/api/customer-api.service';
import { EventService } from '@app/services/event.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { takeUntil } from 'rxjs/operators';
import { Subject } from 'rxjs';
import { UserResourceModel } from '@app/resource-models/user/user.resource-model';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { CustomerDetailsResourceModel, CustomerResourceModel } from '@app/resource-models/customer.resource-model';
import { contentAnimation } from '@assets/animations';
import { AuthenticationService } from '@app/services/authentication.service';
import { Permission } from '@app/helpers';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Assign customer's agent component.
 */
@Component({
    selector: 'app-assign-agent',
    templateUrl: './assign-agent.component.html',
    styleUrls: [
        './assign-agent.component.scss',
        '../../../../assets/css/form-toolbar.scss',
    ],
    animations: [contentAnimation],
})
export class AssignAgentComponent extends DetailPage implements OnInit, OnDestroy {

    public defaultUserImgPath: string = 'assets/imgs/person-md.svg';
    public usersWithManageCustomerRole: Array<UserResourceModel>;
    public selectedAgentUserId: string;
    public isLoading: boolean;
    public hasAgent: boolean;

    private customerId: string;
    private agentDisplayName: string;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        protected eventService: EventService,
        elementRef: ElementRef,
        public injector: Injector,
        public layoutManager: LayoutManagerService,
        private userApiService: UserApiService,
        private customerApiService: CustomerApiService,
        private navProxy: NavProxyService,
        private routeHelper: RouteHelper,
        private alertService: SharedAlertService,
        private authService: AuthenticationService,
    ) {
        super(eventService, elementRef, injector);
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.init();
    }

    public ngOnDestroy(): void {
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    public closeButtonClicked(): void {
        this.navProxy.navigateBackOne();
    }

    public async assignButtonClicked(): Promise<void> {
        this.isLoading = true;

        if (!this.selectedAgentUserId) {
            this.alertService.showWithOk(
                "No user selected",
                `To assign ${this.hasAgent ? 'a new' : 'an'}`
                + ` agent for this customer, please select a user from the list.`,
            );
            return;
        }

        let selectedUser: UserResourceModel = this.usersWithManageCustomerRole
            .filter((u: UserResourceModel) => u.id === this.selectedAgentUserId)[0];
        this.agentDisplayName = selectedUser.displayName ? selectedUser.displayName : selectedUser.fullName;

        try {

            const customer: CustomerResourceModel = await this.customerApiService.getById(this.customerId)
                .pipe(takeUntil(this.destroyed))
                .toPromise();

            await this.customerApiService.assignOwner(this.customerId, this.selectedAgentUserId)
                .pipe(takeUntil(this.destroyed))
                .toPromise();

            const customerDisplayName: string = customer.displayName ? customer.displayName : customer.fullName;
            this.alertService.showToast(`${this.agentDisplayName} assigned as the`
                + `${this.hasAgent ? ' new' : ''} agent for ${customerDisplayName}`);

            if (this.authService.permissions.indexOf(Permission.ManageAllCustomers) > -1) {
                customer.deleteFromList = false;
                this.eventService.getEntityUpdatedSubject('Customer').next(customer);
                this.navProxy.navigateBackOne();
            } else {
                customer.deleteFromList = true;
                this.eventService.getEntityUpdatedSubject('Customer').next(customer);
                this.navProxy.goToCustomerList();
            }
        } catch (error) {
            this.errorMessage = 'There was a problem assigning customer to an agent';
            throw error;
        } finally {
            this.isLoading = false;
        }
    }

    public change(event: any): void {
        this.selectedAgentUserId = event.value;
    }

    private init(): void {
        this.customerId = this.routeHelper.getPathParam("customerId");
        this.hasAgent = this.routeHelper.getQueryParam("hasAgent") === "true";
        this.getUsersAssignableAsAgent();
    }

    private async getUsersAssignableAsAgent(): Promise<void> {
        this.isLoading = true;

        try {
            const customer: CustomerDetailsResourceModel =
                await this.customerApiService.getCustomerDetails(this.customerId).toPromise();
            const users: Array<UserResourceModel> =
                await this.userApiService.getUsersAssignableAsOwner(customer.organisationId).toPromise();
            this.usersWithManageCustomerRole = users.filter((u: UserResourceModel) => u.id !== customer.ownerId);
        } catch (error) {
            this.errorMessage = "There was a problem loading users assignable as owner.";
            throw error;
        } finally {
            this.isLoading = false;
        }
    }
}
