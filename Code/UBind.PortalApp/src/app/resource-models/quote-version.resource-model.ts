import { QuoteDetailResourceModel } from "./quote.resource-model";

/**
 * Resource model for a quote version
 */
export interface QuoteVersionResourceModel {
    id: string;
    quoteId: string;
    quoteVersionNumber: string;
    quoteNumber: string;
    createdDateTime: string;
    lastModifiedDateTime: string;
    organisationId: string;
}

/**
 * Quote version resource model
 */
export interface QuoteVersionDetailResourceModel extends QuoteDetailResourceModel {
    id: string;
    quoteId: string;
    quoteVersionNumber: string;
    quoteStatus: string;
    quoteVersionState: string;
    organisationName: string;
}
