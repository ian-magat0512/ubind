import { StringHelper } from "@app/helpers/string.helper";
import { QuestionMetadata } from "@app/models/question-metadata";
import { TriggerDisplayConfig } from "@app/models/trigger-display-config";
import { ConfigService } from "@app/services/config.service";
import { TriggerProcessingService } from "@app/services/trigger-processing.service";
import { TriggerService } from "@app/services/trigger.service";
import { ApplicationStatus } from "../models/application-status.enum";
import { CalculationResult } from "../models/calculation-result";
import { TriggerState } from "../models/calculation-result-state";
import { SourceRatingSummaryItem } from "../models/source-rating-summary-item";

/**
 * Processes calculation results into a format that's ready for consumption 
 * by the web forms app, it's components and services
 */
export abstract class CalculationResultProcessor {

    protected constructor(
        private triggerProcessingService: TriggerProcessingService,
        private triggerService: TriggerService,
        private configService: ConfigService,
    ) {
    }

    protected process(response: any, result: CalculationResult): void {
        result.calculationState = response.calculationResult.state;
        this.initRisks(response, result);
        this.initTriggers(response, result);
        this.initPaymentInfo(response, result);
        this.initRatingSummaryItems(response, result);
        this.initDeprecatedState(response, result);
    }

    private initRisks(response: any, result: CalculationResult): void {
        const calculationResult: any = response.calculationResult;
        if (calculationResult['risks'] != null) {
            result.risks = calculationResult['risks'];
        } else {
            result.risks = new Array<object>();
            for (let i: number = 0; i < 30; i++) {
                if (calculationResult['risk' + i] != null) {
                    result.risks.push(calculationResult['risk' + i]);

                    // for backwards compatibility for some expressions which expect 
                    // it to be called "risk1" instead of "risk[1]"
                    result['risk' + i] = calculationResult['risk' + i];
                }
            }
        }
    }

    private initTriggers(response: any, result: CalculationResult): void {
        const calculationResult: any = response.calculationResult;
        result.triggers = calculationResult['triggers'] || {};
        this.triggerProcessingService.generateActiveTriggerDisplayConfigs(result.triggers);
        const trigger: TriggerDisplayConfig = this.triggerService.activeTrigger;
        if (trigger != null) {
            result.trigger = trigger;
            result.triggerState = <TriggerState>trigger.type;
        } else {
            result.triggerState = null;
        }
    }

    protected initPaymentInfo(response: any, result: CalculationResult): void {
        const calculationResult: any = response.calculationResult;
        result.payment = calculationResult.payment;
        result.amountPayable = response.amountPayable;

        // for backwards compatibility
        result['payable'] = response.amountPayable;
    }

    private initRatingSummaryItems(response: any, result: CalculationResult): void {
        const calculationResult: any = response.calculationResult;
        if (calculationResult.state && calculationResult.state != ApplicationStatus.Incomplete) {
            if (this.hasSummaryLabel(this.configService.questionMetadata)) {
                result.ratingSummaryItems = this.getSourceRatingSummaryItemsUsingQuestionMetadata(
                    this.configService.questionMetadata, calculationResult);
            } else if (this.configService.sidebarSummaryKeyMapping) {
                result.ratingSummaryItems = this.getSourceRatingSummaryItemsWithoutQuestionMetadata(calculationResult);
            }
        }
    }

    private initDeprecatedState(response: any, result: CalculationResult): void {
        const calculationResult: any = response.calculationResult;
        result.oldStateDeprecated = calculationResult.state;
        if (result.trigger != null) {
            result.oldStateDeprecated = result.trigger.type;
        }

        // for backwards compatibility with some expressions which try to inspect the "state" property
        result['state'] = result.oldStateDeprecated;
    }

    private hasSummaryLabel(questionMetadata: any): boolean {
        if (questionMetadata) {
            const questionSets: any = questionMetadata.questionSets;
            const questionSetKeys: Array<string> = Object.keys(questionSets);
            for (const questionSetKey of questionSetKeys) {
                const questionSet: any = questionSets[questionSetKey];
                const questionsKeys: Array<string> = Object.keys(questionSet);
                for (const questionKey of questionsKeys) {
                    const question: QuestionMetadata = questionSet[questionKey];
                    if (!StringHelper.isNullOrWhitespace(question.summaryLabel)
                        || !StringHelper.isNullOrWhitespace(question.summaryLabelExpression)
                    ) {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private getSourceRatingSummaryItemsWithoutQuestionMetadata(calculationResult: any): Array<SourceRatingSummaryItem> {
        const ratingSummaries: Array<SourceRatingSummaryItem> = [];
        const questions: any = {};
        const questionSetNames: Array<any> = Object.keys(calculationResult['questions']);
        for (const questionSetName of questionSetNames) {
            Object.assign(questions, calculationResult['questions'][questionSetName]);
        }
        const keys: Array<string> = Object.keys(this.configService.sidebarSummaryKeyMapping);
        for (const key of keys) {
            if (questions[key] != null && questions[key] != '') {
                const value: any = this.getCalculationLabelValue(questions, key);
                const ratingSummary: SourceRatingSummaryItem = new SourceRatingSummaryItem(
                    this.configService.sidebarSummaryKeyMapping[key], value);
                ratingSummaries.push(ratingSummary);

            }
        }
        return ratingSummaries;
    }

    private getSourceRatingSummaryItemsUsingQuestionMetadata(questionMetadata: any,
        calculationResult: any): Array<SourceRatingSummaryItem> {
        const ratingSummaries: Array<SourceRatingSummaryItem> = [];
        const questionsKey: string = 'questions';
        const questionSets: any = questionMetadata.questionSets;
        const questionSetKeys: Array<string> = Object.keys(questionSets);
        let defaultQuestionIndex: number = 0;
        for (const questionSetKey of questionSetKeys) {
            const questionSet: any = questionSets[questionSetKey];
            const questionsKeys: Array<string> = Object.keys(questionSet);
            for (const questionKey of questionsKeys) {
                const question: QuestionMetadata = questionSet[questionKey];
                let summaryLabel: string;
                if (question.summaryLabel || question.summaryLabelExpression) {
                    summaryLabel = question.summaryLabel || `%{ ${question.summaryLabelExpression} }%`;
                } else {
                    summaryLabel = this.getSummaryLabelFromMapping(calculationResult, questionKey);
                }
                if (summaryLabel) {
                    const summaryPositionExpression: string = question.summaryPositionExpression;
                    const calculationResultHasQuestionProperty: any = calculationResult[questionsKey] &&
                        calculationResult[questionsKey][questionSetKey] &&
                        calculationResult[questionsKey][questionSetKey][questionKey];
                    if (calculationResultHasQuestionProperty) {
                        const questionValue: any = this.getCalculationLabelExpressionValue(calculationResult,
                            questionSetKey, questionKey);
                        if (questionValue) {
                            const ratingSummary: SourceRatingSummaryItem = new SourceRatingSummaryItem(
                                summaryLabel,
                                questionValue,
                                defaultQuestionIndex,
                                summaryPositionExpression);
                            ratingSummaries.push(ratingSummary);
                        }
                    }
                }
                defaultQuestionIndex += 1;
            }
        }
        return ratingSummaries;
    }

    /**
     * @deprecated this is old and is only used for very old workbooks.
     * @param calculationResult 
     * @param property 
     * @returns 
     */
    private getSummaryLabelFromMapping(calculationResult: any, property: string): string {
        let defaultValue: string = "";
        if (this.configService.sidebarSummaryKeyMapping) {
            const keys: Array<string> = Object.keys(this.configService.sidebarSummaryKeyMapping);
            for (let key of keys) {
                if (key == property) {
                    return keys[key];
                }
            }
        }
        return defaultValue;
    }

    private getCalculationLabelExpressionValue(
        calculationResult: any,
        questionSetKey: any,
        questionKey: any): any {
        const questionsKey: string = 'questions';
        let value: any = calculationResult[questionsKey][questionSetKey][questionKey];
        if (this.hasZeroDecimalPoint(value)) {
            value = this.removeZeroDecimalPoint(value);
        }
        return value;
    }

    private getCalculationLabelValue(questions: any, key: string): any {
        let value: any = questions[key];
        if (this.hasZeroDecimalPoint(value)) {
            value = this.removeZeroDecimalPoint(value);
        }
        return value;
    }

    private hasZeroDecimalPoint(value: any): boolean {
        if (typeof value == 'string' &&
            value.length >= 5 &&
            value.substring(0, 1) == '$' &&
            value.substring(value.length - 3, value.length) == '.00' &&
            !isNaN(parseFloat(value.substring(1, value.length)))) {
            return true;
        }
        return false;
    }

    private removeZeroDecimalPoint(amountWithDecimalPoint: string): string {
        return amountWithDecimalPoint.substring(0, amountWithDecimalPoint.length - 3);
    }

}
