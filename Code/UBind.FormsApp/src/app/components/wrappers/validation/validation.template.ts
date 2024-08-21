import { FormlyFieldConfig, FormlyConfig } from '@ngx-formly/core';

/**
 * Determines whether the field will be wrapped with the validation wrapper
 */
export class ValidationTemplate {
    public run(fc: FormlyConfig): void {
        fc.templateManipulators.postWrapper.unshift((field: FormlyFieldConfig) => {
            // if (field && field.templateOptions && field.templateOptions.validators) {
            return 'validation';
            // }
        });
    }
}
