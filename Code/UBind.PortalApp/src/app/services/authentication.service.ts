import { Injectable } from '@angular/core';
import { JwtHelperService } from '@auth0/angular-jwt';
import { Permission } from '@app/helpers';
import { storageHelper } from '@app/helpers/storage.helper';
import { UserType } from '@app/models/user-type.enum';
import { Result } from '../helpers/result';
import { ResilientStorage } from '@app/storage/resilient-storage';
import { AppConfigService } from './app-config.service';
import { AppConfig } from '@app/models/app-config';
import { AccountLoginCredentialsResourceModel } from '@app/resource-models/account-login-credentials.resource-model';
import { AccountApiService } from './api/account-api.service';
import { UserAuthorisationModel } from '@app/resource-models/authentication-response.resource-model';
import { EventService } from './event.service';
import { ProductFeatureSettingService } from './product-feature-service';
import { SessionDataManager } from '@app/storage/session-data-manager';
import { UserResourceModel } from '@app/resource-models/user/user.resource-model';
import { AppStartupService } from './app-startup.service';
import { Errors } from '@app/models/errors';
import { AuthenticationMethodType } from '@app/models/authentication-method-type.enum';
import { RoutePaths } from '@app/helpers/route-path';
import { RedirectService } from './redirect.service';
import { OrganisationModel } from '@app/models/organisation.model';

/**
 * This service manages user login sessions and assocaited data.
 */
@Injectable({ providedIn: 'root' })
export class AuthenticationService {

    private appConfig: AppConfig;
    private appConfigTenantId: string;
    private appConfigTenantAlias: string;
    private appConfigOrganisationId: string;
    private appConfigOrganisationAlias: string;
    private appConfigPortalAlias: string;
    private isMutual: boolean;
    private sessionExpiryTimestamp: number = -1;
    private sessionExpiryTimeoutId: NodeJS.Timeout;
    private apiBaseUrl: string;
    private portalId: string;

    private portalOrganisationAlias: string;
    private portalOrganisationId: string;
    private originalAppContextOrganisationAlias: string;

    public constructor(
        private accountApiService: AccountApiService,
        private storage: ResilientStorage,
        private jwtHelper: JwtHelperService,
        private eventService: EventService,
        private appConfigService: AppConfigService,
        private appStartupService: AppStartupService,
        private productFeatureService: ProductFeatureSettingService,
        private sessionDataManager: SessionDataManager,
        private redirectService: RedirectService,
    ) {
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.appConfig = appConfig;
            this.appConfigTenantId = appConfig.portal.tenantId;
            this.appConfigTenantAlias = appConfig.portal.tenantAlias;
            this.appConfigOrganisationId = appConfig.portal.organisationId;
            this.appConfigOrganisationAlias = appConfig.portal.organisationAlias;
            this.appConfigPortalAlias = appConfig.portal.alias;
            this.isMutual = appConfig.portal.isMutual;
            this.apiBaseUrl = appConfig.portal.api.baseUrl;
            this.portalId = appConfig.portal.portalId;
        });

        this.originalAppContextOrganisationAlias = this.appConfigOrganisationAlias;
        this.portalOrganisationId = this.appConfigOrganisationId;
        this.portalOrganisationAlias = this.appConfigOrganisationAlias;

        this.listenForChangesToLoggedInUser();

        // set the initial value if we are already logged in
        const existingUserOrganisationId: string = this.userOrganisationId;
        if (existingUserOrganisationId) {
            const userOrganisation: OrganisationModel = {
                id: existingUserOrganisationId,
                alias: this.userOrganisationAlias,
            };
            this.eventService.performingUserOrganisationChanged(userOrganisation);
        } else {
            const userOrganisation: OrganisationModel = {
                id: this.appConfigOrganisationId,
                alias: this.appConfigOrganisationAlias,
            };
            this.eventService.performingUserOrganisationChanged(userOrganisation);
        }

        if (this.isAuthenticated()) {
            this.eventService.userLoggedIn(this.userId);
        }
    }

    /*
     * Gets the user Id from local storage
     */
    public get userId(): string {
        return this.getTenantOrganisationSessionValue(storageHelper.user.userId);
    }

    /*
     * Gets the user email from local storage
     */
    public get userEmail(): string {
        return this.getTenantOrganisationSessionValue(storageHelper.user.emailAddress) || '';
    }

    /*
     * Gets the user's full name from local storage
     */
    public get userFullName(): string {
        return this.getTenantOrganisationSessionValue(storageHelper.user.fullName) || '';
    }

    /*
     * Gets the user's preferred name from local storage
     */
    public get userPreferredName(): string {
        return this.getTenantOrganisationSessionValue(storageHelper.user.preferredName) || '';
    }

    /*
     * Gets the user type from local storage
     */
    public get userType(): UserType {
        return this.getTenantOrganisationSessionValue<UserType>(storageHelper.user.userType);
    }

    /*
     * Gets the permissions from local storage
     */
    public get permissions(): Array<Permission> {
        const permissions: Array<Permission>
            = this.getTenantOrganisationSessionValue<Array<Permission>>(storageHelper.user.permissions);
        return permissions;
    }

    /*
     * Gets the user's customer ID from local storage
     */
    public get customerId(): string {
        return this.getTenantOrganisationSessionValue(storageHelper.user.customerId);
    }

    /**
     * Gets the user's tenant alias from local storage
     */
    public get tenantAlias(): string {
        return this.getTenantOrganisationSessionValue(storageHelper.user.tenantAlias);
    }

    /**
     * Gets the user's tenantID from local storage
     */
    public get tenantId(): string {
        return this.getTenantOrganisationSessionValue(storageHelper.user.tenantId);
    }

    /**
     * Gets the user's organisation Id from local storage
     */
    public get userOrganisationId(): string {
        return this.getTenantOrganisationSessionValue(storageHelper.user.organisationId);
    }

    /**
     * Gets the user's organisation alias from local storage
     */
    public get userOrganisationAlias(): string {
        return this.getTenantOrganisationSessionValue(storageHelper.user.organisationAlias);
    }

    /**
     * Gets the profile picture id from local storage
     */
    public get profilePictureId(): string {
        const idVal: string = this.getTenantOrganisationSessionValue(storageHelper.user.profilePictureId);

        if (idVal === "null") {
            return null;
        }

        return idVal;
    }

    /**
     * Sets the user's profile picture id into local storage
     */
    public set profilePictureId(val: string) {
        this.updateTenantOrganisationSessionValue(storageHelper.user.profilePictureId, val);
    }

    public get accessToken(): string {
        return this.getTenantOrganisationSessionValue(storageHelper.user.accessToken);
    }

    public get userPortalId(): string {
        return this.getTenantOrganisationSessionValue(storageHelper.user.portalId);
    }

    public get authenticationMethodId(): string {
        return this.getTenantOrganisationSessionValue(storageHelper.user.authenticationMethodId);
    }

    public get authenticationMethodType(): AuthenticationMethodType {
        return this.getTenantOrganisationSessionValue(storageHelper.user.authenticationMethodType);
    }

    public get supportsSingleLogout(): boolean {
        return this.getTenantOrganisationSessionValue(storageHelper.user.supportsSingleLogout);
    }

    public async login(credentials: AccountLoginCredentialsResourceModel): Promise<Result<void, any>> {
        try {
            const authDetails: UserAuthorisationModel = await this.accountApiService
                .login(credentials).toPromise();
            this.processAuthorisationModel(authDetails);
            const organisationAlias: string = authDetails.portalOrganisationAlias ?? authDetails.organisationAlias;
            if (organisationAlias != this.appConfigOrganisationAlias) {
                await this.appStartupService
                    .loadPortalAppContext(
                        authDetails.tenantAlias, authDetails.organisationAlias, this.appConfigPortalAlias);
            }
            this.setupSessionExpiry();
            this.eventService.userLoggedIn(authDetails.userId);
            return Promise.resolve(Result.ok());
        } catch (error) {
            return Promise.resolve(Result.fail(error));
        }
    }

    public async loginWithJwt(jwt: string): Promise<void> {
        // check if we already have this jwt in local storage
        if (this.accessToken === jwt) {
            console.log('JWT already in local storage, skipping login.');
            return;
        }

        // write the new JWT to storage
        this.updateTenantOrganisationSessionValue(storageHelper.user.accessToken, jwt);

        let authDetails: UserAuthorisationModel = await this.accountApiService
            .getAuthorisationModel().toPromise();
        authDetails.accessToken = jwt;
        this.processAuthorisationModel(authDetails);
        this.setupSessionExpiry();
        this.eventService.userLoggedIn(authDetails.userId);
    }

    public async logout(): Promise<boolean> {
        let redirectUrl: string;
        if (this.authenticationMethodType === AuthenticationMethodType.Saml
            && this.supportsSingleLogout
        ) {
            redirectUrl = this.generateSamlSignoutUrl(this.authenticationMethodId);
        } else {
            await this.accountApiService.logout().toPromise();
        }
        this.removeUserData();
        this.eventService.userLoggedOut();
        if (redirectUrl) {
            this.redirectService.redirectToUrl(redirectUrl);
            return true;
        }

        return false;
    }

    public checkIfCurrentEmail(email: string): boolean {
        const currentEmail: string = this.getTenantOrganisationSessionValue(storageHelper.user.emailAddress);
        return email == currentEmail;
    }

    public updateEmail(email: string): void {
        this.updateTenantOrganisationSessionValue(storageHelper.user.emailAddress, email);
    }

    public updateFullname(name: string): void {
        this.updateTenantOrganisationSessionValue(storageHelper.user.fullName, name);
    }

    public isAuthenticated(): boolean {
        if (!this.userId) {
            return false;
        }

        const tenancyAliasChanged: boolean
            = this.appConfigTenantAlias != null && this.tenantAlias != this.appConfigTenantAlias;
        const tenantIdChanged: boolean = this.tenantId != null && this.appConfigTenantId != this.tenantId;
        const organisationIdChanged: boolean
            = this.appConfigOrganisationId != null
            && (this.appConfigOrganisationId != this.userOrganisationId
                && this.appConfigOrganisationId != this.portalOrganisationId);
        const organisationAliasChanged: boolean
            = this.appConfigOrganisationAlias != null
            && (this.appConfigOrganisationAlias != this.userOrganisationAlias
                && this.appConfigOrganisationAlias != this.portalOrganisationAlias);
        if (tenancyAliasChanged || tenantIdChanged || organisationIdChanged || organisationAliasChanged) {
            return false;
        }

        const expiresAt: any = JSON.parse(this.getTenantOrganisationSessionValue(
            storageHelper.user.expiryTime) || '{}');
        const nowInSeconds: number = Date.now() / 1000;
        const sessionExpired: boolean = nowInSeconds > expiresAt;
        if (sessionExpired) {
            return false;
        }

        return true;
    }

    public setupSessionExpiry(): void {
        this.sessionExpiryTimestamp = JSON.parse(
            this.getTenantOrganisationSessionValue(storageHelper.user.expiryTime) || '{}');

        if (!Number.isNaN(this.sessionExpiryTimestamp) && this.sessionExpiryTimestamp > 0) {
            const timeoutLengthMillis: number = (this.sessionExpiryTimestamp - Date.now() / 1000) * 1000;
            clearTimeout(this.sessionExpiryTimeoutId);
            this.sessionExpiryTimeoutId = setTimeout(() => this.checkAndExpireSession(), timeoutLengthMillis);
        }
    }

    private checkAndExpireSession(): void {
        this.sessionExpiryTimestamp = JSON.parse(this.getTenantOrganisationSessionValue(
            storageHelper.user.expiryTime) || '{}');

        const expiresAt: any = JSON.parse(this.getTenantOrganisationSessionValue(
            storageHelper.user.expiryTime) || '{}');
        const nowInSeconds: number = Date.now() / 1000;
        const sessionExpired: boolean = nowInSeconds > expiresAt;
        if (sessionExpired) {
            this.logout();
        }
    }

    public isCustomer(): boolean {
        const userType: string = this.getTenantOrganisationSessionValue(storageHelper.user.userType);
        return userType === UserType.Customer;
    }

    public isAgent(): boolean {
        const userType: string = this.getTenantOrganisationSessionValue(storageHelper.user.userType);
        return userType === UserType.Client;
    }

    public isMasterUser(): boolean {
        const userType: string = this.getTenantOrganisationSessionValue(storageHelper.user.userType);
        return userType === UserType.Master;
    }

    public isAgentOrMasterUser(): boolean {
        const userType: string = this.getTenantOrganisationSessionValue(storageHelper.user.userType);
        return userType === UserType.Client || userType === UserType.Master;
    }

    public isMutualTenant(): boolean {
        return this.isMutual;
    }

    public processAuthorisationModel(response: UserAuthorisationModel): void {
        const decodedToken: any = this.jwtHelper.decodeToken(response.accessToken);
        const permissions: Array<Permission> = response.permissions || [];
        if (response.userType != UserType.Master) {
            this.throwIfUserCannotAccessAnyEnvironment(permissions);
        }

        // if the user has no portal, then we can't let them stay logged in.
        if (response.tenantAlias != 'ubind' && response.tenantAlias != 'master' && !response.portalId) {
            throw Errors.User.NoPortal();
        }

        let userSessionData: any = {
            [storageHelper.user.accessToken]: response.accessToken || '',
            [storageHelper.user.expiryTime]: decodedToken.exp || '',
            [storageHelper.user.userId]: response.userId || '',
            [storageHelper.user.customerId]: response.customerId || '',
            [storageHelper.user.emailAddress]: response.emailAddress || '',
            [storageHelper.user.fullName]: response.fullName || '',
            [storageHelper.user.preferredName]: response.preferredName || '',
            [storageHelper.user.userType]: response.userType || '',
            [storageHelper.user.tenantId]: decodedToken.Tenant || '',
            [storageHelper.user.tenantAlias]: response.tenantAlias || '',
            [storageHelper.user.organisationId]: response.organisationId || '',
            [storageHelper.user.organisationAlias]: response.organisationAlias || '',
            [storageHelper.user.profilePictureId]: response.profilePictureId,
            [storageHelper.user.portalId]: response.portalId,
            [storageHelper.user.authenticationMethodId]: response.authenticationMethodId,
            [storageHelper.user.authenticationMethodType]: response.authenticationMethodType,
            [storageHelper.user.supportsSingleLogout]: response.supportsSingleLogout,
        };

        // If the user tried to log into an org, but they are required to login to a portal from a different org
        // (e.g. the default org) then we need to update the organisation id and alias for this session.
        // The user would typically be redirected to that portal after login, and this ensures they won't
        // have to login again.
        this.portalOrganisationId = response.portalOrganisationId ?? response.organisationId;
        this.portalOrganisationAlias = response.portalOrganisationAlias ?? response.organisationAlias;
        if (this.appConfig.portal.organisationId != this.portalOrganisationId) {
            this.appConfigService.updatePortalOrganistionDuringLogin(
                this.portalOrganisationId,
                this.portalOrganisationAlias);
        }
        const userOrganisationModel: OrganisationModel = {
            id: response.organisationId,
            alias: response.organisationAlias,
        };
        this.eventService.performingUserOrganisationChanged(userOrganisationModel);

        userSessionData[storageHelper.user.permissions] = permissions || '';
        const userSessionDataJson: string = JSON.stringify(userSessionData);

        // Note, it's important that we store the user session data keyed by the portalOrganisationId and not
        // the performingUser's actual organisation ID. This is because a user can log into a portal which is
        // from a parent organisation. Since we need to resume their session if they reload the page, we need
        // to look up their session data by the portalOrganisationId, since we don't actaully know their real
        // organisation ID until after we've retrieved their session data.
        this.storage.setItem(response.tenantAlias + " - " + this.portalOrganisationAlias, userSessionDataJson);
        if (this.portalOrganisationAlias != this.originalAppContextOrganisationAlias) {
            // we also need to store the users session data keyed by the appConfigOrganisationId which is the
            // one returned from the PortalAppContext call, because if someone reloads the browser window we don't
            // have the users portalOrganisationId from the UserAuthorisationModel yet. To be able to continue their
            // session, we'll need to find it keyed by the original app context organisation ID.
            this.storage.setItem(
                response.tenantAlias + " - " + this.originalAppContextOrganisationAlias, userSessionDataJson);
        }

        // Sanity check that the values we just set can be accessed
        if (!this.userId) {
            throw Errors.User.BrowserStorageNotWorking();
        }
    }

    private throwIfUserCannotAccessAnyEnvironment(
        permissions: Array<Permission>,
    ): void {
        if (!permissions.includes(Permission.AccessProductionData)
            && !permissions.includes(Permission.AccessStagingData)
            && !permissions.includes(Permission.AccessDevelopmentData)
        ) {
            throw Errors.User.CannotAccessAnyEnvironment();
        }
    }

    public removeUserData(): void {
        this.storage.removeItem(this.appConfigTenantAlias + " - " + this.portalOrganisationAlias);
        this.storage.removeItem(this.appConfigTenantAlias + " - " + this.originalAppContextOrganisationAlias);
        this.productFeatureService.clearProductFeatureSettings();
    }

    private getTenantOrganisationSessionValue<T>(key: string): T {
        return this.sessionDataManager.getTenantOrganisationSessionValue<T>(
            key,
            this.appConfigTenantAlias,
            this.portalOrganisationAlias);
    }

    private updateTenantOrganisationSessionValue<T>(key: string, newValue: T): void {
        this.sessionDataManager.updateTenantOrganisationSessionValue(
            key,
            newValue,
            this.appConfigTenantAlias,
            this.portalOrganisationAlias);
    }

    private listenForChangesToLoggedInUser(): void {
        this.eventService.getEntityUpdatedSubject('User').subscribe((user: UserResourceModel) => {
            if (this.userId == user.id) {
                this.updateFullname(user.fullName);
                this.updateEmail(user.email);
            }
        });

        if (this.isAuthenticated()) {
            this.eventService.userLoggedIn(this.userId);
            const userOrganisationModel: OrganisationModel = {
                id: this.userOrganisationId,
                alias: this.userOrganisationAlias,
            };
            this.eventService.performingUserOrganisationChanged(userOrganisationModel);
        }
    }

    private generateSamlSignoutUrl(authenticationMethodId: string): string {
        let signoutUrl: string = `${this.apiBaseUrl}tenant/${this.tenantId}/saml/`
            + `${authenticationMethodId}/initiate-single-logout`;
        if (this.portalId) {
            signoutUrl = `${signoutUrl}?portalId=${this.portalId}`;
        }
        const loginPath: string = RoutePaths.login;
        signoutUrl = signoutUrl.indexOf('?') === -1
            ? `${signoutUrl}?path=${loginPath}`
            : `${signoutUrl}&path=${loginPath}`;
        signoutUrl += `&environment=${this.appConfig.portal.environment}`;
        return signoutUrl;
    }
}
