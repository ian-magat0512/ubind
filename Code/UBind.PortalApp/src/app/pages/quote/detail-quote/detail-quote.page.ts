import { Component, Injector, ElementRef, OnDestroy, OnInit } from '@angular/core';
import { Subject } from 'rxjs';
import { PopoverController, AlertController } from '@ionic/angular';
import { DomSanitizer } from '@angular/platform-browser';
import { AuthenticationService } from '@app/services/authentication.service';
import { QuoteApiService } from '@app/services/api/quote-api.service';
import { FeatureSettingService } from '@app/services/feature-setting.service';
import { DocumentApiService } from '@app/services/api/document-api.service';
import { PermissionService } from '@app/services/permission.service';
import { Permission, PermissionDataModel } from '@app/helpers';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { QuoteState, PremiumResult } from '@app/models';
import { contentAnimation } from '@assets/animations';
import { scrollbarStyle } from '@assets/scrollbar';
import { PopoverQuotePage } from '../popover-quote/popover-quote.page';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { Errors } from '@app/models/errors';
import { RouteHelper } from '@app/helpers/route.helper';
import { QuoteVersionApiService } from '@app/services/api/quote-version-api.service';
import { QuoteDetailViewModel } from '@app/viewmodels/quote-detail.viewmodel';
import { QuoteVersionViewModel } from '@app/viewmodels/quote-version.viewmodel';
import {
    QuoteCreateResultModel, QuoteDetailResourceModel, QuoteResourceModel,
} from '@app/resource-models/quote.resource-model';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { EventService } from '@app/services/event.service';
import { UserService } from '@app/services/user.service';
import { QuoteType } from '@app/models/quote-type.enum';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { SharedPopoverService } from '@app/services/shared-popover.service';
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import { QuestionViewModel } from '@app/viewmodels/question-view.viewmodel';
import { RepeatingQuestionViewModel } from '@app/viewmodels/repeating-question.viewmodel';
import { QuestionViewModelGenerator } from '@app/helpers/question-view-model-generator';
import { EntityType } from "@app/models/entity-type.enum";
import { AppConfig } from '@app/models/app-config';
import { AppConfigService } from '@app/services/app-config.service';
import { DocumentViewModel } from '@app/viewmodels';
import { ProductApiService } from '@app/services/api/product-api.service';
import { ProductResourceModel } from '@app/resource-models/product.resource-model';
import { PortalPageTriggerResourceModel } from '@app/resource-models/portal-page-trigger.resource-model';
import { PortalExtensionsService } from '@app/services/portal-extensions.service';
import { ActionButtonPopover } from '@app/models/action-button-popover';
import { ActionButton } from '@app/models/action-button';
import { IconLibrary } from '@app/models/icon-library.enum';
import { ActionButtonHelper } from '@app/helpers/action-button.helper';
import { PopoverCommand } from '@app/models/popover-command';
import { PageType } from '@app/models/page-type.enum';
import { ProductFeatureSetting } from '@app/models/product-feature-setting';
import { ProductFeatureSettingService } from '@app/services/product-feature-service';
import { ToastController } from '@ionic/angular';
import { DeploymentEnvironment } from '@app/models/deployment-environment.enum';
import { PolicyIssuanceResourceModel } from '@app/resource-models/policy.resource-model';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { CalculationState } from '@app/models/calculation-state.enum';
import { TriggerType } from '@app/models/trigger-type.enum';

/**
 * Export detail quote page component class
 * This class is to display the details of the quote.
 */
@Component({
    selector: 'app-detail-quote',
    templateUrl: './detail-quote.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-detail.css',
    ],
    styles: [scrollbarStyle],
})
export class DetailQuotePage extends DetailPage implements OnInit, OnDestroy {
    public isRefund: boolean;
    public documentHeaders: Array<string> = [];
    public displayType: string = QuestionViewModelGenerator.type.Quote;
    public segment: string = 'Details';
    public detail: QuoteDetailViewModel;
    public quoteVersionList: Array<QuoteVersionViewModel>;
    public detailsListItems: Array<DetailsListItem>;
    public permission: typeof Permission = Permission;
    public refundOrPriceData: PremiumResult;
    public quoteType: any = QuoteType;
    public questionItems: Array<QuestionViewModel>;
    public repeatingQuestionItems: Array<RepeatingQuestionViewModel>;
    public hasEmailFeature: boolean;
    public environment: DeploymentEnvironment;
    public canViewPremiumBreakdown: boolean;
    public calculationResultMessage: string;

    protected policyUpdateMode: any = { renew: 'Renew', update: 'Update' };
    protected quoteMode: any = { review: 'Review', endorse: 'Endorse', expired: 'Expired', edit: 'Edit' };
    public quoteDetailsLoadComplete: boolean;
    public quoteId: string;
    public entityTypes: typeof EntityType = EntityType;
    public canEditAdditionalPropertyValues: boolean;
    public permissionModel: PermissionDataModel;
    public performingUserTenantId: string;
    private pathTenantAlias: string;
    private product: ProductResourceModel;
    public actionItemIcon: string;
    protected portalPageTriggers: Array<PortalPageTriggerResourceModel>;
    protected actions: Array<ActionButtonPopover> = [];
    protected hasActionsIncludedInMenu: boolean = false;
    public actionButtonList: Array<ActionButton>;
    public flipMoreIcon: boolean = false;
    private productFeatureSetting: ProductFeatureSetting;
    private canManagePolicyNumbers: boolean;
    private calculationState: string;
    private hasEndorseQuotesPermission: boolean;
    private hasReviewQuotesPermission: boolean;

    public constructor(
        public sanitizer: DomSanitizer,
        protected routeHelper: RouteHelper,
        protected quoteApiService: QuoteApiService,
        protected quoteVersionApiService: QuoteVersionApiService,
        protected documentApiService: DocumentApiService,
        public authService: AuthenticationService,
        public navProxy: NavProxyService,
        protected permissionService: PermissionService,
        protected popOverCtrl: PopoverController,
        protected featureSettingService: FeatureSettingService,
        protected userService: UserService,
        protected alertCtrl: AlertController,
        private sharedAlertService: SharedAlertService,
        public layoutManager: LayoutManagerService,
        protected userPath: UserTypePathHelper,
        protected eventService: EventService,
        elementRef: ElementRef,
        injector: Injector,
        protected sharedPopoverService: SharedPopoverService,
        private appConfigService: AppConfigService,
        private productApiService: ProductApiService,
        private portalExtensionService: PortalExtensionsService,
        private productFeatureService: ProductFeatureSettingService,
        private toastCtrl: ToastController,
        private sharedLoaderService: SharedLoaderService,
    ) {
        super(eventService, elementRef, injector);
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.hasEmailFeature = this.featureSettingService.hasEmailFeature();
            if (!this.appConfigService.isMasterPortal()) {
                this.performingUserTenantId = appConfig.portal.tenantId;
            }
        });
    }

    public async ngOnInit(): Promise<void> {
        this.destroyed = new Subject<void>();
        this.hasEndorseQuotesPermission = this.permissionService.hasPermission(Permission.EndorseQuotes);
        this.hasReviewQuotesPermission = this.permissionService.hasPermission(Permission.ReviewQuotes);
        this.quoteId = this.routeHelper.getParam('quoteId');
        this.pathTenantAlias = this.routeHelper.getParam('tenantAlias');
        await this.load();
        this.hasEmailFeature = this.featureSettingService.hasEmailFeature();
        this.segment = this.routeHelper.getParam('segment') || this.segment;
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    protected async preparePortalExtensions(): Promise<void> {
        this.portalPageTriggers =
            await this.portalExtensionService.getEnabledPortalPageTriggers(
                this.authService,
                this.entityTypes.Quote,
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
            this.entityTypes.Quote,
            PageType.Display,
            this.segment,
            this.quoteId,
        );
    }

    public segmentChanged($event: any): void {
        if ($event.detail.value != this.segment) {
            this.segment = $event.detail.value;
            this.preparePortalExtensions().then(() => this.generatePopoverLinks());
            this.navProxy.updateSegmentQueryStringParameter(
                'segment',
                this.segment != 'Details' ? this.segment : null,
            );
        }
    }

    private async load(): Promise<void> {
        this.isLoading = true;
        try {
            const quoteDetail: QuoteDetailResourceModel = await this.quoteApiService
                .getQuoteDetails(this.quoteId).toPromise();
            this.detail = new QuoteDetailViewModel(quoteDetail);
            this.permissionModel = {
                organisationId: this.detail.organisationId,
                ownerUserId: this.detail.owner != null ? this.detail.owner.id : null,
                customerId: this.detail.customerDetails ? this.detail.customerDetails.id : null,
            };
            this.detail.determineIfEditable(this.permissionService, this.permissionModel);

            this.canEditAdditionalPropertyValues =
                quoteDetail.additionalPropertyValues != null && quoteDetail.additionalPropertyValues.length
                && this.permissionService.hasElevatedPermissionsViaModel(
                    Permission.ManageQuotes,
                    this.permissionModel,
                )
                && this.permissionService.hasPermission(Permission.EditAdditionalPropertyValues);

            await this.loadProduct();
            this.productFeatureSetting
                = await this.productFeatureService.getProductFeatureSetting(this.detail.productId);

            this.detail.determineIfHasActionsAvailable(
                this.permissionService,
                this.canEditAdditionalPropertyValues,
                this.product.quoteExpirySettings.enabled,
                this.hasRequiredPermission(),
                this.canResumeQuote(),
            );
            this.initializeDetailsListItems();

            this.quoteDetailsLoadComplete = true;
            this.refundOrPriceData = quoteDetail.premiumData;
            this.isRefund = Number(quoteDetail.premiumData.totalPayable) < 0;
            this.calculationState = this.detail.latestCalculationData
                ? JSON.parse(quoteDetail.latestCalculationData.calculationResultJson).state
                : null;
            this.calculationResultMessage = this.getCalculationResultMessage();
            this.canViewPremiumBreakdown = this.canViewPriceBreakdownByStatus();
            this.documentHeaders = Array.from(new Set(this.detail.documents.map((item: DocumentViewModel) =>
                item.dateGroupHeader)));
            this.questionItems = QuestionViewModelGenerator.getFormDataQuestionItems(
                this.detail.formData,
                this.detail.questionAttachmentKeys,
                this.detail.displayableFieldsModel,
            );
            this.repeatingQuestionItems = QuestionViewModelGenerator.getRepeatingData(
                this.detail.formData,
                this.detail.displayableFieldsModel,
                quoteDetail.questionAttachmentKeys,
            );

            this.isLoading = false;
            this.preparePortalExtensions().then(() => this.generatePopoverLinks());
        } catch (error) {
            this.errorMessage = 'There was a problem loading the quote details';
            this.isLoading = false;
            throw error;
        }
    }

    private async loadProduct(): Promise<void> {
        this.isLoading = true;
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        const tenantId: string = this.getContextTenantIdOrAlias();
        params.set('tenant', tenantId);
        try {
            const product: ProductResourceModel = await this.productApiService.getById(
                this.detail.productId,
                params,
            ).toPromise();
            this.product = product;
        } catch (error) {
            if (error.name != 'EmptyError') {
                this.isLoading = false;
                this.errorMessage = 'There was a problem loading the product details';
                throw error;
            }
        }
    }

    private initializeDetailsListItems(): void {
        this.detailsListItems = this.detail.createDetailsList(
            this.navProxy,
            this.authService.isCustomer(),
            this.authService.isMutualTenant(),
        );
    }

    public goBack(): void {
        this.navProxy.navigateBack(
            [this.userPath.quote, 'list'],
            true,
            { queryParams: { segment: this.detail.status } },
        );
    }

    public getStatus(): string {
        return this.detail && this.detail.status.toLowerCase().replace(' ', '');
    }

    private hasRequiredPermission(): boolean {
        const status: string = this.getStatus();
        const requiredPermission: any = (status === QuoteState.Review) ?
            Permission.ReviewQuotes : (status === QuoteState.Endorsement) ?
                Permission.EndorseQuotes : null;
        return this.permissionService.hasPermission(requiredPermission) &&
            this.permissionService.hasElevatedPermissionsViaModel(
                Permission.ManageQuotes,
                this.permissionModel,
            );
    }

    public async presentPopover(event: any): Promise<void> {
        this.flipMoreIcon = true;
        const status: string = this.getStatus();
        const action: string = (status === QuoteState.Review)
            ? this.quoteMode.review
            : (status === QuoteState.Endorsement)
                ? this.quoteMode.endorse
                : null;

        const hasPermission: boolean = this.hasRequiredPermission();
        const canResumeQuote: boolean = this.canResumeQuote();
        const canIssuePolicy: boolean = this.canIssuePolicy();

        const popoverDismissAction = (command: PopoverCommand): void => {
            this.flipMoreIcon = false;
            if (command && command.data && command.data.action) {
                const commandAction: ActionButtonPopover = command.data.action;
                switch (commandAction.actionName) {
                    case 'setExpiryDate':
                        this.setExpiryDateQuote();
                        break;
                    case 'expire':
                        this.expireQuote();
                        break;
                    case 'editAdditionalPropertyValues':
                        this.navProxy.goToAdditionalPropertyValues(this.getQuoteEntityType());
                        break;
                    case 'resume':
                    case 'edit':
                        this.editQuote();
                        break;
                    case 'Review':
                        this.reviewQuote();
                        break;
                    case 'Endorse':
                        this.endorseQuote();
                        break;
                    default:
                        if (commandAction.portalPageTrigger) {
                            this.portalExtensionService.executePortalPageTrigger(
                                commandAction.portalPageTrigger,
                                this.entityTypes.Quote,
                                PageType.Display,
                                this.segment,
                                this.quoteId,
                            );
                        }
                        break;
                }
            }
        };

        await this.sharedPopoverService.show(
            {
                component: PopoverQuotePage,
                componentProps: {
                    action: hasPermission ? action : null,
                    expiryEnabled: this.product.quoteExpirySettings.enabled,
                    canEditAdditionalPropertyValues: this.canEditAdditionalPropertyValues,
                    status: status,
                    isDefaultOptionsEnabled: this.detail && this.detail.isEditable && this.detail.hasActionsAvailable,
                    actions: this.actions,
                    canResumeQuote: canResumeQuote,
                    canIssuePolicy: canIssuePolicy,
                },
                cssClass: 'custom-popover more-button-top-popover-positioning',
                event: event,
            },
            'Quote option popover',
            popoverDismissAction,
        );
    }

    private getQuoteEntityType(): EntityType {
        switch (this.detail.quoteType) {
            case QuoteType.NewBusiness:
                return EntityType.NewBusinessQuote;
            case QuoteType.Adjustment:
                return EntityType.AdjustmentQuote;
            case QuoteType.Renewal:
                return EntityType.RenewalQuote;
            case QuoteType.Cancellation:
                return EntityType.CancellationQuote;
            default:
                return EntityType.Quote;
        }
    }

    private expireQuote(): void {
        // expire the quote. 
        this.quoteApiService.expireNow(this.detail.id).subscribe((quote: QuoteResourceModel) => {
            // display snackbar to notify user that the quote has been expired.
            this.sharedAlertService.showToast('Quote ' + this.detail.quoteNumber + ' was expired.');

            quote.productName = this.detail.productName;
            if (quote.customerDetails && this.detail.customerDetails) {
                quote.customerDetails.displayName = this.detail.customerDetails.displayName;
                quote.customerDetails.id = this.detail.customerDetails.id;
            }
            this.eventService.getEntityUpdatedSubject('Quote').next(quote);

            // load the quote again
            this.load();
        });
    }

    private setExpiryDateQuote(): void {
        this.navProxy.navigateForward([this.userPath.quote, this.detail.id, 'set-expiry']);
    }

    public async editQuote(): Promise<void> {
        if (!this.permissionService.hasElevatedPermissionsViaModel(
            Permission.ManageQuotes,
            this.permissionModel,
        )) {
            throw Errors.User.Forbidden('edit this quote');
        }

        if (this.getStatus().toLowerCase() == QuoteState.Expired.toLowerCase()
            && this.product.quoteExpirySettings.enabled
        ) {
            if (await this.confirmCloneQuote()) {
                this.clone();
            }
        } else {
            this.navProxy.navigate([this.userPath.quote, this.quoteId, 'edit']);
        }
    }

    private async issuePolicy(): Promise<void> {
        if (this.canManagePolicyNumbers) {
            this.navProxy.navigate([this.userPath.quote, this.quoteId, 'issue-policy']);
        } else {
            try {
                await this.sharedLoaderService.presentWait();
                const data: PolicyIssuanceResourceModel
                    = await this.quoteApiService.issuePolicy(this.quoteId).toPromise();
                this.sharedLoaderService.dismiss();
                this.presentIssueSuccessToast(data);
            } catch (error) {
                this.sharedLoaderService.dismiss();
                const isErrorEmpty: boolean = error == null && error.error == null;
                if (!isErrorEmpty) {
                    throw error;
                }
            }
        }
    }

    private async presentIssueSuccessToast(data: PolicyIssuanceResourceModel): Promise<void> {
        const toast: HTMLIonToastElement = await this.toastCtrl.create(
            {
                id: this.quoteId,
                message: `Policy ${data.policyNumber} issued for quote ${this.detail.quoteNumber}`,
                position: 'bottom',
                duration: 3000,
            });

        return toast.present().then(() => {
            this.goToPolicy(data.policyId);
        });
    }

    public goToPolicy(policyId: string): void {
        this.navProxy.navigateForward(['policy', policyId], true, null);
    }

    public reviewQuote(): void {
        if (!this.hasReviewQuotesPermission) {
            throw Errors.User.Forbidden('review this quote');
        }
        this.navProxy.navigate([this.userPath.quote, this.quoteId, 'review']);
    }

    // create a function handler
    private async confirmCloneQuote(): Promise<boolean> {
        return new Promise(async (resolve: any): Promise<any> => {
            let message: string = this.detail.quoteType == QuoteType.NewBusiness ?
                "The quote you're trying to edit has expired. Would you like to copy the "
                + "question answers from this quote, and use them to pre- populate a new quote?"
                :
                "The quote you're trying to edit has expired. Would you like to copy the "
                + "question answers from this quote, and use them to pre-populate a new quote? "
                + "Doing so will result in the expired quote being discarded.";

            await this.sharedAlertService.showWithActionHandler({
                header: 'Quote expired',
                subHeader: message,
                buttons: [
                    {
                        text: 'Cancel',
                        handler: (): any => {
                            resolve(false);
                        },
                    },
                    {
                        text: 'Copy',
                        handler: (): any => {
                            resolve(true);
                        },
                    },
                ],
            });
        });
    }

    /**
     * this clones the quote copying its formdata.
     */
    private clone(): void {
        // create a new quote from this 'older' quote.
        this.quoteApiService.clone(this.detail.id).subscribe((dt: QuoteCreateResultModel) => {
            this.navProxy.navigate([this.userPath.quote, dt.quoteId, 'edit']);
        });
    }

    public endorseQuote(): void {
        if (!this.hasEndorseQuotesPermission) {
            throw Errors.User.Forbidden('endorse this quote');
        }
        this.navProxy.navigate([this.userPath.quote, this.quoteId, 'endorse']);
    }

    private getContextTenantIdOrAlias(): string {
        return this.pathTenantAlias || this.performingUserTenantId;
    }

    private initializeActionButtonList(): void {
        if (!this.detail) {
            return;
        }

        let actionButtonList: Array<ActionButton> = [];
        const status: string = this.getStatus();
        const hasPermission: boolean = this.hasRequiredPermission();

        const resumeIsPrimary: boolean = status == QuoteState.Incomplete
            && this.permissionService.hasPermission(Permission.ManageQuotes);

        if (this.canResumeQuote()) {
            actionButtonList.push(ActionButton.createActionButton(
                "Resume",
                "resume-quote",
                IconLibrary.AngularMaterial,
                resumeIsPrimary,
                "Resume Quote",
                true,
                (): Promise<void> => {
                    return this.editQuote();
                },
            ));
        }

        const reviewIsPrimary: boolean = status == QuoteState.Review
            && this.hasReviewQuotesPermission;
        if (hasPermission && status == QuoteState.Review) {
            actionButtonList.push(ActionButton.createActionButton(
                "Review",
                "feature-search",
                IconLibrary.AngularMaterial,
                reviewIsPrimary,
                "Review Quote",
                true,
                (): void => {
                    return this.reviewQuote();
                },
            ));
        }

        const endorseIsPrimary: boolean = status == QuoteState.Endorsement
            && this.hasEndorseQuotesPermission;
        if (hasPermission && status == QuoteState.Endorsement) {
            actionButtonList.push(ActionButton.createActionButton(
                "Endorse",
                "stamper",
                IconLibrary.AngularMaterial,
                endorseIsPrimary,
                "Endorse Quote",
                true,
                (): void => {
                    return this.endorseQuote();
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
                            this.entityTypes.Quote,
                            PageType.Display,
                            this.segment,
                            this.quoteId,
                        );
                    },
                ));
            }
        }

        const env: string = this.routeHelper.getParam('environment');
        this.environment = env ? DeploymentEnvironment[env] : 'Production';
        this.canManagePolicyNumbers = this.permissionService.hasPermission(Permission.ManagePolicyNumbers);
        if (this.canIssuePolicy()) {
            actionButtonList.push(ActionButton.createActionButton(
                "Issue Policy",
                "shield",
                IconLibrary.AngularMaterial,
                resumeIsPrimary,
                "Issue Policy",
                true,
                (): Promise<void> => {
                    return this.issuePolicy();
                },
            ));
        }

        this.actionButtonList = ActionButtonHelper.sortActionButtons(actionButtonList);
    }

    private canResumeQuote(): boolean {
        return this.detail.isEditable
            && ((this.detail != null
                && this.permissionService.hasElevatedPermissionsViaModel(
                    Permission.ManageQuotes,
                    this.permissionModel,
                )) || this.authService.isCustomer());
    }

    private canIssuePolicy(): boolean {
        const isQuoteComplete: boolean = this.detail.status == 'Complete';
        const hasPolicy: boolean = this.detail.policyId != null;
        const isPolicyNewBusinessEnabled: boolean = this.productFeatureSetting.areNewBusinessPolicyTransactionsEnabled;
        return this.detail != null
            && isQuoteComplete
            && !hasPolicy
            && isPolicyNewBusinessEnabled;
    }

    private getCalculationResultMessage(): string {
        const quoteStatus: string = this.detail.status.toLowerCase();
        const triggerTypeWhichRequiresPriceToBeHidden: string =
            this.detail.latestCalculationData.triggerTypeWhichRequiresPriceToBeHidden?.toLowerCase();
        if (!this.calculationState) {
            return 'A premium amount has not yet been calculated for this quote';
        }
        if (quoteStatus == QuoteState.Declined.toLowerCase() && !this.hasEndorseQuotesPermission) {
            return 'The price for this quote cannot be displayed because the quote has been declined';
        }
        if (triggerTypeWhichRequiresPriceToBeHidden == TriggerType.Decline) {
            return 'The price for this quote cannot be displayed due to an active ' +
                'decline trigger that prohibits displaying the pricing information';
        }
        if (!this.inApprovedOrCompleteState()) {
            if (triggerTypeWhichRequiresPriceToBeHidden == TriggerType.Review
                && !(this.hasReviewQuotesPermission || this.hasEndorseQuotesPermission)) {
                if (quoteStatus == QuoteState.Review.toLowerCase()) {
                    return 'The price for this quote cannot be displayed because the quote ' +
                        'is currently in a state of review';
                }
                return 'The price for this quote cannot be displayed due to an active review ' +
                    'trigger that prohibits displaying the pricing information';
            }
            if (triggerTypeWhichRequiresPriceToBeHidden == TriggerType.Endorsement
                && !this.hasEndorseQuotesPermission) {
                if (quoteStatus == QuoteState.Endorsement.toLowerCase()) {
                    return 'The price for this quote cannot be displayed because the quote ' +
                        'is currently in a state of endorsement';
                }
                return 'The price for this quote cannot be displayed due to an active ' +
                    'endorsement trigger that prohibits displaying the pricing information';
            }
        }
        if (this.calculationState == CalculationState.Incomplete) {
            return 'Insufficient information has been provided to calculate a premium amount';
        }
        if (this.calculationState == CalculationState.PremiumEstimate) {
            return 'Some information that affects the calculation of a premium amount has not yet been provided. ' +
                'The following pricing information should therefore be considered an estimate only, ' +
                'should be used for indicative purposes only, and does not constitute a quote or offer.';
        }
        if (this.calculationState == CalculationState.PremiumComplete) {
            return 'The price displayed here does not take into consideration certain disclosure questions ' +
                'that still need to be answered, which may result in a referral, decline, or a further change ' +
                'in premium. The following pricing information should therefore be considered conditional.';
        }
        return '';
    }

    public canViewPriceBreakdownByStatus(): boolean {
        if (this.inApprovedOrCompleteState()) {
            return true;
        }
        const triggerTypeWhichRequiresPriceToBeHidden: string =
            this.detail.latestCalculationData.triggerTypeWhichRequiresPriceToBeHidden?.toLowerCase();
        if (!this.calculationState || this.calculationState == CalculationState.Incomplete.toLowerCase()) {
            return false;
        }
        if (triggerTypeWhichRequiresPriceToBeHidden == TriggerType.Decline) {
            return false;
        }
        if (triggerTypeWhichRequiresPriceToBeHidden == TriggerType.Review
            && !(this.hasReviewQuotesPermission || this.hasEndorseQuotesPermission)) {
            return false;
        }
        if (triggerTypeWhichRequiresPriceToBeHidden == TriggerType.Endorsement
            && !this.hasEndorseQuotesPermission) {
            return false;
        }
        return true;
    }

    private inApprovedOrCompleteState(): boolean {
        const quoteStatus: string = this.detail.status.toLowerCase();
        return quoteStatus == QuoteState.Approved || quoteStatus == QuoteState.Complete;
    }
}
