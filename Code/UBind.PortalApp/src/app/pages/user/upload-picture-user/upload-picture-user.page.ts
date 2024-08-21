import { Component, OnDestroy, Injector, ElementRef, OnInit } from '@angular/core';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { finalize } from 'rxjs/operators';
import { scrollbarStyle } from '@assets/scrollbar';
import { AuthenticationService } from '@app/services/authentication.service';
import { UserApiService } from '@app/services/api/user-api.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { AppConfigService } from '@app/services/app-config.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { AppConfig } from '@app/models/app-config';
import { Subscription, SubscriptionLike } from 'rxjs';
import { RouteHelper } from '@app/helpers/route.helper';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { EventService } from '@app/services/event.service';
import { UserResourceModel } from '@app/resource-models/user/user.resource-model';
import { FormHelper } from '@app/helpers/form.helper';
import { ProfilePicUrlPipe } from '@app/pipes/profile-pic-url.pipe';
import { DefaultImgHelper } from '@app/helpers/default-img-helper';

/**
 * Export upload picture user page class.
 * This class manage uploading of users pictures.
 */
@Component({
    selector: 'app-upload-picture-user',
    templateUrl: './upload-picture-user.page.html',
    styleUrls: ['./upload-picture-user.page.scss'],
    styles: [scrollbarStyle],
})
export class UploadPictureUserPage extends DetailPage implements OnInit, OnDestroy {
    public userId: string;
    public userFullName: string;
    public userProfilePicId: string;
    public profilePictureUrl: SafeUrl;
    public imageBaseUrl: string;
    public defaultImgPath: string = 'assets/imgs/default-user.svg';

    protected subscriptions: Array<SubscriptionLike> = new Array<SubscriptionLike>();

    public constructor(
        protected routeHelper: RouteHelper,
        protected authService: AuthenticationService,
        public navProxy: NavProxyService,
        protected appConfigService: AppConfigService,
        protected userApiService: UserApiService,
        protected sharedAlertService: SharedAlertService,
        protected eventService: EventService,
        protected sharedLoaderService: SharedLoaderService,
        protected sanitizer: DomSanitizer,
        public layoutManager: LayoutManagerService,
        elementRef: ElementRef,
        injector: Injector,
        private formHelper: FormHelper,
        private profilePicUrlPipe: ProfilePicUrlPipe,
    ) {
        super(eventService, elementRef, injector);
    }

    public ngOnInit(): void {
        this.userId = this.routeHelper.getParam('userId');
        if (this.userId) {
            this.subscriptions.push(this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
                this.imageBaseUrl = appConfig.portal.api.baseUrl + 'picture/';
            }));

            const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
            params.set('tenant', this.routeHelper.getContextTenantAlias());
            const userGetByIdSub: Subscription = this.userApiService.getById(this.userId, params)
                .subscribe((user: UserResourceModel) => {
                    this.userFullName = user.fullName;
                    this.loadUserProfilePicture(user.profilePictureId);
                });

            this.subscriptions.push(userGetByIdSub);
        }
    }

    public loadUserProfilePicture(profilePictureId: any): void {
        this.profilePictureUrl = this.profilePicUrlPipe.transform(profilePictureId, this.defaultImgPath);
        this.userProfilePicId = profilePictureId;
    }

    public ngOnDestroy(): void {
        this.subscriptions.forEach((s: SubscriptionLike) => s.unsubscribe());
    }

    public async userDidTapCancelButton(files: any): Promise<void> {
        if (files.length > 0 && !await this.formHelper.confirmExitWithUnsavedChanges()) {
            return;
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

        await this.sharedLoaderService.presentWithDelay();

        if (this.userId === this.authService.userId) {
            this.authService.profilePictureId = null;
        }

        const subscription: Subscription = this.userApiService
            .uploadProfilePicture(this.userId, formData, this.routeHelper.getContextTenantAlias())
            .pipe(
                finalize(() => {
                    subscription.unsubscribe();
                    this.sharedLoaderService.dismiss();
                }))
            .subscribe((user: UserResourceModel) => {
                this.eventService.getEntityUpdatedSubject('User').next(user);
                if (this.userId === this.authService.userId) {
                    this.authService.profilePictureId = user.profilePictureId;
                    this.eventService.userPictureChanged(this.userId);
                }
                this.loadUserProfilePicture(user.profilePictureId);
                this.sharedAlertService.showToast(`New user picture for ${this.userFullName} was saved`);
                this.returnToPrevious();
            }, (error: any) => {
                if (this.userId === this.authService.userId) {
                    this.authService.profilePictureId = this.userProfilePicId;
                }
                throw error;
            });
    }

    public updatePicture(files: any): void {
        if (files.length > 0) {
            this.profilePictureUrl = this.sanitizer.bypassSecurityTrustUrl(window.URL.createObjectURL(files[0]));
        }
    }

    private returnToPrevious(): void {
        const pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.pop();
        pathSegments.pop();
        this.navProxy.navigateBack(pathSegments, true, { queryParams: { previous: 'Picture' } });
    }

    public setDefaultImg(event: any): void {
        DefaultImgHelper.setImageSrcAndFilter(event.target, this.defaultImgPath, null);
    }
}
