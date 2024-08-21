import { ActionButtonHelper } from "./action-button.helper";
import { ActionButton } from "@app/models/action-button";
import { IconLibrary } from "@app/models/icon-library.enum";

describe('ActionButtonHelper', () => {
    it('should sort action buttons correctly, buttons with null sort order are last listed', () => {
        let expectedActionButtons: Array<ActionButton> = [];

        const actionButtons: Array<ActionButton> = [
            ActionButton.createActionButton(
                "Filter",
                "filter",
                IconLibrary.IonicV5,
                false,
                `Filter`,
                true,
                (): Promise<void> => null,
                3,
            ),
            ActionButton.createActionButton(
                "Search",
                "search",
                IconLibrary.IonicV5,
                false,
                `Search`,
                true,
                (): Promise<void> => null,
                2,
            ),
            ActionButton.createActionButton(
                "Create",
                "create",
                IconLibrary.IonicV5,
                false,
                `Create`,
                true,
                (): Promise<void> => null,
                null,
            ),
            ActionButton.createActionButton(
                "Refresh",
                "refresh",
                IconLibrary.IonicV5,
                false,
                `refresh`,
                true,
                (): Promise<void> => null,
                1,
            ),
        ];

        expectedActionButtons.push(actionButtons[3]);
        expectedActionButtons.push(actionButtons[1]);
        expectedActionButtons.push(actionButtons[0]);
        expectedActionButtons.push(actionButtons[2]);
        const sortedActionButtons: ActionButtonHelper = ActionButtonHelper.sortActionButtons(actionButtons);

        expect(sortedActionButtons).toEqual(expectedActionButtons);

    });
});
