import { CalculationResult, DisplayableFieldsModel, PremiumResult } from '@app/models';
import { CustomerResourceModel, CustomerDetailsResourceModel } from './customer.resource-model';
import { QuoteType } from '@app/models/quote-type.enum';
import { AdditionalPropertyValue } from '@app/models/additional-property-item-view.model';
import { Entity } from '@app/models/entity';
import { UserResourceModel } from '@app/resource-models/user/user.resource-model';

/**
 * Quote resource model
 */
export interface QuoteResourceModel extends Entity {
    quoteNumber: string;
    quoteTitle: string;
    productId: string;
    productAlias: string;
    productName: string;
    deploymentEnvironment: string;
    customerDetails: CustomerResourceModel;
    totalAmount: string;
    lastModifiedDateTime: string;
    status: string;
    createdDateTime: string;
    expiryDateTime: string;
    isTestData: boolean;
    quoteType: number;
    organisationId: string;
    organisationAlias: string;
    ownerUserId: string;
    policyId: string;
}

/**
 * Quote detail resource model
 */
export interface QuoteDetailResourceModel {
    id: string;
    policyId: string;
    quoteTitle: string;
    quoteNumber: string;
    customerDetails: CustomerDetailsResourceModel;
    productName: string;
    productAlias: string;
    productId: string;
    tenantId: string;
    status: string;
    policyNumber: string;
    policyOwnerUserId: string;
    latestCalculationData: CalculationResult;
    premiumData: PremiumResult;
    createdDateTime: string;
    expiryDateTime: string;
    lastModifiedDateTime: string;
    owner: UserResourceModel;
    quoteType: QuoteType;
    displayableFieldsModel: DisplayableFieldsModel;
    isTestData: boolean;
    isDiscarded: boolean;
    formData: any;
    questionData: any;
    documents: any;
    questionAttachmentKeys: Array<string>;
    organisationId: string;
    organisationName: string;
    additionalPropertyValues: Array<AdditionalPropertyValue>;
    policyExpiryDateTime: string;
    policyTransactionEffectiveDateTime: string;
    policyInceptionDateTime: string;
    productReleaseId: string;
    productReleaseNumber: string;
}

/**
 * Quote form data resource model
 */
export interface QuoteFormDataResourceModel {
    id: string;
    policyId: string;
    productId: string;
    environment: string;
    formData: string;
    organisationId: string;
}

/**
 * Resource Model for creating a quote
 */
export interface QuoteCreateResourceModel {
    tenant?: string;
    organisation: string;
    portal: string;
    product: string;
    environment: string;
    customerId?: string;
    isTestData: boolean;
    formData?: any;
    productRelease?: string;
}

/**
 * Model for the result of creating a quote
 */
export interface QuoteCreateResultModel {
    policyId: string;
    quoteId: string;
    quoteType: string;
    productId: string;
    environment: string;
    organisationId: string;
}

/**
 * Model for quote periodic summaries.
 */
export interface QuotePeriodicSummaryModel {
    label: string;
    fromDateTime: string;
    toDateTime: string;
    createdCount: number;
    createdTotalPremium: number;
    convertedCount: number;
    abandonedCount: number;
}
