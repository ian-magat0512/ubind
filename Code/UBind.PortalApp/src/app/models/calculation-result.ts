/**
 * model for representing a calculation result
 */
export interface CalculationResult {
    calculationResultId: string;
    formDataId: string;
    calculationResultJson: string;
    triggerTypeWhichRequiresPriceToBeHidden: string;
}
