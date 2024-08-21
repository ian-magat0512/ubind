import { PermissionService } from "./permission.service";

describe('PermissionService', () => {
    let service: PermissionService;

    it('getPermissions should return empty array when permission key is not found', () => {
        const authenticationService: any =
            jasmine.createSpyObj('AuthenticationService', ['permissions']);
        authenticationService.permissions = [];
        service = new PermissionService(authenticationService, null);

        expect(service.getPermissions().length).toBe(0);
    });

    it('getPermissions should return ubind.permission value when permission key is found', () => {
        const authenticationService: any =
            jasmine.createSpyObj('AuthenticationService', ['permissions']);
        let ubindPermissions: string = "[\"canDoThis\", \"canDoThat\"]";
        let parsedUbindPermissions: any = JSON.parse(ubindPermissions);
        authenticationService.permissions = parsedUbindPermissions;
        service = new PermissionService(authenticationService, null);
        expect(JSON.stringify(service.getPermissions())).toBe(JSON.stringify(parsedUbindPermissions));
    });

    it(
        'getPermissions should return 2 permissions for demos tenant given updatePolicy and cancelPolicy permissions',
        () => {
            const authenticationService: any =
                jasmine.createSpyObj('AuthenticationService', ['permissions']);
            let ubindPermissions: string = "[\"updatePolicies\", \"cancelPolicies\"]";
            let parsedUbindPermissions: any = JSON.parse(ubindPermissions);
            authenticationService.permissions = parsedUbindPermissions;
            authenticationService.tenantId = "demos";

            service = new PermissionService(authenticationService, null);
            expect(service.getPermissions().length).toBe(2);
        },
    );
});
