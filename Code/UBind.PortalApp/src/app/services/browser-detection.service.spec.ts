/* eslint-disable max-classes-per-file */

import { BrowserDetectionService } from "./browser-detection.service";

/**
 * Mock firefox browser detection service class
 * Spies just won't work, so I just mocked the whole service instead
 */
class MockFireFoxBrowserDetectionService extends BrowserDetectionService {
    public get currentUserAgent(): string {
        return 'someTEXTWith_firefox';
    }
}

/**
 * Mock IE browser detection service class
 */
class MockIEBrowserDetectionService extends BrowserDetectionService {
    public get currentUserAgent(): string {
        return 'someTEXTWith_msie';
    }
}

/**
 * Mock safari browser detection service class
 */
class MockSafariBrowserDetectionService extends BrowserDetectionService {
    public authenticated: boolean = false;

    public get currentUserAgent(): string {
        return 'someTEXTWith_safari';
    }
}

/**
 * Mock opera browser detection service class
 */
class MockOperaBrowserDetectionService extends BrowserDetectionService {
    public get currentUserAgent(): string {
        return 'someTEXTWith_opera';
    }
}

/**
 * Mock chrome browser detection service class
 */
class MockChromeBrowserDetectionService extends BrowserDetectionService {
    public get currentUserAgent(): string {
        return 'someTEXTWith_chrome';
    }
}

/**
 * Mock unknown browser detection service class
 */
class MockUnknownBrowserDetectionService extends BrowserDetectionService {
    public get currentUserAgent(): string {
        return 'abc123';
    }

    public get documentNode(): number {
        return 0;
    }
}

describe('BrowserDetectionService', () => {

    it('should return Firefox when useragent string contains that browser name.', () => {

        // Arrange
        const browserDetectionService: MockFireFoxBrowserDetectionService =
            new MockFireFoxBrowserDetectionService();
        // Assert
        expect(browserDetectionService.currentBrowser).toBe('Firefox');
    });

    it('should return Chrome when useragent string contains that browser name', () => {

        // Arrange
        const browserDetectionService: MockChromeBrowserDetectionService =
            new MockChromeBrowserDetectionService();

        // Assert
        expect(browserDetectionService.currentBrowser).toBe('Chrome');
    });

    it('should return IE when useragent string contains that browser name', () => {

        // Arrange
        const browserDetectionService: MockIEBrowserDetectionService =
            new MockIEBrowserDetectionService();

        // Assert
        expect(browserDetectionService.currentBrowser).toBe('IE');
    });

    it('should return Opera when useragent string contains that browser name', () => {

        // Arrange
        const browserDetectionService: MockOperaBrowserDetectionService =
            new MockOperaBrowserDetectionService();

        // Assert
        expect(browserDetectionService.currentBrowser).toBe('Opera');
    });

    it('should return Safari when useragent string contains that browser name', () => {

        // Arrange
        const browserDetectionService: MockSafariBrowserDetectionService =
            new MockSafariBrowserDetectionService();

        // Assert
        expect(browserDetectionService.currentBrowser).toBe('Safari');
    });

    it('should return Unknown when useragent string does not contain any major browsers name', () => {

        // Arrange
        const browserDetectionService: MockUnknownBrowserDetectionService =
            new MockUnknownBrowserDetectionService();

        // Assert
        expect(browserDetectionService.currentBrowser).toBe('Unknown');
    });
});
