import { ToastController } from '@ionic/angular';
import { Injectable } from '@angular/core';
import { ToastOptions } from '@ionic/core';

/**
 * Export shared toast service class.
 * This class manage shared toast service functions.
 */
@Injectable({ providedIn: 'root' })
export class SharedToastService {

    public constructor(private toastController: ToastController) { }

    public async create(options: ToastOptions): Promise<HTMLIonToastElement> {

        const toast: HTMLIonToastElement = await this.toastController.create(options);

        const roleAttr: Attr = document.createAttribute('role');
        roleAttr.value = 'alert';
        toast.attributes.setNamedItem(roleAttr);

        return toast;
    }
}
