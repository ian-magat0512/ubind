import { FormlyFieldConfig, FormlyConfig } from '@ngx-formly/core';

/**
 * Export tooltip template class.
 * This class manage tooltip template functions.
 * To display the tooltip in the end of label if has label field configured.
 */
export class LabelTooltipTemplate {
    public run(fc: FormlyConfig): void {
        fc.templateManipulators.postWrapper.unshift((field: FormlyFieldConfig) => {
            if (field && field.templateOptions && field.templateOptions.fieldConfiguration
                && field.templateOptions.fieldConfiguration.tooltip
                && field.templateOptions.fieldConfiguration.label) {
                return 'tooltip';
            }
        });
    }
}
