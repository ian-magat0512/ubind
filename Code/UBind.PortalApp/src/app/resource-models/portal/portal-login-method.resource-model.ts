import { AuthenticationMethodType } from "@app/models/authentication-method-type.enum";

/**
 * Base resource model for a portal login methods
 */
export interface PortalLoginMethodResourceModel {
    name: string;
    typeName: AuthenticationMethodType;
    sortOrder: number;
    authenticationMethodId: string;
    includeSignInButtonOnPortalLoginPage: boolean;
    signInButtonBackgroundColor: string;
    signInButtonIconUrl: string;
    signInButtonLabel: string;
}

/**
 * Resource model for a portal Local account login method
 */
export interface PortalLocalAccountLoginMethodResourceModel extends PortalLoginMethodResourceModel {
    allowSelfRegistration: boolean;
}

/**
 * Resource model for a portal SAML login method
 */
export interface PortalSamlLoginMethodResourceModel extends PortalLoginMethodResourceModel {
}
