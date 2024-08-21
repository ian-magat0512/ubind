import { Injectable } from "@angular/core";
import { ConfigService } from "@app/services/config.service";
import { TriggerProcessingService } from "@app/services/trigger-processing.service";
import { TriggerService } from "@app/services/trigger.service";
import { CalculationResultProcessor } from "./calculation-result-processor";
import { CalculationState } from "../models/calculation-result-state";
import { ClaimResult } from "../models/claim-result";
import { ClaimState } from "../models/claim-state.enum";
import { SourceRatingSummaryItem } from "../models/source-rating-summary-item";
import { EventService } from "./event.service";
import { ApplicationService } from "./application.service";

/**
 * Processes claim calculation results into a format that's ready for consumption 
 * by the web forms app, it's components and services
 */
@Injectable({
    providedIn: 'root',
})
export class ClaimResultProcessor extends CalculationResultProcessor {

    public constructor(
        triggerProcessingService: TriggerProcessingService,
        triggerService: TriggerService,
        configService: ConfigService,
        private eventService: EventService,
        private applicationService: ApplicationService,
    ) {
        super(triggerProcessingService, triggerService, configService);
        this.listenForClaimResponses();
    }

    private listenForClaimResponses(): void {
        this.eventService.claimResponseSubject.subscribe((response: any) => {
            this.applicationService.latestClaimResult = this.process(response);
            this.applicationService.claimState = this.applicationService.latestClaimResult.claimState;
            this.eventService.claimResultSubject.next(this.applicationService.latestClaimResult);
            this.applicationService.calculationResultSubject.next(this.applicationService.latestClaimResult);
        });
    }


    public static createEmpty(): ClaimResult {
        return <any>{
            claimState: ClaimState.Incomplete,
            calculationState: CalculationState.Incomplete,
            ratingSummaryItems: new Array<SourceRatingSummaryItem>(),
        };
    }

    public process(response: any): ClaimResult {
        let result: ClaimResult = <any>{};
        result.claimState = response.claimState;
        super.process(response, result);
        return result;
    }
}
