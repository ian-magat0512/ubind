import { PortalUserType } from "@app/models/portal-user-type.enum";

/**
 * The context which the forms app runs within
 */
export interface FormsAppContextModel {
    tenantId: string;
    tenantName: string;
    tenantAlias: string;
    organisationId: string;
    organisationName: string;
    organisationAlias: string;
    productId: string;
    productAlias: string;
    portalAlias: string;
    portalUserType: PortalUserType;
    portalTitle: string;
    portalId: string;
    isDefaultOrganisation: boolean;
    portalStylesheetUrl: string;
    portalStyles: string;
}
