import { Component, Renderer2, ElementRef, Injector, ChangeDetectorRef, OnInit, AfterViewInit } from '@angular/core';
import { LoadingController, AlertController } from '@ionic/angular';
import { DomSanitizer } from '@angular/platform-browser';
import { finalize, takeUntil } from 'rxjs/operators';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { AppConfigService } from '@app/services/app-config.service';
import { AuthenticationService } from '@app/services/authentication.service';
import { QuoteApiService } from '@app/services/api/quote-api.service';
import { scrollbarStyle } from '@assets/scrollbar';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { EventService } from '@app/services/event.service';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { RouteHelper } from '@app/helpers/route.helper';
import { SplitLayoutManager } from '@app/models/split-layout-manager';
import { FormsAppQuotePage } from '@app/pages/forms-app/forms-app-quote.page';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { StringHelper, WebFormHelper } from '@app/helpers';
import { BrowserDetectionService } from '@app/services/browser-detection.service';
import { ProductService } from '@app/services/product.service';
import { fromEvent } from 'rxjs';
import { ProductApiService } from '@app/services/api/product-api.service';
import { SharedLoaderService } from "@app/services/shared-loader.service";
import { ErrorCodeTranslationHelper } from '@app/helpers/error-code-translation.helper';
import { ReleaseService } from '@app/services/release.service';
import { ProblemDetails } from '@app/models/problem-details';

/**
 * Export create quote page component class
 * This class is to create a new quote in the portal.
 */
@Component({
    selector: 'app-create-quote',
    templateUrl: '../../forms-app/forms-app.page.html',
    styleUrls: [
        '../../forms-app/forms-app.page.scss',
        '../../../../assets/css/scrollbar-segment.css',
    ],
    styles: [scrollbarStyle],
})
export class CreateQuotePage extends FormsAppQuotePage implements OnInit, AfterViewInit, SplitLayoutManager {
    public closeButtonLabel: string = this.defaultCancelButtonLabel;
    public formAppUrl: any = '';
    public timeout: any;
    public productName: string;
    public quoteReference: string;

    public constructor(
        public navProxy: NavProxyService,
        protected routeHelper: RouteHelper,
        protected authService: AuthenticationService,
        protected loadingCtr: LoadingController,
        protected render: Renderer2,
        sanitizer: DomSanitizer,
        public quoteApiService: QuoteApiService,
        public alertCtrl: AlertController,
        appConfigService: AppConfigService,
        errorHandlerService: ErrorHandlerService,
        public layoutManager: LayoutManagerService,
        eventService: EventService,
        protected userPath: UserTypePathHelper,
        protected elementRef: ElementRef,
        public injector: Injector,
        changeDetectorRef: ChangeDetectorRef,
        private sharedAlertService: SharedAlertService,
        browserDetectionService: BrowserDetectionService,
        protected productService: ProductService,
        protected productApiService: ProductApiService,
        protected sharedLoaderService: SharedLoaderService,
        protected releaseService: ReleaseService,
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
            authService,
        );
    }

    public ngOnInit(): void {
        super.ngOnInit();

        fromEvent(window, "message")
            .pipe(takeUntil(this.destroyed))
            .subscribe((e: MessageEvent) => {
                if (e.data.messageType === 'quoteReferenceAllocated') {
                    this.quoteReference = e.data.payload;
                    this.title =
                        `Create Quote for ${this.productName} ${this.quoteReference ? `(${this.quoteReference})` : ''}`;
                }
            });
    }

    public async ngAfterViewInit(): Promise<void> {
        this.productAlias = this.routeHelper.getParam('product');

        this.productApiService.getNameByAlias(this.productAlias, this.tenantAlias)
            .subscribe((productName: string) => {
                this.productName = productName;
                this.title = `Create Quote for ${this.productName} `
                    + `${this.quoteReference ? `(${this.quoteReference})` : ''}`;
            });
        await this.createQuote();
    }

    private async createQuote(): Promise<void> {
        let isTestData: boolean = this.routeHelper.getParam('testMode') == 'true';
        let productRelease: string = this.routeHelper.getParam('productRelease');
        if (productRelease) {
            this.productRelease = productRelease;
        }
        let customerId: string = this.routeHelper.getParam('customerId');
        if (!customerId && this.authService.isCustomer()) {
            customerId = this.authService.customerId;
        }

        await this.sharedLoaderService.presentWithDelay('Please wait...');

        this.quoteApiService.createNewBusinessQuote(
            this.organisationId,
            this.productAlias,
            customerId,
            isTestData,
            productRelease,
        ).pipe(
            finalize(() => this.sharedLoaderService.dismiss()),
            takeUntil(this.destroyed),
        ).subscribe(
            (quote: any) => {
                this.quoteId = quote.quoteId;
                this.organisationAlias = quote.organisationAlias;
                this.isTestData = quote.isTestData;
                this.quoteReference = quote.quoteNumber;
                let quoteType: any = StringHelper.camelCase(quote.quoteType);
                this.loadFormsApp(WebFormHelper.modes.Create, quoteType);
            },
            (err: any) => {
                // navigate back so we're not left on a blank page.
                this.closeButtonClicked();
                const localError: any = err;
                if (localError.error && localError.error.code) {
                    let appError: ProblemDetails = ProblemDetails.fromJson(localError.error);
                    const isNewBusinessDisabled: boolean =
                        localError.error.code === 'quote.creation.new.business.quote.type.disabled';
                    if (isNewBusinessDisabled) {
                        let productName: string = this.productName ? this.productName : '';
                        const title: string = `New business quotes are disabled for this product`;
                        const message: string = `When trying to create a new business quote for the ${productName} `
                            + `product, the attempt failed because the product settings for ${productName} `
                            + `prevent the creation of new business quotes. To resolve this issue please enable `
                            + `new business quotes in the product settings for the ${productName} product. `
                            + `If you need further assistance please contact technical support.`;
                        this.sharedAlertService.showWithOk(title, message);
                        return;
                    } else if (ErrorCodeTranslationHelper.isProductReleaseNotFound(appError.Code)) {
                        this.releaseService.handleProductReleaseWasNotSet('quote', 'created', appError);
                        return;
                    }
                }
                throw err;
            },
        );
    }
}
