import { TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { AuthenticationService } from '@app/services/authentication.service';
import { AppConfigService } from '@app/services/app-config.service';
import { PortalUserType } from '@app/models/portal-user-type.enum';
import { UserType } from '@app/models/user-type.enum';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { LoginRedirectService } from '@app/services/login-redirect.service';
import { PermissionService } from '@app/services/permission.service';
import { BehaviorSubject, Observable } from 'rxjs';
import { AppConfig } from '@app/models/app-config';
import { PortalRedirectGuard } from './portal-redirect.guard';
import { AccountApiService } from '@app/services/api/account-api.service';
import { Location } from '@angular/common';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { DeploymentEnvironment } from '@app/models/deployment-environment.enum';

/**
 * Fake app config service for unit test
 */
class FakeAppConfigService {
    public currentConfig: any = {
        portal: {
            portalId: 1,
            organisationId: 1,
            isDefaultOrganisation: true,
            portalUserType: PortalUserType.Agent,
        },
    };

    public appConfigSubject: BehaviorSubject<AppConfig> = new BehaviorSubject<AppConfig>(this.currentConfig);

    public getEnvironment: () => DeploymentEnvironment = (): DeploymentEnvironment => DeploymentEnvironment.Staging;
}

/**
 *  A fake account api Service so we can simulate the API response
 */
class FakeAccountApiService {
    public getPortalUrl(): Observable<string> {
        return new BehaviorSubject<string>(
            'https://app.ubind.com.au/portal/test/test/customer1');
    }
}

describe('PortalRedirectGuard', () => {
    let guard: PortalRedirectGuard;
    let route: ActivatedRouteSnapshot;
    let state: RouterStateSnapshot;
    let isAgent: boolean = false;
    let appConfigService: FakeAppConfigService;
    let authService: any;
    let location: Location;
    let accountApiService: AccountApiService;
    let navProxyService: NavProxyService;
    let errorHandlerService: ErrorHandlerService;

    let fakeAuthenticationService: any = {
        isAuthenticated: (): boolean => true,
        userPortalId: 2,
        userOrganisationId: 2,
        isAgent: (): boolean => isAgent,
        isMasterUser: (): boolean => false,
        logout: (): void => { },
    };

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [RouterTestingModule],
            providers: [
                PortalRedirectGuard,
                { provide: AuthenticationService, useValue: fakeAuthenticationService },
                { provide: AppConfigService, useClass: FakeAppConfigService },
                { provide: PermissionService, useValue: new PermissionService(null, null) },
                { provide: AccountApiService, useClass: FakeAccountApiService },
                ErrorHandlerService,
                UserTypePathHelper,
                NavProxyService,
                LoginRedirectService,
                Location,
            ],
        });

        guard = TestBed.inject(PortalRedirectGuard);
        route = new ActivatedRouteSnapshot();
        state = jasmine.createSpyObj<RouterStateSnapshot>('RouterStateSnapshot', ['toString']);
        appConfigService = TestBed.inject(AppConfigService);
        authService = TestBed.inject(AuthenticationService);
        location = TestBed.inject(Location);
        accountApiService = TestBed.inject(AccountApiService);
        navProxyService = TestBed.inject(NavProxyService);
        spyOn(navProxyService, 'navigateRoot');
        errorHandlerService = TestBed.inject(ErrorHandlerService);
        spyOn(errorHandlerService, 'handleError');
    });

    it('denies the route when the user doesn\'t have any portal allocated', async () => {
        // Arrange
        state.url = '/test/test/customer1/path/quote/list';
        isAgent = false;
        spyOn(guard as any, 'getWindowLocation').and
            .returnValue(new URL('https://app.ubind.com.au/portal/test/test/customer1/path/quote/list'));
        spyOn(location, 'prepareExternalUrl').and
            .returnValue('/portal/test/test/customer1/path/quote/list');
        let currentConfig: any = appConfigService.currentConfig;
        currentConfig.portal.isDefaultOrganisation = true;
        currentConfig.portal.organisationId = 1;
        currentConfig.portal.portalId = 1;
        currentConfig.portal.portalUserType = PortalUserType.Customer;
        authService.userPortalId = null;
        authService.userOrganisationId = 1;
        authService.userType = UserType.Client;

        // Act
        const result: boolean | UrlTree = await guard.canActivate(route, state);

        // Assert
        expect(result).not.toBe(true);
    });

    it('denies the route (and redirects) when user doesn\'t have a portal and their returned portal base URL '
        + 'doesn\'t match', async () => {
        // Arrange
        state.url = '/test/test/customer1/path/quote/list';
        isAgent = true;
        spyOn(guard as any, 'getWindowLocation').and
            .returnValue(new URL('https://app.ubind.com.au/portal/test/test/customer1/path/quote/list'));
        spyOn(location, 'prepareExternalUrl').and
            .returnValue('/portal/test/test/customer1/path/quote/list');
        spyOn(accountApiService, 'getPortalUrl').and
            .returnValue(new BehaviorSubject<string>('https://app.ubind.com.au/portal/test/test/agent1'));
        let currentConfig: any = appConfigService.currentConfig;
        currentConfig.portal.isDefaultOrganisation = true;
        currentConfig.portal.organisationId = 1;
        currentConfig.portal.portalId = 1;
        currentConfig.portal.portalUserType = PortalUserType.Customer;
        authService.userPortalId = null;
        authService.userOrganisationId = 1;
        authService.userType = UserType.Client;

        // Act
        const result: boolean | UrlTree = await guard.canActivate(route, state);

        // Assert
        expect(result).not.toBe(true);
    });
});
