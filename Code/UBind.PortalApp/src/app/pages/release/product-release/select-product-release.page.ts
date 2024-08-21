import { Component, ElementRef, Injector, OnDestroy, OnInit } from '@angular/core';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { EventService } from '@app/services/event.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { finalize, last, takeUntil } from 'rxjs/operators';
import { Observable, Subject } from 'rxjs';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { contentAnimation } from '@assets/animations';
import { AuthenticationService } from '@app/services/authentication.service';
import { ReleaseResourceModel } from '@app/resource-models/release.resource-model';
import { ProductApiService } from '@app/services/api/product-api.service';
import { ReleaseStatus } from '@app/models/release-status.enum';
import { DeploymentEnvironment } from '@app/models/deployment-environment.enum';
import { ReleaseApiService } from '@app/services/api/release-api.service';
import {
    QuotePolicyTransactionCountResourceModel,
} from '@app/resource-models/quote-policy-transaction-count.resource-model';

/**
 * Move the quotes associated on this release to another release.
 */
@Component({
    selector: 'app-select-product-release',
    templateUrl: './select-product-release.page.html',
    styleUrls: [
        './select-product-release.page.scss',
        '../../../../assets/css/form-toolbar.scss',
    ],
    animations: [contentAnimation],
})
export class SelectProductRelease extends DetailPage implements OnInit, OnDestroy {
    public selectedReleaseId: string;
    public isLoading: boolean;
    public release: ReleaseResourceModel;
    public releases: Array<ReleaseResourceModel>;
    public releaseStatus: typeof ReleaseStatus = ReleaseStatus;
    public releaseId: string;
    public tenantAlias: string;
    public productAlias: string;
    public environment: DeploymentEnvironment;
    private quoteCount: number = 0;
    private policyTransactionCount: number = 0;

    public constructor(
        protected eventService: EventService,
        public elementRef: ElementRef,
        public injector: Injector,
        public layoutManager: LayoutManagerService,
        private navProxy: NavProxyService,
        private routeHelper: RouteHelper,
        private authService: AuthenticationService,
        private productApiService: ProductApiService,
        private releaseApiService: ReleaseApiService,
        private sharedAlertService: SharedAlertService) {
        super(eventService, elementRef, injector);
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.releaseId = this.routeHelper.getParam('releaseId');
        this.tenantAlias = this.routeHelper.getParam('tenantAlias');
        this.productAlias = this.routeHelper.getParam('productAlias');
        let env: string = this.routeHelper.getParam('environment');
        this.environment = DeploymentEnvironment[env];
        this.loadReleases();
        this.getQuoteAndPolicyTransactionCount();
    }

    public ngOnDestroy(): void {
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    public async closeButtonClicked(): Promise<void> {
        if (this.selectedReleaseId) {
            await this.sharedAlertService.showWithActionHandler({
                header: 'Unsaved Changes',
                subHeader: 'You have unsaved changes, are you sure you wish '
                    + 'to close the current view without saving them?',
                buttons: [
                    {
                        text: 'No',
                        handler: (): any => {
                            return;
                        },
                    },
                    {
                        text: 'Yes',
                        handler: (): any => {
                            this.returnToRelease();
                        },
                    },
                ],
            });
        } else {
            this.returnToRelease();
        }
    }

    public truncateDescription(release: ReleaseResourceModel): void {
        release.truncatedDescription = !release.truncatedDescription;
        this.selectRelease(release.id);
    }

    public selectRelease(releaseId: string): void {
        this.selectedReleaseId = releaseId;
    }

    public change(event: any): void {
        this.selectRelease(event.value);
    }

    public async moveButtonClicked(): Promise<void> {
        const destinationRelease: ReleaseResourceModel | undefined =
            this.releases.find((r: ReleaseResourceModel) => r.id === this.selectedReleaseId);
        if (!this.selectedReleaseId) {
            await this.sharedAlertService.showWithOk(
                'Please select a product release',
                'You must select a product release from the list before you can proceed',
                true);
        } else if (this.release) {
            this.migrateQuotesAndPolicyTransactionsToNewRelease(destinationRelease);
        } else {
            this.migrateUnassociatedQuotesAndPolicyTransactions(destinationRelease);
        }
    }

    private migrateQuotesAndPolicyTransactionsToNewRelease(destinationRelease: ReleaseResourceModel): void {
        if (this.quoteCount === 0 && this.policyTransactionCount === 0) {
            this.sharedAlertService.showWithOk(
                'No quotes or policy transactions to move',
                `There are no ${this.environment.toLowerCase()} quotes or policy transactions `
                + `associated with release ${this.release.number} to move to release ${destinationRelease.number}.`,
                true);
            return;
        }

        this.sharedAlertService.showWithActionHandler({
            header: 'Confirm moving quotes',
            message: `There are ${this.quoteCount} quotes with ${this.policyTransactionCount} `
                + `associated policy transactions that use release ${this.release.number}. `
                + `Are you sure you would like to move these to use release ${destinationRelease.number}?`,
            buttons: [
                {
                    text: 'CANCEL',
                    handler: (): any => {
                        return;
                    },
                },
                {
                    text: 'MOVE',
                    handler: (): void => {
                        this.migrateQuotesAndPolicyTransactionsToRelease(destinationRelease.number);
                    },
                },
            ],
        });
    }

    private migrateUnassociatedQuotesAndPolicyTransactions(destinationRelease: ReleaseResourceModel): void {
        if (this.quoteCount === 0 && this.policyTransactionCount === 0) {
            this.sharedAlertService.showWithOk(
                'No quotes or policy transactions to move',
                `There are no unassociated ${this.environment.toLowerCase()} quotes or policy transactions `
                    + `to move to release ${destinationRelease.number}.`,
                true);
            return;
        }

        this.sharedAlertService.showWithActionHandler({
            header: 'Move quotes and policy transactions',
            message: `There are ${this.quoteCount} quotes and ${this.policyTransactionCount} policy transactions `
                + `which are not associated with a specific release that will be moved `
                + `to release ${destinationRelease.number}. Do you wish to continue?`,
            buttons: [
                {
                    text: 'NO',
                    handler: (): any => {
                        return;
                    },
                },
                {
                    text: 'YES',
                    handler: (): void => {
                        this.migrateQuotesAndPolicyTransactionsToRelease(destinationRelease.number);
                    },
                },
            ],
        });
    }

    private async loadReleases(): Promise<void> {
        this.isLoading = true;
        return this.productApiService.getReleasesForProduct(this.productAlias, this.tenantAlias)
            .pipe(
                finalize(() => this.isLoading = false),
                takeUntil(this.destroyed),
                last(),
            )
            .toPromise().then(
                (releaseModel: Array<ReleaseResourceModel>) => {
                    this.release = releaseModel.filter((x: ReleaseResourceModel) => x.id == this.releaseId)[0];
                    this.releases = releaseModel.filter((x: ReleaseResourceModel) => x.id != this.releaseId);
                },
                (err: any) => {
                    // needed to be paired with last() rxjs function, throws error when return is undefined
                    // when destroying or canceling the api request
                    if (err.name != 'EmptyError') {
                        this.errorMessage = 'There was a problem loading the releases';
                        throw err;
                    }
                });
    }

    private async migrateQuotesAndPolicyTransactionsToRelease(releaseNumber: number): Promise<void> {
        const migrateApi: Observable<any> = this.releaseId
            ? this.releaseApiService.migrateQuotesAndPolicyTransactionsToRelease(
                this.releaseId,
                this.selectedReleaseId,
                this.environment,
                this.routeHelper.getContextTenantAlias())
            : this.releaseApiService.migrateUnassociatedEntitiesToRelease(
                this.selectedReleaseId,
                this.environment,
                this.routeHelper.getContextTenantAlias(),
                this.productAlias);
        migrateApi
            .pipe(takeUntil(this.destroyed)).subscribe(() => {
                const message: string = `${this.quoteCount} ${this.environment.toLowerCase()} quotes `
                    + `and ${this.policyTransactionCount} policy transactions `
                    + (this.releaseId ? `associated with release ${this.release.number} ` : `which are unassociated `)
                    + `are now being moved to release ${releaseNumber}`;
                this.sharedAlertService.showToast(message);
                this.returnToRelease();
            });
    }

    private async getQuoteAndPolicyTransactionCount(): Promise<void> {
        const getCountApi: Observable<QuotePolicyTransactionCountResourceModel> = this.releaseId
            ? this.releaseApiService.getQuoteAndPolicyTransactionCount(
                this.releaseId, this.environment, this.routeHelper.getContextTenantAlias())
            : this.releaseApiService.getUnassociatedQuoteAndPolicyTransactionCount(
                this.productAlias, this.environment, this.routeHelper.getContextTenantAlias());
        getCountApi
            .pipe(takeUntil(this.destroyed))
            .subscribe((data: QuotePolicyTransactionCountResourceModel) => {
                this.quoteCount = data.quoteCount || 0;
                this.policyTransactionCount = data.policyTransactionCount || 0;
            });
    }

    private returnToRelease(): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.pop();
        pathSegments.pop();
        const queryParams: any = this.releaseId ? {} : { queryParams: { segment: 'Releases' } } ;
        this.navProxy.navigateForward(pathSegments, true, queryParams);
    }
}
