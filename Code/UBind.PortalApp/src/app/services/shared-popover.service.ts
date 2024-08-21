import { Injectable } from '@angular/core';
import { PopoverController } from '@ionic/angular';
import { PopoverOptions } from '@ionic/core';

/**
 * Export shared popover service class.
 * TODO: Write a better class header: popover shared services.
 */
@Injectable({ providedIn: 'root' })
export class SharedPopoverService {

    private displayedPopoversByLabels: Array<string> = [];

    public constructor(public popoverController: PopoverController) {
    }

    public async show(
        options: PopoverOptions,
        ariaLabelValue: string,
        dismissAction?: (data: any) => void,
    ): Promise<void> {
        if (this.displayedPopoversByLabels.indexOf(ariaLabelValue) < 0) {
            this.displayedPopoversByLabels.push(ariaLabelValue);
            const popover: HTMLIonPopoverElement = await this.popoverController.create(options);
            popover.backdropDismiss = true;

            const roleAttr: Attr = document.createAttribute('role');
            roleAttr.value = 'region';
            popover.attributes.setNamedItem(roleAttr);
            const ariaLabel: Attr = document.createAttribute('aria-label');
            ariaLabel.value = ariaLabelValue;
            popover.attributes.setNamedItem(ariaLabel);
            popover.attributes.removeNamedItem('aria-modal');

            popover.onDidDismiss()
                .then(dismissAction)
                .then(() => {
                    this.displayedPopoversByLabels.splice(this.displayedPopoversByLabels.indexOf(ariaLabelValue), 1);
                });

            return popover.present();
        }
    }
}
