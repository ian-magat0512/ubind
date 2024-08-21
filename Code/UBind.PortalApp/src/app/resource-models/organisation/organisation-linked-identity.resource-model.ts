
/**
 * Represents the organisations record in a remote identity provider.
 * This is typically used in SAML integrations.
 */
export interface OrganisationLinkedIdentity {
    authenticationMethodId: string;
    authenticationMethodName: string;
    authenticationMethodType: string;
    uniqueId: string;
}

/**
 * Model for upserting a linked identity for an organisation within the external identity provider.
 */
export interface OrganisationLinkedIdentityUpsertModel {
    authenticationMethodId: string;
    uniqueId: string;
}
