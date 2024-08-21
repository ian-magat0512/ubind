import { TestBed } from '@angular/core/testing';
import { UrlTree } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { PathRedirectGuard } from './path-redirect.guard';

describe('PathRedirectGuard', () => {
    let guard: PathRedirectGuard;
    let route: ActivatedRouteSnapshot;
    let state: RouterStateSnapshot;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [RouterTestingModule],
            providers: [PathRedirectGuard],
        });

        guard = TestBed.inject(PathRedirectGuard);
        route = new ActivatedRouteSnapshot();
        state = jasmine.createSpyObj<RouterStateSnapshot>('RouterStateSnapshot', ['toString']);
    });

    it('should replace path as a query param and insert it into the path', () => {
        // Arrange
        const pathQueryParam: string = encodeURIComponent("quote/list?color=red");
        state.url = '/carl/carl/portal1/customer/create?path=' + pathQueryParam;
        const expectedRedirectUrl: string = '/carl/carl/portal1/path/quote/list?color=red';

        // Act
        const result: boolean | UrlTree = guard.canActivate(route, state);

        // Assert
        expect(result).not.toBe(true);
        expect(result.constructor.name).toBe('UrlTree');
        expect(result.toString()).toBe(expectedRedirectUrl);
    });

    const testCases: Array<any> = [
        { input: '/carl/carl/portal1/quote/list', expectedResult: '/carl/carl/portal1/path/quote/list' },
        { input: '/carl/carl/quote/list', expectedResult: '/carl/carl/path/quote/list' },
        { input: '/carl/quote/list', expectedResult: '/carl/path/quote/list' },
        { input: '/test/test/customer', expectedResult: '/test/test/customer' },
        { input: '/test/test/customer/list', expectedResult: '/test/test/path/customer/list' },
        { input: '/test/test/quote', expectedResult: '/test/test/path/quote' },
        { input: '/test/test/customer/login', expectedResult: '/test/test/customer/path/login' },
        { input: '/test/test/customer/customer', expectedResult: '/test/test/customer/path/customer' },
        { input: '/test/test/customer/activate', expectedResult: '/test/test/customer/path/activate' },
        { input: '/test/test/customer/create-account', expectedResult: '/test/test/customer/path/create-account' },
    ];

    testCases.forEach((testCase: any) => {
        it(`should ${testCase.input != testCase.expectedResult ? '' : 'not '} redirect to add in the \'path\' `
            + `path segment when the url is ${testCase.input}`, () => {
            // Arrange
            state.url = testCase.input;
            const expectedRedirectUrl: string = testCase.expectedResult;
            const shouldRedirect: boolean = testCase.input != testCase.expectedResult;

            // Act
            const result: boolean | UrlTree = guard.canActivate(route, state);

            // Assert
            if (!shouldRedirect) {
                expect(result).toBe(true);
            } else if (shouldRedirect) {
                expect(result.constructor.name).toBe('UrlTree');
                expect(result.toString()).toBe(expectedRedirectUrl);
            }
        });
    });

});
