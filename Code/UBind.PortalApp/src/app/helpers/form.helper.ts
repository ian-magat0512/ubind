import { Injectable } from "@angular/core";
import { SharedAlertService } from "@app/services/shared-alert.service";

/**
 * Export form helper class.
 * This class is manage the Form helper.
 */
@Injectable({
    providedIn: 'root',
})
export class FormHelper {

    public constructor(
        private sharedAlertService: SharedAlertService,
    ) {
    }

    public async confirmExitWithUnsavedChanges(): Promise<boolean> {
        return new Promise((resolve: any, reject: any): void => {
            this.sharedAlertService.showWithActionHandler({
                header: 'Unsaved Changes',
                subHeader: 'You have unsaved changes, are you sure you wish '
                    + 'to close the current view without saving them?',
                buttons: [
                    {
                        text: 'No',
                        handler: (): any => {
                            resolve(false);
                        },
                    },
                    {
                        text: 'Yes',
                        handler: (): any => {
                            resolve(true);
                        },
                    },
                ],
            });
        });
    }
}
