import { ClaimResourceModel } from './claim.resource-model';
import { PolicyResourceModel } from './policy.resource-model';
import { QuoteResourceModel } from './quote.resource-model';
import { Entity } from '../models/entity';
import { PersonResourceModel } from '@app/models';
import { AccountingTransactionResourceModel } from './accounting-transaction.resource-model';
import { AdditionalPropertyValue } from '@app/models/additional-property-item-view.model';
import { EntityViewModel } from '@app/viewmodels/entity.viewmodel';
/**
 * Customer resource model
 */
export interface CustomerResourceModel extends Entity, EntityViewModel {
    id: string;
    fullName: string;
    displayName: string;
    isTestData: boolean;
    status: string;
    createdDateTime: string;
    lastModifiedDateTime: string;
    profilePictureId: string;
    ownerUserId: string;
    userId: string;
    primaryPersonId: string;
}

/**
 * Customer details resource model
 */
export interface CustomerDetailsResourceModel extends PersonResourceModel, CustomerResourceModel {
    id: string;
    primaryPersonId: string;
    tenantId: string;
    ownerId: string;
    ownerFullName: string;
    createdDateTime: string;
    quotes: Array<QuoteResourceModel>;
    policies: Array<PolicyResourceModel>;
    claims: Array<ClaimResourceModel>;
    people: Array<PersonResourceModel>;
    transactions: Array<AccountingTransactionResourceModel>;
    accountBalance: string;
    accountBalanceSubtext: string;
    lastModifiedDateTime: string;
    additionalPropertyValues: Array<AdditionalPropertyValue>;
    portalId: string;
    portalName: string;
}
/**
 * Customer quote association result model
 */
export interface CustomerQuoteAssociationResultModel {
    succeeded: string;
    deploymentEnvironment: string;
    quoteId: string;
    quoteNumber: string;
}

/**
 * Customer quote association verification result model
 */
export interface CustomerQuoteAssociationVerificationResultModel {
    isAvailable: boolean;
    error: string;
}
