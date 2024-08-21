import { LayoutManagerService } from "./layout-manager.service";
import { SharedAlertService } from "./shared-alert.service";
import { LoadingController } from "@ionic/angular";
import { QuoteApiService } from "./api/quote-api.service";
import { Injectable } from "@angular/core";
import { ProductApiService } from "./api/product-api.service";
import { ProductResourceModel } from "@app/resource-models/product.resource-model";
import { NavProxyService } from "./nav-proxy.service";
import { UserTypePathHelper } from "@app/helpers/user-type-path.helper";
import { AppConfigService } from "./app-config.service";
import { ProductFeatureSettingItem } from "@app/models/product-feature-setting-item.enum";
import { ComponentType } from "@app/models/component-type.enum";
import { ProductFeatureSettingService } from "./product-feature-service";
import { PermissionService } from "./permission.service";
import { ProductPortalSettingApiService } from "./api/product-portal-setting.api.service";
import { ProductOrganisationSettingApiService } from "./api/product-organisation-setting.api.service";
import { MatDialog, MatDialogConfig } from "@angular/material/dialog";
import { DialogComponent } from "@app/components/dialog/dialog.component";
import { DialogData, NameValue } from "@app/models/dialog-config";
import { ProductStatus } from "@app/models/product-status.enum";
import { SharedLoaderService } from "./shared-loader.service";
import { finalize } from "rxjs/operators";
import { EventService } from "./event.service";
import { OrganisationModel } from "@app/models/organisation.model";

/**
 * Export quote service class.
 * TODO: Write a better class header: quote services functions.
 */
@Injectable({ providedIn: 'root' })
export class QuoteService {

    public isCreateQuoteDisplayed: boolean = false;
    public performingUserOrganisationId: string;

    public constructor(
        private loadCtrl: LoadingController,
        private layoutManager: LayoutManagerService,
        private sharedLoaderService: SharedLoaderService,
        private sharedAlertService: SharedAlertService,
        private quoteApiService: QuoteApiService,
        private productApiService: ProductApiService,
        private navProxy: NavProxyService,
        private userPath: UserTypePathHelper,
        private appConfigService: AppConfigService,
        private productFeatureService: ProductFeatureSettingService,
        private permissionService: PermissionService,
        private productPortalSettingApiService: ProductPortalSettingApiService,
        private productOrganisationSettingApiService: ProductOrganisationSettingApiService,
        public dialog: MatDialog,
        private eventService: EventService,
    ) {
        this.eventService.performingUserOrganisationSubject$.subscribe((organisation: OrganisationModel) => {
            this.performingUserOrganisationId = organisation.id;
        });
    }

    public async discardQuote(quoteId: string): Promise<void> {
        this.sharedAlertService.closeToast();
        const loader: HTMLIonLoadingElement = await this.loadCtrl.create({
            message: 'Discarding previous quote...',
            cssClass: (this.layoutManager.splitPaneEnabled) ? 'detail-loader' : '',
            showBackdrop: (!this.layoutManager.splitPaneEnabled),
        });
        await loader.present();
        await this.quoteApiService.discardQuote(quoteId).toPromise();
        await loader.dismiss();
    }

    public async createQuoteBySelectingProduct(customerId: string = null): Promise<void> {
        await this.sharedLoaderService.presentWithDelay('Loading products...');
        let products: Array<ProductResourceModel> = await this.retrieveProductsForQuoteCreation();
        let canCreateNewQuoteOrThrow: boolean = products.length >= 1 ? await this.canCreateNewQuoteOrThrow() : false;
        if (products.length == 1 && canCreateNewQuoteOrThrow) {
            this.redirectToCreateQuote(products[0], customerId);
        } else if (products.length > 1 && canCreateNewQuoteOrThrow) {
            this.showCreateQuoteDialog(products, customerId);
        }
        this.sharedLoaderService.dismiss();
    }

    public async retrieveProductsForQuoteCreation(): Promise<Array<ProductResourceModel>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('hasFeatureSettingsEnabled', [ProductFeatureSettingItem.NewBusinessQuotes]);
        params.set('hasComponentTypes', [ComponentType.Quote]);
        params.set('environment', this.appConfigService.getEnvironment());
        params.set('status', ProductStatus.Active);
        const portalAlias: string = this.appConfigService.getCurrentPortalAlias();
        if (portalAlias != null) {
            params.set('portalAlias', portalAlias);
        }

        params.set('organisation', this.performingUserOrganisationId);
        let products: Array<ProductResourceModel> = await this.productApiService.getList(params).toPromise();

        if (products.length === 0) {
            this.sharedAlertService.showWithCustomButton(
                'No products found',
                'You can\'t create a new quote right now. '
                + 'There must be at least one product which has new business transactions enabled '
                + ' and has a quote component deployed to this environmment. '
                + 'If you need assistance, please don\'t hesitate to get in touch with customer support.',
                'Close',
            );
            return [];
        }

        return products;
    }

    public async canCreateNewQuoteOrThrow(): Promise<boolean> {
        try {
            const productFeatureEnabled: boolean =
                await this.productFeatureService.anyProductHasNewBusinessQuoteFeature();

            if (!productFeatureEnabled) {
                throw new Error("You do not have product feature enabled.");
            }

            const hasManageQuotes: boolean = this.permissionService.hasManageQuotePermission();

            if (!hasManageQuotes) {
                throw new Error("You do not have any manage quote permission to create quote.");
            }

            const currentPortal: string = this.appConfigService.getCurrentPortalAlias();
            const productPortalHasNewQuotesAllowed: boolean =
                !currentPortal || await this.productPortalSettingApiService.anyProductHasNewQuotesAllowed();

            if (!productPortalHasNewQuotesAllowed) {
                throw new Error("You do not have any portal permission to create new quote.");
            }

            const productOrganisationHasNewQuotesAllowed: boolean =
                await this.productOrganisationSettingApiService.anyProductHasNewQuotesAllowed();

            if (!productOrganisationHasNewQuotesAllowed) {
                throw new Error("You do not have any organisation permission to create new quote.");
            }

            return productFeatureEnabled &&
                hasManageQuotes &&
                productPortalHasNewQuotesAllowed &&
                productOrganisationHasNewQuotesAllowed;
        } catch (error) {
            this.sharedAlertService.showWithOk(
                'Permission Not Found',
                'You can\'t create a new quote right now. '
                + error.message
                + " Please ask your administrator for access/permission. "
                + ' If you need assistance, please don\'t hesitate to get in touch with customer support.',
                true,
            );

            return false;
        }
    }

    public async canCreateNewQuote(): Promise<boolean> {
        const anyProductHasNewBusinessFeature: boolean
            = await this.productFeatureService.anyProductHasNewBusinessQuoteFeature();
        const hasManageQuotes: boolean = this.permissionService.hasManageQuotePermission();

        const currentPortal: string = this.appConfigService.getCurrentPortalAlias();
        const productPortalHasNewQuotesAllowed: boolean =
            !currentPortal || await this.productPortalSettingApiService.anyProductHasNewQuotesAllowed();

        const productOrganisationHasNewQuotesAllowed: boolean =
            await this.productOrganisationSettingApiService.anyProductHasNewQuotesAllowed();

        return anyProductHasNewBusinessFeature &&
            hasManageQuotes &&
            productPortalHasNewQuotesAllowed &&
            productOrganisationHasNewQuotesAllowed;
    }

    private async displayCreateQuoteDialog(
        customerId: string = null,
        params: Map<string, string | Array<string>>,
    ): Promise<void> {
        await this.sharedLoaderService.presentWithDelay();
        this.productApiService.getList(params)
            .pipe(finalize(() => this.sharedLoaderService.dismiss()))
            .subscribe((products: Array<ProductResourceModel>) => {
                if (products.length === 0) {
                    this.sharedAlertService.showWithCustomButton(
                        'No products found',
                        'You can\'t create a new quote right now. '
                        + 'There must be at least one product which has new business transactions enabled '
                        + ' and has a quote component deployed to this environmment. '
                        + 'If you need assistance, please don\'t hesitate to get in touch with customer support.',
                        'Close',
                    );
                } else {
                    this.showCreateQuoteDialog(products, customerId);
                }
            });
    }

    private async showCreateQuoteDialog(
        products: Array<ProductResourceModel>,
        customerId: string = null,
    ): Promise<void> {
        let productOptions: Array<NameValue<string, string>> = [];
        products.forEach((p: ProductResourceModel) => productOptions.push({ name: p.name, value: p.alias }));

        this.dialog.open(DialogComponent, {
            data: {
                configuration: {
                    header: 'Create quote',
                    subheader: 'Select the product you wish to create a quote for',
                    inputs: [
                        {
                            type: 'radio',
                            key: 'product',
                            options: productOptions,
                        },
                    ],
                    buttons: [
                        {
                            label: 'Cancel',
                        },
                        {
                            label: 'Create',
                            handler: (data: any): void => {
                                if (!data.product) {
                                    return;
                                }
                                let queryParams: any = {
                                    product: data.product,
                                };
                                if (customerId) {
                                    queryParams['customerId'] = customerId;
                                }
                                this.navProxy.navigate([this.userPath.quote, 'create'], { queryParams: queryParams });
                                this.dialog.closeAll();
                            },
                        }],
                },
            },
        } as MatDialogConfig<DialogData>);
    }

    private redirectToCreateQuote(product: ProductResourceModel, customerId: string = null): void {
        let queryParams: any = {
            product: product.alias,
        };
        if (customerId) {
            queryParams['customerId'] = customerId;
        }
        this.navProxy.navigate([this.userPath.quote, 'create'], { queryParams: queryParams });
    }
}
