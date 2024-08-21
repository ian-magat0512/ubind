import { StringHelper } from "@app/helpers/string.helper";
import { QuestionMetadata } from "@app/models/question-metadata";
import { FieldFormatter } from "./field-formatter";

/**
 * Formats ABN (Australian Business Number) fields by inserting spaces in the correct places
 */
export class AbnFieldFormatter implements FieldFormatter {

    public format(value: string, metadata: QuestionMetadata = null): string {
        if (StringHelper.isNullOrWhitespace(value)) {
            return value == null ? null : '';
        }

        // remove non digits
        value = String(value);
        value = value.replace(/[^\d]?/g, '');

        // add spaces
        return value.replace(/\B(?=(\d{3})+(?!\d))/g, " ");
    }
}
