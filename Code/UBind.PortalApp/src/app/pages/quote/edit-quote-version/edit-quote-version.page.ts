import {
    Component, Renderer2, ElementRef, OnDestroy,
    Injector, ChangeDetectorRef, OnInit, AfterViewInit,
} from '@angular/core';
import { LoadingController, AlertController } from '@ionic/angular';
import { DomSanitizer } from '@angular/platform-browser';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { AppConfigService } from '@app/services/app-config.service';
import { AuthenticationService } from '@app/services/authentication.service';
import { QuoteApiService } from '@app/services/api/quote-api.service';
import { QuoteVersionDetailViewModel } from '@app/viewmodels';
import { CreateQuotePage } from '@pages/quote/create-quote/create-quote.page';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { QuoteVersionApiService } from '@app/services/api/quote-version-api.service';
import { Errors } from '@app/models/errors';
import { EventService } from '@app/services/event.service';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { QuoteVersionDetailResourceModel } from '@app/resource-models/quote-version.resource-model';
import { RouteHelper } from '@app/helpers/route.helper';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { BrowserDetectionService } from '@app/services/browser-detection.service';
import { ProductService } from '@app/services/product.service';
import { ProductApiService } from '@app/services/api/product-api.service';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { ReleaseService } from '@app/services/release.service';

/**
 * Export edit quote version page component class.
 * TODO: Write a better class header: editing of quote version details.
 */
@Component({
    selector: 'app-edit-quote-version',
    templateUrl: '../../forms-app/forms-app.page.html',
    styleUrls: ['../../forms-app/forms-app.page.scss'],
})
export class EditQuoteVersionPage extends CreateQuotePage implements OnInit, AfterViewInit, OnDestroy {

    public quoteVersionId: any;
    public quoteVersion: QuoteVersionDetailViewModel;

    public constructor(
        navProxy: NavProxyService,
        routeHelper: RouteHelper,
        authService: AuthenticationService,
        loadingCtr: LoadingController,
        render: Renderer2,
        sanitizer: DomSanitizer,
        quoteApiService: QuoteApiService,
        public quoteVersionApiService: QuoteVersionApiService,
        alertCtrl: AlertController,
        appConfigService: AppConfigService,
        protected errorHandlerService: ErrorHandlerService,
        layoutManager: LayoutManagerService,
        eventService: EventService,
        protected userPath: UserTypePathHelper,
        protected elementRef: ElementRef,
        public injector: Injector,
        changeDetectorRef: ChangeDetectorRef,
        sharedAlertService: SharedAlertService,
        browserDetectionService: BrowserDetectionService,
        protected productService: ProductService,
        protected productApiService: ProductApiService,
        protected sharedLoaderService: SharedLoaderService,
        protected releaseService: ReleaseService,

    ) {
        super(
            navProxy,
            routeHelper,
            authService,
            loadingCtr,
            render,
            sanitizer,
            quoteApiService,
            alertCtrl,
            appConfigService,
            errorHandlerService,
            layoutManager,
            eventService,
            userPath,
            elementRef,
            injector,
            changeDetectorRef,
            sharedAlertService,
            browserDetectionService,
            productService,
            productApiService,
            sharedLoaderService,
            releaseService,
        );
    }

    public ngOnInit(): void {
        super.ngOnInit();
        this.title = 'Edit Quote';
    }

    public async ngAfterViewInit(): Promise<void> {
        this.quoteVersionId = this.routeHelper.getParam('quoteVersionId');
        if (this.quoteVersionId != null) {
            this.editQuoteVersion();
        } else {
            throw Errors.General.NotFound('route path parameter', 'quoteVersionId');
        }
    }

    public async editQuoteVersion(): Promise<void> {
        const qv: QuoteVersionDetailResourceModel =
                await this.quoteVersionApiService.getQuoteVersionDetail(this.quoteVersionId).toPromise();
        this.quoteVersion = new QuoteVersionDetailViewModel(qv);
        this.productAlias = qv.productId;
        this.quoteId = this.quoteVersion.quoteId;
        this.loadFormsApp(null, null, this.quoteVersion.quoteVersionNumber);
        this.productName = qv.productName;
        this.quoteReference = qv.quoteNumber;
        this.title = `Edit Quote for ${this.productName} ${this.quoteReference ? `(${this.quoteReference})` : ''}`;
    }

    public cancel(): void {
        this.navProxy.navigateBack([this.userPath.quote, this.quoteId]);
    }
}
