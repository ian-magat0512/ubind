import { TestBed } from '@angular/core/testing';
import { Router, UrlTree } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { EnvironmentGuard } from './environment.guard';
import { PermissionService } from '@app/services/permission.service';
import { DeploymentEnvironment } from '@app/models/deployment-environment.enum';
import { AppConfigService } from '@app/services/app-config.service';
import { AppConfig } from '@app/models/app-config';
import { BehaviorSubject } from 'rxjs';

describe('EnvironmentGuard', () => {
    let guard: EnvironmentGuard;
    let router: Router;
    let route: ActivatedRouteSnapshot;
    let state: RouterStateSnapshot;
    let permissionService: PermissionService;
    let appConfigService: AppConfigService;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [RouterTestingModule],
            providers: [
                EnvironmentGuard,
                { provide: PermissionService, useValue: new PermissionService(null, null) },
                AppConfigService,
            ],
        });

        permissionService = TestBed.get(PermissionService);
        guard = TestBed.get(EnvironmentGuard);
        router = TestBed.get(Router);
        route = new ActivatedRouteSnapshot();
        state = jasmine.createSpyObj<RouterStateSnapshot>('RouterStateSnapshot', ['toString']);
        appConfigService = TestBed.get(AppConfigService);
        appConfigService.currentConfig = <AppConfig>{
            portal: {
                environment: DeploymentEnvironment.Production,
                baseUrl: 'https://test.domain/',
                api: {
                    baseUrl: 'https://test.domain/',
                },
            },
        };
        appConfigService.appConfigSubject = new BehaviorSubject<AppConfig>(appConfigService.currentConfig);
    });

    it('should redirect them to another environment if they don\'t have access to production', () => {
        // Arrange
        state.url = '/carl/carl/portal1/path/quote/list';
        const expectedRedirectUrl: string = '/carl/carl/portal1/path/quote/list?environment=Staging';
        spyOn(permissionService, 'canAccessEnvironment').and.returnValue(false);
        spyOn(permissionService, 'getAvailableEnvironments').and.returnValue([DeploymentEnvironment.Staging]);

        // Act
        const result: boolean | UrlTree = guard.canActivate(route, state);

        // Assert
        expect(result).not.toBe(true);
        expect(result.constructor.name).toBe('UrlTree');
        expect(result.toString()).toBe(expectedRedirectUrl);
    });

    it('should redirect them to Production environment if they don\'t have access to others', () => {
        // Arrange
        state.url = '/carl/carl/portal1/path/quote/list?environment=Staging';
        const expectedRedirectUrl: string = '/carl/carl/portal1/path/quote/list';
        spyOn(permissionService, 'canAccessEnvironment').and.returnValue(false);
        spyOn(permissionService, 'getAvailableEnvironments').and.returnValue([DeploymentEnvironment.Production]);

        // Act
        const result: boolean | UrlTree = guard.canActivate(route, state);

        // Assert
        expect(result).not.toBe(true);
        expect(result.constructor.name).toBe('UrlTree');
        expect(result.toString()).toBe(expectedRedirectUrl);
    });

    it('should not redirect them if they don\'t have access to any environment', () => {
        // Arrange
        state.url = '/carl/carl/portal1/path/quote/list';
        spyOn(router, 'navigateByUrl');
        spyOn(permissionService, 'canAccessEnvironment').and.returnValue(false);
        spyOn(permissionService, 'getAvailableEnvironments').and.returnValue([]);

        // Act
        const result: boolean | UrlTree = guard.canActivate(route, state);

        // Assert
        expect(result).toBe(true);
    });

    it('should remove environment from the url if it\'s production', () => {
        // Arrange
        state.url = '/carl/carl/portal1/path/quote/list?environment=Production';
        const expectedRedirectUrl: string = '/carl/carl/portal1/path/quote/list';
        spyOn(permissionService, 'canAccessEnvironment').and.returnValue(true);
        spyOn(permissionService, 'getAvailableEnvironments').and.returnValue([DeploymentEnvironment.Production]);

        // Act
        const result: boolean | UrlTree = guard.canActivate(route, state);

        // Assert
        expect(result).not.toBe(true);
        expect(result.constructor.name).toBe('UrlTree');
        expect(result.toString()).toBe(expectedRedirectUrl);
    });

});
