/* eslint-disable no-unused-vars */
import { UserService } from "./user.service";
import { ApplicationService } from "./application.service";
import { SessionDataManager } from '@app/storage/session-data-manager';
import { storageHelper } from '@app/helpers/storage.helper';
import { ApplicationMode } from "@app/models/application-mode.enum";
import { UserType } from "@app/models/user-type.enum";

describe('UserService', () => {
    let service: UserService;
    it('isCustomer should return true when there is no access token', () => {
        let applicationService: ApplicationService = new ApplicationService();
        applicationService.setApplicationConfiguration(
            'https://localhost',
            'tenant',
            'tenant',
            null,
            'organisation',
            true,
            'productId',
            'product',
            'development',
            '',
            '',
            '',
            null,
            null,
            null,
            ApplicationMode.Create,
            null,
            '',
            false);

        const session: any = {
            getTenantOrganisationSessionValue: (
                // eslint-disable-next-line no-unused-vars
                key: string,
                // eslint-disable-next-line no-unused-vars
                tenantAlias: string,
                // eslint-disable-next-line no-unused-vars
                organisationAlias: string): string => null,
        };

        service = new UserService(session as SessionDataManager, applicationService);
        service.retrieveLoggedInUserData();
        expect(service.isCustomer).toBe(true);
    });

    it('isCustomer should return true when localstorage ubind.accessToken key is found but there is no userType entry',
        () => {
            let applicationService: ApplicationService = new ApplicationService();
            applicationService.setApplicationConfiguration(
                'https://localhost',
                'tenant',
                'tenant',
                null,
                'organisation',
                true,
                'productId',
                'product',
                'development',
                '',
                '',
                '',
                null,
                null,
                null,
                ApplicationMode.Create,
                null,
                '',
                false);
            let ubindAccessToken: string = 'accessToken';
            let ubindCustomerIdDefault: string = 'notDefault';
            const session: any = {
                getTenantOrganisationSessionValue: (
                    key: string,
                    tenantAlias: string,
                    organisationAlias: string): string => {
                    if (key == storageHelper.user.tenantAlias) {
                        return "tenant";
                    }

                    if (key == storageHelper.user.accessToken) {
                        return ubindAccessToken;
                    }
                },
            };

            service = new UserService(session as SessionDataManager, applicationService);
            service.retrieveLoggedInUserData();
            expect(service.isCustomer).toBe(true);
        });

    it('isCustomer should return false when localstorage ' +
        'ubind.accessToken key is found and the userType is set to Agent', () => {
        let applicationService: ApplicationService = new ApplicationService();
        applicationService.setApplicationConfiguration(
            'https://localhost',
            'tenant',
            'tenant',
            null,
            'organisation',
            true,
            'productId',
            'product',
            'development',
            '',
            '',
            '',
            null,
            null,
            null,
            ApplicationMode.Create,
            null,
            '',
            false);
        let ubindAccessToken: string = 'accessToken';
        const session: any = {
            getTenantOrganisationSessionValue: (key: string,
                tenantAlias: string, organisationAlias: string): string => {
                if (key == storageHelper.user.accessToken) {
                    return ubindAccessToken;
                }

                if (key == storageHelper.user.tenantAlias) {
                    return "tenant";
                }

                if (key == storageHelper.user.userType) {
                    return UserType.Client;
                }
            },
        };

        service = new UserService(session as SessionDataManager, applicationService);
        service.retrieveLoggedInUserData();
        expect(service.isCustomer).toBe(false);
    });

    it('isCustomer should return false when localstorage '
        + 'ubind.accessToken key is found and the userType is set to ClientAdmin', () => {
        let applicationService: ApplicationService = new ApplicationService();
        applicationService.setApplicationConfiguration(
            'https://localhost',
            'tenant',
            'tenant',
            null,
            'organisation',
            true,
            'productId',
            'product',
            'development',
            '',
            '',
            '',
            null,
            null,
            null,
            ApplicationMode.Create,
            null,
            '',
            false);
        let ubindAccessToken: string = 'accessToken';
        const session: any = {
            getTenantOrganisationSessionValue: (key: string,
                tenantAlias: string, organisationAlias: string): string => {
                if (key == storageHelper.user.accessToken) {
                    return ubindAccessToken;
                }

                if (key == storageHelper.user.tenantAlias) {
                    return "tenant";
                }

                if (key == storageHelper.user.userType) {
                    return UserType.Client;
                }
            },
        };

        service = new UserService(session as SessionDataManager, applicationService);
        service.retrieveLoggedInUserData();
        expect(service.isCustomer).toBe(false);
    });

    it('getPermissions should return empty array when localstorage ubind.permission key is not found', () => {
        let applicationService: ApplicationService = new ApplicationService();
        applicationService.setApplicationConfiguration(
            'https://localhost',
            'tenant',
            'tenant',
            null,
            'organisation',
            true,
            'productId',
            'product',
            'development',
            '',
            '',
            '',
            null,
            null,
            null,
            ApplicationMode.Create,
            null,
            '',
            false);

        const session: any = {
            getTenantOrganisationSessionValue: (key: string,
                tenantAlias: string, organisationAlias: string): string | Array<string> => {
                if (key == storageHelper.user.permissions) {
                    return [];
                }

                if (key == storageHelper.user.tenantAlias) {
                    return "tenant";
                }
            },
        };

        service = new UserService(session, applicationService);
        service.retrieveLoggedInUserData();
        expect(service.permissions.length).toBe(0);
    });

    it('getPermissions should return ubind.permission value when localstorage ubind.permission key is found', () => {
        let applicationService: ApplicationService = new ApplicationService();
        applicationService.setApplicationConfiguration(
            'https://localhost',
            'tenant',
            'tenant',
            'organisation',
            'organisation',
            true,
            'productId',
            'product',
            'development',
            '',
            '',
            '',
            null,
            null,
            null,
            ApplicationMode.Create,
            null,
            '',
            false);
        let ubindPermissions: Array<string> = ["canDoThis", "canDoThat"];
        const session: any = {
            getTenantOrganisationSessionValue: (key: string,
                tenantAlias: string, organisationAlias: string): any => {
                if (key == storageHelper.user.permissions) {
                    return ubindPermissions;
                }

                if (key == storageHelper.user.tenantAlias) {
                    return "tenant";
                }
            },
        };

        service = new UserService(session as SessionDataManager, applicationService);
        service.retrieveLoggedInUserData();
        expect(service.permissions).toBe(ubindPermissions);
    });
});
