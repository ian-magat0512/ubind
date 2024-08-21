import { LocalDateHelper } from '@app/helpers';
import { DetailsListItem } from "@app/models/details-list/details-list-item";
import { DetailListItemHelper } from "@app/helpers/detail-list-item.helper";
import { DetailsListItemCardType } from "@app/models/details-list/details-list-item-card-type.enum";
import { EmailResourceModel } from "@app/resource-models/email.resource-model";
import { DetailsListGroupItemModel } from '@app/models/details-list/details-list-item-model';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { DetailsListItemActionIcon } from '@app/models/details-list/details-list-item-action-icon';
import { RelatedEntityType } from "@app/models/related-entity-type.enum";
import { Tag } from '@app/resource-models/message.resource-model';

/**
 * Export email detail view model class.
 * TODO: Write a better class header: view model of email detail.
 */
export class EmailDetailViewModel {

    public constructor(model: EmailResourceModel) {
        this.bbc = model.bbc;
        this.cc = model.cc;
        this.createdDate = LocalDateHelper.toLocalDate(model.createdDateTime);
        this.createdTime = LocalDateHelper.convertToLocalAndGetTimeOnly(model.createdDateTime);
        this.customer = model.customer;
        this.user = model.user;
        this.documents = model.documents;
        this.from = model.from;
        this.hasAttachment = model.hasAttachment;
        this.htmlMessage = model.htmlMessage;
        this.id = model.id;
        this.localTime = this.createdTime;
        this.plainMessage = model.plainMessage;
        this.policy = model.policy;
        this.recipient = model.recipient;
        this.subject = model.subject;
        this.tags = model.tags;
        this.quote = model.quote;
        this.claim = model.claim;
        this.policyTransaction = model.policyTransaction;
        this.organisation = model.organisation;
    }

    public bbc: Array<string>;
    public cc: Array<string>;
    public createdDate: string;
    public createdTime: string;
    public customer: any;
    public user: any;
    public documents: any;
    public from: string;
    public hasAttachment: boolean;
    public htmlMessage: string;
    public id: string;
    public localTime: string;
    public plainMessage: string;
    public policy: any;
    public recipient: string;
    public subject: string;
    public tags: Array<Tag>;
    public quote: any;
    public claim: any;
    public policyTransaction: any;
    public organisation: any;

    public createDetailsList(
        navProxy: NavProxyService,
        isCustomer: boolean,
        isMutual: boolean = false,
    ): Array<DetailsListItem> {
        let details: Array<DetailsListItem> = [];
        let customerAction: DetailsListItemActionIcon = DetailListItemHelper.createAction(
            () => navProxy.goToCustomer(this.customer ? this.customer.id : null));
        let userAction: DetailsListItemActionIcon = DetailListItemHelper.createAction(
            () => navProxy.goToUser(this.user ? this.user.id : null));
        let quoteAction: DetailsListItemActionIcon = DetailListItemHelper.createAction(
            () => navProxy.goToQuote(this.quote ? this.quote.id : null));
        let claimAction: DetailsListItemActionIcon = DetailListItemHelper.createAction(
            () => navProxy.goToClaim(this.claim ? this.claim.id : null));
        let policyAction: DetailsListItemActionIcon = DetailListItemHelper.createAction(
            () => navProxy.goToPolicy(this.policy ? this.policy.id : null));
        let organisationAction: DetailsListItemActionIcon = DetailListItemHelper.createAction(
            () => navProxy.goToOrganisation(this.organisation ? this.organisation.id : null));

        let icons: typeof DetailListItemHelper.detailListItemIconMap =
            DetailListItemHelper.detailListItemIconMap;

        let policyTransactionAction: DetailsListItemActionIcon = DetailListItemHelper.createAction(
            () => navProxy.goToPolicyTransaction(this.policyTransaction
                ? { policyId: this.policyTransaction.policyId, policyTransactionId: this.policyTransaction.id }
                : null));

        let detailModel: Array<DetailsListGroupItemModel> = [
            DetailsListGroupItemModel.create("to", this.recipient),
            DetailsListGroupItemModel.create("from", this.from),
            DetailsListGroupItemModel.create("cc", this.cc ? this.cc.join(' , ') : null),
            DetailsListGroupItemModel.create("bcc", this.bbc ? this.bbc.join(' , ') : null),
            DetailsListGroupItemModel.create("subject", this.subject),
        ];

        details = details.concat(DetailListItemHelper.createDetailItemGroup(
            DetailsListItemCardType.Details,
            detailModel,
            icons.mail,
        ));

        let relationships: Array<DetailsListGroupItemModel> = [
            DetailsListGroupItemModel.create(
                "customer",
                (this.customer && !isCustomer) ? this.customer.fullName :
                    null,
                null,
                null,
                customerAction,
            )
                .withRelatedEntity(
                    RelatedEntityType.Customer,
                    this.customer ? this.customer.organisationId : null,
                    this.customer ? this.customer.ownerUserId : null,
                    this.customer ? this.customer.id : null),
            DetailsListGroupItemModel.create(
                "quote",
                this.quote ? this.quote.quoteNumber : null,
                null,
                null,
                quoteAction,
            )
                .withRelatedEntity(
                    RelatedEntityType.Quote,
                    this.quote ? this.quote.organisationId : null,
                    this.quote ? this.quote.ownerUserId : null,
                    this.quote ? this.quote.customerId : null),
            DetailsListGroupItemModel.create(
                "claim",
                this.claim ? this.claim.claimNumber : null,
                null,
                null,
                claimAction,
            )
                .withRelatedEntity(
                    RelatedEntityType.Claim,
                    this.claim ? this.claim.organisationId : null,
                    this.claim ? this.claim.ownerUserId : null,
                    this.claim ? this.claim.customerId : null),
            DetailsListGroupItemModel.create(
                "transaction",
                this.policyTransaction ? this.policyTransaction.transactionType : null,
                null,
                null,
                policyTransactionAction,
            )
                .withRelatedEntity(
                    RelatedEntityType.Policy,
                    this.policyTransaction ? this.policyTransaction.organisationId : null,
                    this.policyTransaction ? this.policyTransaction.ownerUserId : null,
                    this.policyTransaction ? this.policyTransaction.customerId : null),
            DetailsListGroupItemModel.create(
                isMutual ? "protection" : "policy",
                this.policy ? this.policy.policyNumber : null,
                null,
                null,
                policyAction,
            )
                .withRelatedEntity(
                    RelatedEntityType.Policy,
                    this.policy ? this.policy.organisationId : null,
                    this.policy ? this.policy.ownerUserId : null,
                    this.policy ? this.policy.customerId : null),
            DetailsListGroupItemModel.create(
                "user",
                (this.user && !isCustomer) ? this.user.fullName :
                    null,
                null,
                null,
                userAction,
            )
                .withRelatedEntity(
                    RelatedEntityType.User,
                    this.user ? this.user.organisationId : null,
                    this.user ? this.user.id : null,
                    this.user ? this.user.customerId : null),
            DetailsListGroupItemModel.create(
                "organisation",
                this.organisation ? this.organisation.name : null,
                null,
                null,
                organisationAction,
            )
                .withRelatedEntity(
                    RelatedEntityType.Organisation,
                    this.organisation ? this.organisation.id : null,
                    null,
                    null,
                ),
        ];

        details = details.concat(DetailListItemHelper.createDetailItemGroup(
            DetailsListItemCardType.Relationships,
            relationships));

        let dates: Array<DetailsListGroupItemModel> = [
            DetailsListGroupItemModel.create("sentDate", this.createdDate),
            DetailsListGroupItemModel.create("sentTime", this.createdTime),
        ];

        details = details.concat(DetailListItemHelper.createDetailItemGroup(
            DetailsListItemCardType.Dates,
            dates));

        return details;
    }
}
