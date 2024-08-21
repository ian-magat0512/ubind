import { Component, Renderer2, ElementRef, Injector, ChangeDetectorRef, AfterViewInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { DomSanitizer } from '@angular/platform-browser';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { AppConfigService } from '@app/services/app-config.service';
import { QuoteApiService } from '@app/services/api/quote-api.service';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { EventService } from '@app/services/event.service';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { QuoteResourceModel } from '@app/resource-models/quote.resource-model';
import { SplitLayoutManager } from '@app/models/split-layout-manager';
import { RouteHelper } from '@app/helpers/route.helper';
import { scrollbarStyle } from '@assets/scrollbar';
import { FormsAppQuotePage } from '@app/pages/forms-app/forms-app-quote.page';
import { ProductFeatureSettingService } from '@app/services/product-feature-service';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { Permission, PermissionDataModel } from '@app/helpers';
import { PermissionService } from '@app/services/permission.service';
import { Errors } from '@app/models/errors';
import { BrowserDetectionService } from '@app/services/browser-detection.service';
import { ProductApiService } from '@app/services/api/product-api.service';
import { ProductResourceModel } from '@app/resource-models/product.resource-model';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { AuthenticationService } from '@app/services/authentication.service';

/**
 * Loads a quote edit form using the formsApp
 */
@Component({
    selector: 'app-edit-quote',
    templateUrl: '../../forms-app/forms-app.page.html',
    styleUrls: [
        '../../forms-app/forms-app.page.scss',
        '../../../../assets/css/scrollbar-segment.css',
    ],
    styles: [scrollbarStyle],
})
export class EditQuotePage extends FormsAppQuotePage implements AfterViewInit, SplitLayoutManager {

    private permission: Permission = null;
    public mode: string = 'edit';

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
        private productFeatureService: ProductFeatureSettingService,
        private sharedAlertService: SharedAlertService,
        private permissionService: PermissionService,
        browserDetectionService: BrowserDetectionService,
        private productApiService: ProductApiService,
        private sharedLoaderService: SharedLoaderService,
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
            routeHelper,
            browserDetectionService,
            authenticationService,
        );
    }

    public ngAfterViewInit(): void {
        const pathSegments: Array<string> = this.routeHelper.getPathSegments();
        const actionPathSegment: string = pathSegments[pathSegments.length - 1];
        switch (actionPathSegment) {
            case 'edit':
                this.title = 'Edit Quote';
                this.permission = Permission.ManageQuotes;
                break;
            case 'review':
                this.title = 'Review Quote';
                this.permission = Permission.ReviewQuotes;
                break;
            case 'endorse':
                this.title = 'Endorse Quote';
                this.permission = Permission.EndorseQuotes;
                break;
            default:
                throw new Error("On edit quote page, an unknown path segment '" +
                    actionPathSegment + "' was received.");
        }
        this.editQuote();
    }

    private async editQuote(): Promise<void> {
        this.quoteId = this.route.snapshot.paramMap.get('quoteId');

        await this.sharedLoaderService.present('Loading quote...');
        try {
            const quote: QuoteResourceModel = await this.quoteApiService.getById(this.quoteId).toPromise();
            const quoteReference: string = quote.quoteNumber;

            this.productApiService.getNameByAlias(quote.productAlias, this.tenantAlias)
                .subscribe((productName: string) => {
                    this.title = `${this.title} for ${productName} ${quoteReference ? `(${quoteReference})` : ''}`;
                });

            const isProductDisabled: boolean = await this.isProductDisabled(quote.productId);
            if (isProductDisabled) {
                this.closeButtonClicked();
                this.sharedAlertService.showWithOk(
                    "You cannot edit this quote",
                    `The product "${quote.productName}" was disabled.`,
                );
                return;
            }

            let hasPermission: boolean = this.checkPermissions(quote);

            if (!hasPermission) {
                throw Errors.User.AccessDenied("quote");
            }

            this.organisationAlias = quote.organisationAlias;
            this.productAlias = quote.productAlias;
            this.sharedLoaderService.dismiss();
            this.loadFormsApp(this.mode, quote.quoteType);
        } catch (error) {
            // navigate back so we're not left on a blank page.
            this.closeButtonClicked();
            throw error;
        } finally {
            this.sharedLoaderService.dismiss();
        }
    }

    private checkPermissions(quote: QuoteResourceModel): boolean {
        let permissionModel: PermissionDataModel = {
            organisationId: quote.organisationId,
            ownerUserId: quote.ownerUserId,
            customerId: quote.customerDetails ? quote.customerDetails.id : null,
        };

        return this.permissionService.hasElevatedPermissionsViaModel(
            this.permission,
            permissionModel,
        )
            && this.permissionService.hasElevatedPermissionsViaModel(
                Permission.ManageQuotes,
                permissionModel,
            );
    }

    private async isProductDisabled(productId: string): Promise<boolean> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set("tenant", this.tenantAlias);
        const product: ProductResourceModel = await this.productApiService.getById(productId, params).toPromise();
        return product.disabled;
    }

    private showModalMessage(title: string, message: string): void {
        this.sharedAlertService.showWithActionHandler({
            header: title,
            subHeader: message,
            buttons: [
                {
                    text: 'OK',
                    handler: (): void => {
                        // navigate back so we're not left on a blank page.
                        this.closeButtonClicked();
                    },
                },
            ],
        });
    }
}
