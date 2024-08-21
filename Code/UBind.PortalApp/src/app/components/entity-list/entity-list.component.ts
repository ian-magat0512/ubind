import {
    ChangeDetectorRef, ViewChild,
    OnDestroy, Component, Input,
    OnInit, TemplateRef, EventEmitter,
    Output,
    HostListener,
    AfterViewInit,
} from '@angular/core';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { LoadDataService } from '@app/services/load-data.service';
import { QueryRequestHelper, StringHelper } from '@app/helpers';
import { SearchComponent } from '@app/components/search/search.component';
import { IncrementalDataRepository } from '@app/repositories/incremental-data.repository';
import { EntityViewModelConstructor } from '@app/repositories/incremental-list.repository';
import { EntityLoaderService } from '@app/services/entity-loader.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { Router, NavigationExtras } from '@angular/router';
import { EntityViewModel } from '@app/viewmodels/entity.viewmodel';
import { IncrementalListRepository } from '@app/repositories/incremental-list.repository';
import { contentAnimation } from '@assets/animations';
import { scrollbarStyle } from '@assets/scrollbar';
import { AlertInput } from '@ionic/core';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { GroupedEntityViewModel } from '@app/viewmodels/grouped-entity.viewmodel';
import { EventService } from '@app/services/event.service';
import { Subject, SubscriptionLike } from 'rxjs';
import { RepositoryRegistry } from '@app/repositories/repository-registry';
import { Entity } from '@app/models/entity';
import { SortedEntityViewModel, SortDirection } from '@app/viewmodels/sorted-entity.viewmodel';
import { MapHelper } from '@app/helpers/map.helper';
import { FilterSelection, SearchKeywordFilterSelection } from '@app/viewmodels/filter-selection.viewmodel';
import { SortOption } from '../filter/sort-option';
import { SharedModalService } from '@app/services/shared-modal.service';
import { AuthenticationService } from '@app/services/authentication.service';
import { EntityType } from '@app/models/entity-type.enum';
import { PortalExtensionsService } from '@app/services/portal-extensions.service';
import { PortalPageTriggerResourceModel } from '@app/resource-models/portal-page-trigger.resource-model';
import { PopoverViewComponent } from '../popover-view/popover-view.component';
import { SharedPopoverService } from '@app/services/shared-popover.service';
import { SortFilterHelper } from '@app/helpers/sort-filter.helper';
import { DeploymentEnvironment } from '@app/models/deployment-environment.enum';
import { EnvironmentChange } from '@app/models/environment-change';
import { ActionButtonPopover } from '@app/models/action-button-popover';
import { ActionButton } from '@app/models/action-button';
import { IconLibrary } from '@app/models/icon-library.enum';
import { ActionButtonHelper } from '@app/helpers/action-button.helper';
import { PopoverCommand } from '@app/models/popover-command';
import { PageType } from '@app/models/page-type.enum';
import { AppConfigService } from '@app/services/app-config.service';
import { takeUntil } from 'rxjs/operators';
import { AppConfig } from '@app/models/app-config';

/** 
 * Export Entity list component class.
 * This class manage the entity component of the list.
 */
@Component({
    selector: 'app-entity-list',
    templateUrl: './entity-list.component.html',
    animations: [contentAnimation],
    styleUrls: [
        './entity-list.component.scss',
        '../../../assets/css/scrollbar-list.scss',
        '../../../assets/css/scrollbar-segment.css',
    ],
    styles: [
        scrollbarStyle,
        // The following is added because it's not coming in from scrollbar-list.scss above. It may be related to:
        // https://github.com/angular/angular-cli/issues/6007
        'ion-content { --overflow: hidden!important; overflow: hidden!important; } '
        + 'ion-content ion-list { overflow: auto!important; height: calc(100vh - 103px)!important; }',
    ],
})
export class EntityListComponent<EntityViewModelType
    extends EntityViewModel, EntityType extends Entity> implements OnInit, AfterViewInit, OnDestroy {

    @ViewChild('searchbar', { read: SearchComponent }) public searchbar: any;

    protected inputsList: Array<AlertInput> = [];
    public headers: Array<string> = [];
    public repository: IncrementalDataRepository;
    public filterSelections: Array<FilterSelection> = new Array<FilterSelection>();

    public filters: Map<string, string | Array<string>> = new Map();
    public searchTerms: Array<string> = [];
    public showSearch: boolean = false;

    public subscriptions: Array<SubscriptionLike> = new Array<SubscriptionLike>();
    public initErrorMessage: string;
    public newlyCreatedOrUpdated: any;
    public isClientRole: boolean;

    // Used in evaluating wether to display the segment or not depending on the type of filter
    public hasStatusFilterSelection: boolean = false;
    public hasSearchTerms: boolean = false;

    @Input() public title: string;
    @Input() public listItemNamePlural: string;
    @Input() public entityLoaderService: EntityLoaderService<EntityType>;
    @Input() public viewModelConstructor: EntityViewModelConstructor<EntityViewModelType, EntityType>;
    @Input() public itemTemplate: TemplateRef<HTMLElement>;
    @Input() public filterStatuses: Array<string> = [];
    @Input() public sortOptions: SortOption;
    @Input() public filterByDates: Array<string> = [];

    /**
     * A callback which will be called when someone clicks the filter icon.
     * Pass in a function here to be called so that you can load a filter and sort 
     * page for the current entity. If you do not pass anything in for this, the default
     * filter and sort components will be used.
     */
    @Input() public getUserFilterSelectionCallback: () => void;

    /**
     * can be used to set the initally selected item in the list
     */
    @Input() public selectedId: string;
    @Input() public overrideHideSearch: boolean;
    @Input() public overrideHideFunnel: boolean;

    /**
     * This tells the list view what route param identifies the item in the list, e.g. quoteId.
     * When the list view detects this item in the route, it knows that is the selected item,
     * and so it marks it as selected, thus presenting it differently (ie with a highlight)
     */
    @Input() public entityRouteParamName: string;

    /**
     * In the application, sometimes two instances of list views are created, so that
     * resizing of the app doesn't cause a list view to be have to be loaded in the other pane.
     * In order to stop unnecessary double loading of data for these list views, they can share
     * the same repository of data. Once the data is loaded for one list view, it's available 
     * for the other. It does this only when the "key" matches. The repository key is used to 
     * identify the set of data for two list views that show that same data.
     */
    @Input() public repositoryKey: string;

    /** 
     * A function which is called when an item in the list is selected (e.g. clicked)
     */
    @Input() public itemSelectedHandler: (item: EntityViewModel) => void; // function(item)

    /**
     * The path segment which represents this entity that's being listed.
     * This is used when an item is selected, so it knows how to create the path
     * to the detail view for that entity.
     */
    @Input() public entityPathSegment: string;

    /**
     * A callback function which returns the id of the selected item in the list.
     * Normally the selected id can be determined automatically from the entityRouteParamName
     * because the id of the selected entity appears in the url. However if there's something 
     * different about the url structure, or it uses a composite id, then you'll need to pass
     * in a function which retrieves the id of the selected item.
     */
    @Input() public getSelectedIdCallback: () => string;

    /**
     * For the purpose of adding http params to api calls which retrive entities, 
     * pass in a function which returns the http params here.
     * This can be useful if you need to filter by tenantId.
     */
    @Input() public getDefaultHttpParamsCallback: () => Map<string, string | Array<string>>;

    /*
     * Set this to automatically listen for created or updated events, and respond to them.
     */
    @Input() public entityTypeName: string;

    /**
     * An event emitter which emits whenever query params are generated, and emits
     * the generated params. You can subscribe and then change the emitted map.
     */
    @Output() public listQueryParamsGenerated: any = new EventEmitter<Map<string, string | Array<string>>>();

    /** 
     * this is an addtional action button to be displayed on the list.
     */
    @Input() public additionalActionButtonList: Array<ActionButton> = [];

    /**
     * An event callback function for more button which return what popover should show.
     */
    @Input() public getMoreButtonCallback: (event: any) => void;

    /**
     * An indentifier if the more button will show or not.
     */
    @Input() public canShowMore: boolean = false;

    /**
     * An indentifier if the more button icon will flip or not.
     */
    @Input() public flipMoreIcon: boolean = false;

    protected initialisationStarted: boolean = false;
    protected initialised: boolean = false;
    protected waitingForNgOnInitCount: number = 0;
    public portalPageTriggers: Array<PortalPageTriggerResourceModel>;
    protected actions: Array<ActionButtonPopover> = [];
    protected hasActionsIncludedInMenu: boolean = false;
    public actionButtonList: Array<ActionButton>;
    protected destroyed: Subject<void> = new Subject<void>();
    protected environment: DeploymentEnvironment;

    public constructor(
        protected changeDetectorRef: ChangeDetectorRef,
        protected navProxy: NavProxyService,
        protected loadDataService: LoadDataService,
        protected routeHelper: RouteHelper,
        public layoutManager: LayoutManagerService,
        protected router: Router,
        protected eventService: EventService,
        protected repositoryRegistry: RepositoryRegistry,
        protected sharedModalService: SharedModalService,
        protected stringHelper: StringHelper,
        protected authService: AuthenticationService,
        protected portalExtensionsService: PortalExtensionsService,
        protected sharedPopoverService: SharedPopoverService,
        protected appConfigService: AppConfigService,
    ) {
        this.isClientRole = authService.isAgent();
        this.appConfigService.appConfigSubject.pipe(takeUntil(this.destroyed))
            .subscribe((appConfig: AppConfig) => {
                this.environment = <DeploymentEnvironment>appConfig.portal.environment;
            });
    }

    protected createRepository(): IncrementalDataRepository {
        return new IncrementalListRepository<EntityViewModelType, EntityType>(
            this.viewModelConstructor,
            this.loadDataService,
            this.listItemNamePlural,
        );
    }

    public ngOnInit(): void {
        this.init();
        this.initialised = true;
    }

    /** checks that required input values have been passed in the component tag */
    protected checkRequiredInputs(): void {
        if (!this.title) {
            this.initErrorMessage = "When using an entity list, the title was not passed. "
                + "Please ensure you configure the entity list with the [title]=\"xxxxx\" attribute.";
            throw this.initErrorMessage;
        }
        if (!this.entityTypeName) {
            this.initErrorMessage = "When using an entity list, the entityTypeName was not passed. "
                + "Please ensure you configure the entity list with the [entityTypeName]=\"xxxxx\" attribute. "
                + "The entityTypeName should be the name of the entity class/interface that you are listing. "
                + "It's needed for subscribing to creation/update events so that the list can update itself "
                + "when a new item is created or an item is updated.";
            throw this.initErrorMessage;
        }
        if (!this.listItemNamePlural) {
            this.initErrorMessage = "When using an entity list, the listItemNamePlural was not passed. "
                + "Please ensure you configure the entity list with the [listItemNamePlural]=\"xxxxx\" "
                + "attribute, e.g. 'quotes'.  This allows us to print messages such as \"no quotes found\" etc.";
            throw this.initErrorMessage;
        }
        if (!this.entityRouteParamName) {
            this.initErrorMessage = "When using an entity list, the entityRouteParamName was not passed. "
                + "Please ensure you configure the entity list with the [entityRouteParamName]=\"xxxxx\" attribute. "
                + "This allows the list to work out the selected item of the list based upon the route parameter, "
                + "and also to contruct navigation paths to detail items automatically.";
            throw this.initErrorMessage;
        }
        if (!this.entityLoaderService) {
            this.initErrorMessage = "When using an entity list, the entityLoaderService was not passed. "
                + "Please ensure you configure the entity list with the [entityLoaderService]=\"xxxxx\" attribute. "
                + "The entityLoaderService should be an instance of a service which loads the entities "
                + "in this list, e.g. QuoteService";
            throw this.initErrorMessage;
        }
        if (!this.viewModelConstructor) {
            this.initErrorMessage = "When using an entity list, the viewModelConstructor was not passed. "
                + "Please ensure you configure the entity list with the [viewModelConstructor]=\"xxxxx\" attribute. "
                + "The viewModelConstructor is a reference to a function that creates a new view model of the type "
                + "that this list is for. Essentially you can pass in the class of the ViewModel that you are using.";
            throw this.initErrorMessage;
        }
        if (!this.itemTemplate) {
            this.initErrorMessage = "When using an entity list, the itemTemplate was not passed. "
                + "Please ensure you configure the entity list with the [itemTemplate]=\"xxxxx\" attribute. "
                + "The value should be a reference to an ng-template defined in your parent page html file, "
                + "with a hash attribute, e.g. \"#quoteListItemtemplate\". Leave out the hash in "
                + "the attribute param though. The item template is the template which renders each list item.";
            throw this.initErrorMessage;
        }
    }

    @HostListener('window:resize', ['$event'])
    public onResize(event: any): void {
        if (this.layoutManager.isMasterVisible()) {
            const selectedId: string = this.routeHelper.getParam(this.entityRouteParamName) ||
                this.routeHelper.getParam('selectedId');

            this.selectedId = selectedId ? selectedId : null;
        }
        this.updateScrollingParentHeight();
    }

    protected init(): void {
        this.destroyed = new Subject<void>();
        this.checkRequiredInputs();
        this.subscriptions.push(this.eventService.environmentChangedSubject$.subscribe(
            (ec: EnvironmentChange) => this.onEnvironmentChange(ec.newEnvironment),
        ));
        this.repository = this.repositoryRegistry.getOrCreate(
            this.getRepositoryKey(),
            this.createRepository.bind(this),
        );
        if (this.entityTypeName) {
            this.subscriptions.push(
                this.eventService.getEntityCreatedSubject(this.entityTypeName).subscribe(
                    (entity: EntityType) => this.onItemCreated(entity),
                ),
            );
            this.subscriptions.push(
                this.eventService.getEntityUpdatedSubject(this.entityTypeName).subscribe(
                    (entity: EntityType) => this.onItemUpdated(entity),
                ),
            );
            this.subscriptions.push(
                this.eventService.getEntityDeletedSubject(this.entityTypeName).subscribe(
                    (entity: EntityType) => this.onItemDeleted(entity),
                ),
            );
        }
        this.subscriptions.push(this.eventService.getEntityFilterChangedSubject(this.entityTypeName)
            .subscribe((filterSelection: Array<FilterSelection>) => this.onFilterChanged(filterSelection)));
        this.initialised = true;

        this.subscriptions.push(
            this.eventService.getEntityListHeadersUpdatedBehaviorSubject(this.entityTypeName)
                .subscribe((headers: Array<string>) => {
                    this.headers = headers ? headers : new Array<string>();
                }),
        );
    }

    public ngAfterViewInit(): void {
        if (this.repository == null) {
            if (this.waitingForNgOnInitCount > 3) {
                console.log("Waited for 4 seconds for EntityComponentList.ngOnInit() to be called, "
                    + "but it didn\'t happen. Giving up.");
                return;
            }
            this.waitingForNgOnInitCount++;
            setTimeout(() => this.ngAfterViewInit(), 1000);
            return;
        }
        if (!this.initErrorMessage) {
            this.getSelectedId().then((selectedId: string) => {
                this.selectedId = selectedId;
                this.preparePortalPageTriggers();

                if (!this.repository.hasLoaded) {
                    this.load();
                }
            });
        }
    }

    protected async getSelectedId(): Promise<string> {
        if (this.getSelectedIdCallback) {
            return this.getSelectedIdCallback();
        } else {
            return this.routeHelper.getParam(this.entityRouteParamName) || this.routeHelper.getParam('selectedId');
        }
    }

    protected async getDefaultHttpParams(): Promise<Map<string, string | Array<string>>> {
        if (this.getDefaultHttpParamsCallback) {
            return await this.getDefaultHttpParamsCallback();
        }
        return new Map<string, string | Array<string>>();
    }

    public ngOnDestroy(): void {
        this.destroy();
    }

    protected destroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
        if (!this.initialised) {
            if (!this.initialisationStarted) {
                console.log('EntityListComponent.ngOnDestroy() called before ngOnInit was called. Not destroying.');
            } else {
                console.log('EntityListComponent.ngOnDestroy() called before ngOnInit completed. Not destroying.');
            }
            return;
        }
        this.subscriptions.forEach((subscription: SubscriptionLike) => subscription.unsubscribe());
        this.repositoryRegistry.remove(this.getRepositoryKey());
    }

    public async load(): Promise<void> {
        this.repository.isDataLoading = true;
        let params: Map<string, string | Array<string>> = await this.getListQueryHttpParams(true);
        this.repository.populateGrid(
            ((): any => this.entityLoaderService.getList(params)).bind(this),
            ((): void => { }),
            ((): void => this.postPopulateData()).bind(this),
        );
    }

    protected async executePortalPageTrigger(trigger: PortalPageTriggerResourceModel): Promise<void> {
        let filters: Map<string, string | Array<string>> = await this.getListQueryHttpParams(true);
        filters.set('tenantId', this.authService.tenantId);
        filters.set('environment', this.environment);
        this.portalExtensionsService.executePortalPageTrigger(
            trigger,
            this.entityTypeName,
            PageType.List,
            null,
            null,
            filters,
        );
    }

    protected async preparePortalPageTriggers(): Promise<void> {
        this.portalPageTriggers =
            await this.portalExtensionsService.getEnabledPortalPageTriggers(
                this.authService,
                this.entityTypeName,
                PageType.List,
            );

        if (!this.canShowMore) {
            // Add portal page trigger actions
            this.actions = this.portalExtensionsService.getActionButtonPopovers(this.portalPageTriggers);
            this.hasActionsIncludedInMenu = this.actions.filter((x: ActionButtonPopover) => x.includeInMenu).length > 0;
        }

        this.canShowMore = this.canShowMore
            ? this.canShowMore
            : this.actions?.length > 0 && this.hasActionsIncludedInMenu;

        this.initializeActionButtonList();
    }

    public async presentPopover(event: any): Promise<void> {
        this.flipMoreIcon = true;
        let filters: Map<string, string | Array<string>> = await this.getListQueryHttpParams();
        filters.set('tenantId', this.authService.tenantId);
        filters.set('environment', this.environment);
        const popoverDismissAction = (command: PopoverCommand): void => {
            this.flipMoreIcon = false;
            if (!(command && command.data && command.data.action)) {
                // nothing was selected
                return;
            }
            if (command.data.action.portalPageTrigger) {
                this.portalExtensionsService.executePortalPageTrigger(
                    command.data.action.portalPageTrigger,
                    this.entityTypeName,
                    PageType.List,
                    null,
                    null,
                    filters,
                );
            }
        };

        await this.sharedPopoverService.show(
            {
                component: PopoverViewComponent,
                cssClass: 'custom-popover-list more-button-top-popover-positioning',
                componentProps: {
                    actions: this.actions,
                },
                event: event,
            },
            this.entityTypeName + 's option popover',
            popoverDismissAction,
        );
    }

    public async addMoreData(event: any): Promise<void> {
        let params: any = await this.getListQueryHttpParams();
        this.repository.addMoreRows(
            ((): any => this.entityLoaderService.getList(params)).bind(this),
            event,
            ((): void => { }),
            ((): void => this.postPopulateData()).bind(this),
        );
        this.updateScrollingParentHeight();
    }

    private updateScrollingParentHeight(): void {
        const scrollingParentElement: HTMLElement = document.querySelector('.entity-list') as HTMLElement;
        const filterChipElement: HTMLElement = document.querySelector('.filter-chips') as HTMLElement;
        if (filterChipElement) {
            const filterChipOffset: number = filterChipElement.clientHeight + 56;
            scrollingParentElement.style.cssText = "height: calc(100vh - " + filterChipOffset + "px) !important";
        }
    }

    public get searchPlaceholderText(): string {
        return 'Search ' + this.listItemNamePlural;
    }

    protected async prepareListQueryHttpParams(): Promise<Map<string, string | Array<string>>> {
        let defaultParams: Map<string, string | Array<string>> = await this.getDefaultHttpParams();
        let filterParams: Map<string, string | Array<string>> = this.getFilterSelectionQueryParameters();
        let params: Map<string, string | Array<string>> = MapHelper.merge(defaultParams, filterParams);
        if (this.searchTerms?.length > 0) {
            params.set('search', this.searchTerms);
        }

        let statusParams: Map<string, string | Array<string>>
            = QueryRequestHelper.getStatusFilterSelectionQueryParameters(this.filterSelections);
        params = MapHelper.merge(params, statusParams);

        return params;
    }

    public async getListQueryHttpParams(
        firstLoad: boolean = false,
    ): Promise<Map<string, string | Array<string>>> {
        let params: Map<string, string | Array<string>> = await this.prepareListQueryHttpParams();

        // give any subscribers the ability to add to the filtering options (or modify them)
        this.listQueryParamsGenerated.emit(params);

        // get the paging query parameters afterwards (this can only be controlled by the list component)
        let pagingQueryParams: Map<string, string | Array<string>> = this.repository.pager.getPagingQueryParameters();
        if (firstLoad) {
            pagingQueryParams.set('page', '1');
        }

        // merge them
        params = MapHelper.merge(params, pagingQueryParams);
        return params;
    }

    protected getFilterSelectionQueryParameters(): Map<string, string | Array<string>> {
        return QueryRequestHelper.getFilterQueryParameters(this.filterSelections);
    }

    public onSearchCancel(): void {
        this.showSearch = false;
    }

    public onSearchButtonClicked(): void {
        this.showSearch = true;
        this.changeDetectorRef.detectChanges();

        if (this.searchbar) {
            this.searchbar.setFocus();
        }
    }

    public onSearchTerm(term: string): void {

        if (this.filterSelections.findIndex((filterSelection: FilterSelection) => filterSelection.value == term) < 0) {
            this.filterSelections.push(new SearchKeywordFilterSelection(term));
        }

        if (this.searchTerms?.filter((search: string) => search == term).length == 0) {
            this.searchTerms.push(term);
            this.hasSearchTerms = true;
        }

        this.load();
    }

    public toggleReload(): void {
        this.load();
    }

    public itemSelected(item: EntityViewModelType): void {
        let segmentValue: string = this.selectedId = item.id;
        if (this.entityTypeName == "Product" || this.entityTypeName == "Tenant") {
            segmentValue = item["alias"];
        }
        if (this.itemSelectedHandler) {
            this.itemSelectedHandler(item);
        } else if (this.entityPathSegment) {
            let pathSegments: Array<string> = this.routeHelper.getPathSegmentsUpUntil(this.entityPathSegment);
            pathSegments.push(segmentValue);
            this.navProxy.navigateForward(pathSegments);
        } else {
            let pathSegments: Array<string> = this.routeHelper.getModulePathSegments();
            pathSegments.push(segmentValue);
            this.navProxy.navigateForward(pathSegments, true);
        }
    }

    /**
     * To be called when FilterSort page triggers a filter changed event.
     * @param Array<FilterSelection>
     */
    public onFilterChanged(statusFilters: Array<FilterSelection>): void {

        let searchFilters: Array<FilterSelection> = this.filterSelections.filter((filterSelection: FilterSelection) =>
            (filterSelection instanceof SearchKeywordFilterSelection));
        this.filterSelections = searchFilters.concat(statusFilters);

        this.hasStatusFilterSelection = statusFilters.filter((x: FilterSelection) =>
            x.propertyName === "status").length > 0;
        this.eventService.filterStatusListChanged(this.hasStatusFilterSelection);

        this.load();
    }

    public onItemDeleted(entity: EntityType): void {
        this.selectedId = entity.id;
        const oldItem: any = this.repository.boundList.filter((i: any) => i.id == entity.id);

        if (oldItem) {
            this.repository.boundList = this.repository.boundList.filter((i: any) => i.id !== entity.id);
            this.deleteEmptyHeaderAfterUpdatingItem(oldItem[0]);
        }
    }

    public onItemCreated(entity: EntityType): void {
        this.selectedId = entity.id;
        let viewModel: any = new this.viewModelConstructor(entity);
        this.addNewOrUpdateItem(viewModel);
    }

    public onItemUpdated(entity: EntityType): void {
        this.selectedId = entity.id;
        let viewModel: any = new this.viewModelConstructor(entity);
        this.addNewOrUpdateItem(viewModel);
    }

    protected addNewOrUpdateItem(viewModel: EntityViewModelType): void {
        let itemIsExist: boolean = false;
        for (let i: number = 0; i < this.repository.boundList.length; i++) {
            if (this.repository.boundList[i].id === viewModel.id) {
                itemIsExist = true;
                const oldItem: any = this.repository.boundList[i];
                if (viewModel.deleteFromList) {
                    this.repository.boundList.splice(i, 1);
                } else {
                    this.repository.boundList[i] = viewModel;
                    this.addNewHeaderAfterItemCreatedOrUpdated(viewModel);
                }
                this.deleteEmptyHeaderAfterUpdatingItem(oldItem);
            }
        }

        if (!itemIsExist) {
            this.repository.boundList.push(viewModel);
            this.addNewHeaderAfterItemCreatedOrUpdated(viewModel);
            this.sortData(this.repository.boundList);
            this.groupData(this.repository.boundList);
        }
    }

    public userDidSelectFilter(): void {
        if (this.getUserFilterSelectionCallback) {
            return this.getUserFilterSelectionCallback();
        } else {
            let dateData: Array<any> = QueryRequestHelper.constructDateFilters(this.filterSelections);

            let navigationExtras: NavigationExtras = {
                state: {
                    filterTitle: 'Filter & Sort ' + this.listItemNamePlural,
                    statusTitle: "Status",
                    selectedId: this.selectedId,
                    entityTypeName: this.entityTypeName,
                    statusList: QueryRequestHelper.constructStringFilters(
                        this.filterStatuses,
                        this.filterSelections,
                    ),
                    filterByDates: this.filterByDates,
                    dateIsBefore: dateData['before'],
                    dateIsAfter: dateData['after'],
                    testData: QueryRequestHelper.getTestDataFilter(this.filterSelections),
                    sortOptions: this.sortOptions,
                    selectedSortOption: QueryRequestHelper.constructSortFilters(this.filterSelections),
                    selectedFilterByDate: QueryRequestHelper.constructFilterByDate(this.filterSelections),
                },
            };
            let pathSegments: Array<string> = this.routeHelper.getPathSegmentsUpUntil(this.entityPathSegment);
            pathSegments.push('filter');
            this.navProxy.navigateForward(pathSegments, true, navigationExtras);
        }
    }

    public removeFilterSelection(filterSelection: FilterSelection): void {
        if (filterSelection instanceof SearchKeywordFilterSelection) {
            const index: number = this.searchTerms.indexOf(filterSelection.value);
            this.searchTerms.splice(index, 1);
        }

        this.hasStatusFilterSelection = this.filterSelections.filter((x: FilterSelection) =>
            x.propertyName === "status").length > 0;

        this.hasSearchTerms = this.searchTerms.length > 0;

        this.load();
    }

    protected addNewHeaderAfterItemCreatedOrUpdated(item: EntityViewModelType): void {
        if (this.isGroupedEntityViewModel(item)) {
            if (!this.headers.includes(item.groupByValue)) {
                this.headers.unshift(item.groupByValue);
            }
        }
    }

    protected deleteEmptyHeaderAfterUpdatingItem(oldItem: EntityViewModelType): void {
        if (this.isGroupedEntityViewModel(oldItem)) {
            const headerValues: Array<any> = this.repository.boundList.filter((item: any) =>
                item.groupByValue === oldItem.groupByValue);
            if (headerValues.length === 0) {
                const headerIndex: number = this.headers.indexOf(oldItem.groupByValue);
                this.headers.splice(headerIndex, 1);
            }
        }
    }

    /* Executed right after populating the List's datasource.
    */
    protected postPopulateData(): void {
        this.updateItemBoundList();
        if (this.repository.boundList.length > 0) {
            // Sort Data
            this.sortData(this.repository.boundList);
            // Group Data 
            this.groupData(this.repository.boundList);
        }
    }

    protected updateItemBoundList(): void {
        if (this.newlyCreatedOrUpdated) {
            this.addNewOrUpdatedItemToBoundList();
        }
    }

    protected addNewOrUpdatedItemToBoundList(): void {
        const newList: Array<any> = [];
        let isNewlyCreatedItemFound: boolean = false;
        newList.push(this.newlyCreatedOrUpdated);
        this.repository.boundList.forEach((item: any) => {
            if (this.newlyCreatedOrUpdated.id !== item.id) {
                newList.push(item);
            } else {
                isNewlyCreatedItemFound = true;
            }
        });

        if (isNewlyCreatedItemFound) {
            this.newlyCreatedOrUpdated = null;
        }
        this.repository.boundList = newList;
    }

    protected isGroupedEntityViewModel(item: unknown): item is GroupedEntityViewModel {
        return (<GroupedEntityViewModel>item).groupByValue != undefined;
    }

    protected isOrderedEntityViewModel(item: unknown): item is SortedEntityViewModel {
        return (<SortedEntityViewModel>item).sortByValue != undefined;
    }

    // Group Data based on sort options 
    protected groupData(data: Array<EntityViewModel>): void {
        if (this.isGroupedEntityViewModel(this.repository.boundList[0])) {
            let sortBy: FilterSelection = this.filterSelections.filter((item: FilterSelection) =>
                item.propertyName === "sortBy")[0];
            if (sortBy !== undefined) {
                if (sortBy.value.indexOf('Date') > 0) {
                    this.repository.boundList = this.repository.boundList[0].setGroupByValue(
                        this.repository.boundList,
                        sortBy.value,
                    );
                    this.headers = Array.from(new Set(this.repository.boundList.map((item: any) => item.groupByValue)));
                } else {
                    this.headers = [];
                }
            } else {
                this.headers = Array.from(new Set(this.repository.boundList.map((item: any) => item.groupByValue)));
            }
            this.eventService.getEntityListHeadersUpdatedBehaviorSubject(EntityType[this.entityTypeName])
                .next(this.headers);
            this.changeDetectorRef.markForCheck();
        }
    }

    /* Prepare the data before adding it to the List's datasource
    */
    protected sortData(data: Array<EntityViewModel>): void {
        if (this.isOrderedEntityViewModel(data[0])) {
            let sortOption: SortOption = QueryRequestHelper.constructSortFilters(this.filterSelections);
            let orderableData: Array<SortedEntityViewModel> = <Array<SortedEntityViewModel>><any>data;
            orderableData = orderableData[0].setSortOptions(
                orderableData,
                sortOption.sortBy[0],
                SortDirection[sortOption.sortOrder[0]],
            );
            if (orderableData[0].sortDirection === SortDirection.Descending) {
                orderableData.sort((a: SortedEntityViewModel, b: SortedEntityViewModel) =>
                    this.getSortComparatorValue(a.sortByValue) >
                        this.getSortComparatorValue(b.sortByValue) ? -1 : 1);
            } else {
                orderableData.sort((a: SortedEntityViewModel, b: SortedEntityViewModel) =>
                    this.getSortComparatorValue(a.sortByValue) <
                        this.getSortComparatorValue(b.sortByValue) ? -1 : 1);
            }
            this.repository.boundList = orderableData;
        }
    }

    private getSortComparatorValue(val: any): any {
        if (!val) {
            return val;
        }
        return val.toLowerCase();
    }

    public onEnvironmentChange(environment: DeploymentEnvironment): void {
        if (environment !== DeploymentEnvironment.Production) {
            this.removeTestDataFilter();
        }
        this.selectedId = null;
        this.load();
        this.preparePortalPageTriggers();
    }

    private getRepositoryKey(): string {
        return this.repositoryKey
            ? this.repositoryKey
            : (this.viewModelConstructor ?
                EntityListComponent.name + '<' + this.viewModelConstructor.name + '>'
                : '');
    }

    private removeTestDataFilter(): void {
        this.filterSelections = this.filterSelections.filter(
            (selection: FilterSelection) => selection.propertyName !== SortFilterHelper.includeTestData,
        );
    }

    public onMoreButtonClick(event: any): void {
        if (this.getMoreButtonCallback) {
            this.getMoreButtonCallback(event);
        } else {
            this.presentPopover(event);
        }
    }

    public initializeActionButtonList(): void {
        let actionButtonList: Array<ActionButton> = [];
        if (!this.overrideHideSearch) {

            actionButtonList.push(ActionButton.createActionButton(
                "Search",
                "magnify",
                IconLibrary.AngularMaterial,
                false,
                `Search ${this.entityTypeName}`,
                true,
                (): void => {
                    return this.onSearchButtonClicked();
                },
                2,
            ));
        }

        if (!this.overrideHideFunnel) {
            actionButtonList.push(ActionButton.createActionButton(
                "Filter",
                "tune",
                IconLibrary.AngularMaterial,
                false,
                "Filter List",
                true,
                (): void => {
                    return this.userDidSelectFilter();
                },
                3,
            ));
        }

        if (this.isClientRole) {
            actionButtonList.push(ActionButton.createActionButton(
                "Refresh",
                "refresh",
                IconLibrary.AngularMaterial,
                false,
                "Refresh List",
                true,
                (): void => {
                    return this.toggleReload();
                },
                4,
            ));
        }

        for (let portalPageTrigger of this.portalPageTriggers) {
            actionButtonList.push(ActionButton.createActionButton(
                portalPageTrigger.actionButtonLabel
                    ? portalPageTrigger.actionButtonLabel
                    : portalPageTrigger.actionName,
                portalPageTrigger.actionIcon,
                portalPageTrigger.actionIconLibrary,
                portalPageTrigger.actionButtonPrimary,
                portalPageTrigger.actionName,
                portalPageTrigger.actionButtonLabel ? true : false,
                (): Promise<void> => this.executePortalPageTrigger(portalPageTrigger),
            ));
        }

        if (this.additionalActionButtonList?.length > 0) {
            actionButtonList = actionButtonList.concat(this.additionalActionButtonList);
        }

        this.actionButtonList = ActionButtonHelper.sortActionButtons(actionButtonList);
    }
}
