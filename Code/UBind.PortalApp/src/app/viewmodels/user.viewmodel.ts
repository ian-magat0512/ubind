import { UserStatus } from '@app/models';
import { UserResourceModel } from '@app/resource-models/user/user.resource-model';
import { LocalDateHelper, Permission } from '@app/helpers';
import { SegmentableEntityViewModel } from './segmentable-entity.viewmodel';
import { DetailsListItem } from '../models/details-list/details-list-item';
import {
    DetailListItemDescriptionMap,
    PersonDetailsHelper,
} from '../helpers/person-details.helper';
import { DetailsListItemGroupType } from '../models/details-list/details-list-item-type.enum';
import { DetailsListItemCard } from '../models/details-list/details-list-item-card';
import { DetailsListItemCardType } from '../models/details-list/details-list-item-card-type.enum';
import {
    RepeatingFieldResourceModel,
    RepeatingAddressFieldResourceModel,
} from '../resource-models/repeating-field.resource-model';
import { GroupedEntityViewModel } from './grouped-entity.viewmodel';
import { SortDirection, SortedEntityViewModel } from './sorted-entity.viewmodel';
import { PersonRepeatingFieldsResourceModel } from '../resource-models/person/person-repeating-fields.resource-model';
import { DetailsListItemActionIcon } from '@app/models/details-list/details-list-item-action-icon';
import { DetailListItemHelper } from '@app/helpers/detail-list-item.helper';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { SharedPopoverService } from '@app/services/shared-popover.service';
import { AdditionalPropertyValue } from '@app/models/additional-property-item-view.model';
import { AdditionalPropertiesHelper } from '@app/helpers/additional-properties.helper';
import { RelatedEntityType } from '@app/models/related-entity-type.enum';
import { DateHelper } from '@app/helpers/date.helper';
import { SortAndFilterBy } from '@app/models/sort-filter-by.enum';
import { UserLinkedIdentity } from '@app/resource-models/user/user-linked-identity.resource-model';
import { PopoverAssignPortalComponent } from '../components/popover-assign-portal/popover-assign-portal.component';
import { IconLibrary } from '@app/models/icon-library.enum';
import { UserService } from '@app/services/user.service';
import { PermissionService } from '@app/services/permission.service';
import { AssignPortalEntityType } from "@app/models/assign-portal-entity-type.enum";

/**
 * Model used for preparing data for rendering the view of a user.
 */
export class UserViewModel implements SegmentableEntityViewModel, GroupedEntityViewModel, SortedEntityViewModel {

    public id: string;
    public personId: string;
    public organisationId: string;
    public organisationName: string;
    public tenantId: string;
    public preferredName: string;
    public fullName: string;
    public displayName: string;
    public email: string;
    public alternativeEmail: string;
    public mobilePhone: string;
    public homePhone: string;
    public workPhone: string;
    public profilePictureId: string;
    public userType: string;
    public createdDate: string;
    public createdDateTime: string;
    public createdTime: string;
    public lastModifiedDateTime: string;
    public lastModifiedDate: string;
    public lastModifiedTime: string;
    public blocked: boolean;
    public status: string;
    public segment: string;
    public deleteFromList: boolean = false;
    public namePrefix: string;
    public firstName: string;
    public middleNames: string;
    public lastName: string;
    public nameSuffix: string;
    public company: string;
    public title: string;
    public groupByValue: string;
    public sortByValue: string;
    public sortDirection: SortDirection;
    public repeatingFields: Array<RepeatingFieldResourceModel> = [];
    public repeatingAddressFields: Array<RepeatingAddressFieldResourceModel> = [];
    public personRepeatingFields: PersonRepeatingFieldsResourceModel;
    public additionalPropertyValues: Array<AdditionalPropertyValue> = [];
    public portalId: string;
    public portalName: string;
    public linkedIdentities: Array<UserLinkedIdentity>;

    public constructor(user: UserResourceModel) {
        this.id = user.id;
        this.personId = user.personId;
        this.organisationId = user.organisationId;
        this.organisationName = user.organisationName;
        this.preferredName = user.preferredName;
        this.fullName = user.fullName;
        this.displayName = user.displayName;
        this.namePrefix = user.namePrefix;
        this.firstName = user.firstName;
        this.middleNames = user.middleNames;
        this.lastName = user.lastName;
        this.nameSuffix = user.nameSuffix;
        this.company = user.company;
        this.title = user.title;
        this.email = user.email;
        this.userType = user.userType;
        this.createdDate = DateHelper.formatDDMMMYYYY(user.createdDateTime);
        this.createdTime = LocalDateHelper.convertToLocalAndGetTimeOnly(user.createdDateTime);
        this.createdDateTime = user.createdDateTime;
        this.lastModifiedDateTime = user.lastModifiedDateTime;
        this.lastModifiedDate = DateHelper.formatDDMMMYYYY(user.lastModifiedDateTime);
        this.lastModifiedTime = LocalDateHelper.convertToLocalAndGetTimeOnly(user.lastModifiedDateTime);
        this.blocked = user.blocked;
        this.status = user.userStatus == UserStatus.Deactivated
            ? UserStatus.Disabled
            : user.userStatus
                ? user.userStatus
                : user.status;
        this.segment = this.status;
        this.personRepeatingFields = {
            emailAddresses: user.emailAddresses,
            phoneNumbers: user.phoneNumbers,
            streetAddresses: user.streetAddresses,
            websiteAddresses: user.websiteAddresses,
            messengerIds: user.messengerIds,
            socialMediaIds: user.socialMediaIds,
        };

        this.groupByValue = this.createdDate;
        this.sortByValue = this.createdDateTime;
        this.sortDirection = SortDirection.Descending;
        this.profilePictureId = user.profilePictureId;
        this.tenantId = user.tenantId;
        this.additionalPropertyValues = user.additionalPropertyValues;
        this.portalId = user.portalId;
        this.portalName = user.portalName;
    }

    public createDetailsList(
        userService: UserService,
        navProxy: NavProxyService,
        popoverService: SharedPopoverService,
        permissionService: PermissionService,
        isCustomer: boolean,
        canViewAdditionalPropertyValues: boolean,
        isBasic: boolean = false,
    ): Array<DetailsListItem> {
        let details: Array<DetailsListItem> = [];
        details = this.createBasicPersonDetailsList();

        this.addAccountCard(userService, details, isBasic);
        this.addRelationshipCard(navProxy, popoverService, details, isCustomer);

        if (isBasic && canViewAdditionalPropertyValues) {
            this.createAdditionalPropertiesList(details);
            return details;
        }

        this.addDatesCard(details);

        if (canViewAdditionalPropertyValues) {
            this.createAdditionalPropertiesList(details);
        }

        this.addStatusCard(details);

        return details;
    }

    private addAccountCard(
        userService: UserService,
        details: Array<DetailsListItem>,
        isBasic: boolean ): void {

        const accountCard: DetailsListItemCard =
            new DetailsListItemCard( DetailsListItemCardType.Account, "Account");

        details.push(PersonDetailsHelper.createItem(
            accountCard,
            DetailsListItemGroupType.Account,
            this.email,
            DetailListItemDescriptionMap.accountEmail,
            'lock',
            false,
            null,
            false));

        if (this.linkedIdentities) {
            for (let linkedIdentity of this.linkedIdentities) {
                let unlinkAction: DetailsListItemActionIcon
                    = DetailListItemHelper.createAction(
                        () => userService.unlinkIdentity(this, linkedIdentity.authenticationMethodId),
                        'link-off',
                        IconLibrary.AngularMaterial);
                let linkedIdentityItem: DetailsListItem = DetailsListItem.createItem(
                    accountCard,
                    DetailsListItemGroupType.LinkedIdentities,
                    linkedIdentity.authenticationMethodName,
                    "Linked Identity",
                    "card-account-details",
                    IconLibrary.AngularMaterial);
                if (!isBasic) {
                    linkedIdentityItem = linkedIdentityItem.withAction(unlinkAction);
                }
                details.push(linkedIdentityItem);
            }
        }
    }

    private addRelationshipCard(
        navProxy: NavProxyService,
        popoverService: SharedPopoverService,
        details: Array<DetailsListItem>,
        isCustomer: boolean,
    ): void {
        let relationshipsCard: DetailsListItemCard =
        new DetailsListItemCard(DetailsListItemCardType.Relationships, "Relationships");

        let organisationAction: DetailsListItemActionIcon =
            DetailListItemHelper.createAction(
                () => navProxy.goToOrganisation(this.organisationId));

        details.push(DetailsListItem.createItem(
            relationshipsCard,
            DetailsListItemCardType.Relationships,
            this.organisationName,
            DetailListItemDescriptionMap.organisation,
            DetailListItemHelper.detailListItemIconMap.link)
            .withRelatedEntity(
                RelatedEntityType.Organisation,
                this.organisationId,
                null,
                null)
            .withAction(isCustomer ? null : organisationAction)
            .roundIcon());

        let portalAction: DetailsListItemActionIcon =
            DetailListItemHelper.createAction(() => navProxy.goToPortal(this.portalId),
                "link",
                IconLibrary.IonicV4,
                [Permission.ViewPortals]);

        let portalOverflowMenuAction: DetailsListItemActionIcon =
            DetailListItemHelper.createAction(() => popoverService.show(
                {
                    component: PopoverAssignPortalComponent,
                    componentProps: {
                        entityType: AssignPortalEntityType.User,
                        userId: this.id,
                        userName: this.fullName,
                        entityOrganisationId: this.organisationId,
                        entityTenantId: this.tenantId,
                        userType: this.userType,
                        portalId: this.portalId,
                        portalName: this.portalName,
                        hasPortal: this.portalId,
                    },
                    showBackdrop: false,
                    mode: 'md',
                    event: event,
                },
                'Portal more action popover',
            ),
            "more",
            IconLibrary.IonicV4,
            null,
            [
                Permission.ManageUsers,
                Permission.ManageUsersForOtherOrganisations,
                Permission.ManageOrganisationAdminUsers,
                Permission.ManageTenantAdminUsers]);

        details.push(DetailsListItem.createItem(
            relationshipsCard,
            DetailsListItemCardType.Relationships,
            this.portalId ? this.portalName : 'None',
            DetailListItemDescriptionMap.portal,
            DetailListItemHelper.detailListItemIconMap.link)
            .withRelatedEntity(RelatedEntityType.Portal,
                this.organisationId,
                null,
                null)
            .withActions(this.portalId ? portalAction : null, portalOverflowMenuAction));
    }

    private addDatesCard( details: Array<DetailsListItem>): void {
        let datesCard: DetailsListItemCard = new DetailsListItemCard(
            DetailsListItemCardType.Dates,
            "Dates");

        let calendarIcon: string = DetailListItemHelper.detailListItemIconMap.calendar;

        details.push(DetailsListItem.createItem(
            datesCard,
            DetailsListItemGroupType.Dates,
            this.createdDate || "-",
            DetailListItemDescriptionMap.createdDate,
            calendarIcon));

        details.push(
            DetailsListItem.createItem(
                datesCard,
                DetailsListItemGroupType.Dates,
                this.createdTime || "-",
                DetailListItemDescriptionMap.createdTime,
                calendarIcon));

        details.push(DetailsListItem.createItem(
            datesCard,
            DetailsListItemGroupType.Dates,
            this.lastModifiedDate || "-",
            DetailListItemDescriptionMap.lastModifiedDate,
            calendarIcon));
        details.push(DetailsListItem.createItem(
            datesCard,
            DetailsListItemGroupType.Dates,
            this.lastModifiedTime || "-",
            DetailListItemDescriptionMap.lastModifiedTime,
            calendarIcon));
    }

    private addStatusCard(details: Array<DetailsListItem>): void {
        let statusDetailsCard: DetailsListItemCard = new DetailsListItemCard(
            DetailsListItemCardType.Others,
            "Status");

        details.push(DetailsListItem.createItem(
            statusDetailsCard,
            DetailsListItemGroupType.Status,
            this.status || 'No',
            DetailListItemDescriptionMap.status,
            DetailListItemHelper.detailListItemIconMap.others));
    }

    private createBasicPersonDetailsList(): Array<DetailsListItem> {
        let persondetailsList: Array<DetailsListItem> = [];
        let personDetailsCard: DetailsListItemCard =
            new DetailsListItemCard(DetailsListItemCardType.ContactDetails,
                "Contact Details");
        let nameDescription: string = this.title && this.company ?
            this.title + " at " + this.company :
            this.title ? this.title :
                this.company ? this.company : '';
        persondetailsList.push(PersonDetailsHelper.createItem(
            personDetailsCard,
            DetailsListItemGroupType.Person,
            this.getAssembledFullName(),
            nameDescription,
            'none',
            false,
            null,
            false));

        persondetailsList = PersonDetailsHelper.createItemFromRepeatingFields(
            this.personRepeatingFields,
            persondetailsList,
            personDetailsCard,
            "phone",
            "call");
        persondetailsList = PersonDetailsHelper.createItemFromRepeatingFields(
            this.personRepeatingFields,
            persondetailsList,
            personDetailsCard,
            "email",
            "mail");
        persondetailsList = PersonDetailsHelper.createItemFromRepeatingFields(
            this.personRepeatingFields,
            persondetailsList,
            personDetailsCard,
            "address",
            "map");
        persondetailsList = PersonDetailsHelper.createItemFromRepeatingFields(
            this.personRepeatingFields,
            persondetailsList,
            personDetailsCard,
            "website",
            "globe");
        persondetailsList = PersonDetailsHelper.createItemFromRepeatingFields(
            this.personRepeatingFields,
            persondetailsList,
            personDetailsCard,
            "messenger",
            "chatboxes");
        persondetailsList = PersonDetailsHelper.createItemFromRepeatingFields(
            this.personRepeatingFields,
            persondetailsList,
            personDetailsCard,
            "social",
            "share");
        return persondetailsList;
    }

    private createAdditionalPropertiesList(details: any): void {
        AdditionalPropertiesHelper.createDetailsList(details, this.additionalPropertyValues);
    }

    private getAssembledFullName(): any {
        return [
            this.namePrefix,
            this.firstName,
            this.preferredName ? '(' + this.preferredName + ')' : '',
            this.middleNames,
            this.lastName,
            this.nameSuffix,
        ].filter((part: string) => !!part).join(" ") || null;
    }

    public setGroupByValue(userList: Array<UserViewModel>, groupBy: string): Array<UserViewModel> {
        if (groupBy === SortAndFilterBy.UserName) {
            userList.forEach((item: UserViewModel) => {
                item.groupByValue = item.fullName;
            });
        } else if (groupBy === SortAndFilterBy.LastModifiedDate) {
            userList.forEach((item: UserViewModel) => {
                item.groupByValue = item.lastModifiedDate;
            });
        } else {
            userList.forEach((item: UserViewModel) => {
                item.groupByValue = item.createdDate;
            });
        }

        return userList;
    }

    public setSortOptions(
        userList: Array<UserViewModel>,
        sortBy: string,
        sortDirection: SortDirection,
    ): Array<UserViewModel> {
        sortDirection = sortDirection == null ? SortDirection.Descending : sortDirection;

        if (sortBy === SortAndFilterBy.UserName) {
            userList.forEach((item: UserViewModel) => {
                item.sortByValue = item.fullName;
                item.sortDirection = sortDirection;
            });
        } else if (sortBy === SortAndFilterBy.LastModifiedDate) {
            userList.forEach((item: UserViewModel) => {
                item.sortByValue = item.lastModifiedDateTime;
                item.sortDirection = sortDirection;
            });
        } else {
            userList.forEach((item: UserViewModel) => {
                item.sortByValue = item.createdDateTime;
                item.sortDirection = sortDirection;
            });
        }

        return userList;
    }
}
