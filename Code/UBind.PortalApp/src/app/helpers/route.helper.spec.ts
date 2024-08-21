import { Router, UrlSegment, UrlTree } from "@angular/router";
import { EventService } from "@app/services/event.service";
import { RouteHelper } from "./route.helper";
import { TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';

describe('RouteHelper', () => {
    let router: Router;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [RouterTestingModule],
        });

        router = TestBed.get(Router);
    });

    it('should replace the path segment with the value of the path query parameter', () => {
        // Arrange
        const eventService: EventService = new EventService();
        const routeHelper: RouteHelper = new RouteHelper(router, eventService);
        const pathQueryParam: string = encodeURIComponent("quote/list?color=red");
        let urlTree: UrlTree = router.parseUrl('/carl/carl/portal1/path/something?path=' + pathQueryParam);

        // Act
        let result: UrlTree = routeHelper.replacePathSegmentWithPathQueryParam(urlTree);

        // Assert
        const pathSegments: Array<UrlSegment> = result.root.children.primary.segments;
        expect(pathSegments.length).toBe(6);
        expect(pathSegments[0].path).toBe('carl');
        expect(pathSegments[1].path).toBe('carl');
        expect(pathSegments[2].path).toBe('portal1');
        expect(pathSegments[3].path).toBe('path');
        expect(pathSegments[4].path).toBe('quote');
        expect(pathSegments[5].path).toBe('list');
        expect(result.queryParams['color']).toBe('red');
        expect(result.queryParams['path']).toBeUndefined();
    });

    const testCases: Array<any> = [
        {
            portalBaseUrl: 'https://portal.com/test/org/portal1',
            partialPathAndQuery: '/abc/path/quote/list?segment=Complete',
            expectedResult: 'https://portal.com/test/org/portal1?path='
                + encodeURIComponent('quote/list') + '&segment=Complete',
        },
        {
            portalBaseUrl: 'https://portal.com/test/org/portal1',
            partialPathAndQuery: '/portal/abc',
            expectedResult: 'https://portal.com/test/org/portal1',
        },
    ];

    testCases.forEach((testCase: any) => {
        it('should add a path to a portal base url using the path query parameter', () => {
            // Arrange
            const eventService: EventService = new EventService();
            const routeHelper: RouteHelper = new RouteHelper(router, eventService);

            // Act
            let result: string = routeHelper.addPathAsQueryParamToPortalBaseUrl(
                testCase.portalBaseUrl,
                testCase.partialPathAndQuery);

            // Assert
            expect(result).toBe(testCase.expectedResult);
        });
    });

    const paths: Array<any> = [
        {
            path: '/path?param1=value1&param2=value2',
            result: new Map([['path', '/path'], ['param1', 'value1'], ['param2', 'value2']]),
        },
        {
            path: 'pathWithoutSlash?param1=value1&param2=value2',
            result: new Map([['path', 'pathWithoutSlash'], ['param1', 'value1'], ['param2', 'value2']]),
        },
        {
            path: '/path/rr',
            result: new Map([['path', '/path/rr']]),
        },
    ];

    describe('RoutHelper.gatherParamsFromPath', () => {
        paths.forEach((testCase: any) => {
            it('should properly separate out a query params to an array', () => {
                // Arrange
                const eventService: EventService = new EventService();
                const routeHelper: RouteHelper = new RouteHelper(router, eventService);

                // Act
                let result: Map<string, string> = routeHelper.gatherQueryParamsFromIncompletePath(testCase.path);

                // Assert
                expect(mapsEqual(result, testCase.result)).toBe(true);
            });

            const mapsEqual = (map1: Map<string, string>, map2: Map<string, string>): boolean => {
                if (map1.size !== map2.size) {
                    return false;
                }

                for (const [key, value] of map1.entries()) {
                    if (map2.get(key) !== value) {
                        return false;
                    }
                }

                return true;
            };
        });
    });
});
