import { Component, OnInit, OnDestroy } from '@angular/core';
import { NavParams, PopoverController } from '@ionic/angular';
import { PolicyStatus } from '@app/models/policy-status.enum';
import { Permission, PermissionDataModel } from '@app/helpers/permissions.helper';
import { AuthenticationService } from '@app/services/authentication.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { ProductFeatureSettingService } from '@app/services/product-feature-service';
import { SubscriptionLike } from 'rxjs';
import { EventService } from '@app/services/event.service';
import { ProductFeatureSetting } from '@app/models/product-feature-setting';
import { PermissionService } from '@app/services/permission.service';
import { ActionButtonPopover } from '@app/models/action-button-popover';
import { IconLibrary } from '@app/models/icon-library.enum';
import { OrganisationApiService } from "@app/services/api/organisation-api.service";

/**
 * Popover for policy page that is used in policy adjustment, renewal, cancellation etc.
 */
@Component({
    selector: 'app-popover-policy',
    templateUrl: './popover-policy.page.html',
})
export class PopoverPolicyPage implements OnInit, OnDestroy {
    public status: string = '';
    public numberOfDaysToExpire: number;
    public permission: typeof Permission = Permission;
    public permissionModel: PermissionDataModel;
    public portalTenantAlias: string;
    public hasSendRenewalEmail: boolean;
    public isMutual: boolean;
    public canEditAdditionalPropertyValues: boolean = false;
    public canRenewPolicy: boolean;
    public canAdjustPolicy: boolean;
    public canCancelPolicy: boolean;
    public canSendRenewalInvitation: boolean;
    public canAdjustProtection: boolean;
    public canCancelProtection: boolean;
    public canRenewProtection: boolean;
    public canShowMoreButton: boolean;
    public allowOrganisationRenewalInvitation: boolean;
    public actions: Array<ActionButtonPopover> = [];
    public shouldAddClaim: boolean = false;
    public iconLibrary: typeof IconLibrary = IconLibrary;
    public canUpdatePolicyNumber: boolean = false;

    private productId: string;
    private productFeatureSetting: ProductFeatureSetting;
    protected subscriptions: Array<SubscriptionLike> = new Array<SubscriptionLike>();
    public constructor(
        public popOverCtrl: PopoverController,
        navParams: NavParams,
        private authService: AuthenticationService,
        private productFeatureService: ProductFeatureSettingService,
        private routeHelper: RouteHelper,
        protected eventService: EventService,
        private permissionService: PermissionService,
        private organisationApiService: OrganisationApiService,
    ) {
        this.status = navParams.get('status');
        this.canShowMoreButton = navParams.get('canShowMoreButton');
        this.actions = navParams.get('actions');
        this.shouldAddClaim = navParams.get('shouldAddClaim');
        this.canEditAdditionalPropertyValues = navParams.get('canEditAdditionalPropertyValues');
        this.permissionModel = navParams.get('permissionModel');
        this.numberOfDaysToExpire = navParams.get('numberOfDaysToExpire');
        this.productId = navParams.get('productId');
    }

    public async ngOnInit(): Promise<void> {
        this.portalTenantAlias = this.routeHelper.getParam('portalTenantAlias');
        this.productFeatureSetting = await this.productFeatureService.getProductFeatureSetting(this.productId);
        this.setHideOrShowMenu();
        this.isMutual = this.authService.isMutualTenant();
    }

    public ngOnDestroy(): void {
        this.subscriptions.forEach((s: SubscriptionLike) => s.unsubscribe());
    }

    private arePolicyChangesAllowed(): boolean {
        return this.status !== PolicyStatus.Expired && this.status !== PolicyStatus.Cancelled;
    }

    public async setHideOrShowMenu(): Promise<void> {
        const isAllowedForRenewalAfterExpiry: boolean =
            await this.productFeatureService.isAllowedForRenewalAfterExpiry(this.productId, this.numberOfDaysToExpire);
        this.canRenewPolicy = ((this.arePolicyChangesAllowed()
            || (this.status == PolicyStatus.Expired && isAllowedForRenewalAfterExpiry)) &&
            this.productFeatureSetting.areRenewalQuotesEnabled) && !this.isMutual;

        this.canRenewProtection = ((this.arePolicyChangesAllowed()
            || (this.status == PolicyStatus.Expired && isAllowedForRenewalAfterExpiry)) &&
            this.productFeatureSetting.areRenewalQuotesEnabled) && this.isMutual;

        this.canAdjustPolicy = (this.arePolicyChangesAllowed() &&
            this.productFeatureSetting.areAdjustmentQuotesEnabled) && !this.isMutual;
        this.canAdjustProtection = (this.arePolicyChangesAllowed() &&
            this.productFeatureSetting.areCancellationQuotesEnabled) && this.isMutual;
        this.canCancelPolicy = (this.arePolicyChangesAllowed() &&
            this.productFeatureSetting.areCancellationQuotesEnabled) && !this.isMutual;
        this.canCancelProtection = (this.arePolicyChangesAllowed() &&
            this.productFeatureSetting.areCancellationQuotesEnabled) && this.isMutual;

        this.canSendRenewalInvitation =
            await this.areConditionsMetToBeAbleToSendRenewalInvitation()
            && this.permissionService
                .hasElevatedPermissionsViaModel(Permission.ManagePolicies, this.permissionModel)
            && this.permissionService
                .hasPermission(Permission.ManageMessages, true);

        this.canUpdatePolicyNumber = this.arePolicyChangesAllowed()
            && this.permissionService
                .hasElevatedPermissionsViaModel(Permission.ManagePolicies, this.permissionModel)
            && this.permissionService
                .hasElevatedPermissionsViaModel(Permission.ManagePolicyNumbers, this.permissionModel);
    }

    public async areConditionsMetToBeAbleToSendRenewalInvitation(): Promise<boolean> {
        const isAllowedForRenewalAfterExpiry: boolean =
            await this.productFeatureService.isAllowedForRenewalAfterExpiry(this.productId, this.numberOfDaysToExpire);

        let isCustomer: boolean = this.authService.isCustomer();

        if (!isCustomer && (this.portalTenantAlias === 'demo' ||
            this.portalTenantAlias === 'demos') && this.status &&
            this.productFeatureSetting.areRenewalQuotesEnabled) {
            return true;
        }

        this.allowOrganisationRenewalInvitation = await this.organisationApiService
            .isSendingRenewalInvitationsAllowed(this.authService.tenantId, this.authService.userOrganisationId)
            .toPromise();

        return (!isCustomer && ((this.arePolicyChangesAllowed() &&
            this.numberOfDaysToExpire <= 60) ||
            (this.status === PolicyStatus.Expired && isAllowedForRenewalAfterExpiry)) &&
            this.productFeatureSetting.areRenewalQuotesEnabled);
    }

    public close(actionName: string): void {
        this.popOverCtrl.dismiss({ action: <ActionButtonPopover>{ actionName: actionName } });
    }
}
