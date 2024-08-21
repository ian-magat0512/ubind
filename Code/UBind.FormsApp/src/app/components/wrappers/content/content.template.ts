import { FormlyFieldConfig, FormlyConfig } from '@ngx-formly/core';

/**
 * Export content template class.
 * This class manage content wrapper functions.
 */
export class ContentTemplate {
    public run(fc: FormlyConfig): any {
        fc.templateManipulators.postWrapper.unshift((field: FormlyFieldConfig) => {
            if (field != null &&
                field.templateOptions != null &&
                field.templateOptions.fieldConfiguration != null &&
                (
                    field.templateOptions.fieldConfiguration.heading2 != null ||
                    field.templateOptions.fieldConfiguration.heading3 != null ||
                    field.templateOptions.fieldConfiguration.heading4 != null ||
                    field.templateOptions.fieldConfiguration.paragraph != null ||
                    field.templateOptions.fieldConfiguration.html != null ||
                    field.templateOptions.fieldConfiguration.htmlTermsAndConditions != null
                )) {
                return 'content';
            }
        });
    }
}
