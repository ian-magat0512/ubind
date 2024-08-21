// <copyright file="FormModelTemplateJObjectProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling.Template_Provider
{
    using System.Linq;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using UBind.Domain;
    using UBind.Domain.Entities;
    using UBind.Domain.Product;

    /// <summary>
    /// Form model template JSON object provider.
    /// </summary>
    public class FormModelTemplateJObjectProvider : IJObjectProvider
    {
        private readonly IFormDataPrettifier formDataPrettifier;
        private readonly IFormDataSchema formDataSchema;
        private readonly IFileAttachmentRepository<QuoteFileAttachment> fileAttachmentRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormModelTemplateJObjectProvider"/> class.
        /// </summary>
        /// <param name="formDataPrettifier">The form data prettifier.</param>
        /// <param name="formDataSchema">The form data schema.</param>
        /// <param name="fileAttachmentRepository">The file attachment repository.</param>
        public FormModelTemplateJObjectProvider(
            IFormDataPrettifier formDataPrettifier,
            IFormDataSchema formDataSchema,
            IFileAttachmentRepository<QuoteFileAttachment> fileAttachmentRepository)
        {
            this.formDataPrettifier = formDataPrettifier;
            this.formDataSchema = formDataSchema;
            this.fileAttachmentRepository = fileAttachmentRepository;
        }

        /// <inheritdoc/>
        public JObject JsonObject { get; private set; } = new JObject();

        /// <inheritdoc/>
        public Task CreateJsonObject(ApplicationEvent applicationEvent)
        {
            var quote = applicationEvent.Aggregate.GetQuoteOrThrow(applicationEvent.QuoteId);
            var formModelJObject = quote.LatestFormData?.Data.FormModel;
            var jObject = formModelJObject;
            if (this.formDataSchema != null && formModelJObject != null)
            {
                var entityId = applicationEvent.Aggregate.Id;
                var entityType = applicationEvent.Aggregate.GetType().ToString();
                var productContext = new ReleaseContext(
                    applicationEvent.Aggregate.TenantId,
                    applicationEvent.Aggregate.ProductId,
                    applicationEvent.Aggregate.Environment,
                    applicationEvent.ProductReleaseId);

                // clone the JObject so we don't change the original data when we prettify it.
                jObject = new JObject(formModelJObject);
                this.formDataPrettifier.Prettify(
                    this.formDataSchema.GetQuestionMetaData(),
                    jObject,
                    entityId,
                    entityType,
                    productContext);
            }

            IJsonObjectParser parser = new GenericJObjectParser(
                string.Empty,
                jObject);
            if (parser.JsonObject == null)
            {
                return Task.CompletedTask;
            }

            var jObjects = parser.JsonObject;
            foreach (var obj in jObjects)
            {
                JToken value = obj.Value;
                var includedAttachments = quote.QuoteFileAttachments
                    .Where(a => value.ToString().Contains(a.Id.ToString()));

                foreach (var attachment in includedAttachments)
                {
                    var file =
                        this.fileAttachmentRepository.GetAttachmentContent(attachment.TenantId, attachment.Id);
                    if (file.HasValue)
                    {
                        var newValue = value.ToString().Replace(
                            attachment.Id.ToString(), file.Value.GetFileContentString());
                        value.Replace(JToken.FromObject(newValue));
                    }
                }
            }

            this.JsonObject = jObjects;
            return Task.CompletedTask;
        }
    }
}
