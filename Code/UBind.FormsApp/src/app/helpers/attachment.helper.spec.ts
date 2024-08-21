import { AttachmentFileProperties } from '@app/models/attachment-file-properties';
import { Errors } from '@app/models/errors';
import { AttachmentHelper } from './attachment.helper';

describe('AttachmentHelper', () => {

    it('parseFileProperties should return the correct properties for an image', () => {
        // Arrange
        const testAttachmentFieldValue: string
            = 'images.jpg:image/jpeg:03fd3f14-985f-4f38-a762-3096f491e0ad:275:183:5754';

        // Act
        const fileProperties: AttachmentFileProperties = AttachmentHelper.parseFileProperties(testAttachmentFieldValue);

        // Assert
        expect(fileProperties.fileName).toBe('images.jpg');
        expect(fileProperties.mimeType).toBe('image/jpeg');
        expect(fileProperties.attachmentId).toBe('03fd3f14-985f-4f38-a762-3096f491e0ad');
        expect(fileProperties.imageWidth).toBe(275);
        expect(fileProperties.imageHeight).toBe(183);
        expect(fileProperties.fileSizeBytes).toBe(5754);
    });

    it('parseFileProperties should return the correct properties for a non-image', () => {
        // Arrange
        const testAttachmentFieldValue: string
            = 'Loan Statement.pdf:application/pdf:e92eb398-2b35-40f5-835e-6e4a5053c71b:::25608';

        // Act
        const fileProperties: AttachmentFileProperties = AttachmentHelper.parseFileProperties(testAttachmentFieldValue);

        // Assert
        expect(fileProperties.fileName).toBe('Loan Statement.pdf');
        expect(fileProperties.mimeType).toBe('application/pdf');
        expect(fileProperties.attachmentId).toBe('e92eb398-2b35-40f5-835e-6e4a5053c71b');
        expect(fileProperties.imageWidth).toBeNull();
        expect(fileProperties.imageHeight).toBeNull();
        expect(fileProperties.fileSizeBytes).toBe(25608);
    });

    it('parseFileProperties should throw an exception when the file data is not valid', () => {
        // Arrange
        const testAttachmentFieldValue: string
            = 'Your mum\'s house:chicken dinner';
        // eslint-disable-next-line no-unused-vars
        let fileProperties: AttachmentFileProperties;

        // Act
        let action: () => AttachmentFileProperties = (): AttachmentFileProperties => fileProperties
                = AttachmentHelper.parseFileProperties(testAttachmentFieldValue);

        // Assert
        expect(action).toThrow(Errors.Attachments.InvalidFileData());

    });

});
