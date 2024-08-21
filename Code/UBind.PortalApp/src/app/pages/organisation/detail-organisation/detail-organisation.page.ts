import {
    AfterViewInit, ChangeDetectorRef, Component, ElementRef,
    Injector, OnDestroy, OnInit, ViewChild,
} from "@angular/core";
import { OrganisationResourceModel } from "@app/resource-models/organisation/organisation.resource-model";
import { RouteHelper } from "@app/helpers/route.helper";
import { DetailPage } from "@app/pages/master-detail/detail.page";
import { OrganisationApiService } from "@app/services/api/organisation-api.service";
import { EventService } from "@app/services/event.service";
import { OrganisationViewModel } from "@app/viewmodels/organisation.viewmodel";
import { contentAnimation } from "@assets/animations";
import { scrollbarStyle } from "@assets/scrollbar";
import { finalize, takeUntil } from "rxjs/operators";
import { LayoutManagerService } from "@app/services/layout-manager.service";
import { Permission, PermissionDataModel } from '@app/helpers/permissions.helper';
import { NavProxyService } from "@app/services/nav-proxy.service";
import { PopoverOrganisationPage } from "../popover-organisation/popover-organisation.page";
import { PopoverController } from "@ionic/angular";
import { SharedAlertService } from "@app/services/shared-alert.service";
import { SharedLoaderService } from "@app/services/shared-loader.service";
import { UserResourceModel } from "@app/resource-models/user/user.resource-model";
import { AppConfigService } from "@app/services/app-config.service";
import { AppConfig } from "@app/models/app-config";
import { SearchComponent } from "@app/components/search/search.component";
import { QueryRequestHelper } from "@app/helpers";
import { MapHelper } from "@app/helpers/map.helper";
import { FilterComponent } from "@app/components/filter/filter.component";
import { FilterSelection, SearchKeywordFilterSelection } from "@app/viewmodels/filter-selection.viewmodel";
import { UserType } from "@app/models/user-type.enum";
import { AuthenticationService } from "@app/services/authentication.service";
import { DetailsListItem } from "@app/models/details-list/details-list-item";
import { Observable, Subject } from "rxjs";
import { SharedModalService } from "@app/services/shared-modal.service";
import { AdditionalPropertyDefinitionApiService } from "@app/services/api/additional-property-definition-api.service";
import { AdditionalPropertyDefinitionContextType } from "@app/models/additional-property-context-type.enum";
import {
    AdditionalPropertyDefinition,
} from "@app/models/additional-property-item-view.model";
import { HttpErrorResponse } from '@angular/common/http';
import { UserApiService } from "@app/services/api/user-api.service";
import { ActivatedRoute, Params } from "@angular/router";
import { AdditionalPropertyDefinitionService } from "@app/services/additional-property-definition.service";
import { EntityType } from "@app/models/entity-type.enum";
import { PortalViewModel } from "@app/viewmodels/portal.viewmodel";
import { PortalDetailResourceModel, PortalResourceModel } from "@app/resource-models/portal/portal.resource-model";
import { PortalApiService } from "@app/services/api/portal-api.service";
import {
    ProductOrganisationSettingResourceModel,
} from "@app/resource-models/product-organisation-setting.resource-model";
import { ProductOrganisationSettingApiService } from "@app/services/api/product-organisation-setting.api.service";
import { PermissionService } from "@app/services/permission.service";
import { PortalPageTriggerResourceModel } from '@app/resource-models/portal-page-trigger.resource-model';
import { PortalExtensionsService } from '@app/services/portal-extensions.service';
import { DkimSettingsApiService } from "@app/services/api/dkim-settings-api.service";
import { DkimSettingsResourceModel } from "@app/resource-models/dkim-settings.resource-model";
import { ActionButtonPopover } from "@app/models/action-button-popover";
import { ActionButton } from "@app/models/action-button";
import { IconLibrary } from "@app/models/icon-library.enum";
import { ActionButtonHelper } from "@app/helpers/action-button.helper";
import { DataTableDefinitionApiService } from "@app/services/api/data-table-definition-api.service";
import { DataTableDefinitionResourceModel } from "@app/resource-models/data-table-definition.resource-model";
import { PopoverCommand } from "@app/models/popover-command";
import { PageType } from "@app/models/page-type.enum";
import { Errors } from "@app/models/errors";
import { SharedPopoverService } from "@app/services/shared-popover.service";
import { AuthenticationMethodApiService } from "@app/services/api/authentication-method-api.service";
import { AuthenticationMethodType } from "@app/models/authentication-method-type.enum";
import { AuthenticationMethodResourceModel } from "@app/resource-models/authentication-method.resource-model";
import {
    instanceOfAdditionalPropertyContextSettingItemModel,
} from "@app/viewmodels/additional-property-definition-context-setting-item.viewmodel";

/**
 * Export detail oraganisation page component class
 * TODO: Write a better class header: displaying of organisation details.
 */
@Component({
    selector: 'app-detail-organisation',
    templateUrl: './detail-organisation.page.html',
    animations: [contentAnimation],
    styleUrls: [
        './detail-organisation.page.scss',
        '../../../../assets/css/scrollbar-detail.css',
    ],
    styles: [scrollbarStyle],
})
export class DetailOrganisationPage extends DetailPage implements OnInit, AfterViewInit, OnDestroy {

    @ViewChild('searchbar', { read: SearchComponent }) public searchbar: any;

    public filterSelections: Array<FilterSelection> = new Array<FilterSelection>();
    public organisation: OrganisationViewModel;
    public users: Array<UserResourceModel>;
    public userParams: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
    public portals: Array<PortalResourceModel>;
    public portalViewModels: Array<PortalViewModel>;
    public segment: string = 'Details';
    public permission: typeof Permission = Permission;
    public detailsListItems: Array<DetailsListItem>;
    public dkimSettings: Array<DkimSettingsResourceModel>;
    public layoutCanShowSplit: boolean = false;
    public additionalPropertyContextType: AdditionalPropertyDefinitionContextType =
        AdditionalPropertyDefinitionContextType.Organisation;
    public additionalPropertyDefinitions: Array<AdditionalPropertyDefinition>;
    public organisationId: string;
    public errorMessage: string;
    public profilePictureBaseUrl: string;
    public searchTerms: Array<string> = [];
    public showSearch: boolean = false;
    public searchPlaceholderText: string;
    public canAddUser: boolean = false;
    private contextTenantAlias: string;
    private canViewAdditionalPropertyValues: boolean = false;
    private userFilterStatuses: Array<string> = ['New', 'Invited', 'Active', 'Deactivated'];
    private portalFilterStatuses: Array<string> = ['Active', 'Disabled'];
    public hasPortalTab: boolean;
    public canGoBack: boolean = true;
    public canViewUsers: boolean = false;
    public productSettings: Array<ProductOrganisationSettingResourceModel>;
    public permissionModel: PermissionDataModel;
    private entityTypes: typeof EntityType = EntityType;
    protected portalPageTriggers: Array<PortalPageTriggerResourceModel>;
    public dkimSettingsStatus: string = '';
    public dataTables: Array<DataTableDefinitionResourceModel>;
    protected hasActionsIncludedInMenu: boolean = false;
    protected actions: Array<ActionButtonPopover> = [];
    public actionButtonList: Array<ActionButton>;
    public flipMoreIcon: boolean = false;
    public iconLibrary: typeof IconLibrary = IconLibrary;
    public pathTenantAlias: string;
    public portalsErrorMessage: string;
    public isLoadingPortals: boolean = true;
    public isLoadingAdditionalProperties: boolean = true;
    public isLoadingDkimSettings: boolean = true;
    public isLoadingDataTables: boolean = true;
    public isLoadingProductSettings: boolean = true;
    public isLoadingAuthenticationMethods: boolean = true;
    public authenticationMethodLocalAccountEnabled: boolean = false;
    public ssoConfigurationsCount: number = 0;
    public title: string = 'Organisation';
    public allowSendingRenewalInvitations: boolean;
    private usersLoaded: boolean | undefined = undefined;

    public constructor(
        public popoverCtrl: PopoverController,
        public navProxy: NavProxyService,
        public layoutManager: LayoutManagerService,
        protected changeDetectorRef: ChangeDetectorRef,
        protected eventService: EventService,
        private authenticationService: AuthenticationService,
        private permissionService: PermissionService,
        public userApiService: UserApiService,
        private portalApiService: PortalApiService,
        private routeHelper: RouteHelper,
        private organisationApiService: OrganisationApiService,
        private sharedAlertService: SharedAlertService,
        private sharedLoaderService: SharedLoaderService,
        private appConfigService: AppConfigService,
        private sharedModalService: SharedModalService,
        private route: ActivatedRoute,
        private productOrganisationSettingApiService: ProductOrganisationSettingApiService,
        elementRef: ElementRef,
        injector: Injector,
        private additionalPropertiesService: AdditionalPropertyDefinitionApiService,
        private additionalPropertyDefinitionService: AdditionalPropertyDefinitionService,
        private portalExtensionService: PortalExtensionsService,
        private dkimSettingsApiService: DkimSettingsApiService,
        private dataTableDefinitionApiService: DataTableDefinitionApiService,
        private sharedPopoverService: SharedPopoverService,
        private authenticationMethodApiService: AuthenticationMethodApiService,
    ) {
        super(eventService, elementRef, injector);
        this.destroyed = new Subject<void>();
        this.onPortalUpdatedReloadPortals();
        this.onOrganisationUpdated();
    }

    public ngOnInit(): void {
        this.route.params.pipe(takeUntil(this.destroyed)).subscribe((params: Params) => {
            this.setDisplayAddButton();
            this.pathTenantAlias = params['tenantAlias'];
        });
    }

    public ngOnDestroy(): void {
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    public async ngAfterViewInit(): Promise<void> {
        this.contextTenantAlias = this.routeHelper.getContextTenantAlias();
        this.canGoBack = this.pathTenantAlias != null;
        this.organisationId = this.routeHelper.getParam('organisationId');
        await this.loadOrganisationDetails();
        this.layoutManager.updateWindowSize();
        this.setDisplayAddButton();
        this.hasPortalTab = this.permissionService.hasViewPortal();
        this.segment = this.routeHelper.getParam('segment') || this.segment;
        this.loadCurrentSegment();
        this.preparePortalExtensions().then(() => this.generatePopoverLinks());
        this.eventService.getEntityUpdatedSubject('Organisation').pipe(takeUntil(this.destroyed))
            .subscribe((updatedOrganisation: OrganisationResourceModel) => {
                if (this.organisationId == updatedOrganisation.id) {
                    this.organisation = new OrganisationViewModel(updatedOrganisation);
                    this.permissionModel = {
                        organisationId: this.organisation.id,
                        ownerUserId: null,
                        customerId: null,
                    };
                    this.organisationId = this.organisation.id;
                    this.initializeDetailsListItems();
                }
            });
    }

    private async preparePortalExtensions(): Promise<void> {
        this.portalPageTriggers =
            await this.portalExtensionService.getEnabledPortalPageTriggers(
                this.authenticationService,
                this.entityTypes.Organisation,
                PageType.Display,
                this.segment);
    }

    private generatePopoverLinks(): void {
        // Add portal page trigger actions
        this.actions = this.portalExtensionService.getActionButtonPopovers(this.portalPageTriggers);
        this.hasActionsIncludedInMenu = this.actions.filter((x: ActionButtonPopover) => x.includeInMenu).length > 0;
        this.initializeActionButtonList();
    }

    protected executePortalPageTrigger(trigger: PortalPageTriggerResourceModel): void {
        this.portalExtensionService.executePortalPageTrigger(
            trigger,
            this.entityTypes.Organisation,
            PageType.Display,
            this.segment,
            this.organisationId);
    }

    public userDidSelectUserItem(user: any): void {
        const segments: Array<string> = this.routeHelper.appendPathSegments(['user', user.id]);
        this.navProxy.navigate(segments);
    }

    public userDidSelectPortalItem(portal: any): void {
        const segments: Array<string> = this.routeHelper.appendPathSegments(['portal', portal.id]);
        this.navProxy.navigate(segments);
    }

    public userDidSelectEdit(): void {
        const segments: Array<string> = this.routeHelper.appendPathSegment('edit');
        this.navProxy.navigate(segments);
    }

    public createNewUser(): void {
        this.additionalPropertyDefinitionService.verifyIfUserIsAllowedToProceed(
            this.contextTenantAlias,
            this.authenticationService.userType,
            AdditionalPropertyDefinitionContextType.Organisation,
            EntityType.User,
            this.organisation.id,
            this.authenticationService.tenantId,
            true,
            this.permissionService.hasPermission(Permission.EditAdditionalPropertyValues),
            () => {
                const pathSegments: Array<string> = this.routeHelper.appendPathSegments(['user', 'create']);
                this.navProxy.navigateForward(pathSegments);
            },
            () => {
                this.sharedAlertService.showWithOk(
                    'You cannot create a new user',
                    'Because users in this context have at least one required additional property,'
                    + ' and you do not have permission to edit additional properties, you are unable '
                    + 'to create new users. For assistance, please speak to your administrator.');
            });
    }

    public onSearchCancel(): void {
        this.showSearch = false;
    }

    public onSearchUsersButtonClicked(): void {
        this.searchPlaceholderText = 'Search users';
        this.showSearch = true;
        this.changeDetectorRef.detectChanges();

        if (this.searchbar) {
            this.searchbar.setFocus();
        }
    }

    public onSearchPortalsButtonClicked(): void {
        this.searchPlaceholderText = 'Search portals';
        this.showSearch = true;
        this.changeDetectorRef.detectChanges();

        if (this.searchbar) {
            this.searchbar.setFocus();
        }
    }

    public async userDidSelectUserFilter(): Promise<void> {
        const addedFilterSelections: Array<FilterSelection> = await this.getUserFilterSelections();
        addedFilterSelections.forEach((addedFilterSelection: FilterSelection) => {
            const index: number = this.filterSelections.findIndex(
                (filterChip: FilterSelection) => filterChip.value == addedFilterSelection.value);
            if (index < 0) {
                this.filterSelections.push(addedFilterSelection);
            }
        });
        this.loadUsers();
    }

    public async userDidSelectPortalFilter(): Promise<void> {
        const addedFilterSelections: Array<FilterSelection> = await this.getPortalFilterSelections();
        addedFilterSelections.forEach((addedFilterSelection: FilterSelection) => {
            const index: number = this.filterSelections.findIndex(
                (filterChip: FilterSelection) => filterChip.value == addedFilterSelection.value);
            if (index < 0) {
                this.filterSelections.push(addedFilterSelection);
            }
        });
        this.loadPortals();
    }

    public segmentChanged($event: CustomEvent): void {
        if ($event.detail.value != this.segment) {
            this.segment = $event.detail.value;
            this.loadCurrentSegment();
            this.preparePortalExtensions().then(() => this.generatePopoverLinks());
            this.filterSelections = new Array<FilterSelection>();
            this.navProxy.updateSegmentQueryStringParameter(
                'segment',
                this.segment != 'Details' ? this.segment : null,
            );
        }
    }

    public goBack(): void {
        const pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.pop();
        let parentEntitySegment: string = pathSegments[pathSegments.length - 3];
        if (parentEntitySegment == 'tenant') {
            pathSegments.pop();
            this.navProxy.navigateBack(pathSegments, true, { queryParams: { segment: 'Organisations' } });
        } else {
            pathSegments.push('list');
            this.navProxy.navigateBack(pathSegments);
        }
    }

    public async presentPopover(event: CustomEvent): Promise<void> {
        this.flipMoreIcon = true;
        const popover: HTMLIonPopoverElement = await this.popoverCtrl.create({
            component: PopoverOrganisationPage,
            componentProps: {
                isDisabled: !this.organisation.isActive,
                isDefault: this.organisation.isDefault,
                actions: this.actions,
                segment: this.segment,
                canAddUser: this.canAddUser && this.usersLoaded != undefined && !this.showSearch,
                canAddPortal: this.portals != null,
            },
            cssClass: 'custom-popover',
            event: event,
        });
        popover.onDidDismiss().then((command: PopoverCommand) => {
            this.flipMoreIcon = false;
            if (!(command && command.data && command.data.action)) {
                return;
            }

            switch (command.data.action.actionName) {
                case 'edit':
                    this.userDidSelectEdit();
                    break;
                case 'defaultOrganisation':
                    this.setOrganisationToDefault();
                    break;
                case 'disableOrganisation':
                    this.disableOrganisation();
                    break;
                case 'enableOrganisation':
                    this.enableOrganisation();
                    break;
                case 'deleteOrganisation':
                    this.deleteOrganisation();
                    break;
                case 'addUser':
                    this.createNewUser();
                    break;
                case 'addPortal':
                    this.createNewPortal();
                    break;
                default:
                    if (command.data.action.portalPageTrigger) {
                        this.portalExtensionService.executePortalPageTrigger(
                            command.data.action.portalPageTrigger,
                            this.entityTypes.Organisation,
                            PageType.Display,
                            this.segment,
                            this.organisationId,
                        );
                    }
                    break;
            }
        });
        await popover.present();
    }

    private async getUserFilterSelections(): Promise<Array<FilterSelection>> {
        return new Promise(async (resolve: any): Promise<any> => {
            const componentProps: any = {
                filterTitle: 'Filter Users',
                statusTitle: 'Status',
                statusList: QueryRequestHelper.constructStringFilters(this.userFilterStatuses, this.filterSelections),
                dateData: QueryRequestHelper.constructDateFilters(this.filterSelections),
            };

            const filterModalDismissAction = (data: any): void => {
                if (data.data) {
                    resolve(data.data);
                }
            };

            await this.sharedModalService.show(
                {
                    component: FilterComponent,
                    cssClass: 'filter-modal',
                    componentProps: componentProps,
                    backdropDismiss: false,
                },
                'Filter Users',
                filterModalDismissAction,
            );
        });
    }

    private async getPortalFilterSelections(): Promise<Array<FilterSelection>> {
        return new Promise(async (resolve: any): Promise<any> => {
            const componentProps: any = {
                filterTitle: 'Filter Portals',
                statusTitle: 'Status',
                statusList: QueryRequestHelper.constructStringFilters(this.portalFilterStatuses, this.filterSelections),
                dateData: QueryRequestHelper.constructDateFilters(this.filterSelections),
            };

            const filterModalDismissAction = (data: any): void => {
                if (data.data) {
                    resolve(data.data);
                }
            };

            await this.sharedModalService.show(
                {
                    component: FilterComponent,
                    cssClass: 'filter-modal',
                    componentProps: componentProps,
                    backdropDismiss: false,
                },
                'Filter Portals',
                filterModalDismissAction,
            );
        });
    }

    public removeFilterSelection(filterSelection: FilterSelection): void {
        if (filterSelection instanceof SearchKeywordFilterSelection) {
            const index: number = this.searchTerms.indexOf(filterSelection.value);
            this.searchTerms.splice(index, 1);
        }
        if (this.segment == 'Users') {
            this.loadUsers();
        } else {
            this.loadPortals();
        }
    }

    public onSearchTerm(term: string): void {
        if (this.filterSelections.findIndex((filterSelection: FilterSelection) => filterSelection.value == term) < 0) {
            this.filterSelections.push(new SearchKeywordFilterSelection(term));
        }

        this.searchTerms.push(term);
        if (this.segment == 'Users') {
            this.loadUsers();
        } else {
            this.loadPortals();
        }
    }

    public createNewPortal(): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.push('portal', 'create');
        this.navProxy.navigate(pathSegments);
    }

    public additionalPropertyContextSettingItemClicked($event: Event): void {
        if (instanceOfAdditionalPropertyContextSettingItemModel($event)) {
            let pathSegments: Array<string> = this.routeHelper.getPathSegmentsAndAppend(
                'additional-property-definition',
                $event.entityType,
            );
            this.navProxy.navigate(pathSegments);
        }
    }

    private getOrganisationId(): string {
        return (this.organisation && this.organisation.id) || this.routeHelper.getParam('organisationId');
    }

    private async disableOrganisation(): Promise<void> {
        this.sharedLoaderService.presentWait().then(() => {
            this.organisationApiService.disable(this.organisation.id, this.contextTenantAlias)
                .pipe(
                    finalize(() => this.sharedLoaderService.dismiss()),
                    takeUntil(this.destroyed),
                )
                .subscribe((organisationResourceModel: OrganisationResourceModel) => {
                    if (organisationResourceModel) { // will be null if we navigate away from the page during loading
                        this.ngAfterViewInit();
                        this.sharedAlertService.showToast(`Organisation ${this.organisation.name} has been disabled`);
                        this.eventService.getEntityUpdatedSubject('Organisation').next(organisationResourceModel);
                    }
                });
        });
    }

    private async setOrganisationToDefault(): Promise<void> {
        this.sharedLoaderService.presentWait().then(() => {
            this.organisationApiService.setToDefault(this.organisation.id, this.contextTenantAlias)
                .pipe(
                    finalize(() => this.sharedLoaderService.dismiss()),
                    takeUntil(this.destroyed),
                )
                .subscribe((organisationResourceModel: OrganisationResourceModel) => {
                    if (organisationResourceModel) { // will be null if we navigate away from the page during loading
                        this.sharedAlertService.showToast(
                            `Organisation ${this.organisation.name} has been set to the default organisation`);
                        this.eventService.getEntityUpdatedSubject('Organisation').next(organisationResourceModel);
                        if (!this.authenticationService.isMasterUser()) {
                            this.appConfigService.appConfigSubject
                                .pipe(takeUntil(this.destroyed))
                                .subscribe((appConfig: AppConfig) => {
                                    let previousOrganisationAlias: string = appConfig.portal.organisationAlias;
                                    let previousTenantAlias: string = appConfig.portal.tenantAlias;
                                    this.authenticationService.logout();
                                    if (previousOrganisationAlias == previousTenantAlias) {
                                        this.navProxy.navigateRoot(
                                            [previousTenantAlias, previousOrganisationAlias, 'path', 'login']);
                                    } else {
                                        this.navProxy.navigateRoot([previousOrganisationAlias, 'path', 'login']);
                                    }
                                    this.sharedLoaderService.dismiss();
                                });
                        }
                    }
                });
        });
    }

    private async enableOrganisation(): Promise<void> {
        this.sharedLoaderService.presentWait().then(() => {
            this.organisationApiService.enable(this.organisation.id, this.contextTenantAlias)
                .pipe(
                    finalize(() => this.sharedLoaderService.dismiss()),
                    takeUntil(this.destroyed),
                )
                .subscribe((organisationResourceModel: OrganisationResourceModel) => {
                    if (organisationResourceModel) { // will be null if we navigate away from the page during loading
                        this.ngAfterViewInit();
                        this.sharedAlertService.showToast(`Organisation ${this.organisation.name} has been enabled`);
                        this.eventService.getEntityUpdatedSubject('Organisation').next(organisationResourceModel);
                    }
                });
        });
    }

    private async deleteOrganisation(): Promise<void> {
        this.sharedLoaderService.presentWait().then(() => {
            this.organisationApiService.delete(this.organisation.id, this.contextTenantAlias)
                .pipe(
                    finalize(() => this.sharedLoaderService.dismiss()),
                    takeUntil(this.destroyed),
                )
                .subscribe((organisationResourceModel: OrganisationResourceModel) => {
                    if (organisationResourceModel) { // will be null if we navigate away from the page during loading
                        this.sharedAlertService.showToast(`Organisation ${this.organisation.name} has been deleted`);
                        this.eventService.organisationStateChanged();
                        this.goBack();
                    }
                });
        });
    }

    public async productOrganisationSettingChanged(
        event: CustomEvent,
        model: ProductOrganisationSettingResourceModel,
    ): Promise<void> {
        if (!model) {
            return;
        }

        this.setProductOrganisationSettingStatus(model, event.detail.checked);
        model.isNewQuotesAllowed = event.detail.checked;
    }

    public async organisationRenewalInvitationSettingChanged(event: any): Promise<void> {
        if (this.allowSendingRenewalInvitations === event.detail.checked) {
            return;
        }
        this.allowSendingRenewalInvitations = event.detail.checked;
        let result: Observable<any> = this.allowSendingRenewalInvitations
            ? this.organisationApiService
                .allowSendingRenewalInvitations(this.organisation.organisation.tenantId, this.organisation.id)
            : this.organisationApiService
                .disallowSendingRenewalInvitations(this.organisation.organisation.tenantId, this.organisation.id);
        await result.toPromise();
    }

    public async dkimSettingItemClicked(event: Event): Promise<void> {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.push('dkim-settings');
        pathSegments.push('list');
        this.navProxy.navigate(pathSegments);
    }

    public async navigateToDataTableDefinition(): Promise<void> {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.push('data-table');
        pathSegments.push('list-detail');
        this.navProxy.navigate(pathSegments);
    }

    private setProductOrganisationSettingStatus(
        model: ProductOrganisationSettingResourceModel,
        status: boolean,
    ): void {
        let result: any = status
            ? this.productOrganisationSettingApiService.enable(
                this.organisation.id, model.productId, this.routeHelper.getContextTenantAlias())
            : this.productOrganisationSettingApiService.disable(
                this.organisation.id, model.productId, this.routeHelper.getContextTenantAlias());
        result.pipe(takeUntil(this.destroyed))
            .subscribe((res: ProductOrganisationSettingResourceModel) => {
                if (res) { // will be null if we navigate away from the page during loading
                    this.sharedAlertService.showToast(
                        `Creating new quotes for ${model.name} has been ${(status ? 'enabled' : 'disabled')}`);
                }
            });
    }

    private loadCurrentSegment(): void {
        switch (this.segment) {
            case 'Settings':
                this.loadSettings();
                break;
            case 'Users':
                this.loadUsers();
                break;
            case 'Portals':
                this.loadPortals();
                break;
            default:
                break;
        }
    }

    private async loadOrganisationDetails(): Promise<void> {
        this.isLoading = true;
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', this.contextTenantAlias);
        try {
            const data: OrganisationResourceModel =
                await this.organisationApiService.getById(this.organisationId, params).toPromise();
            this.organisation = new OrganisationViewModel(data);
            this.title = this.organisation.name;
            this.permissionModel = {
                organisationId: this.organisation.id,
                ownerUserId: null,
                customerId: null,
            };

            this.organisationId = this.organisation.id;
            this.initializeDetailsListItems();
        } catch (error) {
            this.errorMessage = 'There was a problem loading the organisation details';
            throw error;
        } finally {
            this.isLoading = false;
        }
    }

    private async loadSettings(): Promise<void> {
        this.loadProductSettings();
        this.loadAdditionalPropertiesSettings();
        this.loadDkimSettingsStatus();
        this.loadDataTables();
        this.loadAuthenticationMethods();
    }

    private async loadDkimSettingsStatus(): Promise<void> {
        this.isLoadingDkimSettings = true;
        const dkimSettings: Array<DkimSettingsResourceModel> = await this.dkimSettingsApiService
            .getDkimSettingsByOrganisation(this.organisationId, this.contextTenantAlias)
            .pipe(takeUntil(this.destroyed), finalize(() => this.isLoadingDkimSettings = false))
            .toPromise();
        this.dkimSettings = dkimSettings;
        this.dkimSettingsStatus = dkimSettings.length == 1 ?
            `${dkimSettings.length} active domain` : dkimSettings.length > 1 ?
                `${dkimSettings.length} active domains` : 'No active domains';
    }

    private async loadDataTables(): Promise<void> {
        if (this.permissionService.hasPermission(Permission.ViewDataTables)) {
            this.isLoadingDataTables = true;
            const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
            params.set("tenant", this.contextTenantAlias);
            params.set("entityType", EntityType.Organisation);
            params.set("entityId", this.organisationId);
            const dataTables: Array<DataTableDefinitionResourceModel> =
                await this.dataTableDefinitionApiService
                    .getDataTableDefinitions(params)
                    .pipe(takeUntil(this.destroyed), finalize(() => this.isLoadingDataTables = false))
                    .toPromise();
            this.dataTables = dataTables;
        }
    }

    private async loadProductSettings(): Promise<void> {
        this.isLoadingProductSettings = true;
        this.productSettings = await this.productOrganisationSettingApiService
            .getByOrganisation(this.getOrganisationId(), this.contextTenantAlias)
            .pipe(
                takeUntil(this.destroyed),
                finalize(() => this.isLoadingProductSettings = false),
            )
            .toPromise();
        this.allowSendingRenewalInvitations = await this.organisationApiService
            .isSendingRenewalInvitationsAllowed(this.organisation.organisation.tenantId, this.getOrganisationId())
            .toPromise();
    }

    private initializeDetailsListItems(): void {
        this.canViewAdditionalPropertyValues
            = this.permissionService.hasPermission(Permission.ViewAdditionalPropertyValues);
        this.detailsListItems = this.organisation.createDetailsList(
            this.navProxy,
            this.sharedPopoverService,
            this.canViewAdditionalPropertyValues);
    }

    private async loadAdditionalPropertiesSettings(): Promise<void> {
        this.isLoadingAdditionalProperties = true;
        this.additionalPropertiesService
            .getAdditionalPropertyDefinitionsByContextTypeAndContextIdAndParentContextId(
                this.contextTenantAlias,
                AdditionalPropertyDefinitionContextType.Organisation,
                this.organisationId,
                this.contextTenantAlias)
            .toPromise().then((data: Array<AdditionalPropertyDefinition>) => {
                this.additionalPropertyDefinitions = data;
                this.isLoadingAdditionalProperties = false;
            });
    }

    private getDefaultHttpParams(): Map<string, string | Array<string>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (this.pathTenantAlias) {
            params.set('tenant', this.pathTenantAlias);
        }
        return params;
    }

    public getDefaultUserHttpParams(): Map<string, string | Array<string>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', this.routeHelper.getContextTenantAlias());
        if (this.authenticationService.isMasterUser() && !this.routeHelper.hasPathSegment('organisation')) {
            // the user is a master user listing master users
            params.set('userTypes', [UserType.Master]);
            params.set('organisation', this.authenticationService.userOrganisationId);
        } else if (this.authenticationService.isAgent()
            || (this.authenticationService.isMasterUser() && this.routeHelper.hasPathSegment('organisation'))
        ) {
            // the user is a agent user listing agent users, or a master user listing agent users
            params.set('userTypes', [UserType.Client]);
            const contextOrganisationid: string
                = this.routeHelper.getParam('organisationId') || this.authenticationService.userOrganisationId;
            params.set('organisation', contextOrganisationid);
        } else {
            // the user is a a customer, so they shouldn't be listing users
            throw Errors.User.AccessDenied("You are not allowed to list users.");
        }
        return params;
    }

    public raiseUserLoadedEvent(usersLoaded: boolean | undefined): void {
        this.usersLoaded = usersLoaded;
        this.generatePopoverLinks();
    }

    private getDefaultPortalHttpParams(): Map<string, string | Array<string>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('organisation', this.organisationId);
        return params;
    }

    private getFilterSelectionQueryParameters(): Map<string, string | Array<string>> {
        return QueryRequestHelper.getFilterQueryParameters(this.filterSelections);
    }

    private loadUsers(): void {
        let defaultParams: Map<string, string | Array<string>> = this.getDefaultHttpParams();
        let defaultUserParams: Map<string, string | Array<string>> = this.getDefaultUserHttpParams();
        let filterParams: Map<string, string | Array<string>> = this.getFilterSelectionQueryParameters();
        let params: Map<string, string | Array<string>>
            = MapHelper.merge(defaultParams, defaultUserParams, filterParams);
        if (this.searchTerms) {
            params.set('search', this.searchTerms);
        }

        let statusParams: any = QueryRequestHelper.getStatusFilterSelectionQueryParameters(this.filterSelections);
        this.userParams = MapHelper.merge(params, statusParams); // triggers an update on segmented user list
    }

    private async loadPortals(): Promise<void> {
        this.isLoadingPortals = true;
        let defaultParams: Map<string, string | Array<string>> = this.getDefaultHttpParams();
        let defaultPortalParams: Map<string, string | Array<string>> = this.getDefaultPortalHttpParams();
        let filterParams: Map<string, string | Array<string>> = await this.getFilterSelectionQueryParameters();
        let params: Map<string, string | Array<string>>
            = MapHelper.merge(defaultParams, defaultPortalParams, filterParams);
        if (this.searchTerms) {
            params.set('search', this.searchTerms);
        }

        let statusParams: any = QueryRequestHelper.getStatusFilterSelectionQueryParameters(this.filterSelections);
        params = MapHelper.merge(params, statusParams);
        this.portalApiService.getList(params)
            .pipe(
                finalize(() => this.isLoadingPortals = false),
                takeUntil(this.destroyed),
            )
            .subscribe((portals: Array<PortalResourceModel>) => {
                if (portals) { // will be null if we navigate away from the page during loading
                    let portalViewModels: Array<PortalViewModel> = new Array<PortalViewModel>();
                    for (let portal of portals) {
                        portalViewModels.push(new PortalViewModel(portal));
                    }
                    this.portalViewModels = portalViewModels;
                    this.portals = portals;
                    this.generatePopoverLinks();
                }
            }, (err: HttpErrorResponse) => {
                this.portalsErrorMessage = 'There was a problem loading the portals';
            });
    }

    private setDisplayAddButton(): void {
        const userOrganisationId: string = this.authenticationService.userOrganisationId;
        const selectedOrganisationId: string = this.routeHelper.getParam('organisationId');
        if (userOrganisationId && selectedOrganisationId) {
            const sameOrganisation: boolean = selectedOrganisationId == userOrganisationId;
            if (sameOrganisation) {
                this.canAddUser = this.permissionService.hasManageUserPermission();
            } else {
                this.canAddUser = this.permissionService.hasPermission(Permission.ManageOrganisations)
                    && this.permissionService.hasManageUserPermission();
            }
        }
    }

    private initializeActionButtonList(): void {
        let actionButtonList: Array<ActionButton> = [];
        let canShowUserActionButtons: boolean =
            this.segment == 'Users' && this.usersLoaded != undefined && !this.showSearch;
        if (canShowUserActionButtons) {
            actionButtonList.push(ActionButton.createActionButton(
                "Create",
                "plus",
                IconLibrary.AngularMaterial,
                false,
                "Create User",
                true,
                (): void => {
                    return this.createNewUser();
                },
            ));

            actionButtonList.push(ActionButton.createActionButton(
                "Search",
                "search",
                IconLibrary.IonicV4,
                false,
                "Search Users",
                true,
                (): void => {
                    return this.onSearchUsersButtonClicked();
                },
            ));

            actionButtonList.push(ActionButton.createActionButton(
                "Filter",
                "options",
                IconLibrary.IonicV4,
                false,
                "Filter Users",
                true,
                (): Promise<void> => {
                    return this.userDidSelectUserFilter();
                },
            ));
        }

        let canShowPortalActionButtons: boolean = this.segment == 'Portals' && this.portals != null;

        if (canShowPortalActionButtons) {
            actionButtonList.push(ActionButton.createActionButton(
                "Create",
                "plus",
                IconLibrary.AngularMaterial,
                false,
                "Create Portal",
                true,
                (): void => {
                    return this.createNewPortal();
                },
            ));

            actionButtonList.push(ActionButton.createActionButton(
                "Search",
                "search",
                IconLibrary.IonicV4,
                false,
                "Search Portals",
                true,
                (): void => {
                    return this.onSearchPortalsButtonClicked();
                },
            ));

            actionButtonList.push(ActionButton.createActionButton(
                "Filter",
                "options",
                IconLibrary.IonicV4,
                false,
                "Filter Portals",
                true,
                (): Promise<void> => {
                    return this.userDidSelectPortalFilter();
                },
            ));
        }

        if (this.segment == 'Details' && this.permissionService.hasPermission(Permission.ManageOrganisations)) {
            actionButtonList.push(ActionButton.createActionButton(
                "Edit",
                "pencil",
                IconLibrary.AngularMaterial,
                false,
                "Edit Organisation",
                true,
                (): void => {
                    return this.userDidSelectEdit();
                },
            ));
        }

        for (let action of this.actions) {
            if (action.actionButtonLabel) {
                actionButtonList.push(ActionButton.createActionButton(
                    action.actionButtonLabel ? action.actionButtonLabel : action.actionName,
                    action.actionIcon,
                    IconLibrary.IonicV4,
                    action.actionButtonPrimary,
                    action.actionName,
                    action.actionButtonLabel ? true : false,
                    (): Promise<void> => {
                        return this.portalExtensionService.executePortalPageTrigger(
                            action.portalPageTrigger,
                            this.entityTypes.Organisation,
                            PageType.Display,
                            this.segment,
                            this.organisationId);
                    },
                ));
            }
        }

        this.actionButtonList = ActionButtonHelper.sortActionButtons(actionButtonList);
    }

    public onPortalUpdatedReloadPortals(): void {
        this.eventService.getEntityUpdatedSubject('Portal').pipe(takeUntil(this.destroyed))
            .subscribe((resourceModel: PortalDetailResourceModel) => {
                if (resourceModel) { // will be null if we navigate away from the page during loading
                    if (this.portalViewModels) {
                        // find the portal with the same ID in the data list
                        let index: number = this.portalViewModels
                            .findIndex((p: PortalViewModel) => p.id === resourceModel.id);
                        if (index > -1) {
                            let viewModel: PortalViewModel = this.portalViewModels[index];
                            if (viewModel.isDefault != resourceModel.isDefault) {
                                // if the default portal has changed, reload the list
                                this.loadPortals();
                            }
                        }
                    }
                }
            });
    }

    public onOrganisationUpdated(): void {
        this.eventService.getEntityUpdatedSubject('Organisation').pipe(takeUntil(this.destroyed))
            .subscribe((resourceModel: OrganisationResourceModel) => {
                if (resourceModel) { // will be null if we navigate away from the page during loading
                    if (this.organisationId == resourceModel.id) {
                        this.organisation = new OrganisationViewModel(resourceModel);
                        this.initializeDetailsListItems();
                    }
                }
            });
    }

    public authenticationMethodLocalAccountClicked($event: Event): void {
        const urlSegments: Array<string>
            = this.routeHelper.appendPathSegments(['authentication-method', 'local-account']);
        this.navProxy.navigateForward(urlSegments);
    }

    public authenticationMethodLocalAccountToggled($event: CustomEvent): void {
        this.authenticationMethodLocalAccountEnabled = $event.detail.checked;
        if (this.authenticationMethodLocalAccountEnabled) {
            this.organisationApiService
                .enableLocalAccountAuthenticationMethod(this.organisationId, this.routeHelper.getContextTenantAlias())
                .pipe(takeUntil(this.destroyed))
                .subscribe((model: AuthenticationMethodResourceModel) => {
                    if (model) { // will be null if we navigate away from the page during loading
                        this.eventService.getEntityUpdatedSubject('AuthenticationMethod').next(model);
                        this.sharedAlertService.showToast(`The authentication method ${model.name} has been enabled`);
                    }
                });
        } else {
            this.organisationApiService
                .disableLocalAccountAuthenticationMethod(this.organisationId, this.routeHelper.getContextTenantAlias())
                .pipe(takeUntil(this.destroyed))
                .subscribe((model: AuthenticationMethodResourceModel) => {
                    if (model) { // will be null if we navigate away from the page during loading
                        this.eventService.getEntityUpdatedSubject('AuthenticationMethod').next(model);
                        this.sharedAlertService.showToast(`The authentication method ${model.name} has been disabled`);
                    }
                });
        }

    }

    public authenticationMethodSingleSignOnClicked($event: Event): void {
        const urlSegments: Array<string>
            = this.routeHelper.appendPathSegments(['authentication-method', 'sso-configurations', 'list']);
        this.navProxy.navigateForward(urlSegments);
    }

    private async loadAuthenticationMethods(): Promise<void> {
        this.isLoadingAuthenticationMethods = true;
        await this.authenticationMethodApiService
            .getAuthenticationMethods(this.organisationId, this.routeHelper.getContextTenantAlias())
            .pipe(
                takeUntil(this.destroyed),
                finalize(() => this.isLoadingAuthenticationMethods = false))
            .toPromise()
            .then((authenticationMethods: Array<AuthenticationMethodResourceModel>) => {
                if (authenticationMethods) { // will be null if we navigate away from the page during loading
                    // find the local account one to see if it's enabled
                    let localAccountAuthenticationMethod: AuthenticationMethodResourceModel
                        = authenticationMethods.find((value: AuthenticationMethodResourceModel) => {
                            return value.typeName == AuthenticationMethodType.LocalAccount;
                        });
                    this.authenticationMethodLocalAccountEnabled = localAccountAuthenticationMethod
                        ? this.authenticationMethodLocalAccountEnabled = !localAccountAuthenticationMethod.disabled
                        : true;
                    // count the number of non-local account authentication methods
                    this.ssoConfigurationsCount = authenticationMethods
                        .filter((value: AuthenticationMethodResourceModel) => {
                            return value.typeName != AuthenticationMethodType.LocalAccount;
                        }).length;
                }
            });
    }
}
