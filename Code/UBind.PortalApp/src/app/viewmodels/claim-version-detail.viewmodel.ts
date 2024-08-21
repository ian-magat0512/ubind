import { LocalDateHelper } from '@app/helpers';
import { ClaimVersionResourceModel } from '@app/resource-models/claim.resource-model';
import { CustomerResourceModel } from '@app/resource-models/customer.resource-model';
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import { DetailListItemHelper } from '@app/helpers/detail-list-item.helper';
import { DetailsListItemCardType } from '@app/models/details-list/details-list-item-card-type.enum';
import { DetailsListGroupItemModel } from '@app/models/details-list/details-list-item-model';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { AdditionalPropertyValue } from '@app/models/additional-property-item-view.model';
import { AdditionalPropertiesHelper } from '@app/helpers/additional-properties.helper';
import { DetailsListItemActionIcon } from '@app/models/details-list/details-list-item-action-icon';
import { RelatedEntityType } from "@app/models/related-entity-type.enum";
import { Document } from 'app/models/document';

/**
 * Export claim version detail view model class.
 * This class manage Details view of claim version functions.
 */
export class ClaimVersionDetailViewModel {
    public customerId: string;
    public customerName: string;
    public productId: string;
    public productName: string;
    public id: string;
    public claimId: string;
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
    public lastModifiedTime: string;
    public questionData: any;
    public formData: string;
    public displayableFieldsModel: string;
    public customerDetails: CustomerResourceModel;
    public versionNumber: string;
    public additionalPropertyValues: Array<AdditionalPropertyValue> = [];
    public documents: Array<Document>;
    public organisationId: string;

    /**
     * Creates a new instance of a claim detail model
     */
    public constructor(claimVersion: ClaimVersionResourceModel) {
        this.id = claimVersion.id;
        this.claimId = claimVersion.claimId;
        this.productName = claimVersion.productName;
        this.productId = claimVersion.productId;
        this.policyId = claimVersion.policyId;
        this.policyNumber = claimVersion.policyNumber;
        this.status = claimVersion.status;
        this.createdDate = LocalDateHelper.toLocalDate(claimVersion.createdDateTime);
        this.createdTime = LocalDateHelper.convertToLocalAndGetTimeOnly(claimVersion.createdDateTime);
        this.lastModifiedDate = LocalDateHelper.toLocalDate(claimVersion.lastModifiedDateTime);
        this.lastModifiedTime = LocalDateHelper.convertToLocalAndGetTimeOnly(claimVersion.lastModifiedDateTime);
        this.customerDetails = claimVersion.customerDetails;
        if (this.customerDetails) {
            this.customerId = this.customerDetails.id;
            this.customerName = this.customerDetails.fullName;
        }
        this.claimReference = claimVersion.claimReference;
        this.claimNumber = claimVersion.claimNumber;
        this.questionData = claimVersion.calculationResult;
        this.formData = claimVersion.formData;
        this.versionNumber = claimVersion.versionNumber;
        this.additionalPropertyValues = claimVersion.additionalPropertyValues;
        this.documents = claimVersion.documents;
        this.policyOwnerUserId = claimVersion.policyOwnerUserId;
        this.organisationId = claimVersion.organisationId;
    }

    public createDetailsList(
        navProxy: NavProxyService,
        isCustomer: boolean,
        isMutual: boolean,
        tenantAlias: string = "",
    ): Array<DetailsListItem> {
        let details: Array<DetailsListItem> = [];
        let icons: typeof DetailListItemHelper.detailListItemIconMap =
            DetailListItemHelper.detailListItemIconMap;
        let customerAction: DetailsListItemActionIcon = null;
        if (this.customerDetails) {
            DetailListItemHelper.createAction(
                () => navProxy.goToCustomer(this.customerDetails.id));
        }
        let policyAction: DetailsListItemActionIcon = null;
        if (this.policyId) {
            DetailListItemHelper.createAction(
                () => navProxy.goToPolicy(this.policyId));
        }

        let detailModel: Array<DetailsListGroupItemModel> = [
            DetailsListGroupItemModel.create("product",
                this.productName),
            DetailsListGroupItemModel.create("claimReference",
                this.claimReference + "-" + this.versionNumber),
            DetailsListGroupItemModel.create("claimNumber",
                this.claimNumber),
        ];

        details = details.concat(DetailListItemHelper.createDetailItemGroup(
            DetailsListItemCardType.Details,
            detailModel,
            icons.alert));

        let relationships: Array<DetailsListGroupItemModel> = [];

        if (customerAction) {
            relationships.push(DetailsListGroupItemModel.create(
                "customer",
                !isCustomer ? this.customerName : null,
                null,
                null,
                customerAction)
                .withRelatedEntity(
                    RelatedEntityType.Customer,
                    this.organisationId,
                    this.customerDetails.ownerUserId,
                    this.customerId));
        }

        if (policyAction) {
            relationships.push(DetailsListGroupItemModel.create(
                isMutual ? "protection" : "policy",
                this.policyNumber,
                null,
                null,
                policyAction)
                .withRelatedEntity(
                    RelatedEntityType.Policy,
                    this.organisationId,
                    this.policyOwnerUserId,
                    this.customerId));
        }

        details = details.concat(DetailListItemHelper.createDetailItemGroup(
            DetailsListItemCardType.Relationships, relationships));

        let dates: Array<DetailsListGroupItemModel> = [
            DetailsListGroupItemModel.create("createdDate", this.createdDate),
            DetailsListGroupItemModel.create("createdTime", this.createdTime),
            DetailsListGroupItemModel.create("lastModifiedDate", this.lastModifiedDate),
            DetailsListGroupItemModel.create("lastModifiedTime", this.lastModifiedTime),
        ];

        details = details.concat(DetailListItemHelper.createDetailItemGroup(
            DetailsListItemCardType.Dates, dates));

        AdditionalPropertiesHelper.createDetailsList(details, this.additionalPropertyValues);

        return details;
    }
}
