import { CalculationResult } from "./calculation-result";
import { QuoteState } from "./quote-state.enum";

/**
 * Calculation result for a Quote.
 */
export interface QuoteResult extends CalculationResult {
    quoteState: QuoteState;
}
