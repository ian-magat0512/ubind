import { Component, OnInit, Injector, ElementRef, OnDestroy } from '@angular/core';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { contentAnimation } from '@assets/animations';
import { FeatureSettingService } from '@app/services/feature-setting.service';
import { PolicyApiService } from '@app/services/api/policy-api.service';
import { PolicyHelper } from '@app/helpers';
import { PremiumResult } from '@app/models/premium-result';
import { Subject } from 'rxjs';
import { CustomerResourceModel } from '@app/resource-models/customer.resource-model';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { PolicyTransactionDetailViewModel } from '@app/viewmodels/policy-transaction-detail.viewmodel';
import { PolicyTransactionDetailResourceModel } from '@app/resource-models/policy.resource-model';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { EventService } from '@app/services/event.service';
import { finalize, takeUntil } from 'rxjs/operators';
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import { QuestionViewModel } from '@app/viewmodels/question-view.viewmodel';
import { RepeatingQuestionViewModel } from '@app/viewmodels/repeating-question.viewmodel';
import { QuestionViewModelGenerator } from '@app/helpers/question-view-model-generator';
import { AuthenticationService } from '@app/services/authentication.service';
import { scrollbarStyle } from '@assets/scrollbar';
import { HttpErrorResponse } from '@angular/common/http';
import { EmailApiService } from '@app/services/api/email-api.service';
import { EntityType } from "@app/models/entity-type.enum";
import { SharedPopoverService } from '@app/services/shared-popover.service';
import { PopoverPolicyTransactionPage } from '../popover-policy/popover-policy-transaction.page';
import { PolicyTransactionEventNamePastTense } from '@app/models/policy-transaction-event-name-past-tense.enum';
import { Permission, PermissionDataModel } from '@app/helpers/permissions.helper';
import { AppConfigService } from '@app/services/app-config.service';
import { AppConfig } from '@app/models/app-config';
import { PermissionService } from '@app/services/permission.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { PortalPageTriggerResourceModel } from '@app/resource-models/portal-page-trigger.resource-model';
import { PortalExtensionsService } from '@app/services/portal-extensions.service';
import { ActionButtonPopover } from '@app/models/action-button-popover';
import { ActionButton } from '@app/models/action-button';
import { IconLibrary } from '@app/models/icon-library.enum';
import { ActionButtonHelper } from '@app/helpers/action-button.helper';
import { PopoverCommand } from '@app/models/popover-command';
import { PageType } from '@app/models/page-type.enum';

/**
 * Export detail policy transaction page component class.
 * TODO: Write a better class header: displaying of policy transaction details.
 */
@Component({
    selector: 'app-detail-policy-transaction',
    templateUrl: './detail-policy-transaction.page.html',
    animations: [contentAnimation],
    styleUrls: [
        './detail-policy-transaction.page.scss',
        '../../../../assets/css/scrollbar-div.css',
        '../../../../assets/css/scrollbar-detail.css',
    ],
    styles: [scrollbarStyle],
})
export class DetailPolicyTransactionPage extends DetailPage implements OnInit, OnDestroy {
    public isRefund: boolean;
    public isMutual: boolean;

    public policyId: string;
    public transactionId: string;
    public segment: string = 'Details';
    public policyNumber: string;
    public eventTypeSummary: PolicyTransactionEventNamePastTense;
    public quoteId: string;
    public baseUrl: string;
    public pageTitle: string;
    public updateStatus: string;
    public displayType: string = QuestionViewModelGenerator.type.Policy;
    public policyDetail: PolicyTransactionDetailViewModel;
    public customerDetail: CustomerResourceModel;
    public ownerDetail: any;
    public refundOrPriceData: PremiumResult;
    public questionItems: Array<QuestionViewModel>;
    public repeatingQuestionItems: Array<RepeatingQuestionViewModel>;
    public detailsListItems: Array<DetailsListItem>;
    public entityTypes: typeof EntityType = EntityType;
    public permission: typeof Permission = Permission;
    public canEditAdditionalPropertyValues: boolean = false;
    private editPropertiesTitle: string;
    public hasEmailFeature: boolean;
    public hasClaimFeature: boolean;
    public permissionModel: PermissionDataModel;
    protected portalPageTriggers: Array<PortalPageTriggerResourceModel>;
    protected actions: Array<ActionButtonPopover> = [];
    protected hasActionsIncludedInMenu: boolean = false;
    public actionButtonList: Array<ActionButton>;
    public flipMoreIcon: boolean = false;

    public constructor(
        public navProxy: NavProxyService,
        protected routeHelper: RouteHelper,
        private featureSettingService: FeatureSettingService,
        private policyApiService: PolicyApiService,
        public layoutManager: LayoutManagerService,
        private userPath: UserTypePathHelper,
        public eventService: EventService,
        private authService: AuthenticationService,
        elementRef: ElementRef,
        injector: Injector,
        protected emailApiService: EmailApiService,
        protected sharedPopoverService: SharedPopoverService,
        private permissionService: PermissionService,
        private appConfigService: AppConfigService,
        private portalExtensionService: PortalExtensionsService,
    ) {
        super(eventService, elementRef, injector);
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.hasClaimFeature = this.featureSettingService.hasClaimFeature();
        });
    }

    public async ngOnInit(): Promise<void> {
        this.destroyed = new Subject<void>();
        this.policyId = this.routeHelper.getParam('policyId');
        this.transactionId = this.routeHelper.getParam('policyTransactionId');
        this.isMutual = this.authService.isMutualTenant();
        await this.loadPolicyTransactionDetails(this.policyId, this.transactionId);
        this.hasClaimFeature = this.featureSettingService.hasClaimFeature();
        this.segment = this.routeHelper.getParam('segment') || this.segment;
        this.preparePortalExtensions().then(() => this.generatePopoverLinks());
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
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

    private async preparePortalExtensions(): Promise<void> {
        this.portalPageTriggers =
            await this.portalExtensionService.getEnabledPortalPageTriggers(
                this.authService,
                this.entityTypes.PolicyTransaction,
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
            this.entityTypes.PolicyTransaction,
            PageType.Display,
            this.segment,
            this.transactionId,
        );
    }

    public isDisplayable(tab: string): boolean {
        if (this.isLoading) {
            return tab != 'Refund';
        }
        return PolicyHelper.isDisplayableTab(this.eventTypeSummary, tab);
    }

    public goBackToDetail(): void {
        this.navProxy.navigate([this.userPath.policy, this.policyId], { queryParams: { segment: 'History' } });
    }

    private loadPolicyTransactionDetails(policyId: string, transactionId: string): Promise<void> {
        this.isLoading = true;
        return this.policyApiService.getPolicyTransaction(policyId, transactionId)
            .pipe(
                takeUntil(this.destroyed),
                finalize(() => this.isLoading = false),
            )
            .toPromise()
            .then(
                (dt: PolicyTransactionDetailResourceModel) => {
                    this.policyDetail = new PolicyTransactionDetailViewModel(dt);
                    this.permissionModel = {
                        organisationId: this.policyDetail.organisationId,
                        customerId: this.policyDetail.customer?.id,
                        ownerUserId: this.policyDetail.owner?.id,
                    };
                    this.initializeDetailsListItems();
                    this.refundOrPriceData = dt.premium as PremiumResult;
                    this.isRefund = Number(dt.premium.totalPayable) < 0;
                    this.eventTypeSummary = dt.eventTypeSummary;
                    this.policyNumber = dt.policyNumber;
                    this.pageTitle = PolicyHelper.getPageTitle(this.eventTypeSummary);
                    this.pageTitle = this.isMutual ? this.pageTitle.replace('Policy', 'Protection') : this.pageTitle;
                    this.questionItems = QuestionViewModelGenerator.getQuestionSetItems(
                        dt.questions,
                        dt.questionAttachmentKeys,
                        dt.displayableFields,
                    );
                    this.repeatingQuestionItems = QuestionViewModelGenerator.getRepeatingQuestionSets(
                        dt.questions,
                        dt.displayableFields,
                        dt.questionAttachmentKeys,
                    );
                    this.canEditAdditionalPropertyValues =
                    dt.additionalPropertyValues != null && dt.additionalPropertyValues.length
                    && this.permissionService.hasElevatedPermissionsViaModel(
                        Permission.ManagePolicies,
                        this.permissionModel,
                    )
                    && this.permissionService.hasPermission(Permission.EditAdditionalPropertyValues);
                    this.editPropertiesTitle = PolicyHelper.getEditAdditionalPropertiesPopOverTitle(
                        this.eventTypeSummary,
                    );
                },
                (err: HttpErrorResponse) => {
                    this.errorMessage = 'There was a problem loading the policy transaction details';
                    throw err;
                },
            );
    }

    private initializeDetailsListItems(): void {
        this.detailsListItems = this.policyDetail.createDetailsList(
            this.navProxy,
            this.authService.isCustomer(),
            this.authService.isMutualTenant(),
        );
    }

    public async presentPopover(event: any): Promise<void> {
        this.flipMoreIcon = true;
        const popoverDismissAction = (command: PopoverCommand): void => {
            this.flipMoreIcon = false;
            if (command && command.data && command.data.action) {
                switch (command.data.action.actionName) {
                    case 'editAdditionalPropertyValues':
                        this.navProxy.goToAdditionalPropertyValues(this.getPolicyTransactionEntityType());
                        break;
                    default:
                        if (command.data.action.portalPageTrigger) {
                            this.portalExtensionService.executePortalPageTrigger(
                                command.data.action.portalPageTrigger,
                                this.entityTypes.PolicyTransaction,
                                PageType.Display,
                                this.segment,
                                this.transactionId,
                            );
                        }
                        break;
                }
            }
        };

        await this.sharedPopoverService.show(
            {
                component: PopoverPolicyTransactionPage,
                cssClass: 'custom-popover more-button-top-popover-positioning',
                event: event,
                componentProps: {
                    editPropertiesTitle: this.editPropertiesTitle,
                    canEditAdditionalPropertyValues: this.canEditAdditionalPropertyValues,
                    actions: this.actions,
                },
            },
            'Policy transaction option popover',
            popoverDismissAction,
        );
    }

    private getPolicyTransactionEntityType(): EntityType {
        switch (this.policyDetail.eventTypeSummary) {
            case PolicyTransactionEventNamePastTense.Purchased:
                return EntityType.NewBusinessPolicyTransaction;
            case PolicyTransactionEventNamePastTense.Adjusted:
                return EntityType.AdjustmentPolicyTransaction;
            case PolicyTransactionEventNamePastTense.Renewed:
                return EntityType.RenewalPolicyTransaction;
            case PolicyTransactionEventNamePastTense.Cancelled:
                return EntityType.CancellationPolicyTransaction;
            default:
                return EntityType.Quote;
        }
    }

    private async initializeActionButtonList(): Promise<void> {
        let actionButtonList: Array<ActionButton> = [];
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
}
