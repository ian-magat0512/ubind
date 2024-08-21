import { CalculationResult } from "./calculation-result";
import { ClaimState } from "./claim-state.enum";

/**
 * A calculation result for a Claim
 */
export interface ClaimResult extends CalculationResult {
    claimState: ClaimState;
}
