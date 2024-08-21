// <copyright file="UploadedFileProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling
{
    using System;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using MimeKit;
    using UBind.Application.Export;
    using UBind.Application.Helpers;
    using UBind.Domain;
    using UBind.Domain.Entities;
    using UBind.Domain.Extensions;
    using UBind.Domain.Extensions.Domain;

    /// <summary>
    /// Uploaded file provider.
    /// </summary>
    public class UploadedFileProvider : IAttachmentProvider
    {
        private readonly ITextProvider fieldName;
        private readonly ITextProvider outputFileName;
        private readonly EventExporterCondition condition;
        private readonly IFileAttachmentRepository<QuoteFileAttachment> fileAttachmentRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="UploadedFileProvider"/> class.
        /// </summary>
        /// <param name="type">The type of field to which the file is to be taken.</param>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="condition">The condition of the file whether to include in attachments or not.</param>
        /// <param name="outputFileName">The name of the file to be used as attachment. Optional.</param>
        /// <param name="fileAttachmentRepository">The file attachment repository.</param>
        public UploadedFileProvider(
            ITextProvider type,
            ITextProvider fieldName,
            EventExporterCondition condition,
            ITextProvider outputFileName,
            IFileAttachmentRepository<QuoteFileAttachment> fileAttachmentRepository)
        {
            Contract.Assert(type != null);
            Contract.Assert(fieldName != null);
            Contract.Assert(outputFileName != null);

            this.fieldName = fieldName;
            this.condition = condition;
            this.outputFileName = outputFileName;
            this.fileAttachmentRepository = fileAttachmentRepository;
        }

        /// <inheritdoc />
        public async Task<bool> IsIncluded(ApplicationEvent applicationEvent)
        {
            return this.condition != null
                ? await this.condition.Evaluate(applicationEvent)
                : true;
        }

        /// <inheritdoc/>
        public async Task<MimeEntity> Invoke(ApplicationEvent applicationEvent)
        {
            var file = await this.GetFileAttachment(applicationEvent);
            if (file == null)
            {
                return null;
            }

            var attachmentContent
                = this.fileAttachmentRepository.GetAttachmentContent(file.TenantId, file.Id);
            if (attachmentContent.HasNoValue)
            {
                // we do allow a null attachment here because if the file was never uploaded in the form we just don't attach it.
                // TODO: in future we should aim to ensure that the integrations/automations json has a condition to check if the file was uploaded, and if not, it skips.
                // if that behaviour was implemented/enforced, then we could actually raise an error here instead. For now we have no choice but to return a null value
                // which is not great, because then we need null handling for all recipients of file providers.
                return null;
            }

            var content = attachmentContent.Value.FileContent;
            var fileName = await this.outputFileName.Invoke(applicationEvent);
            var outputName = fileName.IsNullOrEmpty() ?
                file.Name : fileName.Contains('*') ?
                string.Concat(fileName.AsSpan(0, fileName.LastIndexOf('.')), file.Name.AsSpan(file.Name.LastIndexOf('.'))) : fileName;
            var mimeType = ContentTypeHelper.GetMimeTypeForFileExtension(fileName);
            ContentType contentType = ContentType.Parse(mimeType);
            var attachment = MimeEntity.Load(contentType, new MemoryStream(content));
            attachment = attachment.ResolveAttachment(outputName, content);

            return attachment;
        }

        private async Task<QuoteFileAttachment> GetFileAttachment(ApplicationEvent applicationEvent)
        {
            var fieldName = await this.fieldName.Invoke(applicationEvent);
            var quote = applicationEvent.Aggregate.GetQuoteOrThrow(applicationEvent.QuoteId);
            var fieldValue = quote.LatestFormData.Data.GetValue(fieldName);
            if (fieldValue.IsNullOrEmpty())
            {
                return null;
            }

            var fieldValueSegments = fieldValue.Split(':');
            var fileIdInString = fieldValueSegments.Length >= 3 ? fieldValueSegments[2] : default;
            if (Guid.TryParse(fileIdInString, out Guid fileId))
            {
                var file = quote.QuoteFileAttachments.FirstOrDefault(x => x.Id == fileId);
                if (file != null)
                {
                    return file;
                }
            }

            return null;
        }
    }
}
