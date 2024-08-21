import { LocalDateHelper, Permission } from '@app/helpers';
import { ProductResourceModel, QuoteExpirySettingsResourceModel } from '@app/resource-models/product.resource-model';
import { SegmentableEntityViewModel } from './segmentable-entity.viewmodel';
import { DetailsListItemCardType } from '@app/models/details-list/details-list-item-card-type.enum';
import { DetailListItemHelper } from '@app/helpers/detail-list-item.helper';
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import { DeploymentEnvironment } from '@app/models/deployment-environment.enum';
import { DetailsListGroupItemModel } from '@app/models/details-list/details-list-item-model';
import { GroupedEntityViewModel } from './grouped-entity.viewmodel';
import { SortDirection, SortedEntityViewModel } from './sorted-entity.viewmodel';
import { DetailsListItemActionIcon } from '@app/models/details-list/details-list-item-action-icon';
import { DetailsListFormItem } from '../models/details-list/details-list-form-item';
import { FormValidatorHelper } from '@app/helpers/form-validator.helper';
import { AdditionalPropertyValue } from '@app/models/additional-property-item-view.model';
import { AdditionalPropertiesHelper } from '@app/helpers/additional-properties.helper';
import { ProductAssetSyncResultModel } from '@app/models/product-asset-sync-result.model';
import { DeploymentResourceModel } from '@app/resource-models/deployment.resource-model';
import { SortAndFilterBy } from '@app/models/sort-filter-by.enum';
import { DetailsListFormTextItem } from '@app/models/details-list/details-list-form-text-item';
import { DetailsListItemCard } from '@app/models/details-list/details-list-item-card';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Export portal view model class.
 * TODO: Write a better class header: view model of portal.
 */
export class ProductViewModel implements SegmentableEntityViewModel, GroupedEntityViewModel, SortedEntityViewModel {

    public constructor(product: ProductResourceModel) {
        this.id = product.id;
        this.alias = product.alias;
        this.tenantAlias = product.tenantId;
        this.name = product.name;
        this.status = product.status;
        this.deleted = product.deleted;
        this.deleteFromList = product.deleted;
        this.disabled = product.disabled;
        this.segment = product.disabled ? 'disabled' : 'active';
        this.createdDate = LocalDateHelper.toLocalDate(product.createdDateTime);
        this.createdTime = LocalDateHelper.convertToLocalAndGetTimeOnly(product.createdDateTime);
        this.createdDateTime = product.createdDateTime;
        this.lastModifiedDateTime = product.lastModifiedDateTime;
        this.lastModifiedDate = LocalDateHelper.toLocalDate(product.lastModifiedDateTime);
        this.lastModifiedTime = LocalDateHelper.convertToLocalAndGetTimeOnly(product.lastModifiedDateTime);
        this.tenantId = product.tenantId;
        this.tenantName = product.tenantName;
        this.quoteExpirySettings = product.quoteExpirySettings;
        this.groupByValue = this.createdDate;
        this.sortByValue = product.createdDateTime;
        this.sortDirection = SortDirection.Descending;
        this.additionalPropertyValues = product.additionalPropertyValues;
    }

    public id: string;
    public segment: string;
    public name: string;
    public status: string;
    public deleted: boolean;
    public disabled: boolean;
    public createdDate: string;
    public createdTime: string;
    public createdDateTime: string;
    public lastModifiedDateTime: string;
    public lastModifiedDate: string;
    public lastModifiedTime: string;
    public tenantId: string;
    public tenantName: string;
    public tenantAlias: string;
    public alias: string;
    public deleteFromList: boolean;
    public quoteExpirySettings: QuoteExpirySettingsResourceModel;
    public product: ProductResourceModel;
    public quoteAssetsSynchronisedDate: string;
    public quoteAssetsSynchronisedTime: string;
    public claimAssetsSynchronisedDate: string;
    public claimAssetsSynchronisedTime: string;
    public deployments: Array<any>;
    public groupByValue: string;
    public sortByValue: string;
    public sortDirection: SortDirection;
    public additionalPropertyValues: Array<AdditionalPropertyValue>;

    public static createDetailsListForEdit(
        additionalPropertyValueFields: Array<AdditionalPropertyValue>,
        canEditAdditionalPropertyValues: boolean,
    ): Array<DetailsListFormItem> {
        let details: Array<DetailsListFormItem> = [];
        const icons: typeof DetailListItemHelper.detailListItemIconMap =
            DetailListItemHelper.detailListItemIconMap;
        const detailsCard: DetailsListItemCard = new DetailsListItemCard(
            DetailsListItemCardType.DeployedReleases,
            'Deployed Releases');
        details.push(DetailsListFormTextItem.create(
            detailsCard,
            "name",
            "Name")
            .withValidator(FormValidatorHelper.entityNameValidator(true))
            .withIcon(icons.cube, IconLibrary.IonicV4));
        details.push(DetailsListFormTextItem.create(
            detailsCard,
            "alias",
            "Alias")
            .withValidator(FormValidatorHelper.aliasValidator(true)));

        if (canEditAdditionalPropertyValues) {
            let additionalPropertyValues: Array<DetailsListFormItem> =
                AdditionalPropertiesHelper.getDetailListForEdit(additionalPropertyValueFields);
            details = details.concat(additionalPropertyValues);
        }

        return details;
    }

    public static isDeployedInCurrentEnvironment(
        currentEnvironment: DeploymentEnvironment,
        productAssetSyncResultModel: ProductAssetSyncResultModel,
        deploymentsResourceModel: Array<DeploymentResourceModel>,
    ): boolean {
        if (currentEnvironment === DeploymentEnvironment.Development
            && productAssetSyncResultModel != null) {
            return true;
        } else {
            const deployments: Array<DeploymentResourceModel> = deploymentsResourceModel.filter(
                (d: DeploymentResourceModel) => d.environment = currentEnvironment,
            );
            return deployments.length > 0;
        }
    }

    public createDetailsList(source: any): Array<DetailsListItem> {
        let details: Array<DetailsListItem> = [];
        const icons: any = DetailListItemHelper.detailListItemIconMap;

        const detailModel: Array<DetailsListGroupItemModel> = [
            DetailsListGroupItemModel.create('name', this.name),
            DetailsListGroupItemModel.create('alias', this.alias),
            DetailsListGroupItemModel.create('status', this.status),
        ];

        details = details.concat(
            DetailListItemHelper.createDetailItemGroup(
                DetailsListItemCardType.Details,
                detailModel,
                icons.cube,
            ),
        );

        let dates: Array<DetailsListGroupItemModel> = [
            DetailsListGroupItemModel.create('createdDate', this.createdDate),
            DetailsListGroupItemModel.create('createdTime', this.createdTime),
            DetailsListGroupItemModel.create('lastModifiedDate', this.lastModifiedDate),
            DetailsListGroupItemModel.create('lastModifiedTime', this.lastModifiedTime),
            DetailsListGroupItemModel.create('quoteAssetsSynchronisedDate', this.quoteAssetsSynchronisedDate),
            DetailsListGroupItemModel.create('quoteAssetsSynchronisedTime', this.quoteAssetsSynchronisedTime),
            DetailsListGroupItemModel.create('claimAssetsSynchronisedDate', this.claimAssetsSynchronisedDate),
            DetailsListGroupItemModel.create('claimAssetsSynchronisedTime', this.claimAssetsSynchronisedTime),
        ];

        details = details.concat(
            DetailListItemHelper.createDetailItemGroup(DetailsListItemCardType.Dates, dates));

        AdditionalPropertiesHelper.createDetailsList(details, this.additionalPropertyValues);

        const productionRelease: any =
            this.deployments.find((x: any) => x.environment === DeploymentEnvironment.Production).release;
        const stagingRelease: any =
            this.deployments.find((x: any) => x.environment === DeploymentEnvironment.Staging).release;

        let productionReleaseAction: DetailsListItemActionIcon =
            DetailListItemHelper.createAction(
                () => source.releaseSelected(productionRelease),
                'link',
                IconLibrary.IonicV4,
                [ Permission.ManageProducts ],
            );
        let stagingReleaseAction: DetailsListItemActionIcon =
            DetailListItemHelper.createAction(
                () => source.releaseSelected(stagingRelease),
                'link',
                IconLibrary.IonicV4,
                [ Permission.ManageProducts ],
            );

        let relationships: Array<DetailsListGroupItemModel> = [
            DetailsListGroupItemModel.create(
                "defaultProductionRelease",
                productionRelease ? productionRelease.number : "None",
                null,
                null,
                productionRelease ? productionReleaseAction : null),
            DetailsListGroupItemModel.create(
                "defaultStagingRelease",
                stagingRelease ? stagingRelease.number : "None",
                null,
                null,
                stagingRelease ? stagingReleaseAction : null),
        ];

        details = details.concat(DetailListItemHelper.createDetailItemGroup(
            DetailsListItemCardType.Deployment,
            relationships,
            icons.cloudUpload));

        return details;
    }

    public addQuoteSynchronisedDateTime(assetsSynchronisedDateTime: string): void {
        this.quoteAssetsSynchronisedDate =
            LocalDateHelper.toLocalDate(assetsSynchronisedDateTime);
        this.quoteAssetsSynchronisedTime =
            LocalDateHelper.convertToLocalAndGetTimeOnly(assetsSynchronisedDateTime);
    }

    public addClaimSynchronisedDateTime(assetsSynchronisedDateTime: string): void {
        this.claimAssetsSynchronisedDate =
            LocalDateHelper.toLocalDate(assetsSynchronisedDateTime);
        this.claimAssetsSynchronisedTime =
            LocalDateHelper.convertToLocalAndGetTimeOnly(assetsSynchronisedDateTime);
    }

    /**
     * After syncing, we might need to show new properties for the quote/claim assets syncrhonised date and time.
     * If we don't we'll at least need to update the existing ones to show the new date and time
     */
    public updateDetailsListAfterSync(detailsListItems: Array<DetailsListItem>): Array<DetailsListItem> {
        let updatedListItems: Array<DetailsListItem> = new Array<DetailsListItem>(...detailsListItems);
        let itemsAdded: boolean = false;
        if (this.claimAssetsSynchronisedDate) {
            // try to find the existing list item
            let claimAssetsSynchronisedDateItem: DetailsListItem
                = detailsListItems.find((item: DetailsListItem) =>
                    item.Description == "Claim Assets Synchronised Date");
            let claimAssetsSynchronisedTimeItem: DetailsListItem
                = detailsListItems.find((item: DetailsListItem) =>
                    item.Description == "Claim Assets Synchronised Time");
            if (claimAssetsSynchronisedDateItem && claimAssetsSynchronisedTimeItem) {
                claimAssetsSynchronisedDateItem.setDisplayValue(this.claimAssetsSynchronisedDate);
                claimAssetsSynchronisedTimeItem.setDisplayValue(this.claimAssetsSynchronisedTime);
            } else {
                // it doesn't exist, let's create it:
                let newItemModels: Array<DetailsListGroupItemModel> = [
                    DetailsListGroupItemModel.create('claimAssetsSynchronisedDate', this.claimAssetsSynchronisedDate),
                    DetailsListGroupItemModel.create('claimAssetsSynchronisedTime', this.claimAssetsSynchronisedTime),
                ];
                let newItems: Array<DetailsListItem>
                    = DetailListItemHelper.createDetailItemGroup(DetailsListItemCardType.Dates, newItemModels);
                updatedListItems.push(...newItems);
                itemsAdded = true;
            }
        }
        if (this.quoteAssetsSynchronisedDate) {
            // try to find the existing list item
            let quoteAssetsSynchronisedDateItem: DetailsListItem
                = detailsListItems.find((item: DetailsListItem) =>
                    item.Description == "Quote Assets Synchronised Date");
            let quoteAssetsSynchronisedTimeItem: DetailsListItem
                = detailsListItems.find((item: DetailsListItem) =>
                    item.Description == "Quote Assets Synchronised Time");
            if (quoteAssetsSynchronisedDateItem && quoteAssetsSynchronisedTimeItem) {
                quoteAssetsSynchronisedDateItem.setDisplayValue(this.quoteAssetsSynchronisedDate);
                quoteAssetsSynchronisedTimeItem.setDisplayValue(this.quoteAssetsSynchronisedTime);
            } else {
                // it doesn't exist, let's create it:
                let newItemModels: Array<DetailsListGroupItemModel> = [
                    DetailsListGroupItemModel.create('quoteAssetsSynchronisedDate', this.quoteAssetsSynchronisedDate),
                    DetailsListGroupItemModel.create('quoteAssetsSynchronisedTime', this.quoteAssetsSynchronisedTime),
                ];
                let newItems: Array<DetailsListItem>
                    = DetailListItemHelper.createDetailItemGroup(DetailsListItemCardType.Dates, newItemModels);
                updatedListItems.push(...newItems);
                itemsAdded = true;
            }
        }

        return itemsAdded ? updatedListItems : detailsListItems;
    }

    public addDeployments(deployments: Array<any>): void {
        this.deployments = deployments;
    }

    public setGroupByValue(
        productList: Array<ProductViewModel>,
        groupBy: string,
    ): Array<ProductViewModel> {
        if (groupBy === SortAndFilterBy.ProductName) {
            productList.forEach((item: ProductViewModel) => item.groupByValue = item.name);
        } else if (groupBy === SortAndFilterBy.LastModifiedDate) {
            productList.forEach((item: ProductViewModel) => item.groupByValue = item.lastModifiedDate);
        } else {
            productList.forEach((item: ProductViewModel) => item.groupByValue = item.createdDate);
        }

        return productList;
    }

    public setSortOptions(
        productList: Array<ProductViewModel>,
        sortBy: string,
        sortDirection: SortDirection,
    ): Array<ProductViewModel> {
        sortDirection = sortDirection == null ? SortDirection.Descending : sortDirection;

        if (sortBy === SortAndFilterBy.ProductName) {
            productList.forEach((item: ProductViewModel) => {
                item.sortByValue = item.name;
                item.sortDirection = sortDirection;
            });
        } else if (sortBy === SortAndFilterBy.LastModifiedDate) {
            productList.forEach((item: ProductViewModel) => {
                item.sortByValue = item.lastModifiedDateTime;
                item.sortDirection = sortDirection;
            });
        } else {
            productList.forEach((item: ProductViewModel) => {
                item.sortByValue = item.createdDateTime;
                item.sortDirection = sortDirection;
            });
        }

        return productList;
    }
}
