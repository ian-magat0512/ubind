import { PolicyViewModel } from "@app/viewmodels/policy.viewmodel";
import { NavProxyService } from "./nav-proxy.service";
import { AlertOptions } from "@ionic/core";
import { Injectable } from "@angular/core";
import { UserTypePathHelper } from "@app/helpers/user-type-path.helper";
import { SharedAlertService } from "@app/services/shared-alert.service";
import { AuthenticationService } from "@app/services/authentication.service";
import { ProductFeatureSettingItem } from "@app/models/product-feature-setting-item.enum";
import { AppConfigService } from "./app-config.service";
import { ProductApiService } from "./api/product-api.service";
import { ProductResourceModel } from "@app/resource-models/product.resource-model";
import { ComponentType } from "@app/models/component-type.enum";
import { SharedLoaderService } from "./shared-loader.service";
import { finalize } from "rxjs/operators";
import { PermissionService } from "./permission.service";
import { Errors } from "@app/models/errors";

/**
 * Export claim service class.
 * TODO: Write a better class header: claims functions services.
 */
@Injectable({ providedIn: 'root' })
export class ClaimService {

    private buttonSelectors: Array<string> = ['.create-button', '.proceed-button'];
    public isCreateClaimDisplayed: boolean = false;

    public constructor(
        private navProxy: NavProxyService,
        private authService: AuthenticationService,
        private userPath: UserTypePathHelper,
        private sharedAlertService: SharedAlertService,
        private sharedLoaderService: SharedLoaderService,
        private appConfigService: AppConfigService,
        private productApiService: ProductApiService,
        private permissionService: PermissionService,
    ) {
    }

    public async createClaimBySelectingPolicy(
        policies: Array<PolicyViewModel>,
        customerId: string,
    ): Promise<void> {
        if (!policies.length) {
            return await this.createClaimBySelectingProduct(customerId);
        }
        if (!await this.askUserIfTheyWouldLikeToMakeAClaimAgainstAnExistingPolicy()) {
            return await this.createClaimBySelectingProduct(customerId);
        }
        let items: Array<any> = [];
        policies.forEach((policy: PolicyViewModel) => {
            items.push({
                type: 'radio',
                label: policy.productName + ' ' + policy.policyNumber,
                value: {
                    id: policy.id,
                    policyNumber: policy.policyNumber,
                    productAlias: policy.productId,
                },
                checked: false,
                handler: (): any => {
                    this.enableButtons();
                },
            });
        });

        const alertOpts: AlertOptions = {
            header: 'Create new claim',
            subHeader: `Select the ${this.authService.isMutualTenant() ? 'protection' : 'policy'} 
                        you would like to create a claim in relation to.`,
            buttons: [
                {
                    text: 'CANCEL',
                    role: 'cancel',
                },
                {
                    text: 'OK',
                    cssClass: 'proceed-button',
                    handler: async (data: any): Promise<any> => {
                        if (data) {
                            await this.verifyProductClaimAndNavigate(data);
                        }
                    },
                },
            ],
            inputs: items,
        };
        this.presentAlertBox(alertOpts);
    }

    public async askUserIfTheyWouldLikeToMakeAClaimAgainstAnExistingPolicy(): Promise<boolean> {
        return new Promise((resolve: any, reject: any): void => {
            this.sharedAlertService.showWithActionHandler({
                header: 'Create claim against policy?',
                subHeader: 'Would you like to create a claim against an existing policy?',
                buttons: [
                    {
                        text: 'No',
                        handler: (): any => {
                            resolve(false);
                        },
                    },
                    {
                        text: 'Yes',
                        handler: (): any => {
                            resolve(true);
                        },
                    },
                ],
            });
        });
    }

    public async createClaimBySelectingProduct(customerId: string = null): Promise<void> {
        let hasManageClaims: boolean = this.permissionService.hasManageClaimPermission();

        if (!hasManageClaims) {
            this.sharedAlertService.showWithOk(
                'Manage Claim Permission Not Found',
                'You cannot create claims. '
                + 'You do not have perimssion to create or manage claims, '
                + 'please ask for the administrator for this permission. '
                + 'If you need assistance, please don\'t hesitate to get in touch with customer support.',
                true,
            );
            return;
        }

        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('hasFeatureSettingsEnabled', [ProductFeatureSettingItem.Claims]);
        params.set('hasComponentTypes', [ComponentType.Claim]);
        params.set('environment', this.appConfigService.getEnvironment());
        await this.sharedLoaderService.presentWithDelay();
        this.productApiService.getList(params)
            .pipe(finalize(() => this.sharedLoaderService.dismiss()))
            .subscribe((products: Array<ProductResourceModel>) => {
                let activeProducts: Array<ProductResourceModel> =
                products.filter((p: ProductResourceModel) => !p.disabled && !p.deleted);
                if (products.length === 0 || activeProducts.length === 0) {
                    this.sharedAlertService.showWithCustomButton(
                        'No products found',
                        'You can\'t create a new claim right from here right now. '
                        + 'There must be at least one product which has claim transactions enabled, '
                        + 'has a claim component deployed to this environment, '
                        + 'and has been configured to allow claims to be created without requiring them to be '
                        + 'created against a policy.'
                        + 'If you want to create a claim against an existing policy, you could try going to that '
                        + 'policy and creating a claim against it directly.',
                        'Close',
                    );
                } else {
                    this.showCreateClaimDialog(products, customerId);
                }
            });
    }

    private async showCreateClaimDialog(
        products: Array<ProductResourceModel>,
        customerId: string = null,
    ): Promise<void> {
        if (this.authService.isCustomer()) {
            customerId = this.authService.customerId;
        }
        if (!this.isCreateClaimDisplayed) {
            this.isCreateClaimDisplayed = true;
            let inputsList: Array<any> = [];
            let activeProducts: Array<ProductResourceModel> =
                products.filter((p: ProductResourceModel) => !p.disabled && !p.deleted);
            if (activeProducts.length === 1) {
                this.redirectToCreateClaim(activeProducts[0], customerId);
                this.isCreateClaimDisplayed = false;
            } else {
                for (const product of activeProducts) {
                    inputsList.unshift({
                        type: 'radio',
                        label: product.name,
                        value: product.alias,
                        checked: false,
                        handler: (): any => {
                            const createButton: HTMLElement = document.querySelector('.create-button') as HTMLElement;
                            createButton.removeAttribute('disabled');
                            createButton.style.removeProperty('color');
                        },
                    });
                }
                const alertOpts: AlertOptions = {
                    header: 'Create claim',
                    subHeader: 'Select the product you wish to create a claim for',
                    buttons: [
                        {
                            text: 'CANCEL',
                            role: 'cancel',
                        },
                        {
                            text: 'CREATE',
                            cssClass: 'create-button',
                            handler: (dataSelected: any): any => {
                                let queryParams: any = {
                                    productAlias: dataSelected,
                                };
                                if (customerId) {
                                    queryParams['customerId'] = customerId;
                                }
                                this.navProxy.navigate([this.userPath.claim, 'create'], { queryParams: queryParams });
                            },
                        },
                    ],
                    inputs: inputsList,
                };
                this.presentAlertBox(alertOpts);
            }
        }
    }

    private async presentAlertBox(alertOpts: any): Promise<void> {
        const alert: HTMLIonAlertElement = await this.sharedAlertService.create({
            id: 'products-alert-box',
            header: alertOpts.header,
            subHeader: alertOpts.subHeader,
            buttons: alertOpts.buttons,
            inputs: alertOpts.inputs,
        });

        alert.componentOnReady().then(() => {
            this.disableButtons();
        });

        alert.onDidDismiss().then(() => {
            this.isCreateClaimDisplayed = false;
        });

        await alert.present();
    }

    private disableButtons(): void {
        this.buttonSelectors.forEach((selector: string) => {
            let button: HTMLElement = <HTMLElement>document.querySelector(selector);
            if (button != null) {
                button.setAttribute('disabled', 'disabled');
                button.style.setProperty('color', 'var(--ion-color-medium,#989aa2)', 'important');
            }
        });
    }

    private enableButtons(): void {
        this.buttonSelectors.forEach((selector: string) => {
            const button: HTMLElement = <HTMLElement>document.querySelector(selector);
            if (button != null) {
                button.removeAttribute('disabled');
                button.style.removeProperty('color');
            }
        });
    }

    private async verifyProductClaimAndNavigate(product: any): Promise<void> {
        await this.sharedLoaderService.presentWithDelay();
        try {
            const hasClaimComponent: boolean = await this.productApiService
                .hasClaimComponent(product.productAlias)
                .toPromise();
            if (!hasClaimComponent) {
                this.sharedAlertService.showError(
                    Errors.Claim.CreationAgainstPolicyFailed(
                        product.productAlias,
                        this.appConfigService.getEnvironment(),
                    ),
                );
            } else {
                this.navProxy.navigateForward(
                    [this.userPath.claim, 'create'],
                    true,
                    {
                        queryParams: {
                            policyId: product.id,
                            productAlias: product.productAlias,
                            policyNumber: product.policyNumber,
                        },
                    },
                );
            }
        } finally {
            this.sharedLoaderService.dismiss();
        }
    }

    private redirectToCreateClaim(product: ProductResourceModel, customerId: string = null): void {
        let queryParams: any = {
            productAlias: product.alias,
        };
        if (customerId) {
            queryParams['customerId'] = customerId;
        }
        this.navProxy.navigate([this.userPath.claim, 'create'], { queryParams: queryParams });
    }
}
