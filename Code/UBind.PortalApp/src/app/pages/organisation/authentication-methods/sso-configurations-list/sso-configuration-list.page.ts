import { Component, ElementRef, Injector, OnDestroy, OnInit } from "@angular/core";
import { RouteHelper } from "@app/helpers/route.helper";
import { ActionButton } from "@app/models/action-button";
import { AuthenticationMethodType } from "@app/models/authentication-method-type.enum";
import { IconLibrary } from "@app/models/icon-library.enum";
import { DetailPage } from "@app/pages/master-detail/detail.page";
import { AuthenticationMethodResourceModel } from "@app/resource-models/authentication-method.resource-model";
import { AuthenticationMethodApiService } from "@app/services/api/authentication-method-api.service";
import { EventService } from "@app/services/event.service";
import { LayoutManagerService } from "@app/services/layout-manager.service";
import { NavProxyService } from "@app/services/nav-proxy.service";
import { SharedAlertService } from "@app/services/shared-alert.service";
import { Subject } from "rxjs";
import { finalize, takeUntil } from "rxjs/operators";

/**
 * Page for listing/managing SSO configurations.
 */
@Component({
    selector: 'app-sso-configurations-list-page',
    templateUrl: './sso-configurations-list.page.html',
    styleUrls: [
        './sso-configurations-list.page.scss',
        '../../../../../assets/css/scrollbar-form.css',
        '../../../../../assets/css/form-toolbar.scss',
    ],
})
export class SsoConfigurationsListPage extends DetailPage implements OnInit, OnDestroy {

    private organisationId: string;
    public ssoConfigurations: Array<AuthenticationMethodResourceModel>;
    public actionButtonList: Array<ActionButton>;

    public constructor(
        private routeHelper: RouteHelper,
        eventService: EventService,
        elementRef: ElementRef,
        injector: Injector,
        private authenticationMethodApiService: AuthenticationMethodApiService,
        private navProxy: NavProxyService,
        public layoutManager: LayoutManagerService,
        private sharedAlertService: SharedAlertService,
    ) {
        super(eventService, elementRef, injector);
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.organisationId = this.routeHelper.getParam('organisationId');
        this.loadSsoConfigurations();
        this.initializeActionButtonList();
    }

    public ngOnDestroy(): void {
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    public goBackOrClose(): void {
        const pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.pop();
        pathSegments.pop();
        pathSegments.pop();
        this.navProxy.navigateBack(pathSegments, true, { queryParams: { segment: 'Settings' } });
    }

    private async loadSsoConfigurations(): Promise<void> {
        this.isLoading = true;
        await this.authenticationMethodApiService
            .getAuthenticationMethods(this.organisationId, this.routeHelper.getContextTenantAlias())
            .pipe(
                takeUntil(this.destroyed),
                finalize(() => this.isLoading = false))
            .toPromise()
            .then((ssoConfigurations: Array<AuthenticationMethodResourceModel>) => {
                this.ssoConfigurations = ssoConfigurations
                    .filter((ssoConfiguration: AuthenticationMethodResourceModel) => {
                        return ssoConfiguration.typeName != AuthenticationMethodType.LocalAccount;
                    });
            });
    }

    public toggle($event: CustomEvent, ssoConfiguration: AuthenticationMethodResourceModel): void {
        $event.stopPropagation();
        ssoConfiguration.disabled = !ssoConfiguration.disabled;
        if (ssoConfiguration.disabled) {
            this.authenticationMethodApiService
                .enableAuthenticationMethod(ssoConfiguration.id, this.routeHelper.getContextTenantAlias())
                .pipe(takeUntil(this.destroyed))
                .subscribe((model: AuthenticationMethodResourceModel) => {
                    if (model) { // will be null if we navigate away from the page whilst loading
                        this.eventService.getEntityUpdatedSubject('AuthenticationMethod').next(model);
                        this.sharedAlertService.showToast(`The authentication method ${model.name} has been enabled`);
                    }
                });
        } else {
            this.authenticationMethodApiService
                .disableAuthenticationMethod(ssoConfiguration.id, this.routeHelper.getContextTenantAlias())
                .pipe(takeUntil(this.destroyed))
                .subscribe((model: AuthenticationMethodResourceModel) => {
                    if (model) { // will be null if we navigate away from the page whilst loading
                        this.eventService.getEntityUpdatedSubject('AuthenticationMethod').next(model);
                        this.sharedAlertService.showToast(`The authentication method ${model.name} has been disabled`);
                    }
                });
        }
    }

    public userDidTapAddButton(): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.pop();
        pathSegments.push("create");
        this.navProxy.navigate(pathSegments);
    }

    public ssoConfigurationSelected(ssoConfiguration: AuthenticationMethodResourceModel): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.pop();
        pathSegments.push(ssoConfiguration.id);
        this.navProxy.navigateForward(pathSegments);
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
