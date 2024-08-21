import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { AppConfigService } from '@app/services/app-config.service';
import { ProductApiService } from '@app/services/api/product-api.service';
import { FeatureSettingService } from '@app/services/feature-setting.service';
import { ProductFilterComponent } from '@app/components/product-filter/product-filter.component';
import { scrollbarStyle } from '@assets/scrollbar';
import { Permission, StringHelper } from '@app/helpers';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { Subject, Subscription } from 'rxjs';
import { AppConfig } from '@app/models/app-config';
import { EventService } from '@app/services/event.service';
import { finalize, takeUntil } from 'rxjs/operators';
import { ProductResourceModel } from '@app/resource-models/product.resource-model';
import { ProductSelection } from '@app/models/product-selection';
import { ChartWidgetComponent } from '@app/components/chart-widget/chart-widget.component';
import { SharedModalService } from '@app/services/shared-modal.service';
import { MenuItem, NavigationSpecification } from '@app/models/menu-item';
import { MenuItemService } from '@app/services/menu-item.service';
import { FeatureSetting } from '@app/models/feature-setting.enum';
import { PermissionService } from '@app/services/permission.service';
import { AuthenticationService } from '@app/services/authentication.service';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Export home page component class
 * This class manage of dispaying the home page.
 */
@Component({
    selector: 'app-agent-home',
    templateUrl: './agent-home.page.html',
    styleUrls: [
        './agent-home.page.scss',
        '../../../../assets/css/scrollbar-grid.css',
    ],
    styles: [scrollbarStyle],
})
export class AgentHomePage implements OnInit, OnDestroy {
    public tenantId: string = "";
    public tenantName: string = "";
    public organisationName: string;
    public portalTitle: string;
    public hasPortalTitle: boolean = false;
    public tenantProductsFilter: Array<ProductSelection> = new Array<ProductSelection>();
    public filterChips: Array<any> = [];

    @ViewChild(ChartWidgetComponent)
    public chartWidgetComponent: ChartWidgetComponent;

    public showWidget: boolean = true;

    public quoteManagementEnabled: boolean = false;
    public policyManagementEnabled: boolean = false;
    public claimManagementEnabled: boolean = false;

    public menuItems: Array<MenuItem> = [];
    public permission: typeof Permission = Permission;
    public iconLibrary: typeof IconLibrary = IconLibrary;
    private destroyed: Subject<void>;

    public constructor(
        public eventService: EventService,
        private permissionService: PermissionService,
        private productApiService: ProductApiService,
        private navProxy: NavProxyService,
        private appConfigService: AppConfigService,
        private featureSettingService: FeatureSettingService,
        public layoutManager: LayoutManagerService,
        private sharedModalService: SharedModalService,
        private menuItemService: MenuItemService,
        private authenticationService: AuthenticationService,
    ) {
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.appConfigService.appConfigSubject
            .pipe(takeUntil(this.destroyed))
            .subscribe((appConfig: AppConfig) => {
                this.tenantName = appConfig.portal.tenantName;
                this.organisationName = appConfig.portal.organisationName;
                this.portalTitle = appConfig.portal.title;
                this.hasPortalTitle = !StringHelper.isNullOrWhitespace(this.portalTitle);
                this.tenantId = appConfig.portal.tenantId;
            });
        this.eventService.environmentChangedSubject$
            .pipe(takeUntil(this.destroyed))
            .subscribe(() => {
                this.loadComponents();
            });
        this.loadComponents();
    }

    public loadComponents(): void {
        if (!this.authenticationService.isAuthenticated()) {
            return;
        }
        this.menuItems = this.menuItemService.getMenuItems();
        this.loadProducts();
        this.showLoadingData();
    }

    public ngOnDestroy(): void {
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    private async showLoadingData(): Promise<void> {
        const featureSettings: Array<FeatureSetting> = this.featureSettingService.getFeatures();
        featureSettings.forEach((featureSetting: FeatureSetting): void => {
            switch (featureSetting) {
                case FeatureSetting.QuoteManagement:
                    this.quoteManagementEnabled = this.permissionService.hasViewQuotePermission();
                    break;
                case FeatureSetting.PolicyManagement:
                    this.policyManagementEnabled = this.permissionService.hasViewPolicyPermission();
                    break;
                case FeatureSetting.ClaimsManagement:
                    this.claimManagementEnabled = this.permissionService.hasViewClaimPermission();
                    break;
                default:
                // no-op
            }
        });

        if (!this.permissionService.hasViewQuotePermission()
            && !this.permissionService.hasViewPolicyPermission()
            && !this.permissionService.hasViewClaimPermission()
        ) {
            this.showWidget = false;
            return;
        }
    }

    public goto(navigate: NavigationSpecification): void {
        this.navProxy.navigate(navigate.commands, navigate.extras);
    }

    private loadProducts(): any {
        let subscription: Subscription = this.productApiService.getProductsByTenantId(this.tenantId)
            .pipe(finalize(() => subscription.unsubscribe()))
            .subscribe((products: Array<ProductResourceModel>) => {
                this.tenantProductsFilter = products.map((p: ProductResourceModel) =>
                    <ProductSelection>{ id: p.id, name: p.name, alias: p.alias, isChecked: true });
                this.filterChips = [];
            });
    }

    public async toggleProductFilter(): Promise<void> {

        const productFilterDismissAction = (result: any): void => {
            if (result.data !== undefined) {
                this.tenantProductsFilter = result.data;
                const filterChecked: Array<ProductSelection> = this.tenantProductsFilter
                    .filter((p: ProductSelection) => p.isChecked === true);
                this.filterChips = filterChecked.length === this.tenantProductsFilter.length ? [] : filterChecked;
            }
        };

        await this.sharedModalService.show(
            {
                component: ProductFilterComponent,
                cssClass: "filter-modal",
                componentProps: {
                    productsCheckBoxes: this.tenantProductsFilter,
                    backdropDismiss: false,
                },
            },
            'Product filter',
            productFilterDismissAction,
        );
    }

    public filterChipClick(filter: any): void {
        this.filterChips = this.filterChips.filter((e: any) => e !== filter);

        if (this.filterChips.length === 0) {
            this.tenantProductsFilter = this.tenantProductsFilter.map((p: ProductSelection) => {
                p.isChecked = true;
                return p;
            });
        } else {
            this.tenantProductsFilter = this.tenantProductsFilter.map((p: ProductSelection) => {
                if (!this.filterChips.includes(p)) {
                    p.isChecked = false;
                }
                return p;
            });
        }
    }
}
