import { LocalDateHelper } from '@app/helpers';
import { SegmentableEntityViewModel } from './segmentable-entity.viewmodel';
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import { DetailListItemHelper } from '@app/helpers/detail-list-item.helper';
import { DetailsListItemCardType } from '@app/models/details-list/details-list-item-card-type.enum';
import { ReleaseType } from '@app/models';
import { ReleaseResourceModel } from '@app/resource-models/release.resource-model';
import { DeploymentEnvironment } from '@app/models/deployment-environment.enum';
import { DetailsListFormItem } from '@app/models/details-list/details-list-form-item';
import { DetailsListGroupItemModel } from '@app/models/details-list/details-list-item-model';
import { DetailsListItemCard } from '@app/models/details-list/details-list-item-card';
import { DetailsListItemActionIcon } from '@app/models/details-list/details-list-item-action-icon';
import { DetailsListFormSelectItem } from '@app/models/details-list/details-list-form-select-item';
import { DetailsListFormTextAreaItem } from '@app/models/details-list/details-list-form-text-area-item';
import { IconLibrary } from '@app/models/icon-library.enum';
import { FormValidatorHelper } from '@app/helpers/form-validator.helper';

/**
 * Export release detail view model class.
 * TODO: Write a better class header: view model of release detail.
 */
export class ReleaseDetailViewModel implements SegmentableEntityViewModel {
    public constructor(release: ReleaseResourceModel) {
        this.id = release.id;
        this.label = release.description;
        this.number = release.number;
        this.productAlias = release.productAlias;
        this.productId = release.productId;
        this.productName = release.productName;
        this.tenantId = release.tenantId;
        this.type = release.type;
        this.createdDate = LocalDateHelper.toLocalDate(
            release.createdDateTime,
        );
        this.createdTime = LocalDateHelper.convertToLocalAndGetTimeOnly(
            release.createdDateTime,
        );
        this.resourceModel = release;
    }

    public id: string;
    public segment: string;
    public name: string;
    public alias: string;
    public disabled: boolean;
    public createdDate: string;
    public createdTime: string;
    public tenantId: string;
    public productName: string;
    public productAlias: string;
    public productId: string;
    public type: ReleaseType;
    public label: string;
    public number: number;
    public resourceModel: ReleaseResourceModel;
    public deleteFromList: boolean;

    public static createDetailsListForEdit(isCreate: boolean): Array<DetailsListFormItem> {
        const details: Array<DetailsListFormItem> = [];
        const icons: typeof DetailListItemHelper.detailListItemIconMap =
            DetailListItemHelper.detailListItemIconMap;
        const detailsCard: DetailsListItemCard = new DetailsListItemCard(
            DetailsListItemCardType.Details,
            'Details');

        let descriptionField: DetailsListFormTextAreaItem = DetailsListFormTextAreaItem
            .create(detailsCard, "description", "Description")
            .withValidator<DetailsListFormTextAreaItem>(FormValidatorHelper.required());
        if (isCreate) {
            details.push(DetailsListFormSelectItem.create(
                detailsCard,
                "releaseType",
                "Release Type")
                .withIcon(icons.cube, IconLibrary.IonicV4));
        } else {
            descriptionField.withIcon(icons.cube, IconLibrary.IonicV4);
        }

        details.push(descriptionField);
        return details;
    }

    public createDetailsList(source: any, deployedTo: string): Array<DetailsListItem> {
        let deployments: Array<string> = deployedTo.split(', ');
        let details: Array<DetailsListItem> = [];
        let icons: any = DetailListItemHelper.detailListItemIconMap;
        let productAction: DetailsListItemActionIcon =
            DetailListItemHelper.createAction(() => source.gotoProduct());

        let releaseDetailsCard: DetailsListItemCard =
            new DetailsListItemCard(DetailsListItemCardType.Details, DetailsListItemCardType.Details);
        details.push(DetailsListItem.createItem(
            releaseDetailsCard,
            "releaseTitle",
            "Release " + this.number.toString(),
            this.label,
            'none',
        ));

        let detailModel: Array<DetailsListGroupItemModel> = [
            DetailsListGroupItemModel.create(
                "releaseType",
                this.type.toString(),
            ),
            DetailsListGroupItemModel.create(
                "releaseNumber",
                this.number.toString(),
            ),
            DetailsListGroupItemModel.create(
                "defaultForProduction",
                deployments.indexOf(
                    DeploymentEnvironment.Production.toString(),
                ) > -1 ? "Yes" : "No",
            ),
            DetailsListGroupItemModel.create(
                "defaultForStaging",
                deployments.indexOf(
                    DeploymentEnvironment.Staging.toString(),
                ) > -1 ? "Yes" : "No",
            ),
        ];

        details = details.concat(DetailListItemHelper.createDetailItemGroup(
            DetailsListItemCardType.Details,
            detailModel,
            icons.cube,
        ));

        let relationships: Array<DetailsListGroupItemModel> = [
            DetailsListGroupItemModel.create(
                "product", this.productName, null, null, productAction),
        ];

        details = details.concat(DetailListItemHelper.createDetailItemGroup(
            DetailsListItemCardType.Relationships,
            relationships,
        ));

        let dates: Array<DetailsListGroupItemModel> = [
            DetailsListGroupItemModel.create("createdDate", this.createdDate),
            DetailsListGroupItemModel.create("createdTime", this.createdTime),
        ];

        details = details.concat(DetailListItemHelper.createDetailItemGroup(
            DetailsListItemCardType.Dates,
            dates,
        ));

        let status: Array<DetailsListGroupItemModel> = [
            DetailsListGroupItemModel.create("id", this.id),
        ];

        details = details.concat(DetailListItemHelper.createDetailItemGroup(
            DetailsListItemCardType.Status,
            status,
            icons.folder,
        ));

        const usage: Array<DetailsListGroupItemModel> = [
            DetailsListGroupItemModel.create("stagingQuotes", " "),
            DetailsListGroupItemModel.create("stagingPolicyTransactions", " "),
            DetailsListGroupItemModel.create("productionQuotes", " "),
            DetailsListGroupItemModel.create("productionPolicyTransactions", " "),
        ];
        details = details.concat(DetailListItemHelper.createDetailItemGroup(
            DetailsListItemCardType.Usage,
            usage,
            'albums-outline',
            IconLibrary.IonicV5,
        ));

        return details;
    }
}
