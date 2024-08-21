import { SegmentableEntityViewModel } from './segmentable-entity.viewmodel';
import { DetailListItemHelper } from '@app/helpers/detail-list-item.helper';
import { DetailsListItemCardType } from '@app/models/details-list/details-list-item-card-type.enum';
import { DetailsListFormItem } from '@app/models/details-list/details-list-form-item';
import { DetailsListGroupItemModel } from '@app/models/details-list/details-list-item-model';
import { DkimSettingsResourceModel } from '@app/resource-models/dkim-settings.resource-model';
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import { FormValidatorHelper } from '@app/helpers/form-validator.helper';
import { DetailsListFormTextItem } from '@app/models/details-list/details-list-form-text-item';
import { DetailsListItemCard } from '@app/models/details-list/details-list-item-card';
import { DetailsListFormTextAreaItem } from '@app/models/details-list/details-list-form-text-area-item';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Export DKIM settings detail view model class.
 * TODO: This class is used to create detail list for DKIM settings.
 */
export class DkimSettingsDetailViewModel implements SegmentableEntityViewModel {
    public constructor(dkimSettingsResourceModel: DkimSettingsResourceModel) {
        this.tenantId = dkimSettingsResourceModel.tenantId;
        this.dkimSettingsId = dkimSettingsResourceModel.id;
        this.organisationId = dkimSettingsResourceModel.organisationId;
        this.domainName = dkimSettingsResourceModel.domainName;
        this.privateKey = dkimSettingsResourceModel.privateKey;
        this.applicableDomainNameList = dkimSettingsResourceModel.applicableDomainNameList;
        this.dnsSelector = dkimSettingsResourceModel.dnsSelector;
        this.applicableDomainNames = dkimSettingsResourceModel.applicableDomainNames;
        this.agentOrUserIdentifier = dkimSettingsResourceModel.agentOrUserIdentifier;
    }
    public segment: string;
    public id: string;
    public deleteFromList: boolean;

    public tenantId: string;
    public dkimSettingsId: string;
    public organisationId: string;
    public agentOrUserIdentifier: string;
    public domainName: string;
    public privateKey: string;
    public dnsSelector: string;
    public applicableDomainNameList: Array<string>;
    public applicableDomainNames: string;
    public label: string;

    public static createDetailsListForCreateAndEdit(): Array<DetailsListFormItem> {
        let details: Array<DetailsListFormItem> = [];
        let validator: typeof FormValidatorHelper = FormValidatorHelper;
        const detailsCard: DetailsListItemCard = new DetailsListItemCard(
            DetailsListItemCardType.Details,
            "Details");
        details.push(DetailsListFormTextItem.create(
            detailsCard,
            "domainName",
            "Domain Name")
            .withIcon("web", IconLibrary.AngularMaterial)
            .withValidator(validator.domainNameValidator(true)));
        details.push(DetailsListFormTextAreaItem.create(
            detailsCard,
            "privateKey",
            "Private Key")
            .withIcon("key", IconLibrary.AngularMaterial)
            .withValidator(validator.required()));
        details.push(DetailsListFormTextItem.create(
            detailsCard,
            "dnsSelector",
            "DNS Selector")
            .withIcon("dns", IconLibrary.AngularMaterial)
            .withValidator(validator.required()));
        details.push(DetailsListFormTextItem.create(
            detailsCard,
            "agentOrUserIdentifier",
            "Agent or User Identifier")
            .withIcon("card-account-details", IconLibrary.AngularMaterial));
        details.push(DetailsListFormTextAreaItem.create(
            detailsCard,
            "applicableDomainNames",
            "Applicable Domain Names")
            .withValidator(validator.required())
            .withHeader("Applicable Domain Names")
            .withSubHeader("Please specify the list of domain names that should use this DKIM Configuration. "
                + "Each domain name should be entered on its own line, and must either be the Domain Name itself "
                + "or a sub-domain of the Domain Name. Wildcard selectors and multiple levels of sub-domains are "
                + "permitted."));
        return details;
    }

    public createDetailsList(): Array<DetailsListItem> {
        let details: Array<DetailsListItem> = [];
        let icons: typeof DetailListItemHelper.detailListItemIconMap =
            DetailListItemHelper.detailListItemIconMap;

        let domainName: DetailsListGroupItemModel = DetailsListGroupItemModel.create(
            "domainName", this.domainName);
        domainName.lineCount = 0;

        let privateKey: DetailsListGroupItemModel =  DetailsListGroupItemModel.create(
            "privateKey", this.privateKey);
        privateKey.lineCount = 0;

        let dnsSelector: DetailsListGroupItemModel =  DetailsListGroupItemModel.create(
            "dnsSelector", this.dnsSelector);
        dnsSelector.lineCount = 0;

        let agentOrUserIdentifier: DetailsListGroupItemModel =  DetailsListGroupItemModel.create(
            "agentOrUserIdentifier", this.agentOrUserIdentifier);
        agentOrUserIdentifier.lineCount = 0;

        let applicableDomainName: DetailsListGroupItemModel = DetailsListGroupItemModel.create(
            "applicableDomainNames", this.applicableDomainNames);
        applicableDomainName.isMultiLine = true;

        let detailModel: Array<DetailsListGroupItemModel> = [
            domainName,
            privateKey,
            dnsSelector,
            agentOrUserIdentifier,
            applicableDomainName,
        ];

        details = details.concat(DetailListItemHelper.createDetailItemGroup(
            DetailsListItemCardType.Details,
            detailModel,
            icons.mail));
        return details;
    }
}
