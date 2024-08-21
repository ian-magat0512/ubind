import { Component, OnInit } from "@angular/core";
import { FormService } from '@app/services/form.service';
import { WorkflowService } from '@app/services/workflow.service';
import { ValidationService } from '@app/services/validation.service';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { CalculationService } from '@app/services/calculation.service';
import { ApplicationService } from '@app/services/application.service';
import { FieldMetadataService } from '@app/services/field-metadata.service';
import { CurrencyHelper } from "@app/helpers/currency.helper";
import { StringHelper } from "@app/helpers/string.helper";
import { FieldType } from "@app/models/field-type.enum";
import { FieldSelector } from "@app/models/field-selectors.enum";
import { CurrencyFieldConfiguration } from "@app/resource-models/configuration/fields/currency-field.configuration";
import { TextInputFormat } from "@app/models/text-input-format.enum";
import { EventService } from "@app/services/event.service";
import { FieldEventLogRegistry } from "@app/services/debug/field-event-log-registry";
import { InputFieldWithMasking } from "../input-field-with-masking";
import { LocaleService } from "@app/services/locale.service";
import { MaskPipe } from "ngx-mask";

/**
 * A field for inputting currency values.
 */
@Component({
    selector: '' + FieldSelector.Currency,
    templateUrl: './currency.field.html',
})
export class CurrencyField extends InputFieldWithMasking implements OnInit {

    private currencyFieldConfiguration: CurrencyFieldConfiguration;
    private localeService: LocaleService;
    public textInputFormat: TextInputFormat;

    public constructor(
        protected formService: FormService,
        protected workflowService: WorkflowService,
        protected validationService: ValidationService,
        protected expressionDependencies: ExpressionDependencies,
        calculationService: CalculationService,
        applicationService: ApplicationService,
        fieldMetadataService: FieldMetadataService,
        eventService: EventService,
        fieldEventLogRegistry: FieldEventLogRegistry,
        localeService: LocaleService,
        protected myMaskPipe: MaskPipe,
    ) {
        super(
            formService,
            workflowService,
            expressionDependencies,
            calculationService,
            applicationService,
            fieldMetadataService,
            eventService,
            fieldEventLogRegistry,
            myMaskPipe);
        this.fieldType = FieldType.Currency;
        this.localeService = localeService;
    }

    public ngOnInit(): void {
        this.currencyFieldConfiguration = this.field.templateOptions.fieldConfiguration;
        this.textInputFormat = this.currencyFieldConfiguration.textInputFormat;
        super.ngOnInit();
    }

    protected getValueForExpressions(): any {
        if (this.isHidden() || StringHelper.isNullOrEmpty(this.formControl.value)) {
            return '';
        }

        let currencyCode: string = this.dataStoringFieldConfiguration.currencyCode
            || this.expressionDependencies.expressionMethodService.getCurrencyCode();
        const locale: string = this.localeService.initialiseOrGetCurrencyLocale(currencyCode);

        return locale ? this.getNumericValue(locale, currencyCode) : this.formControl.value;
    }

    private getNumericValue(locale: string, currencyCode: string): any {
        let cleanedValue: string =
            CurrencyHelper.removeThousandsSeparator(this.formControl.value, currencyCode, locale);

        if (isNaN(<any>cleanedValue)) {
            // it's invalid, however we don't want to change the value, because we need validation expressions to work
            return this.formControl.value;
        }

        let numericValue: number = CurrencyHelper.parse(cleanedValue);
        if (numericValue != this.formControl.value || typeof this.formControl.value != 'number') {
            this.formControl.setValue(
                numericValue,
                { onlySelf: true, emitEvent: true, emitModelToViewChange: false, emitViewToModelChange: false });
        }

        return numericValue;
    }
}
