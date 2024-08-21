import { Component, OnDestroy, ElementRef, Injector, Renderer2, ChangeDetectorRef, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { SplitLayoutManager } from '@app/models/split-layout-manager';
import { AppConfigService } from '@app/services/app-config.service';
import { ClaimApiService } from '@app/services/api/claim-api.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { LoadingController, AlertController } from '@ionic/angular';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { EventService } from '@app/services/event.service';
import { DomSanitizer } from '@angular/platform-browser';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { scrollbarStyle } from '@assets/scrollbar';
import { FormsAppClaimPage } from '@pages/forms-app/forms-app-claim.page';
import { WebFormHelper } from '@app/helpers/webform.helper';
import { RouteHelper } from '@app/helpers/route.helper';
import { PermissionService } from '@app/services/permission.service';
import { BrowserDetectionService } from '@app/services/browser-detection.service';
import { PolicyApiService } from '@app/services/api/policy-api.service';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { AuthenticationService } from '@app/services/authentication.service';

/**
 * Export notification acknowledge claim page component class
 * This class manage notification for the acknowledgement of claims.
 */
@Component({
    selector: 'app-notification-acknowledge-claim',
    templateUrl: '../../forms-app/forms-app.page.html',
    styleUrls: [
        '../../forms-app/forms-app.page.scss',
        '../../../../assets/css/scrollbar-segment.css',
    ],
    styles: [
        scrollbarStyle,
    ],
})
export class NotificationAcknowledgeClaimPage
    extends FormsAppClaimPage implements OnInit, OnDestroy, SplitLayoutManager {

    public constructor(
        public appConfigService: AppConfigService,
        public claimApiService: ClaimApiService,
        public navProxy: NavProxyService,
        public loadingCtrl: LoadingController,
        public alertCltr: AlertController,
        public route: ActivatedRoute,
        public layoutManager: LayoutManagerService,
        protected userPath: UserTypePathHelper,
        eventService: EventService,
        elementRef: ElementRef,
        injector: Injector,
        sanitizer: DomSanitizer,
        protected render: Renderer2,
        errorHandlerService: ErrorHandlerService,
        changeDetectorRef: ChangeDetectorRef,
        routeHelper: RouteHelper,
        public permissionService: PermissionService,
        browserDetectionService: BrowserDetectionService,
        protected policyApiService: PolicyApiService,
        protected sharedLoaderService: SharedLoaderService,
        authenticationService: AuthenticationService,
    ) {
        super(
            appConfigService,
            navProxy,
            layoutManager,
            userPath,
            eventService,
            elementRef,
            injector,
            sanitizer,
            render,
            errorHandlerService,
            changeDetectorRef,
            claimApiService,
            route,
            routeHelper,
            permissionService,
            browserDetectionService,
            policyApiService,
            sharedLoaderService,
            authenticationService,
        );
    }

    public ngOnInit(): void {
        super.ngOnInit();

        this.title = this.action = 'Acknowledge Notification';
        this.loadClaimFormsApp(WebFormHelper.modes.Acknowledge);
    }
}
