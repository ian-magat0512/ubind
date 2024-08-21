/* eslint-disable max-classes-per-file */
import { PhoneNumberPipe } from "../pipes/phone-number.pipe";
import { DetailsListItem } from "../models/details-list/details-list-item";
import { DetailsListItemGroupType } from "../models/details-list/details-list-item-type.enum";
import { DetailsListItemActionIcon } from "../models/details-list/details-list-item-action-icon";
import { DetailsListItemCard } from "../models/details-list/details-list-item-card";
import { DetailsListItemCardType } from "../models/details-list/details-list-item-card-type.enum";
import { DetailsListFormItem } from "../models/details-list/details-list-form-item";
import { FormValidatorHelper } from "./form-validator.helper";
import { PersonCategory } from "../models";
import { PersonRepeatingFieldsResourceModel } from "../resource-models/person/person-repeating-fields.resource-model";
import { AdditionalPropertyValue } from "@app/models/additional-property-item-view.model";
import { AdditionalPropertiesHelper } from "./additional-properties.helper";
import { IconLibrary } from "@app/models/icon-library.enum";
import { DetailsListFormTextItem } from "@app/models/details-list/details-list-form-text-item";
import { DetailsListFormEmailItem } from "@app/models/details-list/details-list-form-email-item";
import { DetailsListFormCheckboxGroupItem } from "@app/models/details-list/details-list-form-checkbox-group-item";
import { DetailsListFormPhoneItem } from "@app/models/details-list/details-list-form-phone-item";
import { DetailsListFormAddressItem } from "@app/models/details-list/details-list-form-address-item";

/**
 * Maps common icons to their names
 */
export class DetailListItemIconMap {
    public static person: string = "person";
    public static phone: string = "call";
    public static email: string = "mail";
    public static others: string = "folder";
    public static link: string = "link";
    public static calendar: string = "calendar";
    public static business: string = "business";
    public static lock: string = "lock";
    public static contact: string = "contact";
    public static globe: string = "globe";
    public static chatBoxes: string = "chatboxes";
    public static share: string = "share";
    public static map: string = "map";
    public static properties: string = "brush";
    public static wallet: string = 'wallet';
    public static userRole: string = "shirt";
    public static portal: string = 'browsers';
}

/**
 * Maps common descriptions from a key
 */
export class DetailListItemDescriptionMap {
    public static customer: string = "Customer";
    public static fullName: string = "Full Name";
    public static namePrefix: string = "Name Prefix";
    public static firstName: string = "First Name";
    public static middleNames: string = "Middle Names";
    public static lastName: string = "Last Name";
    public static nameSuffix: string = "Name Suffix";
    public static company: string = "Company";
    public static title: string = "Title";
    public static preferredName: string = "Preferred Name";
    public static phone: string = "Phone";
    public static mobilePhone: string = "Mobile";
    public static mobilePhoneNumber: string = "Mobile";
    public static homePhone: string = "Home";
    public static homePhoneNumber: string = "Home";
    public static workPhone: string = "Work";
    public static workPhoneNumber: string = "Work";
    public static email: string = "Email";
    public static accountEmail: string = "Account Email";
    public static alternativeEmail: string = "Work";
    public static hasActivePolicy: string = "Has Active Policy";
    public static hasActiveProtection: string = "Has Active Protection";
    public static userStatus: string = "User Account Status";
    public static status: string = "Status";
    public static createdDate: string = "Created Date";
    public static createdTime: string = "Created Time";
    public static userType: string = "User Type";
    public static referrer: string = "Agent";
    public static website: string = "Website";
    public static messenger: string = "Messenger";
    public static social: string = "Social";
    public static lastModifiedDate: string = "Last Modified Date";
    public static lastModifiedTime: string = "Last Modified Time";
    public static applicableRoles: string = "Initial Roles";
    public static properties: string = "Properties";
    public static agent: string = "Agent";
    public static organisation: string = "Organisation";
    public static portal: string = "Portal";
    public static address: string = "Address";
}

/**
 * Maps a field name to a group name.
 */
export class DetailListItemGroupMap {
    public static fullName: string = DetailsListItemGroupType.Person;
    public static preferredName: string = DetailsListItemGroupType.Person;
    public static mobilePhone: string = DetailsListItemGroupType.Phone;
    public static mobilePhoneNumber: string = DetailsListItemGroupType.Phone;
    public static homePhone: string = DetailsListItemGroupType.Phone;
    public static homePhoneNumber: string = DetailsListItemGroupType.Phone;
    public static workPhone: string = DetailsListItemGroupType.Phone;
    public static workPhoneNumber: string = DetailsListItemGroupType.Phone;
    public static email: string = DetailsListItemGroupType.Email;
    public static alternativeEmail: string = DetailsListItemGroupType.Email;
}

/**
 * Creates data structures for rendering person details in views or edit forms.
 */
export class PersonDetailsHelper {
    // Maps
    public static createItem(
        card: DetailsListItemCard,
        group: string,
        title: string,
        description: string,
        icon: string,
        hasLink: boolean = false,
        actionIcon: DetailsListItemActionIcon = null,
        isRoundIcon: boolean = false,
        header: string = "",
    ): DetailsListItem {
        let detailsItem: DetailsListItem = DetailsListItem.createItem(
            card,
            group,
            title,
            description,
            icon,
            IconLibrary.IonicV4,
            header,
        ).withAction(actionIcon).formatItemDisplay();

        if (hasLink) {
            detailsItem = detailsItem.withLink();
        }
        if (isRoundIcon) {
            detailsItem = detailsItem.roundIcon();
        }

        return detailsItem;
    }

    public static createDetailsList(detailListItem: Array<DetailsListItem>, model: any): Array<DetailsListItem> {
        const keys: Array<string> = Object.keys(model);
        const personDetailsCard: DetailsListItemCard =
            new DetailsListItemCard(DetailsListItemCardType.ContactDetails, 'Contact Details');
        keys.forEach((element: string) => {
            const mappedType: any = DetailListItemGroupMap[element];
            const mappedDescription: any = DetailListItemDescriptionMap[element];
            if (mappedType) {
                detailListItem.push(
                    this.createItem(
                        personDetailsCard,
                        mappedType,
                        model[element],
                        mappedDescription,
                        DetailListItemIconMap[mappedType],
                        false));
            }
        });
        return detailListItem;
    }

    public static createPersonDetailsListForEdit(
        isEdit: boolean,
        category: PersonCategory,
        additionalPropertyValueFields: Array<AdditionalPropertyValue>,
        canEditAdditionalPropertyValues: boolean,
        hasUserAccount: boolean,
    ): Array<DetailsListFormItem> {
        let personDetailsFormItems: Array<DetailsListFormItem> = [];
        let descriptions: typeof DetailListItemDescriptionMap = DetailListItemDescriptionMap;
        let icons: typeof DetailListItemIconMap = DetailListItemIconMap;
        let validator: typeof FormValidatorHelper = FormValidatorHelper;
        let personNameActionIcon: DetailsListItemActionIcon =
            new DetailsListItemActionIcon(["chevron-down", "chevron-up"]);
        let contactDetailsCard: DetailsListItemCard
            = new DetailsListItemCard(DetailsListItemCardType.ContactDetails, 'Contact Details');

        personDetailsFormItems.push(DetailsListFormTextItem.create(
            contactDetailsCard,
            "namePrefix",
            descriptions.namePrefix)
            .withGroupName(DetailsListItemGroupType.Person)
            .withValidator(validator.nameValidator())
            .withIcon(icons.person, IconLibrary.IonicV4)
            .withAction(personNameActionIcon)
            .hiddenOnInit());
        personDetailsFormItems.push(DetailsListFormTextItem.create(
            contactDetailsCard,
            "firstName",
            descriptions.firstName)
            .withGroupName(DetailsListItemGroupType.Person)
            .withValidator(validator.nameValidator(true))
            .withIcon(icons.person, IconLibrary.IonicV4)
            .withAction(personNameActionIcon));
        personDetailsFormItems.push(DetailsListFormTextItem.create(
            contactDetailsCard,
            "middleNames",
            descriptions.middleNames)
            .withGroupName(DetailsListItemGroupType.Person)
            .withValidator(validator.nameValidator())
            .hiddenOnInit());
        personDetailsFormItems.push(DetailsListFormTextItem.create(
            contactDetailsCard,
            "lastName",
            descriptions.lastName)
            .withGroupName(DetailsListItemGroupType.Person)
            .withValidator(validator.nameValidator()));
        personDetailsFormItems.push(DetailsListFormTextItem.create(
            contactDetailsCard,
            "nameSuffix",
            descriptions.nameSuffix)
            .withGroupName(DetailsListItemGroupType.Person)
            .withValidator(validator.nameValidator())
            .hiddenOnInit());
        personDetailsFormItems.push(DetailsListFormTextItem.create(
            contactDetailsCard,
            "preferredName",
            descriptions.preferredName)
            .withGroupName(DetailsListItemGroupType.Person)
            .withValidator(validator.nameValidator())
            .hiddenOnInit());

        if (category == PersonCategory.User || (isEdit && hasUserAccount)) {
            personDetailsFormItems.push(DetailsListFormEmailItem.create(
                contactDetailsCard,
                "accountEmail",
                descriptions.accountEmail)
                .withValidator(validator.emailValidator(true))
                .withIcon(icons.lock, IconLibrary.IonicV4)
                .withGroupName(DetailsListItemGroupType.Account));
        }

        if (!isEdit && category == PersonCategory.User) {
            personDetailsFormItems.push(DetailsListFormCheckboxGroupItem.create(
                contactDetailsCard,
                "applicableRoles",
                descriptions.applicableRoles)
                .withIcon(icons.userRole, IconLibrary.IonicV4)
                .withGroupName(DetailsListItemGroupType.Others));
        }

        personDetailsFormItems.push(DetailsListFormTextItem.create(
            contactDetailsCard,
            "company",
            descriptions.company)
            .withValidator(validator.companyNameValidator())
            .withIcon(icons.business, IconLibrary.IonicV4)
            .withGroupName(DetailsListItemGroupType.Company));
        personDetailsFormItems.push(DetailsListFormTextItem.create(
            contactDetailsCard,
            "title",
            descriptions.title)
            .withValidator(validator.nameValidator())
            .withGroupName(DetailsListItemGroupType.Company));
        personDetailsFormItems.push(DetailsListFormPhoneItem.create(
            contactDetailsCard,
            "phone",
            descriptions.phone)
            .withValidator(validator.phoneValidator())
            .withIcon(icons.phone, IconLibrary.IonicV4)
            .withGroupName(DetailsListItemGroupType.Phone)
            .asRepeating());
        personDetailsFormItems.push(DetailsListFormEmailItem.create(
            contactDetailsCard,
            "email",
            descriptions.email)
            .withValidator(validator.emailValidator())
            .withIcon(icons.email, IconLibrary.IonicV4)
            .withGroupName(DetailsListItemGroupType.Email)
            .asRepeating());
        personDetailsFormItems.push(DetailsListFormAddressItem.create(
            contactDetailsCard,
            "address",
            descriptions.address)
            .withIcon(icons.map, IconLibrary.IonicV4)
            .withGroupName(DetailsListItemGroupType.Address)
            .asRepeating());
        personDetailsFormItems.push(DetailsListFormTextItem.create(
            contactDetailsCard,
            "website",
            descriptions.website)
            .withValidator(validator.url())
            .withIcon(icons.globe, IconLibrary.IonicV4)
            .withGroupName(DetailsListItemGroupType.Website)
            .asRepeating());
        personDetailsFormItems.push(DetailsListFormTextItem.create(
            contactDetailsCard,
            "messenger",
            descriptions.messenger)
            .withIcon(icons.chatBoxes, IconLibrary.IonicV4)
            .withGroupName(DetailsListItemGroupType.Messenger)
            .asRepeating());
        personDetailsFormItems.push(DetailsListFormTextItem.create(
            contactDetailsCard,
            "social",
            descriptions.social)
            .withIcon(icons.share, IconLibrary.IonicV4)
            .withGroupName(DetailsListItemGroupType.Social)
            .asRepeating());

        if (canEditAdditionalPropertyValues) {
            let additionalPropertyDetailsFormItem: Array<DetailsListFormItem>
                = AdditionalPropertiesHelper.getDetailListForEdit(additionalPropertyValueFields);
            personDetailsFormItems = personDetailsFormItems.concat(additionalPropertyDetailsFormItem);
        }

        return personDetailsFormItems;
    }

    public static createItemFromRepeatingFields(
        repeatingFields: PersonRepeatingFieldsResourceModel,
        personDetails: Array<DetailsListItem>,
        card: DetailsListItemCard,
        groupName: string,
        groupIcon: string): Array<DetailsListItem> {

        if (repeatingFields) {
            if (groupName == DetailsListItemGroupType.Email && repeatingFields.emailAddresses) {
                for (let field of repeatingFields.emailAddresses) {
                    personDetails.push(this.createRepeatingItem(
                        card,
                        groupName,
                        field.emailAddress,
                        field.label,
                        field.customLabel,
                        groupIcon,
                    ));
                }
            }

            if (groupName == DetailsListItemGroupType.Phone && repeatingFields.phoneNumbers) {
                for (let field of repeatingFields.phoneNumbers) {
                    personDetails.push(this.createRepeatingItem(
                        card,
                        groupName,
                        field.phoneNumber,
                        field.label,
                        field.customLabel,
                        groupIcon,
                    ));
                }
            }

            if (groupName == DetailsListItemGroupType.Address && repeatingFields.streetAddresses) {
                for (let field of repeatingFields.streetAddresses) {
                    let value: string = field.address + ',\n' +
                        field.suburb +
                        ' ' + field.state +
                        ' ' + field.postcode;
                    personDetails.push(this.createRepeatingItem(
                        card,
                        groupName,
                        value,
                        field.label,
                        field.customLabel,
                        groupIcon,
                    ));
                }
            }

            if (groupName == DetailsListItemGroupType.Website && repeatingFields.websiteAddresses) {
                for (let field of repeatingFields.websiteAddresses) {
                    personDetails.push(this.createRepeatingItem(
                        card,
                        groupName,
                        field.websiteAddress,
                        field.label,
                        field.customLabel,
                        groupIcon,
                    ));
                }
            }

            if (groupName == DetailsListItemGroupType.Messenger && repeatingFields.messengerIds) {
                for (let field of repeatingFields.messengerIds) {
                    personDetails.push(this.createRepeatingItem(
                        card,
                        groupName,
                        field.messengerId,
                        field.label,
                        field.customLabel,
                        groupIcon,
                    ));
                }
            }

            if (groupName == DetailsListItemGroupType.Social && repeatingFields.socialMediaIds) {
                for (let field of repeatingFields.socialMediaIds) {
                    personDetails.push(this.createRepeatingItem(
                        card,
                        groupName,
                        field.socialMediaId,
                        field.label,
                        field.customLabel,
                        groupIcon,
                    ));
                }
            }
        }

        return personDetails;
    }

    private static createRepeatingItem(
        category: DetailsListItemCard,
        groupName: string,
        value: string,
        label: string,
        customLabel: string,
        icon: string,
    ): DetailsListItem {
        let description: string = label == "other" ? customLabel : label;
        return this.formatItemDisplay(DetailsListItem.createItem(
            category,
            groupName,
            value,
            description,
            icon,
        ));
    }

    private static formatItemDisplay(detailsItem: DetailsListItem): DetailsListItem {
        detailsItem.Description = detailsItem.Description.charAt(0).toUpperCase() +
            detailsItem.Description.slice(1);
        if (detailsItem.GroupName == DetailsListItemGroupType.Phone) {
            let phoneNumberPipe: PhoneNumberPipe = new PhoneNumberPipe();
            detailsItem.DisplayValue = detailsItem.DisplayValue
                ? phoneNumberPipe.transform(detailsItem.DisplayValue)
                : "";
        }

        if (detailsItem.GroupName == DetailsListItemGroupType.Email) {
            detailsItem.DisplayValue = detailsItem.DisplayValue ? detailsItem.DisplayValue.toLowerCase() : "";
        }

        return detailsItem;
    }
}
