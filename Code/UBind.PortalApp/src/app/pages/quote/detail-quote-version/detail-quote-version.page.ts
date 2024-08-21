import { Component, OnInit, Injector, ElementRef, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import { Permission, PermissionDataModel } from '@app/helpers/permissions.helper';
import { DocumentApiService } from '@app/services/api/document-api.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { QuoteDetailViewModel, QuoteVersionDetailViewModel } from '@app/viewmodels';
import { contentAnimation } from '@assets/animations';
import { scrollbarStyle } from '@assets/scrollbar';
import { PremiumResult } from '@app/models';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { QuoteVersionApiService } from '@app/services/api/quote-version-api.service';
import { AuthenticationService } from '@app/services/authentication.service';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { EventService } from '@app/services/event.service';
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import { takeUntil } from 'rxjs/operators';
import { QuestionViewModel } from '@app/viewmodels/question-view.viewmodel';
import { RepeatingQuestionViewModel } from '@app/viewmodels/repeating-question.viewmodel';
import { QuestionViewModelGenerator } from '@app/helpers/question-view-model-generator';
import { QuoteVersionDetailResourceModel } from '@app/resource-models/quote-version.resource-model';
import { QuoteApiService } from '@app/services/api/quote-api.service';
import { QuoteDetailResourceModel } from '@app/resource-models/quote.resource-model';
import { PermissionService } from '@app/services/permission.service';
import { SharedPopoverService } from '@app/services/shared-popover.service';
import { EntityType } from '@app/models/entity-type.enum';
import { UserService } from '@app/services/user.service';
import { PopoverViewComponent } from '@app/components/popover-view/popover-view.component';
import { AppConfigService } from '@app/services/app-config.service';
import { FeatureSettingService } from '@app/services/feature-setting.service';
import { AppConfig } from '@app/models/app-config';
import { PortalPageTriggerResourceModel } from '@app/resource-models/portal-page-trigger.resource-model';
import { PortalExtensionsService } from '@app/services/portal-extensions.service';
import { ActionButtonPopover } from '@app/models/action-button-popover';
import { ActionButton } from '@app/models/action-button';
import { IconLibrary } from '@app/models/icon-library.enum';
import { ActionButtonHelper } from '@app/helpers/action-button.helper';
import { PopoverCommand } from '@app/models/popover-command';
import { PageType } from '@app/models/page-type.enum';

/**
 * Export detail quote version page component class
 * TODO: Write a better class header: displaying the quote version details.
 */
@Component({
    selector: 'app-detail-quote-version',
    templateUrl: './detail-quote-version.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-detail.css',
        './detail-quote-version.page.scss',
    ],
    styles: [scrollbarStyle],
})
export class DetailQuoteVersionPage extends DetailPage implements OnInit, OnDestroy {

    public isQuestionsReady: boolean;
    public isRefund: boolean = false;
    public isCustomer: boolean = false;
    public detailsListItems: Array<DetailsListItem>;
    public completeAndDeclineQuoteStatus: Set<string> = new Set<string>(['completed', 'declined']);
    public isEditQuoteHidden: boolean;
    public isParentQuoteEditable: boolean;

    private titlePrefix: string = 'Quote Version';
    public title: string = this.titlePrefix + ':';
    public segment: string = 'Details';
    public displayType: string = QuestionViewModelGenerator.type.Quote;
    public entityTypes: typeof EntityType = EntityType;
    public permissionModel: PermissionDataModel;
    public hasEmailFeature: boolean;

    public quoteVersion: QuoteVersionDetailViewModel;
    public permission: typeof Permission = Permission;
    public refundOrPriceData: PremiumResult;
    public questionItems: Array<QuestionViewModel>;
    public repeatingQuestionItems: Array<RepeatingQuestionViewModel>;
    private downloading: any = {};
    private quoteId: string;
    public quoteVersionId: string;
    public canEditAdditionalPropertyValues: boolean = false;
    protected actions: Array<ActionButtonPopover> = [];
    protected hasActionsIncludedInMenu: boolean = false;
    private readonly additionalPropertyPopOverLabel: string = "Edit Quote Version Properties";
    public actionButtonList: Array<ActionButton>;
    public flipMoreIcon: boolean = false;
    protected portalPageTriggers: Array<PortalPageTriggerResourceModel>;

    public constructor(
        private routeHelper: RouteHelper,
        private quoteVersionApiService: QuoteVersionApiService,
        private documentApiService: DocumentApiService,
        public navProxy: NavProxyService,
        public layoutManager: LayoutManagerService,
        public authService: AuthenticationService,
        protected userPath: UserTypePathHelper,
        eventService: EventService,
        elementRef: ElementRef,
        injector: Injector,
        private quoteApiService: QuoteApiService,
        private permissionService: PermissionService,
        protected sharedPopoverService: SharedPopoverService,
        private userService: UserService,
        private appConfigService: AppConfigService,
        protected featureSettingService: FeatureSettingService,
        private portalExtensionService: PortalExtensionsService,
    ) {
        super(eventService, elementRef, injector);
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.hasEmailFeature = this.featureSettingService.hasEmailFeature();
        });
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.isCustomer = this.authService.isCustomer();
        this.isEditQuoteHidden =
            this.quoteVersion && this.completeAndDeclineQuoteStatus.has(this.quoteVersion.quoteStatus.toLowerCase());
        this.quoteId = this.routeHelper.getParam('quoteId');
        this.quoteVersionId = this.routeHelper.getParam('quoteVersionId');
        this.segment = this.routeHelper.getParam('segment') || 'Details';
        this.hasEmailFeature = this.featureSettingService.hasEmailFeature();
        this.load();
    }

    private load(): void {
        this.isLoading = true;
        this.quoteVersionApiService.getQuoteVersionDetail(this.quoteVersionId)
            .pipe(
                takeUntil(this.destroyed),
            )
            .subscribe(async (quoteVersionDetail: QuoteVersionDetailResourceModel) => {
                this.quoteVersion = new QuoteVersionDetailViewModel(quoteVersionDetail);
                this.refundOrPriceData = quoteVersionDetail.premiumData;
                this.isRefund = Number(quoteVersionDetail.premiumData.totalPayable) < 0;
                this.initializedetailsListItems();
                this.questionItems = QuestionViewModelGenerator.getFormDataQuestionItems(
                    this.quoteVersion.formData,
                    this.quoteVersion.questionAttachmentKeys,
                    this.quoteVersion.displayableFieldsModel,
                );
                this.repeatingQuestionItems = QuestionViewModelGenerator.getRepeatingData(
                    this.quoteVersion.formData,
                    this.quoteVersion.displayableFieldsModel,
                    this.quoteVersion.questionAttachmentKeys,
                );
                this.updateTitle();
                await this.loadQuoteDetails();
                this.canEditAdditionalPropertyValues =
                    this.permissionService.hasPermission(Permission.EditAdditionalPropertyValues) &&
                    (quoteVersionDetail.additionalPropertyValues
                        && quoteVersionDetail.additionalPropertyValues.length > 0);
                this.preparePortalExtensions().then(() => this.generatePopoverLinks());
                this.isLoading = false;
            }, (err: any) => {
                this.errorMessage = 'There was a problem loading the quote version details';
                this.isLoading = false;
                throw err;
            });
    }

    private loadQuoteDetails(): Promise<void> {
        return this.quoteApiService.getQuoteDetails(this.quoteId)
            .pipe(takeUntil(this.destroyed))
            .toPromise()
            .then(
                (details: QuoteDetailResourceModel) => {
                    let quoteViewDetailModel: QuoteDetailViewModel = new QuoteDetailViewModel(details);
                    let permissionModel: PermissionDataModel = {
                        customerId: quoteViewDetailModel.customerDetails.id,
                        organisationId: quoteViewDetailModel.organisationId,
                        ownerUserId: quoteViewDetailModel.owner ? quoteViewDetailModel.owner.id : null,
                    };
                    quoteViewDetailModel.determineIfEditable(this.permissionService, permissionModel);
                    this.isParentQuoteEditable = quoteViewDetailModel.isEditable;
                },
                (err: any) => {
                    this.errorMessage = "There was a problem loading the quote details";
                    throw err;
                },
            );
    }

    private async preparePortalExtensions(): Promise<void> {
        this.portalPageTriggers =
            await this.portalExtensionService.getEnabledPortalPageTriggers(
                this.authService,
                this.entityTypes.QuoteVersion,
                PageType.Display,
                this.segment,
            );
    }

    private generatePopoverLinks(): void {
        // Add portal page trigger actions
        this.actions =
            this.portalExtensionService.getActionButtonPopovers(this.portalPageTriggers);
        if (this.canEditAdditionalPropertyValues) {
            this.actions.push({
                actionName: this.additionalPropertyPopOverLabel,
                actionIcon: "brush",
                iconLibrary: IconLibrary.IonicV4,
                actionButtonLabel: "",
                actionButtonPrimary: false,
                includeInMenu: true,
            });
        }
        this.hasActionsIncludedInMenu = this.actions.filter((x: ActionButtonPopover) => x.includeInMenu).length > 0;
        this.initializeActionButtonList();
    }

    protected executePortalPageTrigger(trigger: PortalPageTriggerResourceModel): void {
        this.portalExtensionService.executePortalPageTrigger(
            trigger,
            this.entityTypes.QuoteVersion,
            PageType.Display,
            this.segment,
            this.quoteVersionId,
        );
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    public segmentChanged($event: any): void {
        if ($event.detail.value != this.segment) {
            this.segment = $event.detail.value;
            this.preparePortalExtensions().then(() => this.generatePopoverLinks());
        }
    }

    private initializedetailsListItems(): void {
        this.detailsListItems = this.quoteVersion.createDetailsList(
            this.navProxy,
            this.authService.isCustomer(),
            this.authService.isMutualTenant(),
        );
    }

    private updateTitle(): void {
        this.title = this.titlePrefix + (this.quoteVersion.referenceNumber ? ': ' : ' ') +
            this.quoteVersion.referenceNumber;
    }

    public editQuoteVersion(): void {
        this.navProxy.navigateForward([this.userPath.quote, this.quoteId, 'version', this.quoteVersionId, 'edit']);
    }

    public goBack(): void {
        this.navProxy.navigate(
            [
                this.userPath.quote,
                this.quoteVersion.quoteId],
            {
                queryParams:
                {
                    segment: 'Versions',
                },
            },
        );
    }

    public async presentPopover(event: any): Promise<void> {
        this.flipMoreIcon = true;
        const popoverDismissAction = (command: PopoverCommand): void => {
            this.flipMoreIcon = false;
            if (command && command.data && command.data.action) {
                const commandAction: ActionButtonPopover = command.data.action;
                switch (commandAction.actionName) {
                    case this.additionalPropertyPopOverLabel:
                        this.navProxy.goToAdditionalPropertyValues(EntityType.QuoteVersion);
                        break;
                    default:
                        if (command.data.action.portalPageTrigger) {
                            this.portalExtensionService.executePortalPageTrigger(
                                commandAction.portalPageTrigger,
                                this.entityTypes.QuoteVersion,
                                PageType.Display,
                                this.segment,
                                this.quoteVersionId,
                            );
                        }
                        break;
                }
            }
        };

        await this.sharedPopoverService.show(
            {
                component: PopoverViewComponent,
                cssClass: 'custom-popover more-button-top-popover-positioning',
                componentProps: {
                    actions: this.actions,
                },
                event: event,
            },
            'Quote version option popover',
            popoverDismissAction,
        );
    }

    private initializeActionButtonList(): void {
        let actionButtonList: Array<ActionButton> = [];
        const canEditQuoteVersion: boolean = this.quoteVersion != null
            && !this.isEditQuoteHidden
            && this.isParentQuoteEditable
            && this.permissionService.hasPermission(Permission.ManageQuoteVersions);
        if (canEditQuoteVersion) {
            actionButtonList.push(ActionButton.createActionButton(
                "Edit",
                "pencil",
                IconLibrary.AngularMaterial,
                false,
                "Edit Quote Version",
                true,
                (): void => {
                    return this.editQuoteVersion();
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
                            this.entityTypes.QuoteVersion,
                            PageType.Display,
                            this.segment,
                            this.quoteVersionId,
                        );
                    },
                ));
            }
        }

        this.actionButtonList = ActionButtonHelper.sortActionButtons(actionButtonList);
    }
}
