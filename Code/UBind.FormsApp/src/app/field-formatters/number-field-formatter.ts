import { NumberHelper } from "@app/helpers/number.helper";
import { StringHelper } from "@app/helpers/string.helper";
import { QuestionMetadata } from "@app/models/question-metadata";
import { LocaleService } from "@app/services/locale.service";
import { FieldFormatter } from "./field-formatter";

/**
 * Formats number field values for presenting to end users
 */
export class NumberFieldFormatter implements FieldFormatter {

    public constructor(
        private localeService: LocaleService,
    ) { }

    public format(value: string, metadata: QuestionMetadata = null): string {
        if (StringHelper.isNullOrWhitespace(value)) {
            return value == null ? null : '';
        }

        value = String(value);
        value = value.trim().replace(",", '');
        const floatValue: number = parseFloat(value);
        if (isNaN(floatValue)) {
            return value;
        }

        return NumberHelper.format(floatValue, null, this.localeService.getCurrencyLocale());
    }
}
