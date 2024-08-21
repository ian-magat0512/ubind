import { AdditionalPropertyValue } from '@app/models/additional-property-item-view.model';
import { DisplayableFieldsModel } from '@app/models';
import { DeploymentEnvironment } from '@app/models/deployment-environment.enum';
import { Entity } from '@app/models/entity';
import { CustomerResourceModel } from './customer.resource-model';
import { Document } from '@app/models/document';

/**
 * Claim resource model
 */
export interface ClaimResourceModel extends Entity {
    id: string;
    productId: string;
    customerDetails: CustomerResourceModel;
    productName: string;
    status: string;
    policyId: string;
    policyNumber: string;
    policyOwnerUserId: string;
    claimReference: string;
    claimNumber: string;
    ownerUserId: string;
    ownerName: string;
    createdDateTime: string;
    lastModifiedDateTime: string;
    incidentDateTime: string;
    amount: string;
    description: string;
    formData: any;
    displayableFieldsModel: DisplayableFieldsModel;
    calculationResult: any;
    questionAttachmentKeys: Array<string>;
    additionalPropertyValues: Array<AdditionalPropertyValue>;
    documents: Array<Document>;
    organisationId: string;
    organisationName: string;
    isTestData: boolean;
}

/**
 * Claim version resource model
 */
export interface ClaimVersionResourceModel extends ClaimResourceModel {
    id: string;
    claimId: string;
    versionNumber: string;
}

/**
 * Claim list item resource model
 */
export interface ClaimVersionListResourceModel {
    claimId: string;
    claimVersionId: string;
    versionNumber: string;
    claimReference: string;
    createdDateTime: string;
    lastModifiedDateTime: string;
}

/**
 * Claim email resource model
 */
export interface ClaimEmailResourceModel {
    customerEmail: string;
    emailSubject: string;
    createdDateTime: string;
}

/**
 * Model for creating a claim
 */
export interface ClaimCreateModel {
    tenant?: string;
    organisation?: string;
    product?: string;
    environment?: DeploymentEnvironment;
    policyId?: string;
    customerId?: string;
    isTestData?: boolean;
    formData?: any;
}

/**
 * Model for claim periodic summaries used in dashboard.
 */
export interface ClaimPeriodicSummaryModel {
    label: string;
    fromDateTime: string;
    toDateTime: string;
    settledCount: number;
    declinedCount: number;
    processedCount: number;
    averageSettlementAmount: number;
    averageProcessingTime: number;
}
