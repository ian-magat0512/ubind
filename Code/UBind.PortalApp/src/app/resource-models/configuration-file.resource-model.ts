import { FormType } from "@app/models/form-type.enum";

/**
 * Resource model representing a file which is part of a release
 */
export interface ConfigurationFileResourceModel {
    path: string;
    sourceType: string;
    resourceUrl: string;
    formType: FormType;
    isBrowserViewable: boolean;
}
