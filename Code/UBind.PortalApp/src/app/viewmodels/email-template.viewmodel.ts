import { EmailTemplateSetting } from '@app/models/email-template';
import { DetailListItemHelper } from '@app/helpers/detail-list-item.helper';
import { DetailsListItemCardType } from '@app/models/details-list/details-list-item-card-type.enum';
import { DetailsListFormItem } from '../models/details-list/details-list-form-item';
import { FormValidatorHelper } from '@app/helpers/form-validator.helper';
import { DetailsListItemCard } from '@app/models/details-list/details-list-item-card';
import { DetailsListFormTextItem } from '@app/models/details-list/details-list-form-text-item';
import { DetailsListFormEmailItem } from '@app/models/details-list/details-list-form-email-item';
import { IconLibrary } from '@app/models/icon-library.enum';
import { DetailsListFormTextAreaItem } from '@app/models/details-list/details-list-form-text-area-item';

/**
 * Represents an email template view model which is used to generate emails to send
 * also contains logic for edit page.
 */
export class EmailTemplateSettingViewModel {
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

    public constructor(model: EmailTemplateSetting) {
        this.id = model.id;
        this.tenant = model.tenantId;
        this.name = model.name;
        this.createdDateTime = model.createdDateTime;
        this.type = model.type;
        this.ownerId = model.ownerId;
        this.disabled = model.disabled;
        this.subject = model.subject;
        this.fromAddress = model.fromAddress;
        this.toAddress = model.toAddress;
        this.cc = model.cc;
        this.bcc = model.bcc;
        this.htmlBody = model.htmlBody;
        this.plainTextBody = model.plainTextBody;
        this.smtpServerHost = model.smtpServerHost;
        this.smtpServerPort = model.smtpServerPort;
    }

    public static createDetailsListForEdit(): Array<DetailsListFormItem> {
        const details: Array<DetailsListFormItem> = [];
        const detailsCard: DetailsListItemCard = new DetailsListItemCard(
            DetailsListItemCardType.Details,
            'Details');
        const icons: any = DetailListItemHelper.detailListItemIconMap;
        details.push(DetailsListFormTextItem.create(
            detailsCard,
            "subject",
            "Subject")
            .withValidator(FormValidatorHelper.liquidTemplateValidator())
            .withIcon(icons.mail, IconLibrary.IonicV4));
        details.push(DetailsListFormEmailItem.create(
            detailsCard,
            "fromAddress",
            "From Address")
            .withValidator(FormValidatorHelper.emailWithNameValidator()));
        details.push(DetailsListFormEmailItem.create(
            detailsCard,
            "toAddress",
            "To Address")
            .withValidator(FormValidatorHelper.liquidTemplateValidator()));
        details.push(DetailsListFormTextItem.create(
            detailsCard,
            "cc",
            "CC")
            .withValidator(FormValidatorHelper.emailSeparatedBySemiColonValidator()));
        details.push(DetailsListFormTextItem.create(
            detailsCard,
            "bcc",
            "BCC")
            .withValidator(FormValidatorHelper.emailSeparatedBySemiColonValidator()));
        details.push(DetailsListFormTextAreaItem.create(
            detailsCard,
            "htmlBody",
            "HTML Body")
            .withValidator(FormValidatorHelper.liquidTemplateValidator()));
        details.push(DetailsListFormTextAreaItem.create(
            detailsCard,
            "plainTextBody",
            "Plain Text Body")
            .withValidator(FormValidatorHelper.liquidTemplateValidator()));
        details.push(DetailsListFormTextItem.create(
            detailsCard,
            "smtpServerHost",
            "SMTP Server Host")
            .withValidator(FormValidatorHelper.smtpServerHostValidator()));
        details.push(DetailsListFormTextItem.create(
            detailsCard,
            "smtpServerPort",
            "SMTP Server Port")
            .withValidator(FormValidatorHelper.smtpServerPortValidator()));
        return details;
    }
}
