import { Component, OnInit, Injector, ElementRef, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';
import { AuthenticationService } from '@app/services/authentication.service';
import { ClaimApiService } from '@app/services/api/claim-api.service';
import { ClaimDetailViewModel } from '@app/viewmodels';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { contentAnimation } from '@assets/animations';
import { scrollbarStyle } from '@assets/scrollbar';
import { Permission, PermissionDataModel } from '@app/helpers/permissions.helper';
import { BroadcastService } from '@app/services/broadcast.service';
import { takeUntil, finalize } from 'rxjs/operators';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { EventService } from '@app/services/event.service';
import { PopoverViewComponent } from '@app/components/popover-view/popover-view.component';
import { ClaimHelper } from '@app/helpers/claim.helper';
import { ClaimResourceModel, ClaimVersionListResourceModel } from '@app/resource-models/claim.resource-model';
import { SharedPopoverService } from '@app/services/shared-popover.service';
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import { QuestionViewModel } from '@app/viewmodels/question-view.viewmodel';
import { RepeatingQuestionViewModel } from '@app/viewmodels/repeating-question.viewmodel';
import { QuestionViewModelGenerator } from '@app/helpers/question-view-model-generator';
import { DocumentApiService } from '@app/services/api/document-api.service';
import { saveAs } from 'file-saver';
import { EmailApiService } from '@app/services/api/email-api.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { EntityType } from "@app/models/entity-type.enum";
import { ProductApiService } from '@app/services/api/product-api.service';
import { AppConfig } from '@app/models/app-config';
import { AppConfigService } from '@app/services/app-config.service';
import { PermissionService } from '@app/services/permission.service';
import { PortalPageTriggerResourceModel } from '@app/resource-models/portal-page-trigger.resource-model';
import { PortalExtensionsService } from '@app/services/portal-extensions.service';
import { FeatureSettingService } from '@app/services/feature-setting.service';
import { ActionButtonPopover } from '@app/models/action-button-popover';
import { ActionButton } from '@app/models/action-button';
import { IconLibrary } from '@app/models/icon-library.enum';
import { ActionButtonHelper } from '@app/helpers/action-button.helper';
import { PopoverCommand } from '@app/models/popover-command';
import { PageType } from '@app/models/page-type.enum';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { AlertController, ToastController } from '@ionic/angular';
import { ClaimStateChangedModel } from '@app/models/claim-state-changed.model';
import { ClaimAction } from '@app/models/claim-action.enum';

/**
 * Export detail claim page component class
 * This class is used to display claim details.
 */
@Component({
    selector: 'app-detail-claim',
    templateUrl: './detail-claim.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-detail.css',
    ],
    styles: [
        scrollbarStyle,
    ],
})
export class DetailClaimPage extends DetailPage implements OnInit, OnDestroy {
    public showUpdateClaimActions: boolean = true;
    public segment: string = 'Details';
    public actions: Array<ActionButtonPopover> = [];
    public displayType: string = QuestionViewModelGenerator.type.Claim;
    public permission: typeof Permission = Permission;
    public model: ClaimDetailViewModel;
    public versionsList: Array<ClaimVersionListResourceModel> = null;
    public questionItems: Array<QuestionViewModel>;
    public repeatingQuestionItems: Array<RepeatingQuestionViewModel>;
    public documentHeaders: Array<string> = [];
    public detailsListItems: Array<DetailsListItem>;
    private downloading: any = {};
    public claimId: string;
    public entityTypes: typeof EntityType = EntityType;
    public permissionModel: PermissionDataModel;
    private isMutual: boolean;
    protected portalPageTriggers: Array<PortalPageTriggerResourceModel>;
    public canGoBack: boolean = true;
    protected hasActionsIncludedInMenu: boolean = false;
    public actionButtonList: Array<ActionButton>;
    public flipMoreIcon: boolean = false;
    public iconLibrary: typeof IconLibrary = IconLibrary;
    private canMangeClaims: boolean = false;
    private canAcknowledgeClaims: boolean = false;
    private canReviewClaims: boolean = false;
    private canAssessClaims: boolean = false;
    private canSettleClaims: boolean = false;
    private canEditAdditionalProp: boolean = false;
    private canManageClaimNumbers: boolean = false;

    public constructor(
        public navProxy: NavProxyService,
        private authService: AuthenticationService,
        private route: ActivatedRoute,
        private claimApiService: ClaimApiService,
        private broadCastService: BroadcastService,
        private permissionService: PermissionService,
        public layoutManager: LayoutManagerService,
        protected userPath: UserTypePathHelper,
        private sharedPopoverService: SharedPopoverService,
        eventService: EventService,
        elementRef: ElementRef,
        injector: Injector,
        protected documentApiService: DocumentApiService,
        protected emailApiService: EmailApiService,
        private routeHelper: RouteHelper,
        protected featureSettingService: FeatureSettingService,
        protected productApiService: ProductApiService,
        protected appConfigService: AppConfigService,
        private portalExtensionService: PortalExtensionsService,
        protected sharedAlertService: SharedAlertService,
        private toastCtrl: ToastController,
        private alertCtrl: AlertController,
    ) {
        super(eventService, elementRef, injector);

        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.isMutual = appConfig.portal.isMutual;
        });
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.canGoBack = this.routeHelper.getPathSegments()[1] != 'claim';
        this.claimId = this.routeHelper.getParam('claimId');
        this.segment = this.routeHelper.getParam('segment') || this.segment;
        this.canMangeClaims = this.permissionService.hasPermission(this.permission.ManageClaims);
        this.canReviewClaims = this.permissionService.hasPermission(this.permission.ReviewClaims);
        this.canAssessClaims = this.permissionService.hasPermission(this.permission.AssessClaims);
        this.canAcknowledgeClaims = this.permissionService.hasPermission(this.permission.AcknowledgeClaimNotifications);
        this.canSettleClaims = this.permissionService.hasPermission(this.permission.SettleClaims);
        this.canEditAdditionalProp = this.permissionService.hasPermission(this.permission.EditAdditionalPropertyValues);
        this.canManageClaimNumbers = this.permissionService.hasPermission(this.permission.ManageClaimNumbers);
        this.load();
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    private async preparePortalExtensions(): Promise<void> {
        this.portalPageTriggers =
            await this.portalExtensionService.getEnabledPortalPageTriggers(
                this.authService,
                this.entityTypes.Claim,
                PageType.Display,
                this.segment,
            );
        // Add portal page trigger actions
        this.actions =
            this.portalExtensionService.getActionButtonPopovers(this.portalPageTriggers);
    }

    protected executePortalPageTrigger(trigger: PortalPageTriggerResourceModel): void {
        this.portalExtensionService.executePortalPageTrigger(
            trigger,
            this.entityTypes.Claim,
            PageType.Display,
            this.segment,
            this.claimId,
        );
    }

    public update(): void {
        this.navProxy.navigateForward(
            [this.userPath.claim, this.model.id, 'update'],
            true,
            {
                queryParams: {
                    productAlias: this.model.productId,
                },
            },
        );
    }

    public goBackOrClose(): void {
        this.navProxy.navigateBackOne();
    }

    public async presentPopover(event: any): Promise<void> {
        this.flipMoreIcon = true;
        const popoverDismissAction = (command: PopoverCommand): void => {
            this.flipMoreIcon = false;
            if (!(command && command.data)) {
                // none was selected
                return;
            }
            const action: ActionButtonPopover = command.data.action;
            const params: any = {
                queryParams: {
                    productAlias: this.model.productId,
                    status: this.model.status,
                },
            };
            switch (action.actionName) {
                case ClaimAction.AssignClaimNumber:
                    this.navigateForward('number-assign', params);
                    break;
                case ClaimAction.UpdateClaimNumber:
                    this.navigateForward('number-update',
                        { queryParams: {
                            claimNumber: this.model.claimNumber,
                        } },
                    );
                    break;
                case ClaimAction.AcknowledgeNotification:
                    this.navigateForward('notification-acknowledge', params);
                    break;
                case ClaimAction.ReviewClaim:
                    this.navigateForward('review', params);
                    break;
                case ClaimAction.AssessClaim:
                    this.navigateForward('assess', params);
                    break;
                case ClaimAction.SettleClaim:
                    this.navigateForward('settle', params);
                    break;
                case ClaimAction.EditClaimProperties:
                    this.editClaimProperties();
                    break;
                case ClaimAction.ResumeClaim:
                    this.update();
                    break;
                case ClaimAction.WithdrawClaim:
                    this.confirmClaimWithdrawal();
                    break;
                default:
                    if (action.portalPageTrigger) {
                        this.portalExtensionService.executePortalPageTrigger(
                            action.portalPageTrigger,
                            this.entityTypes.Claim,
                            PageType.Display,
                            this.segment,
                            this.claimId,
                        );
                    }
                    break;
            }
        };

        await this.sharedPopoverService.show(
            {
                component: PopoverViewComponent,
                cssClass: 'custom-popover more-button-top-popover-positioning',
                componentProps: { actions: this.actions },
                event: event,
            },
            'Claim option popover',
            popoverDismissAction,
        );
    }

    public handleSegmentClick($event: any): void {
        if ($event.detail.value != this.segment) {
            this.segment = $event.detail.value;
        }

        switch (this.segment) {
            case 'Versions':
                this.isLoading = true;
                this.loadVersions();
                break;
        }
    }

    public gotoVersionDetail(versionItem: ClaimVersionListResourceModel): void {
        const id: string = this.route.snapshot.paramMap.get('claimId');
        this.navProxy.navigateForward(['claim', id, 'version', versionItem.claimVersionId]);
    }

    public download(documentId: string, fileName: string): void {
        if (this.downloading[documentId]) {
            return;
        }

        this.downloading[documentId] = true;
        this.documentApiService
            .downloadClaimDocument(documentId, this.model.id)
            .pipe(
                finalize(() => this.downloading[documentId] = false),
            )
            .subscribe(
                (blob: any) => {
                    saveAs(blob, fileName);
                },
            );
    }

    private load(): void {
        this.isLoading = true;
        try {
            this.claimApiService.getById(this.claimId)
                .pipe(
                    takeUntil(this.destroyed),
                    finalize(() => this.isLoading = false))
                .subscribe(
                    (dt: ClaimResourceModel) => {
                        this.model = new ClaimDetailViewModel(dt);
                        this.permissionModel = {
                            organisationId: this.model.organisationId,
                            customerId: this.model.customerId,
                            ownerUserId: this.model.ownerUserId,
                        };
                        this.documentHeaders = Array.from(
                            new Set(this.model.documents.map((item: any) => item.dateGroupHeader)),
                        );
                        this.initializeDetailsListItems();
                        this.broadCastService.broadcast('selectedClaimIdChange', this.model.id);
                        this.showUpdateClaimActions = ClaimHelper.canShowUpdateButton(this.model.status);
                        this.repeatingQuestionItems = QuestionViewModelGenerator.getRepeatingData(
                            this.model.formData,
                            dt.displayableFieldsModel,
                            this.model.questionAttachmentKeys,
                        );
                        this.questionItems = QuestionViewModelGenerator.getFormDataQuestionItems(
                            this.model.formData,
                            this.model.questionAttachmentKeys,
                            dt.displayableFieldsModel,
                        );
                        this.preparePortalExtensions().then(() => this.generatePopoverLinks());
                    },
                );
        } catch (error) {
            this.isLoading = false;
            this.errorMessage = 'There was a problem loading the claim details';
            throw error;
        }
    }

    private initializeDetailsListItems(): void {
        this.detailsListItems = this.model.createDetailsList(
            this.navProxy,
            this.authService.isCustomer(),
            this.isMutual,
            this.authService.tenantAlias,
        );
    }

    private async loadVersions(): Promise<void> {
        this.isLoading = true;
        try {
            this.versionsList = await this.claimApiService.getClaimVersions(this.claimId).toPromise();
            this.isLoading = false;
        } catch (error) {
            this.errorMessage = 'There was a problem loading claim versions';
            throw error;
        } finally {
            this.isLoading = false;
        }
    }

    private generatePopoverLinks(): void {
        if (!this.showUpdateClaimActions) {
            this.actionButtonList = [];
            return;
        }
        switch (this.model.status.toLowerCase()) {
            case ClaimHelper.status.Notified.toLowerCase():
                if (this.canAcknowledgeClaims) {
                    this.actions.push({
                        actionName: ClaimAction.AcknowledgeNotification,
                        actionIcon: "clipboard-check",
                        iconLibrary: IconLibrary.AngularMaterial,
                        actionButtonLabel: "",
                        actionButtonPrimary: false,
                        includeInMenu: true,
                    });
                }
                break;
            case ClaimHelper.status.Review.toLowerCase():
                if (this.canReviewClaims) {
                    this.actions.push({
                        actionName: ClaimAction.ReviewClaim,
                        actionIcon: "feature-search",
                        iconLibrary: IconLibrary.AngularMaterial,
                        actionButtonLabel: "",
                        actionButtonPrimary: false,
                        includeInMenu: true,
                    });
                }
                break;
            case ClaimHelper.status.Assessment.toLowerCase():
                if (this.canAssessClaims) {
                    this.actions.push({
                        actionName: ClaimAction.AssessClaim,
                        actionIcon: "ruler",
                        iconLibrary: IconLibrary.AngularMaterial,
                        actionButtonLabel: "",
                        actionButtonPrimary: false,
                        includeInMenu: true,
                    });
                }
                break;
            case ClaimHelper.status.Approved.toLowerCase():
                if (this.canSettleClaims) {
                    this.actions.push({
                        actionName: ClaimAction.SettleClaim,
                        actionIcon: "account-cash",
                        iconLibrary: IconLibrary.AngularMaterial,
                        actionButtonLabel: "",
                        actionButtonPrimary: false,
                        includeInMenu: true,
                    });
                }
                break;
            default:
                break;
        }

        if (this.canMangeClaims) {
            this.actions.push({
                actionName: ClaimAction.ResumeClaim,
                actionIcon: "resume-quote",
                iconLibrary: IconLibrary.AngularMaterial,
                actionButtonLabel: "",
                actionButtonPrimary: false,
                includeInMenu: true,
            });
            this.actions.push({
                actionName: ClaimAction.WithdrawClaim,
                actionIcon: "arrow-u-left-bottom",
                iconLibrary: IconLibrary.AngularMaterial,
                actionButtonLabel: "",
                actionButtonPrimary: false,
                includeInMenu: true,
            });
        }

        if (this.canManageClaimNumbers) {
            if (this.model.claimNumber === null || this.model.claimNumber === '') {
                this.actions.push({
                    actionName: ClaimAction.AssignClaimNumber,
                    actionIcon: 'pound',
                    iconLibrary: IconLibrary.AngularMaterial,
                    actionButtonLabel: "",
                    actionButtonPrimary: false,
                    includeInMenu: true,
                });
            } else if (this.model.claimNumber !== null) {
                this.actions.push({
                    actionName: ClaimAction.UpdateClaimNumber,
                    actionIcon: "pound",
                    iconLibrary: IconLibrary.AngularMaterial,
                    actionButtonLabel: "",
                    actionButtonPrimary: false,
                    includeInMenu: true,
                });
            }
        }

        if (this.canEditAdditionalProp) {
            this.actions.push({
                actionName: ClaimAction.EditClaimProperties,
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

    private initializeActionButtonList(): void {
        let actionButtonList: Array<ActionButton> = [];
        const params: any = {
            queryParams: {
                productAlias: this.model.productId,
                status: this.model.status,
            },
        };

        if (this.model.status.toLowerCase() == ClaimHelper.status.Review.toLowerCase() &&
            this.canReviewClaims) {
            actionButtonList.push(ActionButton.createActionButton(
                "Review",
                "feature-search",
                IconLibrary.AngularMaterial,
                true,
                "Review Claim",
                true,
                (): void => {
                    return this.navigateForward('review', params);
                },
            ));
        }

        if (this.model.status.toLowerCase() == ClaimHelper.status.Notified.toLowerCase() &&
            this.canAcknowledgeClaims) {
            actionButtonList.push(ActionButton.createActionButton(
                "Acknowledge",
                "clipboard-check",
                IconLibrary.AngularMaterial,
                true,
                "Acknowledge Notification",
                true,
                (): void => {
                    return this.navigateForward('notification-acknowledge', params);
                },
            ));
        }

        if (this.model.status.toLowerCase() == ClaimHelper.status.Assessment.toLowerCase() &&
            this.canAssessClaims) {
            actionButtonList.push(ActionButton.createActionButton(
                "Assess",
                "ruler",
                IconLibrary.AngularMaterial,
                true,
                "Assess Claim",
                true,
                (): void => {
                    return this.navigateForward('assess', params);
                },
            ));
        }

        if ((this.model.status.toLowerCase() == ClaimHelper.status.Settlement.toLowerCase() ||
             this.model.status.toLowerCase() == ClaimHelper.status.Approved.toLowerCase()) &&
            this.canSettleClaims) {
            actionButtonList.push(ActionButton.createActionButton(
                "Settle",
                "account-cash",
                IconLibrary.AngularMaterial,
                true,
                "Settle Claim",
                true,
                (): void => {
                    return this.navigateForward('settle',params);
                },
            ));
        }

        if (this.canMangeClaims) {
            const emphasisOnResume: boolean = actionButtonList.length === 0;
            actionButtonList.push(ActionButton.createActionButton(
                "Resume",
                "resume-quote",
                IconLibrary.AngularMaterial,
                emphasisOnResume,
                "Resume Claim",
                true,
                (): void => {
                    return this.update();
                },
            ));
            actionButtonList.push(ActionButton.createActionButton(
                "Withdraw",
                "arrow-u-left-bottom",
                IconLibrary.AngularMaterial,
                false,
                "Withdraw Claim",
                true,
                (): void => {
                    this.confirmClaimWithdrawal();
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
                            this.entityTypes.Claim,
                            PageType.Display,
                            this.segment,
                            this.claimId,
                        );
                    },
                ));
            }
        }

        this.actionButtonList = ActionButtonHelper.sortActionButtons(actionButtonList);
    }

    private async confirmClaimWithdrawal(): Promise<void> {
        const alert: HTMLIonAlertElement = await this.alertCtrl.create({
            id: 'claim-withdraw',
            header: 'Withdraw Claim',
            message: 'Are you sure you wish to withdraw this claim?',
            buttons: [{
                text: 'Cancel',
                role: 'cancel',
            },
            {
                text: 'Withdraw',
                handler: (): any => {
                    this.claimApiService.withdrawClaim(this.model.id)
                        .pipe(takeUntil(this.destroyed))
                        .subscribe((data: ClaimResourceModel) => {
                            return this.showWithdrawSnackbar().then(() => {
                                const claimState: ClaimStateChangedModel = {
                                    claimId: data.id,
                                    previousClaimState: data.status,
                                    newClaimState: ClaimHelper.status.Withdrawn,
                                };
                                this.eventService.claimStateChanged(claimState);
                                this.eventService.getEntityUpdatedSubject('Claim').next(data);
                                this.load();
                            });
                        });
                },
            }],
        });

        await alert.present();
    }

    private async showWithdrawSnackbar(): Promise<void> {
        const snackbar: HTMLIonToastElement = await this.toastCtrl.create({
            id: this.model.id,
            message: 'Claim successfully withdrawn',
            duration: 3000,
        });

        return await snackbar.present();
    }

    private navigateForward(page: string, params: any): void {
        this.navProxy.navigateForward(
            [
                'claim',
                this.model.id,
                page,
            ],
            true,
            params,
        );
    }

    private editClaimProperties(): void {
        if (this.model.additionalPropertyValues?.length > 0) {
            this.navProxy.goToAdditionalPropertyValues(EntityType.Claim);
        } else {
            this.sharedAlertService.showWithOk(
                "No additional properties defined",
                "Claims in this context do not have any additional properties that can be edited.",
            );
        }
    }
}
