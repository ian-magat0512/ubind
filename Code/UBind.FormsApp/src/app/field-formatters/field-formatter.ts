import { QuestionMetadata } from "@app/models/question-metadata";

/**
 * Represents something that can format a field value based upon QuestionMetadata
 */
export interface FieldFormatter {
    format(value: string, metadata: QuestionMetadata): string;
}
