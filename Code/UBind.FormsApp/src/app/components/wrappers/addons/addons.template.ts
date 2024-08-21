import { FormlyFieldConfig, FormlyConfig } from '@ngx-formly/core';

/**
 * Addons allows for a icon or text to be added to the left of right side of an input field,
 */
export class AddonsTemplate {
    public run(fc: FormlyConfig): void {
        fc.templateManipulators.postWrapper.unshift((field: FormlyFieldConfig) => {
            if (field && field.templateOptions &&
                (field.templateOptions.addonLeft || field.templateOptions.addonRight)) {
                return 'addons';
            }
        });
    }
}
