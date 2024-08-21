import { FormlyFieldConfig, FormlyConfig } from '@ngx-formly/core';

/**
 * Export label template class.
 * TODO: Write a better class header: label template functions.
 */
export class LabelTemplate {
    public run(fc: FormlyConfig): void {
        fc.templateManipulators.postWrapper.unshift((field: FormlyFieldConfig) => {
            if (field && field.templateOptions && field.templateOptions.fieldConfiguration
                && field.templateOptions.fieldConfiguration.label) {
                return 'label';
            }
        });
    }
}
