import { Injectable } from "@angular/core";
import { ConfigService } from "@app/services/config.service";
import { TriggerProcessingService } from "@app/services/trigger-processing.service";
import { TriggerService } from "@app/services/trigger.service";
import { CalculationResultProcessor } from "./calculation-result-processor";
import { CalculationState } from "../models/calculation-result-state";
import { QuoteResult } from "../models/quote-result";
import { QuoteState } from "../models/quote-state.enum";
import { SourceRatingSummaryItem } from "../models/source-rating-summary-item";
import { CalculationResult } from "@app/models/calculation-result";
import { EventService } from "./event.service";
import { ApplicationService } from "./application.service";

/**
 * Processes quote calculation results into a format that's ready for consumption 
 * by the web forms app, it's components and services
 */
@Injectable({
    providedIn: 'root',
})
export class QuoteResultProcessor extends CalculationResultProcessor {

    public constructor(
        triggerProcessingService: TriggerProcessingService,
        triggerService: TriggerService,
        configService: ConfigService,
        private eventService: EventService,
        private applicationService: ApplicationService,
    ) {
        super(triggerProcessingService, triggerService, configService);
        this.listenForQuoteResponses();
    }

    private listenForQuoteResponses(): void {
        this.eventService.quoteResponseSubject.subscribe((response: any) => {
            this.applicationService.latestQuoteResult = this.process(response);
            this.applicationService.quoteState = this.applicationService.latestQuoteResult.quoteState;
            this.eventService.quoteResultSubject.next(this.applicationService.latestQuoteResult);
            this.applicationService.calculationResultSubject.next(this.applicationService.latestQuoteResult);
        });
    }

    public static createEmpty(): QuoteResult {
        return <any>{
            quoteState: QuoteState.Incomplete,
            calculationState: CalculationState.Incomplete,
            ratingSummaryItems: new Array<SourceRatingSummaryItem>(),

            // For backwards compatibility on Strategic Pilots and Leasebond
            // TODO: Remove this when strategic pilots and leasebond are fixed.
            risk1: {
                calculation: {},
                other: {},
                // eslint-disable-next-line @typescript-eslint/naming-convention
                XMLFields: {}, // PSC Trades
            },
            risk2: {
                calculation: {},
            },
        };
    }

    public process(response: any): QuoteResult {
        let result: QuoteResult = <any>{};
        result.quoteState = response.quoteState;
        super.process(response, result);
        return result;
    }

    protected initPaymentInfo(response: any, result: CalculationResult): void {
        super.initPaymentInfo(response, result);
        if (!result.payment) {
            result.payment = {};
        }
        result.payment['amountPayable'] = response.amountPayable;
        result.payment['refundComponents'] = response.refundBreakdown;
        result.payment['payableComponents'] = response.priceBreakdown;
        if (response.premiumFundingProposal) {
            result.payment['premiumFundingProposal'] = response.premiumFundingProposal;
        }
        result.funding = response.premiumFundingProposal || result.payment['premiumFundingProposal'];
    }
}
