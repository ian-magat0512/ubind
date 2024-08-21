import { FormlyFieldConfig, FormlyConfig } from '@ngx-formly/core';

/**
 * Formly template which determins whether the descrpition wrapper should be rendered
 */
export class DescriptionTemplate {
    public run(fc: FormlyConfig): void {
        fc.templateManipulators.postWrapper.unshift((field: FormlyFieldConfig) => {
            if (field && field.templateOptions && field.templateOptions.fieldConfiguration
                && field.templateOptions.fieldConfiguration.helpMessage
            ) {
                return 'description';
            }
        });
    }
}
