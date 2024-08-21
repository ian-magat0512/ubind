import { TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { AuthenticationGuard } from './authentication.guard';
import { AuthenticationService } from '@app/services/authentication.service';
import { AppConfigService } from '@app/services/app-config.service';
import { PortalUserType } from '@app/models/portal-user-type.enum';
import { UserType } from '@app/models/user-type.enum';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { LoginRedirectService } from '@app/services/login-redirect.service';
import { PermissionService } from '@app/services/permission.service';
import { BehaviorSubject } from 'rxjs';
import { AppConfig } from '@app/models/app-config';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { SharedAlertService } from '@app/services/shared-alert.service';

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
}

describe('AuthenticationGuard', () => {
    let guard: AuthenticationGuard;
    let route: ActivatedRouteSnapshot;
    let state: RouterStateSnapshot;
    let isAgent: boolean = false;
    let appConfigService: FakeAppConfigService;
    let authService: any;
    let errorHandlerService: ErrorHandlerService;

    let fakeAuthenticationService: any = {
        isAuthenticated: (): boolean => true,
        userPortalId: 2,
        userOrganisationId: 2,
        isAgent: (): boolean => isAgent,
    };

    let sharedAlertServiceStub: any = {
        showToast: (): void => { },
        closeToast: (): void => { },
        showWithOk: (): void => { },
        showWithCustomButton: (): void => { },
        showError: (): void => { },
        showErrorWithCustomButtonLabel: (): void => { },
        showErrorWithCustomButtons: (): void => { },
        showWithActionHandler: (): void => { },
    };

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [RouterTestingModule],
            providers: [
                AuthenticationGuard,
                { provide: AuthenticationService, useValue: fakeAuthenticationService },
                { provide: AppConfigService, useClass: FakeAppConfigService },
                { provide: PermissionService, useValue: new PermissionService(null, null) },
                { provide: SharedAlertService, useValue: sharedAlertServiceStub },
                ErrorHandlerService,
                UserTypePathHelper,
                NavProxyService,
                LoginRedirectService,
            ],
        });

        guard = TestBed.inject(AuthenticationGuard);
        route = new ActivatedRouteSnapshot();
        state = jasmine.createSpyObj<RouterStateSnapshot>('RouterStateSnapshot', ['toString']);
        appConfigService = TestBed.inject(AppConfigService);
        authService = TestBed.inject(AuthenticationService);
        errorHandlerService = TestBed.inject(ErrorHandlerService);
        spyOn(errorHandlerService, 'handleError');
    });

    it('trying to access a customer portal as an agent should be denied', async () => {
        // Arrange
        state.url = '/test/test/customer/path/quote/list';
        isAgent = true;
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

    it('trying to access a the default tenant agent portal as a customer should be allowed', async () => {
        // Arrange
        state.url = '/test/path/quote/list';
        isAgent = false;
        let currentConfig: any = appConfigService.currentConfig;
        currentConfig.portal.isDefaultOrganisation = true;
        currentConfig.portal.organisationId = 1;
        currentConfig.portal.portalId = 1;
        currentConfig.portal.portalUserType = PortalUserType.Agent;
        authService.userPortalId = null;
        authService.userOrganisationId = 2;
        authService.userType = UserType.Customer;

        // Act
        const result: boolean | UrlTree = await guard.canActivate(route, state);

        // Assert
        expect(result).toBe(true);
    });

    it('trying to access the default tenant customer portal as a customer from another organisation '
        + 'should be allowed', async () => {
        // Arrange
        state.url = '/test/test/customer/path/quote/list';
        isAgent = false;
        let currentConfig: any = appConfigService.currentConfig;
        currentConfig.portal.isDefaultOrganisation = true;
        currentConfig.portal.organisationId = 1;
        currentConfig.portal.portalId = 1;
        currentConfig.portal.portalUserType = PortalUserType.Customer;
        authService.userPortalId = null;
        authService.userOrganisationId = 2;
        authService.userType = UserType.Customer;

        // Act
        const result: boolean | UrlTree = await guard.canActivate(route, state);

        // Assert
        expect(result).toBe(true);
    });

    it('trying to access the default customer portal of an organisation as a customer from another organisation '
        + 'should be denied', async () => {
        // Arrange
        state.url = '/test/abc/customer/path/quote/list';
        isAgent = false;
        let currentConfig: any = appConfigService.currentConfig;
        currentConfig.portal.isDefaultOrganisation = false;
        currentConfig.portal.organisationId = 2;
        currentConfig.portal.portalId = 2;
        currentConfig.portal.portalUserType = PortalUserType.Customer;
        authService.userPortalId = null;
        authService.userOrganisationId = 3;
        authService.userType = UserType.Customer;

        // Act
        const result: boolean | UrlTree = await guard.canActivate(route, state);

        // Assert
        expect(result).not.toBe(true);
    });
});
