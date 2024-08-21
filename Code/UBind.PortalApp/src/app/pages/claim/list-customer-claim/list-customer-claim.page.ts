import { Component, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { contentAnimation } from '@assets/animations';
import { ClaimHelper, StringHelper } from '@app/helpers';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { ClaimApiService } from '@app/services/api/claim-api.service';
import { LoadDataService } from '@app/services/load-data.service';
import { scrollbarStyle } from '@assets/scrollbar';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { SegmentedEntityListComponent } from '@app/components/entity-list/segmented-entity-list.component';
import { ClaimResourceModel } from '@app/resource-models/claim.resource-model';
import { CustomerClaimViewModel } from '@app/viewmodels/customer-claim.viewmodel';
import { ListClaimPage } from '../list-claim/list-claim.page';
import { EventService } from '@app/services/event.service';
import { ProductFeatureSettingService } from '@app/services/product-feature-service';
import { ClaimService } from '@app/services/claim.service';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { SortFilterHelper } from '@app/helpers/sort-filter.helper';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Export list customer claim page component class
 * TODO: Write a better class header: list of customers claims.
 */
@Component({
    selector: 'app-list-customer-claim',
    templateUrl: './list-customer-claim.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-segment.css',
    ],
    styles: [scrollbarStyle],
})
export class ListCustomerClaimPage extends ListClaimPage implements OnInit {

    @ViewChild(SegmentedEntityListComponent, { static: true })
    public listComponent: SegmentedEntityListComponent<CustomerClaimViewModel, ClaimResourceModel>;
    public title: string = 'My Claims';
    public segments: Array<string> = ['active', 'inactive'];
    public defaultSegment: string = 'active';
    public filterStatuses: Array<string> = ['Active', 'Inactive'];
    public viewModelConstructor: typeof CustomerClaimViewModel = CustomerClaimViewModel;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        public navProxy: NavProxyService,
        protected changeDetectorRef: ChangeDetectorRef,
        protected route: ActivatedRoute,
        protected router: Router,
        public claimApiService: ClaimApiService,
        protected loadDataService: LoadDataService,
        public layoutManager: LayoutManagerService,
        stringHelper: StringHelper,
        protected eventService: EventService,
        productFeatureService: ProductFeatureSettingService,
        claimService: ClaimService,
        protected sharedAlertService: SharedAlertService,
    ) {
        super(
            navProxy,
            changeDetectorRef,
            route,
            router,
            claimApiService,
            loadDataService,
            layoutManager,
            stringHelper,
            eventService,
            productFeatureService,
            claimService,
            sharedAlertService,
        );
    }

    public onListQueryParamsGenerated(params: Map<string, string | Array<string>>): void {
        const status: string | Array<string> = params.get('status');
        if (!status) {
            params.set('status', this.defaultSegment);
        } else {
            if (status.toString().toLowerCase() == 'active') {
                params.set('status', ClaimHelper.status.Active);
            } else if (status.toString().toLowerCase() == 'inactive') {
                params.set('status', ClaimHelper.status.Inactive);
            }
        }

        const sortBy: any = params.get(SortFilterHelper.sortBy);
        if (sortBy) {
            params = SortFilterHelper.setSortAndFilterByParam(
                params,
                SortFilterHelper.sortBy,
                sortBy,
                this.getSortAndFilters(),
            );
        }

        const dateFilteringPropertyName: any = params.get(SortFilterHelper.dateFilteringPropertyName);
        if (dateFilteringPropertyName) {
            params = SortFilterHelper.setSortAndFilterByParam(
                params,
                SortFilterHelper.dateFilteringPropertyName,
                dateFilteringPropertyName,
                this.getSortAndFilters(),
            );
        }
    }
}
