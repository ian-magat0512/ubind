import { QuestionMetadata } from "@app/models/question-metadata";
import { FieldFormatter } from "./field-formatter";
import * as ChangeCase from 'change-case';
import { StringHelper } from "@app/helpers/string.helper";

/**
 * Formats boolean field values for presentation to end users.
 */
export class BooleanFieldFormatter implements FieldFormatter {

    public format(value: string, metadata: QuestionMetadata = null): string {
        if (StringHelper.isNullOrWhitespace(value)) {
            return value == null ? null : '';
        }

        value = String(value);
        value = value.toLowerCase().trim();
        return (value == "yes" || value == "no")
            ? ChangeCase.sentenceCase(value)
            : value == "true" ? "Yes" : "No";
    }
}
