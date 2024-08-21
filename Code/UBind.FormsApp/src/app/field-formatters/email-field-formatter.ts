import { StringHelper } from "@app/helpers/string.helper";
import { QuestionMetadata } from "@app/models/question-metadata";
import { FieldFormatter } from "./field-formatter";

/**
 * Formats email addresses for presentation to end users.
 */
export class EmailFieldFormatter implements FieldFormatter {

    public format(value: string, metadata: QuestionMetadata = null): string {
        if (StringHelper.isNullOrWhitespace(value)) {
            return value == null ? null : '';
        }

        value = String(value);
        return value.trim().toLocaleLowerCase();
    }
}
