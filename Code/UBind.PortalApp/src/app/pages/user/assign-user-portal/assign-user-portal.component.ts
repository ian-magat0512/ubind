import { Component, OnDestroy, OnInit, Injector, ElementRef, ViewChild } from "@angular/core";
import { DetailPage } from "@app/pages/master-detail/detail.page";
import { contentAnimation } from '@assets/animations';
import { EventService } from "@app/services/event.service";
import { Subject } from "rxjs";
import { LayoutManagerService } from "@app/services/layout-manager.service";
import { NavProxyService } from "@app/services/nav-proxy.service";
import { PortalResourceModel } from "@app/resource-models/portal.resource-model";
import { takeUntil } from "rxjs/operators";
import { IconLibrary } from "@app/models/icon-library.enum";
import { AssignPortalComponent } from "@app/components/assign-portal/assign-portal.component";
import { UserApiService } from "@app/services/api/user-api.service";
import { UserResourceModel } from "@app/resource-models/user/user.resource-model";
import { AssignPortalEntityType } from "@app/models/assign-portal-entity-type.enum";
import { UserType } from "@app/models/user-type.enum";
import { RouteHelper } from "@app/helpers/route.helper";

/**
 * Assign user's portal component.
 */
@Component({
    selector: 'app-assign-user-portal',
    templateUrl: './assign-user-portal.component.html',
    styleUrls: [
        './assign-user-portal.component.scss',
        '../../../../assets/css/form-toolbar.scss',
    ],
    animations: [contentAnimation],
})
export class AssignUserPortalComponent extends DetailPage implements OnInit, OnDestroy {
    @ViewChild(AssignPortalComponent) public assignPortalComponent: AssignPortalComponent;

    public existingAssignedPortalId: string | null;

    private selectedUserId: string;
    public iconLibrary: typeof IconLibrary = IconLibrary;
    public assignPortalEntityType: AssignPortalEntityType;
    public userType: UserType;
    public selectedUserOrganisationId: string;
    public selectedUserTenantId: string;

    public constructor(
        protected eventService: EventService,
        elementRef: ElementRef,
        public injector: Injector,
        public layoutManager: LayoutManagerService,
        private navProxy: NavProxyService,
        private userApiService: UserApiService,
        private routeHelper: RouteHelper,
    ) {
        super(eventService, elementRef, injector);
        this.existingAssignedPortalId = this.routeHelper.getQueryParam('existingAssignedPortalId');
        this.selectedUserOrganisationId = this.routeHelper.getQueryParam('selectedEntityOrganisationId');
        this.selectedUserTenantId =  this.routeHelper.getQueryParam('selectedEntityTenantId');
        this.userType = this.routeHelper.getQueryParam('userType') as UserType;
        this.selectedUserId = this.routeHelper.getParam('userId');
        this.assignPortalEntityType = AssignPortalEntityType.User;
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
        await this.userApiService.assignPortalToUser(this.selectedUserId, selectedPortal.id, this.selectedUserTenantId)
            .pipe(takeUntil(this.destroyed)).toPromise();
        let user: UserResourceModel = await this.getUserModel();
        this.assignPortalComponent.showSuccessToast(user.displayName);
        this.eventService.getEntityUpdatedSubject('User').next(user);
        this.navProxy.navigateBackOne();
    }

    private async getUserModel(): Promise<UserResourceModel> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', this.routeHelper.getContextTenantAlias());
        let user: UserResourceModel = await this.userApiService.getById(this.selectedUserId, params)
            .pipe(takeUntil(this.destroyed)).toPromise();
        return user;
    }
}
