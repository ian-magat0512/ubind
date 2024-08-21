import { EmailTemplateSetting } from "@app/models";
/**
 * Model used to update the email template setting.
 */
export class EmailTemplateSettingUpdateModel {

    public constructor(setting: EmailTemplateSetting) {
        this.id = setting.id;
        this.tenant = setting.tenantId;
        this.name = setting.name;
        this.createdDateTime = setting.createdDateTime;
        this.type = setting.type;
        this.ownerId = setting.ownerId;
        this.disabled = setting.disabled;
        this.subject = setting.subject;
        this.fromAddress = setting.fromAddress;
        this.toAddress = setting.toAddress;
        this.cc = setting.cc;
        this.bcc = setting.bcc;
        this.htmlBody = setting.htmlBody;
        this.plainTextBody = setting.plainTextBody;
        this.smtpServerHost = setting.smtpServerHost;
        this.smtpServerPort = setting.smtpServerPort;
    }

    public id: string;
    public tenant: string;
    public name: string;
    public createdDateTime: string;
    public type: string;
    public ownerId: string;
    public disabled: boolean;
    public subject: string;
    public fromAddress: string;
    public toAddress: string;
    public cc: string;
    public bcc: string;
    public htmlBody: string;
    public plainTextBody: string;
    public smtpServerHost: string;
    public smtpServerPort?: number;
}
