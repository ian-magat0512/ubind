import { CustomerDetailsResourceModel } from '@app/resource-models/customer.resource-model';
import { UserStatus } from '@app/models';
import { QuoteViewModel } from './quote.viewmodel';
import { PolicyViewModel } from './policy.viewmodel';
import { ClaimViewModel } from './claim.viewmodel';
import { LocalDateHelper, Permission } from '@app/helpers';
import { DetailsListItem } from '../models/details-list/details-list-item';
import {
    DetailListItemDescriptionMap,
    DetailListItemIconMap,
    PersonDetailsHelper,
} from '@app/helpers/person-details.helper';
import { DetailsListItemGroupType } from '@app/models/details-list/details-list-item-type.enum';
import { DetailsListItemActionIcon } from '@app/models/details-list/details-list-item-action-icon';
import { DetailsListItemCard } from '@app/models/details-list/details-list-item-card';
import { DetailsListItemCardType } from '@app/models/details-list/details-list-item-card-type.enum';
import {
    RepeatingFieldResourceModel, RepeatingAddressFieldResourceModel,
} from '../resource-models/repeating-field.resource-model';
import { PersonViewModel } from './person.viewmodel';
import { AdditionalPropertyValue } from '@app/models/additional-property-item-view.model';
import { AdditionalPropertiesHelper } from '@app/helpers/additional-properties.helper';
import { AccountingTransactionHistoryItemViewModel } from './accounting-transaction-history-item.viewmodel';
import { AccountingTransactionResourceModel } from '@app/resource-models/accounting-transaction.resource-model';
import { PolicyResourceModel } from '@app/resource-models/policy.resource-model';
import { QuoteResourceModel } from '@app/resource-models/quote.resource-model';
import { PersonRepeatingFieldsResourceModel } from '../resource-models/person/person-repeating-fields.resource-model';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { DetailListItemHelper } from '@app/helpers/detail-list-item.helper';
import { SharedPopoverService } from '@app/services/shared-popover.service';
import { PopoverAgentMoreSelectionComponent }
    from '@app/components/popover-agent-more-selection/popover-agent-more-selection.component';
import { RelatedEntityType } from '@app/models/related-entity-type.enum';
import { DateHelper } from '@app/helpers/date.helper';
import { PopoverAssignPortalComponent } from '../components/popover-assign-portal/popover-assign-portal.component';
import { PermissionService } from '../services/permission.service';
import { IconLibrary } from '@app/models/icon-library.enum';
import { AssignPortalEntityType } from "@app/models/assign-portal-entity-type.enum";

/**
 * View model for customer detail.
 */
export class CustomerDetailViewModel {

    public id: string;
    public organisationId: string;
    public organisationName: string;
    public tenantId: string;
    public fullName: string;
    public namePrefix: string;
    public firstName: string;
    public middleNames: string;
    public lastName: string;
    public nameSuffix: string;
    public company: string;
    public title: string;
    public preferredName: string;
    public email: string;
    public alternativeEmail: string;
    public mobilePhoneNumber: string;
    public homePhoneNumber: string;
    public workPhoneNumber: string;
    public customerStatus: string;
    public hasActivePolicy: string;
    public userStatus: string;
    public createdDate: string;
    public createdTime: string;
    public lastModifiedDate: string;
    public lastModifiedTime: string;
    public ownerFullName: string;
    public ownerId: string;
    public quotes: Array<QuoteViewModel> = [];
    public policies: Array<PolicyViewModel> = [];
    public claims: Array<ClaimViewModel> = [];
    public repeatingFields: Array<RepeatingFieldResourceModel> = [];
    public repeatingAddressFields: Array<RepeatingAddressFieldResourceModel> = [];
    public portalId: string;
    public portalName: string;

    public people: Array<PersonViewModel> = [];
    public transactions: Array<AccountingTransactionHistoryItemViewModel> = [];
    public accountBalance: string;
    public accountBalanceSubtext: string;
    public personRepeatingFields: PersonRepeatingFieldsResourceModel;
    public additionalPropertyValues: Array<AdditionalPropertyValue> = [];
    public primaryPersonId: string;

    public constructor(customer: CustomerDetailsResourceModel) {
        if (!customer) {
            return;
        }

        this.id = customer.id;
        this.organisationId = customer.organisationId;
        this.organisationName = customer.organisationName;
        this.tenantId = customer.tenantId;
        this.fullName = customer.fullName;
        this.firstName = customer.firstName;
        this.lastName = customer.lastName;
        this.middleNames = customer.middleNames;
        this.namePrefix = customer.namePrefix;
        this.nameSuffix = customer.nameSuffix;
        this.company = customer.company;
        this.title = customer.title;
        this.preferredName = customer.preferredName;
        this.email = customer.email;
        this.customerStatus = customer.userStatus;
        this.userStatus = customer.userStatus;
        this.portalId = customer.portalId;
        this.portalName = customer.portalName;

        // just for show
        if (customer.userStatus === UserStatus.Deactivated) {
            this.userStatus = UserStatus.Disabled;
        }

        this.ownerFullName = customer.ownerFullName;
        this.ownerId = customer.ownerId;
        this.createdDate = DateHelper.formatDDMMMYYYY(customer.createdDateTime);
        this.createdTime = LocalDateHelper.convertToLocalAndGetTimeOnly(customer.createdDateTime);
        this.lastModifiedDate = DateHelper.formatDDMMMYYYY(customer.lastModifiedDateTime);
        this.lastModifiedTime = LocalDateHelper.convertToLocalAndGetTimeOnly(customer.lastModifiedDateTime);
        customer.quotes.forEach((quote: QuoteResourceModel) => {
            this.quotes.push(new QuoteViewModel(quote));
        });
        customer.policies.forEach((policy: PolicyResourceModel) => {
            this.policies.push(new PolicyViewModel(policy));
        });

        customer.transactions.forEach((transaction: AccountingTransactionResourceModel) => {
            this.transactions.push(new AccountingTransactionHistoryItemViewModel(transaction));
        });

        this.hasActivePolicy = this.policies.filter((p: PolicyViewModel) =>
            p.status === 'Active').length > 0 ? 'Yes' : 'No';
        this.claims = customer.claims.map((item: any) => new ClaimViewModel(item));
        this.people = customer.people ? customer.people.map((item: any) => new PersonViewModel(item)) : [];
        this.accountBalance = customer.accountBalance;
        this.accountBalanceSubtext = customer.accountBalanceSubtext;
        this.personRepeatingFields = {
            emailAddresses: customer.emailAddresses,
            phoneNumbers: customer.phoneNumbers,
            streetAddresses: customer.streetAddresses,
            websiteAddresses: customer.websiteAddresses,
            messengerIds: customer.messengerIds,
            socialMediaIds: customer.socialMediaIds,
        };

        this.additionalPropertyValues = customer.additionalPropertyValues;
        this.primaryPersonId = customer.primaryPersonId;
    }

    public createPersonDetailsList(source: any): Array<DetailsListItem> {
        let people: Array<DetailsListItem> = [];
        let i: number = 0;
        this.people.forEach((person: PersonViewModel) => {
            let status: string = person.status.replace("Deactivated", "Disabled");
            if (this.primaryPersonId == person.id) {
                status = person.status == UserStatus.New ? "Primary" : status + ' · Primary';
            } else {
                status = person.status == UserStatus.New ? "" : status;
            }

            let listItem: DetailsListItem = DetailsListItem
                .createItem(null, `person${i}`, person.fullName, status, "person", IconLibrary.IonicV5)
                .setId(person.id)
                .withLink()
                .asListViewOnly();

            listItem.ClickEventObservable.subscribe((event: any) => {
                source.personSelected(event["id"]);
            });

            people.push(listItem);
            i++;
        });
        return people;
    }

    public createCustomerDetailsList(
        navProxy: NavProxyService,
        popoverService: SharedPopoverService,
        canViewAdditionalPropertyValues: boolean,
        isMutual: boolean,
        permissionService: PermissionService,
    ): Array<DetailsListItem> {
        let details: Array<DetailsListItem> = [];
        let descriptions: typeof DetailListItemDescriptionMap = DetailListItemDescriptionMap;
        let icons: typeof DetailListItemIconMap = DetailListItemIconMap;
        let personDetailsCard: DetailsListItemCard =
            new DetailsListItemCard(
                DetailsListItemCardType.ContactDetails,
                "Contact Details");
        let nameDescription: string = this.title && this.company ?
            this.title + " at " + this.company :
            this.title ? this.title :
                this.company ? this.company : '';
        details.push(DetailsListItem.createItem(
            personDetailsCard,
            DetailsListItemGroupType.Person,
            this.getAssembledFullName(),
            nameDescription,
            'none'));

        let organisationAction: DetailsListItemActionIcon =
            DetailListItemHelper.createAction(
                () => navProxy.goToOrganisation(this.organisationId));

        let agentLinkAction: DetailsListItemActionIcon =
            DetailListItemHelper.createAction(
                () => navProxy.goToOwner(this.ownerId, this.organisationId));

        let agentOverflowMenuAction: DetailsListItemActionIcon = DetailListItemHelper.createAction(
            () => popoverService.show(
                {
                    component: PopoverAgentMoreSelectionComponent,
                    componentProps: {
                        hasAgent: this.ownerId !== null,
                        customerId: this.id,
                        customerFullName: this.fullName,
                    },
                    showBackdrop: false,
                    mode: 'md',
                    event: event,
                },
                'Agent more action popover',
            ),
            "more",
            IconLibrary.IonicV4,
            null,
            [Permission.ManageCustomers]);
        const ownerLinkActionIsVisible: boolean = this.ownerId !== null;

        let relationshipsCard: DetailsListItemCard =
            new DetailsListItemCard(
                DetailsListItemCardType.Relationships,
                "Relationships");

        details.push(DetailsListItem.createItem(
            relationshipsCard,
            DetailsListItemGroupType.Relationships,
            this.organisationName,
            descriptions.organisation,
            icons.link)
            .roundIcon()
            .withAction(organisationAction)
            .withRelatedEntity(RelatedEntityType.Organisation, this.organisationId, null, null));

        details.push(DetailsListItem.createItem(
            relationshipsCard,
            DetailsListItemGroupType.Relationships,
            this.ownerFullName || "None",
            descriptions.agent,
            icons.link)
            .withActions(
                ownerLinkActionIsVisible ? agentLinkAction : null,
                agentOverflowMenuAction)
            .withRelatedEntity(RelatedEntityType.User, this.organisationId, this.ownerId, null));

        let portalLinkAction: DetailsListItemActionIcon
            = DetailListItemHelper.createAction(() => navProxy.goToPortal(this.portalId),
                "link",
                IconLibrary.IonicV4,
                [Permission.ViewPortals]);
        let portalOverflowMenuAction: DetailsListItemActionIcon =  DetailListItemHelper.createAction(
            () => popoverService.show(
                {
                    component: PopoverAssignPortalComponent,
                    componentProps: {
                        entityType: AssignPortalEntityType.Customer,
                        customerId: this.id,
                        customerName: this.fullName,
                        entityOrganisationId: this.organisationId,
                        entityTenantId: this.tenantId,
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
            [Permission.ManageCustomers]);

        let portalDetailListItem: DetailsListItem = DetailsListItem
            .createItem(
                relationshipsCard,
                DetailsListItemGroupType.Relationships,
                this.portalName || 'None',
                descriptions.portal,
                icons.link)
            .withActions(this.portalId ? portalLinkAction : null, portalOverflowMenuAction)
            .withRelatedEntity(
                RelatedEntityType.Portal,
                this.organisationId,
                this.ownerId,
                this.id);
        details.push(portalDetailListItem);

        let datesCard: DetailsListItemCard =
            new DetailsListItemCard(
                DetailsListItemCardType.Dates,
                "Dates");
        let statusDetailsCard: DetailsListItemCard =
            new DetailsListItemCard(
                DetailsListItemCardType.Others,
                "Status");
        const accountCard: DetailsListItemCard =
            new DetailsListItemCard(DetailsListItemCardType.Account,
                'Account');
        details = PersonDetailsHelper.createItemFromRepeatingFields(
            this.personRepeatingFields,
            details,
            personDetailsCard,
            "phone",
            "call");
        details = PersonDetailsHelper.createItemFromRepeatingFields(
            this.personRepeatingFields,
            details,
            personDetailsCard,
            "email",
            "mail");
        details = PersonDetailsHelper.createItemFromRepeatingFields(
            this.personRepeatingFields,
            details,
            personDetailsCard,
            "address",
            "map");
        details = PersonDetailsHelper.createItemFromRepeatingFields(
            this.personRepeatingFields,
            details,
            personDetailsCard,
            "website",
            "globe");
        details = PersonDetailsHelper.createItemFromRepeatingFields(
            this.personRepeatingFields,
            details,
            personDetailsCard,
            "messenger",
            "chatbox");
        details = PersonDetailsHelper.createItemFromRepeatingFields(
            this.personRepeatingFields,
            details,
            personDetailsCard,
            "social",
            "share-social");

        if (this.userStatus != UserStatus.New) {
            details.push(PersonDetailsHelper.createItem(
                personDetailsCard,
                DetailsListItemGroupType.Account,
                this.email,
                descriptions.accountEmail,
                'lock',
                false,
                null,
                false));
        }

        details.push(DetailsListItem.createItem(
            datesCard,
            DetailsListItemGroupType.Dates,
            this.createdDate || "-",
            descriptions.createdDate,
            icons.calendar));
        details.push(DetailsListItem.createItem(
            datesCard,
            DetailsListItemGroupType.Dates,
            this.createdTime || "-",
            descriptions.createdTime,
            icons.calendar));
        details.push(DetailsListItem.createItem(
            datesCard,
            DetailsListItemGroupType.Dates,
            this.lastModifiedDate || "-",
            descriptions.lastModifiedDate,
            icons.calendar));
        details.push(DetailsListItem.createItem(
            datesCard,
            DetailsListItemGroupType.Dates,
            this.lastModifiedTime || "-",
            descriptions.lastModifiedTime,
            icons.calendar));
        if (canViewAdditionalPropertyValues) {
            AdditionalPropertiesHelper.createDetailsList(details, this.additionalPropertyValues);
        }
        details.push(DetailsListItem.createItem(
            statusDetailsCard,
            DetailsListItemGroupType.Others,
            this.hasActivePolicy || "No",
            isMutual ? descriptions.hasActiveProtection :
                descriptions.hasActivePolicy,
            icons.others));

        if (this.transactions && this.transactions.length > 0) {
            details.push(DetailsListItem.createItem(
                accountCard,
                DetailsListItemGroupType.Account,
                this.accountBalance,
                this.accountBalanceSubtext,
                icons.wallet));
        }

        return details;
    }

    public getAccountEmail(): string {
        if (this.email) {
            return this.email;
        } else if (this.personRepeatingFields.emailAddresses.length > 0) {
            return this.personRepeatingFields.emailAddresses[0].emailAddress;
        } else {
            return undefined;
        }
    }

    private getAssembledFullName(): string {
        return [this.namePrefix,
            this.firstName,
        this.preferredName ? '(' + this.preferredName + ')' : '',
        this.middleNames,
        this.lastName,
        this.nameSuffix].filter((part: string) => !!part).join(" ") || null;
    }
}
