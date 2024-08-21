import { Injectable, OnDestroy } from "@angular/core";
import { UserResourceModel } from "@app/resource-models/user/user.resource-model";
import { UserViewModel } from "@app/viewmodels/user.viewmodel";
import { SharedAlertService } from "./shared-alert.service";
import { InvitationApiService } from "./api/invitation-api.service";
import { SharedLoaderService } from "@app/services/shared-loader.service";
import { UserApiService } from "./api/user-api.service";
import { EventService } from "./event.service";
import { PermissionService } from "./permission.service";
import { Subscription } from "rxjs";

/**
 * Export user service class.
 * This class manage user service functions.
 */
@Injectable({ providedIn: 'root' })
export class UserService implements OnDestroy {
    public profilePictureBaseUrl: string;
    public subscriptions: Array<Subscription> = [];

    public constructor(
        private sharedAlertService: SharedAlertService,
        private invitationApiService: InvitationApiService,
        private sharedLoaderService: SharedLoaderService,
        private userApiService: UserApiService,
        public permissionsService: PermissionService,
        private eventService: EventService,
    ) {
    }

    public ngOnDestroy(): void {
        this.subscriptions.forEach((s: Subscription) => s.unsubscribe);
    }

    public async sendActivation(user: UserViewModel): Promise<void> {
        if (!user.email) {
            await this.sharedAlertService.showToast("Please set the email address for this user "
                + "before sending the activation invitation");
            return Promise.reject();
        }
        await this.sharedAlertService.showWithActionHandler({
            header: 'Send Activation Email',
            subHeader: 'Would you like to send the account activation invitation email to this user?',
            buttons: [
                {
                    text: 'Yes',
                    handler: (): any => {
                        this.invitationApiService.sendActivationForPerson(user.personId, user.tenantId)
                            .subscribe((user: UserResourceModel) => {
                                this.sharedAlertService.showToast("Successfully sent activation email");
                                this.eventService.getEntityUpdatedSubject('User').next(user);
                            });
                    },
                },
                {
                    text: 'No',
                    handler: (): any => { },
                },
            ],
        });
    }

    public async resendActivation(user: UserResourceModel): Promise<void> {
        if (!user.email) {
            await this.sharedAlertService.showToast("Please set the email address for this user "
                + "before sending the activation invitation");
            return Promise.reject();
        }
        await this.sharedAlertService.showWithActionHandler({
            header: 'Resend activation email',
            subHeader: 'Would you like to resend the account activation invitation email to this user?',
            buttons: [
                {
                    text: 'Cancel',
                    handler: (): any => { },
                },
                {
                    text: 'Resend',
                    handler: async (): Promise<void> => {
                        try {
                            await this.invitationApiService
                                .sendActivationForPerson(user.personId, user.tenantId).toPromise();
                            await this.sharedAlertService.showToast("Successfully resent activation email");
                            this.eventService.getEntityUpdatedSubject('User').next(user);
                        } catch (error) {
                            return Promise.reject(error);
                        }
                    },
                },
            ],
        });
    }

    public async enableAccount(user: UserViewModel): Promise<UserResourceModel> {
        try {
            await this.sharedLoaderService.presentWait();
            let userResponse: UserResourceModel = await this.userApiService.enable(user.id, user.tenantId).toPromise();
            await this.sharedAlertService
                .showToast('The user is once again able to sign into the portal using their account.');
            this.eventService.getEntityUpdatedSubject('User').next(userResponse);
            return userResponse;
        } finally {
            this.sharedLoaderService.dismiss();
        }
    }

    public async disableAccount(user: UserViewModel): Promise<UserResourceModel> {
        try {
            await this.sharedLoaderService.presentWait();
            const userResponse: UserResourceModel =
                await this.userApiService.disable(user.id, user.tenantId).toPromise();
            await this.sharedAlertService
                .showToast('The user is no longer able to sign into the portal while their account is disabled.');
            this.eventService.getEntityUpdatedSubject('User').next(userResponse);
            return userResponse;
        } finally {
            this.sharedLoaderService.dismiss();
        }
    }

    public async unlinkIdentity(user: UserViewModel, authenticationMethodId: string): Promise<UserResourceModel> {
        try {
            await this.sharedLoaderService.presentWait();
            const userResponse: UserResourceModel =
                await this.userApiService.unlinkIdentity(user.id, authenticationMethodId, user.tenantId).toPromise();
            await this.sharedAlertService.showToast('Successfully unlinked identity');
            this.eventService.getEntityUpdatedSubject('User').next(userResponse);
            return userResponse;
        } finally {
            this.sharedLoaderService.dismiss();
        }
    }
}
