import { Validatable } from "./validatable";

export interface CalculationAffectingElement extends Validatable {
    affectsPremium: boolean;
    affectsTriggers: boolean;
}

export interface CalculationAffectingField extends CalculationAffectingElement {
    fieldPath: string;
}

export interface CalculationAffectingQuestionSet extends CalculationAffectingElement {
    questionSetPath: string;
}
