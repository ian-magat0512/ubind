import { UrlHelper } from "@app/helpers";
import { AppConfig } from "@app/models/app-config";
import { BehaviorSubject } from "rxjs";
import { AppConfigService } from "./app-config.service";
import { ErrorHandlerService } from "./error-handler.service";
import { EventService } from "./event.service";
import { NavProxyService } from "./nav-proxy.service";

describe('NavProxyService', () => {
    let eventService: EventService;
    let appConfigService: AppConfigService;

    let locationStub: any = {
        path: (): string => '',
    };

    beforeEach(() => {
        eventService = new EventService();
        appConfigService = new AppConfigService(null, eventService, null, locationStub as any);
    });

    it('injectCommandsWithTenantOrgPortalAndPath should not mess up a full path', () => {
        // Arrange
        appConfigService.appConfigSubject = new BehaviorSubject<AppConfig>(<any>{
            portal: {
                environment: 'Production',
                portalAlias: null,
                tenantAlias: 'carl',
            },
        });
        const errorHandlerService: ErrorHandlerService = new ErrorHandlerService(null, null, eventService);
        const navProxyService: NavProxyService
            = new NavProxyService(null, errorHandlerService, null, null, appConfigService, null, null, null, null);
        const commands: Array<any> = ['carl', 'carl', 'path', 'quote', 'list'];

        // Act
        const result: Array<any> = navProxyService.injectCommandsWithTenantOrgPortalAndPath(commands);

        // Assert
        expect(result).toEqual(['carl', 'carl', 'path', 'quote', 'list']);
    });

    it('injectCommandsWithTenantOrgPortalAndPath should add the necessary prefixes when there\'s a tenant but no '
        + 'organisation', () => {
        // Arrange
        appConfigService.appConfigSubject = new BehaviorSubject<AppConfig>(<any>{
            portal: {
                environment: 'Production',
                portalAlias: null,
                tenantAlias: 'carl',
            },
        });
        const errorHandlerService: ErrorHandlerService = new ErrorHandlerService(null, null, eventService);
        const navProxyService: NavProxyService
            = new NavProxyService(null, errorHandlerService, null, null, appConfigService, null, null, null, null);
        const commands: Array<any> = ['login'];

        // Act
        const result: Array<any> = navProxyService.injectCommandsWithTenantOrgPortalAndPath(commands);

        // Assert
        expect(result).toEqual(['carl', 'path', 'login']);
    });

    it('injectCommandsWithTenantOrgPortalAndPath should add the necessary prefixes when there\'s a tenant and '
        + 'organisation', () => {
        // Arrange
        appConfigService.appConfigSubject = new BehaviorSubject<AppConfig>(<any>{
            portal: {
                environment: 'Production',
                portalAlias: null,
                tenantAlias: 'carl',
            },
        });
        spyOn(UrlHelper, 'getOrganisationAliasFromUrl').and.returnValue('abc-brokers');
        const errorHandlerService: ErrorHandlerService = new ErrorHandlerService(null, null, eventService);
        const navProxyService: NavProxyService
            = new NavProxyService(null, errorHandlerService, null, null, appConfigService, null, null, null, null);
        const commands: Array<any> = ['login'];

        // Act
        const result: Array<any> = navProxyService.injectCommandsWithTenantOrgPortalAndPath(commands);

        // Assert
        expect(result).toEqual(['carl', 'abc-brokers', 'path', 'login']);
    });
});
