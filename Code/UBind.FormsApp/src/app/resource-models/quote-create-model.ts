import { DeploymentEnvironment } from "@app/models/deployment-environment";
import { QuoteType } from "@app/models/quote-type.enum";

/**
 * Model for creating various types of quotes
 */
export interface QuoteCreateModel {
    tenant: string;
    organisation?: string;
    portal?: string;
    product?: string;
    environment?: DeploymentEnvironment;
    customerId?: string;
    isTestData?: boolean;
    formData?: any;
    quoteType?: QuoteType;
    policyId?: string;
    discardExistingQuote?: boolean;
    productRelease?: string;
}
