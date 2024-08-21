import { Injectable } from '@angular/core';
import { ModalController } from '@ionic/angular';
import { ModalOptions } from '@ionic/core';

/**
 * Export shared modal service class.
 * This class manage Shared modal service functions.
 */
@Injectable({ providedIn: 'root' })
export class SharedModalService {

    private isModalDisplayed: boolean = false;

    public constructor(private modalController: ModalController) { }

    public async show(
        options: ModalOptions,
        ariaLabelValue: string,
        dismissAction?: (data: any) => void,
    ): Promise<void> {

        if (!this.isModalDisplayed) {
            this.isModalDisplayed = true;
            const modal: HTMLIonModalElement = await this.modalController.create(options);
            modal.backdropDismiss = true;

            const roleAttr: Attr = document.createAttribute('role');
            roleAttr.value = 'region';
            modal.attributes.setNamedItem(roleAttr);
            const ariaLabel: Attr = document.createAttribute('aria-label');
            ariaLabel.value = ariaLabelValue;
            modal.attributes.setNamedItem(ariaLabel);
            modal.attributes.removeNamedItem('aria-modal');

            modal.onDidDismiss()
                .then(dismissAction)
                .then(() => {
                    this.isModalDisplayed = false;
                });
            return await modal.present();
        }
    }
}
