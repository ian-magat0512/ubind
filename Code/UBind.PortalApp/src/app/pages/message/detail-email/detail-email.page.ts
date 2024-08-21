import { Component, Injector, ElementRef, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { saveAs } from 'file-saver';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { EmailResourceModel } from '@app/resource-models/email.resource-model';
import { EmailApiService } from '@app/services/api/email-api.service';
import { DocumentApiService } from '@app/services/api/document-api.service';
import { contentAnimation } from '@assets/animations';
import { scrollbarStyle } from '@assets/scrollbar';
import { FeatureSettingService } from '@app/services/feature-setting.service';
import { LocalDateHelper, Permission } from '@app/helpers';
import { BroadcastService } from '@app/services/broadcast.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { EventService } from '@app/services/event.service';
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import { EmailDetailViewModel } from '@app/viewmodels/email-detail.viewmodel';
import { AuthenticationService } from '@app/services/authentication.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { PortalPageTriggerResourceModel } from '@app/resource-models/portal-page-trigger.resource-model';
import { EntityType } from '@app/models/entity-type.enum';
import { PortalExtensionsService } from '@app/services/portal-extensions.service';
import { SharedPopoverService } from '@app/services/shared-popover.service';
import { PopoverViewComponent } from '@app/components/popover-view/popover-view.component';
import { ActionButtonPopover } from '@app/models/action-button-popover';
import { ActionButton } from '@app/models/action-button';
import { IconLibrary } from '@app/models/icon-library.enum';
import { ActionButtonHelper } from '@app/helpers/action-button.helper';
import { PopoverCommand } from '@app/models/popover-command';
import { PageType } from '@app/models/page-type.enum';

/**
 * Component for displaying detailed email information.
 * Manages the display of email details, including attachments and actions.
 */
@Component({
    selector: 'app-detail-email',
    templateUrl: './detail-email.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-detail.css',
        './detail-email.page.scss',
    ],
    styles: [scrollbarStyle],
})
export class DetailEmailPage extends DetailPage implements OnInit {
    public segment: string = 'Details';
    public questionHeaders: Array<string> = [];
    public questionData: any = {};
    public repeatingData: Array<any> = [];
    public detailsListItems: Array<DetailsListItem>;

    public quoteEmailId: string;
    public currentIndex: number;
    public redirectUrl: any;
    public email: EmailDetailViewModel;
    public portal: any = null;
    public permission: typeof Permission = Permission;
    public isTopLevelEmailPage: boolean = true;

    private downloading: any = {};

    private entityTypes: typeof EntityType = EntityType;
    protected portalPageTriggers: Array<PortalPageTriggerResourceModel>;
    protected actions: Array<ActionButtonPopover> = [];
    public canGoBack: boolean = true;
    protected hasActionsIncludedInMenu: boolean = false;
    public actionButtonList: Array<ActionButton>;
    public flipMoreIcon: boolean = false;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        public navProxy: NavProxyService,
        private route: ActivatedRoute,
        private emailApiService: EmailApiService,
        private featureSettingService: FeatureSettingService,
        private broadcastService: BroadcastService,
        public documentApiService: DocumentApiService,
        public layoutManager: LayoutManagerService,
        protected userPath: UserTypePathHelper,
        eventService: EventService,
        elementRef: ElementRef,
        injector: Injector,
        private authService: AuthenticationService,
        private routeHelper: RouteHelper,
        private portalExtensionService: PortalExtensionsService,
        protected sharedPopoverService: SharedPopoverService,
    ) {
        super(eventService, elementRef, injector);
    }

    public async ngOnInit(): Promise<void> {
        this.quoteEmailId = this.routeHelper.getParam('id');
        const pathSegmentsAfter: Array<string> = this.routeHelper.getPathSegmentsAfter('path');
        this.canGoBack = pathSegmentsAfter[0] != 'message';
        this.isTopLevelEmailPage = pathSegmentsAfter[0] == this.userPath.message;
        try {
            this.isLoading = true;
            const dt: EmailResourceModel = await this.emailApiService.getById(this.quoteEmailId)
                .toPromise();
            this.email = new EmailDetailViewModel(dt);
            this.initializeDetailsListItems();
            this.broadcastService.dispatchEvent('selectedEmailChange', this.email.id);
            this.email.localTime = LocalDateHelper.convertToLocalAndGetTimeOnly(this.email.createdTime);
            this.preparePortalExtensions().then(() => this.generatePopoverLinks());
        } catch (error) {
            this.errorMessage = 'There was a problem loading the email details';
            throw error;
        } finally {
            this.isLoading = false;
        }
    }

    private async preparePortalExtensions(): Promise<void> {
        this.portalPageTriggers =
            await this.portalExtensionService.getEnabledPortalPageTriggers(
                this.authService,
                this.entityTypes.EmailMessage,
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
            this.entityTypes.EmailMessage,
            PageType.Display,
            this.segment,
            this.quoteEmailId,
        );
    }

    public async presentPopover(event: any): Promise<void> {
        this.flipMoreIcon = true;
        const popoverDismissAction = (command: PopoverCommand): void => {
            this.flipMoreIcon = false;
            if (command && command.data && command.data.action && command.data.action.portalPageTrigger) {
                this.portalExtensionService.executePortalPageTrigger(
                    command.data.action.portalPageTrigger,
                    this.entityTypes.EmailMessage,
                    PageType.Display,
                    this.segment,
                    this.quoteEmailId,
                );
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
            'Email option popover',
            popoverDismissAction,
        );
    }

    private initializeDetailsListItems(): void {
        this.detailsListItems = this.email.createDetailsList(
            this.navProxy,
            this.authService.isCustomer(),
            this.authService.isMutualTenant(),
        );
    }

    public hasHtmlMessage(): boolean {
        return !!this.email && !!this.email.htmlMessage;
    }

    public goBack(): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        let modulePathSegment: Array<string> = this.routeHelper.getModulePathSegments();

        // pop the message id
        pathSegments.pop();
        // pop the message type
        pathSegments.pop();

        if (!(modulePathSegment[modulePathSegment.length - 1].indexOf('message') > -1)) {
        // pop the message segment
            pathSegments.pop();
        }

        this.navProxy.navigate(pathSegments, { queryParams: { segment: "Messages" } });
    }

    public download(attachmentId: string, fileName: string): void {
        if (this.downloading[attachmentId]) {
            return;
        }

        this.downloading[attachmentId] = true;
        this.emailApiService
            .downloadAttachment(this.quoteEmailId, attachmentId)
            .pipe(finalize(() => this.downloading[attachmentId] = false))
            .subscribe(
                (blob: any): void => {
                    saveAs(blob, fileName);
                },
            );
    }

    private initializeActionButtonList(): void {
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
                            this.entityTypes.EmailMessage,
                            PageType.Display,
                            this.segment,
                            this.quoteEmailId,
                        );
                    },
                ));
            }
        }

        this.actionButtonList = ActionButtonHelper.sortActionButtons(actionButtonList);
    }

    public segmentChanged($event: any): void {
        if ($event.detail.value != this.segment) {
            this.segment = $event.detail.value;
        }
    }
}
