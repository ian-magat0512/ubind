import { Component, OnInit, ViewChild } from '@angular/core';
import { ReleaseStatus } from '@app/models';
import { ReleaseResourceModel } from '@app/resource-models/release.resource-model';
import { Permission } from '@app/helpers';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { ReleaseApiService } from '@app/services/api/release-api.service';
import { contentAnimation } from '@assets/animations';
import { scrollbarStyle } from '@assets/scrollbar';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { ProductApiService } from '@app/services/api/product-api.service';
import { EntityListComponent } from '@app/components/entity-list/entity-list.component';
import { ReleaseViewModel } from '@app/viewmodels/release.viewmodel';
import { ProductViewModel } from '@app/viewmodels/product.viewmodel';
import { finalize } from 'rxjs/operators';
import { ProductStatus } from '@app/models/product-status.enum';
import { TenantService } from '@app/services/tenant.service';
import { ReplaySubject } from 'rxjs';
import { AppConfigService } from '@app/services/app-config.service';
import { AppConfig } from '@app/models/app-config';
import { ProductResourceModel } from '@app/resource-models/product.resource-model';
import { SortOption } from '@app/components/filter/sort-option';
import { SortDirection } from '@app/viewmodels/sorted-entity.viewmodel';
import { SortFilterHelper } from '@app/helpers/sort-filter.helper';
import { SortAndFilterBy, SortAndFilterByFieldName } from '@app/models/sort-filter-by.enum';
import { IconLibrary } from '@app/models/icon-library.enum';
import { ActionButton } from '@app/models/action-button';

/**
 * Export list release page component class.
 * This class manage displaying the release details in the list.
 */
@Component({
    selector: 'app-list-release',
    templateUrl: './list-release.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-list.scss',
        './list-release.page.scss',
    ],
    styles: [scrollbarStyle],
})
export class ListReleasePage implements OnInit {

    @ViewChild(EntityListComponent, { static: true })
    public listComponent: EntityListComponent<ReleaseViewModel, ReleaseResourceModel>;

    public product: ProductViewModel;
    public title: string = 'Releases';
    public permission: typeof Permission = Permission;
    public viewModelConstructor: any = ReleaseViewModel;

    public productAlias: string = '';
    private tenantAlias: string;
    private tenantId: string;
    public productName: string = '';
    public releases: Array<any> = [];
    public releasesOriginalValue: Array<any> = [];
    public releaseStatus: any = ReleaseStatus;
    public segment: string;
    public selectedId: string;
    public headers: Array<string> = [];
    public errorMessage: Array<Array<string>> = [];
    public searchIsOn: boolean = false;
    public searchStrings: Array<string> = [];
    public filterStrings: any = [];
    public hasStatusFilters: boolean = false;
    public productStatus: any = ProductStatus;
    public sortOptions: SortOption = {
        sortBy: [SortAndFilterBy.ReleaseNumber, SortAndFilterBy.CreatedDate],
        sortOrder: [SortDirection.Descending, SortDirection.Ascending],
    };
    public filterByDates: Array<string> = [SortAndFilterBy.CreatedDate];
    private tenantIdSubject: ReplaySubject<string> = new ReplaySubject<string>(1);
    private productIdSubject: ReplaySubject<string> = new ReplaySubject<string>(1);
    public additionalActionButtonList: Array<ActionButton> = [];
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        private routeHelper: RouteHelper,
        public navProxy: NavProxyService,
        protected tenantService: TenantService,
        public releaseApiService: ReleaseApiService,
        private productApiService: ProductApiService,
        public layoutManager: LayoutManagerService,
        protected appConfigService: AppConfigService,
    ) {
        appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            if (!this.appConfigService.isMasterPortal()) {
                this.tenantId = appConfig.portal.tenantId;
            }
        });
    }

    public ngOnInit(): void {
        this.initialiseAdditionalActionButtons();
        this.productAlias = this.routeHelper.getParam('productAlias');
        this.tenantAlias = this.routeHelper.getParam('tenantAlias');
        this.load();
    }

    private async load(): Promise<void> {
        if (!this.tenantId) {
            this.tenantId = await this.tenantService.getTenantIdFromAlias(this.tenantAlias);
        }
        this.tenantIdSubject.next(this.tenantId);
        let subscription: any = this.productApiService.getByAlias(this.productAlias, this.tenantId)
            .pipe(finalize(() => subscription.unsubscribe()))
            .subscribe((dt: ProductResourceModel) => {
                this.product = new ProductViewModel(dt);
                this.productIdSubject.next(dt.id);
                this.title = 'Releases for ' + this.product.name;
            });

    } // end of fetching releases

    public itemSelected(item: ReleaseViewModel): void {
        const pathSegmentsAfter: Array<string> = this.routeHelper.getPathSegmentsAfter('path');
        if (pathSegmentsAfter[0] === 'product') {
            this.navProxy.navigateForward([
                'product',
                this.productAlias,
                'release',
                item.id]);
        } else {
            this.navProxy.navigateForward([
                'tenant',
                this.tenantAlias,
                'product',
                this.productAlias,
                'release',
                item.id]);
        }
    }

    public async getDefaultHttpParams(): Promise<Map<string, string | Array<string>>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        return new Promise<any>((resolve: any): any => {
            let subscription: any = this.tenantIdSubject
                .pipe(finalize(() => subscription.unsubscribe()))
                .subscribe((tenantId: string) => {
                    params.set('tenant', tenantId);
                    this.productIdSubject.subscribe((productId: string) => {
                        params.set('product', this.product.id);
                        resolve(params);
                    });
                });
        });
    }

    public onClickNewRelease(): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegmentsUpUntil('release');
        pathSegments.push('create');
        this.navProxy.navigateForward(pathSegments);
    }

    public onListQueryParamsGenerated(params: Map<string, string | Array<string>>): void {
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

    private getSortAndFilters(): Map<string, string> {
        let sortAndFilter: Map<string, string> = new Map<string, string>();
        sortAndFilter.set(
            SortAndFilterBy.ReleaseNumber,
            SortAndFilterByFieldName.ReleaseNumber,
        );
        sortAndFilter.set(
            SortAndFilterBy.CreatedDate,
            SortAndFilterByFieldName.CreatedDate,
        );
        return sortAndFilter;
    }

    public initialiseAdditionalActionButtons(): void {
        let additionalActionButtonList: Array<ActionButton> = [];
        additionalActionButtonList.push(ActionButton.createActionButton(
            "Create",
            "plus",
            IconLibrary.AngularMaterial,
            false,
            "Create Release",
            true,
            (): void => {
                return this.onClickNewRelease();
            },
            1,
        ));

        this.additionalActionButtonList = additionalActionButtonList;
    }
}
