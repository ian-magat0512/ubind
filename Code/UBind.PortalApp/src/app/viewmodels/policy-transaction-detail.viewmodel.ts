import { PolicyTransactionDetailResourceModel } from "../resource-models/policy.resource-model";
import { CustomerResourceModel } from "../resource-models/customer.resource-model";
import { DisplayableFieldsModel } from "@app/models/displayable-field";
import { PremiumResult } from "@app/models/premium-result";
import { UserSummaryViewModel } from "./user-summary.viewmodel";
import { LocalDateHelper, Permission, PolicyHelper } from '@app/helpers';
import { DetailsListItem } from "@app/models/details-list/details-list-item";
import { DetailListItemHelper } from "@app/helpers/detail-list-item.helper";
import { DetailsListItemCardType } from "@app/models/details-list/details-list-item-card-type.enum";
import { DetailsListGroupItemModel } from "@app/models/details-list/details-list-item-model";
import { NavProxyService } from "@app/services/nav-proxy.service";
import { DetailsListItemActionIcon } from "@app/models/details-list/details-list-item-action-icon";
import { AdditionalPropertyValue } from "@app/models/additional-property-item-view.model";
import { AdditionalPropertiesHelper } from "@app/helpers/additional-properties.helper";
import { PolicyTransactionEventNamePastTense } from "@app/models/policy-transaction-event-name-past-tense.enum";
import { RelatedEntityType } from "@app/models/related-entity-type.enum";
import { Document } from 'app/models/document';
import { IconLibrary } from "@app/models/icon-library.enum";

/**
 * Export policy or transaction detail view model class.
 * TODO: Write a better class header: view model of policy or transaction detail.
 */
export class PolicyTransactionDetailViewModel {
    public policyId: string;
    public organisationId: string;
    public organisationName: string;
    public policyNumber: string;
    public policyOwnerUserId: string;
    public productAlias: string;
    public productName: string;
    public createdDate: string;
    public createdTime: string;
    public lastModifiedDate: string;
    public lastModifiedTime: string;
    public status: string;
    public quoteId: string;
    public quoteOwnerUserId: string;
    public eventTypeSummary: PolicyTransactionEventNamePastTense;
    public documents: Array<Document>;
    public customer: CustomerResourceModel;
    public owner: UserSummaryViewModel;
    public quoteNumber: string;
    public inceptionTime: string;
    public inceptionDate: string;
    public expiryDate: string;
    public expiryTime: string;
    public questions: string;
    public displayableFields: DisplayableFieldsModel;
    public premium: PremiumResult;
    public updateStatus: string;
    public isPurchase: boolean;
    public isAdjustment: boolean;
    public isRenewal: boolean;
    public isCancellation: boolean;
    public effectiveTimestamp: string;
    public effectiveDate: string;
    public effectiveTime: string;
    public cancellationEffectiveDate: string;
    public cancellationEffectiveTime: string;
    public additionalPropertyValues: Array<AdditionalPropertyValue>;
    public policyIssuedDate: string;
    public policyIssuedTime: string;
    public policyAdjustedDate: string;
    public policyAdjustedTime: string;
    public policyRenewedDate: string;
    public policyRenewedTime: string;
    public policyCancellationDate: string;
    public policyCancellationTime: string;
    public productReleaseId: string;
    public productReleaseNumber: string;

    public constructor(resourceModel: PolicyTransactionDetailResourceModel) {
        this.policyNumber = resourceModel.policyNumber;
        this.policyOwnerUserId = resourceModel.policyOwnerUserId;
        this.productAlias = resourceModel.productAlias;
        this.productName = resourceModel.productName;
        this.quoteNumber = resourceModel.quoteReference;
        this.status = resourceModel.transactionStatus;
        this.quoteId = resourceModel.quoteId;
        this.quoteOwnerUserId = resourceModel.quoteOwnerUserId;
        this.customer = resourceModel.customer;
        this.owner = resourceModel.owner ? new UserSummaryViewModel(resourceModel.owner) : null;
        this.isPurchase = resourceModel.eventTypeSummary === PolicyTransactionEventNamePastTense.Purchased;
        this.isAdjustment = resourceModel.eventTypeSummary === PolicyTransactionEventNamePastTense.Adjusted;
        this.isRenewal = resourceModel.eventTypeSummary === PolicyTransactionEventNamePastTense.Renewed;
        this.isCancellation = resourceModel.eventTypeSummary === PolicyTransactionEventNamePastTense.Cancelled;
        this.effectiveTimestamp = resourceModel.effectiveDateTime;
        this.effectiveDate = LocalDateHelper.toLocalDate(resourceModel.effectiveDateTime);
        this.effectiveTime = LocalDateHelper.convertToLocalAndGetTimeOnly(resourceModel.effectiveDateTime);
        if (resourceModel.cancellationEffectiveDateTime) {
            this.cancellationEffectiveDate = LocalDateHelper.toLocalDate(resourceModel.cancellationEffectiveDateTime);
            this.cancellationEffectiveTime
                = LocalDateHelper.convertToLocalAndGetTimeOnly(resourceModel.cancellationEffectiveDateTime);
        }
        this.updateStatus = this.getUpdatedStatus(resourceModel);
        if (resourceModel.expiryDateTime) {
            this.expiryDate = LocalDateHelper.toLocalDate(resourceModel.expiryDateTime);
            this.expiryTime = LocalDateHelper.convertToLocalAndGetTimeOnly(resourceModel.expiryDateTime);
        }
        this.createdDate = LocalDateHelper.toLocalDate(resourceModel.createdDateTime);
        this.createdTime = LocalDateHelper.convertToLocalAndGetTimeOnly(resourceModel.createdDateTime);
        this.lastModifiedDate = LocalDateHelper.toLocalDate(resourceModel.lastModifiedDateTime);
        this.lastModifiedTime = LocalDateHelper.convertToLocalAndGetTimeOnly(resourceModel.lastModifiedDateTime);
        this.eventTypeSummary = resourceModel.eventTypeSummary;
        this.policyId = resourceModel.policyId;
        this.organisationId = resourceModel.organisationId;
        this.organisationName = resourceModel.organisationName;
        this.additionalPropertyValues = resourceModel.additionalPropertyValues;
        this.policyIssuedDate = LocalDateHelper.toLocalDate(resourceModel.createdDateTime);
        this.policyIssuedTime = LocalDateHelper.convertToLocalAndGetTimeOnly(resourceModel.createdDateTime);
        this.policyAdjustedDate = this.policyIssuedDate;
        this.policyAdjustedTime = this.policyIssuedTime;
        this.policyRenewedDate = this.policyIssuedDate;
        this.policyRenewedTime = this.policyIssuedTime;
        this.policyCancellationDate = this.policyIssuedDate;
        this.policyCancellationTime = this.policyIssuedTime;
        this.productReleaseId = resourceModel.productReleaseId;
        this.productReleaseNumber = resourceModel.productReleaseNumber;
    }

    private getUpdatedStatus(resourceModel: PolicyTransactionDetailResourceModel): string {
        let now: number = Date.now();
        let statusDescription: string = PolicyHelper.constants.Labels.Status.Completed;
        let effectiveTimestamp: number = Date.parse(resourceModel.effectiveDateTime);
        let cancellationEffectiveTimestamp: number = resourceModel.cancellationEffectiveDateTime
            ? Date.parse(resourceModel.cancellationEffectiveDateTime)
            : null;

        if (effectiveTimestamp > now) {
            if (cancellationEffectiveTimestamp && cancellationEffectiveTimestamp < effectiveTimestamp) {
                statusDescription = PolicyHelper.constants.Labels.Status.Cancelled;
            } else {
                statusDescription = PolicyHelper.constants.Labels.Status.Pending;
            }
        }
        return statusDescription;
    }

    public createDetailsList(
        navProxy: NavProxyService,
        isCustomer: boolean,
        isMutual: boolean = false,
    ): Array<DetailsListItem> {
        let details: Array<DetailsListItem> = [];
        let customerAction: DetailsListItemActionIcon = null;
        if (this.customer) {
            customerAction = DetailListItemHelper.createAction(
                () => navProxy.goToCustomer(this.customer.id));
        }

        let quoteAction: DetailsListItemActionIcon = DetailListItemHelper.createAction(
            () => navProxy.goToQuote(this.quoteId));
        let policyAction: DetailsListItemActionIcon = DetailListItemHelper.createAction(
            () => navProxy.goToPolicy(this.policyId));
        let organisationAction: DetailsListItemActionIcon = DetailListItemHelper.createAction(
            () => navProxy.goToOrganisation(this.organisationId));
        let agentAction: DetailsListItemActionIcon = null;
        if (this.owner) {
            agentAction = DetailListItemHelper.createAction(
                () => navProxy.goToOwner(this.owner.id));
        }

        let productReleaseAction: DetailsListItemActionIcon = null;
        if (!isCustomer && this.productReleaseNumber) {
            productReleaseAction =
                DetailListItemHelper.createAction(
                    () => navProxy.goToProductRelease(this.productAlias, this.productReleaseId),
                    'link',
                    IconLibrary.IonicV4,
                    null,
                    [Permission.ViewReleases, Permission.ManageReleases]);
        }

        let icons: any = DetailListItemHelper.detailListItemIconMap;

        let detailModel: Array<DetailsListGroupItemModel> = [];

        if (isMutual) {
            detailModel = [
                DetailsListGroupItemModel.create("protectionNumber", this.policyNumber),
            ];
        } else {
            detailModel = [
                DetailsListGroupItemModel.create("policyNumber", this.policyNumber),
            ];
        }

        if (!isCustomer && this.productReleaseNumber) {
            detailModel.push(DetailsListGroupItemModel.create("productRelease",
                this.productReleaseNumber,
                null,
                null,
                productReleaseAction)
                .withRelatedEntity(RelatedEntityType.Release,
                    this.organisationId,
                    null,
                    null));
        }

        let dates: Array<DetailsListGroupItemModel> = [];
        if (this.isAdjustment || this.isCancellation || this.isRenewal) {
            dates.push(DetailsListGroupItemModel.create("processedDate", this.createdDate));
            dates.push(DetailsListGroupItemModel.create("processedTime", this.createdTime));
        } else if (this.isPurchase) {
            dates.push(DetailsListGroupItemModel.create("purchaseDate", this.createdDate));
            dates.push(DetailsListGroupItemModel.create("purchaseTime", this.createdTime));

            if (this.inceptionDate) {
                dates.push(DetailsListGroupItemModel.create("InceptionDate", this.inceptionDate));
                dates.push(DetailsListGroupItemModel.create("InceptionTime", this.inceptionTime));
            }
        }

        if (this.isRenewal) {
            dates.push(DetailsListGroupItemModel.create("renewalEffectiveDate", this.effectiveDate));
            dates.push(DetailsListGroupItemModel.create("renewalEffectiveTime", this.effectiveTime));
            detailModel.push(DetailsListGroupItemModel.create("renewalStatus", this.updateStatus));
        } else if (this.isAdjustment) {
            dates.push(DetailsListGroupItemModel.create("adjustmentEffectiveDate", this.effectiveDate));
            dates.push(DetailsListGroupItemModel.create("adjustmentEffectiveTime", this.effectiveTime));
            detailModel.push(DetailsListGroupItemModel.create("adjustmentStatus", this.updateStatus));
        } else if (this.isCancellation) {
            dates.push(DetailsListGroupItemModel.create("cancellationEffectiveDate", this.effectiveDate));
            dates.push(DetailsListGroupItemModel.create("cancellationEffectiveTime", this.effectiveTime));
            detailModel.push(DetailsListGroupItemModel.create("cancellationStatus", this.updateStatus));
        } else {
            dates.push(DetailsListGroupItemModel.create("effectiveDate", this.effectiveDate));
            dates.push(DetailsListGroupItemModel.create("effectiveTime", this.effectiveTime));
        }

        if (!this.isCancellation) {
            dates.push(DetailsListGroupItemModel.create("expiryDate", this.expiryDate));
            dates.push(DetailsListGroupItemModel.create("expiryTime", this.expiryTime));
        }

        dates.push(DetailsListGroupItemModel.create("lastModifiedDate", this.lastModifiedDate));
        dates.push(DetailsListGroupItemModel.create("lastModifiedTime", this.lastModifiedTime));

        details = details.concat(DetailListItemHelper.createDetailItemGroup(
            DetailsListItemCardType.Details,
            detailModel,
            icons.shield,
            IconLibrary.AngularMaterial,
        ));

        let relationships: Array<DetailsListGroupItemModel> = new Array<DetailsListGroupItemModel>();
        if (customerAction) {
            relationships.push(DetailsListGroupItemModel.create(
                "customer",
                !isCustomer ? this.customer.displayName : null,
                null,
                null,
                customerAction,
            ).withRelatedEntity(
                RelatedEntityType.Customer,
                this.organisationId,
                this.customer.ownerUserId,
                this.customer.id,
            ));
        }

        if (this.quoteNumber) {
            relationships.push(DetailsListGroupItemModel.create(
                "quote",
                this.quoteNumber,
                null,
                null,
                quoteAction,
            ).withRelatedEntity(
                RelatedEntityType.Quote,
                this.organisationId,
                this.quoteOwnerUserId,
                this.customer?.id,
            ));
        }
        relationships.push(DetailsListGroupItemModel.create(
            isMutual ? "protection" : "policy",
            this.policyNumber,
            null,
            null,
            policyAction,
        ).withRelatedEntity(
            RelatedEntityType.Policy,
            this.organisationId,
            this.policyOwnerUserId,
            this.customer?.id,
        ));
        relationships.push(DetailsListGroupItemModel.create(
            "organisation",
            this.organisationName,
            null,
            null,
            organisationAction,
        ).withRelatedEntity(
            RelatedEntityType.Organisation,
            this.organisationId,
            null,
            null,
        ));
        if (agentAction) {
            relationships.push(DetailsListGroupItemModel.create(
                "agent",
                this.owner.fullName,
                null,
                null,
                agentAction,
            ).withRelatedEntity(
                RelatedEntityType.User,
                this.organisationId,
                this.owner.id,
                null,
            ));
        }

        details = details.concat(DetailListItemHelper.createDetailItemGroup(
            DetailsListItemCardType.Relationships,
            relationships,
        ));

        details = details.concat(DetailListItemHelper.createDetailItemGroup(
            DetailsListItemCardType.Dates,
            dates,
        ));

        AdditionalPropertiesHelper.createDetailsList(details, this.additionalPropertyValues);
        return details;
    }
}
