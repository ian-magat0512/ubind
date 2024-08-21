import { AuthenticationMethodType } from "@app/models/authentication-method-type.enum";

/**
 * Represents a portal sign in method.
 */
export interface PortalSignInMethodResourceModel {
    tenantId: string;
    portalId: string;
    authenticationMethodId: string;
    isEnabled: boolean;
    sortOrder: number;
    name: string;
    typeName: AuthenticationMethodType;
}
