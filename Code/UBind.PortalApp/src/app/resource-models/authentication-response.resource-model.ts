import { Permission } from "@app/helpers";
import { AuthenticationMethodType } from "@app/models/authentication-method-type.enum";
import { DeploymentEnvironment } from "@app/models/deployment-environment.enum";

/**
 * Contains information about the user and what they can access.
 * This is fetched or returned as part of the login process.
 */
export interface UserAuthorisationModel {
    accessToken: string;
    userId: string;
    customerId: string;
    emailAddress: string;
    fullName: string;
    preferredName:  string;
    userType: string;
    tenantId: string;
    tenantAlias: string;
    environment: DeploymentEnvironment;
    organisationId: string;
    organisationAlias: string;
    profilePictureId: string;

    /**
     * The users default portal Id.
     * If their user type doesn't match when they log in, we'll try to redirect them to this portal instead.
     */
    portalId: string;

    /**
     * The ID of the organisation the portal belongs to. Sometimes a user will log into a portal from the default
     * organisation, so if they do that it's going to be different to the organisation alias of the user.
     * If this is the case, we store their credentials against that organisation key so that if they are redirected
     * to that portal, they don't have to login again.
     */
    portalOrganisationId: string;
    portalOrganisationAlias: string;

    permissions: Array<Permission>;

    authenticationMethodId?: string;
    authenticationMethodType?: AuthenticationMethodType;

    /**
     * whether the user's authentication method supports single logout.
     * This typically used by SAML authentication methods, so that when logging out, we can log them
     * out at the Identity provider, which also logs them out of other applications that use the same
     * Idp session
     */
    supportsSingleLogout?: boolean;
}
