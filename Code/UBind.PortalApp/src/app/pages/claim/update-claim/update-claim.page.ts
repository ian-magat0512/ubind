import { Component, ElementRef, Injector, Renderer2, ChangeDetectorRef, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AlertController, LoadingController } from '@ionic/angular';
import { scrollbarStyle } from '@assets/scrollbar';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { ClaimApiService } from '@app/services/api/claim-api.service';
import { EventService } from '@app/services/event.service';
import { DomSanitizer } from '@angular/platform-browser';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { SplitLayoutManager } from '@app/models/split-layout-manager';
import { AppConfigService } from '@app/services/app-config.service';
import { FormsAppClaimPage } from '@pages/forms-app/forms-app-claim.page';
import { WebFormHelper } from '@app/helpers/webform.helper';
import { RouteHelper } from '@app/helpers/route.helper';
import { PermissionService } from '@app/services/permission.service';
import { BrowserDetectionService } from '@app/services/browser-detection.service';
import { PolicyApiService } from '@app/services/api/policy-api.service';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { AuthenticationService } from '@app/services/authentication.service';

/**
 * Export update claim page component class
 * This class manage updating of claims.
 */
@Component({
    selector: 'app-update-claim',
    templateUrl: '../../forms-app/forms-app.page.html',
    styleUrls: [
        '../../forms-app/forms-app.page.scss',
        '../../../../assets/css/scrollbar-segment.css',
    ],
    styles: [
        scrollbarStyle,
    ],
})
export class UpdateClaimPage extends FormsAppClaimPage implements OnInit, SplitLayoutManager {

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

        this.title = this.action = 'Update Claim';
        this.loadClaimFormsApp(WebFormHelper.modes.Edit);
    }
}
