import { IconLibrary } from "@app/models/icon-library.enum";

/**
 * Resource model for feature settings for a tenant, which determines 
 * what platform features are available to a tenant
 */
export interface FeatureSettingResourceModel {
    id: string;
    name: string;
    icon: string;
    disabled: boolean;
    sortOrder: number;
    iconLibrary: IconLibrary;
}
