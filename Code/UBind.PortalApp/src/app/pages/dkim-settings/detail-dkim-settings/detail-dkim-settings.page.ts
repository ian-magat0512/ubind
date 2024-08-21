import { Component, ElementRef, Injector, OnDestroy, OnInit } from '@angular/core';
import { Permission, PermissionDataModel } from '@app/helpers/permissions.helper';
import { RouteHelper } from '@app/helpers/route.helper';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { EventService } from '@app/services/event.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { contentAnimation } from '@assets/animations';
import { scrollbarStyle } from '@assets/scrollbar';
import { Subject } from 'rxjs';
import { finalize, takeUntil } from 'rxjs/operators';
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import { AuthenticationService } from '@app/services/authentication.service';
import { DkimSettingsApiService } from '@app/services/api/dkim-settings-api.service';
import { DkimSettingsResourceModel } from '@app/resource-models/dkim-settings.resource-model';
import { DkimSettingsDetailViewModel } from '@app/viewmodels/dkim-settings-detail.viewmodel';
import { SharedPopoverService } from '@app/services/shared-popover.service';
import { PopoverDkimSettingPage } from '../popover-dkim-settings.page';
import { ActionButton } from '@app/models/action-button';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Export detail DKIM setting page component class.
 * This class manage displaying the DKIM setting details.
 */
@Component({
    selector: 'app-detail-role',
    templateUrl: './detail-dkim-settings.page.html',
    animations: [contentAnimation],
    styleUrls: [
        './detail-dkim-settings.page.scss',
        '../../../../assets/css/scrollbar-div.css',
        '../../../../assets/css/scrollbar-detail.css',
    ],
    styles: [scrollbarStyle],
})
export class DetailDkimSettingPage extends DetailPage implements OnInit, OnDestroy {
    public dkimSettingsDetailViewModel: DkimSettingsDetailViewModel;
    public dkimSettingsResourceModel: DkimSettingsResourceModel;
    public permission: typeof Permission = Permission;
    private dkimSettingsId: string;
    private organisationId: string;
    public detailsListItems: Array<DetailsListItem>;
    public permissionModel: PermissionDataModel;
    public actionButtonList: Array<ActionButton>;
    public flipMoreIcon: boolean = false;

    public constructor(
        public navProxy: NavProxyService,
        private routeHelper: RouteHelper,
        protected eventService: EventService,
        private dkimSettingsApiService: DkimSettingsApiService,
        public layoutManager: LayoutManagerService,
        elementRef: ElementRef,
        injector: Injector,
        protected authService: AuthenticationService,
        private sharedPopoverService: SharedPopoverService,
    ) {
        super(eventService, elementRef, injector);
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.dkimSettingsId = this.routeHelper.getParam('dkimSettingsId');
        this.organisationId = this.routeHelper.getParam('organisationId');
        this.loadDkimSettings();
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    public async showMenu(event: any): Promise<void> {
        this.flipMoreIcon = true;
        const dismissAction = async (command: any): Promise<void> => {
            this.flipMoreIcon = false;
        };

        await this.sharedPopoverService.show(
            {
                component: PopoverDkimSettingPage,
                cssClass: 'custom-popover more-button-top-popover-positioning',
                event: event,
                componentProps: {
                    permissionModel: this.permissionModel,
                    dkimSettingsResourceModel: this.dkimSettingsResourceModel,
                },
            },
            'DKIM Settings option popover',
            dismissAction,
        );
    }

    private async loadDkimSettings(): Promise<void> {
        this.isLoading = true;
        this.dkimSettingsApiService.getDkimSettingsById(
            this.dkimSettingsId,
            this.organisationId,
            this.routeHelper.getContextTenantAlias())
            .pipe(
                takeUntil(this.destroyed),
                finalize(() => this.isLoading = false),
            )
            .subscribe((data: DkimSettingsResourceModel) => {
                this.dkimSettingsResourceModel = data;
                this.dkimSettingsDetailViewModel = new DkimSettingsDetailViewModel(data);
                this.permissionModel = {
                    organisationId: this.dkimSettingsResourceModel.organisationId,
                    ownerUserId: null,
                    customerId: null,
                };
                this.initializeDetailsListItems();
                this.initializeActionButtonList();
            }, (err: any) => {
                this.errorMessage = 'There was a problem loading the DKIM settings';
                throw err;
            });
    }

    private initializeDetailsListItems(): void {
        this.detailsListItems = this.dkimSettingsDetailViewModel.createDetailsList();
    }

    public goBack(): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.pop();
        pathSegments.push('list');
        this.navProxy.navigateBack(pathSegments, true);
    }

    public userDidTapEditButton(): void {
        const pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.push('edit');
        this.navProxy.navigateForward(pathSegments, true);
    }

    private initializeActionButtonList(): void {
        this.actionButtonList = [];

        this.actionButtonList.push(ActionButton.createActionButton(
            "Edit",
            "pencil",
            IconLibrary.AngularMaterial,
            false,
            "Edit DKIM",
            true,
            (): void => {
                return this.userDidTapEditButton();
            },
        ));
    }
}
