/**
 * A user password invitation request resource model
 */
export interface UserPasswordInvitationRequestModel {
    email: string;
    organisation: string;
    environment: string;
    isPasswordExpired: boolean;
    tenant?: string;
}
