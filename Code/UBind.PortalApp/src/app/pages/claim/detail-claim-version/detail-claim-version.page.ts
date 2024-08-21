import { Component, ElementRef, Injector, OnDestroy, OnInit } from '@angular/core';
import { Permission, ClaimHelper } from '@app/helpers';
import { ActivatedRoute } from '@angular/router';
import { contentAnimation } from '@assets/animations';
import { ClaimApiService } from '@app/services/api/claim-api.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { AppConfigService } from '@app/services/app-config.service';
import { AppConfig } from '@app/models/app-config';
import { DetailPage } from '@pages/master-detail/detail.page';
import { EventService } from '@app/services/event.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { takeUntil, finalize } from 'rxjs/operators';
import { QuestionViewModel } from '@app/viewmodels/question-view.viewmodel';
import { RepeatingQuestionViewModel } from '@app/viewmodels/repeating-question.viewmodel';
import { QuestionViewModelGenerator } from '@app/helpers/question-view-model-generator';
import { AuthenticationService } from '@app/services/authentication.service';
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import { ClaimVersionDetailViewModel } from '@app/viewmodels/claim-version-detail.viewmodel';
import { Subject, Subscription } from 'rxjs';
import { DocumentApiService } from '@app/services/api/document-api.service';
import { saveAs } from 'file-saver';
import { ClaimVersionResourceModel } from '@app/resource-models/claim.resource-model';
import { HttpErrorResponse } from '@angular/common/http';
import { PermissionService } from '@app/services/permission.service';
import { SharedPopoverService } from '@app/services/shared-popover.service';
import { PopoverViewComponent } from '@app/components/popover-view/popover-view.component';
import { EntityType } from '@app/models/entity-type.enum';
import { RouteHelper } from '@app/helpers/route.helper';
import { PortalExtensionsService } from '@app/services/portal-extensions.service';
import { ActionButtonPopover } from '@app/models/action-button-popover';
import { ActionButton } from '@app/models/action-button';
import { IconLibrary } from '@app/models/icon-library.enum';
import { ActionButtonHelper } from '@app/helpers/action-button.helper';
import { PortalPageTriggerResourceModel } from '@app/resource-models/portal-page-trigger.resource-model';
import { PageType } from '@app/models/page-type.enum';

/**
 * Export detail claim version page component class
 * This class manage to displaying of the details of claim versions.
 */
@Component({
    selector: 'app-detail-claim-version',
    templateUrl: './detail-claim-version.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-detail.css',
    ],
})
export class DetailClaimVersionPage extends DetailPage implements OnInit, OnDestroy {
    public showUpdateClaim: boolean;
    public displayType: string = QuestionViewModelGenerator.type.ClaimVersion;
    public segment: string = 'Details';
    public portalBaseUrl: string = null;

    private titlePrefix: string = 'Claim Version: ';
    private versionTitle: string;
    public title: string;

    public versionData: ClaimVersionDetailViewModel;
    public permission: typeof Permission = Permission;
    public questionItems: Array<QuestionViewModel>;
    public repeatingQuestionItems: Array<RepeatingQuestionViewModel>;
    public documentHeaders: Array<string> = [];
    public detailsListItems: Array<DetailsListItem>;
    private downloading: any = {};
    private downloadClaimDocumentSubscription: Subscription;
    public shouldShowEditAdditionalProperties: boolean = false;
    private actions: Array<ActionButtonPopover> = [];
    private isMutual: boolean;
    private readonly additionalPropertyPopOverLabel: string = "Edit Claim Version Properties";
    public entityTypes: typeof EntityType = EntityType;
    public claimVersionId: string;
    protected portalPageTriggers: Array<PortalPageTriggerResourceModel>;
    protected hasActionsIncludedInMenu: boolean = false;
    public actionButtonList: Array<ActionButton>;
    public flipMoreIcon: boolean = false;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        private claimApiService: ClaimApiService,
        private router: ActivatedRoute,
        public navProxy: NavProxyService,
        protected appConfigService: AppConfigService,
        private userPath: UserTypePathHelper,
        public layoutManager: LayoutManagerService,
        eventService: EventService,
        elementRef: ElementRef,
        public injector: Injector,
        private authService: AuthenticationService,
        protected documentApiService: DocumentApiService,
        private permissionService: PermissionService,
        private sharedPopoverService: SharedPopoverService,
        private routeHelper: RouteHelper,
        private portalExtensionService: PortalExtensionsService,
    ) {
        super(eventService, elementRef, injector);
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.portalBaseUrl = appConfig.portal.api.baseUrl;
            this.isMutual = appConfig.portal.isMutual;
        });
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.segment = this.routeHelper.getParam('segment') || 'Details';
        this.load();
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    public update(): void {
        this.navProxy.navigate(
            [
                this.userPath.claim,
                this.versionData.claimId,
                'version',
                this.versionData.versionNumber,
                'update'],
            {
                queryParams: {
                    productAlias: this.versionData.productId,
                },
            },
        );
    }

    public handleSegmentClick($event: any): void {
        if ($event.detail.value != this.segment) {
            this.segment = $event.detail.value;
        }
        switch (this.segment) {
            case 'Details':
                this.load();
                break;
            case 'Questions':
                break;
            case 'Amounts':
                break;
            default: break;
        }
    }

    private async preparePortalExtensions(): Promise<void> {
        this.portalPageTriggers =
            await this.portalExtensionService.getEnabledPortalPageTriggers(
                this.authService,
                this.entityTypes.ClaimVersion,
                PageType.Display,
                this.segment,
            );
    }

    private generatePopoverLinks(): void {
        // Add portal page trigger actions
        const triggerActions: Array<ActionButtonPopover>
            = this.portalExtensionService.getActionButtonPopovers(this.portalPageTriggers);
        this.actions = this.actions.concat(triggerActions);
        this.hasActionsIncludedInMenu = this.actions.filter((x: ActionButtonPopover) => x.includeInMenu).length > 0;
        this.initializeActionButtonList();
    }

    protected executePortalPageTrigger(trigger: PortalPageTriggerResourceModel): void {
        this.portalExtensionService.executePortalPageTrigger(
            trigger,
            this.entityTypes.ClaimVersion,
            PageType.Display,
            this.segment,
            this.claimVersionId,
        );
    }

    public goBackOrClose(): void {
        this.navProxy.navigate(
            [this.userPath.claim, this.versionData.claimId],
            {
                queryParams:
                {
                    segment: 'Versions',
                },
            },
        );
    }

    public download(documentId: string, fileName: string): void {
        if (this.downloading[documentId]) {
            return;
        }

        this.downloading[documentId] = true;
        this.downloadClaimDocumentSubscription = this.documentApiService
            .downloadClaimDocument(documentId, this.versionData.claimId)
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
        this.claimVersionId = this.router.snapshot.paramMap.get('claimVersionId');
        const claimId: string = this.router.snapshot.paramMap.get('claimId');
        this.claimApiService.getClaimVersionDetail(claimId, this.claimVersionId)
            .pipe(
                takeUntil(this.destroyed),
                finalize(() => this.isLoading = false),
            )
            .subscribe(async (data: ClaimVersionResourceModel) => {
                this.versionData = new ClaimVersionDetailViewModel(data);
                this.documentHeaders = Array.from(
                    new Set(this.versionData.documents.map((item: any) => item.dateGroupHeader)),
                );
                this.initializeDetailsListItems();
                this.showUpdateClaim = ClaimHelper.canShowUpdateButton(data.status);
                const formModel: any = data.formData ? data.formData.formModel : {};
                this.repeatingQuestionItems = QuestionViewModelGenerator.getRepeatingData(
                    formModel,
                    data.displayableFieldsModel,
                    data.questionAttachmentKeys,
                );
                this.questionItems = QuestionViewModelGenerator.getFormDataQuestionItems(
                    formModel,
                    data.questionAttachmentKeys,
                    data.displayableFieldsModel,
                );
                this.updateTitle();
                await this.preparePortalExtensions();
                this.shouldShowEditAdditionalProperties =
                    (data.additionalPropertyValues?.length > 0
                        && this.permissionService.hasPermission(this.permission.EditAdditionalPropertyValues));
                if (this.shouldShowEditAdditionalProperties) {
                    this.actions.push({
                        actionName: this.additionalPropertyPopOverLabel,
                        actionIcon: "brush",
                        iconLibrary: IconLibrary.IonicV4,
                        actionButtonLabel: "",
                        actionButtonPrimary: false,
                        includeInMenu: true,
                    });
                }
                this.generatePopoverLinks();
            }, (err: HttpErrorResponse) => {
                this.errorMessage = 'There was a problem loading the claim version details';
                throw err;
            });
    }

    private initializeDetailsListItems(): void {
        this.detailsListItems = this.versionData.createDetailsList(
            this.navProxy,
            this.authService.isCustomer(),
            this.isMutual,
            this.authService.tenantAlias,
        );
    }
    private updateTitle(): void {
        if (this.versionData.claimNumber) {
            this.versionTitle = this.versionData.claimNumber;
        } else {
            this.versionTitle = this.versionData.claimReference + '-' + this.versionData.versionNumber;
        }
        this.title = this.titlePrefix + this.versionTitle;
    }

    public async presentPopover(event: any): Promise<void> {
        this.flipMoreIcon = true;
        const popoverDismissAction = (command: any): void => {
            this.flipMoreIcon = false;
            if (command && command.data && command.data.action && this.actions.includes(command.data.action)) {
                const action: ActionButtonPopover = command.data.action;
                switch (action.actionName) {
                    case this.additionalPropertyPopOverLabel:
                        this.navProxy.goToAdditionalPropertyValues(EntityType.ClaimVersion);
                        break;
                    default:
                        if (action.portalPageTrigger) {
                            this.portalExtensionService.executePortalPageTrigger(
                                action.portalPageTrigger,
                                this.entityTypes.ClaimVersion,
                                PageType.Display,
                                this.segment,
                                this.claimVersionId,
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
                componentProps: { actions: this.actions },
                event: event,
            },
            'Claim option popover',
            popoverDismissAction,
        );
    }

    private initializeActionButtonList(): void {
        let actionButtonList: Array<ActionButton> = [];
        if (this.showUpdateClaim && this.permissionService.hasPermission(Permission.ManageClaims)) {
            actionButtonList.push(ActionButton.createActionButton(
                "Edit",
                "pencil",
                IconLibrary.AngularMaterial,
                false,
                "Edit Claim Version",
                true,
                (): void => {
                    return this.update();
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
                            this.entityTypes.ClaimVersion,
                            PageType.Display,
                            this.segment,
                            this.claimVersionId,
                        );
                    },
                ));
            }
        }

        this.actionButtonList = ActionButtonHelper.sortActionButtons(actionButtonList);
    }
}
