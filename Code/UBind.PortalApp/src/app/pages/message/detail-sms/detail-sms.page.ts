import { Component, ElementRef, Injector, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { SmsResourceModel } from '@app/resource-models/sms.resource-model';
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { SmsApiService } from '@app/services/api/sms-api.service';
import { AuthenticationService } from '@app/services/authentication.service';
import { EventService } from '@app/services/event.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { SmsDetailViewModel } from '@app/viewmodels/sms-detail.viewmodel';
import { contentAnimation } from '@assets/animations';
import { scrollbarStyle } from '@assets/scrollbar';
import { RouteHelper } from '@app/helpers/route.helper';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
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
 * Page class for sms detail.
 */
@Component({
    selector: 'app-detail-sms',
    templateUrl: './detail-sms.page.html',
    styleUrls: [
        '../../../../assets/css/scrollbar-detail.css',
        '../../../../assets/css/form-toolbar.scss',
    ],
    animations: [contentAnimation],
    styles: [scrollbarStyle],
})
export class DetailSmsPage extends DetailPage implements OnInit {

    public sms: SmsDetailViewModel;
    public detailsListItems: Array<DetailsListItem>;
    public smsId: string;
    public isTopLevelSmsPage: boolean = true;

    private entityTypes: typeof EntityType = EntityType;
    protected portalPageTriggers: Array<PortalPageTriggerResourceModel>;
    protected actions: Array<ActionButtonPopover> = [];
    public canGoBack: boolean = true;
    protected hasActionsIncludedInMenu: boolean = false;
    public actionButtonList: Array<ActionButton>;
    public flipMoreIcon: boolean = false;

    public constructor(
        protected eventService: EventService,
        elementRef: ElementRef,
        public injector: Injector,
        public layoutManager: LayoutManagerService,
        private smsApiService: SmsApiService,
        private route: ActivatedRoute,
        public navProxy: NavProxyService,
        private authService: AuthenticationService,
        private routeHelper: RouteHelper,
        protected userPath: UserTypePathHelper,
        private portalExtensionService: PortalExtensionsService,
        protected sharedPopoverService: SharedPopoverService,
    ) {
        super(eventService, elementRef, injector);
    }

    public async ngOnInit(): Promise<void> {
        this.smsId = this.route.snapshot.paramMap.get('id');
        const pathSegmentsAfter: Array<string> = this.routeHelper.getPathSegmentsAfter('path');
        this.canGoBack = pathSegmentsAfter[0] != 'message';
        this.preparePortalExtensions().then(() => this.generatePopoverLinks());
        this.isTopLevelSmsPage = pathSegmentsAfter[0] == this.userPath.message;

        try {
            const dt: SmsResourceModel = await this.smsApiService.getById(this.smsId)
                .toPromise();
            this.sms = new SmsDetailViewModel(dt);
            this.detailsListItems = this.sms.createDetailList(this.navProxy, this.authService.isCustomer());

        } catch (error) {
            this.errorMessage = 'There was a problem loading the sms details';
            throw error;
        } finally {
            this.isLoading = false;
        }
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

    private async preparePortalExtensions(): Promise<void> {
        this.portalPageTriggers =
            await this.portalExtensionService.getEnabledPortalPageTriggers(
                this.authService,
                this.entityTypes.SmsMessage,
                PageType.Display,
                null,
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
            this.entityTypes.SmsMessage,
            PageType.Display,
            null,
            this.smsId,
        );
    }

    public async presentPopover(event: any): Promise<void> {
        this.flipMoreIcon = true;
        const popoverDismissAction = (command: PopoverCommand): void => {
            this.flipMoreIcon = false;
            if (command && command.data && command.data.action && command.data.action.portalPageTrigger) {
                this.portalExtensionService.executePortalPageTrigger(
                    command.data.action.portalPageTrigger,
                    this.entityTypes.SmsMessage,
                    PageType.Display,
                    null,
                    this.smsId,
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
            'SMS option popover',
            popoverDismissAction,
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
                            this.entityTypes.SmsMessage,
                            PageType.Display,
                            null,
                            this.smsId,
                        );
                    },
                ));
            }
        }

        this.actionButtonList = ActionButtonHelper.sortActionButtons(actionButtonList);
    }
}
