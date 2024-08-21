import { RoleResourceModel, RolePermissionResourceModel } from '@app/resource-models/role.resource-model';
import { EntityViewModel } from '@app/viewmodels/entity.viewmodel';
import { SortedEntityViewModel, SortDirection } from './sorted-entity.viewmodel';
import { RoleType } from '@app/models/role-type.enum';
import { LocalDateHelper } from '@app/helpers';
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import { DetailListItemHelper } from '@app/helpers/detail-list-item.helper';
import { DetailsListItemCardType } from '@app/models/details-list/details-list-item-card-type.enum';
import { DetailsListFormItem } from '../models/details-list/details-list-form-item';
import { DetailsListGroupItemModel } from '@app/models/details-list/details-list-item-model';
import { GroupedEntityViewModel } from './grouped-entity.viewmodel';
import { FormValidatorHelper } from '@app/helpers/form-validator.helper';
import { SortAndFilterBy } from '@app/models/sort-filter-by.enum';
import { DetailsListItemCard } from '@app/models/details-list/details-list-item-card';
import { IconLibrary } from '@app/models/icon-library.enum';
import { DetailsListFormTextItem } from '@app/models/details-list/details-list-form-text-item';
import { DetailsListFormSelectItem } from '@app/models/details-list/details-list-form-select-item';
import { ValidationMessages } from '@app/models/validation-messages';

/**
 * Export role permission view model class.
 * TODO: Write a better class header: view model of role permission functions.
 */
export class RolePermissionViewModel {

    public constructor(role: RolePermissionResourceModel) {
        this.concern = role.concern;
        this.type = role.type;
        this.description = role.description;
    }

    public type: string;
    public description: string;
    public concern: string;

    public createDetailsList(): Array<DetailsListItem> {
        let details: Array<DetailsListItem> = [];
        let icons: any = DetailListItemHelper.detailListItemIconMap;

        let detailModel: Array<DetailsListGroupItemModel> = [
            DetailsListGroupItemModel.create(
                "permissionType",
                this.description,
            ),
        ];

        details = details.concat(DetailListItemHelper.createDetailItemGroup(
            DetailsListItemCardType.Details,
            detailModel,
            icons.checkmark,
        ));

        return details;
    }
}

/**
 * Role View Model Class.
 * TODO: Write a better class header: view model of the role function.
 */
export class RoleViewModel implements EntityViewModel, GroupedEntityViewModel, SortedEntityViewModel {

    public constructor(role: RoleResourceModel) {
        this.id = role.id;
        this.name = role.name;
        this.description = role.description;
        this.type = role.type;
        this.isFixed = role.isFixed;
        this.isDeletable = role.isDeletable;
        this.isRenamable = role.isRenamable;
        this.isPermanentRole = role.isPermanentRole;
        this.arePermissionsEditable = role.arePermissionsEditable;
        if (role.permissions) {
            this.permissions = role.permissions.map((x: RolePermissionResourceModel) => new RolePermissionViewModel(x));
        }
        this.sortByValue = role.createdDateTime;
        this.sortDirection = SortDirection.Descending;
        this.deleteFromList = role.isDeleted;
        this.createdTime = LocalDateHelper.convertToLocalAndGetTimeOnly(role.createdDateTime);
        this.createdDate = LocalDateHelper.toLocalDate(role.createdDateTime);
        this.createdDateTime = role.createdDateTime;
        this.groupByValue = this.createdDate;
        this.lastModifiedDateTime = role.lastModifiedDateTime;
        this.lastModifiedDate = LocalDateHelper.toLocalDate(role.lastModifiedDateTime);
        this.lastModifiedTime = LocalDateHelper.convertToLocalAndGetTimeOnly(role.lastModifiedDateTime);
        this.organisationId = role.organisationId;
    }

    public id: string;
    public name: string;
    public description: string;
    public type: RoleType;
    public isFixed: boolean;
    public isDeletable: boolean;
    public isRenamable: boolean;
    public arePermissionsEditable: boolean;
    public isDefaultRole: boolean;
    public permissions: Array<RolePermissionViewModel>;
    public sortByValue: string;
    public sortDirection: SortDirection;
    public deleteFromList: boolean = false;
    public createdDate: string;
    public createdDateTime: string;
    public createdTime: string;
    public isPermanentRole: boolean;
    public groupByValue: string;
    public lastModifiedDateTime: string;
    public lastModifiedDate: string;
    public lastModifiedTime: string;
    public organisationId: string;

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
            "Role Name")
            .withValidator(FormValidatorHelper.alphaNumericValidator(
                true,
                ValidationMessages.errorKey.Name))
            .withIcon(icons.shirt, IconLibrary.IonicV4));
        details.push(DetailsListFormTextItem.create(
            detailsCard,
            "description",
            "Description")
            .withValidator(FormValidatorHelper.alphaNumericValidator(
                true,
                ValidationMessages.errorKey.Name)));
        return details;
    }

    public static createDetailsListForPermissionEdit(): Array<DetailsListFormItem> {
        const details: Array<DetailsListFormItem> = [];
        const icons: typeof DetailListItemHelper.detailListItemIconMap =
            DetailListItemHelper.detailListItemIconMap;
        const detailsCard: DetailsListItemCard = new DetailsListItemCard(
            DetailsListItemCardType.Details,
            'Details');
        details.push(DetailsListFormSelectItem.create(
            detailsCard,
            "type",
            "Type")
            .withValidator(FormValidatorHelper.alphaNumericValidator(
                true,
                ValidationMessages.errorKey.Name))
            .withIcon(icons.checkmark, IconLibrary.IonicV4));
        return details;
    }

    public createDetailsList(): Array<DetailsListItem> {
        let details: Array<DetailsListItem> = [];
        let icons: typeof DetailListItemHelper.detailListItemIconMap =
            DetailListItemHelper.detailListItemIconMap;

        let detailModel: Array<DetailsListGroupItemModel> = [
            DetailsListGroupItemModel.create(
                "name",
                this.name),
            DetailsListGroupItemModel.create(
                "description",
                this.description,
            ),
        ];

        let dates: Array<DetailsListGroupItemModel> = [
            DetailsListGroupItemModel.create("createdDate", this.createdDate),
            DetailsListGroupItemModel.create("createdTime", this.createdTime),
            DetailsListGroupItemModel.create("lastModifiedDate", this.lastModifiedDate),
            DetailsListGroupItemModel.create("lastModifiedTime", this.lastModifiedTime),
        ];

        details = details.concat(DetailListItemHelper.createDetailItemGroup(
            DetailsListItemCardType.Details,
            detailModel,
            icons.shirt,
        ));

        details = details.concat(DetailListItemHelper.createDetailItemGroup(
            DetailsListItemCardType.Dates,
            dates,
            icons.calendar,
        ));

        return details;
    }

    public setGroupByValue(
        roleList: Array<RoleViewModel>,
        groupBy: string,
    ): Array<RoleViewModel> {
        if (groupBy === SortAndFilterBy.RoleName) {
            roleList.forEach((item: RoleViewModel) => {
                item.groupByValue = item.name;
            });
        } else if (groupBy === SortAndFilterBy.LastModifiedDate) {
            roleList.forEach((item: RoleViewModel) => {
                item.groupByValue = item.lastModifiedDate;
            });
        } else {
            roleList.forEach((item: RoleViewModel) => {
                item.groupByValue = item.createdDate;
            });
        }

        return roleList;
    }

    public setSortOptions(
        roleList: Array<RoleViewModel>,
        sortBy: string,
        sortDirection: SortDirection,
    ): Array<RoleViewModel> {
        sortDirection = sortDirection == null ? SortDirection.Descending : sortDirection;

        if (sortBy === SortAndFilterBy.RoleName) {
            roleList.forEach((item: RoleViewModel) => {
                item.sortByValue = item.name;
                item.sortDirection = sortDirection;
            });
        } else if (sortBy === SortAndFilterBy.LastModifiedDate) {
            roleList.forEach((item: RoleViewModel) => {
                item.sortByValue = item.lastModifiedDateTime;
                item.sortDirection = sortDirection;
            });
        } else {
            roleList.forEach((item: RoleViewModel) => {
                item.sortByValue = item.createdDateTime;
                item.sortDirection = sortDirection;
            });
        }
        return roleList;
    }
}
