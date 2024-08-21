// <copyright file="MsWordTemplateFileProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using MimeKit;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Export;
    using UBind.Application.Export.ViewModels;
    using UBind.Application.FileHandling.Template_Provider;
    using UBind.Application.Helpers;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions.Domain;
    using UBind.Domain.Product;

    /// <summary>
    /// Microsoft Word template file provider.
    /// </summary>
    public class MsWordTemplateFileProvider
        : IAttachmentProvider
    {
        private const string MERGEFIELDKEY = "MERGEFIELD ";
        private readonly ITextProvider templateName;
        private readonly ITextProvider outputFileName;
        private readonly EventExporterCondition condition;
        private readonly IEnumerable<IJObjectProvider> templateProviders;
        private readonly IFileContentsLoader fileContentLoader;
        private readonly IMsWordEngineService wordEngineService;
        private ApplicationEvent applicationEvent;
        private JsonViewModel datasource;
        private byte[] buffer;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="MsWordTemplateFileProvider"/> class.
        /// </summary>
        /// <param name="templateName">The name of the template to use.</param>
        /// <param name="outputFileName">The name of the output file.</param>
        /// <param name="condition">The condition of the file whether to include in attachments or not.</param>
        /// <param name="templateProviders">JSON object template providers.</param>
        /// <param name="fileContentLoader">For loading file contents.</param>
        /// <param name="wordEngineService">Ms Word engine service.</param>
        public MsWordTemplateFileProvider(
            ITextProvider templateName,
            ITextProvider outputFileName,
            EventExporterCondition condition,
            IEnumerable<IJObjectProvider> templateProviders,
            IFileContentsLoader fileContentLoader,
            IMsWordEngineService wordEngineService)
        {
            Contract.Assert(templateName != null);
            Contract.Assert(outputFileName != null);
            Contract.Assert(templateProviders != null);
            Contract.Assert(fileContentLoader != null);
            Contract.Assert(wordEngineService != null);

            this.templateName = templateName;
            this.outputFileName = outputFileName;
            this.condition = condition;
            this.templateProviders = templateProviders;
            this.fileContentLoader = fileContentLoader;

            this.wordEngineService = wordEngineService;
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
            this.applicationEvent = applicationEvent;
            JsonViewModel jsonFormViewModel = new JsonViewModel(new JObject());
            foreach (IJObjectProvider provider in this.templateProviders)
            {
                await provider.CreateJsonObject(applicationEvent);
                jsonFormViewModel.Merge(provider.JsonObject);
            }

            this.datasource = jsonFormViewModel;
            MimeEntity resultAttachment = await this.GenerateDocumentForAttachment();
            return resultAttachment;
        }

        private async Task<string> GetTemplateSourceFile(JObject errorData)
        {
            try
            {
                var templateSourceFile = await this.templateName.Invoke(this.applicationEvent);
                errorData.Add("templateSourceFile", templateSourceFile);
                return templateSourceFile;
            }
            catch (Exception ex)
            {
                errorData.Add("errorMessage", ex.Message);
                throw new ErrorException(
                        Errors.Automation.RetrievalOfTemplateNameFailed(errorData, ex),
                        ex);
            }
        }

        private async Task<byte[]> MsWordEngineMergefieldSourceAsBytes(JObject errorData, string templateSourceFile)
        {
            try
            {
                // Get the content of the file based on the template name in file
                // content loader. Check 'Startup.cs' for fileContentLoader.
                this.buffer = await this.fileContentLoader.LoadData(
                    new ReleaseContext(
                        this.applicationEvent.Aggregate.TenantId,
                        this.applicationEvent.Aggregate.ProductId,
                        this.applicationEvent.Aggregate.Environment,
                        this.applicationEvent.ProductReleaseId),
                    templateSourceFile);
                return this.buffer;
            }
            catch (Exception ex)
            {
                errorData.Add("errorMessage", ex.Message);
                throw new ErrorException(
                        Errors.Automation.LoadingOfAssetContentFailed(errorData, templateSourceFile),
                        ex);
            }
        }

        private async Task<MimeEntity> GenerateDocumentForAttachment()
        {
            Contract.Assert(this != null);
            var errorData = new JObject
            {
                { "tenantId", this.applicationEvent.Aggregate.TenantId },
                { "productId", this.applicationEvent.Aggregate.ProductId },
                { "environment", this.applicationEvent.Aggregate.Environment.ToString() },
            };
            var templateSourceFile = await this.GetTemplateSourceFile(errorData);
            byte[] mergefieldSrc = await this.MsWordEngineMergefieldSourceAsBytes(errorData, templateSourceFile);
            var outputFilename = await this.outputFileName.Invoke(this.applicationEvent);
            byte[]? outputBytes = null;
            errorData.Add("outputFileName", outputFilename);
            outputBytes = this.wordEngineService.PopulateFieldsToTemplatedData(
                            templateSourceFile,
                            mergefieldSrc,
                            this.applicationEvent.Aggregate.Environment,
                            new PdfMsWordFileFormat(),
                            errorData,
                            (fieldCode, mergeFieldValueCasing) =>
                            {
                                return this.datasource.GetValue(fieldCode, mergeFieldValueCasing);
                            },
                            (fieldCode) =>
                            {
                                return this.CleanFieldCode(fieldCode);
                            });
            var mimeType = ContentTypeHelper.GetMimeTypeForFileExtension(outputFilename);
            ContentType contentType = ContentType.Parse(mimeType);
            var attachment = MimeEntity.Load(contentType, new MemoryStream(outputBytes));
            attachment = attachment.ResolveAttachment(outputFilename, outputBytes);
            return attachment;
        }

        /// <summary>
        /// Cleans the field code of any special appendixes which word puts in, e.g. "\\* MERGEFORMAT".
        /// </summary>
        /// <returns>The field code, cleaned.</returns>
        private string CleanFieldCode(string fieldCode)
        {
            fieldCode = fieldCode.Replace(MERGEFIELDKEY, string.Empty);
            fieldCode = Regex.Replace(fieldCode, @"(.*?)\s*\\\*.*", "$1").Trim();
            return fieldCode;
        }
    }
}
