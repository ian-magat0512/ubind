import { Component, ElementRef, Injector, OnDestroy, OnInit } from "@angular/core";
import { SelectableItem } from "@app/components/item-filter-select/item-filter-select.component";
import { IconLibrary } from "@app/models/icon-library.enum";
import { DetailPage } from "@app/pages/master-detail/detail.page";
import { OrganisationResourceModel } from "@app/resource-models/organisation/organisation.resource-model";
import { OrganisationApiService } from "@app/services/api/organisation-api.service";
import { EventService } from "@app/services/event.service";
import { LayoutManagerService } from "@app/services/layout-manager.service";
import { Subject } from "rxjs";
import { finalize, takeUntil } from "rxjs/operators";
import { scrollbarStyle } from '@assets/scrollbar';
import { RouteHelper } from "@app/helpers/route.helper";
import { NavProxyService } from "@app/services/nav-proxy.service";
import { SharedLoaderService } from "@app/services/shared-loader.service";
import { SharedAlertService } from "@app/services/shared-alert.service";

/**
 * Assigns a mangaging organisation to an organisation
 */
@Component({
    selector: 'app-assign-managing-organisation',
    templateUrl: './assign-managing-organisation.page.html',
    styleUrls: [
        './assign-managing-organisation.page.scss',
        '../../../../assets/css/form-toolbar.scss',
    ],
    styles: [ scrollbarStyle ],
})
export class AssignManagingOrganisationPage extends DetailPage implements OnInit, OnDestroy {

    public organisationItems: Array<SelectableItem> = new Array<SelectableItem>();
    public selectedItem: SelectableItem;
    private contextOrganisationId: string;

    public constructor(
        private organisationApiService: OrganisationApiService,
        public layoutManager: LayoutManagerService,
        private routeHelper: RouteHelper,
        private navProxy: NavProxyService,
        private sharedLoaderService: SharedLoaderService,
        private sharedAlertService: SharedAlertService,
        eventService: EventService,
        elementRef: ElementRef,
        injector: Injector,
    ) {
        super(eventService, elementRef, injector);
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.contextOrganisationId = this.routeHelper.getParam('organisationId');
        this.loadOrganisations();
    }

    public ngOnDestroy(): void {
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    private loadOrganisations(): void {
        this.isLoading = true;
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', this.routeHelper.getContextTenantAlias());
        params.set('eligibleToManageOrganisationId', this.contextOrganisationId);
        this.organisationApiService.getList(params)
            .pipe(
                takeUntil(this.destroyed),
                finalize(() => this.isLoading = false),
            ).subscribe((organisations: Array<OrganisationResourceModel>) => {
                this.organisationItems = organisations
                    .filter((organisation: OrganisationResourceModel) => organisation.id !== this.contextOrganisationId)
                    .map((organisation: OrganisationResourceModel) => {
                        return {
                            id: organisation.id,
                            icon: 'business',
                            iconLibrary: IconLibrary.IonicV4,
                            name: organisation.name,
                            searchableText: `${organisation.name} ${organisation.alias}`,
                        };
                    });
            });
    }

    public onItemSelected(item: SelectableItem): void {
        this.selectedItem = item;
    }

    public async assignButtonClicked(): Promise<void> {
        await this.sharedLoaderService.presentWithDelay();
        this.organisationApiService.setManagingOrganisation(
            this.routeHelper.getParam('organisationId'),
            this.selectedItem.id,
            this.routeHelper.getContextTenantAlias(),
        ).pipe(
            takeUntil(this.destroyed),
            finalize(() => this.sharedLoaderService.dismiss()),
        ).subscribe((resource: OrganisationResourceModel) => {
            this.eventService.getEntityUpdatedSubject('Organisation').next(resource);
            if (resource.managingOrganisationId) {
                this.sharedAlertService.showToast(
                    `This organisation's managing organisation has been set to ${resource.name}`);
            } else {
                this.sharedAlertService.showToast(`This organisation no longer has a managing organisation`);
            }
            this.goBack();
        });

    }

    public goBack(): void {
        this.navProxy.navigateBackOne();
    }
}
