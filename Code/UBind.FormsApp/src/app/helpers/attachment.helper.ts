import { AttachmentFileProperties } from "@app/models/attachment-file-properties";
import { Errors } from "@app/models/errors";
import { StringHelper } from "./string.helper";

/**
 * Helper functions for attachment field values.
 */
export class AttachmentHelper {

    /**
     * Parses an attachment field value into an instance of AttachmentFileProperties.
     * An attachment field value is just a string, stored in the following structure:
     * "{filename}:{mime type}:{attachment ID}:{image width}:{image height}:{file size in bytes}"
     * @param attachmentFieldValue the raw string value from the form data
     * @returns an instance of AttachmentFileProperties.
     * @throws an error if the attachmentFieldValue does not contain valid file details.
     */
    public static parseFileProperties(attachmentFieldValue: string): AttachmentFileProperties {
        const fileRegExp: RegExp = /^([^\:]+)\:([^\:]+)\:([^\:]*)\:([^\:]*)\:([^\:]*)\:?([^\:]*)?$/;
        if (!fileRegExp.test(attachmentFieldValue)) {
            throw Errors.Attachments.InvalidFileData();
        }
        const fieldValueParts: Array<string> = attachmentFieldValue.split(':');
        const imageWidth: number = !StringHelper.isNullOrEmpty(fieldValueParts[3])
            ? Number.parseInt(fieldValueParts[3], 10)
            : null;
        const imageHeight: number = !StringHelper.isNullOrEmpty(fieldValueParts[4])
            ? Number.parseInt(fieldValueParts[4], 10)
            : null;
        const fileSizeBytes: number = !StringHelper.isNullOrEmpty(fieldValueParts[5])
            ? Number.parseInt(fieldValueParts[5], 10)
            : null;

        let result: AttachmentFileProperties = {
            fileName: fieldValueParts[0],
            mimeType: fieldValueParts[1],
            attachmentId: fieldValueParts[2],
            imageWidth: imageWidth,
            imageHeight: imageHeight,
            fileSizeBytes: fileSizeBytes,
        };

        return result;
    }
}
