import { Injectable } from "@angular/core";
import { FieldDataType } from "@app/models/field-data-type.enum";
import { ApplicationService } from "@app/services/application.service";
import { LocaleService } from "@app/services/locale.service";
import { AbnFieldFormatter } from "./abn-field-formatter";
import { AttachmentFieldFormatter } from "./attachment-field-formatter";
import { BooleanFieldFormatter } from "./boolean-field-formatter";
import { CurrencyFieldFormatter } from "./currency-field-formatter";
import { DefaultFieldFormatter } from "./default-field-formatter";
import { EmailFieldFormatter } from "./email-field-formatter";
import { FieldFormatter } from "./field-formatter";
import { NumberFieldFormatter } from "./number-field-formatter";
import { PercentFieldFormatter } from "./percent-field-formatter";
import { PhoneFieldFormatter } from "./phone-field-formatter";

/**
 * Resolves the correct field formatter for the given field data type
 */
@Injectable({
    providedIn: 'root',
})
export class FieldFormatterResolver {

    private fieldFormattersMap: Map<string, FieldFormatter> = new Map<string, FieldFormatter>();
    private defaultFieldFormatter: DefaultFieldFormatter = new DefaultFieldFormatter();

    public constructor(
        applicationService: ApplicationService,
        localeService: LocaleService,
    ) {
        this.fieldFormattersMap.set(FieldDataType.Abn, new AbnFieldFormatter());
        this.fieldFormattersMap.set(FieldDataType.Boolean, new BooleanFieldFormatter());
        this.fieldFormattersMap.set(FieldDataType.Currency, new CurrencyFieldFormatter(localeService));
        this.fieldFormattersMap.set(FieldDataType.Email, new EmailFieldFormatter());
        this.fieldFormattersMap.set(FieldDataType.Number, new NumberFieldFormatter(localeService));
        this.fieldFormattersMap.set(FieldDataType.Percent, new PercentFieldFormatter(localeService));
        this.fieldFormattersMap.set(FieldDataType.Phone, new PhoneFieldFormatter());
        this.fieldFormattersMap.set(FieldDataType.Attachment, new AttachmentFieldFormatter(applicationService));
    }

    /**
     * Returns the field formatter, or null if not found.
     */
    public resolve(dataType: string): FieldFormatter {
        let formatter: FieldFormatter = this.fieldFormattersMap.get(dataType.toLowerCase());
        return formatter ? formatter : this.defaultFieldFormatter;
    }
}
