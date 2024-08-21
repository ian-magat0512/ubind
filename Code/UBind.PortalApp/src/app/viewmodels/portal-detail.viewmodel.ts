import { LocalDateHelper } from '@app/helpers';
import { SegmentableEntityViewModel } from './segmentable-entity.viewmodel';
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import { DetailListItemHelper } from '@app/helpers/detail-list-item.helper';
import { DetailsListItemCardType } from '@app/models/details-list/details-list-item-card-type.enum';
import { PortalDetailResourceModel, PortalResourceModel } from '@app/resource-models/portal/portal.resource-model';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { DetailsListFormItem } from '../models/details-list/details-list-form-item';
import { DetailsListGroupItemModel } from '@app/models/details-list/details-list-item-model';
import { DetailsListItemActionIcon } from '@app/models/details-list/details-list-item-action-icon';
import { FormValidatorHelper } from '@app/helpers/form-validator.helper';
import { AdditionalPropertyValue } from '@app/models/additional-property-item-view.model';
import { AdditionalPropertiesHelper } from '@app/helpers/additional-properties.helper';
import { DetailsListItemCard } from '@app/models/details-list/details-list-item-card';
import { DetailsListFormTextItem } from '@app/models/details-list/details-list-form-text-item';
import { IconLibrary } from '@app/models/icon-library.enum';
import { DetailsListFormRadioItem } from '@app/models/details-list/details-list-form-radio-item';
import { PortalUserType } from '@app/models/portal-user-type.enum';

/**
 * Export portal detail view model class.
 * TODO: Write a better class header: view model of portal detail.
 */
export class PortalDetailViewModel implements SegmentableEntityViewModel {
    public constructor(portal: PortalDetailResourceModel) {
        this.id = portal.id;
        this.name = portal.name;
        this.alias = portal.alias;
        this.title = portal.title;
        this.stylesheetUrl = portal.stylesheetUrl;
        this.styles = portal.styles;
        this.deleted = portal.deleted;
        this.deleteFromList = this.deleted;
        this.disabled = portal.disabled;
        this.isDefault = portal.isDefault;
        this.segment = this.disabled ? 'disabled' : 'active';
        this.createdDate = LocalDateHelper.toLocalDate(portal.createdDateTime);
        this.createdTime = LocalDateHelper.convertToLocalAndGetTimeOnly(portal.createdDateTime);
        this.lastModifiedDate = LocalDateHelper.toLocalDate(portal.lastModifiedDateTime);
        this.lastModifiedTime = LocalDateHelper.convertToLocalAndGetTimeOnly(portal.lastModifiedDateTime);
        this.tenantId = portal.tenantId;
        this.tenantName = portal.tenantName;
        this.userType = portal.userType;
        this.organisationId = portal.organisationId;
        this.organisationName = portal.organisationName;
        this.productionUrl = portal.productionUrl;
        this.stagingUrl = portal.stagingUrl;
        this.defaultUrl = portal.defaultUrl;
        this.developmentUrl = portal.developmentUrl;
        this.additionalPropertyValues = portal.additionalPropertyValues;
    }

    public id: string;
    public segment: string;
    public name: string;
    public alias: string;
    public stylesheetUrl: string;
    public styles: string;
    public deleted: boolean;
    public disabled: boolean;
    public isDefault: boolean;
    public createdDate: string;
    public lastModifiedDate: string;
    public lastModifiedTime: string;
    public masterTenant: string;
    public deleteFromList: boolean;
    public createdTime: string;
    public title: string;
    public tenantId: string;
    public tenantName: string;
    public userType: PortalUserType;
    public organisationId: string;
    public organisationName: string;
    public productionUrl: string;
    public stagingUrl: string;
    public developmentUrl: string;
    public defaultUrl: string;
    public resourceModel: PortalResourceModel;
    public additionalPropertyValues: Array<AdditionalPropertyValue>;

    public static createDetailsListForEdit(
        additionalPropertyValueFields: Array<AdditionalPropertyValue>,
        hasEditAdditionalPropertyPermission: boolean,
        isEdit: boolean,
    ): Array<DetailsListFormItem> {
        const icons: typeof DetailListItemHelper.detailListItemIconMap =
            DetailListItemHelper.detailListItemIconMap;
        const detailsCard: DetailsListItemCard = new DetailsListItemCard(
            DetailsListItemCardType.Details,
            'Details');
        let details: Array<DetailsListFormItem> = [];
        details.push(DetailsListFormTextItem.create(
            detailsCard,
            "name",
            "Name")
            .withValidator(FormValidatorHelper.nameValidator(true))
            .withIcon(icons.browsers, IconLibrary.IonicV4));
        details.push(DetailsListFormTextItem.create(
            detailsCard,
            "alias",
            "Alias")
            .withValidator(FormValidatorHelper.aliasValidator(true)));
        details.push(DetailsListFormTextItem.create(
            detailsCard,
            "title",
            "Title")
            .withValidator(FormValidatorHelper.nameValidator(true)));

        // TODO: in future we will stop you being able to edit the user type after creating the portal.
        // Instead, you will have to create a new portal.
        /*
        if (!isEdit) {
        */
        details.push(DetailsListFormRadioItem.create(
            detailsCard,
            "userType",
            "Who can access this portal?")
            .withValidator<DetailsListFormRadioItem>(FormValidatorHelper.required())
            .withOption({ label: 'Agents', value: 'agent' })
            .withOption({ label: 'Customers', value: 'customer' }));
        /*                
        }
        */

        if (hasEditAdditionalPropertyPermission) {
            let additionalPropertyValues: Array<DetailsListFormItem> =
                AdditionalPropertiesHelper.getDetailListForEdit(additionalPropertyValueFields);
            details = details.concat(additionalPropertyValues);
        }
        return details;
    }

    public createDetailsList(
        navProxy: NavProxyService,
        tenantAlias: string,
        isMasterUser: boolean,
        canViewAdditionalPropertyValues: boolean,
    ): Array<DetailsListItem> {
        let details: Array<DetailsListItem> = [];
        const icons: any = DetailListItemHelper.detailListItemIconMap;
        let tenantAction: DetailsListItemActionIcon = DetailListItemHelper.createAction(
            () => navProxy.gotoTenant(tenantAlias));
        let organisationAction: DetailsListItemActionIcon =
            DetailListItemHelper.createAction(
                () => navProxy.goToOrganisation(this.organisationId));

        let detailItems: Array<DetailsListGroupItemModel> = [
            DetailsListGroupItemModel.create("name", this.name),
            DetailsListGroupItemModel.create("alias", this.alias),
            DetailsListGroupItemModel.create("title", this.title),
            DetailsListGroupItemModel.create("userType", this.userType),
        ];

        if (this.isDefault) {
            detailItems.push(DetailsListGroupItemModel.create("default", "Yes"));
        }

        detailItems.push(DetailsListGroupItemModel.create("status", this.disabled ? 'Disabled' : 'Active'));

        let stylingItems: Array<DetailsListGroupItemModel> = [];
        stylingItems.push(DetailsListGroupItemModel.create("stylesheetUrl", this.stylesheetUrl, null, null, null, 5));
        let stylesSummary: string = this.styles
            ? this.styles.trimLeft().substring(0, 100) + '...'
            : null;

        if (stylesSummary) {
            stylingItems.push(DetailsListGroupItemModel.create("styles", stylesSummary, null, null, null, 5));
        }

        details = details.concat(
            DetailListItemHelper.createDetailItemGroup(
                DetailsListItemCardType.Details, detailItems, icons.browsers));

        details = details.concat(
            DetailListItemHelper.createDetailItemGroup(
                DetailsListItemCardType.Theme, stylingItems, icons.brush));

        let relationships: Array<DetailsListGroupItemModel> = new Array<DetailsListGroupItemModel>();
        if (isMasterUser) {
            relationships.push(DetailsListGroupItemModel.create("tenant", this.tenantName, null, null, tenantAction));
        }
        relationships.push(
            DetailsListGroupItemModel.create("organisation", this.organisationName, null, null, organisationAction));

        details = details.concat(
            DetailListItemHelper.createDetailItemGroup(DetailsListItemCardType.Relationships, relationships));

        const dates: Array<DetailsListGroupItemModel> = [
            DetailsListGroupItemModel.create('createdDate', this.createdDate),
            DetailsListGroupItemModel.create('createdTime', this.createdTime),
            DetailsListGroupItemModel.create('lastModifiedDate', this.lastModifiedDate),
            DetailsListGroupItemModel.create('lastModifiedTime', this.lastModifiedTime),
        ];

        details = details.concat(
            DetailListItemHelper.createDetailItemGroup(DetailsListItemCardType.Dates, dates));

        if (canViewAdditionalPropertyValues) {
            AdditionalPropertiesHelper.createDetailsList(details, this.additionalPropertyValues);
        }
        return details;
    }
}
