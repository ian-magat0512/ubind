import { IconLibrary } from "@app/models/icon-library.enum";
import { PageType } from "@app/models/page-type.enum";

/**
 * Represents trigger resource.
 */
export interface PortalPageTriggerResourceModel {
    tenantId: string;
    productId: string;
    productAlias: string;
    environment: string;
    automationAlias: string;
    triggerAlias: string;
    pages: Array<PageResourceModel>;
    actionName: string;
    actionIcon: string;
    actionIconLibrary: IconLibrary;
    actionButtonLabel: string;
    actionButtonPrimary: boolean;
    includeInMenu: boolean;
    spinnerAlertText: string;
}

/**
 * Represents page resource.
 */
export interface PageResourceModel {
    entityType: string;
    pageType: PageType;
    tabs: Array<string>;
}
