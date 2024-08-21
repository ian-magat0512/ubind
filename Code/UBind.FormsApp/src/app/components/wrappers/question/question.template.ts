import { FormlyFieldConfig, FormlyConfig } from '@ngx-formly/core';

/**
 * Export question template class.
 * This class manage question template functions.
 */
export class QuestionTemplate {
    public run(fc: FormlyConfig): void {
        fc.templateManipulators.postWrapper.unshift((field: FormlyFieldConfig) => {
            if (field && field.templateOptions && field.templateOptions.fieldConfiguration
                && field.templateOptions.fieldConfiguration.question) {
                return 'question';
            }
        });
    }
}
