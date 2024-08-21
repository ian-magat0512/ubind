import { PremiumResult, CalculationResult, DisplayableFieldsModel, QuoteState } from '@app/models';
import { CustomerDetailsResourceModel } from '@app/resource-models/customer.resource-model';
import { QuoteDetailResourceModel } from '@app/resource-models/quote.resource-model';
import { UserSummaryViewModel } from './user-summary.viewmodel';
import { DocumentViewModel } from './document.viewmodel';
import { LocalDateHelper, StringHelper, Permission, PermissionDataModel } from '@app/helpers';
import { QuoteType } from '@app/models/quote-type.enum';
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import { DetailsListItemCardType } from '@app/models/details-list/details-list-item-card-type.enum';
import { DetailListItemHelper } from '@app/helpers/detail-list-item.helper';
import { DetailsListGroupItemModel } from '@app/models/details-list/details-list-item-model';
import { PermissionService } from '@app/services/permission.service';
import { DetailsListItemActionIcon } from '@app/models/details-list/details-list-item-action-icon';
import { DetailsListFormItem } from '@app/models/details-list/details-list-form-item';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { AdditionalPropertiesHelper } from '@app/helpers/additional-properties.helper';
import { AdditionalPropertyValue } from '@app/models/additional-property-item-view.model';
import { RelatedEntityType } from "@app/models/related-entity-type.enum";
import { QuoteViewModel } from './quote.viewmodel';
import { IconLibrary } from '@app/models/icon-library.enum';
import { DateHelper } from '@app/helpers/date.helper';
import { DetailsListItemCard } from '@app/models/details-list/details-list-item-card';
import { DetailsListFormDateItem } from '@app/models/details-list/details-list-form-date-item';
import { DetailsListFormTimeItem } from '@app/models/details-list/details-list-form-time-item';

/**
 * Export quote detail view model class
 * This class is the view model of the quote.
 */
export class QuoteDetailViewModel {
    public constructor(quote: QuoteDetailResourceModel) {
        this.id = quote.id;
        this.organisationId = quote.organisationId;
        this.organisationName = quote.organisationName;
        this.policyId = quote.policyId;
        this.customerDetails = quote.customerDetails;
        this.productName = quote.productName;
        this.productAlias = quote.productAlias;
        this.productId = quote.productId;
        this.tenantId = quote.tenantId;
        this.status = quote.status;
        this.quoteType = quote.quoteType;
        this.quoteNumber = quote.quoteNumber;
        this.policyNumber = quote.policyNumber;
        this.policyOwnerUserId = quote.policyOwnerUserId;
        this.latestCalculationData = quote.latestCalculationData;
        this.premiumData = quote.premiumData;
        this.questionData = quote.questionData;
        this.quoteExpiryTimeStamp = quote.expiryDateTime;
        if (quote.expiryDateTime) {
            this.expiryDate = LocalDateHelper.toLocalDate(quote.expiryDateTime);
            this.expiryTimeOfDay = LocalDateHelper.convertToLocalAndGetTimeOnly(quote.expiryDateTime);
        }
        this.lastModifiedDate = LocalDateHelper.toLocalDate(quote.lastModifiedDateTime);
        this.owner = quote.owner ? new UserSummaryViewModel(quote.owner) : null;
        this.documents = quote.documents;
        this.createdDate = LocalDateHelper.toLocalDate(quote.createdDateTime);
        this.createdTime = LocalDateHelper.convertToLocalAndGetTimeOnly(quote.createdDateTime);
        this.lastModifiedTime = LocalDateHelper.convertToLocalAndGetTimeOnly(quote.lastModifiedDateTime.toString());
        this.displayableFieldsModel = quote.displayableFieldsModel;
        this.isTestData = quote.isTestData;
        this.isDiscarded = quote.isDiscarded;
        this.formData = quote.formData ? quote.formData : {};
        this.questionAttachmentKeys = quote.questionAttachmentKeys;
        this.additionalPropertyValues = quote.additionalPropertyValues;
        this.iconName = QuoteViewModel.getIconNameForQuoteType(this.quoteType);
        if (quote.policyExpiryDateTime) {
            this.policyExpiryDate = LocalDateHelper.toLocalDate(quote.policyExpiryDateTime);
            this.policyExpiryTime = LocalDateHelper.convertToLocalAndGetTimeOnly(quote.policyExpiryDateTime);
        }
        if (quote.policyInceptionDateTime) {
            this.policyInceptionDate = LocalDateHelper.toLocalDate(quote.policyInceptionDateTime);
            this.policyInceptionTime = LocalDateHelper.convertToLocalAndGetTimeOnly(
                quote.policyInceptionDateTime,
            );
        }
        if (quote.policyTransactionEffectiveDateTime) {
            this.policyTransactionEffectiveDate = LocalDateHelper.toLocalDate(quote.policyTransactionEffectiveDateTime);
            this.policyTransactionEffectiveTime = LocalDateHelper.convertToLocalAndGetTimeOnly(
                quote.policyTransactionEffectiveDateTime,
            );
        }
        this.productReleaseId = quote.productReleaseId;
        this.productReleaseNumber = quote.productReleaseNumber;
    }

    public id: string;
    public policyId: string;
    public customerDetails: CustomerDetailsResourceModel;
    public productName: string;
    public productAlias: string;
    public productId: string;
    public tenantId: string;
    public status: string;
    public quoteNumber: string;
    public policyNumber: string;
    public policyOwnerUserId: string;
    public latestCalculationData: CalculationResult;
    public questionData: any;
    public premiumData: PremiumResult;
    public createdDate: string;
    public expiryDate: string;
    public expiryTimeOfDay: string;
    public quoteExpiryTimeStamp: string;
    public lastModifiedDate: string;
    public displayableFieldsModel: DisplayableFieldsModel;
    public owner: UserSummaryViewModel;
    public displayableFields: Array<string>;
    public documents: Array<DocumentViewModel>;
    public quoteType: QuoteType;
    public createdTime: string;
    public isTestData: boolean;
    public lastModifiedTime: string;
    public formData: any;
    public isEditable: boolean;
    public isDiscarded: boolean;
    public hasActionsAvailable: boolean;
    public questionAttachmentKeys: Array<string>;
    public organisationId: string;
    public organisationName: string;
    public additionalPropertyValues: Array<AdditionalPropertyValue> = [];
    public iconName: string;
    public policyExpiryDate: string;
    public policyExpiryTime: string;
    public policyInceptionDate: string;
    public policyInceptionTime: string;
    public policyTransactionEffectiveDate: string;
    public policyTransactionEffectiveTime: string;
    public productReleaseId: string;
    public productReleaseNumber: string;

    public createDetailsListForQuoteExpiryDateEdit(): Array<DetailsListFormItem> {
        const details: Array<DetailsListFormItem> = [];
        const datesCard: DetailsListItemCard = new DetailsListItemCard(
            DetailsListItemCardType.Dates,
            'Dates',
        );
        details.push(DetailsListFormDateItem.create(
            datesCard,
            "expiryDate",
            "Expiry Date",
        )
            // set max date limit to 2 years from today
            .withDateRange(null, DateHelper.getFutureDateIsoString(2, 0, 0))
            .withIcon(this.iconName, IconLibrary.AngularMaterial));
        details.push(DetailsListFormTimeItem.create(
            datesCard,
            "expiryTimeOfDay",
            "Expiry Time Of Day",
        ));
        return details;
    }

    public getFormBuilderFormattedExpiryDateTimeStamp(): string {
        return this.quoteExpiryTimeStamp ?
            LocalDateHelper.fromUtcIsoStringToLocalDateTimeIsoString(
                this.quoteExpiryTimeStamp,
            ) : "";
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
        let policyAction: DetailsListItemActionIcon =
            DetailListItemHelper.createAction(
                () => navProxy.goToPolicy(this.policyId));
        let organisationAction: DetailsListItemActionIcon =
            DetailListItemHelper.createAction(
                () => navProxy.goToOrganisation(this.organisationId));
        let agentAction: DetailsListItemActionIcon =
            DetailListItemHelper.createAction(
                () => navProxy.goToOwner(this.owner ? this.owner.id : null, this.organisationId));

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
                customerAction,
            ).withRelatedEntity(
                RelatedEntityType.Customer,
                this.organisationId,
                this.customerDetails.ownerUserId,
                this.customerDetails.id,
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
            this.customerDetails ? this.customerDetails.id : null,
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

        relationships.push(DetailsListGroupItemModel.create(
            "agent",
            this.owner ? this.owner.fullName : "None",
            null,
            null,
            this.owner ? agentAction : null,
        ).withRelatedEntity(
            RelatedEntityType.User,
            this.organisationId,
            this.owner ? this.owner.id : null,
            null,
        ));

        details = details.concat(DetailListItemHelper.createDetailItemGroup(
            DetailsListItemCardType.Relationships,
            relationships,
        ));

        let dates: Array<DetailsListGroupItemModel> = [];

        switch (this.quoteType) {
            case QuoteType.NewBusiness:
                dates.push(DetailsListGroupItemModel.create("PolicyInceptionDate", this.policyInceptionDate));
                dates.push(DetailsListGroupItemModel.create("PolicyInceptionTime", this.policyInceptionTime));
                dates.push(DetailsListGroupItemModel.create("PolicyExpiryDate", this.policyExpiryDate));
                dates.push(DetailsListGroupItemModel.create("PolicyExpiryTime", this.policyExpiryTime));
                break;
            // if the quote is an adjustment quote, the value of AdjustmentEffectiveDateTime will be taken from
            // the policyTransactionEffectiveDatetime property of the QuoteDetailResourceModel.
            case QuoteType.Adjustment:
                dates.push(DetailsListGroupItemModel.create(
                    "AdjustmentEffectiveDate",
                    this.policyTransactionEffectiveDate));
                dates.push(DetailsListGroupItemModel.create(
                    "AdjustmentEffectiveTime",
                    this.policyTransactionEffectiveTime));
                break;
            // if the quote is a renewal quote, the value of RenewalEffectiveDate will be taken from
            // the policyTransactionEffectiveDatetime property of the QuoteDetailResourceModel.
            case QuoteType.Renewal:
                dates.push(DetailsListGroupItemModel.create(
                    "RenewalEffectiveDate",
                    this.policyTransactionEffectiveDate));
                dates.push(DetailsListGroupItemModel.create(
                    "RenewalEffectiveTime",
                    this.policyTransactionEffectiveTime));
                break;
            // if the quote is a cancellation quote, the value of CancellationEffectiveDate will be taken from
            // the policyTransactionEffectiveDatetime property of the QuoteDetailResourceModel.
            case QuoteType.Cancellation:
                dates.push(DetailsListGroupItemModel.create(
                    "CancellationEffectiveDate",
                    this.policyTransactionEffectiveDate,
                ));
                dates.push(DetailsListGroupItemModel.create(
                    "CancellationEffectiveTime",
                    this.policyTransactionEffectiveTime,
                ));
                break;
        }

        dates.push(...[
            DetailsListGroupItemModel.create("createdDate", this.createdDate),
            DetailsListGroupItemModel.create("createdTime", this.createdTime),
            DetailsListGroupItemModel.create("lastModifiedDate", this.lastModifiedDate),
            DetailsListGroupItemModel.create("lastModifiedTime", this.lastModifiedTime),
            DetailsListGroupItemModel.create("expiryDate", this.expiryDate),
            DetailsListGroupItemModel.create("expiryTimeOfDay", this.expiryTimeOfDay),
        ]);

        details = details.concat(DetailListItemHelper.createDetailItemGroup(
            DetailsListItemCardType.Dates,
            dates,
        ));

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

    public determineIfEditable(permissionService: PermissionService, model: PermissionDataModel): void {
        const status: string = this.getStatus();
        if (status === QuoteState.Review ||
            status === QuoteState.Endorsement ||
            status === QuoteState.Expired) {
            this.isEditable =
                permissionService
                    .hasElevatedPermissionsViaModel(Permission.ManageQuotes, model);
        } else {
            this.isEditable = !(status === QuoteState.Complete || status === QuoteState.Declined);
        }

        this.isEditable = this.isEditable && !this.isDiscarded;
    }

    public determineIfHasActionsAvailable(
        permissionService: PermissionService,
        canEditAdditionalPropertyValues: boolean,
        expiryEnabled: boolean,
        hasRequiredPermission: boolean,
        canResumeQuote: boolean,
    ): void {
        const status: string = this.getStatus();
        let hasExpiryPopup: boolean =
            permissionService.hasPermission(Permission.ManageQuotes) &&
            expiryEnabled;

        const hasReviewAndEndorseActions: boolean = hasRequiredPermission
            && (status == QuoteState.Review || status == QuoteState.Endorsement);

        const hasExpiryActions: boolean = hasExpiryPopup &&
            !(status == QuoteState.Declined
                || status == QuoteState.Complete);

        this.hasActionsAvailable = hasReviewAndEndorseActions
            || hasExpiryActions
            || canEditAdditionalPropertyValues
            || canResumeQuote;
    }

    private getStatus(): string {
        return this.status.toLowerCase().replace(' ', '');
    }
}
