import { OrganisationResourceModel } from "@app/resource-models/organisation/organisation.resource-model";
import { SegmentableEntityViewModel } from "./segmentable-entity.viewmodel";
import { LocalDateHelper, Permission } from '@app/helpers';
import { DetailsListItem } from "@app/models/details-list/details-list-item";
import { DetailsListItemCardType } from "@app/models/details-list/details-list-item-card-type.enum";
import { DetailListItemHelper } from "@app/helpers/detail-list-item.helper";
import { DetailsListGroupItemModel } from "@app/models/details-list/details-list-item-model";
import { DetailsListFormItem } from '@app/models/details-list/details-list-form-item';
import { SortDirection, SortedEntityViewModel } from './sorted-entity.viewmodel';
import { FormValidatorHelper } from "@app/helpers/form-validator.helper";
import { AdditionalPropertiesHelper } from "@app/helpers/additional-properties.helper";
import { AdditionalPropertyValue } from "@app/models/additional-property-item-view.model";
import { SortAndFilterBy } from "@app/models/sort-filter-by.enum";
import { DetailsListFormTextItem } from "@app/models/details-list/details-list-form-text-item";
import { DetailsListItemCard } from "@app/models/details-list/details-list-item-card";
import { IconLibrary } from "@app/models/icon-library.enum";
import { DetailsListItemActionIcon } from "@app/models/details-list/details-list-item-action-icon";
import { NavProxyService } from "@app/services/nav-proxy.service";
import { RelatedEntityType } from "@app/models/related-entity-type.enum";
import { SharedPopoverService } from "@app/services/shared-popover.service";
import {
    PopoverManagingOrganisationComponent,
} from "@app/pages/organisation/popover-managing-organisation/popover-managing-organisation.component";
import { DetailsListItemGroupType } from "@app/models/details-list/details-list-item-type.enum";
import {
    OrganisationLinkedIdentity,
} from "@app/resource-models/organisation/organisation-linked-identity.resource-model";
import { DetailsListFormItemArray } from "@app/models/details-list/details-list-form-item-array";
import { DetailsListFormItemGroup } from "@app/models/details-list/details-list-form-item-group";
import { GroupedEntityViewModel } from './grouped-entity.viewmodel';

/**
 * View model used for preparing for rendering organisation details.
 */
export class OrganisationViewModel
implements SegmentableEntityViewModel, SortedEntityViewModel, GroupedEntityViewModel {

    public deleteFromList: boolean;
    public status: string;
    public id: string;
    public segment: string;
    public alias: string;
    public name: string;
    public managingOrganisationId: string;
    public managingOrganisationName: string;
    public isDefault: boolean;
    public isActive: boolean;
    public isDeleted: boolean;
    public createdDate: string;
    public createdTime: string;
    public lastModifiedDateTime: string;
    public lastModifiedDate: string;
    public lastModifiedTime: string;
    public organisation: OrganisationResourceModel;
    public createdDateTimestamp: string;
    public sortByValue: string;
    public sortDirection: SortDirection;
    public additionalPropertyValues: Array<AdditionalPropertyValue> = [];
    public linkedIdentities: Array<OrganisationLinkedIdentity> = [];
    public groupByValue: string;

    public constructor(organisation: OrganisationResourceModel) {
        this.id = organisation.id;
        this.isDefault = organisation.isDefault;
        this.isActive = organisation.isActive;
        this.segment = organisation.status;
        this.name = organisation.name;
        this.alias = organisation.alias;
        this.managingOrganisationId = organisation.managingOrganisationId;
        this.managingOrganisationName = organisation.managingOrganisationName;
        this.status = organisation.status;
        this.organisation = organisation;
        this.createdDate = LocalDateHelper.toLocalDate(organisation.createdDateTime);
        this.createdDateTimestamp = organisation.createdDateTime;
        this.createdTime = LocalDateHelper.convertToLocalAndGetTimeOnly(organisation.createdDateTime);
        this.lastModifiedDateTime = organisation.lastModifiedDateTime;
        this.lastModifiedDate = LocalDateHelper.toLocalDate(organisation.lastModifiedDateTime);
        this.lastModifiedTime = LocalDateHelper.convertToLocalAndGetTimeOnly(organisation.lastModifiedDateTime);
        this.sortByValue = organisation.name;
        this.sortDirection = SortDirection.Ascending;
        this.additionalPropertyValues = organisation.additionalPropertyValues;
        this.linkedIdentities = organisation.linkedIdentities;
        this.groupByValue = this.createdDate;
    }

    public static createDetailsListForEdit(
        additionalPropertyValueFields: Array<AdditionalPropertyValue>,
        canEditAdditionalPropertyValues: boolean,
        linkedIdentities?: Array<OrganisationLinkedIdentity>,
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
            .withIcon(icons.business, IconLibrary.IonicV4));
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

        if (linkedIdentities && linkedIdentities.length > 0) {
            const linkedIdentitiesCard: DetailsListItemCard =
            new DetailsListItemCard(
                DetailsListItemCardType.LinkedIdentities,
                "Linked Identities");
            let fieldArray: DetailsListFormItemArray = DetailsListFormItemArray.create(
                linkedIdentitiesCard,
                "linkedIdentities",
                "Linked Identities")
                .withHeader("Linked Identities")
                .withGroupName("linkedIdentities")
                .withParagraph("You may manually enter the unique identifiers of this "
                    + "organisation within external identity providers. This is usually only necessary where you "
                    + "don't rely on auto provisioning of organisations during sign in, or you need to update the "
                    + "unique identifier.")
                .withIcon<DetailsListFormItemArray>('card-account-details', IconLibrary.AngularMaterial);
            for (let linkedIdentity of linkedIdentities) {
                let fieldGroup: DetailsListFormItemGroup = DetailsListFormItemGroup.create(
                    linkedIdentitiesCard,
                    "linkedIdentityEntry",
                    "Linked Identity Entry")
                    .withGroupName<DetailsListFormItemGroup>("linkedIdentities")
                    .withItem(DetailsListFormTextItem.create(
                        linkedIdentitiesCard,
                        "uniqueId",
                        linkedIdentity.authenticationMethodName)
                        .withGroupName("linkedIdentities"));
                fieldArray = fieldArray.withItem(fieldGroup);
            }
            details.push(fieldArray);
        }

        return details;
    }

    public createDetailsList(
        navProxy: NavProxyService,
        popoverService: SharedPopoverService,
        canViewAdditionalPropertyValues: boolean,
    ): Array<DetailsListItem> {
        let items: Array<DetailsListItem> = [];
        const icons: any = DetailListItemHelper.detailListItemIconMap;

        const detailsItems: Array<DetailsListGroupItemModel> = [
            DetailsListGroupItemModel.create('name', this.name),
            DetailsListGroupItemModel.create('alias', this.alias),
            DetailsListGroupItemModel.create('status', this.status),
        ];

        if (this.isDefault) {
            detailsItems.push(DetailsListGroupItemModel.create("default", "Yes"));
        }

        items = items.concat(
            DetailListItemHelper.createDetailItemGroup(
                DetailsListItemCardType.Details, detailsItems, icons.business));

        let relationshipsCard: DetailsListItemCard =
            new DetailsListItemCard(
                DetailsListItemCardType.Relationships,
                "Relationships");

        let managingOrganisationActions: Array<DetailsListItemActionIcon> = [];
        let organisationLinkAction: DetailsListItemActionIcon;
        if (this.managingOrganisationId) {
            organisationLinkAction = DetailListItemHelper.createAction(
                () => navProxy.goToOrganisation(this.managingOrganisationId));
            managingOrganisationActions.push(organisationLinkAction);
        }

        let managingOrganisationMenuAction: DetailsListItemActionIcon =
            DetailListItemHelper.createAction(
                () =>  popoverService.show(
                    {
                        component: PopoverManagingOrganisationComponent,
                        componentProps: {
                            organisationId: this.id,
                            organisationName: this.name,
                            managingOrganisationId: this.managingOrganisationId,
                            managingOrganisationName: this.managingOrganisationName,
                        },
                        showBackdrop: false,
                        mode: 'md',
                        event: event,
                    },
                    'Mananging Organisation Popover',
                ),
                "more",
                IconLibrary.IonicV4,
                null,
                [Permission.ManageOrganisations, Permission.ManageAllOrganisations],
            );
        managingOrganisationActions.push(managingOrganisationMenuAction);

        items.push(DetailsListItem.createItem(
            relationshipsCard,
            DetailsListItemGroupType.Relationships,
            this.managingOrganisationName || 'None',
            "Managing Organisation",
            icons.link)
            .roundIcon()
            .withActions(...managingOrganisationActions)
            .withRelatedEntity(RelatedEntityType.Organisation, this.managingOrganisationId, null, null));

        const dates: Array<DetailsListGroupItemModel> = [
            DetailsListGroupItemModel.create('createdDate', this.createdDate),
            DetailsListGroupItemModel.create('createdTime', this.createdTime),
            DetailsListGroupItemModel.create('lastModifiedDate', this.lastModifiedDate),
            DetailsListGroupItemModel.create('lastModifiedTime', this.lastModifiedTime),
        ];

        items = items.concat(
            DetailListItemHelper.createDetailItemGroup(DetailsListItemCardType.Dates, dates));
        if (canViewAdditionalPropertyValues) {
            AdditionalPropertiesHelper.createDetailsList(items, this.additionalPropertyValues);
        }

        if (this.linkedIdentities && this.linkedIdentities.length > 0) {
            const linkedIdentitiesCard: DetailsListItemCard =
                new DetailsListItemCard(
                    DetailsListItemCardType.LinkedIdentities,
                    "Linked Identities");
            for (let linkedIdentity of this.linkedIdentities) {
                items.push(DetailsListItem.createItem(
                    linkedIdentitiesCard,
                    DetailsListItemGroupType.LinkedIdentities,
                    linkedIdentity.uniqueId,
                    linkedIdentity.authenticationMethodName,
                    "card-account-details",
                    IconLibrary.AngularMaterial,
                ));
            }
        }
        return items;
    }

    public setSortOptions(
        orgList: Array<OrganisationViewModel>,
        sortBy: string,
        sortDirection: SortDirection,
    ): Array<OrganisationViewModel> {
        sortDirection = sortDirection == null ? SortDirection.Ascending : sortDirection;

        if (sortBy === SortAndFilterBy.CreatedDate) {
            orgList.forEach((item: OrganisationViewModel) => {
                item.sortByValue = item.createdDateTimestamp;
                item.sortDirection = sortDirection;
            });
        } else if (sortBy === SortAndFilterBy.LastModifiedDate) {
            orgList.forEach((item: OrganisationViewModel) => {
                item.sortByValue = item.lastModifiedDateTime;
                item.sortDirection = sortDirection;
            });
        } else {
            orgList.forEach((item: OrganisationViewModel) => {
                item.sortByValue = item.name;
                item.sortDirection = sortDirection;
            });
        }
        return orgList;
    }

    public setGroupByValue(
        organisationList: Array<OrganisationViewModel>,
        groupBy: string,
    ): Array<OrganisationViewModel> {
        return organisationList;
    }
}
