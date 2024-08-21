import {
    ChangeDetectorRef, OnDestroy, Component, Input,
    OnInit, Output, EventEmitter, AfterViewInit,
} from '@angular/core';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { LoadDataService } from '@app/services/load-data.service';
import { QueryRequestHelper, StringHelper } from '@app/helpers';
import { RouteHelper } from '@app/helpers/route.helper';
import { Router } from '@angular/router';
import { SegmentableEntityViewModel } from '@app/viewmodels/segmentable-entity.viewmodel';
import { contentAnimation } from '@assets/animations';
import { scrollbarStyle } from '@assets/scrollbar';
import { EntityListComponent } from '@app/components/entity-list/entity-list.component';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { EventService } from '@app/services/event.service';
import { RepositoryRegistry } from '@app/repositories/repository-registry';
import { Entity } from '@app/models/entity';
import { IncrementalListRepository } from '@app/repositories/incremental-list.repository';
import { IncrementalDataRepository } from '@app/repositories/incremental-data.repository';
import { SharedModalService } from '@app/services/shared-modal.service';
import { AuthenticationService } from '@app/services/authentication.service';
import { IonicHelper } from '@app/helpers/ionic.helper';
import { PortalExtensionsService } from '@app/services/portal-extensions.service';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { PortalPageTriggerResourceModel } from '@app/resource-models/portal-page-trigger.resource-model';
import { Subject } from 'rxjs';
import { PopoverViewComponent } from '@app/components/popover-view/popover-view.component';
import { SharedPopoverService } from '@app/services/shared-popover.service';
import { ActionButtonPopover } from '@app/models/action-button-popover';
import { MapHelper } from '@app/helpers/map.helper';
import { PopoverCommand } from '@app/models/popover-command';
import { PageType } from '@app/models/page-type.enum';
import { AppConfigService } from '@app/services/app-config.service';

/**
 * Export segmented entity list component class
 * Displaying the active segments list.
 */
@Component({
    selector: 'app-segmented-entity-list',
    templateUrl: './segmented-entity-list.component.html',
    animations: [contentAnimation],
    styleUrls: [
        './entity-list.component.scss',
        '../../../assets/css/scrollbar-list.scss',
        '../../../assets/css/scrollbar-segment.css',
    ],
    styles: [
        scrollbarStyle,
        // The following is added because it's not coming in from scrollbar-list.scss above. It may be related to:
        // Https://github.com/angular/angular-cli/issues/6007
        'ion-content { --overflow: hidden!important; overflow: hidden!important; } '
        + 'ion-content ion-list { overflow: auto!important; height: calc(100vh - 103px)!important; }',
    ],
})
export class SegmentedEntityListComponent<EntityViewModelType
    extends SegmentableEntityViewModel, EntityType extends Entity>
    extends EntityListComponent<EntityViewModelType, EntityType> implements OnInit, AfterViewInit, OnDestroy {

    @Input() public segments: Array<string>;
    @Input() public defaultSegment: string;
    @Output() public selectedSegmentChanged: EventEmitter<any> = new EventEmitter();

    public repository: IncrementalDataRepository;
    public activeSegment: string;
    public isClientRole: boolean;
    public isDownloadEnabled: boolean;
    public trigger: PortalPageTriggerResourceModel;
    protected destroyed: Subject<void> = new Subject<void>();
    public portalPageTriggers: Array<PortalPageTriggerResourceModel>;
    protected actions: Array<ActionButtonPopover> = [];

    public constructor(
        protected changeDetectorRef: ChangeDetectorRef,
        protected navProxy: NavProxyService,
        protected loadDataService: LoadDataService,
        protected routeHelper: RouteHelper,
        public layoutManager: LayoutManagerService,
        protected router: Router,
        eventService: EventService,
        repositoryRegistry: RepositoryRegistry,
        protected sharedModalService: SharedModalService,
        protected stringHelper: StringHelper,
        protected authService: AuthenticationService,
        protected portalExtensionService: PortalExtensionsService,
        protected sharedLoaderService: SharedLoaderService,
        protected sharedAlert: SharedAlertService,
        protected sharedPopoverService: SharedPopoverService,
        protected appConfigService: AppConfigService,
    ) {
        super(
            changeDetectorRef,
            navProxy,
            loadDataService,
            routeHelper,
            layoutManager,
            router,
            eventService,
            repositoryRegistry,
            sharedModalService,
            stringHelper,
            authService,
            portalExtensionService,
            sharedPopoverService,
            appConfigService,
        );

        this.isClientRole = this.authService.isAgent();
    }

    protected createRepository(): IncrementalDataRepository {
        return new IncrementalListRepository<EntityViewModelType, EntityType>(
            this.viewModelConstructor,
            this.loadDataService,
            this.listItemNamePlural,
        );
    }

    public ngOnInit(): void {
        this.initialisationStarted = true;
        this.init();
        if (!this.defaultSegment) {
            this.initErrorMessage = "Fatal Error: In SegmentedEntityListComponent, "
                + "when trying to create a repository but did not have a value for defaultSegment.";
            throw this.initErrorMessage;
        }
        this.subscriptions.push(
            this.eventService.filterStatusListChangedSubject$.subscribe((hasStatusListChange: boolean) =>
                this.onFilterStatusListChanged(hasStatusListChange)),
        );
        this.initialised = true;
    }

    public ngOnDestroy(): void {
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
        super.ngOnDestroy();
    }

    protected async preparePortalPageTriggers(): Promise<void> {
        this.portalPageTriggers =
            await this.portalExtensionService.getEnabledPortalPageTriggers(
                this.authService,
                this.entityTypeName,
                PageType.List,
                this.activeSegment || this.defaultSegment,
            );

        if (!this.canShowMore) {
        // Add portal page trigger actions
            this.actions = this.portalExtensionService.getActionButtonPopovers(this.portalPageTriggers);
            this.hasActionsIncludedInMenu = this.actions.filter((x: ActionButtonPopover) => x.includeInMenu).length > 0;
        }

        this.canShowMore = this.canShowMore
            ? this.canShowMore
            : this.actions?.length > 0 && this.hasActionsIncludedInMenu;
        this.initializeActionButtonList();
    }

    protected async executePortalPageTrigger(trigger: PortalPageTriggerResourceModel): Promise<void> {
        let filters: Map<string, string | Array<string>> = await this.getListQueryHttpParams();
        filters.set('tenantId', this.authService.tenantId);
        filters.set('environment', this.environment);
        await this.portalExtensionService.executePortalPageTrigger(
            trigger,
            this.entityTypeName,
            PageType.List,
            this.getActiveSegment(),
            null,
            filters,
        );
    }

    private getActiveSegment(): string {
        return this.activeSegment || this.defaultSegment;
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
                    this.getActiveSegment(),
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

    /** checks that required input values have been passed in the component tag */
    protected checkRequiredInputs(): void {
        super.checkRequiredInputs();
    }

    public ngAfterViewInit(): void {
        if (!this.initErrorMessage) {
            if (this.repository == null) {
                if (this.waitingForNgOnInitCount > 3) {
                    console.log("Waited for 4 seconds for SegmentedEntityComponentList.ngOnInit() to be called, "
                        + "but it didn\'t happen. Giving up.");
                    return;
                }
                this.waitingForNgOnInitCount++;
                setTimeout(() => this.ngAfterViewInit(), 1000);
                return;
            }
            if (this.entityLoaderService == null) {
                throw new Error("Fatal Error: Did not receive an instance of entityLoaderService. "
                    + "Did you pass in a valid property to the component?");
            }
            this.getSelectedId().then((selectedId: string) => {
                this.selectedId = selectedId;
                if (!this.selectedId) {
                    let givenSegment: string = this.routeHelper.getParam('segment') ||
                        this.activeSegment || this.defaultSegment;
                    this.activeSegment = this.getMatchingSegment(givenSegment);
                    if (!this.repository.isDataLoading) {
                        this.load();
                    }
                } else {
                    // Get the segment of the selected item
                    this.getDefaultHttpParams().then((defaultHttpParams: any) => {
                        this.entityLoaderService.getById(this.selectedId, defaultHttpParams).subscribe((model: any) => {
                            let selectedViewModel: any = new this.viewModelConstructor(model);
                            const selectedItemSegment: string = this.getMatchingSegment(selectedViewModel.segment);
                            if (!selectedItemSegment) {
                                this.activeSegment = this.defaultSegment;
                                this.selectedId = null;
                            } else {
                                this.activeSegment = selectedItemSegment;
                            }
                            if (!this.repository.isDataLoading) {
                                this.load();
                            }
                        });
                    });
                }
            });
        }

        IonicHelper.initIonSegmentButtons();
        this.preparePortalPageTriggers();
    }

    private getMatchingSegment(segment: string): string {
        if (segment == null) {
            return null;
        }
        const index: number = this.segments.map((v: string) => v.toLowerCase()).indexOf(segment.toLowerCase());
        if (index == -1) {
            return null;
        }
        return this.segments[index];
    }

    public segmentChanged($event: any): void {
        let segmentName: string = $event.detail.value;
        if (!this.stringHelper.equalsIgnoreCase(this.activeSegment, segmentName)) {
            this.setActiveSegment($event.detail.value);
            this.selectedSegmentChanged.emit($event);
        }
    }

    public setActiveSegment(segmentName: string): void {
        if (!this.stringHelper.equalsIgnoreCase(this.activeSegment, segmentName)) {
            this.activeSegment = segmentName;
            this.repository.pager.currentPage = 1;
            this.load();
            this.preparePortalPageTriggers();
        }
    }
    /**
     * To be called when a new item has been created and should be added to the list.
     * @param string
     */
    public onItemCreated(entity: EntityType): void {
        this.selectedId = entity.id;
        const viewModel: any = new this.viewModelConstructor(entity);
        if (!this.stringHelper.equalsIgnoreCase(this.activeSegment, viewModel.segment)) {
            this.activeSegment = this.getMatchingSegment(viewModel.segment);
            this.load();
        } else {
            this.addNewOrUpdateItem(viewModel);
        }
    }

    public onItemUpdated(entity: EntityType): void {
        this.selectedId = entity.id;
        if (this.viewModelConstructor) {
            const viewModel: any = new this.viewModelConstructor(entity);
            if (viewModel.segment === 'nascent') {
                return;
            }

            if (!this.stringHelper.equalsIgnoreCase(this.activeSegment, viewModel.segment)) {
                this.activeSegment = this.getMatchingSegment(viewModel.segment);
                this.newlyCreatedOrUpdated = viewModel;
                this.newlyCreatedOrUpdated.segment = viewModel.segment;
                this.load();
            } else {
                this.addNewOrUpdateItem(viewModel);
            }
        }
    }

    /**
     * To be called when a filter selection is made so that this 
     * component can determine if there will be Status Filters left
     * @param boolean
     */
    public onFilterStatusListChanged(hasStatusListChange: boolean): void {
        this.hasStatusFilterSelection = hasStatusListChange;
    }

    public userDidSelectFilter(): void {
        super.userDidSelectFilter();
    }

    protected getFilterSelectionQueryParameters(): Map<string, string | Array<string>> {
        return QueryRequestHelper.getFilterQueryParameters(this.filterSelections, this.activeSegment);
    }

    public trackByIndex(index: any, item: any): any {
        return item.id;
    }

    protected updateItemBoundList(): void {
        if (this.newlyCreatedOrUpdated) {
            if (this.stringHelper.equalsIgnoreCase(this.activeSegment, this.newlyCreatedOrUpdated.segment)) {
                this.addNewOrUpdatedItemToBoundList();
            } else {
                this.removeItemFromListInAnotherSegment();
            }
        }
    }

    private removeItemFromListInAnotherSegment(): void {
        this.repository.boundList.forEach((item: any, index: number) => {
            if (this.newlyCreatedOrUpdated.id === item.id) {
                this.repository.boundList.splice(index, 1);
                return;
            }
        });
    }

    protected async prepareListQueryHttpParams(): Promise<Map<string, string | Array<string>>> {
        let params: Map<string, string | Array<string>> = await super.prepareListQueryHttpParams();
        if (!this.hasSearchTerms && !this.hasStatusFilterSelection) {
            MapHelper.add(params, 'status', this.activeSegment || this.defaultSegment);
        }
        return params;
    }
}
