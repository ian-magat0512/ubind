import { Component, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { LoadingController } from '@ionic/angular';
import { InvitationApiService } from '@app/services/api/invitation-api.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { AppConfigService } from '@app/services/app-config.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { EntryPage } from '../entry.page';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { finalize } from 'rxjs/operators';
import { PortalApiService } from '@app/services/api/portal-api.service';
import { Title } from '@angular/platform-browser';

/**
 * Export request password page component class.
 * TODO: Write a better class header: Requesting of password function.
 */
@Component({
    selector: 'app-request-password',
    templateUrl: './request-password.page.html',
    styleUrls: [
        './request-password.page.scss',
    ],
})
export class RequestPasswordPage extends EntryPage implements OnDestroy {
    private requestPasswordForm: FormGroup;
    public errorDisplay: string = '';
    public formHasError: boolean = false;

    public constructor(
        private sharedAlertService: SharedAlertService,
        private formBuilder: FormBuilder,
        private invitationService: InvitationApiService,
        private loadCtrl: LoadingController,
        private navProxy: NavProxyService,
        appConfigService: AppConfigService,
        public layoutManager: LayoutManagerService,
        portalApiService: PortalApiService,
        titleService: Title,
    ) {
        super(appConfigService, portalApiService, titleService);
        this.requestPasswordForm = this.formBuilder.group({
            email: ['', [Validators.required, Validators.email]],
        });
    }

    public ngOnDestroy(): void {
        super.ngOnDestroy();
    }

    public async userDidTapResetPassword(value: any): Promise<void> {
        if (this.requestPasswordForm.valid) {
            this.formHasError = false;
            const loading: HTMLIonLoadingElement = await this.loadCtrl.create({
                message: 'Please wait...',
            });
            await loading.present().then(() => {
                const email: string = value.email;
                if (email) {
                    this.invitationService.requestResetPassword(email, this.portalTenantId, this.portalId)
                        .pipe(finalize(() => loading.dismiss()))
                        .subscribe(
                            () => {
                                this.sharedAlertService.showWithActionHandler({
                                    header: "Password reset request received",
                                    subHeader: 'Your password reset request has been received. If "' + email
                                        + '" is a registered email address, a message will be sent to '
                                        + 'that account containing a link that can be used to reset your password.',
                                    buttons: [{
                                        text: 'OK',
                                        handler: (): any => {
                                            this.navProxy.navigateForward(['login']);
                                        },
                                    }],
                                });
                            },
                        );
                }
            });
        } else {
            this.formHasError = true;
        }
    }

    public userDidTapCancel(): void {
        this.navProxy.navigate(['login']);
    }
}
