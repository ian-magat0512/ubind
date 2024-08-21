import { Component, OnDestroy, Injector, ElementRef, OnInit } from '@angular/core';
import { PopoverController, AlertController, ToastController, LoadingController } from '@ionic/angular';
import { AuthenticationService } from '@app/services/authentication.service';
import { CustomerApiService } from '@app/services/api/customer-api.service';
import { FeatureSettingService } from '@app/services/feature-setting.service';
import { PolicyApiService } from '@app/services/api/policy-api.service';
import { QuoteApiService } from '@app/services/api/quote-api.service';
import { TenantApiService } from '@app/services/api/tenant-api.service';
import { UserApiService } from '@app/services/api/user-api.service';
import { PolicyDetailViewModel } from '@app/viewmodels/policy-detail.viewmodel';
import { Permission, PolicyHelper, PermissionDataModel } from '@app/helpers';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { PopoverPolicyPage } from '../popover-policy/popover-policy.page';
import { contentAnimation } from '@assets/animations';
import { scrollbarStyle } from '@assets/scrollbar';
import { PolicyStatus, UserStatus } from '@app/models';
import { CustomerResourceModel } from '@app/resource-models/customer.resource-model';
import { Subject } from 'rxjs';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { EventService } from '@app/services/event.service';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { PolicyService } from '@app/services/policy.service';
import {
    PolicyDetailResourceModel,
    PolicyResourceModel,
    PolicyTransactionDetailResourceModel,
    PolicyTransactionResourceModel,
} from '@app/resource-models/policy.resource-model';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { CustomerService } from '@app/services/customer.service';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { QuoteType } from '@app/models/quote-type.enum';
import { SharedPopoverService } from '@app/services/shared-popover.service';
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import { finalize, takeUntil } from 'rxjs/operators';
import { QuestionViewModel } from '@app/viewmodels/question-view.viewmodel';
import { RepeatingQuestionViewModel } from '@app/viewmodels/repeating-question.viewmodel';
import { QuestionViewModelGenerator } from '@app/helpers/question-view-model-generator';
import { ProductFeatureSettingService } from '@app/services/product-feature-service';
import { ProductFeatureSetting } from '@app/models/product-feature-setting';
import { EntityType } from "@app/models/entity-type.enum";
import { Errors } from '@app/models/errors';
import { UserService } from '@app/services/user.service';
import { PermissionService } from '@app/services/permission.service';
import { AppConfigService } from '@app/services/app-config.service';
import { AppConfig } from '@app/models/app-config';
import { PortalExtensionsService } from '@app/services/portal-extensions.service';
import { ActionButton } from '@app/models/action-button';
import { ActionButtonPopover } from '@app/models/action-button-popover';
import { IconLibrary } from '@app/models/icon-library.enum';
import { ActionButtonHelper } from '@app/helpers/action-button.helper';
import { PortalPageTriggerResourceModel } from '@app/resource-models/portal-page-trigger.resource-model';
import { PopoverCommand } from '@app/models/popover-command';
import { PageType } from '@app/models/page-type.enum';
import { OrganisationApiService } from "@app/services/api/organisation-api.service";

/**
 * Export detail policy page component class.
 * This class is used to display policy details like policy premium, 
 * customer, transactions history, policy document, and all 
 * other policy related policy information
 */
@Component({
    selector: 'app-detail-policy',
    templateUrl: './detail-policy.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-detail.css',
    ],
    styles: [scrollbarStyle],
})
export class DetailPolicyPage extends DetailPage implements OnInit, OnDestroy {
    public detail: PolicyDetailViewModel;
    public canShowCreateClaim: boolean;
    public canShowMoreButton: boolean;
    public isMutual: boolean;

    public policyId: string;
    public policyNumber: string;
    public productId: string;
    public quoteId: string;
    public segment: string = 'Details';
    public status: string;

    public displayType: string = QuestionViewModelGenerator.type.Policy;
    public hasClaimConfigurationAndPortalFeature: boolean = false;
    public hasEmailFeature: boolean;
    public hasClaimFeature: boolean;
    public loadingPolicy: boolean = false;
    public questionItems: Array<QuestionViewModel>;
    public repeatingQuestionItems: Array<RepeatingQuestionViewModel>;
    public canEditAdditionalPropertyValues: boolean = false;
    public permission: typeof Permission = Permission;
    private modesMapping: any = null;
    public detailsListItems: Array<DetailsListItem>;
    public actionButtonList: Array<ActionButton>;
    public entityTypes: typeof EntityType = EntityType;
    public permissionModel: PermissionDataModel;
    public portalPageTriggers: Array<PortalPageTriggerResourceModel>;
    protected actions: Array<ActionButtonPopover> = [];
    protected hasActionsIncludedInMenu: boolean = false;
    private productFeatureSetting: ProductFeatureSetting;
    public flipMoreIcon: boolean = false;
    private allowOrganisationRenewalInvitation: boolean;

    public constructor(
        public navProxy: NavProxyService,
        public policyApiService: PolicyApiService,
        public layoutManager: LayoutManagerService,
        protected routeHelper: RouteHelper,
        protected quoteApiService: QuoteApiService,
        protected policyService: PolicyService,
        protected customerService: CustomerService,
        protected authService: AuthenticationService,
        protected loadCtrl: LoadingController,
        protected popOverCtrl: PopoverController,
        protected alertCtrl: AlertController,
        protected toastCtrl: ToastController,
        protected featureSettingService: FeatureSettingService,
        protected customerApiService: CustomerApiService,
        protected tenantApiService: TenantApiService,
        protected userApiService: UserApiService,
        protected sharedAlertService: SharedAlertService,
        protected sharedLoaderService: SharedLoaderService,
        protected eventService: EventService,
        protected userPath: UserTypePathHelper,
        protected productFeatureService: ProductFeatureSettingService,
        elementRef: ElementRef,
        injector: Injector,
        private userService: UserService,
        private sharedPopoverService: SharedPopoverService,
        private permissionService: PermissionService,
        private appConfigService: AppConfigService,
        private portalExtensionService: PortalExtensionsService,
        private organisationApiService: OrganisationApiService,
    ) {
        super(eventService, elementRef, injector);
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.hasEmailFeature = this.featureSettingService.hasEmailFeature();
            this.hasClaimFeature = this.featureSettingService.hasClaimFeature();
        });
    }

    public async ngOnInit(): Promise<void> {
        this.destroyed = new Subject<void>();
        this.policyId = this.routeHelper.getParam('policyId');
        this.productId = this.routeHelper.getParam('productid');
        this.isMutual = this.authService.isMutualTenant();
        this.isLoading = false;
        this.hasClaimConfigurationAndPortalFeature = false;

        let _this: this = this;
        this.modesMapping = {
            "renew": {
                title: "Renew Policy",
                quoteType: QuoteType.Renewal,
                action: (): any => {
                    return _this.policyService.renewPolicy(_this.policyId);
                },
            },
            "adjust": {
                title: "Adjust Policy",
                quoteType: QuoteType.Adjustment,
                action: (): any => {
                    return _this.policyService.adjustPolicy({
                        id: _this.policyId,
                        ..._this.detail as any,
                    } as PolicyResourceModel);
                },
            },
            "cancel": {
                title: "Cancel Policy",
                quoteType: QuoteType.Cancellation,
                action: (): any => {
                    return _this.policyService.cancelPolicy(<PolicyResourceModel><any>_this.detail);
                },
            },
            "send": {
                action: (): any => {
                    return _this.processSendRenewal();
                },
            },
            "claim": {
                action: (): any => {
                    return _this.createClaim();
                },
            },
            "updatePolicyNumber": {
                action: (): any => {
                    return _this.updatePolicyNumber();
                },
            },
        };

        await this.loadDetails();
        this.hasEmailFeature = this.featureSettingService.hasEmailFeature();
        this.hasClaimFeature = this.featureSettingService.hasClaimFeature();

        this.segment = this.routeHelper.getParam('segment') || this.segment;
    }

    public ngOnDestroy(): void {
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
        this.policyId = null;
    }

    protected async preparePortalExtensions(): Promise<void> {
        this.portalPageTriggers =
            await this.portalExtensionService.getEnabledPortalPageTriggers(
                this.authService,
                this.entityTypes.Policy,
                PageType.Display,
                this.segment,
            );
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
            this.entityTypes.Policy,
            PageType.Display,
            this.segment,
            this.policyId,
        );
    }

    public onPolicyHistoryEvent(event: string): void {
        this.policyNumber = this.policyNumber ? this.policyNumber : event;
    }

    public async createClaim(): Promise<void> {
        this.navProxy.navigate([this.userPath.claim, 'create'], {
            queryParams: {
                policyId: this.policyId,
                productAlias: this.productId,
            },
        });
    }

    public async showMenu(event: any): Promise<void> {
        this.flipMoreIcon = true;
        const popoverDismissAction = (command: PopoverCommand): void => {
            this.flipMoreIcon = false;
            if (!(command && command.data)) {
                // none was selected
                return;
            }
            let mapping: any = this.modesMapping[command.data.action.actionName];
            if (mapping) {
                mapping.action();
            } else if (command && command.data && command.data.action && command.data.action.portalPageTrigger) {
                this.portalExtensionService.executePortalPageTrigger(
                    command.data.action.portalPageTrigger,
                    this.entityTypes.Policy,
                    PageType.Display,
                    this.segment,
                    this.policyId,
                );
            }

            if (command.data.action.actionName === "editAdditionalPropertyValues" && mapping === undefined) {
                this.sharedAlertService.showWithOk(
                    "No additional properties defined",
                    "Policies in this context do not have any additional properties that can be edited.",
                );
            }
        };
        let canShowAddClaim: boolean = this.canShowAddClaim();
        await this.sharedPopoverService.show(
            {
                component: PopoverPolicyPage,
                event: event,
                cssClass: 'custom-popover more-button-top-popover-positioning',
                componentProps: {
                    status: this.status,
                    numberOfDaysToExpire: this.detail.numberOfDaysToExpire,
                    productId: this.productId,
                    canEditAdditionalPropertyValues: this.canEditAdditionalPropertyValues,
                    permissionModel: this.permissionModel,
                    canShowMoreButton: this.canShowMoreButton,
                    actions: this.actions,
                    shouldAddClaim: canShowAddClaim,
                },
            },
            'Policy option popover',
            popoverDismissAction,
        );
    }

    protected goBack(): void {
        let policySegment: string = PolicyHelper.getTab(this.detail.status, this.authService.isCustomer());
        this.navProxy.navigateBack(
            [this.userPath.policy, 'list'],
            true,
            { queryParams: { segment: policySegment } },
        );
    }

    private processSendRenewal(): void {
        let policyId: string = this.policyId;
        let productId: string = this.productId;
        if (!this.detail.customerDetails) {
            throw Errors.Policy.Renewal.NoCustomerToSendRenewalTo();
        }
        this.policyApiService.getCustomerOfPolicy(this.detail.id)
            .subscribe(
                (customerResourceModel: CustomerResourceModel) => {
                    if (!this.doesCustomerHaveAccount(customerResourceModel)) {
                        this.confirmCreateUserAccountForRenewalInvitation()
                            .then((result: boolean) => {
                                this.sendPolicyRenewalInvitation(policyId, productId, customerResourceModel, result);
                            });
                    } else {
                        this.sendPolicyRenewalInvitation(policyId, productId, customerResourceModel, true);
                    }
                },
            );
    }

    private sendPolicyRenewalInvitation(
        policyId: string,
        productId: string,
        customerResourceModel: CustomerResourceModel,
        shouldHaveAUserAccount: boolean): void {
        this.policyApiService.sendPolicyRenewalInvitation(policyId, productId, shouldHaveAUserAccount)
            .subscribe(
                async () => {
                    const isMutualTenant: boolean = this.authService.isMutualTenant();
                    if (!this.doesCustomerHaveAccount(customerResourceModel) && shouldHaveAUserAccount) {
                        await this.sharedAlertService.showToast(`A customer account was created for `
                            + `${customerResourceModel.fullName} and a `
                            + `renewal invitation email was sent`);
                    } else if (!this.doesCustomerHaveAccount(customerResourceModel) && !shouldHaveAUserAccount) {
                        await this.sharedAlertService.showToast(`A renewal invite email has been sent for `
                            + `${customerResourceModel.fullName}`);
                    } else if (this.doesCustomerHaveAccountButNoActivationEmailSent(customerResourceModel)) {
                        await this.sharedAlertService.showToast(`${isMutualTenant ? 'Protection' : 'Policy'} `
                            + `renewal invitation email was sent to ${customerResourceModel.fullName}`);
                    } else if (this.wasCustomerInvitedToActivateButAccountNotYetActivated(
                        customerResourceModel,
                    )) {
                        await this.sharedAlertService.showToast(`${isMutualTenant ? 'Protection' : 'Policy'} `
                            + `renewal invitation email was sent to ${customerResourceModel.fullName}`);
                    } else if (this.doesCustomerHaveAccountWhichHasBeenActivated(customerResourceModel)) {
                        await this.sharedAlertService.showToast(`${isMutualTenant ? 'Protection' : 'Policy'} `
                            + `renewal invitation email was sent to ${customerResourceModel.fullName}`);
                    }
                },
            );
    }

    private confirmCreateUserAccountForRenewalInvitation(): Promise<boolean> {
        return new Promise((resolve: any): any => {
            this.sharedAlertService.showWithActionHandler({
                header: 'Create a user account?',
                subHeader:
                    `This customer doesn't have a user account. If you would like them to renew using the customer `
                    + `portal, you can create a user account now. If not, the customer would need to renew their `
                    + "policy outside of the portal. Would you like to create a user account?",
                buttons: [
                    {
                        text: 'No',
                        handler: (): any => {
                            resolve(false);
                        },
                    },
                    {
                        text: 'Yes',
                        handler: (): any => {
                            resolve(true);
                        },
                    },
                ],
            });
        });
    }

    private doesCustomerHaveAccount(customer: CustomerResourceModel): boolean {
        return customer.userId != null;
    }

    private doesCustomerHaveAccountButNoActivationEmailSent(
        customer: CustomerResourceModel,
    ): boolean {
        return this.doesCustomerHaveAccount(customer) && customer.status == UserStatus.New;
    }

    private wasCustomerInvitedToActivateButAccountNotYetActivated(
        customer: CustomerResourceModel,
    ): boolean {
        return this.doesCustomerHaveAccount(customer) && customer.status == UserStatus.Invited;
    }

    private doesCustomerHaveAccountWhichHasBeenActivated(
        customer: CustomerResourceModel,
    ): boolean {
        return this.doesCustomerHaveAccount(customer) && customer.status == UserStatus.Active;
    }

    public async segmentChanged($event: any): Promise<void> {
        if ($event.detail.value != this.segment) {
            this.segment = $event.detail.value;
            await this.loadQuestionSegment();
            this.navProxy.updateSegmentQueryStringParameter(
                'segment',
                this.segment != 'Details' ? this.segment : null,
            );
        }
    }

    private async hasRenewAndAdjustAndCancelProductFeatureSettingEnabled(): Promise<boolean> {
        const hasRenewOrAdjustOrCancelFeature: boolean =
            this.productFeatureSetting
            && (this.productFeatureSetting.areRenewalQuotesEnabled
                || this.productFeatureSetting.areAdjustmentQuotesEnabled
                || this.productFeatureSetting.areCancellationQuotesEnabled);
        return hasRenewOrAdjustOrCancelFeature;
    }

    private async loadQuestionSegment(): Promise<void> {
        switch (this.segment) {
            case 'Questions':
                this.loadRecentNonCancellationTransaction(this.policyId);
                break;
            case 'History':
                await this.loadDetails();
                break;
            default:
                break;
        }

        this.preparePortalExtensions().then(() => this.generatePopoverLinks());
    }

    private async doesShowMoreButton(): Promise<boolean> {
        const productFeatureEnabled: boolean = await
        this.hasRenewAndAdjustAndCancelProductFeatureSettingEnabled();
        const isRenewalAllowedAfterExpiry: boolean =
            await this.productFeatureService.isAllowedForRenewalAfterExpiry(
                this.detail.productId,
                this.detail.numberOfDaysToExpire,
            );
        const doesShowMoreButton: boolean = PolicyDetailViewModel.doesShowMoreButton(
            this.detail,
            this.segment,
            productFeatureEnabled,
            isRenewalAllowedAfterExpiry)
            && this.permissionService
                .hasElevatedPermissionsViaModel(Permission.ManagePolicies, this.permissionModel);
        const canShowUpdatePolicyNumber: boolean = this.permissionService
            .hasElevatedPermissionsViaModel(Permission.ManagePolicyNumbers, this.permissionModel);
        return doesShowMoreButton || canShowUpdatePolicyNumber;
    }

    private async loadDetails(): Promise<void> {
        if (this.isLoading) {
            return;
        }

        this.isLoading = true;
        try {
            const policyDetail: PolicyDetailResourceModel =
                await this.policyApiService.getPolicyBaseDetails(this.policyId).toPromise();
            this.detail = new PolicyDetailViewModel(policyDetail);
            this.permissionModel = {
                organisationId: this.detail.organisationId,
                customerId: this.detail.customerDetails != null ? this.detail.customerDetails.id : null,
                ownerUserId: this.detail.owner != null ? this.detail.owner.id : null,
            };
            this.initializeDetailsListItems();
            this.status = this.detail.status;
            this.productId = this.detail.productId;
            this.productFeatureSetting = await this.productFeatureService.getProductFeatureSetting(this.productId);
            this.policyNumber = this.detail.policyNumber;
            this.quoteId = this.detail.quoteId;
            this.canShowCreateClaim = true;
            this.canEditAdditionalPropertyValues = this.detail.additionalPropertyValues?.length > 0
                && this.permissionService.hasPermission(Permission.EditAdditionalPropertyValues);
            if (this.canEditAdditionalPropertyValues) {
                this.modesMapping["editAdditionalPropertyValues"] = {
                    action: (): any => {
                        return this.navProxy.goToAdditionalPropertyValues(EntityType.Policy);
                    },
                };
            }
            this.canShowMoreButton = await this.doesShowMoreButton();

            this.hasClaimConfigurationAndPortalFeature = policyDetail.hasClaimConfiguration
                && await this.permissionService.hasViewClaimPermission();

            this.preparePortalExtensions().then(() => this.generatePopoverLinks());
        } catch (error) {
            this.errorMessage = `There was a problem loading the `
                + `${this.authService.isMutualTenant() ? 'protection' : 'policy'} details`;
            throw error;
        } finally {
            this.isLoading = false;
        }
    }

    private initializeDetailsListItems(): void {
        this.detailsListItems = this.detail.createDetailsList(
            this.navProxy,
            this.authService.isCustomer(),
            this.authService.isMutualTenant(),
        );
    }

    private loadPolicyTransactionDetails(policyId: string, transactionId: string): void {
        this.isLoading = true;
        this.policyApiService.getPolicyTransaction(policyId, transactionId)
            .pipe(
                finalize(() => {
                    this.isLoading = false;
                }),
                takeUntil(this.destroyed),
            )
            .subscribe(async (dt: PolicyTransactionDetailResourceModel) => {
                let formModel: any = dt.formData["formModel"] ? dt.formData["formModel"] : null;
                this.questionItems = QuestionViewModelGenerator.getFormDataQuestionItems(
                    formModel,
                    dt.questionAttachmentKeys,
                    dt.displayableFields,
                );
                this.repeatingQuestionItems = QuestionViewModelGenerator.getRepeatingQuestionSets(
                    formModel,
                    dt.displayableFields,
                    dt.questionAttachmentKeys,
                );
                this.canShowMoreButton = await this.doesShowMoreButton();
            });
    }

    private loadRecentNonCancellationTransaction(policyId: string): void {
        this.isLoading = true;
        this.policyApiService.getPolicyHistoryList(policyId)
            .pipe(takeUntil(this.destroyed))
            .subscribe((dt: Array<PolicyTransactionResourceModel>) => {
                if (dt) {
                    const historyData: Array<PolicyTransactionResourceModel> =
                        dt.filter((dt: PolicyTransactionResourceModel) =>
                            dt.eventTypeSummary != PolicyHelper.constants.Labels.Status.Cancelled)
                            .sort((a: PolicyTransactionResourceModel, b: PolicyTransactionResourceModel) =>
                                PolicyHelper.sortDate(a, b));
                    const transactionId: string = historyData[0].transactionId;
                    this.loadPolicyTransactionDetails(this.policyId, transactionId);
                }
            });
    }

    private async initializeActionButtonList(): Promise<void> {
        if (!this.detail) {
            return;
        }

        const hasManagePoliciesPermission: boolean =
            this.permissionService.hasElevatedPermissionsViaModel(Permission.ManagePolicies, this.permissionModel);
        let actionButtonList: Array<ActionButton> = [];
        const isAllowedForRenewalAfterExpiry: boolean =
            await this.productFeatureService.isAllowedForRenewalAfterExpiry(
                this.productId,
                this.detail.numberOfDaysToExpire,
            );
        let canRenewPolicy: boolean = (this.arePolicyChangesAllowed()
            || (this.status == PolicyStatus.Expired && isAllowedForRenewalAfterExpiry))
            && this.productFeatureSetting.areRenewalQuotesEnabled
            && hasManagePoliciesPermission;
        let canRenewalIsPrimary: boolean = this.detail.numberOfDaysToExpire <= 60
            && this.permissionService
                .hasElevatedPermissionsViaModel(Permission.ManagePolicies, this.permissionModel);
        const policyLabel: string = !this.isMutual ? 'Policy' : 'Protection';
        if (canRenewPolicy) {
            actionButtonList.push(ActionButton.createActionButton(
                "Renew",
                "renew",
                IconLibrary.AngularMaterial,
                canRenewalIsPrimary,
                `Renew ${policyLabel}`,
                true,
                (): Promise<void> => {
                    return this.policyService.renewPolicy(this.policyId);
                },
            ));
        }

        let canAdjustPolicy: boolean = (this.arePolicyChangesAllowed()
            && this.productFeatureSetting.areAdjustmentQuotesEnabled
            && hasManagePoliciesPermission);
        if (canAdjustPolicy) {
            actionButtonList.push(ActionButton.createActionButton(
                "Adjust",
                "pencil",
                IconLibrary.AngularMaterial,
                false,
                `Adjust ${policyLabel}`,
                true,
                (): Promise<void> => {
                    return this.policyService.adjustPolicy({
                        id: this.policyId,
                        ...this.detail as any,
                    } as PolicyResourceModel);
                },
            ));
        }

        let canCancelPolicy: boolean = (this.arePolicyChangesAllowed()
            && this.productFeatureSetting.areCancellationQuotesEnabled
            && hasManagePoliciesPermission);
        if (canCancelPolicy) {
            actionButtonList.push(ActionButton.createActionButton(
                "Cancel",
                "cancel",
                IconLibrary.AngularMaterial,
                false,
                `Cancel ${policyLabel}`,
                true,
                (): Promise<void> => {
                    return this.policyService.cancelPolicy(<PolicyResourceModel><any> this.detail);
                },
            ));
        }

        let canShowAddClaim: boolean = this.canShowAddClaim();
        if (canShowAddClaim) {
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
                            this.entityTypes.Policy,
                            PageType.Display,
                            this.segment,
                            this.policyId,
                        );
                    },
                ));
            }
        }

        this.actionButtonList = ActionButtonHelper.sortActionButtons(actionButtonList);
    }

    private arePolicyChangesAllowed(): boolean {
        return this.status !== PolicyStatus.Expired && this.status !== PolicyStatus.Cancelled;
    }

    private canShowAddClaim(): boolean {
        return this.segment === 'Claims'
            && this.canShowCreateClaim
            && this.permissionService
                .hasElevatedPermissionsViaModel(Permission.ManageClaims, this.permissionModel);
    }

    private async updatePolicyNumber(): Promise<void> {
        this.navProxy.navigate([this.userPath.policy, this.policyId, 'update-number', this.policyNumber]);
    }
}
