import { TenantResourceModel } from '@app/resource-models/tenant.resource-model';
import { LocalDateHelper } from '@app/helpers';
import { SegmentableEntityViewModel } from './segmentable-entity.viewmodel';
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import { DetailListItemHelper } from '@app/helpers/detail-list-item.helper';
import { DetailsListItemCardType } from '@app/models/details-list/details-list-item-card-type.enum';
import { DetailsListFormItem } from '../models/details-list/details-list-form-item';
import { DetailsListGroupItemModel } from '@app/models/details-list/details-list-item-model';
import { SortedEntityViewModel, SortDirection } from './sorted-entity.viewmodel';
import { FormValidatorHelper } from '@app/helpers/form-validator.helper';
import { AdditionalPropertyValue } from '@app/models/additional-property-item-view.model';
import { AdditionalPropertiesHelper } from '@app/helpers/additional-properties.helper';
import { SortAndFilterBy } from '@app/models/sort-filter-by.enum';
import { DetailsListItemCard } from '@app/models/details-list/details-list-item-card';
import { DetailsListFormTextItem } from '@app/models/details-list/details-list-form-text-item';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Export tenant View Model Class.
 * This class is used define Tenant Details for creating and updating tenant.
 */
export class TenantViewModel implements SegmentableEntityViewModel, SortedEntityViewModel {
    public constructor(tenant: TenantResourceModel) {
        this.id = tenant.id;
        this.name = tenant.name;
        this.alias = tenant.alias;
        this.customDomain = tenant.customDomain;
        this.deleted = tenant.deleted;
        this.deleteFromList = tenant.deleted;
        this.disabled = tenant.disabled;
        this.segment = this.disabled ? 'disabled' : 'active';
        this.createdDateTimestamp = tenant.createdDateTime;
        this.createdDate = LocalDateHelper.toLocalDate(tenant.createdDateTime);
        this.createdTime = LocalDateHelper.convertToLocalAndGetTimeOnly(tenant.createdDateTime);
        this.lastModifiedDateTime = tenant.lastModifiedDateTime;
        this.lastModifiedDate = LocalDateHelper.toLocalDate(tenant.lastModifiedDateTime);
        this.lastModifiedTime = LocalDateHelper.convertToLocalAndGetTimeOnly(tenant.lastModifiedDateTime);
        this.masterTenant = tenant.masterTenant;
        this.tenant = tenant;
        this.additionalPropertyValues = tenant.additionalPropertyValues;
        this.sortByValue = tenant.name;
        this.sortDirection = SortDirection.Descending;
        this.defaultOrganisationId = tenant.defaultOrganisationId;
    }

    public id: string;
    public segment: string;
    public name: string;
    public alias: string;
    public customDomain: string;
    public deleted: boolean;
    public disabled: boolean;
    public createdDate: string;
    public lastModifiedDateTime: string;
    public lastModifiedDate: string;
    public lastModifiedTime: string;
    public masterTenant: string;
    public deleteFromList: boolean;
    public createdTime: string;
    public tenant: TenantResourceModel;
    public createdDateTimestamp: string;
    public sortByValue: string;
    public sortDirection: SortDirection;
    public additionalPropertyValues: Array<AdditionalPropertyValue>;
    public defaultOrganisationId: string;

    public static createDetailsListForEdit(
        additionalPropertyValueFields: Array<AdditionalPropertyValue> = [],
        canEditAdditionalPropertyValues: boolean,
    ): Array<DetailsListFormItem> {
        let details: Array<DetailsListFormItem> = [];
        const icons: typeof DetailListItemHelper.detailListItemIconMap =
            DetailListItemHelper.detailListItemIconMap;
        const detailsCard: DetailsListItemCard = new DetailsListItemCard(
            DetailsListItemCardType.Details,
            'Details');
        details.push(DetailsListFormTextItem.create(
            detailsCard,
            "name",
            "Name")
            .withValidator(FormValidatorHelper.entityNameValidator(true))
            .withIcon(icons.cloudCircle, IconLibrary.IonicV4));
        details.push(DetailsListFormTextItem.create(
            detailsCard,
            "alias",
            "Alias")
            .withValidator(FormValidatorHelper.aliasValidator(true)));
        details.push(DetailsListFormTextItem.create(
            detailsCard,
            "customDomain",
            "Custom Domain")
            .withValidator(FormValidatorHelper.domainNameValidator()));
        if (canEditAdditionalPropertyValues) {
            let additionalPropertyValues: Array<DetailsListFormItem> =
                AdditionalPropertiesHelper.getDetailListForEdit(additionalPropertyValueFields);
            details = details.concat(additionalPropertyValues);
        }
        return details;
    }

    public createDetailsList(canViewAdditionalPropertyValues: boolean): Array<DetailsListItem> {
        let details: Array<DetailsListItem> = [];
        const icons: any = DetailListItemHelper.detailListItemIconMap;

        let detailModel: Array<DetailsListGroupItemModel> = [
            DetailsListGroupItemModel.create("name", this.name),
            DetailsListGroupItemModel.create("alias", this.alias),
            DetailsListGroupItemModel.create(
                "customDomain",
                this.customDomain,
                null,
                null,
                null,
                5),
            DetailsListGroupItemModel.create("status", this.disabled ? 'Disabled' : 'Active'),
        ];

        details = details.concat(
            DetailListItemHelper.createDetailItemGroup(
                DetailsListItemCardType.Details, detailModel, icons.cloudCircle));

        const dates: Array<DetailsListGroupItemModel> = [
            DetailsListGroupItemModel.create('createdDate', this.createdDate),
            DetailsListGroupItemModel.create('createdTime', this.createdTime),
            DetailsListGroupItemModel.create('lastModifiedDate', this.lastModifiedDate),
            DetailsListGroupItemModel.create('lastModifiedTime', this.lastModifiedTime),
        ];

        details = details.concat(DetailListItemHelper.createDetailItemGroup(
            DetailsListItemCardType.Dates,
            dates));
        if (canViewAdditionalPropertyValues) {
            AdditionalPropertiesHelper.createDetailsList(details, this.additionalPropertyValues);
        }
        return details;
    }

    public setSortOptions(
        tenantList: Array<TenantViewModel>,
        sortBy: string,
        sortDirection: SortDirection,
    ): Array<TenantViewModel> {
        sortDirection = sortDirection == null ? SortDirection.Ascending : sortDirection;

        if (sortBy == SortAndFilterBy.CreatedDate) {
            tenantList.forEach((item: TenantViewModel) => {
                item.sortByValue = item.createdDateTimestamp;
                item.sortDirection = sortDirection;
            });
        } else if (sortBy == SortAndFilterBy.LastModifiedDate) {
            tenantList.forEach((item: TenantViewModel) => {
                item.sortByValue = item.lastModifiedDateTime;
                item.sortDirection = sortDirection;
            });
        } else {
            tenantList.forEach((item: TenantViewModel) => {
                item.sortByValue = item.name;
                item.sortDirection = sortDirection;
            });
        }

        return tenantList;
    }
}
