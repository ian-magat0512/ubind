import { ResilientStorage } from './resilient-storage';
import { SessionDataManager } from './session-data-manager';
import { storageHelper } from '@app/helpers/storage.helper';

describe('Session', () => {

    beforeEach(() => {
        const resilientStorage: ResilientStorage = new ResilientStorage();
        const tenantAlias: string = 'figi';
        const organisationAlias: string = 'figiiii';

        let uBindSession: any = {
            [storageHelper.user.accessToken]: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54'
                + 'bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjI5Y2I5NjlhLTRk'
                + 'MWQtNDBkNC1hN2QxLTliZjRmNjkxNzIwMCIsIlRlbmFudCI6ImZpZ2kiLCJPcmdhbmlzYXRpb25JZCI6IjIxZjdi'
                + 'NTZlLTVmZGItNGQ0My1hMDVmLTIwY2YxNDMwYjFkMiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3M'
                + 'vMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkNsaWVudEFkbWluIiwiU2Vzc2lvbklkIjoiNDllYmNkYT'
                + 'MtMDIzZS00ODQ0LWE3MzctNzk3OWU4OTZhY2ZjIiwiUGVybWlzc2lvbnMiOiJbXCJ2aWV3TXlBY2NvdW50XCIs'
                + 'XCJlZGl0TXlBY2NvdW50XCIsXCJ2aWV3VXNlcnNcIixcIm1hbmFnZVVzZXJzXCIsXCJtYW5hZ2VDbGllbnR'
                + 'BZG1pblVzZXJzXCIsXCJ2aWV3Um9sZXNcIixcIm1hbmFnZVJvbGVzXCIsXCJ2aWV3UXVvdGVzXCIsXCJtY'
                + 'W5hZ2VRdW90ZXNcIixcImVuZG9yc2VRdW90ZXNcIixcInJldmlld1F1b3Rlc1wiLFwiZXhwb3J0UXVvdGVzX'
                + 'CIsXCJ2aWV3UXVvdGVWZXJzaW9uc1wiLFwibWFuYWdlUXVvdGVWZXJzaW9uc1wiLFwidmlld1BvbGljaWVzXC'
                + 'IsXCJhZGp1c3RQb2xpY2llc1wiLFwicmVuZXdQb2xpY2llc1wiLFwiY2FuY2VsUG9saWNpZXNcIixcImV4c'
                + 'G9ydFBvbGljaWVzXCIsXCJpbXBvcnRQb2xpY2llc1wiLFwidmlld0NsYWltc1wiLFwibWFuYWdlQ2xhaW1zXCIsX'
                + 'CJhY2tub3dsZWRnZUNsYWltTm90aWZpY2F0aW9uc1wiLFwiYXNzaWduQ2xhaW1OdW1iZXJzXCIsXCJyZXZpZXdDb'
                + 'GFpbXNcIixcImFzc2Vzc0NsYWltc1wiLFwic2V0dGxlQ2xhaW1zXCIsXCJhc3NvY2lhdGVDbGFpbXNcIixcImV4c'
                + 'G9ydENsYWltc1wiLFwiaW1wb3J0Q2xhaW1zXCIsXCJ2aWV3Q3VzdG9tZXJzXCIsXCJtYW5hZ2VDdXN0b21lcnNcIixcImlt'
                + 'cG9ydEN1c3RvbWVyc1wiLFwidmlld0VtYWlsc1wiLFwibWFuYWdlRW1haWxzXCIsXCJ2aWV3UmVwb3J0c1wiLFw'
                + 'ibWFuYWdlUmVwb3J0c1wiLFwiZ2VuZXJhdGVSZXBvcnRzXCIsXCJhY2Nlc3NEZXZlbG9wbWVudERhdGFcIixcI'
                + 'mFjY2Vzc1N0YWdpbmdEYXRhXCIsXCJhY2Nlc3NQcm9kdWN0aW9uRGF0YVwiLFwidmlld09yZ2FuaXNhdGlvbnN'
                + 'cIixcIm1hbmFnZU9yZ2FuaXNhdGlvbnNcIixcInZpZXdQcm9kdWN0c1wiLFwibWFuYWdlUHJvZHVjdHNcIix'
                + 'cInZpZXdSZWxlYXNlc1wiLFwibWFuYWdlUmVsZWFzZXNcIixcInByb21vdGVSZWxlYXNlc1RvU3RhZ2luZ1'
                + 'wiLFwicHJvbW90ZVJlbGVhc2VzVG9Qcm9kdWN0aW9uXCIsXCJpbXBvcnREYXRhXCIsXCJ2aWV3QmFja2dy'
                + 'b3VuZEpvYnNcIixcIm1hbmFnZUJhY2tncm91bmRKb2JzXCIsXCJ2aWV3QWRkaXRpb25hbFByb3BlcnRpZX'
                + 'NcIixcImVkaXRBZGRpdGlvbmFsUHJvcGVydGllc1wiXSIsImV4cCI6MTY1NTUxNTI5OCwiaXNzIjoiaHR0'
                + 'cHM6Ly9sb2NhbGhvc3Q6NDQzNjYiLCJhdWQiOiJodHRwczovL2xvY2FsaG9zdDo0NDM2NiJ9.'
                + 'X-s-PjSvkM7JFJvtHsvW9W4haSDQVz7RjEtuWZrpxQ4',
            [storageHelper.user.permissions]: JSON.parse('["viewMyAccount","editMyAccount","viewUsers","manageUsers",'
                + '"manageTenantAdminUsers","viewRoles","manageRoles","viewQuotes","manageQuotes","endorseQuotes",'
                + '"reviewQuotes","exportQuotes","viewQuoteVersions","manageQuoteVersions","viewPolicies",'
                + '"adjustPolicies","renewPolicies","cancelPolicies","exportPolicies","importPolicies","viewClaims",'
                + '"manageClaims","acknowledgeClaimNotifications","assignClaimNumbers","reviewClaims","assessClaims",'
                + '"settleClaims","associateClaims","exportClaims","importClaims","viewCustomers","manageCustomers",'
                + '"importCustomers","viewMessages","manageMessages","viewReports","manageReports","generateReports",'
                + '"accessDevelopmentData","accessStagingData","accessProductionData","viewOrganisations",'
                + '"manageOrganisations","viewProducts","manageProducts","viewReleases","manageReleases",'
                + '"promoteReleasesToStaging","promoteReleasesToProduction","importData","viewBackgroundJobs",'
                + '"manageBackgroundJobs","viewAdditionalPropertyValues","editAdditionalPropertyValues"]'),
            [storageHelper.user.expiryTime]: '1655515298',
            [storageHelper.user.userId]: '29cb969a-4d1d-40d4-a7d1-9bf4f6917200',
            [storageHelper.user.customerId]: '00000000-0000-0000-0000-000000000000',
            [storageHelper.user.emailAddress]: 'figi.client.admin@ubind.com.au',
            [storageHelper.user.fullName]: 'Default figi client admin',
            [storageHelper.user.preferredName]: 'Client admin',
            [storageHelper.user.environment]: 'Production',
            [storageHelper.user.userType]: 'ClientAdmin',
            [storageHelper.user.tenantId]: 'figi',
            [storageHelper.user.tenantAlias]: 'figi',
            [storageHelper.user.organisationId]: '21f7b56e-5fdb-4d43-a05f-20cf1430b1d2',
            [storageHelper.user.profilePictureId]: null,
        };

        resilientStorage.setItem(tenantAlias + ' - ' + organisationAlias, JSON.stringify(uBindSession));
    });

    it('should get tenantAlias for figi from local storage.', () => {
        const tenantAlias: string = 'figi';
        const organisationAlias: string = 'figiiii';

        const sessionDataManager: SessionDataManager = new SessionDataManager();
        const value: string = sessionDataManager.getTenantOrganisationSessionValue(
            storageHelper.user.tenantAlias,
            tenantAlias,
            organisationAlias,
        );

        expect(value).toEqual(tenantAlias);
    });

    it('should get userType (ClientAdmin) from local storage.', () => {
        const tenantAlias: string = 'figi';
        const organisationAlias: string = 'figiiii';

        const sessionDataManager: SessionDataManager = new SessionDataManager();
        const value: string = sessionDataManager.getTenantOrganisationSessionValue(
            storageHelper.user.userType,
            tenantAlias,
            organisationAlias,
        );

        expect(value).toEqual('ClientAdmin');
    });

    it('should be able to get non-null accessToken from local storage', () => {
        const tenantAlias: string = 'figi';
        const organisationAlias: string = 'figiiii';

        const session: SessionDataManager = new SessionDataManager();
        const value: string = session.getTenantOrganisationSessionValue(
            storageHelper.user.accessToken,
            tenantAlias,
            organisationAlias,
        );

        expect(value).not.toBeNull();
    });

    it('should be able to find existing tenant and organisation key prefix from local storage.', () => {
        const tenantAlias: string = 'figi';
        const organisationAlias: string = 'figiiii';

        const sessionDataManager: SessionDataManager = new SessionDataManager();
        const value: boolean = sessionDataManager.tenantAndOrganisationSessionExists(
            tenantAlias,
            organisationAlias,
        );

        expect(value).toBeTruthy();
    });

    it('should get tenant and organisation key prefix in local storage should not be found return false.', () => {
        const tenantAlias: string = 'demos';
        const organisationAlias: string = 'demos';

        const sessionDataManager: SessionDataManager = new SessionDataManager();
        const value: boolean = sessionDataManager.tenantAndOrganisationSessionExists(
            tenantAlias,
            organisationAlias,
        );

        expect(value).toBeFalsy();
    });
});
