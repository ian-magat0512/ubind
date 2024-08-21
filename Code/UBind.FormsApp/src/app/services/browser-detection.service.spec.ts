/* eslint-disable max-classes-per-file */
import { BrowserDetectionService } from "./browser-detection.service";
import { EventService } from "./event.service";

/* global spyOnProperty */

describe('BrowserDetectionService', () => {
    let browserDetectionService: BrowserDetectionService;
    let eventService: EventService;

    beforeEach(async () => {
        eventService = new EventService();
        browserDetectionService = new BrowserDetectionService(null, null, eventService);
    });

    it('should return Firefox when useragent string contains that browser name.', () => {
        // Arrange
        spyOnProperty(browserDetectionService, "currentUserAgent", "get").and.returnValue('someTEXTWith_firefox');
        // Assert
        expect(browserDetectionService.currentBrowser).toBe('Firefox');
    });

    it('should return Chrome when useragent string contains that browser name', () => {

        // Arrange
        spyOnProperty(browserDetectionService, "currentUserAgent", "get").and.returnValue('someTEXTWith_chrome');

        // Assert
        expect(browserDetectionService.currentBrowser).toBe('Chrome');
    });

    it('should return IE when useragent string contains that browser name', () => {

        // Arrange
        spyOnProperty(browserDetectionService, "currentUserAgent", "get").and.returnValue('someTEXTWith_msie');

        // Assert
        expect(browserDetectionService.currentBrowser).toBe('IE');
    });

    it('should return Opera when useragent string contains that browser name', () => {

        // Arrange
        spyOnProperty(browserDetectionService, "currentUserAgent", "get").and.returnValue('someTEXTWith_opera');

        // Assert
        expect(browserDetectionService.currentBrowser).toBe('Opera');
    });

    it('should return Safari when useragent string contains that browser name', () => {

        // Arrange
        spyOnProperty(browserDetectionService, "currentUserAgent", "get").and.returnValue('someTEXTWith_safari');

        // Assert
        expect(browserDetectionService.currentBrowser).toBe('Safari');
    });

    it('should return Unknown when useragent string does not contain any major browsers name', () => {

        // Arrange
        spyOnProperty(browserDetectionService, "currentUserAgent", "get").and.returnValue('abc123');
        spyOnProperty(browserDetectionService, "documentNode", "get").and.returnValue(0);

        // Assert
        expect(browserDetectionService.currentBrowser).toBe('Unknown');
    });
});
