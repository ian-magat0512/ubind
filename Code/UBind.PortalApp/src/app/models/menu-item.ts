import { Permission } from "../helpers/permissions.helper";
import { NavigationExtras } from "@angular/router";
import { FeatureSetting } from "./feature-setting.enum";

/**
 * Represents a menu item
 */
export interface MenuItem {
    identifier: string;
    requiresFeature?: FeatureSetting;
    title: string;
    icon: string;
    iconLibrary: string;
    permissions?: Array<Permission>;
    navigate: NavigationSpecification;
}

/**
 * Specifications for where and how to navigate to
 */
export interface NavigationSpecification {
    commands: Array<string>;
    extras?: NavigationExtras;
}
