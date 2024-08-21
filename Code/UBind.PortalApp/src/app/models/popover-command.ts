import { ActionButtonPopover } from "./action-button-popover";
import { DeploymentEnvironment } from "./deployment-environment.enum";

/**
 * Represents a PopoverCommand which was clicked on
 */
export interface PopoverCommand {
    data: PopoverCommandData;
}

/**
 * Represents the data sent with a PopoverComand
 */
export interface PopoverCommandData {
    action: ActionButtonPopover;
    environment?: DeploymentEnvironment;
}
