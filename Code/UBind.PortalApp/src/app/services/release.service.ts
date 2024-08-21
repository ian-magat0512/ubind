import { SharedLoaderService } from "./shared-loader.service";
import { ReleaseApiService } from "./api/release-api.service";
import { finalize } from "rxjs/operators";
import { ReleaseResourceModel } from "@app/resource-models/release.resource-model";
import { ReleaseRequestModel } from "@app/models/release-request.model";
import { SharedAlertService } from "./shared-alert.service";
import { EventService } from "./event.service";
import { RouteHelper } from "@app/helpers/route.helper";
import { NavProxyService } from "./nav-proxy.service";
import { ReleaseType } from "@app/models";
import { Injectable } from "@angular/core";
import { Subscription } from "rxjs";
import { ProblemDetails } from "@app/models/problem-details";

/**
 * Export release service class.
 * TODO: Write a better class header: release functions of the service.
 */
@Injectable({ providedIn: 'root' })
export class ReleaseService {

    public constructor(
        private sharedLoaderService: SharedLoaderService,
        private sharedAlertService: SharedAlertService,
        private releaseApiService: ReleaseApiService,
        private eventService: EventService,
        private routeHelper: RouteHelper,
        private navProxy: NavProxyService,
    ) {
    }

    public async createRelease(
        tenant: string,
        product: string,
        description: string,
        type: ReleaseType,
    ): Promise<void> {
        await this.sharedLoaderService.presentWithDelay('Creating a new release...');
        let subscription: Subscription = this.releaseApiService.create(tenant, product, description, type)
            .pipe(finalize(() => {
                this.sharedLoaderService.dismiss();
                subscription.unsubscribe();
            }))
            .subscribe(
                (release: ReleaseResourceModel) => {
                    this.sharedAlertService.showToast(`Release ${release.number} `
                        + `was created for ${release.productName}`);
                    this.eventService.getEntityCreatedSubject('Release').next(release);
                    this.navigateToRelease(release, tenant);
                });
    }

    public async updateRelease(
        model: ReleaseResourceModel,
        tenantAlias: string,
    ): Promise<void> {
        await this.sharedLoaderService.presentWithDelay();
        let request: ReleaseRequestModel = new ReleaseRequestModel(model);
        const subscription: Subscription = this.releaseApiService.update(model.id, request)
            .pipe(finalize(() => {
                this.sharedLoaderService.dismiss();
                subscription.unsubscribe();
            }))
            .subscribe((release: ReleaseResourceModel) => {
                this.sharedAlertService.showToast(
                    `Release details for release ${release.number} of ${release.productName} were saved`,
                );
                this.eventService.getEntityUpdatedSubject('Release').next(release);
                this.navigateToRelease(release, tenantAlias);
            });
    }

    public async handleProductReleaseWasNotSet(
        entityType: string,
        action: string,
        appError: ProblemDetails,
        entityId: string = null,
        path: string = null): Promise<void> {
        const title: string = `No default product release set for environment`;
        const message: string = `This ${entityType} cannot be ${action} because the `
            + `${appError.Data.productName} product does not have a default product `
            + `release set for the ${appError.Data.environment.toLowerCase()} environment.`;
        this.showModalMessage(title, message, entityId, path);
    }

    private navigateToRelease(release: ReleaseResourceModel, tenantAlias: string): void {
        const pathSegmentsAfter: Array<string> = this.routeHelper.getPathSegmentsAfter('path');
        if (pathSegmentsAfter[0] === 'product') {
            this.navProxy.navigateBack(['product', release.productAlias, 'release', release.id]);
        } else {
            this.navProxy.navigateBack([
                'tenant',
                tenantAlias,
                'product',
                release.productAlias,
                'release',
                release.id,
            ]);
        }
    }

    private showModalMessage(title: string, message: string, entityId: string, path: string): void {
        if (entityId) {
            this.sharedAlertService.showWithActionHandler({
                header: title,
                subHeader: message,
                buttons: [
                    {
                        text: 'OK', handler: (): void => {
                            this.navProxy.navigateBack([path, entityId]);
                        },
                    },
                ],
            });
        } else {
            this.sharedAlertService.showWithOk(title, message);
        }
    }
}
