import { SharedLoaderService } from "./shared-loader.service";
import { finalize } from "rxjs/operators";
import { SharedAlertService } from "./shared-alert.service";
import { NavProxyService } from "./nav-proxy.service";
import { Injectable } from "@angular/core";
import { DkimSettingsResourceModel, DkimSettingsUpsertModel } from "@app/resource-models/dkim-settings.resource-model";
import { DkimSettingsApiService } from "./api/dkim-settings-api.service";
import { RouteHelper } from "@app/helpers/route.helper";
import { DkimTestEmailResourceModel } from "@app/resource-models/dkim-test-email.resource-model";
import { LoadingController } from "@ionic/angular";

/**
 * Export DKIM setting service class.
 * TODO: This class is used to perform DKIM settings API call.
 */
@Injectable({ providedIn: 'root' })
export class DkimSettingsService {

    public constructor(
        private sharedLoaderService: SharedLoaderService,
        private sharedAlertService: SharedAlertService,
        private dkimSettingsApiService: DkimSettingsApiService,
        private navProxy: NavProxyService,
        private routeHelper: RouteHelper,
        private loadCtrl: LoadingController,
    ) {
    }

    public async create(dkimSettingsResourceModel: DkimSettingsUpsertModel): Promise<DkimSettingsResourceModel> {
        await this.sharedLoaderService.presentWithDelay();
        return this.dkimSettingsApiService.createDkimSettings(dkimSettingsResourceModel)
            .pipe(finalize(() => {
                this.sharedLoaderService.dismiss();
            }))
            .toPromise().then((dkimSettingsResourceModel: DkimSettingsResourceModel) => {
                const domainName: string = dkimSettingsResourceModel.domainName;
                this.sharedAlertService.showToast(`DKIM configuration for ${domainName} was created`);
                return dkimSettingsResourceModel;
            });
    }

    public async update(
        dkimSettingsId: string,
        organisationId: string,
        model: DkimSettingsUpsertModel,
    ): Promise<DkimSettingsResourceModel> {
        await this.sharedLoaderService.presentWithDelay();
        return this.dkimSettingsApiService.updateDkimSettings(dkimSettingsId, model, organisationId)
            .pipe(finalize(() => {
                this.sharedLoaderService.dismiss();
            }))
            .toPromise().then((dkimSettingsResourceModel: DkimSettingsResourceModel) => {
                this.sharedAlertService.showToast(
                    `DKIM configuration for ${dkimSettingsResourceModel.domainName} was updated`);
                return dkimSettingsResourceModel;
            });
    }

    public async sendDkimTestEmail(model: DkimTestEmailResourceModel): Promise<void> {
        await this.sharedLoaderService.presentWithDelay("Sending DKIM test email");
        await this.dkimSettingsApiService
            .sendDkimTestEmail(
                model,
            )
            .pipe(finalize(() => {
                this.sharedLoaderService.dismiss();
            }))
            .toPromise().then(() => {
                this.sharedAlertService.showToast(`A DKIM test email was sent to ${model.recipientEmailAddress}`);
            });
    }

    public async delete(dkimSettingsId: string, organisationId: string, domainName: string): Promise<void> {
        await this.sharedLoaderService.presentWithDelay();
        await this.dkimSettingsApiService
            .deleteDkimSettings(dkimSettingsId, organisationId, this.routeHelper.getContextTenantAlias())
            .pipe(finalize(() => {
                this.sharedLoaderService.dismiss();
            }))
            .toPromise().then(() => {
                this.sharedAlertService.showToast(`DKIM configuration for ${domainName} was deleted`);
            });
    }
}
