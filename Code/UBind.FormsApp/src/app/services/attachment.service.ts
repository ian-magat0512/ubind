import { Injectable } from '@angular/core';
import { AttachmentFileProperties } from '@app/models/attachment-file-properties';

/**
 * Export attachment service class.
 * TODO: Write a better class header: attachment functions.
 */
@Injectable()
export class AttachmentService {

    public static readonly AttachmentSignatureRegExp: RegExp
        = /^([^:]+)\:([^\:]+)\:([^\:]*)\:([^\:]*)\:([^\:]*)\:?([^\:]*)?$/;
    private attachments: any = {};

    public constructor() {
    }

    public getAttachment(attachmentId: string): AttachmentFileProperties {
        return this.attachments[attachmentId];
    }

    public setAttachment(attachment: AttachmentFileProperties): void {
        this.attachments[attachment.attachmentId] = attachment;
    }

    public clearAttachment(attachmentId: string): void {
        if (this.attachments[attachmentId]) {
            delete this.attachments[attachmentId];
        }
    }

    public isAttachmentSignature(value: string): boolean {
        return AttachmentService.AttachmentSignatureRegExp.test(value);
    }
}
