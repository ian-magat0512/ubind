import { FormlyFieldConfig, FormlyConfig } from '@ngx-formly/core';

/**
 * Export tooltip with no label template class.
 * This class manage tooltip template functions.
 * To display the tooltip in the end of the question field if no label field configured.
 */
export class QuestionTooltipTemplate {
    public run(fc: FormlyConfig): void {
        fc.templateManipulators.postWrapper.unshift((field: FormlyFieldConfig) => {
            if (field && field.templateOptions && field.templateOptions.fieldConfiguration
                && field.templateOptions.fieldConfiguration.tooltip
                && !field.templateOptions.fieldConfiguration.label) {
                return 'tooltip';
            }
        });
    }
}
