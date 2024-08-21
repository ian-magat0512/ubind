import { ReportResourceModel } from '@app/resource-models/report.resource-model';
import { EntityViewModel } from "./entity.viewmodel";
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import { DetailListItemHelper } from '@app/helpers/detail-list-item.helper';
import { DetailsListItemCardType } from '@app/models/details-list/details-list-item-card-type.enum';
import { ProductResourceModel } from '@app/resource-models/product.resource-model';
import { DetailsListGroupItemModel } from '@app/models/details-list/details-list-item-model';
import { DetailsListFormItem } from '@app/models/details-list/details-list-form-item';
import { LocalDateHelper } from '@app/helpers/local-date.helper';
import { DetailsListItemCard } from '@app/models/details-list/details-list-item-card';
import { IconLibrary } from '@app/models/icon-library.enum';
import { DetailsListFormTextItem } from '@app/models/details-list/details-list-form-text-item';
import { DetailsListFormCheckboxGroupItem } from '@app/models/details-list/details-list-form-checkbox-group-item';
import { DetailsListFormTextAreaItem } from '@app/models/details-list/details-list-form-text-area-item';

/**
 * Export report detail view model class.
 * TODO: Write a better class header: view model of report detail.
 */
export class ReportDetailViewModel implements EntityViewModel {
    public constructor(report: ReportResourceModel) {
        this.id = report.id;
        this.name = report.name;
        this.description = report.description;
        this.isDeleted = report.isDeleted;
        this.deleteFromList = this.isDeleted;
        this.products = report.products;
        this.tenantId = report.tenantId;
        this.sourceData = report.sourceData;
        this.mimeType = report.mimeType;
        this.filename = report.filename;
        this.body = report.body;
        this.createdDateTime = report.createdDateTime;
        this.createdDate = LocalDateHelper.toLocalDate(report.createdDateTime);
        this.createdTime = LocalDateHelper.convertToLocalAndGetTimeOnly(report.createdDateTime);
        this.lastModifiedDate = LocalDateHelper.toLocalDate(report.lastModifiedDateTime);
        this.lastModifiedTime = LocalDateHelper.convertToLocalAndGetTimeOnly(report.lastModifiedDateTime);
    }

    public id: string;
    public name: string;
    public description: string;
    public isDeleted: boolean;
    public deleteFromList: boolean;
    public products: Array<ProductResourceModel>;
    public tenantId: string;
    public sourceData: string;
    public mimeType: string;
    public filename: string;
    public body: string;
    public createdDateTime: string;
    public createdDate: string;
    public createdTime: string;
    public lastModifiedDate: string;
    public lastModifiedTime: string;

    public static createDetailsListForEdit(): Array<DetailsListFormItem> {
        const details: Array<DetailsListFormItem> = [];
        const icons: typeof DetailListItemHelper.detailListItemIconMap =
            DetailListItemHelper.detailListItemIconMap;
        const detailsCard: DetailsListItemCard = new DetailsListItemCard(
            DetailsListItemCardType.Details,
            'Details');
        details.push(DetailsListFormTextItem.create(
            detailsCard,
            "name",
            "Name")
            .withIcon(icons.today, IconLibrary.IonicV4));
        details.push(DetailsListFormTextItem.create(
            detailsCard,
            "description",
            "Description"));

        const dataSourcesCard: DetailsListItemCard = new DetailsListItemCard(
            DetailsListItemCardType.DataSources,
            'Data Sources');
        details.push(DetailsListFormCheckboxGroupItem.create(
            dataSourcesCard,
            "products",
            "Products")
            .withIcon(icons.list, IconLibrary.IonicV4));
        details.push(DetailsListFormCheckboxGroupItem.create(
            dataSourcesCard,
            "sourceData",
            "Source Data"));

        const templateCard: DetailsListItemCard = new DetailsListItemCard(
            DetailsListItemCardType.Template,
            'Template');
        details.push(DetailsListFormTextItem.create(
            templateCard,
            "mimeType",
            "Mime Type")
            .withIcon(icons.code, IconLibrary.IonicV4));
        details.push(DetailsListFormTextItem.create(
            templateCard,
            "filename",
            "Filename"));
        details.push(DetailsListFormTextAreaItem.create(
            templateCard,
            "textBody",
            "Text Body"));
        return details;
    }

    public createDetailsList(): Array<DetailsListItem> {
        let details: Array<DetailsListItem> = [];
        let icons: typeof DetailListItemHelper.detailListItemIconMap =
            DetailListItemHelper.detailListItemIconMap;
        let sourceDataArray: Array<string> = this.sourceData ?
            this.sourceData.split(',') : [];

        let detailModel: Array<DetailsListGroupItemModel> = [
            DetailsListGroupItemModel.create(
                "name",
                this.name),
            DetailsListGroupItemModel.create(
                "description",
                this.description,
            ),
        ];

        details = details.concat(DetailListItemHelper.createDetailItemGroup(
            DetailsListItemCardType.Details,
            detailModel,
            icons.today,
        ));

        let dataSources: Array<DetailsListGroupItemModel> = [
            DetailsListGroupItemModel.create(
                "products",
                this.products.map((p: ProductResourceModel) => p.name).join(', ')),
            DetailsListGroupItemModel.create(
                "sourceData",
                sourceDataArray.join(', '),
            ),
        ];

        details = details.concat(DetailListItemHelper.createDetailItemGroup(
            DetailsListItemCardType.DataSources,
            dataSources,
            icons.list,
        ));

        let template: Array<DetailsListGroupItemModel> = [
            DetailsListGroupItemModel.create("mimeType", this.mimeType),
            DetailsListGroupItemModel.create("fileName", this.filename, null, null, null, 0, null, 0),
            DetailsListGroupItemModel.create("textBody", this.body, null, null, null, 0, null, 0),
        ];

        let dates: Array<DetailsListGroupItemModel> = [
            DetailsListGroupItemModel.create("createdDate", this.createdDate),
            DetailsListGroupItemModel.create("createdTime", this.createdTime),
            DetailsListGroupItemModel.create("lastModifiedDate", this.lastModifiedDate),
            DetailsListGroupItemModel.create("lastModifiedTime", this.lastModifiedTime),
        ];

        details = details.concat(DetailListItemHelper.createDetailItemGroup(
            DetailsListItemCardType.Template,
            template,
            icons.code,
        ));

        details = details.concat(DetailListItemHelper.createDetailItemGroup(
            DetailsListItemCardType.Dates,
            dates,
            icons.calendar,
        ));

        return details;
    }
}
