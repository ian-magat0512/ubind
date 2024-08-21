import { Subject } from 'rxjs';
import { finalize, takeUntil, filter } from 'rxjs/operators';
import { Component, OnInit, ChangeDetectorRef, ViewChild, OnDestroy } from '@angular/core';
import { saveFile, QueryRequestHelper, Permission, UrlHelper, PolicyHelper } from '@app/helpers';
import { PolicyApiService } from '@app/services/api/policy-api.service';
import { LoadDataService } from '@app/services/load-data.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { scrollbarStyle } from '@assets/scrollbar';
import { contentAnimation } from '@assets/animations';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { PolicyViewModel } from '@app/viewmodels/policy.viewmodel';
import { PolicyResourceModel } from '@app/resource-models/policy.resource-model';
import { SegmentedEntityListComponent } from '@app/components/entity-list/segmented-entity-list.component';
import { PolicyStatus } from '@app/models/policy-status.enum';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { SharedPopoverService } from '@app/services/shared-popover.service';
import { AuthenticationService } from '@app/services/authentication.service';
import { AppConfigService } from '@app/services/app-config.service';
import { SortOption } from '@app/components/filter/sort-option';
import { MapHelper } from '@app/helpers/map.helper';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { AppConfig } from '@app/models/app-config';
import { EventService } from '@app/services/event.service';
import { NavigationExtras } from '@angular/router';
import { ProductApiService } from '@app/services/api/product-api.service';
import { ProductBaseEntityListPage } from '@app/pages/product/product-base-entity-list/product-base-entity-list.page';
import { PopoverViewComponent } from '@app/components/popover-view/popover-view.component';
import { EntityType } from '@app/models/entity-type.enum';
import { PortalExtensionsService } from '@app/services/portal-extensions.service';
import { PortalPageTriggerResourceModel } from '@app/resource-models/portal-page-trigger.resource-model';
import { PermissionService } from '@app/services/permission.service';
import { QuoteStepChangedModel } from '@app/models/quote-step-changed.model';
import { QuoteApiService } from '@app/services/api/quote-api.service';
import { QuoteResourceModel } from '@app/resource-models/quote.resource-model';
import { PopoverCommand } from '@app/models/popover-command';
import { ActionButtonPopover } from '@app/models/action-button-popover';
import { IconLibrary } from '@app/models/icon-library.enum';
import { PageType } from '@app/models/page-type.enum';
import { SortFilterHelper } from '@app/helpers/sort-filter.helper';
import { DeploymentEnvironment } from '@app/models/deployment-environment.enum';

/**
 * Export list policy page component class
 * It manage the policy page and exporting of policies.
 */
@Component({
    selector: 'app-list-policy',
    templateUrl: './list-policy.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-segment.css',
    ],
    styles: [scrollbarStyle],
})
export class ListPolicyPage extends ProductBaseEntityListPage implements OnInit, OnDestroy {

    @ViewChild(SegmentedEntityListComponent, { static: true })
    public listComponent: SegmentedEntityListComponent<PolicyViewModel, PolicyResourceModel>;
    private tenantAlias: string;
    public title: string = 'Policies';
    public permission: typeof Permission = Permission;
    public segments: Array<string> = [
        PolicyStatus.Current,
        PolicyStatus.Expired,
        PolicyStatus.Cancelled,
    ];
    public defaultSegment: string = PolicyStatus.Current;
    public filterStatuses: Array<string> = [
        PolicyStatus.Issued,
        PolicyStatus.Active,
        PolicyStatus.Expired,
        PolicyStatus.Cancelled];
    public sortOptions: SortOption = PolicyHelper.sortOptions;
    public filterByDates: Array<string> = PolicyHelper.filterByDates;
    public viewModelConstructor: typeof PolicyViewModel = PolicyViewModel;
    public listItemNamePlural: string = 'policies';

    private entityTypes: typeof EntityType = EntityType;
    protected portalPageTriggers: Array<PortalPageTriggerResourceModel>;
    public actions: Array<ActionButtonPopover> = [];
    public hasActionsIncludedInMenu: boolean = false;
    public flipMoreIcon: boolean = false;
    public iconLibrary: typeof IconLibrary = IconLibrary;
    public environment: DeploymentEnvironment;

    public constructor(
        protected changeDetectorRef: ChangeDetectorRef,
        protected routeHelper: RouteHelper,
        private authService: AuthenticationService,
        public navProxy: NavProxyService,
        public policyApiService: PolicyApiService,
        protected loadDataService: LoadDataService,
        public layoutManager: LayoutManagerService,
        private sharedAlertService: SharedAlertService,
        private sharedPopoverService: SharedPopoverService,
        private appConfigService: AppConfigService,
        private sharedLoaderService: SharedLoaderService,
        public eventService: EventService,
        protected productApiService: ProductApiService,
        private permissionService: PermissionService,
        private portalExtensionService: PortalExtensionsService,
        public quoteApiService: QuoteApiService,
    ) {
        super(productApiService, eventService);

        this.filterStatuses = [PolicyStatus.Issued, PolicyStatus.Active, PolicyStatus.Expired, PolicyStatus.Cancelled];
        this.appConfigService.appConfigSubject.pipe(takeUntil(this.destroyed)).subscribe((appConfig: AppConfig) => {
            this.environment = <DeploymentEnvironment>appConfig.portal.environment;
        });
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.tenantAlias = UrlHelper.getTenantAliasFromUrl();
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.tenantAlias = appConfig.portal.tenantAlias;
            this.tenantId = appConfig.portal.tenantId;
        });
        this.title = this.authService.isMutualTenant() ? 'Protections' : 'Policies';
        this.listItemNamePlural = this.authService.isMutualTenant() ? 'protections' : this.listItemNamePlural;
        this.loadProducts();
        this.handlePolicyStatusChangedEvents();
        this.handleQuoteStepChangedEvents();
        this.preparePortalExtensions().then(() => this.generatePopoverLinks());
    }

    public ngOnDestroy(): void {
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    public async toggleExport(): Promise<void> {
        this.flipMoreIcon = true;
        let filters: Map<string, string | Array<string>> = await this.listComponent.getListQueryHttpParams();
        filters.set('tenantId', this.authService.tenantId);
        filters.set('environment', this.environment);
        const popoverDismissAction = (command: PopoverCommand): void => {
            this.flipMoreIcon = false;
            if (command && command.data && command.data.action) {
                const actionName: string = command.data.action.actionName;
                if (actionName.startsWith("Export")) {
                    this.presentExportPopOver();
                } else if (command.data.action.portalPageTrigger) {
                    this.portalExtensionService.executePortalPageTrigger(
                        command.data.action.portalPageTrigger,
                        this.entityTypes.Policy,
                        PageType.List,
                        this.getActiveSegment(),
                        null,
                        filters,
                    );
                }
            }
        };

        await this.sharedPopoverService.show(
            {
                component: PopoverViewComponent,
                cssClass: 'custom-popover-list more-button-top-popover-positioning',
                componentProps: { actions: this.actions },
                event,
            },
            `List ${this.authService.isMutualTenant() ? 'protection' : 'policy'} option popover`,
            popoverDismissAction,
        );
    }

    public async presentExportPopOver(): Promise<void> {

        await this.sharedAlertService.showWithActionHandler({
            header: 'Select format',
            subHeader: `Which format would you like to export these `
                + `${this.authService.isMutualTenant() ? 'protections' : 'policies'} in?`,
            id: 'export-options-alert-box',
            inputs: [
                {
                    type: 'radio',
                    label: 'CSV',
                    value: 'csv',
                    checked: false,
                },
                {
                    type: 'radio',
                    label: 'JSON',
                    value: 'json',
                    checked: false,
                }],
            buttons: [
                {
                    text: 'Cancel',
                },
                {
                    text: 'Export',
                    handler: async (value: any): Promise<void> => {
                        await this.downloadData(value);
                    },
                }],
        });
    }

    public segmentChanged($event: any): void {
        this.preparePortalExtensions().then(() => this.generatePopoverLinks());
    }

    private async preparePortalExtensions(): Promise<void> {
        this.portalPageTriggers =
            await this.portalExtensionService.getEnabledPortalPageTriggers(
                this.authService,
                this.entityTypes.Policy,
                PageType.List,
                this.getActiveSegment(),
            );
    }

    private generatePopoverLinks(): void {
        this.actions = [];
        if (this.permissionService.hasPermission(this.permission.ExportPolicies)) {
            const exportOption: string = this.authService.isMutualTenant ? "Export policies" : "Export protections";
            this.actions.push({
                actionName: exportOption,
                actionIcon: "download",
                iconLibrary: IconLibrary.IonicV4,
                actionButtonLabel: "",
                actionButtonPrimary: false,
                includeInMenu: true,
            });
        }
        // Add portal page trigger actions
        const actions: Array<ActionButtonPopover>
            = this.portalExtensionService.getActionButtonPopovers(this.portalPageTriggers);
        this.actions = this.actions.concat(actions);
        this.hasActionsIncludedInMenu = this.actions.filter((x: ActionButtonPopover) => x.includeInMenu).length > 0;
    }

    protected async executePortalPageTrigger(trigger: PortalPageTriggerResourceModel): Promise<void> {
        const params: Map<string, string | Array<string>> = await this.listComponent.getListQueryHttpParams(true);
        this.portalExtensionService.executePortalPageTrigger(
            trigger,
            this.entityTypes.Policy,
            PageType.List,
            this.getActiveSegment(),
            null,
            params,
        );
    }

    private handlePolicyStatusChangedEvents(): void {
        this.eventService.getEntityUpdatedSubject('Policy').pipe(takeUntil(this.destroyed))
            .subscribe((policyResourceModel: PolicyResourceModel) => {
                if (policyResourceModel) {
                    this.listComponent.onItemUpdated(policyResourceModel);
                }
            });
    }

    private handleQuoteStepChangedEvents(): void {
        this.eventService.quoteStepChangedSubject$
            .pipe(
                filter((data: QuoteStepChangedModel) => data.quoteId != null),
                takeUntil(this.destroyed),
            )
            .subscribe(
                async (data: QuoteStepChangedModel) => {
                    const quote: QuoteResourceModel =
                        await this.quoteApiService.getById(data.quoteId).toPromise();
                    if (quote.policyId) {
                        const policy: PolicyResourceModel =
                            await this.policyApiService.getById(quote.policyId).toPromise();
                        if (policy) {
                            this.listComponent.onItemUpdated(policy);
                        }
                    }
                },
            );
    }

    private getActiveSegment(): string {
        return this.listComponent.activeSegment || this.defaultSegment;
    }

    public async downloadData(exportType: string): Promise<void> {
        let defaultParams: any = this.listComponent.getDefaultHttpParamsCallback != null ?
            this.listComponent.getDefaultHttpParamsCallback() :
            new Map<string, string | Array<string>>();
        let filterParams: Map<string, string | Array<string>> = QueryRequestHelper.getFilterQueryParameters(
            this.listComponent.filterSelections,
            this.listComponent.activeSegment,
        );
        let params: Map<string, string | Array<string>> = MapHelper.merge(filterParams, defaultParams);
        if (this.listComponent.searchTerms && this.listComponent.searchTerms.length > 0) {
            params.set('search', this.listComponent.searchTerms);
        }

        if (!params.has('status') && this.listComponent.activeSegment) {
            if (this.listComponent.activeSegment == PolicyStatus.Current) {
                params.set('status', [PolicyStatus.Issued, PolicyStatus.Active]);
            } else {
                params.set('status', this.listComponent.activeSegment);
            }
        }

        const d: Date = new Date();
        let month: string = '' + (d.getMonth() + 1);
        let day: string = '' + d.getDate();
        const year: number = d.getFullYear();
        const type: string = this.authService.isMutualTenant() ? 'Protections' : 'Policies';

        if (month.length < 2) {
            month = '0' + month;
        }
        if (day.length < 2) {
            day = '0' + day;
        }

        await this.sharedLoaderService.present('Exporting ' + type.toLowerCase() + ', Please wait...');
        const data: Blob = await this.policyApiService.exportPolicy(exportType, params)
            .pipe(
                finalize(() => this.sharedLoaderService.dismiss()),
                takeUntil(this.destroyed),
            )
            .toPromise();
        const exportFileName: string = `${[year, month, day].join('-')}${' - Exported ' +
            type}.${exportType}`;
        saveFile(data, exportFileName, data.type);
    }

    public getSelectedId(): string {
        return this.routeHelper.getParam('policyId');
    }

    public onListQueryParamsGenerated(params: Map<string, string | Array<string>>): void {
        const status: any = params.get('status');
        if (status && !Array.isArray(status) && status == PolicyStatus.Current) {
            params.set('status', [PolicyStatus.Issued, PolicyStatus.Active]);
        } else if (status && !Array.isArray(status) && status == PolicyStatus.Issued) {
            params.set('status', [PolicyStatus.Issued]);
        }

        const sortBy: any = params.get('sortBy');
        if (sortBy) {
            params = PolicyHelper.setSortAndFilterByParam(params, sortBy, 'sortBy');
        }

        const dateFilteringPropertyName: any = params.get(SortFilterHelper.dateFilteringPropertyName);
        if (dateFilteringPropertyName) {
            params = PolicyHelper.setSortAndFilterByParam(
                params,
                dateFilteringPropertyName,
                SortFilterHelper.dateFilteringPropertyName,
            );
        }
    }

    public getUserFilterChips(): void {

        let dateData: Array<any> = QueryRequestHelper.constructDateFilters(this.listComponent.filterSelections);
        let policyId: string = this.getSelectedId();

        let navigationExtras: NavigationExtras = {
            state: {
                filterTitle: 'Filter & Sort Policies',
                statusTitle: 'Status',
                entityTypeName: this.listComponent.entityTypeName,
                isProductLoading: this.isProductLoading,
                productList: QueryRequestHelper.constructProductFilters(
                    this.filterProducts,
                    this.listComponent.filterSelections,
                ),
                statusList: QueryRequestHelper.constructStringFilters(
                    this.filterStatuses,
                    this.listComponent.filterSelections,
                ),
                filterByDates: this.filterByDates,
                dateIsBefore: dateData['before'],
                dateIsAfter: dateData['after'],
                selectedId: policyId,
                testData: QueryRequestHelper.getTestDataFilter(this.listComponent.filterSelections),
                sortOptions: this.sortOptions,
                selectedSortOption: QueryRequestHelper.constructSortFilters(this.listComponent.filterSelections),
                selectedDateFilteringPropertyName: QueryRequestHelper.constructFilterByDate(
                    this.listComponent.filterSelections,
                ),
            },
        };
        this.navProxy.navigateForward(
            [
                this.listComponent.entityTypeName.toLowerCase(),
                'filter',
            ],
            true,
            navigationExtras,
        );

    }

    protected handleProductsWasLoaded(): void {
        this.handleProductFilterUpdateEvent(this.listComponent.filterSelections);
    }
}
