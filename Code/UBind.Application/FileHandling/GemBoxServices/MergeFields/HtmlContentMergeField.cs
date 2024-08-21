// <copyright file="HtmlContentMergeField.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.FileHandling.GemBoxServices.MergeFields
{
    using GemBox.Document.MailMerging;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers.File;
    using UBind.Application.FileHandling.GemBoxServices.Enums;
    using UBind.Application.FileHandling.GemBoxServices.Extensions;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Represents an HTML content merge field.
    /// </summary>
    public class HtmlContentMergeField : HtmlMergeField
    {
        private readonly IEnumerable<ContentSourceFile> sourceFiles;

        public HtmlContentMergeField(IEnumerable<ContentSourceFile> sourceFiles)
        {
            this.sourceFiles = sourceFiles;
        }

        public override MsWordMergeFieldType Type => MsWordMergeFieldType.HtmlContent;

        public override void Merge(FieldMergingEventArgs e)
        {
            var fieldName = e.ExtractFieldName();
            var fileAlias = fieldName.ToCamelCase();
            var sourceFile = this.sourceFiles.Where(a => a.Alias == fileAlias).FirstOrDefault();
            if (sourceFile == null)
            {
                // We don't need to throw an error here because this will should be treated as an unused merge field
                // and will be/not be cleared according to the provided clear options
                return;
            }

            var fileName = sourceFile.File.FileName.ToString();
            string extension = Path.GetExtension(fileName);

            if (string.IsNullOrEmpty(extension) || !string.Equals(extension, ".html", StringComparison.OrdinalIgnoreCase))
            {
                var errorData = new JObject();
                errorData.Add("contentAlias", sourceFile.Alias);
                throw new ErrorException(
                    Errors.DocumentGeneration.HtmlContentFieldMerging.InvalidFileFormat(
                        fieldName,
                        fileName,
                        errorData));
            }

            string htmlString = sourceFile.File.ReadContentToEnd();
            this.MergeHtmlStringToField(e, htmlString);
            e.Inline = null;
        }
    }
}
