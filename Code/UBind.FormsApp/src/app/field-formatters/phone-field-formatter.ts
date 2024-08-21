import { QuestionMetadata } from "@app/models/question-metadata";
import { FieldFormatter } from "./field-formatter";
import parsePhoneNumber, { PhoneNumber } from 'libphonenumber-js';
import { StringHelper } from "@app/helpers/string.helper";

/**
 * Formats phone number fields
 */
export class PhoneFieldFormatter implements FieldFormatter {

    public format(value: string, metadata: QuestionMetadata = null): string {
        if (StringHelper.isNullOrWhitespace(value)) {
            return value == null ? null : '';
        }
        value = String(value);
        const phoneNumber: PhoneNumber = parsePhoneNumber(value, "AU");
        if (!phoneNumber) {
            return value;
        }
        let result: string = '';
        result = value.startsWith('+')
            ? phoneNumber.formatInternational()
            : phoneNumber.formatNational();
        if (result.length == 6 && result.indexOf(' ') == -1) {
            result = result.substring(0, 2) + ' '
                + result.substring(2, 4) + ' '
                + result.substring(4);
        }
        return result;
    }
}
