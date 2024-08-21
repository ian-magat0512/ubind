import { Component, OnDestroy, OnInit } from '@angular/core';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { EntryPage } from '../entry.page';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { AppConfigService } from '@app/services/app-config.service';
import { PortalApiService } from '@app/services/api/portal-api.service';
import { Title } from '@angular/platform-browser';
import { AuthenticationService } from '@app/services/authentication.service';
import { LoginRedirectService } from '@app/services/login-redirect.service';
import { EventService } from '@app/services/event.service';
import { LoadingController } from '@ionic/angular';
import { InvitationApiService } from '@app/services/api/invitation-api.service';
import { finalize, takeUntil } from 'rxjs/operators';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { RouteHelper } from '@app/helpers/route.helper';
/**
 * This page should display a message that the user password has been expired.
 */
@Component({
    selector: 'app-password-expired',
    templateUrl: './password-expired.page.html',
    styleUrls: [
        './password-expired.page.scss',
    ],
})
export class PasswordExpiredPage extends EntryPage implements OnInit, OnDestroy {

    public isCurrentlyLoggedIn: boolean = false;
    private userEmailAddress: string;

    public constructor(
        private eventService: EventService,
        private authenticationService: AuthenticationService,
        private sharedAlertService: SharedAlertService,
        private invitationService: InvitationApiService,
        private loginRedirectService: LoginRedirectService,
        private loadCtrl: LoadingController,
        protected routeHelper: RouteHelper,
        public navProxy: NavProxyService,
        public layoutManager: LayoutManagerService,
        appConfigService: AppConfigService,
        portalApiService: PortalApiService,
        titleService: Title,
    ) {
        super(appConfigService, portalApiService, titleService);
    }

    public ngOnInit(): void {
        super.ngOnInit();
        if (this.authenticationService.isAuthenticated()) {
            this.isCurrentlyLoggedIn = true;
            this.userEmailAddress = this.authenticationService.userEmail;
        }
    }

    public ngOnDestroy(): void {
        super.ngOnDestroy();
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    public goBackToLogin(): void {
        if (this.authenticationService.isAuthenticated()) {
            this.authenticationService.logout();
            this.loginRedirectService.clear();
        }
        this.navProxy.navigate(['login']);
        this.eventService.userPasswordExpired();
    }

    public async userDidTapResetPassword(): Promise<void> {
        const loading: HTMLIonLoadingElement = await this.loadCtrl.create({
            message: 'Please wait...',
        });
        await loading.present().then(() => {
            const emailAddress: string = this.routeHelper.getParam('emailAddress')
                || this.userEmailAddress;
            if (emailAddress) {
                this.invitationService.requestResetPassword(emailAddress, this.portalTenantId, this.portalId, true)
                    .pipe(
                        finalize(() => loading.dismiss()),
                        takeUntil(this.destroyed),
                    )
                    .subscribe(
                        () => {
                            this.sharedAlertService.showWithActionHandler({
                                header: "Reset link sent",
                                subHeader: 'An email has been sent to you with a link to reset your password.',
                                buttons: [{
                                    text: 'OK',
                                    handler: (): any => {
                                        this.goBackToLogin();
                                    },
                                }],
                            });
                        },
                    );
            }
        });
    }
}
