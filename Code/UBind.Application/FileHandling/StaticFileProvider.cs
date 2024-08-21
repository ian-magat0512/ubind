// <copyright file="StaticFileProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling
{
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Threading.Tasks;
    using MimeKit;
    using UBind.Application.Export;
    using UBind.Application.Helpers;
    using UBind.Domain;
    using UBind.Domain.Extensions.Domain;
    using UBind.Domain.Product;

    /// <summary>
    /// Static file provider.
    /// </summary>
    public class StaticFileProvider : IAttachmentProvider
    {
        private readonly ITextProvider templateName;
        private readonly ITextProvider outputFileName;
        private readonly IFileContentsLoader fileContentLoader;
        private readonly EventExporterCondition condition;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="StaticFileProvider"/> class.
        /// </summary>
        /// <param name="templateName">The name of the template to use.</param>
        /// <param name="outputFileName">The name of the output file.</param>
        /// <param name="condition">The condition of the file whether to include in attachments or not.</param>
        /// <param name="fileContentLoader">For loading file contents.</param>
        public StaticFileProvider(
            ITextProvider templateName,
            ITextProvider outputFileName,
            EventExporterCondition condition,
            IFileContentsLoader fileContentLoader)
        {
            Contract.Assert(templateName != null);
            Contract.Assert(outputFileName != null);
            Contract.Assert(fileContentLoader != null);

            this.templateName = templateName;
            this.outputFileName = outputFileName;
            this.condition = condition;
            this.fileContentLoader = fileContentLoader;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticFileProvider"/> class.
        /// Force to use the parameter version of static file provider.
        /// </summary>
        private StaticFileProvider()
        {
            // Nothing to do
        }

        /// <inheritdoc />
        public async Task<bool> IsIncluded(ApplicationEvent applicationEvent)
        {
            return this.condition != null
                ? await this.condition.Evaluate(applicationEvent)
                : true;
        }

        /// <inheritdoc />
        public async Task<MimeEntity> Invoke(ApplicationEvent applicationEvent)
        {
            var templateName = await this.templateName.Invoke(applicationEvent);
            var outputFilename = await this.outputFileName.Invoke(applicationEvent);
            var mimeType = ContentTypeHelper.GetMimeTypeForFileExtension(outputFilename);
            ContentType contentType = ContentType.Parse(mimeType);
            var content = await this.fileContentLoader.LoadData(
                        new ReleaseContext(
                            applicationEvent.Aggregate.TenantId,
                            applicationEvent.Aggregate.ProductId,
                            applicationEvent.Aggregate.Environment,
                            applicationEvent.ProductReleaseId),
                        templateName);
            var attachment = MimeEntity.Load(contentType, new MemoryStream(content));
            attachment = attachment.ResolveAttachment(outputFilename, content);
            return attachment;
        }
    }
}
