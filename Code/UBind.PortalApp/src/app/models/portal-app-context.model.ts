import { PortalUserType } from "@app/models/portal-user-type.enum";

/**
 * Resource model for a the essential contextual information for the environmet in which the portal app runs.
 */
export interface PortalAppContextModel {
    tenantId: string;
    tenantName: string;
    organisationId: string;
    organisationAlias: string;
    organisationName: string;
    isDefaultOrganisation: boolean;
    portalId: string;
    portalAlias: string;
    portalUserType: PortalUserType;
    isDefaultAgentPortal: boolean;
    portalTitle: string;
    portalStylesheetUrl: string;
    portalStyles: string;
    customDomain: string;
    appBaseUrl: string;
    isMutual: boolean;
}
