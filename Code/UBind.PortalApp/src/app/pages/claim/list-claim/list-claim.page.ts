import { Component, ChangeDetectorRef, ViewChild, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Permission, ClaimHelper, StringHelper } from '@app/helpers';
import { contentAnimation } from '@assets/animations';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { ClaimApiService } from '@app/services/api/claim-api.service';
import { LoadDataService } from '@app/services/load-data.service';
import { scrollbarStyle } from '@assets/scrollbar';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { ClaimViewModel } from '@app/viewmodels/claim.viewmodel';
import { ClaimResourceModel } from '@app/resource-models/claim.resource-model';
import { SegmentedEntityListComponent } from '@app/components/entity-list/segmented-entity-list.component';
import { SortOption } from '@app/components/filter/sort-option';
import { EventService } from '@app/services/event.service';
import { ClaimStateChangedModel } from '@app/models/claim-state-changed.model';
import { Subject } from 'rxjs';
import { ProductFeatureSettingService } from '@app/services/product-feature-service';
import { ClaimService } from '@app/services/claim.service';
import { SortFilterHelper } from '@app/helpers/sort-filter.helper';
import { SortAndFilterBy, SortAndFilterByFieldName } from '@app/models/sort-filter-by.enum';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { IconLibrary } from '@app/models/icon-library.enum';
import { ActionButton } from '@app/models/action-button';

/**
 * Export list claim page component class
 * TODO: Write a better class header: list of the claims state.
 */
@Component({
    selector: 'app-list-claim',
    templateUrl: './list-claim.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-segment.css',
    ],
    styles: [scrollbarStyle],
})

export class ListClaimPage implements OnInit, OnDestroy {

    @ViewChild(SegmentedEntityListComponent, { static: true })
    public listComponent: SegmentedEntityListComponent<ClaimViewModel, ClaimResourceModel>;
    public anyProductCanCreateStandaloneClaim: boolean;
    public hasManageClaims: boolean;
    public title: string = 'Claims';
    public permission: typeof Permission = Permission;
    public segments: Array<string> = ClaimHelper.segmentsListLowerCase;
    public defaultSegment: string = ClaimHelper.status.Incomplete;
    public filterStatuses: Array<string> = ClaimHelper.dashboardDisplayableStatuses;
    public sortOptions: SortOption = ClaimHelper.sortOptions;
    public filterByDates: Array<string> = SortFilterHelper.getEntitySortAndFilter();
    public viewModelConstructor: typeof ClaimViewModel = ClaimViewModel;
    private destroyed: Subject<void>;
    public additionalActionButtonList: Array<ActionButton> = [];
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        public navProxy: NavProxyService,
        protected changeDetectorRef: ChangeDetectorRef,
        protected route: ActivatedRoute,
        protected router: Router,
        public claimApiService: ClaimApiService,
        protected loadDataService: LoadDataService,
        public layoutManager: LayoutManagerService,
        private stringHelper: StringHelper,
        protected eventService: EventService,
        private productFeatureService: ProductFeatureSettingService,
        private claimService: ClaimService,
        protected sharedAlertService: SharedAlertService,
    ) {
        this.handleClaimStateChangedEvents();
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.initialiseAdditionalActionButtons();
    }

    public ngOnDestroy(): void {
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    public onListQueryParamsGenerated(params: Map<string, string | Array<string>>): void {
        const status: any = params.get('status');
        if (Array.isArray(status)) {
            const paramsArr: Array<string> = status;
            const newParamsArr: Array<string> = [];
            paramsArr.forEach((x: string) => {
                switch (x) {
                    case ClaimHelper.status.Settlement.toLowerCase():
                        newParamsArr.push(ClaimHelper.status.Approved);
                        break;
                    case ClaimHelper.status.Complete.toLowerCase():
                        newParamsArr.push(ClaimHelper.status.Complete);
                        newParamsArr.push(ClaimHelper.status.Withdrawn);
                        newParamsArr.push(ClaimHelper.status.Declined);
                        break;
                    default:
                        newParamsArr.push(x);
                        break;
                }
            });
            params.set('status', newParamsArr);
        } else {
            if (this.stringHelper.equalsIgnoreCase(status, ClaimHelper.status.Settlement)) {
                params.set('status', ClaimHelper.status.Approved);
            } else if (this.stringHelper.equalsIgnoreCase(status, ClaimHelper.status.Complete)) {
                params.set(
                    'status',
                    [ClaimHelper.status.Complete, ClaimHelper.status.Withdrawn, ClaimHelper.status.Declined],
                );
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

    private handleClaimStateChangedEvents(): void {
        this.eventService.claimStateChangedSubject$.subscribe(
            (data: ClaimStateChangedModel) => {
                this.claimApiService.getById(data.claimId).subscribe((claim: ClaimResourceModel) => {
                    if (data && data.previousClaimState && data.newClaimState) {
                        if ((data.previousClaimState.toLowerCase() === ClaimHelper.status.Nascent.toLowerCase()
                            && data.newClaimState.toLowerCase() !== ClaimHelper.status.Nascent.toLowerCase())) {
                            this.listComponent.onItemCreated(claim);
                        } else {
                            this.listComponent.onItemUpdated(claim);
                        }
                    }
                });
            },
        );
    }

    public async createNewClaim(): Promise<void> {
        let anyProductCanCreateStandaloneClaim: boolean =
            await this.productFeatureService.anyProductCanCreateStandaloneClaim();
        if (!anyProductCanCreateStandaloneClaim) {
            this.sharedAlertService.showWithOk(
                'There are no products supporting standalone claims',
                'There are no products configured to allow claims to be created without an associated policy. '
                + 'If you would like to create a claim, please go to the policy first, '
                + 'and create the claim against that policy. '
                + 'Alternatively, each product can be configured to allow standalone claims, '
                + 'by changing the setting "Must create claims against a policy". '
                + 'If you need assistance, please don\'t hesitate to get in touch with customer support.',
                true,
            );
            return;
        }

        this.claimService.createClaimBySelectingProduct();
    }

    protected getSortAndFilters(): Map<string, string> {
        let sortAndFilter: Map<string, string> = new Map<string, string>();
        sortAndFilter.set(
            SortAndFilterBy.CustomerName,
            SortAndFilterByFieldName.CustomerFullName,
        );
        sortAndFilter.set(
            SortAndFilterBy.ClaimNumber,
            SortAndFilterByFieldName.ClaimNumber,
        );
        sortAndFilter.set(
            SortAndFilterBy.CreatedDate,
            SortAndFilterByFieldName.CreatedDate,
        );
        sortAndFilter.set(
            SortAndFilterBy.LastModifiedDate,
            SortAndFilterByFieldName.LastModifiedDate,
        );
        return sortAndFilter;
    }

    public initialiseAdditionalActionButtons(): void {
        let additionalActionButtonList: Array<ActionButton> = [];
        additionalActionButtonList.push(ActionButton.createActionButton(
            "Create",
            "plus",
            IconLibrary.AngularMaterial,
            true,
            "Create Claim",
            true,
            (): Promise<void> => {
                return this.createNewClaim();
            },
        ));
        this.additionalActionButtonList = additionalActionButtonList;
    }
}
