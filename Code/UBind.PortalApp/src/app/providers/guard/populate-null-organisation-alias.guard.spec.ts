import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { AppConfigService } from '@app/services/app-config.service';
import { PopulateNullOrganisationAliasGuard } from './populate-null-organisation-alias.guard';

describe('PopulateNullOrganisationAliasGuard', () => {

    let guard: PopulateNullOrganisationAliasGuard;
    let mockRouter: any;
    let mockAppConfigService: any;
    let routeSnapshot: ActivatedRouteSnapshot;
    let stateSnapshot: RouterStateSnapshot;

    beforeEach(() => {
        mockRouter = {
            parseUrl: jasmine.createSpy('parseUrl').and.callFake((url: string) => new UrlTree()),
        };
        mockAppConfigService = { currentConfig: { portal: { organisationAlias: 'test' } } };
        routeSnapshot = new ActivatedRouteSnapshot();
        stateSnapshot = { url: '/test/null' } as RouterStateSnapshot;
        TestBed.configureTestingModule({
            providers: [
                PopulateNullOrganisationAliasGuard,
                { provide: Router, useValue: mockRouter },
                { provide: AppConfigService, useValue: mockAppConfigService },
            ],
        });
        guard = TestBed.inject(PopulateNullOrganisationAliasGuard);
    });

    const testCases: Array<any> = [
        { input: '/test/null/customer1/path/quote/list', expectedResult: '/test/test/customer1/path/quote/list' },
        { input: '/test/null/customer1', expectedResult: '/test/test/customer1' },
    ];

    testCases.forEach((testCase: any) => {
        it(`should redirect from "${testCase.input}" to "${testCase.expectedResult}"`, async () => {
            // Arrange
            stateSnapshot.url = testCase.input;
            routeSnapshot.params = {
                portalTenantAlias: 'test',
                portalOrganisationAlias: 'null',
                portalAlias: 'customer1',
            };

            // Act
            const result: boolean | UrlTree = await guard.canActivate(routeSnapshot, stateSnapshot);

            // Assert
            expect(result instanceof UrlTree).toBe(true);
            expect(mockRouter.parseUrl).toHaveBeenCalledWith(testCase.expectedResult);
        });
    });


});
