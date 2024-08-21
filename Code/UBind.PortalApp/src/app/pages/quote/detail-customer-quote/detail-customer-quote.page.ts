import { Component, Injector, ElementRef } from '@angular/core';
import { AlertController, PopoverController } from '@ionic/angular';
import { QuoteApiService } from '@app/services/api/quote-api.service';
import { QuoteVersionApiService } from '@app/services/api/quote-version-api.service';
import { DomSanitizer } from '@angular/platform-browser';
import { DocumentApiService } from '@app/services/api/document-api.service';
import { PermissionService } from '@app/services/permission.service';
import { FeatureSettingService } from '@app/services/feature-setting.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { scrollbarStyle } from '@assets/scrollbar';
import { contentAnimation } from '@assets/animations';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { DetailQuotePage } from '../detail-quote/detail-quote.page';
import { AuthenticationService } from '@app/services/authentication.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { EventService } from '@app/services/event.service';
import { UserService } from '@app/services/user.service';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { SharedPopoverService } from '@app/services/shared-popover.service';
import { AppConfigService } from '@app/services/app-config.service';
import { ProductApiService } from '@app/services/api/product-api.service';
import { PortalExtensionsService } from '@app/services/portal-extensions.service';
import { ProductFeatureSettingService } from '@app/services/product-feature-service';
import { ToastController } from '@ionic/angular';
import { SharedLoaderService } from '@app/services/shared-loader.service';

/**
 * Export detail customer quote page component class
 * TODO: Write a better class header: displaying of customer quote details.
 */
@Component({
    selector: 'app-detail-customer-quote',
    templateUrl: '../detail-quote/detail-quote.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-detail.css',
    ],
    styles: [scrollbarStyle],
})
export class DetailCustomerQuotePage extends DetailQuotePage {

    public constructor(
        sanitizer: DomSanitizer,
        documentApiService: DocumentApiService,
        quoteApiService: QuoteApiService,
        quoteVersionApiService: QuoteVersionApiService,
        navProxy: NavProxyService,
        permissionService: PermissionService,
        featureSettingService: FeatureSettingService,
        userService: UserService,
        alertCtrl: AlertController,
        layoutManager: LayoutManagerService,
        routeHelper: RouteHelper,
        authService: AuthenticationService,
        popOverCtrl: PopoverController,
        userPath: UserTypePathHelper,
        eventService: EventService,
        elementRef: ElementRef,
        injector: Injector,
        sharedAlertService: SharedAlertService,
        sharedPopoverService: SharedPopoverService,
        appConfigService: AppConfigService,
        productApiService: ProductApiService,
        portalExtensionService: PortalExtensionsService,
        productFeatureService: ProductFeatureSettingService,
        toastCtrl: ToastController,
        sharedLoaderService: SharedLoaderService,
    ) {
        super(
            sanitizer,
            routeHelper,
            quoteApiService,
            quoteVersionApiService,
            documentApiService,
            authService,
            navProxy,
            permissionService,
            popOverCtrl,
            featureSettingService,
            userService,
            alertCtrl,
            sharedAlertService,
            layoutManager,
            userPath,
            eventService,
            elementRef,
            injector,
            sharedPopoverService,
            appConfigService,
            productApiService,
            portalExtensionService,
            productFeatureService,
            toastCtrl,
            sharedLoaderService,
        );
    }

    public goToPolicy(): void {
        if (this.featureSettingService.hasPolicyFeature() && this.detail.policyNumber) {
            this.navProxy.navigate([this.userPath.policy, this.detail.policyId]);
        }
    }

    public gotoVersionDetail(quoteVersion: any): void {
        this.navProxy.navigateForward([this.userPath.quote, this.detail.id, 'version', quoteVersion.quoteVersionId]);
    }
}
