import { StringHelper } from "@app/helpers/string.helper";
import { QuestionMetadata } from "@app/models/question-metadata";
import { FieldFormatter } from "./field-formatter";

/**
 * If no field formatter has been defined for the give field type, this
 * one is used.
 */
export class DefaultFieldFormatter implements FieldFormatter {

    public format(value: string, metadata: QuestionMetadata = null): string {
        if (StringHelper.isNullOrWhitespace(value)) {
            return value == null ? null : '';
        }

        value = String(value);
        return value.trim();
    }
}
