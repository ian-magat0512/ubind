import { SidebarOffsetService } from "./sidebar-offset.service";
import { ConfigService } from "./config.service";
import { EventService } from "./event.service";
import { ApplicationService } from "./application.service";
import { BrowserDetectionService } from "./browser-detection.service";

/**
 * An aside is rendered in the right hand column.
 */

declare const viewport: any;
describe('SidebarOffsetService', () => {
    let service: SidebarOffsetService;
    let applicationService: ApplicationService;
    let browserDetectionService: BrowserDetectionService;
    let eventService: EventService;

    beforeEach(() => {
        eventService = new EventService();
        applicationService = new ApplicationService();
        let configService: ConfigService = new ConfigService(eventService, applicationService);
        browserDetectionService = new BrowserDetectionService(configService, applicationService, eventService);
        service = new SidebarOffsetService(
            configService,
            applicationService,
            browserDetectionService,
            new EventService());
    });

    it('should calculate correct offset for bootstrap specific', () => {
        const testCases: any = [
            { name: 'xs breakpoint below window width', sidebarOffset: 'xs,200', width: 574, expectedOffset: 200 },
            { name: 'xs breakpoint equal to window width', sidebarOffset: 'xs,200', width: 575, expectedOffset: 200 },
            { name: 'xs breakpoint above window width', sidebarOffset: 'xs,200', width: 576, expectedOffset: 200 },

            { name: 'sm breakpoint below window width', sidebarOffset: 'xs,100|sm,200', width: 575,
                expectedOffset: 100 },
            { name: 'sm breakpoint equal to window width', sidebarOffset: 'xs,100|sm,200', width: 576,
                expectedOffset: 200 },
            { name: 'sm breakpoint above window width', sidebarOffset: 'xs,100|sm,200', width: 577,
                expectedOffset: 200 },

            { name: 'md breakpoint below window width', sidebarOffset: 'xs,100|sm,200|md,300', width: 767,
                expectedOffset: 200 },
            { name: 'md breakpoint equal to window width', sidebarOffset: 'xs,100|sm,200|md,300', width: 768,
                expectedOffset: 300 },
            { name: 'md breakpoint above window width', sidebarOffset: 'xs,100|sm,200|md,300', width: 768,
                expectedOffset: 300 },

            { name: 'lg breakpoint below window width', sidebarOffset: 'xs,100|sm,200|md,300|lg,400', width: 991,
                expectedOffset: 300 },
            { name: 'lg breakpoint equal to window width', sidebarOffset: 'xs,100|sm,200|md,300|lg,400', width: 992,
                expectedOffset: 400 },
            { name: 'lg breakpoint above window width', sidebarOffset: 'xs,100|sm,200|md,300|lg,400', width: 993,
                expectedOffset: 400 },

            { name: 'xl breakpoint below window width', sidebarOffset: 'xs,100|sm,200|md,300|lg,400|xl,500',
                width: 1199, expectedOffset: 400 },
            { name: 'xl breakpoint equal to window width', sidebarOffset: 'xs,100|sm,200|md,300|lg,400|xl,500',
                width: 1200, expectedOffset: 500 },
            { name: 'xl breakpoint above window width', sidebarOffset: 'xs,100|sm,200|md,300|lg,400|xl,500',
                width: 1201, expectedOffset: 500 },

            { name: 'xl breakpoint below window width', sidebarOffset: 'xs,100|sm,200|md,300|lg,400|xl,500|xxl,600',
                width: 1399, expectedOffset: 500 },
            { name: 'xl breakpoint equal to window width', sidebarOffset: 'xs,100|sm,200|md,300|lg,400|xl,500|xxl,600',
                width: 1400, expectedOffset: 600 },
            { name: 'xl breakpoint above window width', sidebarOffset: 'xs,100|sm,200|md,300|lg,400|xl,500|xxl,600',
                width: 1401, expectedOffset: 600 },

            { name: 'breakpoint between xs and lg window width', sidebarOffset: 'xs,100|lg,200',
                width: 768, expectedOffset: 100 },
        ];

        testCases.forEach((testCase: any) => {
            // Arrange
            viewport.set(testCase.width);
            eventService.windowResizeSubject.next(testCase.width);

            // Act
            (<any>applicationService)._sidebarOffsetConfiguration = testCase.sidebarOffset;
            let offset: number = service.getOffsetTop();

            // Assert
            expect(offset).toBe(testCase.expectedOffset,
                `${testCase.name} : Expected ${offset} to be ${testCase.expectedOffset}.`);
        });

    });

    it('should calculate correct offset for pixel specific', () => {
        const testCases: any = [
            { name: 'breakpoint below width', sidebarOffset: '0,100|768,200', width: 767, expectedOffset: 100 },
            { name: 'breakpoint equal to width', sidebarOffset: '0,100|768,200', width: 768, expectedOffset: 200 },
            { name: 'breakpoint above width', sidebarOffset: '0,100|768,200', width: 769, expectedOffset: 200 },
        ];

        testCases.forEach((testCase: any) => {

            // Arrange
            viewport.set(testCase.width);
            (<any>applicationService)._sidebarOffsetConfiguration = testCase.sidebarOffset;
            eventService.windowResizeSubject.next(testCase.width);

            // Act
            let offset: number = service.getOffsetTop();

            // Assert
            expect(offset).toBe(testCase.expectedOffset,
                `on ${testCase.name}`);
        });

    });

});
