import { Component, OnDestroy, Injector, ElementRef } from '@angular/core';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { finalize } from 'rxjs/operators';
import { AccountApiService } from '@app/services/api/account-api.service';
import { AuthenticationService } from '@app/services/authentication.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { AppConfigService } from '@app/services/app-config.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { Subscription, SubscriptionLike } from 'rxjs';
import { EventService } from '@app/services/event.service';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { UserResourceModel } from '@app/resource-models/user/user.resource-model';
import { FormHelper } from '@app/helpers/form.helper';
import { DefaultImgHelper } from '@app/helpers/default-img-helper';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Export upload picture account page component class
 * This class manage for uploading of user account picture.
 */
@Component({
    selector: 'app-upload-picture-account',
    templateUrl: './upload-picture-account.page.html',
    styleUrls: [
        './upload-picture-account.page.scss',
        '../../../../assets/css/form-toolbar.scss',
    ],
})
export class UploadPictureAccountPage extends DetailPage implements OnDestroy {
    public defaultImgPath: string = 'assets/imgs/default-user.svg';
    public profilePictureUrl: SafeUrl;
    protected subscriptions: Array<SubscriptionLike> = new Array<SubscriptionLike>();
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        public authService: AuthenticationService,
        public accountApiService: AccountApiService,
        public eventService: EventService,
        public sharedLoaderService: SharedLoaderService,
        public appConfigService: AppConfigService,
        public sanitizer: DomSanitizer,
        public navProxy: NavProxyService,
        public layoutManager: LayoutManagerService,
        public userPath: UserTypePathHelper,
        private sharedLoader: SharedLoaderService,
        private sharedAlert: SharedAlertService,
        public elementRef: ElementRef,
        public injector: Injector,
        private formHelper: FormHelper,
    ) {
        super(eventService, elementRef, injector);
    }

    public ngOnDestroy(): void {
        this.subscriptions.forEach((s: SubscriptionLike) => s.unsubscribe());
    }

    public async userDidTapCancelButton(files: any): Promise<void> {
        if (files.length !== 0) {
            if (!await this.formHelper.confirmExitWithUnsavedChanges()) {
                return;
            }
        }
        this.returnToPrevious();
    }

    public async userDidTapSaveButton(files: any): Promise<void> {
        if (files.length === 0) {
            return;
        }

        const formData: FormData = new FormData();

        for (const file of files) {
            formData.append(file.name, file);
        }

        const userId: string = this.authService.userId;

        await this.sharedLoader.presentWithDelay();
        let currentUserProfilePicId: string = this.authService.profilePictureId;
        this.authService.profilePictureId = null;

        const subscription: Subscription = this.accountApiService
            .uploadProfilePicture(formData)
            .pipe(finalize(() => {
                this.sharedLoaderService.dismiss();
                subscription.unsubscribe();
            }))
            .subscribe(
                (user: UserResourceModel) => {
                    this.eventService.getEntityUpdatedSubject('User').next(user);
                    this.authService.profilePictureId = user.profilePictureId;
                    this.eventService.userPictureChanged(userId);
                    this.sharedAlert.showToast('Your new account picture was saved');
                    this.returnToPrevious();
                },
                (error: any) => {
                    this.authService.profilePictureId = currentUserProfilePicId;
                },
            );
    }

    public updatePicture(files: any): void {
        if (files.length > 0) {
            this.profilePictureUrl = this.sanitizer.bypassSecurityTrustUrl(window.URL.createObjectURL(files[0]));
        }
    }

    private returnToPrevious(): void {
        this.navProxy.navigateBack([this.userPath.account]);
    }

    public setDefaultImg(event: any): void {
        DefaultImgHelper.setImageSrcAndFilter(event.target, this.defaultImgPath, null);
    }
}
