import { Component, Injector, ElementRef, OnInit, OnDestroy } from '@angular/core';
import { AlertController } from '@ionic/angular';
import { CustomerDetailViewModel } from '@app/viewmodels';
import { AuthenticationService } from '@app/services/authentication.service';
import { CustomerApiService } from '@app/services/api/customer-api.service';
import { FeatureSettingService } from '@app/services/feature-setting.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { PopoverPersonComponent } from '@app/components/popover-person/popover-person.component';
import { contentAnimation } from '@assets/animations';
import { scrollbarStyle } from '@assets/scrollbar';
import { UserStatus } from '@app/models';
import { Permission, PermissionDataModel, StringHelper, LocalDateHelper } from '@app/helpers';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { Subject } from 'rxjs';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { QuoteViewModel } from '@app/viewmodels/quote.viewmodel';
import { PolicyViewModel } from '@app/viewmodels/policy.viewmodel';
import { ClaimViewModel } from '@app/viewmodels/claim.viewmodel';
import { EventService } from '@app/services/event.service';
import { CustomerDetailsResourceModel, CustomerResourceModel } from '@app/resource-models/customer.resource-model';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { QuoteService } from '@app/services/quote.service';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import { AppConfigService } from '@app/services/app-config.service';
import { PopoverTransactionComponent } from '@app/components/popover-transaction/popover-transaction.component';
import { SharedPopoverService } from '@app/services/shared-popover.service';
import { filter, takeUntil } from 'rxjs/operators';
import { DeploymentEnvironment } from '@app/models/deployment-environment.enum';
import { EntityType } from "@app/models/entity-type.enum";
import { PermissionService } from '@app/services/permission.service';
import { ClaimService } from '@app/services/claim.service';
import { AppConfig } from '@app/models/app-config';
import { PersonService } from '@app/services/person.service';
import { PopoverOptions } from '@ionic/core';
import { Router } from '@angular/router';
import { AdditionalPropertyDefinitionApiService } from '@app/services/api/additional-property-definition-api.service';
import { AdditionalPropertyDefinitionContextType } from '@app/models/additional-property-context-type.enum';
import { AdditionalPropertyDefinition } from '@app/models/additional-property-item-view.model';
import { DateHelper } from '@app/helpers/date.helper';
import { PortalPageTriggerResourceModel } from '@app/resource-models/portal-page-trigger.resource-model';
import { PortalExtensionsService } from '@app/services/portal-extensions.service';
import { ActionButtonPopover } from '@app/models/action-button-popover';
import { ActionButton } from '@app/models/action-button';
import { IconLibrary } from '@app/models/icon-library.enum';
import { ActionButtonHelper } from '@app/helpers/action-button.helper';
import { PopoverCommand } from '@app/models/popover-command';
import { PageType } from '@app/models/page-type.enum';

/**
 * Export detail customer page component class
 * This class component manages the display of customer details and the available segments
 * of the customer.
 */
@Component({
    selector: 'app-detail-customer',
    templateUrl: './detail-customer.page.html',
    animations: [contentAnimation],
    styleUrls: [
        './detail-customer.page.scss',
        '../../../../assets/css/scrollbar-detail.css',
    ],
    styles: [scrollbarStyle],
})
export class DetailCustomerPage extends DetailPage implements OnInit, OnDestroy {

    public customerDetailsListItems: Array<DetailsListItem>;
    public segment: string = 'Details';
    public model: CustomerDetailViewModel;
    public permission: typeof Permission = Permission;
    public isMutual: boolean;
    public customerId: string;
    public people: Array<DetailsListItem> = [];
    public tenantAlias: string;
    public hasPurchaseProductFeature: boolean;
    public isLoadingTransactions: boolean = false;
    public transactionsHidden: boolean = false;
    private canViewAdditionalPropertyValues: boolean = false;
    public entityTypes: typeof EntityType = EntityType;
    public isCreateNewQuoteEnabled: boolean;
    public hasQuoteFeature: boolean;
    public hasClaimFeature: boolean;
    public hasPolicyFeature: boolean;
    public hasEmailFeature: boolean;
    public permissionDataModel: PermissionDataModel;
    private hasAdditionalProperty: boolean;
    protected portalPageTriggers: Array<PortalPageTriggerResourceModel>;
    protected actions: Array<ActionButtonPopover> = [];
    protected hasActionsIncludedInMenu: boolean = false;
    public actionButtonList: Array<ActionButton>;
    public canShowMoreButton: boolean = false;
    public flipMoreIcon: boolean = false;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        protected eventService: EventService,
        public navProxy: NavProxyService,
        private routeHelper: RouteHelper,
        private customerApiService: CustomerApiService,
        private authService: AuthenticationService,
        private sharedLoaderService: SharedLoaderService,
        protected alertCtrl: AlertController,
        private quoteService: QuoteService,
        private claimService: ClaimService,
        private featureSettingService: FeatureSettingService,
        public layoutManager: LayoutManagerService,
        protected userPath: UserTypePathHelper,
        elementRef: ElementRef,
        injector: Injector,
        private appConfigService: AppConfigService,
        public sharedPopoverService: SharedPopoverService,
        public permissionService: PermissionService,
        private personService: PersonService,
        private stringHelper: StringHelper,
        private additionalPropertiesService: AdditionalPropertyDefinitionApiService,
        private router: Router,
        private portalExtensionService: PortalExtensionsService,
    ) {
        super(eventService, elementRef, injector);
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.customerId = this.routeHelper.getParam('customerId');

        this.listenForAppConfigUpdates();

        // TODO: Once migration work on UB-4657 is done, we must remove this HIDING mechanism, as well as the 
        // usage of transactionsHidden  variable in the HTML template
        if (this.appConfigService.getEnvironment() === DeploymentEnvironment.Production) {
            this.transactionsHidden = true;
        }
        this.initialized();
    }

    public ngOnDestroy(): void {
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    public async initialized(): Promise<void> {
        this.canViewAdditionalPropertyValues
            = this.permissionService.hasPermission(Permission.ViewAdditionalPropertyValues);
        await this.load();
        this.hasQuoteFeature = this.featureSettingService.hasQuoteFeature();
        this.hasClaimFeature = this.featureSettingService.hasClaimFeature();
        this.hasPolicyFeature = this.featureSettingService.hasPolicyFeature();
        this.hasEmailFeature = this.featureSettingService.hasEmailFeature();
        this.preparePortalExtensions().then(() => this.generatePopoverLinks());
        this.listenForCustomerUpdates();
    }

    private listenForAppConfigUpdates(): void {
        this.appConfigService.appConfigSubject
            .pipe(takeUntil(this.destroyed))
            .subscribe((appConfig: AppConfig) => {
                this.tenantAlias = appConfig.portal.tenantAlias;
                this.hasQuoteFeature = this.featureSettingService.hasQuoteFeature();
                this.hasClaimFeature = this.featureSettingService.hasClaimFeature();
                this.hasPolicyFeature = this.featureSettingService.hasPolicyFeature();
                this.hasEmailFeature = this.featureSettingService.hasEmailFeature();
            });
    }

    private async preparePortalExtensions(): Promise<void> {
        this.portalPageTriggers =
            await this.portalExtensionService.getEnabledPortalPageTriggers(
                this.authService, this.entityTypes.Customer, PageType.Display, this.segment);
    }

    private generatePopoverLinks(): void {
        // Add portal page trigger actions
        this.actions.concat(this.portalExtensionService.getActionButtonPopovers(this.portalPageTriggers));
        this.hasActionsIncludedInMenu = this.actions.filter((x: ActionButtonPopover) => x.includeInMenu).length > 0;
        this.initializeActionButtonList();
    }

    protected executePortalPageTrigger(trigger: PortalPageTriggerResourceModel): void {
        this.portalExtensionService.executePortalPageTrigger(
            trigger,
            this.entityTypes.Customer,
            PageType.Display,
            this.segment,
            this.customerId);
    }

    private updateSegment(): void {
        const segmentParam: string = this.routeHelper.getParam('segment');
        this.segment = segmentParam ? segmentParam : this.segment;
    }

    public quoteSelected(quote: QuoteViewModel): void {
        this.navProxy.navigate([this.userPath.quote, quote.id]);
    }

    public policySelected(policy: PolicyViewModel): void {
        this.navProxy.navigate([this.userPath.policy, policy.id]);
    }

    public claimSelected(claim: ClaimViewModel): void {
        this.navProxy.navigate([this.userPath.claim, claim.id]);
    }

    public async didSelectEdit(): Promise<void> {
        this.navProxy.navigate([this.userPath.customer, this.model.id, 'edit']);
    }

    public async createQuote(): Promise<void> {
        this.quoteService.createQuoteBySelectingProduct(this.model.id);
    }

    public async createClaim(): Promise<void> {
        this.claimService.createClaimBySelectingPolicy(this.model.policies, this.model.id);
    }

    public goToOwner(): void {
        if (this.canAccessOwner(this.model)) {
            if (this.authService.isAgentOrMasterUser()) {
                this.navProxy.navigate([this.userPath.user, this.model.ownerId]);
            }
        }
    }

    public goBack(): void {
        this.navProxy.navigate([this.userPath.customer, 'list']);
    }

    public async showMenu(event: any): Promise<void> {
        if (!this.model) {
            return;
        }
        if (this.segment === 'Transactions') {
            await this.showPaymentPopOver(event);

        } else {
            await this.showCustomerPopover(event);
        }
    }

    public personSelected(id: string): void {
        this.navProxy.navigate([this.userPath.customer, this.model.id, this.userPath.person, id]);
    }

    public createPerson(): void {
        this.navProxy.navigateForward(
            [this.userPath.customer, this.model.id, this.userPath.person, 'create'],
            true,
            { queryParams: { prevPage: 'Customer' } },
        );
    }

    public segmentChanged($event: any): void {
        if ($event.detail.value != this.segment) {
            this.segment = $event.detail.value;

            // Remove any existing segment query params
            this.router.navigate([], {
                queryParams: { 'segment': null },
                queryParamsHandling: 'merge',
            });

            this.preparePortalExtensions().then(() => this.generatePopoverLinks());
            this.canShowMoreButton = this.doesShowMoreButton();
            this.navProxy.updateSegmentQueryStringParameter(
                'segment',
                this.segment != 'Details' ? this.segment : null);
        }
    }

    private listenForCustomerUpdates(): void {
        this.eventService.getEntityUpdatedSubject('Customer')
            .pipe(
                takeUntil(this.destroyed),
                filter((customer: CustomerResourceModel) => customer.id == this.model.id))
            .subscribe((customer: CustomerResourceModel) => this.load());
    }

    private loadPeople(): void {
        this.people = this.model.createPersonDetailsList(this);
    }

    private async load(): Promise<void> {
        this.isLoading = true;
        try {
            const customer: CustomerDetailsResourceModel = await this.customerApiService
                .getCustomerDetails(this.customerId)
                .pipe(takeUntil(this.destroyed))
                .toPromise();
            if (customer) {
                this.model = new CustomerDetailViewModel(customer);
                this.additionalPropertiesService
                    .getAdditionalPropertyDefinitionsByContextTypeAndEntityTypeAndContextIdAndParentContextId(
                        this.tenantAlias,
                        AdditionalPropertyDefinitionContextType.Organisation,
                        EntityType.Customer,
                        this.model.organisationId,
                        this.tenantAlias,
                        true)
                    .pipe(takeUntil(this.destroyed))
                    .subscribe((result: Array<AdditionalPropertyDefinition>) =>
                        this.hasAdditionalProperty = result?.length > 0);

                this.permissionDataModel = {
                    organisationId: this.model.organisationId,
                    ownerUserId: this.model.ownerId,
                    customerId: this.model.id,
                };

                this.isCreateNewQuoteEnabled =
                    this.permissionService.hasElevatedPermissionsViaModel(
                        Permission.ManageQuotes,
                        this.permissionDataModel)
                            ? await this.quoteService.canCreateNewQuote()
                            : false;

                this.model.customerStatus
                    = this.stringHelper.equalsIgnoreCase(this.model.customerStatus, "active") ? "Yes" : "No";
                this.initializeCustomerDetailsListItems();
                setTimeout(() => { // this will refresh the details page.
                    this.isLoading = false;
                }, 100);
                this.loadPeople();
                this.segment = this.routeHelper.getParam('segment') || this.segment;
                this.canShowMoreButton = this.doesShowMoreButton();
            }

            this.updateSegment();
        } catch (error) {
            this.errorMessage = 'There was a problem loading the customer details';
            throw error;
        }
    }

    private initializeCustomerDetailsListItems(): void {
        this.customerDetailsListItems =
            this.model.createCustomerDetailsList(
                this.navProxy,
                this.sharedPopoverService,
                this.canViewAdditionalPropertyValues,
                this.isMutual,
                this.permissionService);
    }

    private canAccessOwner(model: any): boolean {
        return model.referrer && this.featureSettingService.hasUserManagementFeature();
    }

    private async showCustomerPopover(event: any): Promise<void> {
        this.flipMoreIcon = true;
        const email: string = this.model.getAccountEmail();
        const name: string = this.model.fullName;
        const popoverActions: any = {
            createQuote: {
                condition: (): any => false,
                action: (): Promise<void> => this.createQuote(),
                hasLoader: false,
            },
            createClaim: {
                condition: (): any => false,
                action: (): Promise<void> => this.createClaim(),
                hasLoader: false,
            },
            createPerson: {
                condition: (): any => false,
                action: (): void => this.createPerson(),
                hasLoader: false,
            },
            activate: {
                condition: async (): Promise<any> => await this.personService.isValidEmailAddress(email)
                    && await this.personService.confirmCreateUserAccount(email, name),
                action: (): Promise<void> => this.personService
                    .createUserAccount(this.model.primaryPersonId, name, ['Customer'])
                    .then(() => {
                        this.model.userStatus = UserStatus.Invited;
                    }),
                hasLoader: true,
            },
            resendActivate: {
                condition: async (): Promise<any> => await this.personService.isValidEmailAddress(email),
                action: (): Promise<void> => this.personService.resendActivation(this.model.primaryPersonId, name),
                hasLoader: true,
            },
            disable: {
                condition: async (): Promise<any> =>
                    await this.personService.confirmDisableUserAccount(this.model.userStatus, name),
                action: async (): Promise<void> => {
                    const customerResourceModel: CustomerResourceModel =
                        await this.personService.disable(this.model.primaryPersonId, name, ['Customer']);
                    this.model.customerStatus = customerResourceModel.status;
                    this.model.userStatus = customerResourceModel.status === UserStatus.Disabled
                        ? customerResourceModel.status : UserStatus.Active;
                    this.model.lastModifiedDate =
                        DateHelper.formatDDMMMYYYY(customerResourceModel.lastModifiedDateTime);
                    this.model.lastModifiedTime
                        = LocalDateHelper.convertToLocalAndGetTimeOnly(customerResourceModel.lastModifiedDateTime);
                    this.customerDetailsListItems = [];
                    this.customerDetailsListItems = this.model.createCustomerDetailsList(
                        this.navProxy,
                        this.sharedPopoverService,
                        this.canViewAdditionalPropertyValues,
                        this.isMutual,
                        this.permissionService);
                },
                hasLoader: true,
            },
            enable: {
                condition: (): any => true,
                action: async (): Promise<void> => {
                    const dto: CustomerResourceModel =
                        await this.personService.enable(this.model.primaryPersonId, name, ['Customer']);
                    this.model.customerStatus = dto.status;
                    this.model.userStatus = dto.status === UserStatus.Invited ? dto.status : UserStatus.Active;
                    this.model.lastModifiedDate = DateHelper.formatDDMMMYYYY(dto.lastModifiedDateTime);
                    this.model.lastModifiedTime
                        = LocalDateHelper.convertToLocalAndGetTimeOnly(dto.lastModifiedDateTime);
                    this.customerDetailsListItems = [];
                    this.customerDetailsListItems = this.model.createCustomerDetailsList(
                        this.navProxy,
                        this.sharedPopoverService,
                        this.canViewAdditionalPropertyValues,
                        this.isMutual,
                        this.permissionService);
                },
                hasLoader: true,
            },
            edit: {
                condition: (): any => true,
                action: (): Promise<void> => this.didSelectEdit(),
                hasLoader: true,
            },
            editAdditionalPropertyValues: {
                condition: (): any => true,
                action: (): Promise<void> => this.navProxy.goToAdditionalPropertyValues(EntityType.Customer),
                hasLoader: true,
            },
        };
        const dismissAction = async (command: any): Promise<void> => {
            this.flipMoreIcon = false;
            if (!(command && command.data)) {
                // no option was selected.
                return;
            }
            let actionName: string = command.data.action.actionName;
            const isCustomerActionConfirmed: boolean = await popoverActions[actionName].condition();
            if (Object.prototype.hasOwnProperty.call(popoverActions, actionName)
                && isCustomerActionConfirmed) {
                try {
                    await this.sharedLoaderService.presentWait();
                    await popoverActions[actionName].action();
                    this.eventService.customerUpdated();
                } finally {
                    await this.sharedLoaderService.dismiss();
                }
            } else if (Object.prototype.hasOwnProperty.call(popoverActions, actionName)
                && !isCustomerActionConfirmed
                && !popoverActions[actionName].hasLoader) {
                await popoverActions[actionName].action();
            } else {
                if (command.data.action && command.data.action.portalPageTrigger) {
                    this.executePortalPageTrigger(command.data.action.portalPageTrigger);
                }
            }
        };
        const canManageCustomer: boolean = this.permissionService.hasElevatedPermissionsViaModel(
            Permission.ManageCustomers, this.permissionDataModel);
        const canManageAllCustomers: boolean = this.permissionService.hasElevatedPermissionsViaModel(
            Permission.ManageAllCustomers, this.permissionDataModel);
        const canManageAllCustomersForAllOrganisations: boolean = this.permissionService.hasElevatedPermissionsViaModel(
            Permission.ManageAllCustomersForAllOrganisations, this.permissionDataModel);

        const canEditAdditionalPropertyValues: boolean = this.hasAdditionalProperty
            && this.permissionService.hasPermission(Permission.EditAdditionalPropertyValues);
        const isDefaultOptionsEnabled: boolean = this.segment.toLowerCase() == "details";
        const canManageClaims: boolean = this.permissionService.hasElevatedPermissionsViaModel(
            Permission.ManageClaims,
            this.permissionDataModel);

        const canManageCustomerPermissions: boolean = canManageCustomer
            || canManageAllCustomers
            || canManageAllCustomersForAllOrganisations;

        const options: PopoverOptions<any> = {
            component: PopoverPersonComponent,
            cssClass: 'custom-popover more-button-top-popover-positioning',
            componentProps: {
                segment: this.segment,
                status: this.model.customerStatus,
                newStatusTitle: 'Create user account',
                isDefaultOptionsEnabled: isDefaultOptionsEnabled,
                actions: this.actions,
                entityType: EntityType.Customer,
                shouldShowPopOverEdit: canManageCustomerPermissions,
                shouldShowPopOverEditAdditionalProperties:
                    canEditAdditionalPropertyValues && canManageCustomerPermissions,
                shouldShowPopOverNewStatus: this.model.userStatus === UserStatus.New
                    && canManageCustomerPermissions,
                shouldShowPopOverResendStatus: this.model.userStatus === UserStatus.Invited
                    && canManageCustomerPermissions,
                shouldShowPopOverDisableStatus: (this.model.userStatus === UserStatus.Active
                    || this.model.userStatus === UserStatus.Invited)
                    && canManageCustomerPermissions,
                shouldShowPopOverEnableStatus: (this.model.userStatus === UserStatus.Deactivated
                    || this.model.userStatus === UserStatus.Disabled)
                    && canManageCustomerPermissions,
                shouldShowPopOverCreateQuote: this.hasQuoteFeature && this.isCreateNewQuoteEnabled,
                shouldShowPopOverCreateClaim: this.hasClaimFeature && canManageClaims,
                shouldShowPopOverCreatePerson: canManageCustomerPermissions,
            },
            event: event,
        };

        await this.sharedPopoverService.show(options, 'Person option popover', dismissAction);
    }

    private async showPaymentPopOver(event: any): Promise<void> {
        this.flipMoreIcon = true;
        const popoverActions: any = {
            createPayment: (): Promise<void> => this.createNewPaymentForCustomer(this.model.id),
            createRefund: (): Promise<void> => this.createNewRefundForCustomer(this.model.id),
        };
        const dismissAction = async (command: PopoverCommand): Promise<void> => {
            this.flipMoreIcon = false;
            if (!(command && command.data)) {
                // no option was selected.
                return;
            }
            if (Object.prototype.hasOwnProperty.call(popoverActions, command.data.action.actionName)) {
                popoverActions[command.data.action.actionName]();
            } else {
                const actionItemLabel: string = command.data.action.actionName;
                if (actionItemLabel) {
                    this.executePortalPageTrigger(command.data.action.portalPageTrigger);
                }
            }
        };
        const options: PopoverOptions<any> = {
            component: PopoverTransactionComponent,
            cssClass: 'custom-popover more-button-top-popover-positioning',
            componentProps: {
                shouldShowPopOverCreatePayment: true,
                shouldShowPopOverCreateRefund: true,
                actions: this.actions,
            },
            event: event,
        };
        await this.sharedPopoverService.show(options, 'Accounting Transactions option popover', dismissAction);
    }

    private async createNewPaymentForCustomer(customerId: string): Promise<void> {
        this.navProxy.navigate([this.userPath.customer, customerId, 'payment', 'create']);
    }

    private async createNewRefundForCustomer(customerId: string): Promise<void> {
        this.navProxy.navigate([this.userPath.customer, customerId, 'refund', 'create']);
    }

    private initializeActionButtonList(): void {
        let actionButtonList: Array<ActionButton> = [];
        if (this.permissionDataModel == null) {
            return;
        }

        if (this.hasQuoteFeature
            && this.segment === 'Quotes'
            && this.isCreateNewQuoteEnabled) {
            actionButtonList.push(ActionButton.createActionButton(
                "Create",
                "plus",
                IconLibrary.AngularMaterial,
                false,
                "Create Quote",
                true,
                (): Promise<void> => {
                    return this.createQuote();
                },
            ));
        }

        if (this.hasClaimFeature
            && this.segment === 'Claims'
            && this.permissionService.hasElevatedPermissionsViaModel(
                Permission.ManageClaims,
                this.permissionDataModel)) {
            actionButtonList.push(ActionButton.createActionButton(
                "Create",
                "plus",
                IconLibrary.AngularMaterial,
                false,
                "Create Claim",
                true,
                (): Promise<void> => {
                    return this.createClaim();
                },
            ));
        }
        const canManageCustomer: boolean = this.permissionService.hasElevatedPermissionsViaModel(
            Permission.ManageCustomers,
            this.permissionDataModel);
        const canManageAllCustomers: boolean = this.permissionService.hasElevatedPermissionsViaModel(
            Permission.ManageAllCustomers, this.permissionDataModel);
        const canManageAllCustomersForAllOrganisations: boolean = this.permissionService.hasElevatedPermissionsViaModel(
            Permission.ManageAllCustomersForAllOrganisations, this.permissionDataModel);
        const canManageCustomerPermissions: boolean = canManageCustomer
            || canManageAllCustomers
            || canManageAllCustomersForAllOrganisations;

        if (this.segment === 'Details' && this.model != null && canManageCustomerPermissions) {
            actionButtonList.push(ActionButton.createActionButton(
                "Edit",
                "pencil",
                IconLibrary.AngularMaterial,
                false,
                "Edit Customer",
                true,
                (): Promise<void> => {
                    return this.didSelectEdit();
                },
            ));
        }

        if (this.segment === 'People' && canManageCustomerPermissions) {
            actionButtonList.push(ActionButton.createActionButton(
                "Create",
                "plus",
                IconLibrary.AngularMaterial,
                false,
                "Create Person",
                true,
                (): void => {
                    return this.createPerson();
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
                            this.entityTypes.Customer,
                            PageType.Display,
                            this.segment,
                            this.customerId);
                    },
                ));
            }
        }

        this.actionButtonList = ActionButtonHelper.sortActionButtons(actionButtonList);
    }

    private doesShowMoreButton(): boolean {
        const canManageUser: boolean = this.permissionService.hasElevatedPermissionsViaModel(
            Permission.ManageUsers, this.permissionDataModel);
        const canManageCustomer: boolean = this.permissionService.hasElevatedPermissionsViaModel(
            Permission.ManageCustomers, this.permissionDataModel);
        const canManageAllCustomers: boolean = this.permissionService.hasElevatedPermissionsViaModel(
            Permission.ManageAllCustomers, this.permissionDataModel);
        const canManageAllCustomersForAllOrganisations: boolean = this.permissionService.hasElevatedPermissionsViaModel(
            Permission.ManageAllCustomersForAllOrganisations, this.permissionDataModel);
        const canManageCustomerPermissions: boolean = canManageCustomer
            || canManageAllCustomers
            || canManageAllCustomersForAllOrganisations;
        const canEditAdditionalPropertyValues: boolean = this.hasAdditionalProperty
            && this.permissionService.hasPermission(Permission.EditAdditionalPropertyValues);
        const canManageClaims: boolean = this.permissionService.hasElevatedPermissionsViaModel(
            Permission.ManageClaims,
            this.permissionDataModel);

        return (this.segment == 'Details' && ((this.model.userStatus === UserStatus.Invited && canManageUser)
            || (this.model.userStatus === UserStatus.New && canManageCustomerPermissions)
            || ((this.model.userStatus === UserStatus.Active
                || this.model.userStatus === UserStatus.Invited)
                && canManageCustomerPermissions)
            || ((this.model.userStatus === UserStatus.Deactivated
                || this.model.userStatus === UserStatus.Disabled)
                && canManageCustomerPermissions)
            || (canEditAdditionalPropertyValues && canManageCustomerPermissions)))
            || (this.segment == 'People' && canManageCustomerPermissions)
            || (this.segment == 'Quotes' && (this.hasQuoteFeature && this.isCreateNewQuoteEnabled))
            || (this.segment == 'Claims' && (this.hasClaimFeature && canManageClaims));
    }
}
