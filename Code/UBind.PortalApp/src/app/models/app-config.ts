import { PortalUserType } from "./portal-user-type.enum";

/**
 * Defines the structure of configuration used by the portal app to run
 */
export interface AppConfig {
    portal: {
        auth0: {
            redirectUri: string;
            audience: string;
        };
        api: {
            baseUrl: string;
            accountUrl: string;
        };
        baseUrl: string;
        environment: string;
        tenantId: string;
        customDomain: string;
        tenantAlias: string;
        tenantName: string;
        alias: string;
        title: string;
        stylesheetUrl: string;
        styles: string;
        organisationId: string;
        organisationAlias: string;
        organisationName: string;
        isDefaultOrganisation: boolean;
        isMutual: boolean;
        portalId: string;
        portalAlias: string;
        portalUserType: PortalUserType;
        isDefaultAgentPortal: boolean;
    };
    formsApp: {
        baseUrl: string;
    };
}
