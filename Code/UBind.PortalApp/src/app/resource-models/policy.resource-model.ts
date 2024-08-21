import { CalculationResult, PremiumResult, DisplayableFieldsModel, Document } from '@app/models';
import { AdditionalPropertyValue } from '@app/models/additional-property-item-view.model';
import { Entity } from '@app/models/entity';
import { CustomerResourceModel } from '@app/resource-models/customer.resource-model';
import { UserResourceModel } from '@app/resource-models/user/user.resource-model';
import { PolicyTransactionEventNamePastTense } from '@app/models/policy-transaction-event-name-past-tense.enum';

/**
 * Resource model for a policy
 */
export interface PolicyResourceModel extends Entity {
    /**
     * The id of the quote that was used to create this policy
     */
    quoteId: string;
    policyTitle: string;
    policyNumber: string;
    productName: string;
    productId: string;
    totalPayable: string;
    status: string;
    createdDateTime: string;
    issuedDateTime: string;
    inceptionDateTime: string;
    expiryDateTime: string;
    isForRenewal?: boolean;
    customer: CustomerResourceModel;
    isTestData: boolean;
    cancellationEffectiveDateTime: string;
    lastModifiedDateTime: string;
    latestRenewalEffectiveDateTime: string;
}

/**
 * Resource model for a policy details
 */
export interface PolicyDetailResourceModel {
    id: string;
    policyTitle: string;
    organisationId: string;
    organisationName: string;
    quoteId: string;
    policyNumber: string;
    productName: string;
    productId: string;
    status: string;
    tenantId: string;
    numberOfDaysToExpire: number;
    quoteNumber: string;
    quoteOwnerUserId: string;
    createdDateTime: string;
    expiryDateTime: string;
    lastModifiedDateTime: string;
    inceptionDateTime: string;
    owner: UserResourceModel;
    calculationResult: CalculationResult;
    customer: CustomerResourceModel;
    formData: any;
    hasFutureTransaction: boolean;
    futureTransactionDateTime: string;
    futureTransactionId: string;
    futureTransactionType: string;
    effectiveDateTime: string;
    cancellationEffectiveDateTime: string;
    eventTypeSummary: string;
    hasClaimConfiguration: boolean;
    additionalPropertyValues: Array<AdditionalPropertyValue>;
    isTestData: boolean;
}

/**
 * Resource model for policy details containing the premium/pricing
 */
export interface PolicyPremiumDetailResourceModel extends PolicyDetailResourceModel {
    premiumData: PremiumResult;
}

/**
 * Resource model for policy details containing documents
 */
export interface PolicyDocumentsDetailResourceModel extends PolicyDetailResourceModel {
    documents: Array<Document>;
}

/**
 * Resource model for policy details containing questions and displayable fields
 */
export interface PolicyQuestionDetailResourceModel extends PolicyDetailResourceModel {
    questions: any;
    displayableFieldsModel: DisplayableFieldsModel;
}

/**
 * Resource model for a policy transaction
 */
export interface PolicyTransactionResourceModel extends Entity {
    transactionId: string;
    policyId: string;
    premium: PremiumResult;
    policyNumber: string;
    quoteId: string;
    productId: string;
    eventTypeSummary: string;
    transactionStatus: string;
    effectiveDateTime: string;
    cancellationEffectiveDateTime: string;
    createdDateTime: string;
    expiryDateTime: string;
}

/**
 * Resource model for a policy transaction's details
 */
export interface PolicyTransactionDetailResourceModel {
    // Base policy transactiond detail base properties
    policyNumber: string;
    policyOwnerUserId: string;
    productAlias: string;
    productName: string;
    createdDateTime: string;
    lastModifiedDateTime: string;
    effectiveDateTime: string;
    transactionStatus: string;
    eventTypeSummary: PolicyTransactionEventNamePastTense;
    documents: Array<Document>;
    customer?: CustomerResourceModel;
    owner?: UserResourceModel;
    cancellationEffectiveDateTime?: string;
    status: string;
    questionAttachmentKeys: Array<string>;
    formData: any;

    // Policy upsert transaction additional properties
    quoteId: string;
    quoteOwnerUserId: string;
    policyId: string;
    quoteReference: string;
    expiryDateTime?: string;
    questions: string;
    displayableFields: DisplayableFieldsModel;
    premium: PremiumResult;
    organisationId: string;
    organisationName: string;

    additionalPropertyValues: Array<AdditionalPropertyValue>;
    productReleaseId: string;
    productReleaseNumber: string;
}

/**
 * Model for policy transaction periodic summaries.
 */
export interface PolicyTransactionPeriodicSummaryModel {
    label: string;
    fromDateTime: string;
    toDateTime: string;
    createdCount: number;
    createdTotalPremium: number;
}

/**
 * Resource model for a policy's issuance
 */
export interface PolicyIssuanceResourceModel {
    policyId: string;
    policyNumber: string;
    returnOldPolicyNumberToPool: boolean;
}

/**
 * Resource model for a issuing a policy
 */
export interface IssuePolicyResourceModel {
    policyNumber: string;
}

/**
 * Resource model for updating a policy number
 */
export interface UpdatePolicyNumberResourceModel {
    policyNumber: string;
    returnOldPolicyNumberToPool: boolean;
}
