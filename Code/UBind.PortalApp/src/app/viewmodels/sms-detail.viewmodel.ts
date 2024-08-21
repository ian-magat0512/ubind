import { SmsResourceModel } from "@app/resource-models/sms.resource-model";
import { LocalDateHelper } from "@app/helpers";
import { DetailListItemHelper } from "@app/helpers/detail-list-item.helper";
import { DetailsListItem } from "@app/models/details-list/details-list-item";
import { DetailsListItemActionIcon } from "@app/models/details-list/details-list-item-action-icon";
import { DetailsListItemCardType } from "@app/models/details-list/details-list-item-card-type.enum";
import { DetailsListGroupItemModel } from "@app/models/details-list/details-list-item-model";
import { RelatedEntityType } from "@app/models/related-entity-type.enum";
import { NavProxyService } from "@app/services/nav-proxy.service";

/** 
 * View model class for sms detail.
 */
export class SmsDetailViewModel {

    public constructor(sms: SmsResourceModel) {
        this.from = sms.from;
        this.to = sms.to;
        this.message = sms.message;
        this.createdDate = LocalDateHelper.toLocalDate(sms.createdDateTime);
        this.createdTime = LocalDateHelper.convertToLocalAndGetTimeOnly(sms.createdDateTime);

        this.customer = sms.customer;
        this.quote = sms.quote;
        this.claim = sms.claim;
        this.policy = sms.policy;
        this.policyTransaction = sms.policyTransaction;
        this.user = sms.user;
        this.organisation = sms.organisation;
    }

    public from: string;
    public to: string;
    public message: string;
    public createdDate: string;
    public createdTime: string;

    public customer: any;
    public quote: any;
    public claim: any;
    public policy: any;
    public policyTransaction: any;
    public user: any;
    public organisation: any;

    public createDetailList(
        navProxy: NavProxyService,
        isCustomer: boolean,
        isMutual: boolean = false,
    ): Array<DetailsListItem> {

        let details: Array<DetailsListItem> = [];
        let icons: any = DetailListItemHelper.detailListItemIconMap;

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
        let policyTransactionAction: DetailsListItemActionIcon = DetailListItemHelper.createAction(
            () => navProxy.goToPolicyTransaction(this.policyTransaction
                ? { policyId: this.policyTransaction.policyId, policyTransactionId: this.policyTransaction.id }
                : null));
        let organisationAction: DetailsListItemActionIcon = DetailListItemHelper.createAction(
            () => navProxy.goToOrganisation(this.organisation ? this.organisation.id : null));

        let detailModel: Array<DetailsListGroupItemModel> = [
            DetailsListGroupItemModel.create("to", this.to),
            DetailsListGroupItemModel.create("from", this.from),
            DetailsListGroupItemModel.create("content", this.message, null, null, null, 2, null, 0),
        ];

        details = details.concat(DetailListItemHelper.createDetailItemGroup(
            DetailsListItemCardType.Details,
            detailModel,
            icons.sms,
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
