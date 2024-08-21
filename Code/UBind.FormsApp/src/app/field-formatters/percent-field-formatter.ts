import { NumberHelper } from "@app/helpers/number.helper";
import { StringHelper } from "@app/helpers/string.helper";
import { QuestionMetadata } from "@app/models/question-metadata";
import { LocaleService } from "@app/services/locale.service";
import { FieldFormatter } from "./field-formatter";

/**
 * Formats percent fields
 */
export class PercentFieldFormatter implements FieldFormatter {

    public constructor(
        private localeService: LocaleService,
    ) { }

    public format(value: string, metadata: QuestionMetadata = null): string {
        if (StringHelper.isNullOrWhitespace(value)) {
            return value == null ? null : '';
        }

        value = String(value);
        value = value.trim().replace(",", '').replace("%", '');
        let numberValue: number = parseFloat(value);
        let formattedNumberValue: string =
            NumberHelper.format(numberValue, null, this.localeService.getCurrencyLocale());
        return `${formattedNumberValue}%`;
    }
}
