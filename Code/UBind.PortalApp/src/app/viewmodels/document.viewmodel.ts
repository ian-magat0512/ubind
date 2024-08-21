import { Document } from '@app/models';

/**
 * Export document view model class.
 * View model of documents.
 */
export class DocumentViewModel {
    public constructor(document: Document) {
        this.id = document.id;
        this.fileName = document.fileName;
        this.fileSize = document.fileSize;
        this.mimeType = document.mimeType;
        this.createdDateTime = document.createdDateTime;
        this.dateGroupHeader = document.dateGroupHeader;
    }

    public id: string;
    public fileName: string;
    public fileSize: string;
    public mimeType: string;
    public createdDateTime: string;
    public dateGroupHeader: string;
}
