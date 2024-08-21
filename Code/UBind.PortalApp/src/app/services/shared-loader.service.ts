import { Injectable } from '@angular/core';
import { LoadingController } from '@ionic/angular';
import { LayoutManagerService } from './layout-manager.service';
import { BehaviorSubject, Observable } from 'rxjs';

/**
 * Export shared loader service class.
 * This class manage shared loader services functions.
 */
@Injectable({ providedIn: 'root' })
export class SharedLoaderService {

    private loader: HTMLIonLoadingElement;
    private timeout: any;
    private canPresentLoading: boolean = false;
    private isDelayed: boolean = false;
    private isDelayedLoaderPresented: boolean = false;
    private showBackdrop: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);

    public $showBackdrop: Observable<boolean> = this.showBackdrop.asObservable();

    public constructor(
        private loadCtrl: LoadingController,
        private layoutManager: LayoutManagerService,
    ) { }

    public async presentWithDelay(
        message: string = 'Please wait...', cssClass: string = null, delayInMilliseconds: number = 250): Promise<void> {

        this.isDelayed = true;
        this.isDelayedLoaderPresented = false;
        this.canPresentLoading = true;
        this.showBackdrop.next(true);

        this.loader = await this.createLoader(message, cssClass, true);

        this.timeout = setTimeout(async () => {
            if (this.canPresentLoading) {
                this.isDelayedLoaderPresented = true;
                await this.loader.present();
            }
        }, delayInMilliseconds);
    }

    public async presentWait(cssClass: string = null, showBackdrop: boolean = true): Promise<void> {
        return await this.present('Please wait...', cssClass, showBackdrop);
    }

    public async present(
        message: string = 'Please wait...',
        cssClass: string = null,
        showBackdrop: boolean = true,
    ): Promise<any> {
        this.loader = await this.createLoader(message, cssClass, showBackdrop);
        return await this.loader.present();
    }

    public dismiss(): void {
        if (!this.loader) {
            return;
        }

        if (this.isDelayed) {
            clearTimeout(this.timeout);
            this.canPresentLoading = false;
            this.showBackdrop.next(false);

            if (this.isDelayedLoaderPresented) {
                this.loader.dismiss();
            } else {
                this.loader.remove();
            }

        } else {
            this.loader.dismiss();
        }
    }

    private async createLoader(
        message: string,
        cssClass: string,
        showBackdrop: boolean,
    ): Promise<HTMLIonLoadingElement> {
        if (!cssClass) {
            cssClass = this.layoutManager.splitPaneEnabled ? 'detail-loader' : '';
        }
        if (!showBackdrop) {
            showBackdrop = !this.layoutManager.splitPaneEnabled;
        }

        const loader: HTMLIonLoadingElement = await this.loadCtrl.create({
            message: message,
            cssClass: cssClass,
            showBackdrop: showBackdrop,
        });

        return loader;
    }
}
