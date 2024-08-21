import { Renderer2, ElementRef, Injector, ChangeDetectorRef, Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { DomSanitizer } from '@angular/platform-browser';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { AppConfigService } from '@app/services/app-config.service';
import { QuoteApiService } from '@app/services/api/quote-api.service';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { EventService } from '@app/services/event.service';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { RouteHelper } from '@app/helpers/route.helper';
import { ProductFeatureSettingService } from '@app/services/product-feature-service';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { PermissionService } from '@app/services/permission.service';
import { BrowserDetectionService } from '@app/services/browser-detection.service';
import { ProductApiService } from '@app/services/api/product-api.service';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { EditQuotePage } from "./edit-quote.page";
import { scrollbarStyle } from '@assets/scrollbar';
import { AuthenticationService } from '@app/services/authentication.service';

/**
 * Page for loading the formsapp for endorsing a quote
 */
@Component({
    selector: 'app-endorse-quote',
    templateUrl: '../../forms-app/forms-app.page.html',
    styleUrls: [
        '../../forms-app/forms-app.page.scss',
        '../../../../assets/css/scrollbar-segment.css',
    ],
    styles: [scrollbarStyle],
})
export class EndorseQuotePage extends EditQuotePage {

    public constructor(
        public navProxy: NavProxyService,
        protected route: ActivatedRoute,
        protected render: Renderer2,
        sanitizer: DomSanitizer,
        protected quoteApiService: QuoteApiService,
        appConfigService: AppConfigService,
        errorHandlerService: ErrorHandlerService,
        layoutManager: LayoutManagerService,
        eventService: EventService,
        protected userPath: UserTypePathHelper,
        protected routeHelper: RouteHelper,
        elementRef: ElementRef,
        injector: Injector,
        changeDetectorRef: ChangeDetectorRef,
        productFeatureService: ProductFeatureSettingService,
        sharedAlertService: SharedAlertService,
        permissionService: PermissionService,
        browserDetectionService: BrowserDetectionService,
        productApiService: ProductApiService,
        sharedLoaderService: SharedLoaderService,
        authenticationService: AuthenticationService,
    ) {
        super(
            navProxy,
            route,
            render,
            sanitizer,
            quoteApiService,
            appConfigService,
            errorHandlerService,
            layoutManager,
            eventService,
            userPath,
            routeHelper,
            elementRef,
            injector,
            changeDetectorRef,
            productFeatureService,
            sharedAlertService,
            permissionService,
            browserDetectionService,
            productApiService,
            sharedLoaderService,
            authenticationService);
        this.mode = 'endorse';
    }
}
