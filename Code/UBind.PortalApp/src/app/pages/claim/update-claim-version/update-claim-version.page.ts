import { Component, ElementRef, Injector, Renderer2, ChangeDetectorRef, OnInit } from '@angular/core';
import { AlertController, LoadingController } from '@ionic/angular';
import { ActivatedRoute } from '@angular/router';
import { EventService } from '@app/services/event.service';
import { ClaimApiService } from '@app/services/api/claim-api.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { AppConfigService } from '@app/services/app-config.service';
import { SplitLayoutManager } from '@app/models/split-layout-manager';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
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
 * Export update claim version page component class
 * This class manage updating the version of the claims.
 */
@Component({
    selector: 'app-update-claim-version',
    templateUrl: '../../forms-app/forms-app.page.html',
    styleUrls: [
        '../../forms-app/forms-app.page.scss',
        '../../../../assets/css/scrollbar-segment.css',
    ],
    styles: [
        scrollbarStyle,
    ],
})
export class UpdateClaimVersionPage extends FormsAppClaimPage implements OnInit, SplitLayoutManager {

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

        this.version = this.route.snapshot.paramMap.get('versionnumber') || null;
        this.title = this.action = 'Update Claim';

        this.loadClaimFormsApp(WebFormHelper.modes.Edit, this.version);
    }
}
