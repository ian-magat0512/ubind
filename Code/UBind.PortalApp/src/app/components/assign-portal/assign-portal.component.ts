import { Component, OnDestroy, OnInit, Injector, ElementRef, Input, Output, EventEmitter } from "@angular/core";
import { DetailPage } from "@app/pages/master-detail/detail.page";
import { contentAnimation } from '@assets/animations';
import { EventService } from "@app/services/event.service";
import { Subject } from "rxjs";
import { LayoutManagerService } from "@app/services/layout-manager.service";
import { NavProxyService } from "@app/services/nav-proxy.service";
import { PortalResourceModel } from "@app/resource-models/portal.resource-model";
import { PortalApiService } from "@app/services/api/portal-api.service";
import { takeUntil, finalize } from "rxjs/operators";
import { AppConfigService } from "@app/services/app-config.service";
import { SharedAlertService } from "@app/services/shared-alert.service";
import { SharedLoaderService } from "@app/services/shared-loader.service";
import { IconLibrary } from "@app/models/icon-library.enum";
import { PortalUserType } from "@app/models/portal-user-type.enum";
import { AssignPortalEntityType } from "@app/models/assign-portal-entity-type.enum";
import { UserType } from "@app/models/user-type.enum";

/**
 * Assign portal component.
 */
@Component({
    selector: 'app-assign-portal',
    templateUrl: './assign-portal.component.html',
    styleUrls: [
        './assign-portal.component.scss',
        '../../../assets/css/form-toolbar.scss',
    ],
    animations: [contentAnimation],
})
export class AssignPortalComponent extends DetailPage implements OnInit, OnDestroy {

    @Input () public assignPortalEntityType: AssignPortalEntityType;
    @Input () public userType: UserType;
    @Input () public selectedEntityTenantId: string;
    @Input () public selectedEntityOrganisationId: string;
    @Input () public existingAssignedPortalId: string | null;
    @Output() public assignButtonClicked: EventEmitter<any> = new EventEmitter();

    public portals: Array<PortalResourceModel>;
    public iconLibrary: typeof IconLibrary = IconLibrary;
    private selectedPortal: PortalResourceModel = null;
    private portalUserType: PortalUserType;
    private portalPrefixText: string;

    public constructor(
        protected eventService: EventService,
        elementRef: ElementRef,
        public injector: Injector,
        public layoutManager: LayoutManagerService,
        private navProxy: NavProxyService,
        private portalApiService: PortalApiService,
        private appConfigService: AppConfigService,
        private alertService: SharedAlertService,
        private sharedLoaderService: SharedLoaderService,
    ) {
        super(eventService, elementRef, injector);
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.portalUserType = this.getPortalUserType();
        this.selectedPortal = null;
        this.loadPortals();
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

    public change(event: any): void {
        this.selectedPortal = this.portals.filter((p: PortalResourceModel) =>
            p.id == event.value)[0] ?? null;
    }

    public showSuccessToast(entitiyDisplayName: string): void {
        this.alertService.showToast(
            `${this.selectedPortal.name} assigned as the ${this.portalPrefixText}portal for ${entitiyDisplayName}`,
        );
    }

    public navigateBack(): void {
        this.navProxy.navigateBackOne();
    }

    public async handleAssignButtonClicked(): Promise<void> {
        this.portalPrefixText = this.existingAssignedPortalId ? 'new ' : '';
        if (this.selectedPortal === null) {
            this.alertService.showWithOk(
                `No portal selected`,
                `To assign a ${this.portalPrefixText}portal for this `
                + `${this.assignPortalEntityType}, please select a portal from the list.`,
            );
            return;
        }

        await this.sharedLoaderService.present();

        try {
            this.assignButtonClicked.emit({
                selectedPortal: this.selectedPortal,
            });
        } catch (error) {
            this.errorMessage = `There was a problem assigning portal to ${this.assignPortalEntityType}`;
            throw error;
        } finally {
            this.sharedLoaderService.dismiss();
        }
    }

    private getPortalUserType(): PortalUserType {
        if (this.assignPortalEntityType === AssignPortalEntityType.Customer || this.userType === UserType.Customer) {
            return PortalUserType.Customer;
        }
        return PortalUserType.Agent;
    }

    private async loadPortals(): Promise<void> {
        this.isLoading = true;
        this.portalApiService.getActivePortals(
            this.selectedEntityTenantId, this.selectedEntityOrganisationId, this.portalUserType)
            .pipe(
                takeUntil(this.destroyed),
                finalize(() => this.isLoading = false),
            )
            .toPromise().then((portals: Array<PortalResourceModel>) => {
                this.portals = portals;
                if (this.existingAssignedPortalId) {
                    this.portals = portals.filter((portal: PortalResourceModel) =>
                        portal.id !== this.existingAssignedPortalId);
                }
            });
    }
}
