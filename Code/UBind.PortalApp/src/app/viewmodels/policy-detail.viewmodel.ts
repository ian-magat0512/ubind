import { CustomerResourceModel } from '@app/resource-models/customer.resource-model';
import { UserSummaryViewModel } from './user-summary.viewmodel';
import { LocalDateHelper, StringHelper } from '@app/helpers';
import { PolicyDetailResourceModel } from '@app/resource-models/policy.resource-model';
import { PolicyStatus } from '@app/models';
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import { DetailListItemHelper } from '@app/helpers/detail-list-item.helper';
import { DetailsListItemCardType } from '@app/models/details-list/details-list-item-card-type.enum';
import { DetailsListGroupItemModel } from '@app/models/details-list/details-list-item-model';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { DetailsListItemActionIcon } from '@app/models/details-list/details-list-item-action-icon';
import { AdditionalPropertyValue } from '@app/models/additional-property-item-view.model';
import { AdditionalPropertiesHelper } from '@app/helpers/additional-properties.helper';
import { RelatedEntityType } from "@app/models/related-entity-type.enum";
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Export policy detail view model class.
 * This class is used to transform policy detail Resource  Model into policy detail View model.
 * This class used helper class to transform data like date and time into expected format.
 */
export class PolicyDetailViewModel {
    public constructor(policy: PolicyDetailResourceModel) {
        this.id = policy.id;
        this.organisationId = policy.organisationId;
        this.organisationName = policy.organisationName;
        this.customerDetails = policy.customer;
        this.expiryDate = LocalDateHelper.toLocalDate(policy.expiryDateTime);
        this.expiryTime = LocalDateHelper.convertToLocalAndGetTimeOnly(policy.expiryDateTime);
        this.inceptionDate = LocalDateHelper.toLocalDate(policy.inceptionDateTime);
        this.inceptionTime = LocalDateHelper.convertToLocalAndGetTimeOnly(policy.inceptionDateTime);
        this.owner = policy.owner ? new UserSummaryViewModel(policy.owner) : null;
        this.policyNumber = policy.policyNumber;
        this.productId = policy.productId;
        this.productName = policy.productName;
        this.quoteId = policy.quoteId;
        this.quoteNumber = policy.quoteNumber;
        this.quoteOwnerUserId = policy.quoteOwnerUserId;
        this.status = policy.status;
        this.segment = policy.status == PolicyStatus.Active || policy.status == PolicyStatus.Issued
            ? PolicyStatus.Current.toLowerCase()
            : policy.status.toLowerCase();
        this.createdDateTime = policy.createdDateTime;
        this.quoteType = policy.eventTypeSummary;
        this.tenantId = policy.tenantId;
        this.issuedDate = LocalDateHelper.toLocalDate(policy.createdDateTime);
        this.issuedTime = LocalDateHelper.convertToLocalAndGetTimeOnly(policy.createdDateTime);
        this.lastModifiedDate = LocalDateHelper.toLocalDate(policy.lastModifiedDateTime);
        this.lastModifiedTime = LocalDateHelper.convertToLocalAndGetTimeOnly(policy.lastModifiedDateTime);
        this.numberOfDaysToExpire = policy.numberOfDaysToExpire;
        this.futureTransactionDate = LocalDateHelper.toLocalDate(policy.futureTransactionDateTime);
        this.futureTransactionTime = LocalDateHelper.convertToLocalAndGetTimeOnly(policy.futureTransactionDateTime);
        this.hasFutureTransaction = policy.hasFutureTransaction;
        this.futureTransactionType = policy.futureTransactionType;
        this.cancellationEffectiveDate = LocalDateHelper.toLocalDate(policy.cancellationEffectiveDateTime);
        this.cancellationEffectiveTime
            = LocalDateHelper.convertToLocalAndGetTimeOnly(policy.cancellationEffectiveDateTime);
        this.effectiveTime = policy.effectiveDateTime;
        this.formData = policy.formData["formModel"] ? policy.formData["formModel"] : null;
        this.hasClaimConfiguration = policy.hasClaimConfiguration;
        this.additionalPropertyValues = policy.additionalPropertyValues;
        this.isTestData = policy.isTestData;
    }

    public id: string;
    public organisationId: string;
    public organisationName: string;
    public createdDateTime: string;
    public customerDetails: CustomerResourceModel;
    public expiryDate: string;
    public expiryTime: string;
    public inceptionDate: string;
    public inceptionTime: string;
    public effectiveTime: string;
    public cancellationEffectiveDate: string;
    public cancellationEffectiveTime: string;
    public owner: UserSummaryViewModel;
    public policyNumber: string;
    public productId: string;
    public productName: string;
    public questionData: any;
    public quoteId: string;
    public quoteNumber: string;
    public quoteOwnerUserId: string;
    public status: string;
    public segment: string;
    public tenantId: string;
    public numberOfDaysToExpire: number;
    public hasFutureTransaction: boolean;
    public futureTransactionDate: string;
    public futureTransactionTime: string;
    public futureTransactionType: string;
    public formData: string;
    public issuedDate: string;
    public issuedTime: string;
    public lastModifiedDate: string;
    public quoteType: string;
    public lastModifiedTime: string;
    public hasClaimConfiguration: boolean;
    public additionalPropertyValues: Array<AdditionalPropertyValue>;
    public isTestData: boolean;

    public static doesShowMoreButton(
        detail: PolicyDetailViewModel,
        segment: string,
        productFeatureEnabled: boolean,
        isRenewalAllowedAfterExpiry: boolean,
    ): boolean {
        let stringHelper: StringHelper = new StringHelper();
        return detail
            ? ((!stringHelper.equalsIgnoreCase(segment, 'claims') &&
                !stringHelper.equalsIgnoreCase(segment, 'documents'))
                && detail.status != PolicyStatus.Cancelled
                && (detail.status != PolicyStatus.Expired || isRenewalAllowedAfterExpiry))
            && !detail.hasFutureTransaction && productFeatureEnabled
            : false;
    }

    public createDetailsList(
        navProxy: NavProxyService,
        isCustomer: boolean,
        isMutual: boolean = false,
    ): Array<DetailsListItem> {
        let details: Array<DetailsListItem> = [];
        let customerAction: DetailsListItemActionIcon = null;
        if (this.customerDetails) {
            customerAction = DetailListItemHelper.createAction(
                () => navProxy.goToCustomer(this.customerDetails.id));
        }
        let quoteAction: DetailsListItemActionIcon =
            DetailListItemHelper.createAction(
                () => navProxy.goToQuote(this.quoteId));
        let organisationAction: DetailsListItemActionIcon =
            DetailListItemHelper.createAction(
                () => navProxy.goToOrganisation(this.organisationId));
        let agentAction: DetailsListItemActionIcon =
            DetailListItemHelper.createAction(
                () => navProxy.goToOwner(this.owner ? this.owner.id : null, this.organisationId));
        let icons: any = DetailListItemHelper.detailListItemIconMap;

        let detailModel: Array<DetailsListGroupItemModel> = [
            DetailsListGroupItemModel.create("product", this.productName),
            DetailsListGroupItemModel.create("status", this.status),
            DetailsListGroupItemModel.create(
                isMutual ? "protectionNumber" : "policyNumber",
                this.policyNumber),
        ];

        details = details.concat(DetailListItemHelper.createDetailItemGroup(
            DetailsListItemCardType.Details,
            detailModel,
            icons.shield,
            IconLibrary.AngularMaterial));

        let relationships: Array<DetailsListGroupItemModel> = new Array<DetailsListGroupItemModel>();
        if (customerAction && this.customerDetails) {
            relationships.push(DetailsListGroupItemModel.create(
                "customer",
                !isCustomer ? this.customerDetails.displayName : null,
                null,
                null,
                customerAction)
                .withRelatedEntity(
                    RelatedEntityType.Customer,
                    this.organisationId,
                    this.customerDetails.ownerUserId,
                    this.customerDetails.id));
        }
        if (this.quoteNumber) {
            relationships.push(DetailsListGroupItemModel.create(
                "quote",
                this.quoteNumber,
                null,
                null,
                quoteAction)
                .withRelatedEntity(
                    RelatedEntityType.Quote,
                    this.organisationId,
                    this.quoteOwnerUserId,
                    this.customerDetails ? this.customerDetails.id : null));
        }
        relationships.push(DetailsListGroupItemModel.create(
            "organisation",
            this.organisationName,
            null,
            null,
            organisationAction)
            .withRelatedEntity(RelatedEntityType.Organisation, this.organisationId, null, null));

        relationships.push(DetailsListGroupItemModel.create(
            "agent",
            this.owner ? this.owner.fullName : 'None',
            null,
            null,
            this.owner ? agentAction : null)
            .withRelatedEntity(RelatedEntityType.User, this.organisationId, this.owner ? this.owner.id : null, null));

        details = details.concat(DetailListItemHelper.createDetailItemGroup(
            DetailsListItemCardType.Relationships,
            relationships,
        ));

        let dates: Array<DetailsListGroupItemModel> = [];

        dates = [
            DetailsListGroupItemModel.create("InceptionDate", this.inceptionDate),
            DetailsListGroupItemModel.create("InceptionTime", this.inceptionTime),
            this.status === PolicyStatus.Cancelled
                ? DetailsListGroupItemModel.create("cancellationEffectiveDate", this.cancellationEffectiveDate)
                : DetailsListGroupItemModel.create("ExpiryDate", this.expiryDate),
            this.status === PolicyStatus.Cancelled
                ? DetailsListGroupItemModel.create("cancellationEffectiveTime", this.cancellationEffectiveTime)
                : DetailsListGroupItemModel.create("ExpiryTime", this.expiryTime),
        ];

        if (this.hasFutureTransaction && this.futureTransactionType == 'RenewalTransaction') {
            dates.push(DetailsListGroupItemModel.create("renewalEffectiveDate", this.futureTransactionDate));
            dates.push(DetailsListGroupItemModel.create("renewalEffectiveTime", this.futureTransactionTime));
        }

        dates.push(DetailsListGroupItemModel.create("issuedDate", this.issuedDate));
        dates.push(DetailsListGroupItemModel.create("issuedTime", this.issuedTime));
        dates.push(DetailsListGroupItemModel.create("lastModifiedDate", this.lastModifiedDate));
        dates.push(DetailsListGroupItemModel.create("lastModifiedTime", this.lastModifiedTime));
        details = details.concat(
            DetailListItemHelper.createDetailItemGroup(
                DetailsListItemCardType.Dates,
                dates),
        );

        AdditionalPropertiesHelper.createDetailsList(details, this.additionalPropertyValues);

        if (this.isTestData) {
            let isTestDataItem: Array<DetailsListGroupItemModel> =
                [DetailsListGroupItemModel.create(
                    "testData",
                    this.isTestData ? "Yes" : "No")];
            details = details.concat(DetailListItemHelper.createDetailItemGroup(
                DetailsListItemCardType.Status,
                isTestDataItem,
                icons.folder));
        }

        return details;
    }
}
