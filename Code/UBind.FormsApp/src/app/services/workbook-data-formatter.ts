import { Injectable } from "@angular/core";
import { FieldConfigurationHelper } from "@app/helpers/field-configuration.helper";
import { FieldDataType } from "@app/models/field-data-type.enum";
import { Errors } from "@app/models/errors";
import { FieldConfiguration } from "@app/resource-models/configuration/fields/field.configuration";
import * as _ from 'lodash-es';
import { ConfigService } from "./config.service";
import { AnyHelper } from "@app/helpers/any.helper";
import { Field } from "@app/models/configuration/field";

/**
 * Formats data for inputting into the workbook for calculations
 */
@Injectable({
    providedIn: 'root',
})
export class WorkbookDataFormatter {
    private standardWorkbookHeadingRowIndex: number = 4;

    public constructor(
        private configService: ConfigService,
    ) {
    }

    public generateQuestionAnswers(formModel: any): Array<Array<string>> {
        if (this.configService.configuration.version.startsWith('2')) {
            return this.generateQuestionAnswersFromOrderedFields(formModel);
        } else {
            throw Errors.General.Unexpected(
                `Unsupported configuration version "${this.configService.configuration.version}".`);
        }
    }

    public generateRepeatingQuestionAnswers(formModel: any): Array<Array<string>> {
        if (this.configService.configuration.version.startsWith('2')) {
            return this.generateRepeatingQuestionAnswersFromOrderedFields(formModel);
        } else {
            throw Errors.General.Unexpected(
                `Unsupported configuration version "${this.configService.configuration.version}".`);
        }
    }

    public tabulate(table: Array<Array<string>>): string {
        let output: string = '';
        table.forEach((row: Array<string>) => {
            output += row.join('\t') + '\n';
        });
        return output;
    }

    private generateQuestionAnswersFromOrderedFields(formModel: any): Array<Array<string>> {
        const orderedFields: Array<FieldConfiguration> =
            this.configService.configuration.fieldsOrderedByCalculationWorkbookRow;
        const table: Array<Array<string>> = new Array<Array<string>>();
        let row: Array<string> = new Array<string>('Value');
        table[0] = row;
        for (let field of orderedFields) {
            if (FieldConfigurationHelper.isDataStoringField(field)) {
                let fieldRowIndex: number = field.calculationWorkbookCellLocation.rowIndex;
                let fieldValue: any = formModel[field.key];
                let anyFieldValue: any =AnyHelper.hasValue(fieldValue) && _.isArray(fieldValue) ? '' : fieldValue ?? '';
                anyFieldValue = AnyHelper.hasValue(anyFieldValue) ? anyFieldValue : this.getDefaultValue(field);
                row = new Array<string>(anyFieldValue.toString());
                table[fieldRowIndex - this.standardWorkbookHeadingRowIndex] = row;
            }
            if (FieldConfigurationHelper.isRepeatingField(field)) {
                if(!field.calculationWorkbookCellLocation) {
                    continue;
                }
                let fieldRowIndex: number = field.calculationWorkbookCellLocation.rowIndex;
                let repeatingQuestionSetFieldValue: any =
                    formModel[field.key]?.length ? formModel[field.key] : undefined;
                let repeatingQuestionSetFields: Array<Field> =
                    this.configService.configuration.repeatingQuestionSets[field.key][0].fieldGroup;
                let repeatingQuestionSetIsComplete: boolean =
                    this.repeatingQuestionSetIsComplete(
                        repeatingQuestionSetFieldValue, repeatingQuestionSetFields);
                let anyFieldValue: any =
                    !AnyHelper.hasValue(repeatingQuestionSetFieldValue)
                        || (AnyHelper.hasValue(repeatingQuestionSetFieldValue) && repeatingQuestionSetIsComplete)
                        ? 'complete' : 'incomplete';
                anyFieldValue = AnyHelper.hasValue(anyFieldValue) ? anyFieldValue : this.getDefaultValue(field);
                row = new Array<string>(anyFieldValue.toString());
                table[fieldRowIndex - this.standardWorkbookHeadingRowIndex] = row;
            }
        }
        for (let i: number = 0; i < table.length; i++) {
            if (!table[i]) {
                table[i] = new Array<string>('');
            }
        }
        return table;
    }

    private generateRepeatingQuestionAnswersFromOrderedFields(formModel: any): Array<Array<string>> {
        let repeatingInstanceMaxQuantity: number = this.configService.configuration.repeatingInstanceMaxQuantity;
        const orderedFields: Array<FieldConfiguration> =
            this.configService.configuration.repeatingFieldsOrderedByCalculationWorkbookRow;
        const table: Array<Array<string>> = new Array<Array<string>>();
        let rowIndex: number = this.standardWorkbookHeadingRowIndex;
        let row: Array<string> = new Array<string>();
        table.push(row);
        rowIndex++;
        for (let field of orderedFields) {
            if (FieldConfigurationHelper.isDataStoringField(field)) {
                let fieldRowIndex: number = field.calculationWorkbookCellLocation.rowIndex;
                let repeatingQuestionSetKey: string = field.questionSetKey;
                while (rowIndex < fieldRowIndex) {
                    row = new Array<string>(this.getDefaultValue(field).toString());
                    table.push(row);
                    rowIndex++;
                }
                row = new Array<string>();
                if (formModel[repeatingQuestionSetKey] != null &&
                    Object.prototype.toString.call(formModel[repeatingQuestionSetKey]) === '[object Array]') {
                    if (formModel[repeatingQuestionSetKey].length > repeatingInstanceMaxQuantity) {
                        throw Errors.Product.Configuration(
                            `The number of repeating instances for "${repeatingQuestionSetKey}" is ` +
                            `${formModel[repeatingQuestionSetKey].length} which is greater than ` +
                            `${repeatingInstanceMaxQuantity}. Please ensure that the "Maximum Quantity Expression" ` +
                            `does not exceed ${repeatingInstanceMaxQuantity} or update the ` +
                            `"Repeating Question Sets" configuration to handle ` +
                            `${formModel[repeatingQuestionSetKey].length} repeating instances.`);
                    }
                    for (const repeatingQuestionSetFields of formModel[repeatingQuestionSetKey]) {
                        if (!repeatingQuestionSetFields) {
                            continue;
                        }
                        if (repeatingQuestionSetFields[field.key] != null) {
                            row.push(repeatingQuestionSetFields[field.key]);
                        } else {
                            row.push(this.getDefaultValue(field).toString());
                        }
                    }
                }
                table.push(row);
                rowIndex++;
            }
        }
        if (this.configService.configuration.calculatesUsingStandardWorkbook) {
            let firstRow: Array<string> = table[0];
            for (let i: number = 0; i < repeatingInstanceMaxQuantity; i++) {
                firstRow[i] = 'Value ' + (i + 1);
            }
        }
        for (let row of table) {
            while (row.length < repeatingInstanceMaxQuantity) {
                row.push('');
            }
        }
        return table;
    }

    private getDefaultValue(field: FieldConfiguration): any {
        if (field.dataType == FieldDataType.Boolean) {
            return false;
        }
        return '';
    }

    private getFirstDataRowIndex(orderedFields: Array<FieldConfiguration>): number {
        for (let field of orderedFields) {
            if (FieldConfigurationHelper.isDataStoringField(field)) {
                return field.calculationWorkbookCellLocation.rowIndex;
            }
        }
    }

    private repeatingQuestionSetIsComplete(
        repeatingQuestionSetFieldValue: Array<any>, repeatingQuestionSetFields: Array<Field>) {
        let repeatingFieldIsComplete: boolean = true;
        if(!_.isArray(repeatingQuestionSetFieldValue)) {
            return repeatingFieldIsComplete;
        }
        for (const repeatingFieldArrayItem of repeatingQuestionSetFieldValue) {
            if(Object.keys(repeatingFieldArrayItem).length === 0) {
                continue;
            }
            for (const repeatingQuestionField of repeatingQuestionSetFields) {
                if (FieldConfigurationHelper.isInteractiveField(
                    repeatingQuestionField.templateOptions.fieldConfiguration)) {
                    let isRequired: boolean =
                        repeatingQuestionField.templateOptions.fieldConfiguration.required;
                    if (isRequired && !repeatingFieldArrayItem[repeatingQuestionField.key]) {
                        return false;
                    }
                }
            }
        }
        // if there are no repeating instances then we'll consider it complete.
        return true;
    }
}
