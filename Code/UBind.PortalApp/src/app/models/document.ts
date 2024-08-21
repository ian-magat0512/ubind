/**
 * Represents a document associated with a quote/policy/transaction
 */
export interface Document {
    id: string;
    quoteOrPolicyTransactionId: string;
    quoteId: string;
    fileName: string;
    mimeType: string;
    fileSize: string;
    createdDateTime: string;
    dateGroupHeader: string;
}
