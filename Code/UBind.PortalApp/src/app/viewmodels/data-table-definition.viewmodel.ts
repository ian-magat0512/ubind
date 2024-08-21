import { ValidatorFn } from "@angular/forms";
import { DetailListItemHelper } from "@app/helpers/detail-list-item.helper";
import { FormValidatorHelper } from "@app/helpers/form-validator.helper";
import { DetailsListFormItem } from "@app/models/details-list/details-list-form-item";
import { DetailsListItem } from "@app/models/details-list/details-list-item";
import { DataTableDefinitionResourceModel } from "@app/resource-models/data-table-definition.resource-model";
import { SortDirection, SortedEntityViewModel } from "./sorted-entity.viewmodel";
import { SortAndFilterBy } from '@app/models/sort-filter-by.enum';
import { DetailsListFormTextItem } from "@app/models/details-list/details-list-form-text-item";
import { DetailsListItemCard } from "@app/models/details-list/details-list-item-card";
import { DetailsListItemCardType } from "@app/models/details-list/details-list-item-card-type.enum";
import { DetailsListFormTextAreaItem } from "@app/models/details-list/details-list-form-text-area-item";
import { DetailsListGroupItemModel } from "@app/models/details-list/details-list-item-model";
import { LocalDateHelper } from "@app/helpers/local-date.helper";
import { DataTableSchema } from "@app/models/data-table-definition.model";
import { DetailsListFormCheckboxItem } from "@app/models/details-list/details-list-form-checkbox-item";

/**
 * Data table view model.
 */
export class DataTableDefinitionViewModel implements SortedEntityViewModel {
    public id: string;
    public name: string;
    public alias: string;
    public recordCount: number;
    public columnCount: number;
    public tableSchema: DataTableSchema;
    public deleteFromList: boolean;
    public sortByValue: string;
    public sortDirection: SortDirection;
    public createdDateTime: string;
    public createdDate: string;
    public createdTime: string;
    public lastModifiedDateTime: string;
    public lastModifiedDate: string;
    public lastModifiedTime: string;
    public groupByValue: string;
    public memoryCachingEnabled: boolean;
    public cacheExpiryInSeconds: number;

    public static readonly dataTableFormItemAlias: { [key: string]: string } = {
        name: "name",
        alias: "alias",
        tableSchemaJson: "tableSchemaJson",
        csvData: "csvData",
        memoryCachingEnabled: "memoryCachingEnabled",
        cacheExpiryInSeconds: "cacheExpiryInSeconds",
    };

    public constructor(model: DataTableDefinitionResourceModel) {
        this.id = model.id;
        this.name = model.name;
        this.alias = model.alias;
        this.memoryCachingEnabled = model.memoryCachingEnabled;
        this.cacheExpiryInSeconds = model.cacheExpiryInSeconds;
        this.recordCount = model.recordCount;
        this.columnCount = model.columnCount;
        this.tableSchema = model.tableSchema;
        this.createdDateTime = model.createdTimestamp;
        this.createdDate = LocalDateHelper.toLocalDate(model.createdTimestamp);
        this.createdTime = LocalDateHelper.convertToLocalAndGetTimeOnly(model.createdTimestamp);
        this.lastModifiedDateTime = model.lastModifiedTimestamp;
        this.lastModifiedDate = LocalDateHelper.toLocalDate(model.lastModifiedTimestamp);
        this.lastModifiedTime = LocalDateHelper.convertToLocalAndGetTimeOnly(model.lastModifiedTimestamp);
        this.sortByValue = model.createdTimestamp;
        this.sortDirection = SortDirection.Ascending;
        this.groupByValue = this.createdDate;
    }

    public static createFormDetailsList(
        csvValidator: ValidatorFn, jsonSchemaValidator: ValidatorFn): Array<DetailsListFormItem> {
        let detailsListFormItems: Array<DetailsListFormItem> = [];
        const icons: typeof DetailListItemHelper.detailListItemIconMap =
            DetailListItemHelper.detailListItemIconMap;
        let detailsCard: DetailsListItemCard
            = new DetailsListItemCard(DetailsListItemCardType.Details, 'Details');
        detailsListFormItems.push(DetailsListFormTextItem.create(
            detailsCard,
            this.dataTableFormItemAlias.name,
            "Name")
            .withValidator(FormValidatorHelper.entityNameValidator(true))
            .withIcon(icons.grid));
        detailsListFormItems.push(DetailsListFormTextItem.create(
            detailsCard,
            this.dataTableFormItemAlias.alias,
            "Alias")
            .withValidator(FormValidatorHelper.aliasValidator(true)));
        detailsListFormItems.push(DetailsListFormTextAreaItem.create(
            detailsCard,
            this.dataTableFormItemAlias.tableSchemaJson,
            "Configuration JSON")
            .withValidator([...FormValidatorHelper.required(), FormValidatorHelper.validJson, jsonSchemaValidator]));
        detailsListFormItems.push(DetailsListFormTextAreaItem.create(
            detailsCard,
            this.dataTableFormItemAlias.csvData,
            "CSV Data")
            .withValidator([...FormValidatorHelper.required(), csvValidator]));
        detailsListFormItems.push(DetailsListFormCheckboxItem.create(
            detailsCard,
            this.dataTableFormItemAlias.memoryCachingEnabled,
            "Cache the data table in memory"));
        detailsListFormItems.push(DetailsListFormTextItem.create(
            detailsCard,
            this.dataTableFormItemAlias.cacheExpiryInSeconds,
            "Cache expiry in seconds")
            .withValidator(FormValidatorHelper.positiveNoneZeroWholeNumberValidator(true)));
        return detailsListFormItems;
    }

    public dataTableDetailList(): Array<DetailsListItem> {
        let details: Array<DetailsListItem> = [];
        let icons: typeof DetailListItemHelper.detailListItemIconMap = DetailListItemHelper.detailListItemIconMap;

        let detailListModel: Array<DetailsListGroupItemModel> = [
            DetailsListGroupItemModel.create("name", this.name),
            DetailsListGroupItemModel.create("alias", this.alias),
            DetailsListGroupItemModel.create("columns", `${this.columnCount}`),
            DetailsListGroupItemModel.create("records", this.recordCount === 0 ? "No" : `${this.recordCount}`),
            DetailsListGroupItemModel.create("cacheInMemory", this.memoryCachingEnabled ? "Yes" : "No"),
        ];

        if (this.memoryCachingEnabled) {
            detailListModel.push(
                DetailsListGroupItemModel.create("cacheExpiryInSeconds", this.cacheExpiryInSeconds.toString()),
            );
        }

        details = details.concat(
            DetailListItemHelper.createDetailItemGroup(
                DetailsListItemCardType.Details, detailListModel, icons.grid));

        return details;
    }

    public setGroupByValue(
        dataTableDefinitionList: Array<DataTableDefinitionViewModel>,
        groupBy: string,
    ): Array<DataTableDefinitionViewModel> {
        if (groupBy === SortAndFilterBy.ProductName) {
            dataTableDefinitionList.forEach(
                (item: DataTableDefinitionViewModel) => item.groupByValue = item.name,
            );
        } else if (groupBy === SortAndFilterBy.LastModifiedDate) {
            dataTableDefinitionList.forEach(
                (item: DataTableDefinitionViewModel) => item.groupByValue = item.lastModifiedDateTime,
            );
        } else {
            dataTableDefinitionList.forEach(
                (item: DataTableDefinitionViewModel) => item.groupByValue = item.createdDateTime,
            );
        }

        return dataTableDefinitionList;
    }

    public setSortOptions(
        dataTableDefinitionList: Array<DataTableDefinitionViewModel>,
        sortBy: string,
        sortDirection: SortDirection,
    ): Array<DataTableDefinitionViewModel> {
        sortDirection = sortDirection == null ? SortDirection.Descending : sortDirection;
        if (sortBy === SortAndFilterBy.DataTableName) {
            dataTableDefinitionList.forEach((item: DataTableDefinitionViewModel) => {
                item.sortByValue = item.name;
                item.sortDirection = sortDirection;
            });
        } else if (sortBy === SortAndFilterBy.LastModifiedDate) {
            dataTableDefinitionList.forEach((item: DataTableDefinitionViewModel) => {
                item.sortByValue = item.lastModifiedDateTime;
                item.sortDirection = sortDirection;
            });
        } else {
            dataTableDefinitionList.forEach((item: DataTableDefinitionViewModel) => {
                item.sortByValue = item.createdDateTime;
                item.sortDirection = sortDirection;
            });
        }
        return dataTableDefinitionList;
    }
}
