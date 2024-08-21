import { QuestionSetConfiguration } from "./question-set.configuration";

/**
 * Represents the configuration for a repeating question set.
 */
export interface RepeatingQuestionSetConfiguration extends QuestionSetConfiguration {
    repeatingFieldKey: string;
}
