// <copyright file="ImageContentMergeField.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling.GemBoxServices.MergeFields
{
    using GemBox.Document;
    using GemBox.Document.MailMerging;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Providers.File;
    using UBind.Application.FileHandling.GemBoxServices.Enums;
    using UBind.Application.FileHandling.GemBoxServices.Extensions;
    using UBind.Application.FileHandling.GemBoxServices.Helpers;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Represents an Image content merge field.
    /// </summary>
    public class ImageContentMergeField : ImageMergeField
    {
        private readonly IEnumerable<ContentSourceFile> sourceFiles;

        public ImageContentMergeField(IEnumerable<ContentSourceFile> sourceFiles)
        {
            this.sourceFiles = sourceFiles;
        }

        public override MsWordMergeFieldType Type => MsWordMergeFieldType.ImageContent;

        public override void Merge(FieldMergingEventArgs e)
        {
            var fieldName = e.ExtractFieldName()
                .Split(" \\", StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault()
                ?.Trim();

            var fileAlias = fieldName?.ToCamelCase();
            var sourceFile = this.sourceFiles.FirstOrDefault(a => a.Alias == fileAlias);
            if (sourceFile == null)
            {
                // We don't need to throw an error here because this will should be treated as an unused merge field
                // and will be/not be cleared according to the provided clear options
                return;
            }

            var fileName = sourceFile.File.FileName.ToString();
            if (!GemBoxImageHelper.IsValidImageFileName(fileName))
            {
                var errorData = new JObject();
                errorData.Add("contentAlias", sourceFile.Alias);
                throw new ErrorException(
                    Errors.DocumentGeneration.ImageContentFieldMerging.InvalidFileFormat(
                        fieldName,
                        fileName,
                        errorData));
            }

            var pictureStream = new MemoryStream(sourceFile.File.Content);
            Picture picture = null;
            Size imageSize;
            try
            {
                picture = new Picture(e.Document, pictureStream);
                imageSize = picture.Layout.Size;
            }
            catch (InvalidOperationException)
            {
                pictureStream?.Dispose();
                var errorData = new JObject { { "contentAlias", sourceFile.Alias } };
                throw new ErrorException(
                    Errors.DocumentGeneration.ImageContentFieldMerging.InvalidContent(
                        fieldName,
                        errorData));
            }

            var layout = this.CreatePictureLayout(e.Field, imageSize);
            picture.Layout = layout;
            e.Inline = picture;
            e.Cancel = false;
        }
    }
}
