import { Field } from "@app/models/configuration/field";
import { FieldGroup } from "@app/models/configuration/field-group";
import { LineInputFieldConfiguration } from "@app/resource-models/configuration/fields/line-input-field.configuration";
import { VisibleFieldConfiguration } from "@app/resource-models/configuration/fields/visible-field.configuration";
import { FormlyFieldConfig } from "@ngx-formly/core";
import * as _ from 'lodash-es';
import { FieldConfigurationHelper } from "./field-configuration.helper";

/**
 * Holds the position of a field
 */
interface FieldPosition {
    rowIndex: number;
    fieldIndex: number;
}

/**
 * Stats about what was changed
 */
export interface QuestionSetSyncResults {
    rowsAdded: number;
    rowsRemoved: number;
    fieldsAdded: number;
    fieldsRemoved: number;
    addedFieldKeys: Array<string>;
    removedFieldKeys: Array<string>;
}

/**
 * Synchronises changes from one set of FormlyFieldConfigs to another, so as to
 * modify the existing array, so that the entire page will not be reloaded
 */
export class QuestionSetConfigSyncHelper {

    public static synchronise(
        source: Array<FormlyFieldConfig>,
        target: Array<FormlyFieldConfig>,
    ): QuestionSetSyncResults {
        let results: QuestionSetSyncResults = {
            rowsAdded: 0,
            rowsRemoved: 0,
            fieldsAdded: 0,
            fieldsRemoved: 0,
            addedFieldKeys: new Array<string>(),
            removedFieldKeys: new Array<string>(),
        };

        // remove any fields from the target which are no longer in the source
        for (let fieldRow of target) {
            for (let j: number = 0; j < fieldRow.fieldGroup.length; j++) {
                if (!QuestionSetConfigSyncHelper.fieldExists(
                    fieldRow.fieldGroup[j],
                    source)) {
                    const deletedField: FormlyFieldConfig = fieldRow.fieldGroup.splice(j, 1)[0];
                    j--;
                    results.fieldsRemoved++;
                    results.removedFieldKeys.push(<string>deletedField.key);
                }
            }
        }

        // make sure the target has enough rows
        while (source.length > target.length) {
            let fieldGroupFields: Array<Field> = new Array<Field>();
            let fieldGroup: FieldGroup = {
                className: 'row',
                fieldGroup: fieldGroupFields,
            };
            target.push(fieldGroup);
            results.rowsAdded++;
        }

        // reorder and insert items to ensure the order of items matches the source
        for (let i: number = 0; i < source.length; i++) {
            for (let j: number = 0; j < source[i].fieldGroup.length; j++) {
                let desiredFieldPosition: FieldPosition = {
                    rowIndex: i,
                    fieldIndex: j,
                };
                let actualFieldPosition: FieldPosition
                    = QuestionSetConfigSyncHelper.getFieldPosition(<string>source[i].fieldGroup[j].key, target);
                if (actualFieldPosition == null) {
                    // insert it
                    target[desiredFieldPosition.rowIndex].fieldGroup.splice(
                        desiredFieldPosition.fieldIndex,
                        0,
                        source[i].fieldGroup[j]);
                    results.fieldsAdded++;
                    results.addedFieldKeys.push(<string>source[i].fieldGroup[j].key);
                } else if (!_.isEqual(desiredFieldPosition, actualFieldPosition)) {
                    // move it
                    target[desiredFieldPosition.rowIndex].fieldGroup.splice(
                        desiredFieldPosition.fieldIndex,
                        0,
                        target[actualFieldPosition.rowIndex].fieldGroup.splice(
                            actualFieldPosition.fieldIndex,
                            1)[0]);
                }
            }
        }

        // remove any excess rows
        while (target.length > source.length) {
            target.pop();
            results.rowsRemoved++;
        }

        return results;
    }

    private static fieldExists(
        targetField: FormlyFieldConfig,
        fieldRows: Array<FormlyFieldConfig>,
    ): boolean {
        for (let fieldRow of fieldRows) {
            for (let field of fieldRow.fieldGroup) {
                if (field.key == targetField.key) {
                    // if the type changes, we'll consider it a different field
                    if (field.type != targetField.type) {
                        return false;
                    }
                    if (FieldConfigurationHelper.isVisibleField(field.templateOptions.fieldConfiguration)) {
                        if (QuestionSetConfigSyncHelper.hasVisibleFieldContainerWrapperPropertiesChanged(
                            field.templateOptions.fieldConfiguration,
                            targetField.templateOptions.fieldConfiguration)
                            || QuestionSetConfigSyncHelper.hasVisibleFieldLabelWrapperPropertiesChanged(
                                field.templateOptions.fieldConfiguration,
                                targetField.templateOptions.fieldConfiguration)
                            || QuestionSetConfigSyncHelper.hasVisibleFieldQuestionWrapperPropertiesChanged(
                                field.templateOptions.fieldConfiguration,
                                targetField.templateOptions.fieldConfiguration)
                            || QuestionSetConfigSyncHelper.hasVisibleFieldContentWrapperPropertiesChanged(
                                field.templateOptions.fieldConfiguration,
                                targetField.templateOptions.fieldConfiguration)
                            || QuestionSetConfigSyncHelper.hasVisibleFieldTooltipWrapperPropertiesChanged(
                                field.templateOptions.fieldConfiguration,
                                targetField.templateOptions.fieldConfiguration)
                        ) {
                            return false;
                        }
                    }
                    if (FieldConfigurationHelper.isLineInputField(field.templateOptions.fieldConfiguration)) {
                        if (QuestionSetConfigSyncHelper.hasLineInputFieldAddonsWrapperPropertiesChanged(
                            field.templateOptions.fieldConfiguration,
                            targetField.templateOptions.fieldConfiguration)
                        ) {
                            return false;
                        }
                    }
                    return true;
                }
            }
        }
        return false;
    }

    private static getFieldPosition(key: string, fieldRows: Array<FormlyFieldConfig>): FieldPosition {
        for (let i: number = 0; i < fieldRows.length; i++) {
            for (let j: number = 0; j < fieldRows[i].fieldGroup.length; j++) {
                if (fieldRows[i].fieldGroup[j].key == key) {
                    return {
                        rowIndex: i,
                        fieldIndex: j,
                    };
                }
            }
        }
        return null;
    }

    private static hasVisibleFieldContainerWrapperPropertiesChanged(
        source: VisibleFieldConfiguration,
        target: VisibleFieldConfiguration,
    ): boolean {
        // if the widgetCssWidth has been added or removed, let's consider it a new field
        // so that it re-renders it with/without a container wrapper
        let sourceFieldHasWidgetCssWidth: boolean = source.widgetCssWidth != null;
        let targetFieldHasWidgetCssWidgth: boolean = target.widgetCssWidth != null;
        if (sourceFieldHasWidgetCssWidth != targetFieldHasWidgetCssWidgth) {
            return true;
        }

        // if there was an addition/removal of containerClass then consider it a new field
        // so that it re-renders it with/without a container wrapper
        let sourceFieldHasContainerClass: boolean = source.containerClass != null;
        let targetFieldHasContainerClass: boolean = target.containerClass != null;
        if (sourceFieldHasContainerClass != targetFieldHasContainerClass) {
            return true;
        }

        // if there was an addition/removal of containerCss then consider it a new field
        // so that it re-renders it with/without a container wrapper
        let sourceFieldHasContainerCss: boolean = source.containerCss != null;
        let targetFieldHasContainerCss: boolean = target.containerCss != null;
        if (sourceFieldHasContainerCss != targetFieldHasContainerCss) {
            return true;
        }

        return false;
    }

    private static hasVisibleFieldLabelWrapperPropertiesChanged(
        source: VisibleFieldConfiguration,
        target: VisibleFieldConfiguration,
    ): boolean {
        // if the label has been added or removed, let's consider it a new field
        // so that it re-renders it with/without a label wrapper
        let sourceFieldHasLabel: boolean = source.label != null;
        let targetFieldHasLabel: boolean = target.label != null;
        if (sourceFieldHasLabel != targetFieldHasLabel) {
            return true;
        }

        return false;
    }

    private static hasVisibleFieldQuestionWrapperPropertiesChanged(
        source: VisibleFieldConfiguration,
        target: VisibleFieldConfiguration,
    ): boolean {
        // if the question has been added or removed, let's consider it a new field
        // so that it re-renders it with/without a question wrapper
        let sourceFieldHasQuestion: boolean = source.question != null;
        let targetFieldHasQuestion: boolean = target.question != null;
        if (sourceFieldHasQuestion != targetFieldHasQuestion) {
            return true;
        }

        return false;
    }

    private static hasVisibleFieldContentWrapperPropertiesChanged(
        source: VisibleFieldConfiguration,
        target: VisibleFieldConfiguration,
    ): boolean {
        // if the heading2 has been added or removed, let's consider it a new field
        // so that it re-renders it with/without a content wrapper
        let sourceFieldHasHeading2: boolean = source.heading2 != null;
        let targetFieldHasHeading2: boolean = target.heading2 != null;
        if (sourceFieldHasHeading2 != targetFieldHasHeading2) {
            return true;
        }

        // if the heading3 has been added or removed, let's consider it a new field
        // so that it re-renders it with/without a content wrapper
        let sourceFieldHasHeading3: boolean = source.heading3 != null;
        let targetFieldHasHeading3: boolean = target.heading3 != null;
        if (sourceFieldHasHeading3 != targetFieldHasHeading3) {
            return true;
        }

        // if the heading4 has been added or removed, let's consider it a new field
        // so that it re-renders it with/without a content wrapper
        let sourceFieldHasHeading4: boolean = source.heading4 != null;
        let targetFieldHasHeading4: boolean = target.heading4 != null;
        if (sourceFieldHasHeading4 != targetFieldHasHeading4) {
            return true;
        }

        // if the paragraph has been added or removed, let's consider it a new field
        // so that it re-renders it with/without a content wrapper
        let sourceFieldHasParagraph: boolean = source.paragraph != null;
        let targetFieldHasParagraph: boolean = target.paragraph != null;
        if (sourceFieldHasParagraph != targetFieldHasParagraph) {
            return true;
        }

        // if the html has been added or removed, let's consider it a new field
        // so that it re-renders it with/without a content wrapper
        let sourceFieldHasHtml: boolean = source.html != null;
        let targetFieldHasHtml: boolean = target.html != null;
        if (sourceFieldHasHtml != targetFieldHasHtml) {
            return true;
        }

        // if the htmlTermsAndConditions has been added or removed, let's consider it a new field
        // so that it re-renders it with/without a content wrapper
        let sourceFieldHasHtmlTermsAndConditions: boolean = source.htmlTermsAndConditions != null;
        let targetFieldHasHtmlTermsAndConditions: boolean = target.htmlTermsAndConditions != null;
        if (sourceFieldHasHtmlTermsAndConditions != targetFieldHasHtmlTermsAndConditions) {
            return true;
        }

        return false;
    }

    private static hasVisibleFieldTooltipWrapperPropertiesChanged(
        source: VisibleFieldConfiguration,
        target: VisibleFieldConfiguration,
    ): boolean {
        // if the tooltip has been added or removed, let's consider it a new field
        // so that it re-renders it with/without a tooltip wrapper
        let sourceFieldHasTooltip: boolean = source.tooltip != null;
        let targetFieldHasTooltip: boolean = target.tooltip != null;
        if (sourceFieldHasTooltip != targetFieldHasTooltip) {
            return true;
        }

        return false;
    }

    private static hasLineInputFieldAddonsWrapperPropertiesChanged(
        source: LineInputFieldConfiguration,
        target: LineInputFieldConfiguration,
    ): boolean {
        // if icon or text addons have been added or removed, let's consider it a new field
        // so that it re-renders it with/without a tooltip wrapper
        let sourceFieldHasIconLeft: boolean = source.iconLeft != null;
        let targetFieldHasIconLeft: boolean = target.iconLeft != null;
        if (sourceFieldHasIconLeft != targetFieldHasIconLeft) {
            return true;
        }
        let sourceFieldHasIconRight: boolean = source.iconRight != null;
        let targetFieldHasIconRight: boolean = target.iconRight != null;
        if (sourceFieldHasIconRight != targetFieldHasIconRight) {
            return true;
        }
        let sourceFieldHasTextLeft: boolean = source.textLeft != null;
        let targetFieldHasTextLeft: boolean = target.textLeft != null;
        if (sourceFieldHasTextLeft != targetFieldHasTextLeft) {
            return true;
        }
        let sourceFieldHasTextRight: boolean = source.textRight != null;
        let targetFieldHasTextRight: boolean = target.textRight != null;
        if (sourceFieldHasTextRight != targetFieldHasTextRight) {
            return true;
        }

        return false;
    }
}
