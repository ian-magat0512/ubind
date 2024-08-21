import { UserStatus, PersonResourceModel } from '@app/models';
import {
    RepeatingFieldResourceModel,
    RepeatingAddressFieldResourceModel,
} from '@app/resource-models/repeating-field.resource-model';
import { DetailsListItemGroupType } from '../models/details-list/details-list-item-type.enum';
import { AdditionalPropertyValue } from '@app/models/additional-property-item-view.model';
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import {
    DetailListItemDescriptionMap,
    DetailListItemIconMap,
    PersonDetailsHelper,
} from '@app/helpers/person-details.helper';
import { LocalDateHelper } from '@app/helpers';
import { DetailsListItemCard } from '@app/models/details-list/details-list-item-card';
import { DetailsListItemCardType } from '@app/models/details-list/details-list-item-card-type.enum';
import { DetailsListItemActionIcon } from '@app/models/details-list/details-list-item-action-icon';
import { PersonRepeatingFieldsResourceModel } from '../resource-models/person/person-repeating-fields.resource-model';
import { RelatedEntityType } from '../models/related-entity-type.enum';
import { DetailListItemHelper } from '../helpers/detail-list-item.helper';
import { UserResourceModel } from '../resource-models/user/user.resource-model';
import { CustomerDetailsResourceModel } from '../resource-models/customer.resource-model';
import { EntityType } from '@app/models/entity-type.enum';

/**
 * View model for a person
 */
export class PersonViewModel {

    public id: string;

    // Entity Type is used to store the type of entity either Person, Customer or User.
    public entityType: string;
    // Entity Id is used to store the id of the entity that the person is associated with.
    // This will either be the Person Id, Customer Id or the User Id.
    public entityId: string;
    public tenantId: string;
    public organisationId: string;
    public preferredName: string;
    public fullName: string;
    public displayName: string;
    public namePrefix: string;
    public firstName: string;
    public middleNames: string;
    public lastName: string;
    public nameSuffix: string;
    public company: string;
    public title: string;
    public email: string;
    public alternativeEmail: string;
    public mobilePhoneNumber: string;
    public homePhoneNumber: string;
    public workPhoneNumber: string;
    public pictureId: string;
    public userType: string;
    public createdDateTime: string;
    public status: UserStatus;
    public ownerId: string;
    public repeatingFields: Array<RepeatingFieldResourceModel>;
    public repeatingAddressFields: Array<RepeatingAddressFieldResourceModel>;
    public additionalPropertyValues: Array<AdditionalPropertyValue>;
    public customerId: string;
    public ownerFullName: string;
    public createdDate: string;
    public createdTime: string;
    public lastModifiedDate: string;
    public lastModifiedTime: string;
    public hasActivePolicies: boolean;
    public personRepeatingFields: PersonRepeatingFieldsResourceModel;
    public description: string;
    public deleteFromList: boolean;

    public constructor(
        person: PersonResourceModel,
        entityType: EntityType = EntityType.Person,
        entityId: string = null,
    ) {
        if (!person) {
            return;
        }

        this.id = person.id;
        this.entityType = entityType;
        this.entityId = entityId || person.id;
        this.ownerId = person.ownerId;
        this.tenantId = person.tenantId;
        this.organisationId = person.organisationId;
        this.preferredName = person.preferredName;
        this.fullName = person.fullName;
        this.displayName = person.displayName;
        this.namePrefix = person.namePrefix;
        this.firstName = person.firstName;
        this.middleNames = person.middleNames;
        this.lastName = person.lastName;
        this.nameSuffix = person.nameSuffix;
        this.company = person.company;
        this.title = person.title;
        this.email = person.email;
        this.pictureId = person.pictureId;
        this.userType = person.userType;
        this.status = person.userStatus ? person.blocked ?
            UserStatus.Disabled : person.userStatus : UserStatus.New;
        this.setRepeatingFields(person);
        this.additionalPropertyValues = person.additionalPropertyValues;
        this.customerId = person.customerId;
        this.createdDateTime = person.createdDateTime;
        this.createdDate = LocalDateHelper.toLocalDate(person.createdDateTime);
        this.createdTime = LocalDateHelper.convertToLocalAndGetTimeOnly(person.createdDateTime);
        this.lastModifiedDate = LocalDateHelper.toLocalDate(person.lastModifiedDateTime);
        this.lastModifiedTime = LocalDateHelper.convertToLocalAndGetTimeOnly(person.lastModifiedDateTime);
        this.hasActivePolicies = person.hasActivePolicies;
        this.personRepeatingFields = {
            emailAddresses: person.emailAddresses,
            phoneNumbers: person.phoneNumbers,
            streetAddresses: person.streetAddresses,
            websiteAddresses: person.websiteAddresses,
            messengerIds: person.messengerIds,
            socialMediaIds: person.socialMediaIds,
        };

        this.description = this.status.replace("Deactivated", "Disabled").replace("New", "");
        this.ownerFullName = person.ownerFullName;
    }

    public static createPersonFromUser(userResourceModel: UserResourceModel): PersonViewModel {
        let personViewModel: PersonViewModel =
            new PersonViewModel(userResourceModel, EntityType.User, userResourceModel.id);
        personViewModel.id = userResourceModel.personId;
        return personViewModel;
    }

    public static createPersonFromCustomer(customerResourceModel: CustomerDetailsResourceModel): PersonViewModel {
        let personViewModel: PersonViewModel =
            new PersonViewModel(customerResourceModel, EntityType.Customer, customerResourceModel.id);
        personViewModel.id = customerResourceModel.primaryPersonId;
        return personViewModel;
    }

    // collate all repeating fields into a single array.
    private setRepeatingFields(person: PersonResourceModel): void {
        this.repeatingFields = [];
        this.repeatingAddressFields = [];
        if (person.emailAddresses) {
            let seqNo: number = 0;
            for (let email of person.emailAddresses) {
                let field: RepeatingFieldResourceModel = {
                    parentFieldName: DetailsListItemGroupType.Email,
                    label: email.label,
                    customLabel: email.customLabel,
                    name: DetailsListItemGroupType.Email + seqNo,
                    value: email.emailAddress,
                    referenceId: null,
                    sequenceNo: seqNo,
                    default: email ? email.default : null,
                };
                this.repeatingFields.push(field);
                seqNo++;
            }
        }

        if (person.phoneNumbers) {
            let seqNo: number = 0;
            for (let phone of person.phoneNumbers) {
                let field: RepeatingFieldResourceModel = {
                    parentFieldName: DetailsListItemGroupType.Phone,
                    label: phone.label,
                    customLabel: phone.customLabel,
                    name: DetailsListItemGroupType.Phone + seqNo,
                    value: phone.phoneNumber,
                    referenceId: null,
                    sequenceNo: seqNo,
                    default: phone ? phone.default : null,
                };
                this.repeatingFields.push(field);
                seqNo++;
            }
        }

        if (person.websiteAddresses) {
            let seqNo: number = 0;
            for (let website of person.websiteAddresses) {
                let field: RepeatingFieldResourceModel = {
                    parentFieldName: DetailsListItemGroupType.Website,
                    label: website.label,
                    customLabel: website.customLabel,
                    name: DetailsListItemGroupType.Website + seqNo,
                    value: website.websiteAddress,
                    referenceId: null,
                    sequenceNo: seqNo,
                    default: website ? website.default : null,
                };
                this.repeatingFields.push(field);
                seqNo++;
            }
        }

        if (person.messengerIds) {
            let seqNo: number = 0;
            for (let messenger of person.messengerIds) {
                let field: RepeatingFieldResourceModel = {
                    parentFieldName: DetailsListItemGroupType.Messenger,
                    label: messenger.label,
                    customLabel: messenger.customLabel,
                    name: DetailsListItemGroupType.Messenger + seqNo,
                    value: messenger.messengerId,
                    referenceId: null,
                    sequenceNo: seqNo,
                    default: messenger ? messenger.default : null,
                };
                this.repeatingFields.push(field);
                seqNo++;
            }
        }

        if (person.socialMediaIds) {
            let seqNo: number = 0;
            for (let social of person.socialMediaIds) {
                let field: RepeatingFieldResourceModel = {
                    parentFieldName: DetailsListItemGroupType.Social,
                    label: social.label,
                    customLabel: social.customLabel,
                    name: DetailsListItemGroupType.Social + seqNo,
                    value: social.socialMediaId,
                    referenceId: null,
                    sequenceNo: seqNo,
                    default: social ? social.default : null,
                };
                this.repeatingFields.push(field);
                seqNo++;
            }
        }

        if (person.streetAddresses) {
            let seqNo: number = 0;
            for (let address of person.streetAddresses) {
                let field: RepeatingAddressFieldResourceModel = {
                    parentFieldName: DetailsListItemGroupType.Address,
                    label: address.label,
                    customLabel: address.customLabel,
                    name: DetailsListItemGroupType.Address + seqNo,
                    value: null,
                    referenceId: null,
                    sequenceNo: seqNo,
                    address: address.address,
                    suburb: address.suburb,
                    postcode: address.postcode,
                    state: address.state,
                    default: address ? address.default : null,
                };
                this.repeatingAddressFields.push(field);
                seqNo++;
            }
        }
    }

    public createDetailsList(source: any): Array<DetailsListItem> {
        let customerDetails: Array<DetailsListItem> = [];
        let descriptions: typeof DetailListItemDescriptionMap = DetailListItemDescriptionMap;
        let icons: typeof DetailListItemIconMap = DetailListItemIconMap;
        let personDetailsCard: DetailsListItemCard =
            new DetailsListItemCard(
                DetailsListItemCardType.ContactDetails,
                "Contact Details");
        let nameDescription: string = this.title && this.company ?
            this.title + " at  " + this.company :
            this.title ? this.title :
                this.company ? this.company : '';
        customerDetails.push(DetailsListItem.createItem(
            personDetailsCard, DetailsListItemGroupType.Person,
            this.getAssembledFullName(), nameDescription, 'none'));

        let relationshipsCard: DetailsListItemCard =
            new DetailsListItemCard(DetailsListItemCardType.Relationships, "Relationships");
        let datesCard: DetailsListItemCard =
            new DetailsListItemCard(DetailsListItemCardType.Dates, "Dates");

        customerDetails = PersonDetailsHelper.createItemFromRepeatingFields(
            this.personRepeatingFields,
            customerDetails,
            personDetailsCard,
            "phone",
            "call",
        );
        customerDetails = PersonDetailsHelper.createItemFromRepeatingFields(
            this.personRepeatingFields,
            customerDetails,
            personDetailsCard,
            "email",
            "mail",
        );
        customerDetails = PersonDetailsHelper.createItemFromRepeatingFields(
            this.personRepeatingFields,
            customerDetails,
            personDetailsCard,
            "address",
            "map",
        );
        customerDetails = PersonDetailsHelper.createItemFromRepeatingFields(
            this.personRepeatingFields,
            customerDetails,
            personDetailsCard,
            "website",
            "globe",
        );
        customerDetails = PersonDetailsHelper.createItemFromRepeatingFields(
            this.personRepeatingFields,
            customerDetails,
            personDetailsCard,
            "messenger",
            "chatbox",
        );
        customerDetails = PersonDetailsHelper.createItemFromRepeatingFields(
            this.personRepeatingFields,
            customerDetails,
            personDetailsCard,
            "social",
            "share-social",
        );

        if (this.status != UserStatus.New) {
            customerDetails.push(PersonDetailsHelper.createItem(
                personDetailsCard,
                DetailsListItemGroupType.Account,
                this.email,
                descriptions.accountEmail,
                'lock',
                false,
                null,
                false));
        }

        let ownerActionIcon: DetailsListItemActionIcon =
            this.ownerId && DetailListItemHelper.createAction(
                () => source.navProxy.goToOwner(this.ownerId));
        customerDetails.push(
            DetailsListItem
                .createItem(
                    relationshipsCard,
                    DetailsListItemGroupType.Relationships,
                    this.ownerFullName || 'None',
                    descriptions.agent,
                    icons.link)
                .withAction(ownerActionIcon)
                .roundIcon());

        let customerLinkAction: DetailsListItemActionIcon =
            DetailListItemHelper.createAction(
                () => source.navProxy.goToCustomer(this.customerId));
        customerDetails.push(
            DetailsListItem
                .createItem(
                    relationshipsCard,
                    DetailsListItemGroupType.Relationships,
                    source.customerDisplayName || 'None',
                    descriptions.customer,
                    icons.link)
                .withAction(customerLinkAction)
                .withRelatedEntity(RelatedEntityType.Customer, this.organisationId, this.ownerId, null));

        customerDetails.push(
            DetailsListItem
                .createItem(
                    datesCard,
                    DetailsListItemGroupType.Dates,
                    this.createdDate || "-",
                    descriptions.createdDate,
                    icons.calendar));
        customerDetails.push(
            DetailsListItem
                .createItem(
                    datesCard,
                    DetailsListItemGroupType.Dates,
                    this.createdTime || "-",
                    descriptions.createdTime,
                    icons.calendar));
        customerDetails.push(
            DetailsListItem
                .createItem(
                    datesCard,
                    DetailsListItemGroupType.Dates,
                    this.lastModifiedDate || "-",
                    descriptions.lastModifiedDate,
                    icons.calendar));
        customerDetails.push(
            DetailsListItem
                .createItem(
                    datesCard,
                    DetailsListItemGroupType.Dates,
                    this.lastModifiedTime || "-",
                    descriptions.lastModifiedTime,
                    icons.calendar));

        let statusDetailsCard: DetailsListItemCard =
            new DetailsListItemCard(DetailsListItemCardType.Others, "Status");
        const statusDisplay: any = {
            deactivated: 'Disabled',
            new: 'None',
        };
        customerDetails.push(
            DetailsListItem.createItem(
                statusDetailsCard,
                DetailsListItemGroupType.Others,
                statusDisplay[this.status.toLowerCase()] || this.status,
                descriptions.userStatus,
                icons.others));

        return customerDetails;
    }

    private getAssembledFullName(): string {
        return [
            this.namePrefix,
            this.firstName,
            this.preferredName ? `(${this.preferredName})` : '',
            this.middleNames,
            this.lastName,
            this.nameSuffix,
        ].filter((part: string) => !!part).join(" ") || null;
    }
}
