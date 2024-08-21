/**
 * Represents an email template which is used to generate emails to send
 */
export interface EmailTemplateSetting {
    id: string;
    tenantId: string;
    name: string;
    createdDateTime: string;
    type: string;
    ownerId: string;
    disabled: boolean;
    subject: string;
    fromAddress: string;
    toAddress: string;
    cc: string;
    bcc: string;
    htmlBody: string;
    plainTextBody: string;
    smtpServerHost: string;
    smtpServerPort?: number;
}
