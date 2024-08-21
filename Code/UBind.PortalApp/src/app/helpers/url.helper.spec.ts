import { UrlComponents, UrlHelper } from './url.helper';
import { } from 'jasmine';

/**
 * Mocks the browsers location
 */
class MockLocation {
    public testPath: string = '/portal/test/test/portal1/path/login';

    public path(): string {
        return this.testPath;
    }

    public get pathname(): string {
        return this.testPath;
    }
}

describe('UrlHelper', () => {

    const portalTestCases: Array<any> = [
        { input: '/portal/test/test/portal1/path/login', expectedResult: 'portal1' },
        { input: '/portal/test/test/portal1/login', expectedResult: 'portal1' },
        { input: '/portal/test/test/portal1/', expectedResult: 'portal1' },
        { input: '/portal/test/test/portal1', expectedResult: 'portal1' },
        { input: '/portal/test/test/', expectedResult: null },
        { input: '/portal/test/test', expectedResult: null },
        { input: '/portal/test/test/customer', expectedResult: 'customer' }, // special exception for customer
        { input: '/portal/test/test/customer/list', expectedResult: null },
        { input: '/portal/test/test/quote', expectedResult: null },
        { input: '/portal/test/test/customer/path/quote/list', expectedResult: 'customer' },
    ];

    portalTestCases.forEach((testCase: any) => {
        it(`should detect the portal alias '${testCase.expectedResult}' when the path is '${testCase.input}'`, () => {
            // Arrange
            const location: MockLocation = new MockLocation();
            location.testPath = testCase.input;

            // Act
            const result: string = UrlHelper.getPortalAliasFromUrl(location as any as Location);

            // Assert
            expect(result).toBe(testCase.expectedResult);
        });
    });

    const organisationTestCases: Array<any> = [
        { input: '/portal/test/org/portal1/path/login', expectedResult: 'org' },
        { input: '/portal/test/org/portal1/login', expectedResult: 'org' },
        { input: '/portal/test/org/portal1/', expectedResult: 'org' },
        { input: '/portal/test/org/', expectedResult: 'org' },
        { input: '/portal/test/org', expectedResult: 'org' },
        { input: '/portal/test/', expectedResult: null },
        { input: '/portal/test', expectedResult: null },
    ];

    organisationTestCases.forEach((testCase: any) => {
        it(`should detect the organisation alias '${testCase.expectedResult}' when the path is `
            + `'${testCase.input}'`, () => {
            // Arrange
            const location: MockLocation = new MockLocation();
            location.testPath = testCase.input;

            // Act
            const result: string = UrlHelper.getOrganisationAliasFromUrl(location as any as Location);

            // Assert
            expect(result).toBe(testCase.expectedResult);
        });
    });

    const tenantTestCases: Array<any> = [
        { input: '/portal/test/org/portal1/path/login', expectedResult: 'test' },
        { input: '/portal/test/org/portal1/login', expectedResult: 'test' },
        { input: '/portal/test/org/portal1/', expectedResult: 'test' },
        { input: '/portal/test/org/', expectedResult: 'test' },
        { input: '/portal/test/org', expectedResult: 'test' },
        { input: '/portal/test/', expectedResult: 'test' },
        { input: '/portal/test', expectedResult: 'test' },
        { input: '/portal/', expectedResult: null },
        { input: '/portal', expectedResult: null },
    ];

    tenantTestCases.forEach((testCase: any) => {
        it(`should detect the tenant alias '${testCase.expectedResult}' when the path is '${testCase.input}'`, () => {
            // Arrange
            const location: MockLocation = new MockLocation();
            location.testPath = testCase.input;

            // Act
            const result: string = UrlHelper.getTenantAliasFromUrl(location as any as Location);

            // Assert
            expect(result).toBe(testCase.expectedResult);
        });
    });

    const baseUrlTestCases: Array<any> = [
        { input: '/portal/test/org/portal1/path/login', expectedResult: '/portal/test/org/portal1' },
        { input: '/portal/test/org/portal1/login', expectedResult: '/portal/test/org/portal1' },
        { input: '/portal/test/org/portal1/', expectedResult: '/portal/test/org/portal1' },
        { input: '/portal/test/org/', expectedResult: '/portal/test/org' },
        { input: '/portal/test/org', expectedResult: '/portal/test/org' },
        { input: '/portal/test/', expectedResult: '/portal/test' },
        { input: '/portal/test', expectedResult: '/portal/test' },
        { input: '/portal/', expectedResult: '/portal' },
        { input: '/portal', expectedResult: '/portal' },
        { input: '/portal?abc=true&xyz=false', expectedResult: '/portal' },
        { input: 'https://app.ubind.com.au/portal', expectedResult: 'https://app.ubind.com.au/portal' },
        { input: 'https://app.ubind.com.au/portal/', expectedResult: 'https://app.ubind.com.au/portal' },
        {
            input: 'https://app.ubind.com.au/portal/test/org/portal1',
            expectedResult: 'https://app.ubind.com.au/portal/test/org/portal1',
        },
        {
            input: 'https://app.ubind.com.au/portal/test/org/portal1/path/quote/list',
            expectedResult: 'https://app.ubind.com.au/portal/test/org/portal1',
        },
    ];

    baseUrlTestCases.forEach((testCase: any) => {
        it(`should extract the portal part of the url as '${testCase.expectedResult}' when the path `
            + `is '${testCase.input}'`, () => {
            // Arrange
            const location: MockLocation = new MockLocation();
            location.testPath = testCase.input;

            // Act
            const result: string = UrlHelper.getPortalBaseUrl(testCase.input);

            // Assert
            expect(result).toBe(testCase.expectedResult);
        });
    });

    const extractUrlComponentsTestCases: Array<any> = [
        {
            input: 'https://app.ubind.com.au/portal/test/org/portal1',
            expectedResult: '/portal/test/org/portal1',
        },
        {
            input: 'https://app.ubind.com.au/portal/test/org/portal1/path/quote/list',
            expectedResult: '/portal/test/org/portal1/path/quote/list',
        },
        {
            input: 'https://localhost:44366/portal/test/test?'
                + 'frameId=ubind-portal-iframe---test---test---undefined---portal1&portal=portal1&'
                + 'data-parent-url=https:%2F%2Fub9162.ubind.com.au%2Fportal.html&referrer=ub9162.ubind.com.au&'
                + 'environment=undefined',
            expectedResult: '/portal/test/test',
        },
    ];
    extractUrlComponentsTestCases.forEach((testCase: any) => {
        it(`should successfully extract URL components with the path being '${testCase.expectedResult}' when the URL `
            + `is '${testCase.input}'`, () => {
            // Arrange
            const location: MockLocation = new MockLocation();
            location.testPath = testCase.input;

            // Act
            const result: UrlComponents = UrlHelper.extractUrlComponents(testCase.input);

            // Assert
            expect(result.path).toBe(testCase.expectedResult);
        });
    });

    const replacePathTestCases: Array<any> = [
        {
            input: 'https://app.ubind.com.au/portal/test?path=/quote/list',
            expectedResult: 'https://app.ubind.com.au/portal/test/quote/list',
        },
        {
            input: 'https://app.ubind.com.au/portal/test?path=%2Fquote%2Flist',
            expectedResult: 'https://app.ubind.com.au/portal/test/quote/list',
        },
        {
            input: 'https://app.ubind.com.au/portal/test?'
                + 'path=%2Fquote%2F10314ce5-a64b-4760-858d-ee9114b38447%3Fsegment%3DDetails',
            expectedResult: 'https://app.ubind.com.au/portal/test/quote/10314ce5-a64b-4760-858d-ee9114b38447?'
                + 'segment=Details',
        },
    ];
    replacePathTestCases.forEach((testCase: any) => {
        it(`should append the query param path from ${testCase.input} to the URL path`, () => {
            // Arrange
            const url: string = testCase.input;

            // Act
            const result: string = UrlHelper.appendPathWithPathQueryParm(url);

            // Assert
            expect(result).toBe(testCase.expectedResult);
        });
    });
});

describe('UrlHelper.extractPathUrl', () => {
    it('should remove iframe properties correctly', () => {
        // Arrange
        const inputOutputs: Array<Array<string>> = [
            ['/portal/carl/path/quote/list', '/quote/list'],
            ['/portal/carl/path/home?environment=Development', '/home?environment=Development'],
            [
                '/portal/carl/path/user/df6cdfb7-b460-4d7f-97b8-146d46776227?environment=Development'
                + '&frameId=ubind-portal-iframe---carl---carl---undefined---dev&portal'
                + '=dev&data-parent-url=https:%2F%2Fub8771-feature.ubind.com.au%2Fportal&referrer='
                + 'ub8771-feature.ubind.com.au',
                '/user/df6cdfb7-b460-4d7f-97b8-146d46776227?environment=Development',
            ],
            [
                '/portal/carl/path/customer/427bd9ea-a490-46f7-8a0f-fef6f0d40b4d'
                + '?segment=Details&environment=Development',
                '/customer/427bd9ea-a490-46f7-8a0f-fef6f0d40b4d?segment=Details&environment=Development',
            ],
            [
                '/portal/carl/path/quote/e306d545-823b-4f1e-ae36-71b2c655de90'
                + '?segments=Questions&environment=Development',
                '/quote/e306d545-823b-4f1e-ae36-71b2c655de90?segments=Questions&environment=Development',
            ],
        ];

        // Act
        inputOutputs.forEach((inputOutput: Array<string>) => {
            let result: string = UrlHelper.extractPathUrl(inputOutput[0]);

            // Assert
            expect(result).toBe(inputOutput[1]);
        });
    });
});
