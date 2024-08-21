/**
 * A user password resource model
 */
export interface UserPasswordResourceModel {
    tenant: string;
    userId: string;
    invitationId: string;
    clearTextPassword: string;
    organisation: string;
}
