import { Injectable } from '@angular/core';
import { AlertController, ToastController } from '@ionic/angular';
import { ProblemDetails } from '@app/models/problem-details';
import { Queue } from 'queue-typescript';
import { AlertOptions } from '@ionic/core';
import { SharedToastService } from './shared-toast.service';

/**
 * Export shared alert service class.
 * This class manage shared alert service functions.
 */
@Injectable({ providedIn: 'root' })
export class SharedAlertService {

    // a queue of alerts waiting to be shown (since only one alert can be shown at a time)
    private alertQueue: Queue<AlertOptions> = new Queue<AlertOptions>();
    private currentAlertBeingShown: AlertOptions = null;

    public constructor(
private alertCtrl: AlertController,
        private toastCtrl: ToastController,
        private sharedToastService: SharedToastService,
    ) { }

    public async showToast(_message: string): Promise<void> {
        const toast: HTMLIonToastElement = await this.sharedToastService.create({
            message: _message,
            duration: 5000,
            cssClass: "toast-center",
        });

        return await toast.present();
    }

    public closeToast(): void {
        this.toastCtrl.getTop().then((t: HTMLIonToastElement) => {
            if (t) {
                t.dismiss();
            }
        });
    }

    public async showWithOk(
        header: string,
        subHeader: string,
        backdropDismiss: boolean = true,
    ): Promise<HTMLIonAlertElement> {
        return this.showWithCustomButton(header, subHeader, 'OK', backdropDismiss);
    }

    public async showWithCustomButton(
        header: string,
        subHeader: string,
        buttonLabel: string,
        backdropDismiss: boolean = true,
    ): Promise<HTMLIonAlertElement> {
        return this.showWithActionHandler({
            header: header,
            subHeader: subHeader,
            buttons: [buttonLabel],
            backdropDismiss: backdropDismiss,
        });
    }

    public async showError(error: ProblemDetails, backdropDismiss: boolean = true): Promise<HTMLIonAlertElement> {
        return this.showErrorWithCustomButtonLabel(error, 'OK', backdropDismiss);
    }

    public async showErrorWithCustomButtonLabel(
        error: ProblemDetails,
        buttonLabel: string,
        backdropDismiss: boolean = true,
    ): Promise<HTMLIonAlertElement> {
        return this.showErrorWithCustomButtons(
            error,
            new Array({ text: buttonLabel, handler: (): any => { } }),
            backdropDismiss,
        );
    }

    public async showErrorWithCustomButtons(
        error: ProblemDetails,
        buttons: Array<{ text: string; handler: () => void }>,
        backdropDismiss: boolean = true,
    ): Promise<HTMLIonAlertElement> {
        let subHeading: string = error.Detail;
        let message: string = '';
        if (error.AdditionalDetails.length) {
            message = '<p>Additional details:</p><ul>';
            error.AdditionalDetails.forEach((item: string) => message += '<li>' + item + '</li>');
            message += '</ul>';
        }

        return this.showWithActionHandler({
            header: error.Title,
            subHeader: subHeading,
            message: message,
            buttons: buttons,
            backdropDismiss: backdropDismiss,
        });
    }

    public async showWithActionHandler(alertOptions: AlertOptions): Promise<HTMLIonAlertElement> {
        this.alertQueue.enqueue(alertOptions);
        return this.processAlertQueue();
    }

    public getAlertCtrl(): AlertController {
        return this.alertCtrl;
    }

    public async create(options: AlertOptions): Promise<HTMLIonAlertElement> {
        const alert: HTMLIonAlertElement = await this.alertCtrl.create(options);

        const roleAttr: Attr = document.createAttribute('role');
        roleAttr.value = 'alert';
        alert.attributes.setNamedItem(roleAttr);
        const ariaLabel: Attr = document.createAttribute('aria-label');
        ariaLabel.value = options.header;
        alert.attributes.setNamedItem(ariaLabel);
        alert.attributes.removeNamedItem('aria-modal');

        return alert;
    }

    private async processAlertQueue(): Promise<HTMLIonAlertElement> {
        if (this.currentAlertBeingShown != null) {
            setTimeout(() => {
                this.processAlertQueue();
            }, 700);
        } else {
            this.currentAlertBeingShown = this.alertQueue.dequeue();
            const alert: HTMLIonAlertElement = await this.create(this.currentAlertBeingShown);
            alert.onDidDismiss().then(this.onDidDismiss.bind(this));
            alert.present();
            return alert;
        }
    }

    private onDidDismiss(): void {
        // wait before allowing the next alert to be shown
        setTimeout(() => {
            this.currentAlertBeingShown = null;
        }, 300);
    }
}
