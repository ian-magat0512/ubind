import { WorkflowStep } from "@app/models/configuration/workflow-step";
import { OptionSetConfiguration } from "./option-set.configuration";
import { QuestionSetConfiguration } from "./question-set.configuration";
import { RepeatingQuestionSetConfiguration } from "./repeating-question-set.configuration";
import { TextElementConfiguration } from "./text-element.configuration";
import { ThemeConfiguration } from "./theme.configuration";

/**
 * Represents the configuration for a form
 */
export interface FormConfiguration {
    defaultCurrencyCode: string;
    questionSets: Array<QuestionSetConfiguration>;
    repeatingQuestionSets: Array<RepeatingQuestionSetConfiguration>;
    optionSets: Array<OptionSetConfiguration>;
    textElements: Array<TextElementConfiguration>;
    theme: ThemeConfiguration;
    workflowConfiguration: { [key: string]: WorkflowStep };
    formModel: object;
    repeatingInstanceMaxQuantity: number;
}
