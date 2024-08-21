import { PortalPageTriggerResourceModel } from "@app/resource-models/portal-page-trigger.resource-model";
import { IconLibrary } from "./icon-library.enum";

/**
 * This is action button model for popover settings.
 */
export interface ActionButtonPopover {
    actionName: string;
    actionIcon: string;
    iconLibrary: IconLibrary;
    actionButtonLabel: string;
    actionButtonPrimary: boolean;
    includeInMenu: boolean;
    portalPageTrigger?: PortalPageTriggerResourceModel;
    callback?: () => Promise<void> | void;
}
