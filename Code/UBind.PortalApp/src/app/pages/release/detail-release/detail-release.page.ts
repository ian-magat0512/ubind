import { Component, Injector, ElementRef, OnDestroy, OnInit, AfterViewInit } from '@angular/core';
import { PopoverController, ToastController, AlertController, LoadingController } from '@ionic/angular';
import { Subject } from 'rxjs';
import { StringHelper } from '@app/helpers';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { ReleaseType, ConfigurationFileResourceModel, DeploymentResourceModel } from '@app/models';
import { DeploymentEnvironment } from '@app/models/deployment-environment.enum';
import { ReleaseResourceModel } from '@app/resource-models/release.resource-model';
import { ReleaseApiService } from '@app/services/api/release-api.service';
import { DeploymentApiService } from '@app/services/api/deployment-api.service';
import { PopoverReleasePage } from '../popover-release/popover-release.page';
import { contentAnimation } from '../../../../assets/animations';
import { scrollbarStyle } from '@assets/scrollbar';
import { Permission } from '@app/helpers/permissions.helper';
import { saveAs } from 'file-saver';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { finalize, last, takeUntil } from 'rxjs/operators';
import { EventService } from '@app/services/event.service';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { SharedPopoverService } from '@app/services/shared-popover.service';
import { SharedToastService } from '@app/services/shared-toast.service';
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import { ReleaseDetailViewModel } from '@app/viewmodels/release-detail.viewmodel';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { PermissionService } from '@app/services/permission.service';
import { ActionButton } from '@app/models/action-button';
import { IconLibrary } from '@app/models/icon-library.enum';
import { PopoverCommand, PopoverCommandData } from '@app/models/popover-command';
import { titleCase } from 'title-case';
import { DetailListItemHelper } from '@app/helpers/detail-list-item.helper';
import { ConfigurationFileViewModel } from '@app/viewmodels/configuration-file.viewmodel';
import { FormType } from '@app/models/form-type.enum';
import {
    QuotePolicyTransactionCountResourceModel,
} from '@app/resource-models/quote-policy-transaction-count.resource-model';
/**
 * Export detail release page component class.
 * This class manage displaying the release details.
 */
@Component({
    selector: 'app-detail-release',
    templateUrl: './detail-release.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-detail.css',
    ],
    styles: [scrollbarStyle],
})
export class DetailReleasePage extends DetailPage implements OnInit, AfterViewInit, OnDestroy {
    public currentIndex: number;
    public segment: string = 'Details';
    public title: string = 'Release';
    public release: ReleaseDetailViewModel;
    public releaseType: any = ReleaseType;
    public deploymentEnvironment: any = DeploymentEnvironment;
    public deployedTo: string = this.deploymentEnvironment.None.toString();
    public deployments: Array<string> = [];
    public sourceFiles: Array<Array<ConfigurationFileResourceModel>> = [];
    public permission: typeof Permission = Permission;
    private productAlias: string;
    private tenantAlias: string;
    public componentTypes: Array<FormType> = [FormType.Quote, FormType.Claim];
    public detailsListItems: Array<DetailsListItem>;
    public releaseId: string;
    public canShowMore: boolean = false;
    public flipMoreIcon: boolean = false;
    public actionButtonList: Array<ActionButton>;
    public isLoadingSourceFiles: boolean;
    public sourceFilesErrorMessage: string;
    public rootFiles: Map<FormType, Array<ConfigurationFileViewModel>>
        = new Map<FormType, Array<ConfigurationFileViewModel>>();
    public privateFiles: Map<FormType, Array<ConfigurationFileViewModel>>
        = new Map<FormType, Array<ConfigurationFileViewModel>>();
    public assetFiles: Map<FormType, Array<ConfigurationFileViewModel>>
        = new Map<FormType, Array<ConfigurationFileViewModel>>();
    private sourceFilesLoaded: boolean = false;
    public sourceFilesFound: boolean = false;
    public canGoBack: boolean = true;

    public constructor(
        public navProxy: NavProxyService,
        private routeHelper: RouteHelper,
        private alertCtrl: AlertController,
        private sharedAlertService: SharedAlertService,
        private popoverCtrl: PopoverController,
        private releaseApiService: ReleaseApiService,
        private toastCtrl: ToastController,
        private deploymentApiService: DeploymentApiService,
        protected loadCtrl: LoadingController,
        public layoutManager: LayoutManagerService,
        protected eventService: EventService,
        elementRef: ElementRef,
        injector: Injector,
        private sharedPopoverService: SharedPopoverService,
        private sharedToastService: SharedToastService,
        private sharedLoaderService: SharedLoaderService,
        private permissionService: PermissionService,
    ) {
        super(eventService, elementRef, injector);
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.segment = this.routeHelper.getParam('segment') || this.segment;
        this.loadCurrentSegment();
        this.initializeActionButtonList();
    }

    public ngAfterViewInit(): void {
        this.tenantAlias = this.routeHelper.getParam('tenantAlias');
        this.productAlias = this.routeHelper.getParam('productAlias');
        this.eventService.getEntityCreatedSubject('Release')
            .pipe(takeUntil(this.destroyed))
            .subscribe((release: ReleaseResourceModel) => {
                if (this.release.id == release.id) {
                    this.release = new ReleaseDetailViewModel(release);
                    this.loadDetails();
                }
            });
    }

    public async loadDetails(): Promise<void> {
        this.isLoading = true;
        this.releaseId = this.routeHelper.getParam('releaseId');
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', this.routeHelper.getContextTenantAlias());
        return this.releaseApiService.getById(this.releaseId, params)
            .pipe(
                takeUntil(this.destroyed),
                finalize(() => this.isLoading = false),
            ).toPromise().then(async (result: ReleaseResourceModel) => {
                if (result) { // will be null if we navigate away from the page during loading
                    this.release = new ReleaseDetailViewModel(result);
                    this.title = 'Release ' + this.release.number;
                    await this.loadDeployment();
                    this.initializeDetailsListItems();
                    this.canShowMore = this.permissionService.hasOneOfPermissions([
                        Permission.ManageReleases,
                        Permission.PromoteReleasesToProduction,
                        Permission.PromoteReleasesToStaging]);
                }
            }, (err: any) => {
                this.errorMessage = 'There was a problem loading the release details';
                throw err;
            });
    }

    public async loadDeployment(): Promise<void> {
        this.isLoading = true;
        if (this.release) {
            return new Promise((resolve: any, reject: any): any => {
                this.deploymentApiService.getCurrentDeployments(
                    this.release.tenantId,
                    this.release.productId,
                ).pipe(
                    finalize(() => this.isLoading = false),
                    takeUntil(this.destroyed),
                ).subscribe(
                    async (deployments: Array<DeploymentResourceModel>) => {
                        this.deployments = [];
                        this.deployedTo = '';
                        if (deployments && deployments.length > 0) {
                            const activeDeployments: Array<DeploymentResourceModel> = deployments.filter(
                                (el: DeploymentResourceModel) =>
                                    el.release != null && el.release.id == this.release.id,
                            );
                            activeDeployments.forEach((item: DeploymentResourceModel) => {
                                this.deployments.push(item.environment);
                            });
                            if (activeDeployments && activeDeployments.length > 0) {
                                this.deployedTo = this.deployments.join(', ');
                            } else {
                                this.deployedTo = DeploymentEnvironment.None.toString();
                            }
                        }
                        resolve();
                    },
                );
            });
        }
    } // end of fetching deployments

    private async loadSourceFiles(): Promise<void> {
        if (this.sourceFilesLoaded) {
            // let's not load them again if they're already loaded.
            return;
        }
        this.isLoadingSourceFiles = true;
        if (!this.release) {
            await this.loadDetails();
            if (!this.release) {
                // someone has navigated away from the page
                return;
            }
        }
        this.rootFiles[FormType.Quote] = [];
        this.privateFiles[FormType.Quote] = [];
        this.assetFiles[FormType.Quote] = [];
        this.rootFiles[FormType.Claim] = [];
        this.privateFiles[FormType.Claim] = [];
        this.assetFiles[FormType.Claim] = [];
        return this.releaseApiService.getSourceFiles(this.release.id, this.routeHelper.getContextTenantAlias())
            .pipe(
                finalize(() => this.isLoadingSourceFiles = false),
                takeUntil(this.destroyed),
                last())
            .toPromise().then((resourceModels: Array<ConfigurationFileResourceModel>) => {
                if (resourceModels) { // will be null if we navigate away from the page during loading
                    this.sourceFilesFound = resourceModels.length > 0;
                    resourceModels.forEach((file: ConfigurationFileResourceModel) => {
                        if (file.path.indexOf('/') === -1) {
                            this.rootFiles[file.formType].push(new ConfigurationFileViewModel(file));
                        } else if (file.path.startsWith("files/")) {
                            this.privateFiles[file.formType].push(new ConfigurationFileViewModel(file));
                        } else if (file.path.startsWith("assets/")) {
                            this.assetFiles[file.formType].push(new ConfigurationFileViewModel(file));
                        }
                    });
                    this.sourceFilesLoaded = true;
                }
            },
            (err: any) => {
                // needed to be paired with last() rxjs function, throws error when return is undefined
                // when destroying or canceling the api request
                if (err.name != 'EmptyError') {
                    this.sourceFilesErrorMessage = 'There was a problem loading the source files';
                    throw err;
                }
            });
    }


    public userDidTapEditButton(): void {
        const pathSegmentsAfter: Array<string> = this.routeHelper.getPathSegmentsAfter('path');
        if (pathSegmentsAfter[0] === 'product') {
            this.navProxy.navigateForward([
                'product',
                this.productAlias,
                'release',
                this.release.id,
                'edit']);
        } else {
            this.navProxy.navigateForward([
                'tenant',
                this.tenantAlias,
                'product',
                this.productAlias,
                'release',
                this.release.id,
                'edit']);
        }
    }

    public async presentPopover(event: any): Promise<void> {
        this.flipMoreIcon = true;
        const popoverDismissAction = (command: PopoverCommand): void => {
            this.flipMoreIcon = false;
            if (!(command && command.data)) {
                return;
            }
            const environment: DeploymentEnvironment = command.data.environment;
            const actionName: string = command.data.action.actionName;
            if (environment === DeploymentEnvironment.Production && actionName === 'set') {
                this.userDidConfirmDeploymentToProduction(command.data)
                    .then((confirmed: boolean) => {
                        if (confirmed) {
                            this.setOrUnsetDeployment(this.releaseId, environment, actionName);
                        }
                    });
                return;
            }

            switch (command.data.action.actionName) {
                case 'edit': {
                    this.userDidTapEditButton();
                    break;
                }
                case 'set': {
                    this.setOrUnsetDeployment(this.releaseId, environment, actionName);
                    break;
                }
                case 'unset': {
                    this.userDidConfirmDemotion(command.data).then((confirmed: boolean) => {
                        if (confirmed) {
                            this.setOrUnsetDeployment(
                                null, command.data.environment, command.data.action.actionName);
                        }
                    });
                    break;
                }
                case 'restore': {
                    this.restoreToDevelopment();
                    break;
                }
                case 'move': {
                    this.migrateQuotesAndPolicyTransactionsToRelease(environment);
                    break;
                }
            }
        };

        await this.sharedPopoverService.show(
            {
                component: PopoverReleasePage,
                componentProps: { deployments: this.deployments },
                cssClass: 'custom-popover more-button-top-popover-positioning',
                event: event,
                id: 'detail-release-more-icon-popover',
            },
            'Release option popover',
            popoverDismissAction,
        );
    }

    public gotoProduct(): void {
        this.goBack();
    }

    private userDidConfirmDemotion(data: PopoverCommandData): Promise<boolean> {
        const env: string = data.environment.toLowerCase();
        const action: string = StringHelper.capitalizeFirstLetter(data.action.actionName);
        const header: string = `Unset as default product release for ${env}`;
        const message: string = `If you unset the default product release certain types of quotes ` +
            `can no longer be created for the ${env} environment. Are you sure you wish to proceed?`;
        return this.presentConfirmDialog(header, message, action);
    }

    private userDidConfirmDeploymentToProduction(data: PopoverCommandData): Promise<boolean> {
        const header: string = 'Set as default product release for production';
        const action: string = titleCase(data.action.actionName);
        const message: string = `Are you sure you wish to set this product release `
            + `as default for the production environment?`;
        return this.presentConfirmDialog(header, message, action);
    }

    private presentConfirmDialog(header: string, message: string, action: string): Promise<boolean> {
        return new Promise((resolve: any): void => {
            this.sharedAlertService.showWithActionHandler({
                header, message, buttons: [
                    {
                        text: 'Cancel',
                        handler: (): any => {
                            resolve(false);
                        },
                    },
                    {
                        text: action,
                        handler: (): any => {
                            resolve(true);
                        },
                    },
                ],
            });
        });
    }

    public async openFile(file: ConfigurationFileViewModel): Promise<void> {
        if (file.isDownloading) {
            return;
        }
        file.isDownloading = true;
        await this.sharedLoaderService.presentWithDelay('Downloading file...');
        this.releaseApiService.downloadSourceFile(
            this.release.id,
            file.sourceType,
            file.path,
            this.routeHelper.getContextTenantAlias())
            .pipe(
                takeUntil(this.destroyed),
                finalize(() => {
                    file.isDownloading = false;
                    this.sharedLoaderService.dismiss();
                }))
            .subscribe((blob: any) => {
                if (file.isBrowserViewable) {
                    const url: string = window.URL.createObjectURL(blob);
                    const newWindow: Window = window.open();
                    if (newWindow) {
                        newWindow.location.href = url;
                    }
                    window.URL.revokeObjectURL(url);
                } else {
                    saveAs(blob, file.path);
                }
            });
    }

    public segmentChanged($event: any): void {
        if ($event.detail.value != this.segment) {
            this.segment = $event.detail.value;
            this.loadCurrentSegment();
            this.initializeActionButtonList();
            this.navProxy.updateSegmentQueryStringParameter(
                'segment',
                this.segment != 'Details' ? this.segment : null);
        }
    }

    private loadCurrentSegment(): void {
        switch (this.segment) {
            case 'Details': {
                this.loadDetails();
                break;
            }
            case 'Source': {
                this.loadSourceFiles();
                break;
            }
            default:
                break;
        }
    }

    private initializeDetailsListItems(): void {
        this.detailsListItems = this.release.createDetailsList(this, this.deployedTo);
        this.updateDetailsListForEnvironment(DeploymentEnvironment.Staging);
        this.updateDetailsListForEnvironment(DeploymentEnvironment.Production);
    }

    private updateDetailsListForEnvironment(environment: DeploymentEnvironment): void {
        this.releaseApiService.getQuoteAndPolicyTransactionCount(this.releaseId, environment, this.tenantAlias)
            .subscribe((data: QuotePolicyTransactionCountResourceModel) => {
                this.updateUsageInDetailsList(environment, data.quoteCount, data.policyTransactionCount);
            });
    }

    private updateUsageInDetailsList(
        environment: DeploymentEnvironment, quoteCount: number, policyTransactionCount: number): void {
        const convertToDisplayValue = (value: number): string => (value !== 0 ? value.toString() : 'None');
        this.detailsListItems.forEach((value: DetailsListItem) => {
            if (value.GroupName === 'Usage') {
                if (value.Description === `${environment} Quotes`) {
                    value.setDisplayValue(convertToDisplayValue(quoteCount));
                } else if (value.Description === `${environment} Policy Transactions`) {
                    value.setDisplayValue(convertToDisplayValue(policyTransactionCount));
                }
            }
        });

        this.eventService.detailViewDataChanged();
    }

    private goBack(): void {
        const pathSegmentsAfter: Array<string> = this.routeHelper.getPathSegmentsAfter('path');
        if (pathSegmentsAfter[0] === 'product') {
            this.navProxy.navigateBack(
                [ 'product', this.productAlias],
                true,
                { queryParams: { segment: 'Releases' } });
        } else {
            this.navProxy.navigateBack(
                [
                    'tenant',
                    this.tenantAlias, 'product',
                    this.productAlias],
                true,
                { queryParams: { segment: 'Releases' } },
            );
        }
    }

    private async setOrUnsetDeployment(
        releaseId: string,
        environment: DeploymentEnvironment,
        actionName: string,
    ): Promise<void> {
        let message: string = this.getLoaderMessage(actionName, environment);
        await this.sharedLoaderService.present(message, null, true);

        let subscription: any = this.deploymentApiService.create(
            this.release.tenantId,
            this.release.productId,
            environment,
            releaseId,
        )
            .pipe(finalize(() => {
                subscription.unsubscribe();
                this.sharedLoaderService.dismiss();
            }))
            .subscribe(async (): Promise<any> => {
                await this.loadDeployment();
                this.updateDeploymentStatusInDetailsList();

                if (actionName === 'set') {
                    this.presentToast(`Product release ${this.release.number} has been ` +
                        `set as default for the ${environment.toLowerCase()} environment`);
                } else {
                    this.presentToast(`Product release ${this.release.number} has been ` +
                        `unset as default for the ${environment.toLowerCase()} environment`);
                }
            });
    }

    private getLoaderMessage(action: string, environment: DeploymentEnvironment): string {
        const env: string = environment.toLowerCase().concat('...    ');
        let message: string = action === 'set' ? 'Set as default for ' : 'Unset as default product release for ';
        return message.concat(env);
    }

    private updateDeploymentStatusInDetailsList(): void {
        let deployments: Array<string> = this.deployedTo.split(', ');
        const productionDeploymentStatus: string = deployments.indexOf(
            DeploymentEnvironment.Production.toString(),
        ) > -1 ? "Yes" : "No";
        const stagingDeploymentStatus: string = deployments.indexOf(
            DeploymentEnvironment.Staging.toString(),
        ) > -1 ? "Yes" : "No";

        this.detailsListItems.forEach((value: DetailsListItem) => {
            if (value.Description == DetailListItemHelper.detailListItemDescriptionMap.defaultForProduction) {
                value.setDisplayValue(productionDeploymentStatus);
            }
            if (value.Description == DetailListItemHelper.detailListItemDescriptionMap.defaultForStaging) {
                value.setDisplayValue(stagingDeploymentStatus);
            }
        });

        this.eventService.detailViewDataChanged();
    }

    private async restoreToDevelopment(): Promise<void> {
        await this.sharedLoaderService.present('Restoring files...');
        return this.releaseApiService.restoreToDevelopment(this.release.id, this.routeHelper.getContextTenantAlias())
            .pipe(finalize(() => this.sharedLoaderService.dismiss()))
            .toPromise().then(() => {
                this.presentAlert(
                    'The files associated with the selected release have been restored to development.',
                    'Release Restored',
                );
            });
    }

    private async presentToast(_message: string): Promise<void> {
        const toast: HTMLIonToastElement = await this.sharedToastService.create({
            message: _message,
            duration: 3000,
        });

        return await toast.present();
    }

    private async presentAlert(_message: string, _header: string): Promise<void> {
        await this.sharedAlertService.showWithActionHandler({
            header: _header,
            message: _message,
            buttons: [
                {
                    text: 'OK',
                    role: 'cancel',
                },
            ],
        });
    }

    private initializeActionButtonList(): void {
        this.actionButtonList = [];
        if (this.segment == 'Details' && this.permissionService.hasPermission(Permission.ManageReleases)) {
            this.actionButtonList.push(ActionButton.createActionButton(
                "Edit",
                "pencil",
                IconLibrary.AngularMaterial,
                false,
                "Edit Release",
                true,
                (): void => {
                    return this.userDidTapEditButton();
                },
            ));
        }
    }

    private migrateQuotesAndPolicyTransactionsToRelease(environment: DeploymentEnvironment): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.push('select-product-release');
        pathSegments.push(environment);
        this.navProxy.navigateForward(pathSegments);
    }
}
