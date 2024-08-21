import { Injectable } from '@angular/core';
import { ApplicationService } from './application.service';
import { SessionDataManager } from '@app/storage/session-data-manager';
import { storageHelper } from '@app/helpers/storage.helper';
import { UserType } from '@app/models/user-type.enum';

/**
 * Export user service class.
 * TODO: Write a better class header: user service functions.
 */
@Injectable()
export class UserService {

    private _userType: string = '';
    private _accessToken: string = '';
    private _permissions: Array<string> = [];
    private _tenantAlias: string = '';
    private _isLoadedCustomerHasUser: boolean;

    public constructor(
        private sessionDataManager: SessionDataManager,
        private applicationService: ApplicationService,
    ) {
    }

    // retrieve data from storage if same tenant only.
    public retrieveLoggedInUserData(): void {
        const organisationAlias: string = this.applicationService.portalOrganisationAlias;
        this._tenantAlias = (this.sessionDataManager.getTenantOrganisationSessionValue(
            storageHelper.user.tenantAlias,
            this.applicationService.tenantAlias,
            organisationAlias) || '') as string;
        if (this.applicationService
            && (this.applicationService.tenantAlias || '').toLowerCase() == this._tenantAlias.toLowerCase()) {
            this._accessToken = (this.sessionDataManager.getTenantOrganisationSessionValue(
                storageHelper.user.accessToken,
                this.applicationService.tenantAlias,
                organisationAlias)) as string;
            this._userType = (this.sessionDataManager.getTenantOrganisationSessionValue(
                storageHelper.user.userType,
                this.applicationService.tenantAlias,
                organisationAlias)) as string;
            this._userType = !this._userType ? UserType.Customer : this._userType.replace(/"/g, '');
            this._permissions = this.sessionDataManager.getTenantOrganisationSessionValue<Array<string>>(
                storageHelper.user.permissions,
                this.applicationService.tenantAlias,
                organisationAlias);
            if (this._permissions == null) {
                this._permissions = [];
            }
        } else {
            this._userType = UserType.Customer;
        }

        if (this.applicationService.debug) {
            console.log("current user type: ", this._userType);
        }
    }

    public get isCustomer(): boolean {
        return this._userType == UserType.Customer;
    }

    public get isCustomerOrClientLoggedIn(): boolean {
        return this.isClientLoggedIn || this.isCustomerLoggedIn;
    }

    public get isCustomerLoggedIn(): boolean {
        return this._accessToken
            ? this._userType == UserType.Customer
            : false;
    }

    public get isClientLoggedIn(): boolean {
        return this._accessToken
            ? this._userType == UserType.Client
            : false;
    }

    public get isLoadedCustomerHasUser(): boolean {
        return this._isLoadedCustomerHasUser;
    }

    public set isLoadedCustomerHasUser(isLoadedCustomerHasUser: boolean) {
        this._isLoadedCustomerHasUser = isLoadedCustomerHasUser;
    }

    public get userType(): string {
        return this._userType;
    }

    public get permissions(): Array<string> {
        return this._permissions;
    }
}
