import { Injectable } from "@angular/core";
import { Errors } from "@app/models/errors";
import { finalize } from "rxjs/operators";
import { ErrorHandlerService } from "./error-handler.service";
import { EventService } from "./event.service";
import { SharedAlertService } from "./shared-alert.service";
import { PersonApiService } from "./api/person-api.service";
import { PersonResourceModel } from "@app/models";
import { InvitationApiService } from "./api/invitation-api.service";
import { CustomerApiService } from "./api/customer-api.service";
import { NavProxyService } from "./nav-proxy.service";
import { OverlayEventDetail } from "@ionic/core";
import { CustomerResourceModel } from "@app/resource-models/customer.resource-model";
import { UserTypePathHelper } from "@app/helpers/user-type-path.helper";

/**
 * This class handles all person related api requests and other services calls.
 * This is created as this should not contain direct http utilisation/call,
 * instead the person-api.service is responsible for it.
 */
@Injectable({ providedIn: 'root' })
export class PersonService {

    public constructor(
        private personApiService: PersonApiService,
        private customerApiService: CustomerApiService,
        private sharedAlertService: SharedAlertService,
        private eventService: EventService,
        private errorHandlerService: ErrorHandlerService,
        private invitationService: InvitationApiService,
        public navProxy: NavProxyService,
        protected userPath: UserTypePathHelper,
    ) {
    }

    public async confirmCreateUserAccount(emailAddress: string, displayName: string): Promise<boolean> {
        return new Promise(async (resolve: any, reject: any): Promise<void> => {
            (await this.sharedAlertService.showWithActionHandler({
                header: 'Create user account',
                subHeader:
                    `An email will be sent to ${displayName} with a link to activate their user account. ` +
                    `Would you like to proceed?`,
                buttons: [
                    { text: 'No', handler: (): void => resolve(false) },
                    { text: 'Yes', handler: (): void => resolve(true) },
                ],
            })).onDidDismiss().then((data: OverlayEventDetail) => {
                if (!data.data) {
                    resolve(false);
                }
            });
        });
    }

    public async isValidEmailAddress(email: string): Promise<boolean> {
        return new Promise((resolve: any): void => {
            if (!email) {
                this.errorHandlerService.handleError(Errors.Customer.CreateUserAccount.MissingEmailAddress());
                resolve(false);
            }
            resolve(true);
        });
    }

    public async confirmDeletePerson(model: PersonResourceModel): Promise<boolean> {
        let displayName: string = model.fullName;

        return new Promise(async (resolve: any): Promise<void> => {
            let primaryPerson: PersonResourceModel
                = await this.customerApiService.getPrimaryPerson(model.customerId).toPromise();
            const alertMessage: any = {
                active: `${displayName} has an active user account. If you delete this person they will no longer be `
                    + `able to access the portal using their account. This action cannot be undone. `
                    + `Would you like to proceed?`,
                invited: `${displayName} has been sent an email with a link inviting them to activate their user `
                    + `account. If you delete ${displayName} then they will be unable to use the link to activate `
                    + `their user account. This action cannot be undone. Are you sure you wish to proceed?`,
                isPrimary: `${displayName} cannot be deleted because they are the primary person for this customer. `
                    + `Before you can delete ${displayName} you must make a different person the primary person for `
                    + `this customer.`,
                default: "This action cannot be undone. Are you sure you wish to proceed?",
            };

            if (primaryPerson.id == model.id) {
                this.sharedAlertService.showWithOk('Delete person', alertMessage['isPrimary']);
                resolve(false);
            } else {
                (await this.sharedAlertService.showWithActionHandler({
                    header: 'Delete person',
                    subHeader: alertMessage[status.toLowerCase()] || alertMessage['default'],
                    buttons: [
                        {
                            text: 'No', handler: (): void => {
                                resolve(false);
                            },
                        },
                        {
                            text: 'Yes', handler: (): void => {
                                resolve(true);
                            },
                        },
                    ],
                })).onDidDismiss().then((data: OverlayEventDetail) => {
                    if (!data.data) {
                        resolve(false);
                    }
                });
            }
        });
    }

    public async confirmDisableUserAccount(status: string, displayName: string): Promise<boolean> {
        return new Promise(async (resolve: any): Promise<void> => {
            const alertMessage: any = {
                invited: `${displayName} has been sent an email with a link inviting them to activate their user `
                    + `account. If you disable the user account for ${displayName} then they will be unable to use `
                    + `the link to activate their user account. Are you sure you wish to proceed?`,
                default: `If you disable the user account for ${displayName} they will no longer be able to access `
                    + `the portal using their account. Would you like to proceed?`,
            };
            (await this.sharedAlertService.showWithActionHandler({
                header: 'Disable user account',
                subHeader: alertMessage[status.toLowerCase()] || alertMessage['default'],
                buttons: [
                    {
                        text: 'No', handler: (): void => {
                            resolve(false);
                        },
                    },
                    {
                        text: 'Yes', handler: (): void => {
                            resolve(true);
                        },
                    },
                ],
            })).onDidDismiss().then((data: OverlayEventDetail) => {
                if (!data.data) {
                    resolve(false);
                }
            });
        });
    }

    public createUserAccount(
        personId: string,
        displayName: string,
        updateSubjects: Array<string> = [],
    ): Promise<void> {
        return new Promise(async (resolve: any): Promise<void> => {
            this.personApiService.createAccount(personId)
                .pipe(finalize(() => resolve()))
                .subscribe((person: any) => {
                    this.sharedAlertService.showToast(
                        `An email has been sent to ${displayName} with a link to activate their user account`,
                    );
                    updateSubjects
                        .forEach((entity: any) => this.eventService.getEntityUpdatedSubject(entity).next(person));
                });
        });
    }

    public resendActivation(personId: string, displayName: string): Promise<void> {
        return new Promise((resolve: any): void => {
            this.invitationService
                .sendAccountActivationForPerson(personId)
                .pipe(finalize(() => resolve()))
                .subscribe(() => {
                    this.sharedAlertService.showToast(
                        `A new email has been sent to ${displayName} with a link to activate their user account`,
                    );
                });
        });
    }

    public async disable(
        personId: string,
        displayName: string,
        updateSubjects: Array<string> = [],
    ): Promise<CustomerResourceModel> {
        const person: CustomerResourceModel = await this.personApiService.deactivateAccount(personId).toPromise();
        this.sharedAlertService.showToast(`The user account for ${displayName} has been disabled`);
        updateSubjects
            .forEach((entity: any) => this.eventService.getEntityUpdatedSubject(entity).next(person));
        return person;
    }

    public async enable(
        personId: string,
        displayName: string,
        updateSubjects: Array<string> = [],
    ): Promise<CustomerResourceModel> {
        const person: CustomerResourceModel = await this.personApiService.activateAccount(personId).toPromise();
        this.sharedAlertService.showToast(`The user account for ${displayName} has been enabled`);
        updateSubjects
            .forEach((entity: any) => this.eventService.getEntityUpdatedSubject(entity).next(person));
        return person;
    }

    public edit(personModel: PersonResourceModel): Promise<void> {
        return new Promise(async (resolve: any): Promise<void> => {
            this.navProxy
                .navigate([
                    this.userPath.customer,
                    personModel.customerId,
                    this.userPath.person,
                    personModel.id,
                    'edit']);
            resolve();
        });
    }

    public delete(id: string, personModel: PersonResourceModel, updateSubjects: Array<string> = []): Promise<void> {
        return new Promise(async (resolve: any): Promise<void> => {
            this.personApiService
                .delete(id)
                .pipe(finalize(() => resolve()))
                .subscribe(() => {
                    this.sharedAlertService.showToast(`${personModel.fullName} has been deleted`);
                    personModel['fromEvent'] = 'deleted';
                    updateSubjects
                        .forEach((entity: any) => this.eventService.getEntityUpdatedSubject(entity).next(personModel));
                });
        });
    }

    public setToPrimary(personId: string, personModel: PersonResourceModel): Promise<void> {
        return new Promise((resolve: any): void => {
            this.customerApiService
                .setPersonToPrimaryForCustomer(personModel.customerId, personId)
                .pipe(finalize(() => resolve()))
                .subscribe((person: any) => {
                    this.sharedAlertService.showToast(
                        `${person.fullName} has been set as the primary person for this customer`,
                    );
                    person['fromEvent'] = 'setToPrimary';
                    ['Customer', 'Person']
                        .forEach((entity: any) => this.eventService.getEntityUpdatedSubject(entity).next(person));
                });
        });
    }
}
