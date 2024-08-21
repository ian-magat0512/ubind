/**
 * Resource model for account login credentials, for sending to the api as part of a login request.
 */
export interface AccountLoginCredentialsResourceModel {
    tenant: string;
    organisation: string;
    emailAddress: string;
    plaintextPassword: string;
}
