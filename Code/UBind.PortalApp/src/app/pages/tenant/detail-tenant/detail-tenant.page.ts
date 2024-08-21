import { Component, Injector, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { Subject, Observable } from 'rxjs';
import { PopoverTenantPage } from '../popover-tenant/popover-tenant.page';
import { contentAnimation } from '@assets/animations';
import { scrollbarStyle } from '@assets/scrollbar';
import { EmailTemplateApiService } from '@app/services/api/email-template-api.service';
import { Permission } from '@app/helpers/permissions.helper';
import { ProductResourceModel } from '@app/resource-models/product.resource-model';
import { FeatureSettingResourceModel, SystemAlertResourceModel, EmailTemplateSetting } from '@app/models';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { TenantApiService } from '@app/services/api/tenant-api.service';
import { ProductApiService } from '@app/services/api/product-api.service';
import { SystemAlertApiService } from '@app/services/api/system-alert-api.service';
import { FeatureSettingApiService } from '@app/services/api/feature-setting-api.service';
import { ReportApiService } from '@app/services/api/report-api.service';
import { ReportResourceModel } from '@app/resource-models/report.resource-model';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { ProductViewModel } from '@app/viewmodels/product.viewmodel';
import { finalize, takeUntil, last } from 'rxjs/operators';
import { TenantViewModel } from '@app/viewmodels/tenant.viewmodel';
import { OrganisationViewModel } from '@app/viewmodels/organisation.viewmodel';
import { RouteHelper } from '@app/helpers/route.helper';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { TenantResourceModel } from '@app/resource-models/tenant.resource-model';
import { EventService } from '@app/services/event.service';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { TenantSessionSettingResourceModel } from '@app/resource-models/tenant-session-settings.resource-model';
import { SessionExpiryMode } from '@app/models/session-expiry-mode.enum';
import { EmailTemplateService } from '@app/services/email-template.service';
import { SystemAlertService } from '@app/services/system-alert.service';
import { AdditionalPropertyDefinitionContextType } from '@app/models/additional-property-context-type.enum';
import {
    AdditionalPropertyDefinition,
} from '@app/models/additional-property-item-view.model';
import { SharedPopoverService } from '@app/services/shared-popover.service';
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import { AdditionalPropertyDefinitionApiService } from '@app/services/api/additional-property-definition-api.service';
import { PermissionService } from '@app/services/permission.service';
import { TenantPasswordExpirySettingResourceModel }
    from '@app/resource-models/tenant-password-expiry-settings.resource-model';
import { ActionButton } from '@app/models/action-button';
import { IconLibrary } from '@app/models/icon-library.enum';
import { ActionButtonHelper } from '@app/helpers/action-button.helper';
import { DataTableDefinitionApiService } from '@app/services/api/data-table-definition-api.service';
import { EntityType } from '@app/models/entity-type.enum';
import { DataTableDefinitionResourceModel } from '@app/resource-models/data-table-definition.resource-model';
import { PopoverCommand } from '@app/models/popover-command';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { OrganisationResourceModel } from '@app/resource-models/organisation/organisation.resource-model';
import { OrganisationApiService } from '@app/services/api/organisation-api.service';
import {
    instanceOfAdditionalPropertyContextSettingItemModel,
} from '@app/viewmodels/additional-property-definition-context-setting-item.viewmodel';
import {
    EntityDetailSegmentListComponent,
} from '@app/components/entity-detail-segment-list/entity-detail-segment-list.component';

/**
 * Export detail tenant page component class.
 * This class manage of displaying tenant details.
 */
@Component({
    selector: 'app-detail-tenant',
    templateUrl: './detail-tenant.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-detail.css',
    ],
    styles: [scrollbarStyle],
})
export class DetailTenantPage extends DetailPage implements OnInit, OnDestroy {

    @ViewChild('organisationsList') private organisationsList: EntityDetailSegmentListComponent;
    public tenant: TenantViewModel;
    public currentIndex: number;
    public segment: string = 'Details';

    public selected: any = [];
    public organisations: Array<OrganisationResourceModel>;
    public products: Array<ProductViewModel> = new Array<ProductViewModel>();
    public featureSettings: Array<FeatureSettingResourceModel>;
    public systemAlerts: Array<SystemAlertResourceModel>;
    public systemAlert: SystemAlertResourceModel;
    public sessionSettings: TenantSessionSettingResourceModel;
    public passwordExpirySettings: TenantPasswordExpirySettingResourceModel;
    public permission: typeof Permission = Permission;
    public reports: Array<ReportResourceModel> = [];
    public sessionExpiryMode: any = SessionExpiryMode;
    public additionalPropertyContextType: AdditionalPropertyDefinitionContextType =
        AdditionalPropertyDefinitionContextType.Tenant;
    public additionalPropertyDefinitions: Array<AdditionalPropertyDefinition>;
    private canViewAdditionalPropertyValues: boolean = false;
    public emailTemplateSettings: Array<EmailTemplateSetting>;
    public isLoadingSettings: boolean = true;
    public isLoadingProducts: boolean = true;
    public isLoadingReports: boolean = true;
    public settingsErrorMessage: string;
    public productsErrorMessage: string;
    public reportsErrorMessage: string;
    public detailsListItems: Array<DetailsListItem>;
    public dataTables: Array<DataTableDefinitionResourceModel>;
    public actionButtonList: Array<ActionButton>;
    public canShowMore: boolean = false;
    public flipMoreIcon: boolean = false;
    public iconLibrary: typeof IconLibrary = IconLibrary;
    public organisationTypeViewModel: typeof OrganisationViewModel = OrganisationViewModel;

    private tenantAlias: string;
    public constructor(
        eventService: EventService,
        public navProxy: NavProxyService,
        private routeHelper: RouteHelper,
        private tenantApiService: TenantApiService,
        private sharedAlertService: SharedAlertService,
        private organisationApiService: OrganisationApiService,
        private productApiService: ProductApiService,
        private featureSettingApiService: FeatureSettingApiService,
        private emailTemplateApiService: EmailTemplateApiService,
        private emailTemplateService: EmailTemplateService,
        private systemAlertApiService: SystemAlertApiService,
        private systemAlertService: SystemAlertService,
        private reportApiService: ReportApiService,
        public layoutManager: LayoutManagerService,
        private sharedLoaderService: SharedLoaderService,
        public injector: Injector,
        public elementRef: ElementRef,
        private sharedPopoverService: SharedPopoverService,
        public additionalPropertiesService: AdditionalPropertyDefinitionApiService,
        private permissionService: PermissionService,
        private dataTableDefinitionApiService: DataTableDefinitionApiService,
        private userPath: UserTypePathHelper,
    ) {
        super(eventService, elementRef, injector);
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.eventService.getEntityUpdatedSubject('Tenant')
            .pipe(takeUntil(this.destroyed))
            .subscribe((tenant: TenantResourceModel) => {
                if (tenant.id == this.tenant.id) {
                    this.tenant = new TenantViewModel(tenant);
                    this.initializeDetailsListItems();
                    this.initializeActionButtonList();
                }
            });
        this.segment = this.routeHelper.getParam('segment') || this.segment;
        this.canViewAdditionalPropertyValues
            = this.permissionService.hasPermission(Permission.ViewAdditionalPropertyValues);
        this.loadCurrentSegment();
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    public segmentChanged($event: any): void {
        if ($event.detail.value != this.segment) {
            this.segment = $event.detail.value;
            this.loadCurrentSegment();
            this.navProxy.updateSegmentQueryStringParameter(
                'segment',
                this.segment != 'Details' ? this.segment : null,
            );
        }
    }

    private loadCurrentSegment(): void {
        switch (this.segment) {
            case 'Details':
                this.loadDetails();
                break;
            case 'Settings':
                this.loadSettings();
                break;
            case 'Products':
                this.loadProducts();
                break;
            case 'Organisations':
                this.loadOrganisations();
                break;
            case 'Reports':
                this.loadReports();
                break;
            default:
        }

        this.initializeActionButtonList();
    }

    private async loadDetails(): Promise<void> {
        this.isLoading = true;
        this.tenantAlias = this.routeHelper.getParam('tenantAlias');

        await this.tenantApiService.get(this.tenantAlias)
            .pipe(
                finalize(() => this.isLoading = false),
                takeUntil(this.destroyed),
                last(),
            )
            .toPromise().then(
                (tenant: TenantResourceModel) => {
                    this.tenant = new TenantViewModel(tenant);
                    this.initializeDetailsListItems();
                    this.initializeActionButtonList();
                },
                (err: any) => {
                    // needed to be paired with last() rxjs function, throws error when return is undefined
                    // when destroying or canceling the api request
                    if (err.name != 'EmptyError') {
                        throw err;
                    }
                },
            );
    }

    private async loadOrganisations(): Promise<void> {
        this.isLoading = true;
        this.tenantAlias = this.routeHelper.getParam('tenantAlias');

        await this.tenantApiService.get(this.tenantAlias)
            .pipe(
                finalize(() => this.isLoading = false),
                takeUntil(this.destroyed),
                last(),
            )
            .toPromise().then(
                (tenant: TenantResourceModel) => {
                    this.tenant = new TenantViewModel(tenant);
                    this.organisationsList?.load();
                    this.initializeActionButtonList();
                },
                (err: any) => {
                    // needed to be paired with last() rxjs function, throws error when return is undefined
                    // when destroying or canceling the api request
                    if (err.name != 'EmptyError') {
                        throw err;
                    }
                },
            );
    }

    private initializeDetailsListItems(): void {
        this.detailsListItems = this.tenant.createDetailsList(this.canViewAdditionalPropertyValues);
    }

    private async loadAdditionalProperties(): Promise<void> {
        return this.additionalPropertiesService.getAdditionalPropertyDefinitionsByContextTypeAndContextId(
            this.tenant.id,
            this.additionalPropertyContextType,
            this.tenant.id)
            .pipe(takeUntil(this.destroyed))
            .toPromise()
            .then((resultAdditionalProperties: Array<AdditionalPropertyDefinition>) => {
                this.additionalPropertyDefinitions = resultAdditionalProperties;
            });
    }

    private async loadDataTables(): Promise<void> {
        if (this.permissionService.hasPermission(Permission.ViewDataTables)) {
            const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
            params.set("tenant", this.tenant.id);
            params.set("entityType", EntityType.Tenant);
            params.set("entityId", this.tenant.id);
            const dataTables: Array<DataTableDefinitionResourceModel> =
                await this.dataTableDefinitionApiService
                    .getDataTableDefinitions(params)
                    .toPromise();
            this.dataTables = dataTables;
        }
    }

    public organisationSelected(organisation: OrganisationViewModel): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.push(this.userPath.organisation, organisation.id);
        this.navProxy.navigateForward(pathSegments);
    }

    public reportSelected(report: ReportResourceModel): void {
        this.navProxy.navigate([this.userPath.report, report.id]);
    }

    public additionalPropertyContextSettingItemClicked($event: any): void {
        if (instanceOfAdditionalPropertyContextSettingItemModel($event)) {
            let pathSegments: Array<string> = this.routeHelper.getPathSegmentsAndAppend(
                'additional-property-definition', $event.entityType);
            this.navProxy.navigate(pathSegments);
        }
    }

    public productSelected(product: ProductViewModel): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.push('product', product.alias);
        this.navProxy.navigateForward(pathSegments);
    }

    public async editSessionExpiry(): Promise<void> {
        this.navProxy.navigateForward(['tenant', this.tenant.alias, 'session-settings']);
    }

    public async editPasswordExpiry(): Promise<void> {
        this.navProxy.navigateForward(['tenant', this.tenant.alias, 'password-expiry-settings']);
    }

    public loadCreateSystemAlertPanel(systemAlert: SystemAlertResourceModel): void {
        this.navProxy.navigateForward([
            'tenant',
            this.tenant.alias,
            'system-alert',
            'create'],
        true,
        { queryParams: { typeId: systemAlert.systemAlertType.id } });
    }

    public loadEditSystemAlertPanel(systemAlert: SystemAlertResourceModel): void {
        this.navProxy.navigate(['tenant', this.tenant.alias, 'system-alert', systemAlert.id, 'edit']);
    }

    public async presentPopover(event: any): Promise<void> {
        this.flipMoreIcon = true;
        const popoverDismissAction = (command: PopoverCommand): void => {
            this.flipMoreIcon = false;
            if (command && command.data && command.data.action) {
                const actionName: string = command.data.action.actionName;
                if (actionName === 'edit') {
                    this.userDidTapEditButton();
                } else if (actionName === 'createProduct') {
                    this.userDidTapCreateProductButton();
                } else if (actionName === 'createOrganisation') {
                    this.userDidTapCreateOrganisationButton();
                } else if (actionName == 'enableTenant') {
                    this.enableTenant();
                } else if (actionName == 'disableTenant') {
                    this.disableTenant();
                } else if (actionName === 'deleteTenant') {
                    this.deleteTenant();
                }
            }
        };

        await this.sharedPopoverService.show(
            {
                component: PopoverTenantPage,
                componentProps: {
                    status: this.tenant.disabled,
                    segment: this.segment,
                    canEditTenant: this.canEdit(),
                    canCreateProduct: this.canCreateProduct(),
                    canCreateOrganisation: this.canCreateOrganisation(),
                },
                cssClass: 'custom-popover more-button-top-popover-positioning',
                event: event,
            },
            'Tenant option popover',
            popoverDismissAction,
        );
    }

    public userDidTapCreateProductButton(): void {
        this.navProxy.navigateForward(this.routeHelper.appendPathSegments(['product', 'create']));
    }

    public userDidTapCreateOrganisationButton(): void {
        this.navProxy.navigateForward(this.routeHelper.appendPathSegments(['organisation', 'create']));
    }

    public reportItemClicked(report: any): void {
        this.navProxy.navigate(['report', report.reportId]);
        this.navProxy.navigateForward(this.routeHelper.appendPathSegments(['report', report.reportId]));
    }

    public navigateToDataTableDefinition(): void {
        this.navProxy.navigateForward(this.routeHelper.appendPathSegments(['data-table', 'list-detail']));
    }

    private goBack(): void {
        this.navProxy.navigateBack(['tenant', 'list'], true, { queryParams: { segment: this.tenant.segment } });
    }

    private async disableTenant(): Promise<void> {
        await this.sharedLoaderService.presentWait();
        this.tenant.disabled = true;
        this.tenant.tenant.disabled = true;

        const tenant: TenantResourceModel = await this.tenantApiService.disable(this.tenant.id)
            .pipe(
                finalize(() => this.sharedLoaderService.dismiss()),
                takeUntil(this.destroyed),
            )
            .toPromise();
        this.sharedAlertService.showToast(`The ${tenant.name} tenant has been disabled`);
        this.eventService.getEntityUpdatedSubject('Tenant').next(tenant);
    }

    private async enableTenant(): Promise<void> {
        await this.sharedLoaderService.presentWait();
        this.tenant.disabled = false;
        this.tenant.tenant.disabled = false;

        const tenant: TenantResourceModel = await this.tenantApiService.enable(this.tenant.id)
            .pipe(
                finalize(() => this.sharedLoaderService.dismiss()),
                takeUntil(this.destroyed),
            )
            .toPromise();
        this.sharedAlertService.showToast(`The ${tenant.name} tenant has been enabled`);
        this.eventService.getEntityUpdatedSubject('Tenant').next(tenant);
    }

    private async deleteTenant(): Promise<void> {
        await this.sharedLoaderService.presentWait();
        this.tenant.deleted = true;
        this.tenant.tenant.deleted = true;

        try {
            await this.tenantApiService.delete(this.tenant.id)
                .pipe(
                    finalize(() => this.sharedLoaderService.dismiss()),
                    takeUntil(this.destroyed))
                .toPromise();
            this.eventService.getEntityUpdatedSubject('Tenant').next(this.tenant.tenant);
            this.sharedAlertService.showToast(`The ${this.tenant.name} tenant has been deleted`);
            this.goBack();
        } catch (err) {
            this.tenant.deleted = false;
            throw err;
        }
    }

    public getSegmentOrganisationList(
        params?: Map<string, string | Array<string>>,
    ): Observable<Array<OrganisationResourceModel>> {
        params.set('tenant', this.tenant.id);
        return this.organisationApiService.getList(params);
    }

    private async loadProducts(): Promise<void> {
        if (!this.products || this.products.length == 0) {
            this.isLoadingProducts = true;
        }
        if (!this.tenant) {
            // fill this.tenant with value
            await this.loadDetails();
        }

        // if still not found. dont continue
        if (!this.tenant) {
            return;
        }

        let newProducts: Array<ProductViewModel> = new Array<ProductViewModel>();
        return this.productApiService.getProductsByTenantId(this.tenant.id)
            .pipe(
                finalize(() => this.isLoadingProducts = false),
                takeUntil(this.destroyed),
                last())
            .toPromise().then((products: Array<ProductResourceModel>) => {
                products.forEach((product: ProductResourceModel) => {
                    newProducts.push(new ProductViewModel(product));
                });
                this.products = newProducts;
            },
            (err: any) => {
                // needed to be paired with last() rxjs function, throws error when return
                // is undefined when destroying or canceling the api request
                if (err.name != 'EmptyError') {
                    this.productsErrorMessage = 'There was an error loading the products';
                    throw err;
                }
            });
    }

    private async loadSettings(): Promise<void> {
        if (!this.tenant) {
            await this.loadDetails();
        }
        let featuresPromise: Promise<void> = this.loadTenantFeatureSettings();
        let emailSettingsPromise: Promise<void> = this.loadEmailTemplateSetting();
        let systemAlertsPromise: Promise<void> = this.loadSystemAlerts();
        let sessionSettingsPromise: Promise<void> = this.loadSessionSettings();
        let passwordExpiryPromise: Promise<void> = this.loadPasswordExpirySettings();
        let loadAdditionalProperties: Promise<void> = this.loadAdditionalProperties();
        let loadDataTable: Promise<void> = this.loadDataTables();
        Promise.all([
            featuresPromise,
            emailSettingsPromise,
            systemAlertsPromise,
            sessionSettingsPromise,
            passwordExpiryPromise,
            loadAdditionalProperties,
            loadDataTable])
            .then(
                () => this.isLoadingSettings = false,
                (err: any) => {
                    this.settingsErrorMessage = 'There was an error loading the settings';
                    throw err;
                });
    }

    private async loadTenantFeatureSettings(): Promise<void> {
        return this.featureSettingApiService.getTenantSettings(this.tenant.id)
            .pipe(takeUntil(this.destroyed))
            .toPromise()
            .then((dt: Array<FeatureSettingResourceModel>) => {
                this.featureSettings = dt;
            });
    }

    private async loadEmailTemplateSetting(): Promise<void> {
        return this.emailTemplateApiService.getEmailTemplatesByTenant(this.tenant.id)
            .pipe(takeUntil(this.destroyed))
            .toPromise()
            .then((dt: Array<EmailTemplateSetting>) => {
                this.emailTemplateSettings = dt;
                this.segment = 'Settings';
            });
    }

    private async loadSystemAlerts(): Promise<void> {
        return this.systemAlertApiService.getSystemAlertByTenant(this.tenant.id)
            .pipe(takeUntil(this.destroyed))
            .toPromise()
            .then((dt: any) => {
                this.systemAlerts = dt;
            });
    }

    private async loadSessionSettings(): Promise<void> {
        return this.tenantApiService.getSessionSettings(this.tenant.id)
            .pipe(takeUntil(this.destroyed))
            .toPromise()
            .then((dt: TenantSessionSettingResourceModel) => {
                this.sessionSettings = dt;
            });
    }

    private async loadPasswordExpirySettings(): Promise<void> {
        return this.tenantApiService.getPasswordExpirySettings(this.tenant.id)
            .pipe(takeUntil(this.destroyed))
            .toPromise()
            .then((dt: TenantPasswordExpirySettingResourceModel) => {
                this.passwordExpirySettings = dt;
            });
    }

    private async loadReports(): Promise<void> {
        if (!this.reports || this.reports.length == 0) {
            this.isLoadingReports = true;
        }
        if (!this.tenant) {
            await this.loadDetails();
        }
        return this.reportApiService.getReportsByTenantId(this.tenant.id)
            .pipe(
                finalize(() => this.isLoadingReports = false),
                takeUntil(this.destroyed))
            .toPromise().then(
                (reports: Array<ReportResourceModel>) => {
                    this.reports = reports;
                },
                (err: any) => {
                    this.reportsErrorMessage = 'There was an error loading the reports';
                    throw err;
                });
    }

    public userDidTapEditButton(): void {
        this.navProxy.navigateForward(['tenant', this.tenant ? this.tenant.alias : this.tenantAlias, 'edit']);
    }

    public updateFeatureSetting(event: any, setting: FeatureSettingResourceModel): void {
        const _disabled: boolean = (event.detail.checked ? false : true);

        if (setting.disabled != _disabled) {
            setting.disabled = _disabled;
            this.featureSettingApiService.updateTenantSetting(this.tenant.id, setting.id, setting)
                .pipe(takeUntil(this.destroyed))
                .subscribe(
                    (dt: FeatureSettingResourceModel) => {
                        const action: string = setting.disabled ? 'disabled' : 'enabled';
                        this.sharedAlertService.showToast(
                            `The ${dt.name.toLowerCase()} feature has been ${action}`);
                    },
                    (err: any) => {
                        setting.disabled = !_disabled;
                        throw err;
                    });
        }
    } // end of updating settings

    // START - ALL ABOUT EMAIL TEMPLATE SETTING
    public userDidTapTemplateItem(model: EmailTemplateSetting): void {
        this.navProxy.navigateForward(['tenant', this.tenant.alias, 'email-template', model.id, 'edit']);
    }

    public async userDidChangeEmailTemplateStatus(event: any, emailTemplateId: string): Promise<void> {
        if (!emailTemplateId) {
            return;
        }

        const _disabled: boolean = (event.detail.checked ? false : true);
        if (_disabled) {
            this.emailTemplateService.disableEmailTemplate(this.tenant.id, emailTemplateId, this.destroyed);
        } else {
            this.emailTemplateService.enableEmailTemplate(this.tenant.id, emailTemplateId, this.destroyed);
        }
    }
    // END - ALL ABOUT EMAIL TEMPLATE SETTING

    public updateSystemAlertStatus(event: any, systemAlert: SystemAlertResourceModel): void {
        if (systemAlert.id == null) {
            this.loadCreateSystemAlertPanel(systemAlert);
        } else {
            const isDisabled: boolean = (event.detail.checked ? false : true);
            if (isDisabled) {
                this.systemAlertService.disableSystemAlert(this.tenantAlias, systemAlert, this.destroyed);
            } else {
                this.systemAlertService.enableSystemAlert(this.tenantAlias, systemAlert, this.destroyed);
            }
        }
    }

    public async editSystemAlert(systemAlert: SystemAlertResourceModel): Promise<void> {
        if (systemAlert.id) {
            this.loadEditSystemAlertPanel(systemAlert);
        } else {
            this.loadCreateSystemAlertPanel(systemAlert);
        }
    }

    private initializeActionButtonList(): void {
        let actionButtonList: Array<ActionButton> = [];
        this.canShowMore = this.canEdit() || this.canCreateProduct();

        if (this.canEdit()) {
            actionButtonList.push(ActionButton.createActionButton(
                "Edit",
                "pencil",
                IconLibrary.AngularMaterial,
                false,
                "Edit Tenant",
                true,
                (): void => {
                    return this.userDidTapEditButton();
                }));
        }

        if (this.canCreateProduct()) {
            actionButtonList.push(ActionButton.createActionButton(
                "Create",
                "plus",
                IconLibrary.AngularMaterial,
                false,
                "Create Product",
                true,
                (): void => {
                    return this.userDidTapCreateProductButton();
                }));
        }

        if (this.canCreateOrganisation()) {
            actionButtonList.push(ActionButton.createActionButton(
                "Create",
                "plus",
                IconLibrary.AngularMaterial,
                false,
                "Create Organisation",
                true,
                (): void => {
                    return this.userDidTapCreateOrganisationButton();
                }));
        }

        this.actionButtonList = ActionButtonHelper.sortActionButtons(actionButtonList);
    }

    private canEdit(): boolean {
        return this.segment == 'Details'
            && this.tenant != null
            && this.permissionService.hasPermission(
                Permission.ManageTenants);
    }

    private canCreateProduct(): boolean {
        return this.segment == 'Products'
            && this.tenant != null
            && (!this.tenant.disabled)
            && this.permissionService.hasPermission(
                Permission.ManageProducts);
    }

    private canCreateOrganisation(): boolean {
        return this.segment == 'Organisations'
            && this.tenant != null
            && (!this.tenant.disabled)
            && this.permissionService.hasOneOfPermissions(
                [Permission.ManageOrganisations, Permission.ManageAllOrganisations]);
    }
}
