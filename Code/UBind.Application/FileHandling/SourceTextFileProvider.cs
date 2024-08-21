// <copyright file="SourceTextFileProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling
{
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using MimeKit;
    using UBind.Application.Export;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions.Domain;

    /// <summary>
    /// Source text file provider.
    /// </summary>
    public class SourceTextFileProvider : IAttachmentProvider
    {
        private readonly ITextProvider sourceTextProvider;
        private readonly ITextProvider outputFileName;
        private readonly ITextProvider mimeType;
        private readonly EventExporterCondition condition;
        private readonly IFileContentsLoader fileContentLoader;

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceTextFileProvider"/> class.
        /// </summary>
        /// <param name="sourceTextProvider">Factory for generating source text.</param>
        /// <param name="outputFileName">The output file name.</param>
        /// <param name="mimeType">The mime type of file.</param>
        /// <param name="condition">The condition of the file whether to include in attachments or not.</param>
        /// <param name="fileContentLoader">For loading file contents.</param>
        public SourceTextFileProvider(
            ITextProvider sourceTextProvider,
            ITextProvider outputFileName,
            ITextProvider mimeType,
            EventExporterCondition condition,
            IFileContentsLoader fileContentLoader)
        {
            Contract.Assert(sourceTextProvider != null);
            Contract.Assert(outputFileName != null);
            Contract.Assert(mimeType != null);
            Contract.Assert(fileContentLoader != null);

            this.sourceTextProvider = sourceTextProvider;
            this.outputFileName = outputFileName;
            this.mimeType = mimeType;
            this.condition = condition;
            this.fileContentLoader = fileContentLoader;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceTextFileProvider"/> class.
        /// Force to use the parameter version of static file provider.
        /// </summary>
        private SourceTextFileProvider()
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
            var sourceText = await this.sourceTextProvider.Invoke(applicationEvent);
            if (sourceText == null)
            {
                throw new ErrorException(Errors.File.NotFound("inline", "text provider"));
            }

            var outputFilename = await this.outputFileName.Invoke(applicationEvent);
            var mimeType = await this.mimeType.Invoke(applicationEvent);
            ContentType contentType = ContentType.Parse(mimeType);
            byte[] byteArray = Encoding.ASCII.GetBytes(sourceText);
            MemoryStream stream = new MemoryStream(byteArray);
            var attachment = MimeEntity.Load(contentType, stream);
            attachment = attachment.ResolveAttachment(outputFilename, byteArray);
            return attachment;
        }
    }
}
