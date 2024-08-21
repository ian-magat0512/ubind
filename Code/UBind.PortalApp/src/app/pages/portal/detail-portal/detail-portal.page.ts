import { Component, Injector, ElementRef, OnInit, OnDestroy, AfterViewInit } from '@angular/core';
import { AlertController } from '@ionic/angular';
import { Observable, Subject, Subscription, SubscriptionLike } from 'rxjs';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { EmailTemplateSetting } from '@app/models';
import {
    PortalDetailResourceModel, PortalResourceModel,
} from '@app/resource-models/portal/portal.resource-model';
import { PortalApiService } from '@app/services/api/portal-api.service';
import { PopoverPortalPage } from '../popover-portal/popover-portal.page';
import { contentAnimation } from '@assets/animations';
import { scrollbarStyle } from '@assets/scrollbar';
import { Permission } from '@app/helpers/permissions.helper';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { EventService } from '@app/services/event.service';
import { finalize, takeUntil } from 'rxjs/operators';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { SharedPopoverService } from '@app/services/shared-popover.service';
import { EmailTemplateApiService } from '@app/services/api/email-template-api.service';
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import { PortalDetailViewModel } from '@app/viewmodels/portal-detail.viewmodel';
import { ProductPortalSettingApiService } from '@app/services/api/product-portal-setting.api.service';
import { ProductPortalSettingResourceModel } from '@app/resource-models/product-portal-setting.resource-model';
import { AppConfigService } from '@app/services/app-config.service';
import { AppConfig } from '@app/models/app-config';
import { ActivatedRoute, Params } from '@angular/router';
import { EmailTemplateService } from '@app/services/email-template.service';
import { AuthenticationService } from '@app/services/authentication.service';
import { PermissionService } from '@app/services/permission.service';
import { PortalPageTriggerResourceModel } from '@app/resource-models/portal-page-trigger.resource-model';
import { EntityType } from '@app/models/entity-type.enum';
import { PortalExtensionsService } from '@app/services/portal-extensions.service';
import { TenantApiService } from '@app/services/api/tenant-api.service';
import { TenantService } from '@app/services/tenant.service';
import { ActionButtonPopover } from '@app/models/action-button-popover';
import { ActionButton } from '@app/models/action-button';
import { IconLibrary } from '@app/models/icon-library.enum';
import { ActionButtonHelper } from '@app/helpers/action-button.helper';
import { PopoverCommand } from '@app/models/popover-command';
import { PageType } from '@app/models/page-type.enum';
import { DeploymentEnvironment } from '@app/models/deployment-environment.enum';
import { PortalUserType } from '@app/models/portal-user-type.enum';
import { PortalSignInMethodResourceModel } from '@app/resource-models/portal/portal-sign-in-method.resource-model';
import { OrganisationModel } from '@app/models/organisation.model';

/**
 * Detail view for a portal.
 */
@Component({
    selector: 'app-detail-portal',
    templateUrl: './detail-portal.page.html',
    styleUrls: [
        './detail-portal.page.scss',
        '../../../../assets/css/scrollbar-detail.css',
    ],
    animations: [contentAnimation],
    styles: [scrollbarStyle],
})
export class DetailPortalPage extends DetailPage implements OnInit, AfterViewInit, OnDestroy {

    public title: string = 'Portal';
    public currentIndex: number;
    public segment: string = 'Details';
    public pathTenantAlias: string;
    public performingUserTenantId: string;
    public permission: typeof Permission = Permission;
    public canGoBack: boolean = true;
    protected pathOrganisationId: string;
    private portalOrganisationId: string;
    protected performingUserOrganisationId: string;
    public userId: string;
    public canShowMore: boolean;

    // TODO: Remove this hack which someone wrote.
    // Deployment Delete Edit Selectors
    public deploymentCRUDButtonSelectors: Array<string> =
        ['ion-alert div.alert-button-group button:nth-of-type(2)',
            'ion-alert div.alert-button-group button:nth-of-type(3)'];
    public loadEmailTemplateSubscription: SubscriptionLike;
    public saveEmailTemplateSubscription: SubscriptionLike;

    private portalId: string;
    public portal: PortalDetailViewModel;
    public portalResourceModel: PortalResourceModel;
    public detailsListItems: Array<DetailsListItem>;
    private canViewAdditionalPropertyValues: boolean = false;
    public emailTemplateSettings: Array<EmailTemplateSetting>;
    public productPortalSettings: Array<ProductPortalSettingResourceModel>;
    public defaultGuid: string = '00000000-0000-0000-0000-000000000000';

    private entityTypes: typeof EntityType = EntityType;
    protected portalPageTriggers: Array<PortalPageTriggerResourceModel>;
    protected actions: Array<ActionButtonPopover> = [];
    protected hasActionsIncludedInMenu: boolean = false;
    public actionButtonList: Array<ActionButton>;
    public flipMoreIcon: boolean = false;

    // eslint-disable-next-line @typescript-eslint/naming-convention
    public PortalUserType: typeof PortalUserType = PortalUserType;
    public allowCustomerSelfAccountCreation: boolean;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public developmentUrl: string = '';
    public stagingUrl: string = '';
    public productionUrl: string = '';

    public isLoadingSettings: boolean = true;
    public settingsErrorMessage: string;
    private isDetailsLoaded: boolean = false;
    private isSettingsLoaded: boolean = false;

    public isLoadingSignInMethods: boolean;
    public enabledSignInMethodsCount: number = 0;

    public constructor(
        public tenantService: TenantService,
        public tenantApiService: TenantApiService,
        public navProxy: NavProxyService,
        private routeHelper: RouteHelper,
        private portalApiService: PortalApiService,
        private emailTemplateApiService: EmailTemplateApiService,
        private emailTemplateService: EmailTemplateService,
        private productPortalSettingApiService: ProductPortalSettingApiService,
        public layoutManager: LayoutManagerService,
        protected sharedLoaderService: SharedLoaderService,
        protected sharedAlertService: SharedAlertService,
        protected eventService: EventService,
        protected alertCtrl: AlertController,
        elementRef: ElementRef,
        private sharedPopoverService: SharedPopoverService,
        injector: Injector,
        private appConfigService: AppConfigService,
        protected route: ActivatedRoute,
        private authenticationService: AuthenticationService,
        private permissionService: PermissionService,
        private portalExtensionService: PortalExtensionsService,
    ) {
        super(eventService, elementRef, injector);
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            if (!this.appConfigService.isMasterPortal()) {
                this.performingUserTenantId = appConfig.portal.tenantId;
            }
        });
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.eventService.performingUserOrganisationSubject$.pipe(takeUntil(this.destroyed))
            .subscribe((organisation: OrganisationModel) => {
                this.performingUserOrganisationId = organisation.id;
            });
        this.segment = this.route.snapshot.queryParamMap.get('previous') || this.segment;
        this.route.params.pipe(takeUntil(this.destroyed)).subscribe((params: Params) => {
            this.pathOrganisationId = params['organisationId'];
        });
        this.eventService.getEntityUpdatedSubject('Portal').pipe(takeUntil(this.destroyed))
            .subscribe((portalDetailResourceModel: PortalDetailResourceModel) => {
                if (this.portalId == portalDetailResourceModel.id) {
                    this.portal = new PortalDetailViewModel(portalDetailResourceModel);
                    this.initializeDetailsListItems();
                    this.generatePopoverLinks();
                }
            });
    }

    public ngAfterViewInit(): void {
        this.pathTenantAlias = this.routeHelper.getParam('tenantAlias');
        this.portalId = this.routeHelper.getParam('portalId');
        this.canGoBack = this.pathTenantAlias != null || this.pathOrganisationId != null;
        this.canShowMore = this.permissionService.hasPermission(Permission.ManagePortals);
        this.segment = this.routeHelper.getParam('segment') || this.segment;
        let promises: Array<Promise<void>> = [];
        if (this.segment == 'Settings') {
            promises.push(this.loadSettings());
        } else {
            promises.push(this.loadDetails());
        }
        promises.push(this.preparePortalExtensions());
        Promise.all(promises).then(() => {
            this.generatePopoverLinks();
        });
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    public async loadDetails(): Promise<void> {
        this.isLoading = true;
        this.canViewAdditionalPropertyValues
            = this.permissionService.hasPermission(Permission.ViewAdditionalPropertyValues);
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', this.routeHelper.getContextTenantAlias());
        params.set("useCache", 'false');
        try {
            let portalDetailDto: PortalDetailResourceModel =
                await this.portalApiService.getById(this.portalId, params).toPromise();
            this.portal = new PortalDetailViewModel(portalDetailDto);
            this.portalOrganisationId = this.portal.organisationId;
            this.portalResourceModel = portalDetailDto;
            this.initializeDetailsListItems();
            this.title = portalDetailDto.name;
            this.determinePortalLocations();
            this.isDetailsLoaded = true;
        } catch (error) {
            if (error.name != 'EmptyError') {
                this.errorMessage = 'There was a problem loading the portal details';
            }
            throw error;
        } finally {
            this.isLoading = false;
        }
    }

    private async loadSettings(): Promise<void> {
        try {
            this.isLoadingSettings = true;
            if (!this.portal) {
                await this.loadDetails();
            }
            let promises: Array<Promise<void>> = [];
            promises.push(this.loadyEmailTemplatesByPortal());
            promises.push(this.loadProductPortalSettingsByTenant());
            promises.push(this.loadAccountCreationSetting());
            promises.push(this.loadEnabledSignInMethodCount());
            await Promise.all(promises);
            this.isSettingsLoaded = true;
        } catch (error) {
            this.settingsErrorMessage = 'There was a problem loading the portal settings';
            this.isSettingsLoaded = false;
            throw error;
        } finally {
            this.isLoadingSettings = false;
        }
    }

    private async preparePortalExtensions(): Promise<void> {
        this.portalPageTriggers =
            await this.portalExtensionService.getEnabledPortalPageTriggers(
                this.authenticationService,
                this.entityTypes.Portal,
                PageType.Display,
                this.segment);
    }

    private generatePopoverLinks(): void {
        // Add portal page trigger actions
        this.actions = this.portalExtensionService.getActionButtonPopovers(this.portalPageTriggers);
        if (this.permissionService.hasPermission(Permission.ManagePortals)) {
            if (!this.portal.disabled) {
                if (this.portal.isDefault) {
                    this.actions.push({
                        actionName: 'Unset as Default',
                        actionIcon: 'star-off',
                        iconLibrary: IconLibrary.AngularMaterial,
                        actionButtonLabel: 'Unset as Default',
                        actionButtonPrimary: false,
                        includeInMenu: true,
                        // eslint-disable-next-line brace-style
                        callback: (): void => { this.setAsDefault(false); },
                    });
                } else {
                    this.actions.push({
                        actionName: 'Set as Default',
                        actionIcon: 'star',
                        iconLibrary: IconLibrary.AngularMaterial,
                        actionButtonLabel: 'Set as Default',
                        actionButtonPrimary: false,
                        includeInMenu: true,
                        // eslint-disable-next-line brace-style
                        callback: (): void => { this.setAsDefault(true); },
                    });
                }
            }
        }
        this.hasActionsIncludedInMenu = this.actions.filter((x: ActionButtonPopover) => x.includeInMenu).length > 0;
        this.initializeActionButtonList();
    }

    protected executePortalPageTrigger(trigger: PortalPageTriggerResourceModel): void {
        this.portalExtensionService.executePortalPageTrigger(
            trigger,
            this.entityTypes.Portal,
            PageType.Display,
            this.segment,
            this.portalId,
        );
    }

    private initializeDetailsListItems(): void {
        this.detailsListItems = this.portal.createDetailsList(
            this.navProxy,
            this.pathTenantAlias,
            this.authenticationService.isMasterUser(),
            this.canViewAdditionalPropertyValues);
    }

    public userDidTapTemplateItem(model: EmailTemplateSetting): void {
        const pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.push('email-template');
        pathSegments.push(model.id);
        pathSegments.push('edit');
        this.navProxy.navigateForward(pathSegments);
    }

    public async userDidChangeEmailTemplateStatus(event: any, emailTemplate: EmailTemplateSetting): Promise<void> {
        if (!emailTemplate) {
            return;
        }
        emailTemplate.disabled = !emailTemplate.disabled;
        if (emailTemplate.disabled) {
            this.disableEmailTemplate(emailTemplate.id);
        } else {
            this.enableEmailTemplate(emailTemplate.id);
        }
    }

    private enableEmailTemplate(emailTemplateId: string): void {
        this.emailTemplateService.enableEmailTemplate(
            this.routeHelper.getContextTenantAlias(), emailTemplateId, this.destroyed);
    }

    private disableEmailTemplate(emailTemplateId: string): void {
        this.emailTemplateService.disableEmailTemplate(
            this.routeHelper.getContextTenantAlias(), emailTemplateId, this.destroyed);
    }

    public async productPortalSettingChanged(event: any, setting: ProductPortalSettingResourceModel): Promise<void> {
        if (!setting.productId) {
            return;
        }

        await this.setProductPortalSettingStatus(setting, event.detail.checked);
        setting.isNewQuotesAllowed = event.detail.checked;
    }

    public async accountCreationSettingChanged(event: any): Promise<void> {
        if (this.allowCustomerSelfAccountCreation === event.detail.checked) {
            return;
        }
        this.allowCustomerSelfAccountCreation = event.detail.checked;
        let result: any = this.allowCustomerSelfAccountCreation
            ? this.portalApiService
                .enableCustomerSelfAccountCreationSetting(this.routeHelper.getContextTenantAlias(), this.portalId)
            : this.portalApiService
                .disableCustomerSelfAccountCreationSetting(this.routeHelper.getContextTenantAlias(), this.portalId);
        await result.toPromise();
        this.sharedAlertService.showToast(
            `Customer User Account creation from the Login page is ` +
            `${(this.allowCustomerSelfAccountCreation ? 'enabled' : 'disabled')} ` +
            `for this Portal`);
    }

    private async setProductPortalSettingStatus(
        setting: ProductPortalSettingResourceModel,
        status: boolean,
    ): Promise<void> {
        let result: Observable<ProductPortalSettingResourceModel> = status
            ? this.productPortalSettingApiService.enable(
                this.portalId, this.routeHelper.getContextTenantAlias(), setting.productId)
            : this.productPortalSettingApiService.disable(
                this.portalId, this.routeHelper.getContextTenantAlias(), setting.productId);

        await result.toPromise();
        this.sharedAlertService.showToast(
            `New quotes can ${(status ? 'now be' : 'no longer be')} ` +
            `created for the product "${setting.name}" ` +
            `in the portal "${this.portal.name}"`);
    }

    public async userDidTapEditButton(): Promise<void> {
        const pathSegments: Array<string> = this.routeHelper.appendPathSegment('edit');
        this.navProxy.navigate(pathSegments);
    }

    public async presentPopover(event: any): Promise<void> {
        this.flipMoreIcon = true;
        const popoverDismissAction = (command: PopoverCommand): void => {
            this.flipMoreIcon = false;
            if (command && command.data && command.data.action) {
                const commandAction: string = command.data.action.actionName;
                if (command.data.action.callback) {
                    command.data.action.callback();
                } else if (commandAction === 'edit') {
                    this.userDidTapEditButton();
                } else if (commandAction === 'enablePortal') {
                    this.enablePortal();
                } else if (commandAction === 'disablePortal') {
                    this.disablePortal();
                } else if (commandAction === 'deletePortal') {
                    this.deletePortal();
                } else if (command.data.action.portalPageTrigger) {
                    this.portalExtensionService.executePortalPageTrigger(
                        command.data.action.portalPageTrigger,
                        this.entityTypes.Portal,
                        PageType.Display,
                        this.segment,
                        this.portalId,
                    );
                }
            }
        };

        await this.sharedPopoverService.show(
            {
                component: PopoverPortalPage,
                componentProps: {
                    isPortalDisabled: this.portal.disabled,
                    segment: this.segment,
                    actions: this.actions,
                },
                cssClass: 'custom-popover more-button-top-popover-positioning',
                event: event,
            },
            'Portal option popover',
            popoverDismissAction);
    }

    private goBack(): void {
        const pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.pop();
        let parentEntitySegment: string = pathSegments[pathSegments.length - 3];
        if (parentEntitySegment == 'tenant' || parentEntitySegment == 'organisation') {
            pathSegments.pop();
            this.navProxy.navigateBack(pathSegments, true, { queryParams: { segment: 'Portals' } });
        } else if (this.isPortalOrganisationDifferentFromLoggedInUsersOrganisation()) {
            this.navProxy.navigateBack(
                ['organisation', this.portalOrganisationId],
                true,
                { queryParams: { segment: 'Portals' } });
        } else {
            pathSegments.push('list');
            this.navProxy.navigateBack(pathSegments, true, { queryParams: { segment: this.portal.segment } });
        }
    }

    private isPortalOrganisationDifferentFromLoggedInUsersOrganisation(): boolean {
        return this.performingUserOrganisationId != this.portalOrganisationId;
    }

    public editDevelopmentUrl(): void {
        const pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.push('location');
        pathSegments.push('development');
        this.navProxy.navigateForward(pathSegments);
    }

    public editStagingUrl(): void {
        const pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.push('location');
        pathSegments.push('staging');
        this.navProxy.navigateForward(pathSegments);
    }

    public editProductionUrl(): void {
        const pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.push('location');
        pathSegments.push('production');
        this.navProxy.navigateForward(pathSegments);
    }

    public editStyling(): void {
        const pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.push('theme');
        this.navProxy.navigateForward(pathSegments);
    }

    public segmentChanged($event: any): void {
        if ($event.detail.value != this.segment) {
            this.segment = $event.detail.value;
            switch (this.segment) {
                case 'Details':
                    if (!this.isDetailsLoaded) {
                        this.loadDetails()
                            .then(() => this.preparePortalExtensions())
                            .then(() => this.generatePopoverLinks());
                    }
                    break;
                case 'Settings':
                    if (!this.isSettingsLoaded) {
                        this.loadSettings()
                            .then(() => this.preparePortalExtensions())
                            .then(() => this.generatePopoverLinks());
                    }
                    break;
            }

            this.navProxy.updateSegmentQueryStringParameter(
                'segment',
                this.segment != 'Details' ? this.segment : null,
            );
        }
    }

    private async disablePortal(): Promise<void> {
        await this.sharedLoaderService.presentWait();
        const portal: PortalDetailResourceModel
            = await this.portalApiService.disable(this.portalId, this.routeHelper.getContextTenantAlias())
                .pipe(
                    finalize(() => this.sharedLoaderService.dismiss()),
                    takeUntil(this.destroyed),
                )
                .toPromise();
        this.portal.disabled = true;
        this.portalResourceModel.disabled = true;
        this.sharedAlertService.showToast(`The ${portal.name} portal has been disabled`);
        this.eventService.getEntityUpdatedSubject('Portal').next(portal);
    }

    private async enablePortal(): Promise<void> {
        await this.sharedLoaderService.presentWait();
        const portal: PortalDetailResourceModel
            = await this.portalApiService.enable(this.portalId, this.routeHelper.getContextTenantAlias())
                .pipe(
                    finalize(() => this.sharedLoaderService.dismiss()),
                    takeUntil(this.destroyed),
                )
                .toPromise();
        this.portal.disabled = false;
        this.portalResourceModel.disabled = false;
        this.sharedAlertService.showToast(`The ${portal.name} portal has been enabled`);
        this.eventService.getEntityUpdatedSubject('Portal').next(portal);
    }

    private async deletePortal(): Promise<void> {
        await this.sharedLoaderService.presentWait();
        try {
            await this.portalApiService.delete(this.portalId, this.routeHelper.getContextTenantAlias())
                .pipe(
                    finalize(() => this.sharedLoaderService.dismiss()),
                    takeUntil(this.destroyed),
                )
                .toPromise();
            this.portal.deleted = true;
            this.portalResourceModel.deleted = true;
            this.eventService.getEntityUpdatedSubject('Portal').next(this.portal);
            this.sharedAlertService.showToast(`The ${this.portal.name} portal has been deleted`);
            this.goBack();
        } catch (err) {
            this.portal.deleted = false;
            this.portalResourceModel.deleted = false;
            throw err;
        }
    }

    private async loadyEmailTemplatesByPortal(): Promise<void> {
        this.emailTemplateSettings =
            await this.emailTemplateApiService.getEmailTemplatesByPortal(
                this.portalId, this.routeHelper.getContextTenantAlias())
                .toPromise();
    }

    private async loadProductPortalSettingsByTenant(): Promise<void> {
        this.productPortalSettings =
            await this.productPortalSettingApiService.getByTenant(
                this.portalId, this.routeHelper.getContextTenantAlias())
                .toPromise();
    }

    private async loadAccountCreationSetting(): Promise<void> {
        this.allowCustomerSelfAccountCreation = await this.portalApiService
            .canCustomersSelfRegister(this.routeHelper.getContextTenantAlias(), this.portalId)
            .toPromise();
    }

    private determinePortalLocations(): void {
        this.developmentUrl = this.portal.developmentUrl
            || this.getDefaultPortalLocation(DeploymentEnvironment.Development);
        this.stagingUrl = this.portal.stagingUrl
            || this.getDefaultPortalLocation(DeploymentEnvironment.Staging);
        this.productionUrl = this.portal.productionUrl
            || this.getDefaultPortalLocation(DeploymentEnvironment.Production);
    }

    private initializeActionButtonList(): void {
        let actionButtonList: Array<ActionButton> = [];
        if (this.segment == 'Details'
            && this.permissionService.hasPermission(
                Permission.ManagePortals)) {
            actionButtonList.push(ActionButton.createActionButton(
                "Edit",
                "pencil",
                IconLibrary.AngularMaterial,
                false,
                "Edit Portal",
                true,
                (): any => {
                    return this.userDidTapEditButton();
                },
            ));
        }

        for (let action of this.actions) {
            if (action.actionButtonLabel) {
                actionButtonList.push(ActionButton.createActionButton(
                    action.actionButtonLabel ? action.actionButtonLabel : action.actionName,
                    action.actionIcon,
                    action.iconLibrary,
                    action.actionButtonPrimary,
                    action.actionName,
                    action.actionButtonLabel ? true : false,
                    action.callback
                        ? action.callback
                        : (): Promise<void> => {
                            if (action.portalPageTrigger) {
                                return this.portalExtensionService.executePortalPageTrigger(
                                    action.portalPageTrigger,
                                    this.entityTypes.Portal,
                                    PageType.Display,
                                    this.segment,
                                    this.portalId,
                                );
                            }
                        },
                ));
            }
        }

        this.actionButtonList = ActionButtonHelper.sortActionButtons(actionButtonList);
    }

    private getDefaultPortalLocation(environment: DeploymentEnvironment): string {
        let url: string = this.portal.defaultUrl;
        if (environment.toLowerCase() != DeploymentEnvironment.Production.toLowerCase()) {
            url += '?environment=' + environment.toLowerCase();
        }
        return url;
    }

    private async setAsDefault(isDefault: boolean): Promise<void> {
        await this.sharedLoaderService.presentWithDelay();

        const subscription: Subscription = this.portalApiService.setAsDefault(
            this.portalId, isDefault, this.routeHelper.getContextTenantAlias())
            .pipe(finalize(() => {
                this.sharedLoaderService.dismiss();
                subscription.unsubscribe();
            }))
            .subscribe((portal: PortalDetailResourceModel) => {
                if (portal) { // will be null if we navigate away from the page whilst loading
                    this.eventService.getEntityUpdatedSubject('Portal').next(portal);
                    let message: string;
                    if (isDefault) {
                        message = `The ${portal.name} portal has been set as the default ${portal.userType} portal for `
                            + `${portal.organisationName}`;
                    } else {
                        message = `The ${portal.name} portal is no longer the default ${portal.userType} portal for `
                            + `${portal.organisationName}`;
                    }
                    this.sharedAlertService.showToast(message);
                }
            });
    }

    private async loadEnabledSignInMethodCount(): Promise<void> {
        this.isLoadingSignInMethods = true;
        await this.portalApiService
            .getSignInMethods(this.portalId, this.routeHelper.getContextTenantAlias())
            .pipe(
                takeUntil(this.destroyed),
                finalize(() => this.isLoadingSignInMethods = false))
            .toPromise()
            .then((signInMethods: Array<PortalSignInMethodResourceModel>) => {
                if (signInMethods) { // will be null if we navigate way whilst loading
                    this.enabledSignInMethodsCount = signInMethods
                        .filter((signInMethod: PortalSignInMethodResourceModel) => {
                            return signInMethod.isEnabled;
                        }).length;
                }
            });
    }

    public signInMethodsClicked($event: Event): void {
        const pathSegments: Array<string> = this.routeHelper.appendPathSegment('sign-in-methods');
        this.navProxy.navigateForward(pathSegments);
    }
}
