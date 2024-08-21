// <copyright file="ImageMergeField.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling.GemBoxServices.MergeFields
{
    using GemBox.Document;
    using GemBox.Document.MailMerging;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.FileHandling.GemBoxServices.Enums;
    using UBind.Application.FileHandling.GemBoxServices.Extensions;
    using UBind.Application.FileHandling.GemBoxServices.Helpers;
    using UBind.Application.FileHandling.GemBoxServices.Models;
    using UBind.Application.Queries.FileAttachment;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Represents an image merge field.
    /// </summary>
    public class ImageMergeField : MsWordMergeField
    {
        private readonly Guid tenantId;
        private readonly ICqrsMediator mediator;

        public ImageMergeField(Guid tenantId, ICqrsMediator mediator)
        {
            this.tenantId = tenantId;
            this.mediator = mediator;
        }

        protected ImageMergeField()
        {
        }

        public override MsWordMergeFieldType Type => MsWordMergeFieldType.Image;

        public override void Merge(FieldMergingEventArgs e)
        {
            if (!e.IsValueFound)
            {
                return;
            }

            var fieldName = e.ExtractFieldName();
            var value = this.GetStringOrThrow(e.Value, fieldName);

            GemBoxImageFileAttachmentModel imageFileAttachment = this.CreateImageFileAttachmentModel(value, fieldName);
            var content = this.mediator.Send(
                new GetFileAttachmentContentQuery(this.tenantId, imageFileAttachment.AttachmentId)).Result;

            if (content == null)
            {
                var errorData = new JObject
                {
                    { "imageData", value },
                    { "attachmentId", imageFileAttachment.AttachmentId },
                };
                throw new ErrorException(
                    Errors.DocumentGeneration.ImageFieldMerging.AttachmentNotFound(
                        fieldName,
                        errorData));
            }

            Picture picture = this.CreatePictureFromDocument(e, fieldName, value, imageFileAttachment, content.FileContent);

            Size imageSize = new Size(imageFileAttachment.Width, imageFileAttachment.Height);
            var layout = this.CreatePictureLayout(e.Field, imageSize);
            picture.Layout = layout;
            e.Inline = picture;
        }

        protected Layout CreatePictureLayout(Field field, Size imageSize)
        {
            // If the merge field contains an image template with layout, we must use that layout,
            // if it doesn't, then we should use the attachment image's dimensions for the layout
            var templateImage = field.ResultInlines
                        .Where(a => a.ElementType == ElementType.Picture)
                        .Select(a => (Picture)a).FirstOrDefault();
            if (templateImage != null && templateImage.Layout != null)
            {
                var instruction = field.InstructionInlines
                            .Where(a => a.ElementType == ElementType.Run)
                            .FirstOrDefault();
                var preserveWidth = false;
                var preserveHeight = false;
                if (instruction != null && instruction is Run run)
                {
                    // if it contains the switch \x, the template width must be preserved
                    preserveWidth = run.Text.Contains("\\x");

                    // if it contains the switch \y, the template height must be preserved
                    preserveHeight = run.Text.Contains("\\y");
                }

                // If it does not contain any switches, it does not preserve the aspect ratio and just fits
                // the image to the template
                var lockAspectRatio = preserveWidth || preserveHeight;
                var size = GemBoxImageHelper.CalculateSize(
                    imageSize,
                    templateImage.Layout.Size,
                    preserveWidth,
                    preserveHeight);

                return new InlineLayout(size) { LockAspectRatio = lockAspectRatio };
            }

            return new InlineLayout(imageSize);
        }

        private GemBoxImageFileAttachmentModel CreateImageFileAttachmentModel(string data, string fieldName)
        {
            try
            {
                return GemBoxImageFileAttachmentModel.Create(data, fieldName);
            }
            catch (ErrorException ex)
            {
                ex.EnrichAndRethrow(null);
                throw;
            }
        }

        private Picture CreatePictureFromDocument(FieldMergingEventArgs e, string fieldName, string value, GemBoxImageFileAttachmentModel imageFileAttachment, byte[] fileContentReadModel)
        {
            var pictureStream = new MemoryStream(fileContentReadModel);
            Picture picture;
            try
            {
                picture = new Picture(e.Document, pictureStream);
            }
            catch (InvalidOperationException)
            {
                // We should only dispose the stream if an exception is thrown because Gembox uses
                // the stream to load the image and we don't want to dispose it if the image is valid.
                // Gembox will manage the disposal of these streams once the SaveDocument method is called.
                if (pictureStream != null)
                {
                    pictureStream.Dispose();
                }
                var errorData = new JObject();
                errorData.Add("imageData", value);
                errorData.Add("attachmentId", imageFileAttachment.AttachmentId);
                throw new ErrorException(
                    Errors.DocumentGeneration.ImageFieldMerging.InvalidImageAttachment(
                        fieldName,
                        errorData));
            }

            return picture;
        }
    }
}
