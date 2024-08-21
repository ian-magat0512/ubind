import { Component, OnInit, ElementRef, Injector } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { LoadingController } from '@ionic/angular';
import { DeploymentEnvironment } from '@app/models/deployment-environment.enum';
import Pluralize from 'pluralize';
import { NumberPoolGetResultModel } from '@app/models/number-pool-result.model';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { RouteHelper } from '@app/helpers/route.helper';
import { NumberPoolApiService } from '@app/services/api/number-pool-api.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { scrollbarStyle } from '@assets/scrollbar';
import { FormGroup, FormBuilder } from '@angular/forms';
import { FormHelper } from '@app/helpers/form.helper';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { EventService } from '@app/services/event.service';
import { AppConfig } from '@app/models/app-config';
import { AppConfigService } from '@app/services/app-config.service';
import { TenantService } from '@app/services/tenant.service';
import { ProductService } from '@app/services/product.service';
import { NumberPoolAddResultModel } from '@app/models/number-pool-add-result.model';
import { NumberPoolUpdateResultModel } from '@app/models/number-pool-update-result.model';
import { NumberPoolDeleteResultModel } from '@app/models/number-pool-delete-result.model';

/**
 * Export edit number pool page component class.
 * TODO: Write a better class header: editing of pool number.
 */
@Component({
    selector: 'app-edit-number-pool',
    templateUrl: './edit-number-pool.page.html',
    styleUrls: [
        '../../../../assets/css/scrollbar-form.css',
        '../../../../assets/css/form-toolbar.scss',
        '../../../../assets/css/edit-form.scss',
    ],
    styles: [scrollbarStyle],
})
export class EditNumberPoolPage extends DetailPage implements OnInit {

    public title: string = 'Number Pool';
    private tenantId: string;
    private tenantAlias: string;
    private productAlias: string;
    private numberPoolId: string;
    public isMutual: boolean;

    public numberPoolForm: FormGroup;

    public environments: Array<DeploymentEnvironment> = [
        DeploymentEnvironment.Development,
        DeploymentEnvironment.Staging,
        DeploymentEnvironment.Production];
    public deploymentEnvironments: any = DeploymentEnvironment;
    public environmentExistingNumbers: Map<DeploymentEnvironment, Array<string>> =
        new Map<DeploymentEnvironment, Array<string>>();
    public environmentNumberString: Map<DeploymentEnvironment, string> =
        new Map<DeploymentEnvironment, string>();
    public environmentOldNumberString: Map<DeploymentEnvironment, string> =
        new Map<DeploymentEnvironment, string>();

    public productionToAdd: Array<string>;
    public productionToDelete: Array<string>;
    public developmentToAdd: Array<string>;
    public developmentToDelete: Array<string>;
    public stagingToAdd: Array<string>;
    public stagingToDelete: Array<string>;

    public numberType: string;

    public constructor(
        private sharedAlertService: SharedAlertService,
        private loadingCtrl: LoadingController,
        protected routeHelper: RouteHelper,
        private numberPoolApiService: NumberPoolApiService,
        private formBuilder: FormBuilder,
        public layoutManager: LayoutManagerService,
        protected formHelper: FormHelper,
        protected navProxy: NavProxyService,
        eventService: EventService,
        elementRef: ElementRef,
        injector: Injector,
        appConfigService: AppConfigService,
        protected tenantService: TenantService,
        protected productService: ProductService,
    ) {
        super(eventService, elementRef, injector);
        appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            if (!appConfigService.isMasterPortal()) {
                this.tenantId = appConfig.portal.tenantId;
                this.isMutual = appConfig.portal.isMutual;
            }
        });
        this.buildForm();
    }

    public ngOnInit(): void {
        this.tenantAlias = this.routeHelper.getParam("tenantAlias") || this.routeHelper.getParam("portalTenantAlias");
        this.productAlias = this.routeHelper.getParam("productAlias");
        this.numberPoolId = this.routeHelper.getParam('numberPoolId');
        this.numberType = this.numberPoolId == 'credit-note' ?
            'Credit Note' : this.capitalizeFirstLetter(this.numberPoolId);
        this.title = this.capitalizeFirstLetter(this.numberPoolId) + ' Number Pool';
        this.loadNumbers();
    }

    protected buildForm(): void {
        this.numberPoolForm = this.formBuilder.group({
            developmentNumbers: '',
            stagingNumbers: '',
            productionNumbers: '',
        });
    }

    private async loadNumbers(): Promise<void> {
        if (!this.tenantId) {
            this.tenantId = await this.tenantService.getTenantIdFromAlias(this.tenantAlias);
        }
        let promises: Array<Promise<void>> = new Array<Promise<void>>();
        this.environments.forEach((environment: DeploymentEnvironment) => {
            promises.push(this.numberPoolApiService.getAvailableNumbers(
                this.tenantId,
                this.productAlias,
                this.numberPoolId,
                environment,
            )
                .toPromise().then(
                    (res: NumberPoolGetResultModel) => {
                        this.environmentExistingNumbers.set(environment, res.numbers);
                        let joinedNumbersString: string = this.environmentExistingNumbers.get(environment).join('\n');
                        this.environmentNumberString.set(environment, joinedNumbersString);
                        this.environmentOldNumberString.set(environment, joinedNumbersString);
                    },
                    (err: HttpErrorResponse) => {
                        this.errorMessage = 'There was a problem loading numbers.';
                        throw err;
                    },
                ));
        });

        Promise.all(promises).then(() => {
            let model: any = {
                developmentNumbers: this.environmentNumberString.get(DeploymentEnvironment.Development),
                stagingNumbers: this.environmentNumberString.get(DeploymentEnvironment.Staging),
                productionNumbers: this.environmentNumberString.get(DeploymentEnvironment.Production),
            };
            this.numberPoolForm.setValue(model);
            this.isLoading = false;
        });
    }

    public getFirstFiveResourceNumbers(numberPoolAddResult: NumberPoolAddResultModel): string {
        let firstFiveNumbers: string = '';
        numberPoolAddResult.duplicateNumbers.forEach((item: string, index: number) => {
            let isFirstFiveNumber: boolean = index < 5;
            let lastIndex: boolean = index === numberPoolAddResult.duplicateNumbers.length - 1;
            if (isFirstFiveNumber && !lastIndex) firstFiveNumbers += ' ' + item + ',';
            if (isFirstFiveNumber && lastIndex) firstFiveNumbers += item;
        });
        return firstFiveNumbers;
    }

    public getNumberPoolUpdateResultMessageForAllEnvironments(
        numberPoolUpdateResult: NumberPoolUpdateResultModel,
    ): string {
        let message: string = "";
        this.environments.forEach((environment: DeploymentEnvironment) => {
            let numberPoolAddResult: NumberPoolAddResultModel = numberPoolUpdateResult.numberPoolAddResults
                .filter((r: NumberPoolAddResultModel) => r.environment == environment)[0];
            let numberPoolDeleteResult:
                NumberPoolDeleteResultModel = numberPoolUpdateResult.numberPoolDeleteResults
                    .filter((r: NumberPoolDeleteResultModel) => r.environment == environment)[0];
            if (numberPoolAddResult || numberPoolDeleteResult) {
                message += `<b>${environment}</b> :  <br/>  `
                    + `${this.getResourceNumberUpdateResultMessage(
                        numberPoolAddResult,
                        numberPoolDeleteResult,
                    )}<br/><br/>`;
            }
        });
        return message;
    }

    public getResourceNumberUpdateResultMessage(
        numberPoolAddResult: NumberPoolAddResultModel,
        numberPoolDeleteResult: NumberPoolDeleteResultModel,
    ): string {
        let message: string = "";
        let numbersWereAdded: boolean = numberPoolAddResult && numberPoolAddResult.addedNumbers.length > 0;
        let numbersWereRemoved: boolean = numberPoolDeleteResult && numberPoolDeleteResult.deletedNumbers.length > 0;
        let lessThanFiveNumbersWereDuplicated: boolean = numberPoolAddResult &&
            numberPoolAddResult.duplicateNumbers.length > 0 &&
            numberPoolAddResult.duplicateNumbers.length <= 5;
        let moreThanFiveNumbersWereDuplicated: boolean = numberPoolAddResult &&
            numberPoolAddResult.duplicateNumbers.length > 5;

        if (numbersWereAdded) {
            message += `${numberPoolAddResult.addedNumbers.length} ${numberPoolAddResult.numberType} numbers added.`;
        }
        if (numbersWereRemoved) {
            message += `${numberPoolDeleteResult.deletedNumbers.length} `
                + `${numberPoolDeleteResult.numberType} numbers removed.`;
        }
        if (lessThanFiveNumbersWereDuplicated) {
            let firstFiveDuplicateResourceNumbers: string = this.getFirstFiveResourceNumbers(numberPoolAddResult);
            message += `The following numbers were not added because they have already been allocated `
                + `to ${Pluralize(numberPoolAddResult.numberType, 100)}:${firstFiveDuplicateResourceNumbers}`;
        }
        if (moreThanFiveNumbersWereDuplicated) {
            let otherDuplicateNumbers: number = numberPoolAddResult.duplicateNumbers.length - 5;
            let firstFiveDuplicateResourceNumbers: string = this.getFirstFiveResourceNumbers(numberPoolAddResult);
            message += `The following numbers were not added because they have already been allocated `
                + `to ${Pluralize(numberPoolAddResult.numberType, 100)}:${firstFiveDuplicateResourceNumbers} `
                + `and ${otherDuplicateNumbers} more.`;
        }
        return message;
    }

    public async saveNumbers(
        env: DeploymentEnvironment,
        numbers: Array<string>,
    ): Promise<NumberPoolAddResultModel> {
        const loading: HTMLIonLoadingElement = await this.loadingCtrl.create({
            message: 'Please wait...',
            cssClass: 'detail-loader',
            showBackdrop: true,
        });
        return loading.present().then(() => {
            return this.numberPoolApiService.saveNumbers(
                this.tenantId,
                this.productAlias,
                this.numberPoolId,
                numbers,
                env,
            ).toPromise().then(
                (res: any) => {
                    loading.dismiss();
                    return res;
                },
                (err: HttpErrorResponse) => {
                    console.log(err);
                    loading.dismiss();
                },
            );
        });
    }

    public async deleteNumbers(
        env: DeploymentEnvironment,
        numbers: Array<string>,
    ): Promise<NumberPoolDeleteResultModel> {
        const loading: HTMLIonLoadingElement = await this.loadingCtrl.create({
            message: 'Please wait...',
            cssClass: 'detail-loader',
            showBackdrop: true,
        });

        return loading.present().then(() => {
            return this.numberPoolApiService.deleteNumbers(
                this.tenantId,
                this.productAlias,
                this.numberPoolId,
                numbers,
                env,
            ).toPromise().then(
                (res: any) => {
                    loading.dismiss();
                    return res;
                },
                (err: HttpErrorResponse) => {
                    console.log(err);
                    loading.dismiss();
                },
            );
        });
    }

    public onSave(): void {
        let promises: Array<any> = [];
        let numberPoolUpdateResult: NumberPoolUpdateResultModel = new NumberPoolUpdateResultModel();
        this.environments.forEach((environment: DeploymentEnvironment) => {
            this.environmentNumberString.set(
                environment,
                this.numberPoolForm.controls[environment.toLowerCase() + 'Numbers'].value,
            );
            let environmentNumbersToAdd: Array<string> =
                this.getNumbersToAdd(
                    this.environmentExistingNumbers.get(environment),
                    this.environmentNumberString.get(environment),
                );
            let environmentNumbersToDelete: Array<string> =
                this.getNumbersToDelete(
                    this.environmentExistingNumbers.get(environment),
                    this.environmentNumberString.get(environment),
                );

            if (environmentNumbersToAdd && environmentNumbersToAdd.length > 0) {
                promises.push(this.saveNumbers(
                    environment,
                    environmentNumbersToAdd,
                ).then((res: NumberPoolAddResultModel) => {
                    res.numberType = this.numberPoolId;
                    res.environment = environment;
                    numberPoolUpdateResult.numberPoolAddResults.push(res);
                }));
            }
            if (environmentNumbersToDelete && environmentNumbersToDelete.length > 0) {
                promises.push(this.deleteNumbers(
                    environment,
                    environmentNumbersToDelete,
                ).then((res: NumberPoolDeleteResultModel) => {
                    res.numberType = this.numberPoolId;
                    res.environment = environment;
                    numberPoolUpdateResult.numberPoolDeleteResults.push(res);
                }));
            }
        });
        if (promises.length > 0) {
            Promise.all(promises).then(() => {
                this.sharedAlertService.showWithActionHandler({
                    header: this.numberType + ' numbers successfully updated',
                    message: this.getNumberPoolUpdateResultMessageForAllEnvironments(numberPoolUpdateResult),
                    buttons: [
                        {
                            text: 'OK',
                            handler: (): any => this.navProxy.navigateBackN(
                                3,
                                true,
                                {
                                    queryParams: {
                                        segment: 'Settings',
                                    },
                                },
                            ),
                        },
                    ],
                });
            });
        }
    } // end of save

    public async onCancel(): Promise<void> {
        if (this.numberPoolForm.dirty) {
            if (!await this.formHelper.confirmExitWithUnsavedChanges()) {
                return;
            }
        }
        this.navProxy.navigateBackN(3, true, { queryParams: { segment: 'Settings' } });
    }

    private getNumbersToDelete(oldValue: Array<string>, newValue: string): Array<string> {
        let result: Array<string> = [];

        if (newValue.trim() == '') {
            return oldValue;
        }

        const newValueArray: Array<string> = newValue.split('\n');
        oldValue.forEach((item: string) => {
            const _find: string = newValueArray.find((el: string) => el.toLowerCase() == item.toLowerCase());
            if (!_find && item.trim() != '') {
                result.push(item.trim());
            }
        });

        return result;
    }

    private getNumbersToAdd(oldValue: Array<string>, newValue: string): Array<string> {
        let result: Array<string> = [];

        if (newValue.trim() == '') {
            return [];
        }

        const newValueArray: Array<string> = newValue.split('\n');
        newValueArray.forEach((item: string) => {
            const _find: string = oldValue.find((el: string) => el.toLowerCase() === item.toLowerCase());
            const _exists: string = result.find((el: string) => el.toLowerCase() === item.toLowerCase());
            if (!_find && item.trim() != '' && !_exists) {
                result.push(item.trim());
            }
        });

        return result;
    }

    private capitalizeFirstLetter(s: string): string {
        if (typeof s !== 'string') return '';
        return s.charAt(0).toUpperCase() + s.slice(1);
    }
}
