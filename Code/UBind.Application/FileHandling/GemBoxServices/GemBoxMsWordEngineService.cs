// <copyright file="GemBoxMsWordEngineService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.FileHandling.GemBoxServices
{
    using System;
    using System.IO;
    using GemBox.Document;
    using GemBox.Document.MailMerging;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Providers.File;
    using UBind.Application.FileHandling;
    using UBind.Application.FileHandling.GemBoxServices.Enums;
    using UBind.Application.FileHandling.GemBoxServices.Extensions;
    using UBind.Application.FileHandling.GemBoxServices.Helpers;
    using UBind.Application.FileHandling.GemBoxServices.MergeFields;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;

    /// <inheritdoc />
    public class GemBoxMsWordEngineService : EngineBaseService<GemBoxMsWordEngineService>, IGemBoxMsWordEngineService
    {
        private readonly ICqrsMediator mediator;
        private readonly List<string> wordDocumentFieldNames;

        /// <summary>
        /// Initializes a new instance of the <see cref="GemBoxMsWordEngineService"/> class.
        /// </summary>
        /// <param name="logger">Logging service.</param>
        /// <param name="mediator">cqrs mediator.</param>
        public GemBoxMsWordEngineService(ILogger<GemBoxMsWordEngineService> logger, ICqrsMediator mediator)
            : base(logger)
        {
            this.mediator = mediator;
            this.wordDocumentFieldNames = new List<string>();
        }

        public byte[] MergeDataToTemplate(
            Guid tenantId,
            JObject dataSource,
            byte[] templateContent,
            bool removeUnusedMergeFields,
            bool removeRangesWhereAllMergeFieldsAreUnused,
            bool removeTablesWhereAllMergeFieldsAreUnused,
            bool removeTableRowsWhereAllMergeFieldsAreUnused,
            bool removeParagraphsWhereAllMergeFieldsAreUnused,
            IEnumerable<ContentSourceFile> sourceFiles)
        {
            DocumentModel document = GemBoxDocumentHelper.LoadDocument(templateContent, LoadOptions.DocxDefault);
            var data = GemBoxDataSourceHelper.Format(dataSource);

            document.MailMerge.ClearOptions = GemBoxDocumentHelper.GetClearOptions(
                    removeUnusedMergeFields,
                    removeRangesWhereAllMergeFieldsAreUnused,
                    removeTablesWhereAllMergeFieldsAreUnused,
                    removeTableRowsWhereAllMergeFieldsAreUnused,
                    removeParagraphsWhereAllMergeFieldsAreUnused);

            foreach (string fieldName in document.MailMerge.GetMergeFieldNames())
            {
                int index = fieldName.IndexOf(':');
                if (index > 0 && !document.MailMerge.FieldMappings.ContainsKey(fieldName))
                {
                    document.MailMerge.FieldMappings.Add(fieldName, fieldName.Substring(index + 1));
                }
            }

            document.MailMerge.FieldMerging += (sender, e) => this.FieldMergingEventHandler(tenantId, e, sourceFiles);
            document.MailMerge.Execute(data);

            // Since merging of external word documents is custom merging, we must manually remove their respective merge fields
            foreach (var section in document.Sections)
            {
                var mergeFields = section.GetChildElements(true, ElementType.Field).ToList();
                for (int i = 0; i < mergeFields.Count; i++)
                {
                    Field field = (Field)mergeFields[0];
                    var fieldName = field.GetInstructionText();
                    if (this.wordDocumentFieldNames.Contains(fieldName))
                    {
                        field.Parent?.Content?.Delete();
                    }
                }
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    document.Save(stream, SaveOptions.DocxDefault);
                    return stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                var errorData = new JObject();
                errorData.Add(this.ErrorMesssageKey, ex.Message);
                throw new ErrorException(
                    Errors.DocumentGeneration.GenerateFinalOutputFailed(errorData), ex);
            }
        }

        private void FieldMergingEventHandler(Guid tenantId, FieldMergingEventArgs e, IEnumerable<ContentSourceFile> sourceFiles)
        {
            var fieldType = e.GetMsWordMergeFieldType();
            IMsWordMergeField mergeField;
            switch (fieldType)
            {
                case MsWordMergeFieldType.Html:
                    mergeField = new HtmlMergeField();
                    break;
                case MsWordMergeFieldType.Image:
                    mergeField = new ImageMergeField(tenantId, this.mediator);
                    break;
                case MsWordMergeFieldType.WordContent:
                    mergeField = new WordContentMergeField(sourceFiles, this.wordDocumentFieldNames);
                    break;
                case MsWordMergeFieldType.HtmlContent:
                    mergeField = new HtmlContentMergeField(sourceFiles);
                    break;
                case MsWordMergeFieldType.ImageContent:
                    mergeField = new ImageContentMergeField(sourceFiles);
                    break;
                default:
                    mergeField = new MsWordMergeField();
                    break;
            }

            mergeField.Merge(e);
        }
    }
}
