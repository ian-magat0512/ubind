import { FieldGroup } from "./field-group";

/**
 * Holds working configuration about a group of question sets.
 */
export interface QuestionSets {
    [key: string]: Array<FieldGroup>;
}
