import { FieldConfiguration } from "./fields/field.configuration";

/**
 * Represents the configuration for a QuestionSet
 */
export interface QuestionSetConfiguration {
    name: string;
    key: string;
    fields: Array<FieldConfiguration>;
}
