/**
 * Export DkimTestEmailResourceModel
 * This class is used for Sending DKIM test email.
 */
export interface DkimTestEmailResourceModel {
    tenant: string;
    dkimSettingsId: string;
    organisationId: string;
    senderEmailAddress: string;
    recipientEmailAddress: string;
}
