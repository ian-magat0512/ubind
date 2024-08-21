/**
 * Properties and metadata for an attachment that was uploaded into a web form.
 */
export interface AttachmentFileProperties {
    fileName: string;
    mimeType: string;
    fileData?: any;
    fileSizeBytes: number;
    imageWidth: number;
    imageHeight: number;
    attachmentId: string;
    quoteId?: string;
    claimId?: string;
}
