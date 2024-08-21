import { SummaryTableEntry } from "./summary-table-entry";
import { SummaryTableRow } from "./summary-table-row";
import * as ChangeCase from "change-case";
import { SummaryTableCell } from "./summary-table-cell";
import { MatchingFieldsSubjectService } from "@app/expressions/matching-fields-subject.service";
import { ExpressionInputSubjectService } from "@app/expressions/expression-input-subject.service";
import { FieldMetadataService } from "@app/services/field-metadata.service";
import { FieldFormatterResolver } from "@app/field-formatters/field-formatter-resolver";
import * as _ from 'lodash-es';
import { StringHelper } from "@app/helpers/string.helper";
import { FieldPathHelper } from "@app/helpers/field-path.helper";
import { QuestionMetadata } from "@app/models/question-metadata";
import { TextCase } from "@app/models/text-case.enum";

/**
 * Renders a repeating question set as an entry in a larger Summary Table
 */
export class SummaryTableRepeatingEntry implements SummaryTableEntry {

    /**
     * @param repeatingFieldPath the fieldPath for the repeating field
     */
    public constructor(
        protected repeatingFieldPath: string,
        protected matchingFieldsSubjectService: MatchingFieldsSubjectService,
        protected expressionInputSubjectService: ExpressionInputSubjectService,
        protected fieldMetadataService: FieldMetadataService,
        protected fieldFormatterResolver: FieldFormatterResolver,
        protected skipRowWhenCellEmpty: boolean = true,
        protected emptyCellContent: string = "-",
        protected nameCase?: TextCase,
    ) {
    }

    public toHtml(): string {
        let rows: Array<SummaryTableRow> = this.generateRows();
        let html: string = '';
        for (let row of rows) {
            html += row.toHtml();
        }
        return html;
    }

    private generateRows(): Array<SummaryTableRow> {
        let rows: Array<SummaryTableRow> = new Array<SummaryTableRow>();
        let fieldPaths: Array<string>
            = this.matchingFieldsSubjectService.getFieldPathsMatchingPattern(this.repeatingFieldPath);
        fieldPaths = _.difference(fieldPaths, [this.repeatingFieldPath]);
        let repeatingIndex: number = null;
        let isANewRepeatingInstance: boolean = false;
        let lastReapeatingInstanceRow: SummaryTableRow = null;
        for (let fieldPath of fieldPaths) {
            if (!this.fieldMetadataService.isFieldDisplayable(fieldPath)) {
                continue;
            }

            let isRepeatingInstance: boolean = fieldPath.endsWith(']');
            let metadata: QuestionMetadata = null;
            if (!isRepeatingInstance) {
                metadata = this.fieldMetadataService.getMetadataForField(fieldPath);
            }
            let cssClasses: Set<string> = new Set<string>(['summary-repeating-item']);
            let thisRepeatingIndex: number = FieldPathHelper.getRepeatingIndex(fieldPath);
            isANewRepeatingInstance = thisRepeatingIndex != repeatingIndex;
            if (isANewRepeatingInstance) {
                if (lastReapeatingInstanceRow) {
                    lastReapeatingInstanceRow.cssClasses.add('summary-repeating-item-last-row');
                }
                repeatingIndex = thisRepeatingIndex;
                let repeatingFieldPathBase: string = this.repeatingFieldPath.replace(/\[\d*\]/, '');
                let repeatingFieldHeading: string = ChangeCase.capitalCase(repeatingFieldPathBase);
                repeatingFieldHeading += ` ${repeatingIndex + 1}`;
                rows.push(
                    new SummaryTableRow(
                        [new SummaryTableCell(repeatingFieldHeading, null, 2)],
                        new Set<string>(['summary-repeating-heading'])));
                cssClasses.add('summary-repeating-item-first-row');
            }
            let value: string = this.getFieldValueFormatted(fieldPath);
            if (!this.skipRowWhenCellEmpty || !StringHelper.isNullOrWhitespace(value)) {
                if (StringHelper.isNullOrWhitespace(value) && !StringHelper.isNullOrEmpty(this.emptyCellContent)) {
                    value = this.emptyCellContent;
                }
                let fieldKey: string = FieldPathHelper.getFieldKey(fieldPath);
                let name: string = metadata
                    ? (metadata.name ? metadata.name : fieldKey)
                    : fieldKey;
                name = this.nameCase ? StringHelper.toCase(name, this.nameCase) : name;
                let nameCell: SummaryTableCell = new SummaryTableCell(name, new Set<string>(['summary-name']));
                let valueCell: SummaryTableCell = new SummaryTableCell(value, new Set<string>(['summary-value']));
                lastReapeatingInstanceRow = new SummaryTableRow([nameCell, valueCell], cssClasses);
                rows.push(lastReapeatingInstanceRow);
            }
        }
        if (lastReapeatingInstanceRow) {
            lastReapeatingInstanceRow.cssClasses.add('summary-repeating-item-last-row');
            lastReapeatingInstanceRow.cssClasses.add('summary-repeating-last-item-last-row');
        }
        return rows;
    }

    private getFieldValueFormatted(fieldPath: string): string {
        let fieldValue: string = this.expressionInputSubjectService.getLatestFieldValue(fieldPath);
        let metadata: QuestionMetadata = this.fieldMetadataService.getMetadataForField(fieldPath);
        return metadata
            ? this.fieldFormatterResolver.resolve(metadata.dataType).format(fieldValue, metadata)
            : fieldValue;
    }
}
