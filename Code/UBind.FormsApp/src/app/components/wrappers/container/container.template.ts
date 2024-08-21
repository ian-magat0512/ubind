import { FormlyFieldConfig, FormlyConfig } from '@ngx-formly/core';

/**
 * Export container template class.
 * This class manage container template functions.
 */
export class ContainerTemplate {
    public run(fc: FormlyConfig): void {
        fc.templateManipulators.postWrapper.unshift((field: FormlyFieldConfig) => {
            if (field?.templateOptions?.fieldConfiguration &&
                (field.templateOptions.fieldConfiguration.containerClass
                    || field.templateOptions.fieldConfiguration.containerCss
                    || field.templateOptions.fieldConfiguration.widgetCssWidth)
            ) {
                return 'container';
            }
        });
    }
}
