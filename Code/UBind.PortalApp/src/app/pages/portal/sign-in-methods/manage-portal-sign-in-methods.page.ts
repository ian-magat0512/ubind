import { Component, ElementRef, Injector, OnDestroy, OnInit } from "@angular/core";
import { RouteHelper } from "@app/helpers/route.helper";
import { ActionButton } from "@app/models/action-button";
import { AuthenticationMethodType } from "@app/models/authentication-method-type.enum";
import { DetailPage } from "@app/pages/master-detail/detail.page";
import { PortalSignInMethodResourceModel } from "@app/resource-models/portal/portal-sign-in-method.resource-model";
import { PortalApiService } from "@app/services/api/portal-api.service";
import { EventService } from "@app/services/event.service";
import { LayoutManagerService } from "@app/services/layout-manager.service";
import { NavProxyService } from "@app/services/nav-proxy.service";
import { SharedAlertService } from "@app/services/shared-alert.service";
import { Subject } from "rxjs";
import { finalize, takeUntil } from "rxjs/operators";

/**
 * Allows a portal to turn on or off the authentication methods defined against it's organisation
 * (or a managing organisation).
 */
@Component({
    selector: 'app-manage-portal-sign-in-methods-page',
    templateUrl: './manage-portal-sign-in-methods.page.html',
    styleUrls: [
        './manage-portal-sign-in-methods.page.scss',
        '../../../../assets/css/scrollbar-form.css',
        '../../../../assets/css/form-toolbar.scss',
    ],
})
export class ManagePortalSignInMethodsPage extends DetailPage implements OnInit, OnDestroy {

    private portalId: string;
    public signInMethods: Array<PortalSignInMethodResourceModel>;
    public actionButtonList: Array<ActionButton>;
    // eslint-disable-next-line @typescript-eslint/naming-convention
    public AuthenticationMethodType: typeof AuthenticationMethodType = AuthenticationMethodType;

    public constructor(
        private routeHelper: RouteHelper,
        eventService: EventService,
        elementRef: ElementRef,
        injector: Injector,
        private portalApiService: PortalApiService,
        private navProxy: NavProxyService,
        public layoutManager: LayoutManagerService,
        private sharedAlertService: SharedAlertService,
    ) {
        super(eventService, elementRef, injector);
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.portalId = this.routeHelper.getParam('portalId');
        this.loadPortalSignInMethods();
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
        this.navProxy.navigateBack(pathSegments, true, { queryParams: { segment: 'Settings' } });
    }

    private async loadPortalSignInMethods(): Promise<void> {
        this.isLoading = true;
        await this.portalApiService
            .getSignInMethods(this.portalId, this.routeHelper.getContextTenantAlias())
            .pipe(
                takeUntil(this.destroyed),
                finalize(() => this.isLoading = false))
            .toPromise()
            .then((resourceModels: Array<PortalSignInMethodResourceModel>) => {
                this.signInMethods = resourceModels;
            });
    }

    public toggle($event: CustomEvent, signInMethod: PortalSignInMethodResourceModel): void {
        signInMethod.isEnabled = !signInMethod.isEnabled;
        if (signInMethod.isEnabled) {
            this.portalApiService
                .enableSignInMethod(
                    this.portalId,
                    signInMethod.authenticationMethodId,
                    this.routeHelper.getContextTenantAlias(),
                ).pipe(takeUntil(this.destroyed))
                .subscribe((model: PortalSignInMethodResourceModel) => {
                    if (model) { // will be null if we navigate away from the page whilst loading
                        this.eventService.getEntityUpdatedSubject('PortalSignInMethod').next(model);
                        this.sharedAlertService.showToast(`The sign-in method ${model.name} has been enabled`);
                    }
                });
        } else {
            this.portalApiService
                .disableSignInMethod(
                    this.portalId,
                    signInMethod.authenticationMethodId,
                    this.routeHelper.getContextTenantAlias(),
                ).pipe(takeUntil(this.destroyed))
                .subscribe((model: PortalSignInMethodResourceModel) => {
                    if (model) { // will be null if we navigate away from the page whilst loading
                        this.eventService.getEntityUpdatedSubject('PortalSignInMethod').next(model);
                        this.sharedAlertService.showToast(`The sign-in method ${model.name} has been disabled`);
                    }
                });
        }
    }

    private initializeActionButtonList(): void {
        this.actionButtonList = [];

        // for now there are no actions for this page.
    }
}
