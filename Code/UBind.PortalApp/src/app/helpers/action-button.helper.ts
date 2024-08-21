import { ActionButton } from "@app/models/action-button";

/**
 * This is a helper for action buttons.
 */
export class ActionButtonHelper {

    /**
     * Sort the action buttons based on primary buttons. 
     * It should sort it ascending by primary buttons. 
     * Which means all primary buttons should in the first index and so on.
     * @param actionButtons list of action butons to be sort.
     * @returns return the list of action button.
     */
    public static sortActionButtons(actionButtons: Array<ActionButton>): Array<ActionButton> {
        actionButtons.sort((a: ActionButton, b: ActionButton) => {
            if (a.IsPrimary && !b.IsPrimary) {
                return -1;
            }
            if (!a.IsPrimary && b.IsPrimary) {
                return 1;
            }
            if (!a.IsPrimary && !b.IsPrimary) {
                if (a.SortOrder === null && b.SortOrder !== null) {
                    return 1;
                }
                if (a.SortOrder !== null && b.SortOrder === null) {
                    return -1;
                }
                if (a.SortOrder !== null && b.SortOrder !== null) {
                    return a.SortOrder - b.SortOrder;
                }
            }
            return 0;
        });

        return actionButtons;
    }
}
