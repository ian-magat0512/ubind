
/**
 * Represents the user's identity within a remote identity provider
 */
export interface UserLinkedIdentity {
    authenticationMethodId: string;
    authenticationMethodName: string;
    authenticationMethodType: string;
    uniqueId: string;
}
