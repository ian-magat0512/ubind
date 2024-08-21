import { Injectable } from "@angular/core";
import { ContextEntitiesConfigResourceModel } from "@app/models/context-entities-config.model";
import { FormType } from "@app/models/form-type.enum";
import { ContextEntityApiService } from "./api/context-entity-api.service";
import { ApplicationService } from "./application.service";
import { ConfigService } from "./config.service";
import { Errors } from "@app/models/errors";

/**
 * Context entity service class.
 * This class manages the context entity services functions.
 */
@Injectable({
    providedIn: 'root',
})
export class ContextEntityService {
    private contextEntities: any;
    private intervalHandle: any;
    private contextEntitySettings: any;

    public constructor(
        private contextEntityApiService: ContextEntityApiService,
        private configService: ConfigService,
        private applicationService: ApplicationService,
    ) {
    }

    public getContextEntities(): any {
        if (!this.contextEntities) {
            this.loadContextEntities();
        }

        return this.contextEntities;
    }

    public async loadContextEntities(): Promise<any> {
        const settings: ContextEntitiesConfigResourceModel =
        await this.getContextEntitySettings(this.applicationService.formType, this.applicationService.quoteType);
        if (!settings) {
            return;
        }
        const entityId: string = this.applicationService.formType == FormType.Claim
            ? this.applicationService.claimId
            : this.applicationService.quoteId;

        if (this.applicationService.formType == FormType.Quote
            && this.applicationService.quoteType == null
        ) {
            throw Errors.General.Unexpected(
                "When trying to load context entities for this quote, the quote type was null.");
        }

        this.contextEntities = await this.contextEntityApiService.getContextEntities(
            this.applicationService.organisationId,
            this.applicationService.formType,
            entityId,
            this.applicationService.quoteType,
        ).toPromise();
        return this.contextEntities;
    }

    public async setIntervalLoadingContextEntities(formType: string, quoteType: string): Promise<void> {
        if (this.intervalHandle) {
            return;
        }
        const config: ContextEntitiesConfigResourceModel = await this.getContextEntitySettings(formType, quoteType);
        if (config?.reloadIntervalSeconds) {
            // Minimum background request interval is 30 seconds
            const intervalSeconds: number = Math.max(config.reloadIntervalSeconds, 30) * 1000;
            this.intervalHandle =
                setInterval(() => this.loadContextEntities(), intervalSeconds);
        }
    }

    private async getContextEntitySettings(
        formType: string,
        quoteType: string,
    ): Promise<ContextEntitiesConfigResourceModel> {
        if (formType == FormType.Claim) {
            this.contextEntitySettings = this.configService.contextEntities?.claims;
        } else {
            const quoteSettings: any = {
                "newBusiness": this.configService.contextEntities?.newBusinessQuotes,
                "adjustment": this.configService.contextEntities?.adjustmentQuotes,
                "cancellation": this.configService.contextEntities?.cancellationQuotes,
                "renewal": this.configService.contextEntities?.renewalQuotes,
            };

            this.contextEntitySettings =
                quoteSettings[quoteType] ?? this.configService.contextEntities?.quotes;
        }

        return this.contextEntitySettings;
    }
}
