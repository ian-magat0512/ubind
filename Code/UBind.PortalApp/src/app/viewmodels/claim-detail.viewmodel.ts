import { ClaimResourceModel } from '@app/resource-models/claim.resource-model';
import { LocalDateHelper, StringHelper } from '@app/helpers';
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import { DetailListItemHelper } from '@app/helpers/detail-list-item.helper';
import { DetailsListItemCardType } from '@app/models/details-list/details-list-item-card-type.enum';
import { DetailsListGroupItemModel } from '@app/models/details-list/details-list-item-model';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { AdditionalPropertyValue } from '@app/models/additional-property-item-view.model';
import { AdditionalPropertiesHelper } from '@app/helpers/additional-properties.helper';
import { DetailsListItemActionIcon } from '@app/models/details-list/details-list-item-action-icon';
import { RelatedEntityType } from '@app/models/related-entity-type.enum';
import { Document } from 'app/models/document';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Export claim detail view model class.
 * This class manage claim details view model.
 */
export class ClaimDetailViewModel {

    /**
     * Creates a new instance of a claim detail model
     */
    public constructor(claim: ClaimResourceModel) {
        this.id = claim.id;
        this.organisationId = claim.organisationId;
        this.organisationName = claim.organisationName;
        this.productName = claim.productName;
        this.productId = claim.productId;
        this.policyId = claim.policyId;
        this.policyNumber = claim.policyNumber;
        this.policyOwnerUserId = claim.policyOwnerUserId;
        this.status = claim.status;
        this.createdDate = LocalDateHelper.toLocalDate(claim.createdDateTime);
        this.createdTime = LocalDateHelper.convertToLocalAndGetTimeOnly(claim.createdDateTime);
        this.lastModifiedDate = LocalDateHelper.toLocalDate(claim.lastModifiedDateTime);
        this.lastModifiedTime = LocalDateHelper.convertToLocalAndGetTimeOnly(claim.lastModifiedDateTime);
        this.customerId = claim.customerDetails ? claim.customerDetails.id : null;
        this.customerName = claim.customerDetails ? claim.customerDetails.displayName : null;
        this.customerOwnerUserId = claim.customerDetails ? claim.customerDetails.ownerUserId : null;
        this.claimReference = claim.claimReference;
        this.claimNumber = claim.claimNumber;
        if (claim.incidentDateTime) {
            this.incidentDate = LocalDateHelper.toLocalDate(claim.incidentDateTime);
        }
        this.amount = claim.amount;
        this.description = claim.description;
        this.ownerName = claim.ownerName;
        this.ownerUserId = claim.ownerUserId;
        this.formData = claim.formData.formModel ? claim.formData.formModel : {};
        this.questionAttachmentKeys = claim.questionAttachmentKeys;
        this.questionData = claim.calculationResult;
        this.additionalPropertyValues = claim.additionalPropertyValues;
        this.documents = claim.documents;
        this.isTestData = claim.isTestData;
    }

    public organisationId: string;
    public organisationName: string;
    public customerId: string;
    public customerName: string;
    public customerOwnerUserId: string;
    public productId: string;
    public productName: string;
    public id: string;
    public claimNumber: string;
    public claimReference: string;
    public policyId: string;
    public policyNumber: string;
    public policyOwnerUserId: string;
    public status: string;
    public createdDate: string;
    public createdTime: string;
    public incidentDate: string;
    public description: string;
    public amount: string;
    public lastModifiedDate: string;
    public isTestData: boolean;
    public lastModifiedTime: string;
    public questionData: any;
    public ownerName: string;
    public ownerUserId: string;
    public formData: any;
    public questionAttachmentKeys: Array<string>;
    public documents: Array<Document>;
    public additionalPropertyValues: Array<AdditionalPropertyValue>;

    public createDetailsList(
        navProxy: NavProxyService,
        isCustomer: boolean,
        isMutual: boolean,
        tenantAlias: string = "",
    ): Array<DetailsListItem> {
        let details: Array<DetailsListItem> = [];
        let icons: any = DetailListItemHelper.detailListItemIconMap;
        let customerAction: DetailsListItemActionIcon = DetailListItemHelper.createAction(
            () => navProxy.goToCustomer(this.customerId));
        let policyAction: DetailsListItemActionIcon = DetailListItemHelper.createAction(
            () => navProxy.goToPolicy(this.policyId));
        let organisationAction: DetailsListItemActionIcon =
            DetailListItemHelper.createAction(
                () => navProxy.goToOrganisation(this.organisationId));
        let agentAction: DetailsListItemActionIcon = DetailListItemHelper.createAction(
            () => navProxy.goToOwner(this.ownerUserId, this.organisationId));

        let detailModel: Array<DetailsListGroupItemModel> = [
            DetailsListGroupItemModel.create("product", this.productName),
            DetailsListGroupItemModel.create("status", StringHelper.capitalizeFirstLetter(this.status)),
            DetailsListGroupItemModel.create("claimReference", this.claimReference),
            DetailsListGroupItemModel.create("claimNumber", this.claimNumber),
            DetailsListGroupItemModel.create("claimAmount", this.amount),
            DetailsListGroupItemModel.create("description", this.description),
        ];

        details = details.concat(DetailListItemHelper.createDetailItemGroup(
            DetailsListItemCardType.Details,
            detailModel,
            icons.clipboard,
            IconLibrary.AngularMaterial));

        let relationships: Array<DetailsListGroupItemModel> = [
            DetailsListGroupItemModel.create(
                "customer",
                !isCustomer ? this.customerName : null,
                null,
                null,
                customerAction)
                .withRelatedEntity(
                    RelatedEntityType.Customer,
                    this.organisationId,
                    this.customerOwnerUserId,
                    this.customerId),
            DetailsListGroupItemModel.create(
                isMutual ? "protection" : "policy",
                this.policyNumber,
                null,
                null,
                policyAction)
                .withRelatedEntity(
                    RelatedEntityType.Policy,
                    this.organisationId,
                    this.policyOwnerUserId,
                    this.customerId),
            DetailsListGroupItemModel.create("organisation",
                this.organisationName,
                null,
                null,
                organisationAction)
                .withRelatedEntity(
                    RelatedEntityType.Organisation,
                    this.organisationId,
                    null,
                    null),
            DetailsListGroupItemModel.create("agent",
                this.ownerName || 'None',
                null,
                null,
                this.ownerUserId ? agentAction : null)
                .withRelatedEntity(
                    RelatedEntityType.User,
                    this.organisationId,
                    this.ownerUserId,
                    this.customerId),
        ];

        details = details.concat(DetailListItemHelper.createDetailItemGroup(
            DetailsListItemCardType.Relationships,
            relationships,
        ));

        let dates: Array<DetailsListGroupItemModel> = [
            DetailsListGroupItemModel.create("createdDate", this.createdDate),
            DetailsListGroupItemModel.create("createdTime", this.createdTime),
            DetailsListGroupItemModel.create("incidentDate", this.incidentDate),
            DetailsListGroupItemModel.create("lastModifiedDate", this.lastModifiedDate),
            DetailsListGroupItemModel.create("lastModifiedTime", this.lastModifiedTime),
        ];

        details = details.concat(
            DetailListItemHelper.createDetailItemGroup(DetailsListItemCardType.Dates, dates));

        AdditionalPropertiesHelper.createDetailsList(details, this.additionalPropertyValues);

        if (this.isTestData) {
            let isTestDataItem: Array<DetailsListGroupItemModel> = [DetailsListGroupItemModel.create(
                "testData",
                this.isTestData ? "Yes" : "No",
            )];
            details = details.concat(DetailListItemHelper.createDetailItemGroup(
                DetailsListItemCardType.Status,
                isTestDataItem,
                icons.folder));
        }

        return details;
    }
}
