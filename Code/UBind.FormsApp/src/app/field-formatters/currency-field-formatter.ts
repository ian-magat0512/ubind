import { CurrencyHelper } from "@app/helpers/currency.helper";
import { StringHelper } from "@app/helpers/string.helper";
import { QuestionMetadata } from "@app/models/question-metadata";
import { LocaleService } from "@app/services/locale.service";
import { FieldFormatter } from "./field-formatter";

/**
 * Formats currency values, including putting the currency symbol or code, thousands separator and units separator
 */
export class CurrencyFieldFormatter implements FieldFormatter {

    public constructor(
        private localeService: LocaleService,
    ) { }

    public format(value: string, metadata: QuestionMetadata): string {
        if (StringHelper.isNullOrWhitespace(value)) {
            return value == null ? null : '';
        }
        value = String(value);
        value = value.replace(/[^0-9.]/g, '');
        value = value.trim().replace(",", '');
        const floatValue: number = parseFloat(value);

        let isWholeNumber: boolean = floatValue % 1 == 0;
        let digitsInfo: string = isWholeNumber ? '1.0-0' : '1.2-2';
        // TODO: get the currently chosen currency code, e.g. using quoteDataLocator
        const currencyCode: string = metadata?.currencyCode || this.localeService.getCurrencyCode();
        const locale: string = this.localeService.initialiseOrGetCurrencyLocale(currencyCode);

        return CurrencyHelper.format(floatValue, currencyCode, 'symbol-narrow', digitsInfo, locale);
    }
}
