import { QuoteDetailViewModel } from './quote-detail.viewmodel';
import { QuoteVersionDetailResourceModel } from '@app/resource-models/quote-version.resource-model';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import { DetailsListItemActionIcon } from '@app/models/details-list/details-list-item-action-icon';
import { DetailListItemHelper } from '@app/helpers/detail-list-item.helper';
import { QuoteType } from '@app/models';
import { DetailsListGroupItemModel } from '@app/models/details-list/details-list-item-model';
import { StringHelper } from '@app/helpers';
import { DetailsListItemCardType } from '@app/models/details-list/details-list-item-card-type.enum';
import { RelatedEntityType } from '@app/models/related-entity-type.enum';
import { AdditionalPropertiesHelper } from '@app/helpers/additional-properties.helper';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Export quite version detail view model class.
 * TODO: Write a better class header: view model of quote version detail.
 */
export class QuoteVersionDetailViewModel extends QuoteDetailViewModel {

    public constructor(quoteVersionDetail: QuoteVersionDetailResourceModel) {
        super(quoteVersionDetail);
        this.id = quoteVersionDetail.id;
        this.quoteId = quoteVersionDetail.quoteId;
        this.quoteVersionNumber = quoteVersionDetail.quoteVersionNumber;
        this.quoteStatus = quoteVersionDetail.quoteStatus;
        this.referenceNumber = quoteVersionDetail.quoteNumber
            ? quoteVersionDetail.quoteNumber + '-' + quoteVersionDetail.quoteVersionNumber
            : quoteVersionDetail.quoteVersionNumber;
        this.status = quoteVersionDetail.quoteVersionState;
    }

    public quoteId: string;
    public quoteVersionNumber: string;
    public quoteStatus: string;
    public referenceNumber: string;

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
        let policyAction: DetailsListItemActionIcon =
            DetailListItemHelper.createAction(
                () => navProxy.goToPolicy(this.policyId));
        let organisationAction: DetailsListItemActionIcon =
            DetailListItemHelper.createAction(
                () => navProxy.goToOrganisation(this.organisationId));
        let agentAction: DetailsListItemActionIcon =
            DetailListItemHelper.createAction(
                () => navProxy.goToOwner(this.owner ? this.owner.id : null));

        let quoteType: string = this.quoteType == QuoteType.Adjustment ? "Adjustment" :
            this.quoteType == QuoteType.Cancellation ? "Cancellation" :
                this.quoteType == QuoteType.NewBusiness ? "New Business" :
                    "Renewal";

        let detailModel: Array<DetailsListGroupItemModel> = [
            DetailsListGroupItemModel.create("product", this.productName),
            DetailsListGroupItemModel.create("quoteType", quoteType),
            DetailsListGroupItemModel.create("status",
                StringHelper.capitalizeFirstLetter(this.status)),
            DetailsListGroupItemModel.create("quoteReference", this.quoteNumber),
        ];

        details = details.concat(DetailListItemHelper.createDetailItemGroup(
            DetailsListItemCardType.Details,
            detailModel,
            this.iconName,
            IconLibrary.AngularMaterial,
        ));

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
                this.customerDetails ? this.customerDetails.id : null));
        relationships.push(DetailsListGroupItemModel.create(
            "organisation",
            this.organisationName,
            null,
            null,
            organisationAction)
            .withRelatedEntity(
                RelatedEntityType.Organisation,
                this.organisationId,
                null,
                null));

        relationships.push(DetailsListGroupItemModel.create("agent",
            this.owner ? this.owner.fullName : "None",
            null,
            null,
            this.owner ? agentAction : null)
            .withRelatedEntity(
                RelatedEntityType.User,
                this.organisationId,
                this.owner ? this.owner.id : null,
                null));

        details = details.concat(DetailListItemHelper.createDetailItemGroup(
            DetailsListItemCardType.Relationships, relationships));

        let dates: Array<DetailsListGroupItemModel> = [
            DetailsListGroupItemModel.create("createdDate", this.createdDate),
            DetailsListGroupItemModel.create("createdTime", this.createdTime),
            DetailsListGroupItemModel.create("lastModifiedDate", this.lastModifiedDate),
            DetailsListGroupItemModel.create("lastModifiedTime", this.lastModifiedTime),
        ];

        details = details.concat(DetailListItemHelper.createDetailItemGroup(
            DetailsListItemCardType.Dates,
            dates,
        ));

        AdditionalPropertiesHelper.createDetailsList(details, this.additionalPropertyValues);

        return details;
    }
}
