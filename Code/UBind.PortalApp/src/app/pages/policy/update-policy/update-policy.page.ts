import {
    Component, Renderer2, ElementRef, OnDestroy,
    Injector, ChangeDetectorRef, AfterViewInit, OnInit,
} from '@angular/core';
import { LoadingController } from '@ionic/angular';
import { DomSanitizer } from '@angular/platform-browser';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { AppConfigService } from '@app/services/app-config.service';
import { scrollbarStyle } from '@assets/scrollbar';
import { QuoteApiService } from '@app/services/api/quote-api.service';
import { QuoteResourceModel } from '@app/resource-models/quote.resource-model';
import { PolicyApiService } from '@app/services/api/policy-api.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { PolicyResourceModel } from '@app/resource-models/policy.resource-model';
import { EventService } from '@app/services/event.service';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { ActivatedRoute } from '@angular/router';
import { SplitLayoutManager } from '@app/models/split-layout-manager';
import { QuoteType } from '@app/models/quote-type.enum';
import { ProductFeatureSettingService } from '@app/services/product-feature-service';
import { FormsAppPage } from '@pages/forms-app/forms-app.page';
import { AuthenticationService } from '@app/services/authentication.service';
import { WebFormHelper } from '@app/helpers/webform.helper';
import { ProductFeatureSetting } from '@app/models/product-feature-setting';
import { ErrorCodeTranslationHelper } from '@app/helpers/error-code-translation.helper';
import { Observable, SubscriptionLike } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { PermissionDataModel, Permission } from '@app/helpers';
import { PermissionService } from '@app/services/permission.service';
import { Errors } from '@app/models/errors';
import { BrowserDetectionService } from '@app/services/browser-detection.service';
import { SharedLoaderService } from '@app/services/shared-loader.service';
/**
 * Export update policy page component class
 * This class is for updating the policies.
 */
@Component({
    selector: 'app-update-policy',
    templateUrl: '../../forms-app/forms-app.page.html',
    styleUrls: [
        '../../forms-app/forms-app.page.scss',
        '../../../../assets/css/scrollbar-segment.css',
    ],
    styles: [scrollbarStyle],
})
export class UpdatePolicyPage extends FormsAppPage
    implements OnInit, AfterViewInit, OnDestroy, SplitLayoutManager {
    protected quoteType: QuoteType;
    private permission: Permission = null;

    public constructor(
        protected navProxy: NavProxyService,
        protected route: ActivatedRoute,
        protected loadingCtr: LoadingController,
        protected render: Renderer2,
        sanitizer: DomSanitizer,
        protected quoteApiService: QuoteApiService,
        appConfigService: AppConfigService,
        errorHandlerService: ErrorHandlerService,
        layoutManager: LayoutManagerService,
        eventService: EventService,
        protected userPath: UserTypePathHelper,
        private authService: AuthenticationService,
        public routeHelper: RouteHelper,
        private policyApiService: PolicyApiService,
        protected elementRef: ElementRef,
        public injector: Injector,
        private productFeatureService: ProductFeatureSettingService,
        changeDetectorRef: ChangeDetectorRef,
        private sharedAlertService: SharedAlertService,
        private permissionService: PermissionService,
        browserDetectionService: BrowserDetectionService,
        private sharedLoaderService: SharedLoaderService,
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
            authService,
        );
    }

    public ngOnDestroy(): void {
        super.ngOnDestroy();
        this.subscriptions.forEach((s: SubscriptionLike) => s.unsubscribe());
    }

    public ngOnInit(): void {
        super.ngOnInit();
    }

    public async ngAfterViewInit(): Promise<void> {
        const pathSegments: Array<string> = this.routeHelper.getPathSegments();
        const actionPathSegment: string = pathSegments[pathSegments.length - 2];
        this.permission = Permission.ManagePolicies;
        try {
            await this.sharedLoaderService.present("Loading quote...");
            switch (actionPathSegment) {
                case 'renew':
                    this.title = 'Renew Policy';
                    this.quoteType = QuoteType.Renewal;
                    break;
                case 'adjust':
                    this.title = 'Adjust Policy';
                    this.quoteType = QuoteType.Adjustment;
                    break;
                case 'cancel':
                    this.title = 'Cancel Policy';
                    this.quoteType = QuoteType.Cancellation;
                    break;
                default:
                    throw new Error(
                        `On update policy page, an unknown path segment '${actionPathSegment}' was received.`);
            }

            this.title = this.authService.isMutualTenant() ? this.title.replace('Policy', 'Protection') : this.title;
            let productRelease: string = this.routeHelper.getQueryParam('productRelease');
            if (productRelease) {
                this.productRelease = productRelease;
            }
            this.policyId = this.routeHelper.getParam('policyId');
            this.quoteId = this.routeHelper.getParam('quoteId');
            const dt: QuoteResourceModel = await this.quoteApiService.getById(this.quoteId).toPromise();
            let hasPermission: boolean = this.checkPermissions(dt);
            this.policyApiService.getById(this.policyId)
                .subscribe((policy: PolicyResourceModel) => {
                    const quoteReference: string = dt.quoteNumber;
                    const productName: string = dt.productName;
                    this.title =
                        `${this.title} ${policy.policyNumber} for ${productName} `
                        + `${quoteReference ? `(${quoteReference})` : ''}`;
                });

            if (!hasPermission) {
                throw Errors.User.AccessDenied("policy");
            }

            this.productAlias = dt.productAlias;
            this.organisationAlias = dt.organisationAlias;
            const productFeature: ProductFeatureSetting
                = await this.productFeatureService.getProductFeatureSetting(dt.productId);
            const hasProductFeature: boolean =
                (this.quoteType === QuoteType.Renewal && productFeature.areRenewalQuotesEnabled) ||
                (this.quoteType === QuoteType.Adjustment && productFeature.areAdjustmentQuotesEnabled) ||
                (this.quoteType === QuoteType.Cancellation && productFeature.areCancellationQuotesEnabled);
            if (hasProductFeature) {
                this.sharedLoaderService.dismiss();
                this.loadFormsApp(WebFormHelper.modes.Edit, this.quoteType);
            } else {
                const transactionType: string = this.quoteType === QuoteType.Renewal ? 'Renewal' :
                    this.quoteType === QuoteType.Adjustment ? 'Adjustment' : 'Cancellation';
                const title: string = `${transactionType} transaction disabled`;
                const message: string = `The ${transactionType.toLowerCase()} policy transaction type `
                    + `has been disabled for the ${dt.productName} product.`;
                console.log(message);
                this.showModalMessage(title, message);
            }
        } catch (err) {
            // navigate back so we're not left on a blank page.
            const localError: any = err;
            if (localError.error && localError.error.code) {
                const errorCode: any = err.error.code;
                const isAdjustmentDisabled: boolean =
                    ErrorCodeTranslationHelper.isAdjustmentDisabled(errorCode);
                const isRenewalDisabled: boolean =
                    ErrorCodeTranslationHelper.isRenewalDisabled(errorCode);
                const isCancellationDisabled: boolean =
                    ErrorCodeTranslationHelper.isCancellationDisabled(errorCode);
                const transactionType: string = isAdjustmentDisabled ? 'Adjustment' : isRenewalDisabled ?
                    'Renewal' : isCancellationDisabled ? 'Cancellation' : '';
                if (isAdjustmentDisabled || isRenewalDisabled || isCancellationDisabled) {
                    const title: string = `${transactionType} transaction disabled`;
                    const message: string = `The ${transactionType.toLowerCase()} policy transaction type `
                        + `has been disabled for the ${localError.error.data.productName} product.`;
                    this.showModalMessage(title, message);
                    console.log(message);
                    return;
                }
            }
            this.closeButtonClicked();
            throw err;
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
        );
    }

    private showModalMessage(title: string, message: string): void {
        this.sharedAlertService.showWithActionHandler({
            header: title,
            subHeader: message,
            buttons: [
                {
                    text: 'OK',
                    handler: (): void => {
                        this.navProxy.navigateBack([this.userPath.policy, this.policyId]);
                    },
                },
            ],
        });
    }

    public receiveMessage(event: any): void {
        if (event.data.messageType == "closeApp") {
            let policyObservable: Observable<PolicyResourceModel>;
            policyObservable = this.policyApiService.getById(this.policyId);
            this.subscriptions.push(policyObservable
                .pipe(takeUntil(this.destroyed))
                .subscribe((policy: PolicyResourceModel) => {
                    this.eventService.getEntityUpdatedSubject('Policy').next(policy);
                    this.closeButtonClicked();
                }));
        } else {
            super.receiveMessage(event);
        }
    }

    public closeButtonClicked(): void {
        this.navProxy.navigateBack([this.userPath.policy, this.policyId]);
    }

    public goBackButtonClicked(): void {
        this.navProxy.navigateBack([this.userPath.policy, this.policyId]);
    }
}
