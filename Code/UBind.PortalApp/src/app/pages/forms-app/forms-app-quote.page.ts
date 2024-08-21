import { FormsAppPage } from "./forms-app.page";
import { NavProxyService } from "@app/services/nav-proxy.service";
import { AppConfigService } from "@app/services/app-config.service";
import { LayoutManagerService } from "@app/services/layout-manager.service";
import { UserTypePathHelper } from "@app/helpers/user-type-path.helper";
import { EventService } from "@app/services/event.service";
import { ElementRef, Injector, Renderer2, ChangeDetectorRef, OnDestroy, Directive } from "@angular/core";
import { DomSanitizer } from "@angular/platform-browser";
import { ErrorHandlerService } from "@app/services/error-handler.service";
import { RouteHelper } from "@app/helpers/route.helper";
import { BrowserDetectionService } from "@app/services/browser-detection.service";
import { AuthenticationService } from "@app/services/authentication.service";

/**
 * Export forms app quote page abstract class that extends the formsApp page.
 * TODO: Write a better class header: Form App quote button functions.
 */
@Directive({
    selector: '[formsAppQuotePage]',
})
export class FormsAppQuotePage extends FormsAppPage implements OnDestroy {

    public constructor(
        public appConfigService: AppConfigService,
        public navProxy: NavProxyService,
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
        browserDetectionService: BrowserDetectionService,
        authenticationService: AuthenticationService,
    ) {
        super(
            appConfigService,
            layoutManager,
            sanitizer,
            errorHandlerService,
            eventService,
            elementRef,
            render,
            injector,
            changeDetectorRef,
            routeHelper,
            browserDetectionService,
            authenticationService,
        );

        this.formType = 'quote';
    }

    public goBackButtonClicked(): void {
        if (this.quoteId) {
            this.navProxy.navigateBack([this.userPath.quote, this.quoteId]);
        } else {
            this.navProxy.navigateBack([this.userPath.quote, 'list']);
        }
    }

    public closeButtonClicked(): void {
        if (this.complete) {
            this.navProxy.navigateBack([this.userPath.quote, this.quoteId]);
        } else {
            return this.goBackButtonClicked();
        }
    }
}
