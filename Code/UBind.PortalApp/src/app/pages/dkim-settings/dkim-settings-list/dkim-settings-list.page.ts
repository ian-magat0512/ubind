import { Component, Injector, ElementRef, OnInit } from '@angular/core';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { RouteHelper } from '../../../helpers/route.helper';
import { EventService } from '@app/services/event.service';
import { takeUntil } from 'rxjs/operators';
import { scrollbarStyle } from '@assets/scrollbar';
import { contentAnimation } from '@assets/animations';
import { SubscriptionLike } from 'rxjs';
import { DetailPage } from "@app/pages/master-detail/detail.page";
import { DkimSettingsApiService } from '@app/services/api/dkim-settings-api.service';
import { DkimSettingsResourceModel } from '@app/resource-models/dkim-settings.resource-model';
import { Subject } from 'rxjs';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { ActionButton } from '@app/models/action-button';
import { IconLibrary } from '@app/models/icon-library.enum';
import { AuthenticationService } from '@app/services/authentication.service';

/**
 * DKIM Settings list page component.
 */
@Component({
    selector: 'app-additional-properties',
    templateUrl: './dkim-settings-list.page.html',
    styleUrls: [
        './dkim-settings-list.page.scss',
        '../../../../assets/css/scrollbar-form.css',
        '../../../../assets/css/form-toolbar.scss',
    ],
    animations: [contentAnimation],
    styles: [
        scrollbarStyle,
    ],
})

export class DkimSettingsListPage extends DetailPage implements OnInit {
    public subscriptions: Array<SubscriptionLike> = new Array<SubscriptionLike>();
    private dkimSettingsId: string;
    private organisationId: string;
    public loading: boolean;
    public dkimSettings: Array<DkimSettingsResourceModel> = [];
    public actionButtonList: Array<ActionButton>;

    public constructor(
        public routeHelper: RouteHelper,
        public navProxy: NavProxyService,
        eventService: EventService,
        elementRef: ElementRef,
        injector: Injector,
        public layoutManager: LayoutManagerService,
        protected dkimSettingsApiService: DkimSettingsApiService,
        private authenticationService: AuthenticationService,
    ) {
        super(eventService, elementRef, injector);
    }

    public async ngOnInit(): Promise<void> {
        this.isLoading = false;
        this.destroyed = new Subject<void>();
        await this.loadDkimSettings();
        this.initializeActionButtonList();
    }

    private async loadDkimSettings(): Promise<void> {
        this.organisationId = this.routeHelper.getParam('organisationId');
        this.isLoading = true;
        await this.dkimSettingsApiService
            .getDkimSettingsByOrganisation(this.organisationId, this.routeHelper.getContextTenantAlias())
            .pipe(takeUntil(this.destroyed))
            .toPromise()
            .then((dkimSettings: Array<DkimSettingsResourceModel>) => {
                this.dkimSettings = dkimSettings;
                this.isLoading = false;
            });
    }

    public dkimSettingSelected(dkimSetting: DkimSettingsResourceModel): void {
        const pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.pop();
        pathSegments.push(dkimSetting.id);
        this.navProxy.navigateForward(pathSegments, true);
    }

    public userDidTapReturnButton(): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.pop();
        pathSegments.pop();
        this.navProxy.navigateBack(pathSegments, true, { queryParams: { segment: 'Settings' } });
    }

    public returnToPrevious(): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        if (pathSegments[pathSegments.length - 1] == 'edit') {
            pathSegments.pop();
            this.navProxy.navigateForward(pathSegments);
        } else if (pathSegments[pathSegments.length - 1] == 'create') {
            pathSegments.pop();
            if (this.dkimSettingsId) {
                pathSegments.push(this.dkimSettingsId);
                this.navProxy.navigateForward(pathSegments);
            } else {
                pathSegments.pop();
                this.navProxy.navigateBack(pathSegments, true, { queryParams: { segment: 'dkimSettings' } });
            }
        }
    }

    public userDidTapAddButton(): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.pop();
        pathSegments.push("create");
        this.navProxy.navigate(pathSegments);
    }

    private initializeActionButtonList(): void {
        this.actionButtonList = [];

        this.actionButtonList.push(ActionButton.createActionButton(
            "Create",
            "plus",
            IconLibrary.AngularMaterial,
            false,
            "Create DKIM",
            true,
            (): void => {
                return this.userDidTapAddButton();
            },
        ));
    }
}
