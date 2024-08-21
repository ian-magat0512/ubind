import { Component, Input, OnInit } from "@angular/core";
import { ExpressionDependencies } from "@app/expressions/expression-dependencies";
import { CurrencyHelper } from "@app/helpers/currency.helper";
import { ApplicationMode } from "@app/models/application-mode.enum";
import { CalculationResult } from "@app/models/calculation-result";
import { CalculationState } from "@app/models/calculation-result-state";
import { ClaimResult } from "@app/models/claim-result";
import { FormType } from "@app/models/form-type.enum";
import { QuoteResult } from "@app/models/quote-result";
import { QuoteState } from "@app/models/quote-state.enum";
import { QuoteType } from "@app/models/quote-type.enum";
import { ApplicationService } from "@app/services/application.service";
import { ClaimResultProcessor } from "@app/services/claim-result-processor";
import { EventService } from "@app/services/event.service";
import { LocaleService } from "@app/services/locale.service";
import { QuoteResultProcessor } from "@app/services/quote-result-processor";
import { filter, takeUntil } from "rxjs/operators";
import { Widget } from "../widget";

export enum ShowSign {

    /**
     * Never show the sign
     */
    Never = 'never',

    /**
     * Always show the sign, unless the amount is zero
     * */
    Always = 'always',

    /**
     * Only shows the sign when it's a refund
     */
    Negative = 'negative',

    /**
     * Only shows the sign when it's a positive payable amount
     */
    Positive = 'positive'
}

/**
 * Shows the price (payable or refund amount)
 */
@Component({
    selector: 'ubind-price-widget-ng',
    templateUrl: './price.widget.html',
    styleUrls: ['./price.widget.scss'],
})
export class PriceWidget extends Widget implements OnInit {

    /**
     * Shows a plus sign for a positive price, and a minus sign for a negative price
     */
    @Input('show-sign')
    public showSign: ShowSign = ShowSign.Negative;

    // eslint-disable-next-line @typescript-eslint/naming-convention
    public ShowSign: typeof ShowSign = ShowSign;

    public loading: boolean = false;
    public displayPrice: boolean = false;
    public displayablePriceAmount: number;
    public priceMajorUnits: string;
    public priceMinorUnits: string;
    public initialInstalmentAmount: string = null;
    public numberOfInstalments: string = null;

    public constructor(
        private applicationService: ApplicationService,
        private expressionDependencies: ExpressionDependencies,
        private eventService: EventService,
        private localeService: LocaleService,
    ) {
        super();
    }

    public ngOnInit(): void {
        this.applicationService.calculationInProgressSubject.pipe(
            takeUntil(this.destroyed))
            .subscribe((inProgress: boolean) => {
                this.onCalculation(inProgress);
            });

        this.eventService.quoteResultSubject
            .pipe(
                filter((qs: QuoteResult) => qs != null),
                takeUntil(this.destroyed),
            )
            .subscribe((quoteResult: QuoteResult) => {
                this.onCalculationResult(quoteResult);
            });
        this.eventService.claimResultSubject
            .pipe(
                filter((qs: ClaimResult) => qs != null),
                takeUntil(this.destroyed),
            )
            .subscribe((claimResult: ClaimResult) => {
                this.onCalculationResult(claimResult);
            });

        let initialCalculationResult: CalculationResult;
        if (this.applicationService.formType == FormType.Claim) {
            initialCalculationResult = this.applicationService.latestClaimResult
                ? this.applicationService.latestClaimResult
                : ClaimResultProcessor.createEmpty();
        } else if (this.applicationService.formType == FormType.Quote) {
            initialCalculationResult = this.applicationService.latestQuoteResult
                ? this.applicationService.latestQuoteResult
                : QuoteResultProcessor.createEmpty();
        } else {
            throw new Error("Unexpected: could not determine the form type - whether it's for a quote or a claim.");
        }
        this.onCalculationResult(initialCalculationResult);
    }

    protected onCalculation(inProgress: boolean): void {
        this.loading = inProgress;
    }

    public async onCalculationResult(calculationResult: CalculationResult): Promise<void> {
        await this.onCalculationResultUpdatePrice(calculationResult);
    }

    private async onCalculationResultUpdatePrice(calculationResult: CalculationResult): Promise<void> {
        this.displayPrice = false;
        if (calculationResult.calculationState == CalculationState.Incomplete) {
            // do not display the price since the calculation state is incomplete
            return;
        }
        let triggerSaysNotToDisplayPrice: boolean = calculationResult.trigger != null &&
            calculationResult.trigger.displayPrice == false;
        let thereIsNoPaymentInformation: boolean = calculationResult.payment == null;
        let isReviewOrEndorseMode: boolean = this.applicationService.mode == ApplicationMode.Review ||
            this.applicationService.mode == ApplicationMode.Endorse;
        let isQuoteApproved: boolean =
            this.isQuoteResult(calculationResult) && calculationResult.quoteState == QuoteState.Approved;
        if ((!isReviewOrEndorseMode && !isQuoteApproved && triggerSaysNotToDisplayPrice)
            || thereIsNoPaymentInformation
            // TODO: work out if a price can be displayed for a claim. 
            // For now claims does not display a monetary amount.
            || this.applicationService.formType == FormType.Claim
        ) {
            return;
        }

        this.initialInstalmentAmount = null;
        let payment: any = calculationResult.payment;
        let funding: any = calculationResult.funding;
        let regularInstalmentAmountString: string;
        let initialInstalmentAmountString: string;
        // we don't currently support instalments for adjustments or cancellations. 
        // The calculation result shouldn't actually be sending it 
        // through, but if it is we are going to ignore it anyway.
        let isAdjustmentOrCancellation: boolean =
            this.applicationService.quoteType == QuoteType.Adjustment ||
            this.applicationService.quoteType == QuoteType.Cancellation;

        this.displayablePriceAmount = null;
        if (!isAdjustmentOrCancellation && payment && payment.instalments &&
            payment.instalments.instalmentsPerYear > 1 && calculationResult.funding != null) {
            regularInstalmentAmountString = '' + funding.regularInstalmentAmount || funding.regularInstalmentAmount;
            initialInstalmentAmountString = '' + funding.initialInstalmentAmount || funding.intialInstalmentAmount;
            this.numberOfInstalments = funding.numberOfInstalments;
            this.displayablePriceAmount =
                this.expressionDependencies.expressionMethodService.currencyAsNumber(regularInstalmentAmountString);
            if (regularInstalmentAmountString != initialInstalmentAmountString) {
                let firstAmount: number =
                    this.expressionDependencies.expressionMethodService.currencyAsNumber(
                        initialInstalmentAmountString);
                this.initialInstalmentAmount =
                    this.expressionDependencies.expressionMethodService.currencyAsString(firstAmount,
                        true, null);
            }
        } else if (!isAdjustmentOrCancellation && payment.instalments &&
            payment.instalments.instalmentAmount) {
            this.displayablePriceAmount =
                this.expressionDependencies.expressionMethodService.currencyAsNumber(
                    payment.instalments.instalmentAmount);
            this.numberOfInstalments = null;
        } else {
            this.displayablePriceAmount =
                this.expressionDependencies.expressionMethodService.currencyAsNumber(
                    calculationResult.amountPayable);
            this.numberOfInstalments = null;
        }

        if (this.displayablePriceAmount != null) {
            // we never show a negative amount, we just change the price label to "refund" if it's negative.
            let displayablePrice: number = this.displayablePriceAmount;
            if (this.displayablePriceAmount < 0) {
                displayablePrice = displayablePrice * -1;
            }
            let currencyCode: string = this.expressionDependencies.expressionMethodService.getCurrencyCode();

            await this.localeService.initialiseOrGetCurrencyLocaleAsync(currencyCode).then((locale: string) => {
                this.setPriceUnits(displayablePrice, locale, currencyCode);
                this.loading = false;
                this.displayPrice = true;
            });
        } else {
            this.loading = false;
        }
    }

    private setPriceUnits(displayablePrice: number, locale: string, currencyCode: string): void {
        this.priceMajorUnits =
            CurrencyHelper.getMajorUnitsFormatted(displayablePrice, currencyCode, locale);
        this.priceMinorUnits = '';
        if (displayablePrice < 100000) {
            let unitsSeparator: string = CurrencyHelper.getUnitsSeparator(currencyCode, locale);
            this.priceMinorUnits = unitsSeparator + CurrencyHelper.getMinorUnitsFormatted(displayablePrice, true);
        }
    }

    private isQuoteResult(calculationResult: any): calculationResult is QuoteResult {
        return calculationResult.quoteState != undefined;
    }
}
