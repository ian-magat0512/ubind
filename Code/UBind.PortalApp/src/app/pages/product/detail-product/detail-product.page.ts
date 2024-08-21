import { Component, Injector, ElementRef, OnDestroy, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { interval, Subject, Observable, of } from 'rxjs';
import { EmailTemplateApiService } from '@app/services/api/email-template-api.service';
import { StringHelper } from '@app/helpers';
import { NavProxyService } from '@app/services/nav-proxy.service';
import {
    ReleaseStatus,
    ConfigurationFileResourceModel,
    SystemAlertResourceModel,
    EmailTemplateSetting,
    DeploymentResourceModel,
} from '@app/models';
import { DeploymentEnvironment } from '@app/models/deployment-environment.enum';
import { ReleaseResourceModel } from '@app/resource-models/release.resource-model';
import { ProductApiService } from '@app/services/api/product-api.service';
import { DeploymentApiService } from '@app/services/api/deployment-api.service';
import { NumberPoolApiService } from '@app/services/api/number-pool-api.service';
import { SystemAlertApiService } from '@app/services/api/system-alert-api.service';
import { PopoverProductAction, PopoverProductPage } from '../popover-product/popover-product.page';
import { scrollbarStyle } from '@assets/scrollbar';
import { Permission } from '@app/helpers/permissions.helper';
import { contentAnimation } from '@assets/animations';
import { ProductDeploymentSettingResourceModel } from '@app/resource-models/product-deployment-setting.resource-model';
import { ProductDeploymentSettingApiService } from '@app/services/api/product-deployment-setting.api.service';
import { ProblemDetails } from '@app/models/problem-details';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { ProductViewModel } from '@app/viewmodels/product.viewmodel';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { EventService } from '@app/services/event.service';
import {
    ProductResourceModel, ProductUpdateRequestResourceModel,
} from '@app/resource-models/product.resource-model';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { finalize, takeUntil, last } from 'rxjs/operators';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { ProductStatus } from '@app/models/product-status.enum';
import { ProductAssetSyncResultModel } from '@app/models/product-asset-sync-result.model';
import { NumberPool } from '@app/models/number-pool.enum';
import { NumberPoolGetResultModel } from '@app/models/number-pool-result.model';
import { AppConfigService } from '@app/services/app-config.service';
import { AppConfig } from '@app/models/app-config';
import { EmailTemplateService } from '@app/services/email-template.service';
import { SystemAlertService } from '@app/services/system-alert.service';
import { SharedPopoverService } from '@app/services/shared-popover.service';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { AdditionalPropertyDefinitionContextType } from '@app/models/additional-property-context-type.enum';
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import { AdditionalPropertyDefinitionApiService } from '@app/services/api/additional-property-definition-api.service';
import {
    AdditionalPropertyDefinition,
} from '@app/models/additional-property-item-view.model';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { ProductFeatureSetting } from '@app/models/product-feature-setting';
import { ProductFeatureSettingService } from '@app/services/product-feature-service';
import { ProductFeatureSettingItem } from '@app/models/product-feature-setting-item.enum';
import { ProductFeatureApiService } from '@app/services/api/product-feature-api.service';
import { ProductFeatureSettingViewModel } from '@app/viewmodels/product-feature-setting.viewmodel';
import { AdditionalPropertiesHelper } from '@app/helpers/additional-properties.helper';
import { PermissionService } from '@app/services/permission.service';
import { TenantService } from '@app/services/tenant.service';
import { EntityType } from '@app/models/entity-type.enum';
import { PortalPageTriggerResourceModel } from '@app/resource-models/portal-page-trigger.resource-model';
import { PortalExtensionsService } from '@app/services/portal-extensions.service';
import { AuthenticationService } from '@app/services/authentication.service';
import { ActionButtonPopover } from '@app/models/action-button-popover';
import { ActionButton } from '@app/models/action-button';
import { IconLibrary } from '@app/models/icon-library.enum';
import { ActionButtonHelper } from '@app/helpers/action-button.helper';
import { DataTableDefinitionApiService } from '@app/services/api/data-table-definition-api.service';
import { DataTableDefinitionResourceModel } from '@app/resource-models/data-table-definition.resource-model';
import { PopoverCommand } from '@app/models/popover-command';
import { PageType } from '@app/models/page-type.enum';
import { ProductReleaseSettingsResourceModel } from '@app/resource-models/product-release-settings.resource-model';
import { ProductReleaseSettingsModel } from '@app/models/product-release-settings.model';
import {
    instanceOfAdditionalPropertyContextSettingItemModel,
} from '@app/viewmodels/additional-property-definition-context-setting-item.viewmodel';
import { FormType } from '@app/models/form-type.enum';
import { ConfigurationFileViewModel } from '@app/viewmodels/configuration-file.viewmodel';
import { saveAs } from 'file-saver';
import { ReleaseViewModel } from '@app/viewmodels/release.viewmodel';
import {
    EntityDetailSegmentListComponent,
} from '@app/components/entity-detail-segment-list/entity-detail-segment-list.component';

/**
 * Export detail product page component class.
 * TODO: Write a better class header: displaying of product details.
 */
@Component({
    selector: 'app-detail-product',
    templateUrl: './detail-product.page.html',
    animations: [contentAnimation],
    styleUrls: [
        './detail-product.page.scss',
        '../../../../assets/css/scrollbar-detail.css',
    ],
    styles: [scrollbarStyle],
})
export class DetailProductPage extends DetailPage implements OnInit, AfterViewInit, OnDestroy {

    @ViewChild('releaseList') private releaseList: EntityDetailSegmentListComponent;
    public title: string = 'Product';
    public currentIndex: number;
    public segment: string = 'Details';
    public quoterAppBaseURL: string = '';
    public tenantId: string;
    public product: ProductViewModel;
    public releaseStatus: typeof ReleaseStatus = ReleaseStatus;
    public deployments: Array<any> = [];
    public systemAlerts: Array<SystemAlertResourceModel>;
    public policyTransactionTypeSettings: Array<ProductFeatureSettingViewModel>;
    public quoteTypeSettings: Array<ProductFeatureSettingViewModel>;
    public claimSettings: Array<ProductFeatureSettingViewModel>;
    public organisationSettings: Array<ProductFeatureSettingViewModel>;
    public deploymentSetting: ProductDeploymentSettingResourceModel = new ProductDeploymentSettingResourceModel();

    public deploymentEnvironments: any = DeploymentEnvironment;
    public productStatus: any = ProductStatus;
    public quoteAssetsSynchronisedDateTime: string;
    public claimAssetsSynchronisedDateTime: string;
    public numberPoolIds: Array<string> = Object.values(NumberPool);
    public numberPoolCount: Map<string, number> = new Map<string, number>();

    public emailTemplateSettings: Array<EmailTemplateSetting>;
    public defaultGuid: string = '00000000-0000-0000-0000-000000000000';
    private observeProductStatus: any;
    private productId: string;
    private pathTenantAlias: string;
    private productAlias: string;
    public permission: typeof Permission = Permission;
    public canGoBack: boolean = true;
    public detailsListItems: Array<DetailsListItem>;
    public productResourceModel: ProductResourceModel;
    public additionalPropertyContextType: AdditionalPropertyDefinitionContextType =
        AdditionalPropertyDefinitionContextType.Product;
    public additionalPropertyDefinitions: Array<AdditionalPropertyDefinition>;

    public componentTypes: Array<FormType> = [FormType.Quote, FormType.Claim];
    private canViewAdditionalPropertyValues: boolean = false;
    public refundRuleDescription: string = '';
    public isMutual: boolean;
    private entityTypes: typeof EntityType = EntityType;
    public dataTables: Array<DataTableDefinitionResourceModel>;
    protected portalPageTriggers: Array<PortalPageTriggerResourceModel>;
    protected actions: Array<ActionButtonPopover> = [];
    protected hasActionsIncludedInMenu: boolean = false;
    public actionButtonList: Array<ActionButton>;
    public canShowMore: boolean = false;
    public flipMoreIcon: boolean = false;
    public iconLibrary: typeof IconLibrary = IconLibrary;
    public productReleaseSettings: ProductReleaseSettingsResourceModel;
    public isLoadingSourceFiles: boolean;
    public sourceFilesErrorMessage: string;
    public isLoadingSettings: boolean;
    public settingsErrorMessage: string;
    public rootFiles: Map<FormType, Array<ConfigurationFileViewModel>>
        = new Map<FormType, Array<ConfigurationFileViewModel>>();
    public privateFiles: Map<FormType, Array<ConfigurationFileViewModel>>
        = new Map<FormType, Array<ConfigurationFileViewModel>>();
    public assetFiles: Map<FormType, Array<ConfigurationFileViewModel>>
        = new Map<FormType, Array<ConfigurationFileViewModel>>();
    private sourceFilesLoaded: boolean = false;
    public sourceFilesFound: boolean = false;
    public releaseTypeViewModel: typeof ReleaseViewModel = ReleaseViewModel;

    public constructor(
        public navProxy: NavProxyService,
        private sharedAlertService: SharedAlertService,
        private productApiService: ProductApiService,
        public eventService: EventService,
        private deploymentApiService: DeploymentApiService,
        private routeHelper: RouteHelper,
        private emailTemplateApiService: EmailTemplateApiService,
        private productFeatureService: ProductFeatureSettingService,
        private productFeatureApiService: ProductFeatureApiService,
        private emailTemplateService: EmailTemplateService,
        private systemAlertApiService: SystemAlertApiService,
        private systemAlertService: SystemAlertService,
        private numberPoolApiService: NumberPoolApiService,
        private productDeploymentSettingService: ProductDeploymentSettingApiService,
        private sharedLoaderService: SharedLoaderService,
        public layoutManager: LayoutManagerService,
        public userTypePathHelper: UserTypePathHelper,
        public elementRef: ElementRef,
        public injector: Injector,
        private appConfigService: AppConfigService,
        private sharedPopoverService: SharedPopoverService,
        private errorHandlerService: ErrorHandlerService,
        private additionalPropertiesService: AdditionalPropertyDefinitionApiService,
        private permissionService: PermissionService,
        private tenantService: TenantService,
        private portalExtensionService: PortalExtensionsService,
        private authService: AuthenticationService,
        private dataTableDefintionApiService: DataTableDefinitionApiService,
    ) {
        super(eventService, elementRef, injector);

        this.numberPoolIds.forEach((id: string) => this.numberPoolCount.set(id, -1));
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            if (!this.appConfigService.isMasterPortal()) {
                this.tenantId = appConfig.portal.tenantId;
                this.isMutual = appConfig.portal.isMutual;
            }
        });
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
    }

    public async ngAfterViewInit(): Promise<void> {
        this.destroyed = new Subject<void>();
        this.productAlias = this.routeHelper.getParam('productAlias');
        this.pathTenantAlias = this.routeHelper.getParam('tenantAlias');
        this.canGoBack = this.pathTenantAlias != null;
        if (!this.tenantId && this.pathTenantAlias) {
            this.tenantId = await this.tenantService.getTenantIdFromAlias(this.pathTenantAlias);
        }

        this.canViewAdditionalPropertyValues
            = this.permissionService.hasPermission(Permission.ViewAdditionalPropertyValues);
        this.eventService.getEntityUpdatedSubject('Product').pipe(takeUntil(this.destroyed))
            .subscribe((product: ProductResourceModel) => {
                if (this.product && product && !product.deleted && product.id == this.product.id) {
                    this.loadCurrentSegment();
                }
            });
        this.segment = this.routeHelper.getParam('segment') || this.segment;
        this.eventService.getEntityUpdatedSubject('deployment-settings').pipe(takeUntil(this.destroyed))
            .subscribe((result: any) => {
                if (this.deploymentSetting) {
                    this.deploymentSetting[result.environment] = result.urls;
                }
            });
        this.eventService.getEntityUpdatedSubject('quote-expiry-settings').pipe(takeUntil(this.destroyed))
            .subscribe((result: any) => {
                if (this.product && this.product.quoteExpirySettings) {
                    this.product.quoteExpirySettings.expiryDays = result.expiryDays;
                    this.product.quoteExpirySettings.enabled = result.enabled;
                }
            });

        this.loadCurrentSegment();
        this.preparePortalExtensions().then(() => this.generatePopoverLinks());
    }

    public ngOnDestroy(): void {
        if (this.observeProductStatus) {
            this.observeProductStatus.unsubscribe();
        }
        this.destroyed.next();
        this.destroyed.complete();
    }

    public segmentChanged($event: any): void {
        if ($event.detail.value !== this.segment) {
            this.segment = $event.detail.value;
            this.loadCurrentSegment();
            this.preparePortalExtensions().then(() => this.generatePopoverLinks());
            this.navProxy.updateSegmentQueryStringParameter(
                'segment',
                this.segment != 'Details' ? this.segment : null);
        }
    }

    private async preparePortalExtensions(): Promise<void> {
        this.portalPageTriggers =
            await this.portalExtensionService.getEnabledPortalPageTriggers(
                this.authService,
                this.entityTypes.Product,
                PageType.Display,
                this.segment);
    }

    private generatePopoverLinks(): void {
        this.actions = [];
        // Add portal page trigger actions
        this.actions = this.portalExtensionService.getActionButtonPopovers(this.portalPageTriggers);
        this.hasActionsIncludedInMenu = this.actions.filter((x: ActionButtonPopover) => x.includeInMenu).length > 0;
        this.initializeActionButtonList();
    }

    protected executePortalPageTrigger(trigger: PortalPageTriggerResourceModel): void {
        this.portalExtensionService.executePortalPageTrigger(
            trigger,
            this.entityTypes.Product,
            PageType.Display,
            this.segment,
            this.productId,
        );
    }

    private loadCurrentSegment(): void {
        switch (this.segment) {
            case 'Details': {
                this.loadDetails();
                break;
            }
            case 'Releases': {
                break;
            }
            case 'Settings': {
                this.loadSettings();
                break;
            }
            case 'Source': {
                this.loadSourceFiles();
                break;
            }
            default:
                break;
        }
    }

    public async loadDetails(): Promise<void> {
        if (this.product) {
            // let's not load it again if it's already loaded.
            return;
        }
        this.isLoading = true;
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', this.tenantId);

        if (!this.productId && this.productAlias) {
            this.productId = await this.productApiService.getIdByAlias(this.productAlias, this.tenantId).toPromise();
        }

        if (this.productId) {
            return this.productApiService.getById(this.productId, params)
                .pipe(
                    takeUntil(this.destroyed),
                    last())
                .toPromise().then((dt: ProductResourceModel) => {
                    if (dt) {// will be null if we navigate away from the page during loading
                        this.productUpdated(dt);
                        this.loadProductDevRelease().then((x: void) => {
                            this.loadDeployment().finally(() => {
                                setTimeout(() => { // this will refresh the details page.
                                    this.isLoading = false;
                                    this.preparePortalExtensions().then(() => this.generatePopoverLinks());
                                }, 100);
                            });
                        });
                    }
                }, (err: any) => {
                    // needed to be paired with last() rxjs function, throws error when return is undefined
                    // when destroying or canceling the api request
                    if (err.name != 'EmptyError') {
                        this.isLoading = false;
                        this.errorMessage = 'There was a problem loading the product details';
                        throw err;
                    }
                });
        }
    }

    private productUpdated(productResourceModel: ProductResourceModel): void {
        productResourceModel.status = productResourceModel.disabled ?
            ProductStatus.Disabled : productResourceModel.deleted ?
                ProductStatus.Deleted : productResourceModel.status;
        this.product = new ProductViewModel(productResourceModel);
        this.title = productResourceModel.name;
        this.productResourceModel = productResourceModel;
        if (this.segment == 'Details') {
            if (this.product && this.product.status === 'Initializing' && !this.observeProductStatus) {
                this.observeProductStatus =
                    interval(1000).pipe(takeUntil(this.destroyed)).subscribe(() => this.loadDetails());
            }
        } else if (this.segment === 'Source') {
            if (this.product.status !== 'Initializing') {
                if (this.observeProductStatus) {
                    this.observeProductStatus.unsubscribe();
                }
                this.loadSourceFiles();
            }
        }
    }

    private initializeDetailsListItems(): void {
        this.detailsListItems = this.product.createDetailsList(this);
        if (this.canViewAdditionalPropertyValues) {
            AdditionalPropertiesHelper.updateDetailList(
                this.detailsListItems,
                this.product.additionalPropertyValues,
            );
        }
    }

    public goBack(): void {
        const pathSegmentsAfter: Array<string> = this.routeHelper.getPathSegmentsAfter('path');
        if (pathSegmentsAfter[0] === 'product') {
            this.navProxy.navigateBack(['product', 'list']);
        } else {
            this.navProxy.navigateBack(
                ['tenant', this.pathTenantAlias],
                true,
                { queryParams: { segment: 'Products' } });
        }
    }

    private async loadProductDevRelease(): Promise<void> {
        return this.productApiService.getLatestSyncResult(this.product.id, this.product.tenantId)
            .pipe(takeUntil(this.destroyed))
            .toPromise().then(
                (dt: ProductAssetSyncResultModel) => { // will be null if we navigate away from the page during loading
                    if (dt?.quoteAssetsSynchronisedDateTime) {
                        this.quoteAssetsSynchronisedDateTime = dt.quoteAssetsSynchronisedDateTime;
                        this.product.addQuoteSynchronisedDateTime(dt.quoteAssetsSynchronisedDateTime);
                    }
                    if (dt?.claimAssetsSynchronisedDateTime) {
                        this.claimAssetsSynchronisedDateTime = dt.claimAssetsSynchronisedDateTime;
                        this.product.addClaimSynchronisedDateTime(dt.claimAssetsSynchronisedDateTime);
                    }
                },
                (err: HttpErrorResponse) => {
                    let throwError: boolean = true;
                    if (ProblemDetails.isProblemDetailsResponse(err)) {
                        let appError: ProblemDetails = ProblemDetails.fromJson(err.error);
                        // Ignore release not found errors, since a release may not have been created yet.
                        if (appError.Code == 'release.assets.not.synchronised') {
                            throwError = false;
                        }
                        // Notify but don't throw
                        if (appError.Code == 'release.initialisation.failed') {
                            this.errorHandlerService.handleError(appError);
                            throwError = false;
                        }
                    }
                    if (throwError) {
                        throw err;
                    }
                });
    }

    public async editSystemAlert(systemAlert: SystemAlertResourceModel): Promise<void> {
        if (systemAlert.id) {
            this.loadEditSystemAlertPanel(systemAlert);
        } else {
            this.loadCreateSystemAlertPanel(systemAlert);
        }
    }

    public editDeploymentSettings(environment: DeploymentEnvironment): void {
        const pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.push('deployment-settings', environment, 'edit');
        this.navProxy.navigateForward(pathSegments);
    }

    public async editQuoteExpirySettings(environment: DeploymentEnvironment): Promise<void> {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.push('quote-expiry-settings');
        this.navProxy.navigateForward(pathSegments);
    }

    public async editRefundSettings(environment: DeploymentEnvironment): Promise<void> {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.push('refund-settings');
        this.navProxy.navigateForward(pathSegments);
    }

    public loadCreateSystemAlertPanel(systemAlert: SystemAlertResourceModel): void {
        const pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.push('system-alert', 'create');
        this.navProxy.navigateForward(pathSegments, true, { queryParams: { typeId: systemAlert.systemAlertType.id } });
    }

    public loadEditSystemAlertPanel(systemAlert: SystemAlertResourceModel): void {
        const pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.push('system-alert', systemAlert.id, 'edit');
        this.navProxy.navigateForward(pathSegments);
    }

    public additionalPropertyContextSettingItemClicked($event: any): void {
        if (instanceOfAdditionalPropertyContextSettingItemModel($event)) {
            let withAppendedSegments: Array<string> = this.routeHelper.getPathSegmentsAndAppend(
                'additional-property-definition', $event.entityType);
            this.navProxy.navigate(withAppendedSegments);
        }
    }

    public releaseSelectionSettingItemClicked(productReleaseSettingsModel: ProductReleaseSettingsModel): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.push('release-selection-settings');
        this.navProxy.navigateForward(
            pathSegments,
            true,
            {
                queryParams:
                {
                    quoteType: productReleaseSettingsModel.quoteType.toLowerCase(),
                },
            });
    }

    private async loadSettings(): Promise<void> {
        this.isLoadingSettings = true;
        if (!this.product) {
            await this.loadDetails();
        }

        if (this.product) {
            const promises: Array<any> = [];
            promises.push(this.loadSystemAlerts());
            promises.push(this.loadDeploymentSettings());
            promises.push(this.loadNumberCounts());
            promises.push(this.loadEmailTemplateSetting());
            promises.push(this.loadAdditionalProperties());
            promises.push(this.loadProductFeatures());
            promises.push(this.loadDataTables());
            promises.push(this.loadProductRelease());
            await Promise.all(promises).then(
                () => {
                    this.isLoadingSettings = false;
                },
                (err: any) => {
                    this.isLoadingSettings = false;
                    throw err;
                });
        }
    }

    private async loadSystemAlerts(): Promise<void> {
        return this.systemAlertApiService.getSystemAlertByProduct(this.product.tenantId, this.product.id)
            .pipe(takeUntil(this.destroyed), last())
            .toPromise().then((dt: any) => {
                this.systemAlerts = dt;
            }, (err: any) => {
                // needed to be paired with last() rxjs function, throws error when return is undefined
                // when destroying or canceling the api request
                if (err.name != 'EmptyError') {
                    throw err;
                }
            });
    }

    private async loadAdditionalProperties(): Promise<void> {
        return this.additionalPropertiesService
            .getAdditionalPropertyDefinitionsByContextTypeAndContextIdAndParentContextId(
                this.product.tenantId,
                AdditionalPropertyDefinitionContextType.Product,
                this.product.id,
                this.product.tenantId)
            .pipe(takeUntil(this.destroyed))
            .toPromise()
            .then((resultAdditionalProperties: Array<AdditionalPropertyDefinition>) => {
                this.additionalPropertyDefinitions = resultAdditionalProperties;
            });
    }

    private async loadDeploymentSettings(): Promise<void> {
        return this.productDeploymentSettingService.getById(this.product.tenantId, this.product.id)
            .pipe(takeUntil(this.destroyed), last())
            .toPromise().then((ds: ProductDeploymentSettingResourceModel) => {
                this.deploymentSetting = ds;
            }, (err: any) => {
                // needed to be paired with last() rxjs function, throws error when return is undefined
                // when destroying or canceling the api request
                if (err.name != 'EmptyError') {
                    throw err;
                }
            });
    }

    private async loadDeployment(): Promise<void> {
        return this.deploymentApiService.getCurrentDeployments(this.product.tenantId, this.product.id)
            .pipe(takeUntil(this.destroyed))
            .toPromise()
            .then((_deployments: Array<DeploymentResourceModel>) => {
                this.deployments = [];
                let deployedToEnvironment: DeploymentEnvironment = DeploymentEnvironment.Development;
                for (const env of [DeploymentEnvironment.Production, DeploymentEnvironment.Staging]) {
                    const deployment: DeploymentResourceModel = _deployments.filter((d: DeploymentResourceModel) =>
                        d.environment === env)[0];
                    if (deployment) {
                        this.deployments.push(deployment);
                        if (deployedToEnvironment === DeploymentEnvironment.Development
                            && deployment.release != null && deployment.release.number !== 0) {
                            deployedToEnvironment = env;
                        }
                    } else {
                        this.deployments.push({ 'environment': env });
                    }
                }
                this.product.addDeployments(this.deployments);
                this.initializeDetailsListItems();
            });
    } // end of fetching deployments

    private async loadSourceFiles(): Promise<void> {
        if (this.sourceFilesLoaded) {
            // let's not load them again if they're already loaded.
            return;
        }
        this.isLoadingSourceFiles = true;
        if (!this.product) {
            await this.loadDetails();
        }
        this.rootFiles[FormType.Quote] = [];
        this.privateFiles[FormType.Quote] = [];
        this.assetFiles[FormType.Quote] = [];
        this.rootFiles[FormType.Claim] = [];
        this.privateFiles[FormType.Claim] = [];
        this.assetFiles[FormType.Claim] = [];
        return this.productApiService.getSourceFiles(this.product.id, this.product.tenantId)
            .pipe(
                finalize(() => this.isLoadingSourceFiles = false),
                takeUntil(this.destroyed),
                last())
            .toPromise().then((resourceModels: Array<ConfigurationFileResourceModel>) => {
                this.sourceFilesFound = resourceModels.length > 0;
                resourceModels.forEach((file: ConfigurationFileResourceModel) => {
                    if (file.path.indexOf('/') === -1) {
                        this.rootFiles[file.formType].push(new ConfigurationFileViewModel(file));
                    } else if (file.path.startsWith("files/")) {
                        this.privateFiles[file.formType].push(new ConfigurationFileViewModel(file));
                    } else if (file.path.startsWith("assets/")) {
                        this.assetFiles[file.formType].push(new ConfigurationFileViewModel(file));
                    }
                });
                this.sourceFilesLoaded = true;
            },
            (err: any) => {
                // needed to be paired with last() rxjs function, throws error when return is undefined
                // when destroying or canceling the api request
                if (err.name != 'EmptyError') {
                    this.sourceFilesErrorMessage = 'There was a problem loading the source files';
                    throw err;
                }
            });
    }

    public getSegmentReleaseList(params?: Map<string, string | Array<string>>,
    ): Observable<Array<ReleaseResourceModel>> {
        if (this.product) {
            return this.productApiService.getReleasesForProduct(this.product.id, this.product.tenantId, params);
        }
        this.loadDetails().then(() => this.releaseList.load());
        return of([]);
    }

    public releasesLoaded(hasReleases: boolean | undefined): void {
        this.preparePortalExtensions().then(() => this.generatePopoverLinks());
    }

    public releaseSelected(release: ReleaseViewModel): void {
        if (release) {
            let pathSegments: Array<string> = this.routeHelper.getPathSegments();
            if (pathSegments.indexOf('release') < 0) {
                pathSegments.push('release', release.id);
                this.navProxy.navigateForward(pathSegments);
            }
        }
    }

    public goToTenant(): void {
        this.navProxy.navigate(['tenant', this.product.tenantAlias], { queryParams: { segment: 'Products' } });
    }

    public onClickNewRelease(): void {
        const pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.push('release', 'create');
        this.navProxy.navigateForward(pathSegments);
    }

    public userDidTapUserTransaction(transactionType: ProductFeatureSettingViewModel): void {
        if (transactionType.productFeatureSettingItem == ProductFeatureSettingItem.RenewalQuotes) {
            const pathSegments: Array<string> = this.routeHelper.getPathSegments();
            pathSegments.push('transaction-type', transactionType.productFeatureSettingItem, 'edit');
            this.navProxy.navigateForward(pathSegments);
        }
    }

    public userDidTapEditButton(): void {
        const pathSegmentsAfter: Array<string> = this.routeHelper.getPathSegmentsAfter('path');
        if (pathSegmentsAfter[0] === 'product') {
            this.navProxy.navigateForward(['product', this.productAlias, 'edit']);
        } else {
            this.navProxy.navigateForward(['tenant', this.pathTenantAlias, 'product', this.productAlias, 'edit']);
        }
    }

    public async presentPopover(event: any): Promise<void> {
        this.flipMoreIcon = true;
        const popoverDismissAction = async (command: PopoverCommand): Promise<any> => {
            this.flipMoreIcon = false;
            if (command && command.data && command.data.action) {
                if (command.data.action.actionName === PopoverProductAction.Edit) {
                    this.userDidTapEditButton();
                } else if (command.data.action.actionName === PopoverProductAction.CreateRelease) {
                    this.onClickNewRelease();
                } else if (command.data.action.actionName === PopoverProductAction.EnableProduct) {
                    this.updateProductStatus(false);
                } else if (command.data.action.actionName === PopoverProductAction.DisableProduct) {
                    this.updateProductStatus(true);
                } else if (command.data.action.actionName == PopoverProductAction.DeleteProduct) {
                    this.deleteProduct();
                } else if (command.data.action.actionName === PopoverProductAction.SynchronizeProductQuoteAssets) {
                    await this.synchronizeProductQuoteAssets();
                } else if (command.data.action.actionName === PopoverProductAction.SynchronizeProductClaimAssets) {
                    await this.synchronizeProductClaimAssets();
                } else if (command.data.action.actionName == PopoverProductAction.MoveUnassociatedQuotes) {
                    this.moveUnassociatedQuotes(command.data.environment);
                } else if (command.data.action.portalPageTrigger) {
                    this.portalExtensionService.executePortalPageTrigger(
                        command.data.action.portalPageTrigger,
                        this.entityTypes.Product,
                        PageType.Display,
                        this.segment,
                        this.productId,
                    );
                }
            }
        };

        await this.sharedPopoverService.show(
            {
                component: PopoverProductPage,
                componentProps: {
                    isDisabled: this.product.disabled,
                    actions: this.actions,
                    segment: this.segment,
                    canCreateRelease: this.canCreateNewRelease(),
                },
                cssClass: 'custom-popover more-button-top-popover-positioning',
                event: event,
            },
            'Product option popover',
            popoverDismissAction);
    }

    private moveUnassociatedQuotes(environment: DeploymentEnvironment): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.push('select-product-release');
        pathSegments.push(environment);
        this.navProxy.navigateForward(pathSegments);
    }

    public async synchronizeProductQuoteAssets(): Promise<void> {
        await this.sharedLoaderService.present('Synchronising quote assets...');
        let t0: number = performance.now();
        return this.productApiService.syncQuoteAssets(this.product.id, this.product.tenantId)
            .pipe(
                finalize(() => this.sharedLoaderService.dismiss()),
            )
            .toPromise().then(
                async (res: ProductAssetSyncResultModel) => {
                    if (res.quoteAssetsSynchronisedDateTime) {
                        this.quoteAssetsSynchronisedDateTime = res.quoteAssetsSynchronisedDateTime;
                        this.product.addQuoteSynchronisedDateTime(res.quoteAssetsSynchronisedDateTime);
                        this.detailsListItems = this.product.updateDetailsListAfterSync(this.detailsListItems);
                        let t1: number = performance.now();
                        let seconds: number = Math.round((t1 - t0) / 1000);
                        this.sharedAlertService.showToast(
                            `Quote assets were successfully synchronised in ${seconds} seconds`);
                        this.eventService.detailViewDataChanged();
                    }
                },
                (err: HttpErrorResponse) => {
                    throw err;
                },
            );
    }

    public async synchronizeProductClaimAssets(): Promise<void> {
        await this.sharedLoaderService.present('Synchronising claim assets...');
        let t0: number = performance.now();
        return this.productApiService.syncClaimAssets(this.product.id, this.product.tenantId)
            .pipe(
                finalize(() => this.sharedLoaderService.dismiss()),
            )
            .toPromise().then(
                async (res: ProductAssetSyncResultModel) => {
                    if (res.quoteAssetsSynchronisedDateTime) {
                        this.claimAssetsSynchronisedDateTime = res.claimAssetsSynchronisedDateTime;
                        this.product.addClaimSynchronisedDateTime(res.claimAssetsSynchronisedDateTime);
                        this.detailsListItems = this.product.updateDetailsListAfterSync(this.detailsListItems);
                        let t1: number = performance.now();
                        let seconds: number = Math.round((t1 - t0) / 1000);
                        this.sharedAlertService.showToast(
                            `Claim assets were successfully synchronised in ${seconds} seconds`);
                        this.eventService.detailViewDataChanged();
                    }
                },
                (err: HttpErrorResponse) => {
                    throw err;
                });
    }

    public async openFile(file: ConfigurationFileViewModel): Promise<void> {
        if (file.isDownloading) {
            return;
        }
        file.isDownloading = true;
        await this.sharedLoaderService.presentWithDelay('Downloading file...');
        this.productApiService.downloadSourceFile(
            this.product.id,
            file.sourceType,
            file.path,
            this.routeHelper.getContextTenantAlias())
            .pipe(
                takeUntil(this.destroyed),
                finalize(() => {
                    file.isDownloading = false;
                    this.sharedLoaderService.dismiss();
                }))
            .subscribe((blob: any) => {
                if (file.isBrowserViewable) {
                    const url: string = window.URL.createObjectURL(blob);
                    const newWindow: Window = window.open();
                    if (newWindow) {
                        newWindow.location.href = url;
                    }
                    window.URL.revokeObjectURL(url);
                } else {
                    saveAs(blob, file.path);
                }
            });
    }

    private async updateProductStatus(isDisabled: boolean): Promise<void> {
        await this.sharedLoaderService.presentWait();
        this.productResourceModel.disabled = this.product.disabled = isDisabled;
        let updateModel: ProductUpdateRequestResourceModel =
            new ProductUpdateRequestResourceModel(this.productResourceModel);
        return this.productApiService.update(this.product.id, updateModel)
            .pipe(
                finalize(() => this.sharedLoaderService.dismiss()),
                takeUntil(this.destroyed))
            .toPromise().then(
                (res: ProductResourceModel) => {
                    this.sharedAlertService.showToast('The ' + res.name + ' product has been ' +
                        (res.disabled ? 'disabled' : 'enabled') + '.');
                    this.product = new ProductViewModel(res);
                    this.eventService.getEntityUpdatedSubject('Product').next(res);
                },
                (err: HttpErrorResponse) => {
                    this.product.disabled = !isDisabled;
                    this.productResourceModel.disabled = !isDisabled;
                    throw err;
                });
    } // end of update product status

    private async deleteProduct(): Promise<void> {
        await this.sharedLoaderService.presentWait();
        this.productResourceModel.deleted = this.product.deleted = true;
        let updateModel: ProductUpdateRequestResourceModel =
            new ProductUpdateRequestResourceModel(this.productResourceModel);
        return this.productApiService.update(this.product.id, updateModel)
            .pipe(
                finalize(() => this.sharedLoaderService.dismiss()),
                takeUntil(this.destroyed))
            .toPromise().then(
                (res: ProductResourceModel) => {
                    this.eventService.getEntityUpdatedSubject('Product').next(res);
                    this.sharedAlertService.showToast('The ' + res.name + ' product has been deleted');
                    this.goBack();
                },
                (err: HttpErrorResponse) => {
                    this.product.deleted = false;
                    throw err;
                });
    } // end of delete product

    public updateSystemAlertStatus(event: any, systemAlert: SystemAlertResourceModel): void {
        if (systemAlert.id == null) {
            this.loadCreateSystemAlertPanel(systemAlert);
        } else {
            const isDisabled: boolean = (event.detail.checked ? false : true);
            if (isDisabled) {
                this.systemAlertService.disableSystemAlert(this.tenantId, systemAlert, this.destroyed);
            } else {
                this.systemAlertService.enableSystemAlert(this.tenantId, systemAlert, this.destroyed);
            }

        }
    }

    public updateDeploymentSettingsActiveState(event: any, environment: DeploymentEnvironment): void {
        if (this.deploymentSetting) {
            const updateModel: any = JSON.parse(JSON.stringify(this.deploymentSetting));
            updateModel[environment + 'IsActive'] = event.detail.checked;

            this.deploymentSetting[environment + 'IsActive'] = event.detail.checked;
            // send it as parameter
            this.productDeploymentSettingService.update(this.product.tenantId, this.product.id, updateModel)
                .pipe(takeUntil(this.destroyed))
                .subscribe(
                    (dt: ProductDeploymentSettingResourceModel) => {
                        const actioned: string = event.detail.checked ? 'activated' : 'deactivated';
                        const env: string = StringHelper.capitalizeFirstLetter(environment);
                        this.sharedAlertService.showToast(env + ' deployment restrictions have been ' + actioned + '.');
                    });
        }
    }

    public async navigateToDataTableDefinition(): Promise<void> {
        const pathSegmentsAfter: Array<string> = this.routeHelper.getPathSegmentsAfter('path');
        if (pathSegmentsAfter[0] === 'product') {
            this.navProxy.navigateForward(
                ['product', this.productAlias, 'data-table', 'list-detail']);
        } else {
            await this.navProxy.navigateForward(
                ['tenant', this.pathTenantAlias, 'product', this.productAlias, 'data-table', 'list-detail']);
        }
    }

    // START - ALL ABOUT EMAIL TEMPLATE SETTING
    private async loadEmailTemplateSetting(): Promise<void> {
        return this.emailTemplateApiService.getEmailTemplatesByProduct(
            this.product.id, this.tenantId)
            .pipe(takeUntil(this.destroyed))
            .toPromise().then((dt: Array<EmailTemplateSetting>) => {
                this.emailTemplateSettings = dt;
            });
    }

    private async loadProductFeatures(): Promise<void> {
        this.productFeatureApiService.getProductFeature(this.tenantId, this.product.id)
            .pipe(takeUntil(this.destroyed), last())
            .toPromise().then((productFeature: ProductFeatureSetting) => {
                this.refundRuleDescription = this.getRefundRuleDescription(productFeature);
                const policyTransactionTypeSettingModels: Array<ProductFeatureSettingViewModel> = [
                    new ProductFeatureSettingViewModel(
                        ProductFeatureSettingItem.NewBusinessPolicyTransactions,
                        'New Business',
                        productFeature.areNewBusinessPolicyTransactionsEnabled,
                        'shield-add',
                    ),
                    new ProductFeatureSettingViewModel(
                        ProductFeatureSettingItem.RenewalPolicyTransactions,
                        'Renewal',
                        productFeature.areRenewalPolicyTransactionsEnabled,
                        'shield-refresh',
                    ),
                    new ProductFeatureSettingViewModel(
                        ProductFeatureSettingItem.AdjustmentPolicyTransactions,
                        'Adjustment',
                        productFeature.areAdjustmentPolicyTransactionsEnabled,
                        'shield-pen',
                    ),
                    new ProductFeatureSettingViewModel(
                        ProductFeatureSettingItem.CancellationPolicyTransactions,
                        'Cancellation',
                        productFeature.areCancellationPolicyTransactionsEnabled,
                        'shield-ban',
                    ),
                ];
                this.policyTransactionTypeSettings = policyTransactionTypeSettingModels;

                const claimSettingModels: Array<ProductFeatureSettingViewModel> = [
                    new ProductFeatureSettingViewModel(
                        ProductFeatureSettingItem.Claims,
                        'Claims',
                        productFeature.isClaimsEnabled,
                        'clipboard'),
                    new ProductFeatureSettingViewModel(
                        ProductFeatureSettingItem.MustCreateClaimsAgainstPolicy,
                        'Must create claims against a policy',
                        productFeature.mustCreateClaimAgainstPolicy,
                        'clipboard'),
                ];
                this.claimSettings = claimSettingModels;

                const organisationSettingModels: Array<ProductFeatureSettingViewModel> = [
                    new ProductFeatureSettingViewModel(
                        ProductFeatureSettingItem.AllowQuotesForNewOrganisations,
                        'Allow quotes for new organisations',
                        productFeature.allowQuotesForNewOrganisations,
                        'domain'),
                ];
                this.organisationSettings = organisationSettingModels;

                const quoteTypeSettingModels: Array<ProductFeatureSettingViewModel> = [
                    new ProductFeatureSettingViewModel(
                        ProductFeatureSettingItem.NewBusinessQuotes,
                        'New Business',
                        productFeature.areNewBusinessQuotesEnabled,
                        'calculator-add',
                    ),
                    new ProductFeatureSettingViewModel(
                        ProductFeatureSettingItem.RenewalQuotes,
                        'Renewal',
                        productFeature.areRenewalQuotesEnabled,
                        'calculator-refresh',
                    ),
                    new ProductFeatureSettingViewModel(
                        ProductFeatureSettingItem.AdjustmentQuotes,
                        'Adjustment',
                        productFeature.areAdjustmentQuotesEnabled,
                        'calculator-pen',
                    ),
                    new ProductFeatureSettingViewModel(
                        ProductFeatureSettingItem.CancellationQuotes,
                        'Cancellation',
                        productFeature.areCancellationQuotesEnabled,
                        'calculator-ban',
                    ),
                ];
                this.quoteTypeSettings = quoteTypeSettingModels;
            }, (err: any) => {
                // needed to be paired with last() rxjs function, throws error when return is undefined
                // when destroying or canceling the api request
                if (err.name != 'EmptyError') {
                    this.errorMessage = 'There was a problem loading the portal details';
                    throw err;
                }
            });
    }

    private async loadDataTables(): Promise<void> {
        if (this.permissionService.hasPermission(Permission.ViewDataTables)) {
            const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
            params.set("tenant", this.tenantId);
            params.set("entityType", EntityType.Product);
            params.set("entityId", this.product.id);
            const dataTables: Array<DataTableDefinitionResourceModel> =
                await this.dataTableDefintionApiService
                    .getDataTableDefinitions(params)
                    .toPromise();
            this.dataTables = dataTables;
        }
    }

    private getRefundRuleDescription(productFeatureSetting: ProductFeatureSetting): string {

        if (productFeatureSetting.refundPolicy == "RefundsAreProvidedIfNoClaimsWereMade" &&
            productFeatureSetting.periodWhichNoClaimsMade == "AtAnyTime") {
            return "Provided if no claims have been made at any time";
        }
        const lastNumberOfYears: string = productFeatureSetting.lastNumberOfYearsWhichNoClaimsMade &&
            productFeatureSetting.lastNumberOfYearsWhichNoClaimsMade === 1 ? 'year' :
            `${productFeatureSetting.lastNumberOfYearsWhichNoClaimsMade} years`;

        let periodWhichNoClaimsMadeMap: Map<string, string> = new Map<string, string>([
            ["CurrentPolicyPeriod", "the current policy period"],
            ["LifeTimeOfThePolicy", "the lifetime of the policy"],
            ["LastNumberOfYears", `the last ${lastNumberOfYears}`],
        ]);

        const periodWhichNoClaimsMade: string = productFeatureSetting.periodWhichNoClaimsMade != undefined ?
            periodWhichNoClaimsMadeMap.get(productFeatureSetting.periodWhichNoClaimsMade) : '';
        let refundPolicyMap: Map<string, string> = new Map<string, string>([
            ["RefundsAreAlwaysProvided", "Always provided"],
            ["RefundsAreNeverProvided", "Never provided"],
            ["RefundsAreProvidedIfNoClaimsWereMade",
                `Provided if no claims were made during ${periodWhichNoClaimsMade}`],
            ["RefundsCanOptionallyBeProvided", "Manually selected during review or endorsement"],
        ]);

        return refundPolicyMap.get(productFeatureSetting.refundPolicy);
    }

    public createTemplate(model: EmailTemplateSetting): void {
        const pathSegmentsAfter: Array<string> = this.routeHelper.getPathSegmentsAfter('path');
        if (pathSegmentsAfter[0] === 'product') {
            this.navProxy.navigateForward(['product', this.productAlias, 'email-template', 'create']);
        } else {
            this.navProxy.navigateForward([
                'tenant',
                this.pathTenantAlias,
                'product',
                this.productAlias,
                'email-template',
                'create']);
        }
    }

    public userDidTapTemplateItem(model: EmailTemplateSetting): void {
        const pathSegmentsAfter: Array<string> = this.routeHelper.getPathSegmentsAfter('path');
        if (pathSegmentsAfter[0] === 'product') {
            this.navProxy.navigateForward([
                'product',
                this.productAlias,
                'email-template',
                model.id,
                'edit']);
        } else {
            this.navProxy.navigateForward([
                'tenant',
                this.pathTenantAlias,
                'product',
                this.productAlias,
                'email-template',
                model.id,
                'edit']);
        }
    }

    public async userDidChangeEmailTemplateStatus(
        event: any,
        model: EmailTemplateSetting,
        detailsId: string,
    ): Promise<void> {
        if (!model) {
            return;
        }

        const enabled: boolean = (event.detail.checked ? true : false);
        if (enabled) {
            this.emailTemplateService.enableEmailTemplate(this.product.tenantId, model.id, this.destroyed);
        } else {
            this.emailTemplateService.disableEmailTemplate(this.product.tenantId, model.id, this.destroyed);
        }
    }

    public async userDidTogglePolicyTransaction(event: any, model: ProductFeatureSettingViewModel): Promise<void> {
        await this.userDidToggleProductFeature(event, model, 'policy transaction');
    }

    public async userDidToggleOrganisationSetting(event: any, model: ProductFeatureSettingViewModel): Promise<void> {
        await this.userDidToggleProductFeature(event, model, 'setting');
    }

    public async userDidToggleQuoteTransaction(event: any, model: ProductFeatureSettingViewModel): Promise<void> {
        await this.userDidToggleProductFeature(event, model, 'quote type');
    }

    public async userDidToggleClaimSetting(event: any, model: ProductFeatureSettingViewModel): Promise<void> {
        await this.userDidToggleProductFeature(event, model, 'setting');
    }

    private async userDidToggleProductFeature(
        event: any,
        model: ProductFeatureSettingViewModel,
        suffix: string,
    ): Promise<void> {
        if (!model) {
            return;
        }

        const disabled: boolean = (event.detail.checked ? true : false);
        if (disabled) {
            this.productFeatureApiService.enable(
                this.tenantId, this.productId, model.productFeatureSettingItem)
                .pipe(takeUntil(this.destroyed))
                .toPromise().then(() => {
                    this.productFeatureService.clearProductFeatureSettings();
                    this.sharedAlertService.showToast(`"${model.name}" ${suffix} has been enabled.`);
                });
        } else {
            this.productFeatureApiService.disable(
                this.tenantId, this.productId, model.productFeatureSettingItem)
                .pipe(takeUntil(this.destroyed))
                .toPromise().then(() => {
                    this.productFeatureService.clearProductFeatureSettings();
                    this.sharedAlertService.showToast(`"${model.name}" ${suffix} has been disabled.`);
                });
        }
        model.isEnabled = event.detail.checked;
    }

    public editNumberPool(numberPoolId: string): void {
        const pathSegmentsAfter: Array<string> = this.routeHelper.getPathSegmentsAfter('path');
        if (pathSegmentsAfter[0] === 'product') {
            this.navProxy.navigateForward([
                'product',
                this.productAlias,
                'number-pool',
                numberPoolId,
                'edit']);
        } else {
            this.navProxy.navigateForward([
                'tenant',
                this.pathTenantAlias,
                'product',
                this.productAlias,
                'number-pool',
                numberPoolId,
                'edit']);
        }
    }

    private async loadNumberCounts(): Promise<Array<void>> {
        const promises: Array<Promise<void>> = new Array<Promise<void>>();
        this.numberPoolIds.forEach((id: string) => {
            promises.push(this.loadNumberCount(id));
        });
        return Promise.all(promises);
    }

    private async loadNumberCount(numberPoolId: string): Promise<void> {
        return this.numberPoolApiService
            .getAvailableNumbers(
                this.product.tenantId,
                this.product.id,
                numberPoolId,
                this.deploymentEnvironments.Production)
            .pipe(takeUntil(this.destroyed), last())
            .toPromise().then((res: NumberPoolGetResultModel) => {
                this.numberPoolCount.set(numberPoolId, res.numbers.length);
            }, (err: any) => {
                // needed to be paired with last() rxjs function, throws error when return is undefined
                // when destroying or canceling the api request
                if (err.name != 'EmptyError') {
                    throw err;
                }
            });
    }

    private initializeActionButtonList(): void {
        let actionButtonList: Array<ActionButton> = [];
        this.canShowMore = this.segment == 'Details' || this.canCreateNewRelease();
        if (this.canCreateNewRelease()) {
            actionButtonList.push(ActionButton.createActionButton(
                "Create",
                "plus",
                IconLibrary.AngularMaterial,
                false,
                "Create Release",
                true,
                (): void => {
                    return this.onClickNewRelease();
                },
            ));
        }

        if (this.segment == 'Details'
            && !this.isLoading
            && this.permissionService.hasPermission(Permission.ManageProducts)
        ) {
            actionButtonList.push(ActionButton.createActionButton(
                "Edit",
                "pencil",
                IconLibrary.AngularMaterial,
                false,
                "Edit Product",
                true,
                (): void => {
                    return this.userDidTapEditButton();
                },
            ));
        }

        if (this.segment == 'Details'
            && !this.isLoading
            && this.permissionService.hasPermission(
                Permission.ManageProducts,
            )
            && this.permissionService.hasPermission(
                Permission.ManageReleases)
        ) {
            actionButtonList.push(ActionButton.createActionButton(
                "Synchronise Quote",
                "sync-calculator",
                IconLibrary.AngularMaterial,
                false,
                "Synchronise Quote Assets",
                true,
                (): Promise<void> => {
                    return this.synchronizeProductQuoteAssets();
                },
            ));
            actionButtonList.push(ActionButton.createActionButton(
                "Synchronise Claim",
                "sync-clipboard",
                IconLibrary.AngularMaterial,
                false,
                "Synchronise Claim Assets",
                true,
                (): Promise<void> => {
                    return this.synchronizeProductClaimAssets();
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
                    (): any => {
                        return this.portalExtensionService.executePortalPageTrigger(
                            action.portalPageTrigger,
                            this.entityTypes.Product,
                            PageType.Display,
                            this.segment,
                            this.productId,
                        );
                    },
                ));
            }
        }

        this.actionButtonList = ActionButtonHelper.sortActionButtons(actionButtonList);
    }

    private canCreateNewRelease(): boolean {
        return this.segment == 'Releases'
            && this.product != null
            && !this.product.disabled
            && this.product.status == this.productStatus.Initialised
            && (this.releaseList && !this.releaseList.isLoadingItems)
            && this.permissionService.hasPermission(
                Permission.ManageReleases);
    }

    private async loadProductRelease(): Promise<void> {
        let productReleaseSettings: ProductReleaseSettingsResourceModel = await this.productApiService
            .getProductReleaseSettings(this.tenantId, this.product.id)
            .toPromise();
        this.productReleaseSettings = productReleaseSettings;
    }
}
