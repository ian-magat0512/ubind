// <copyright file="QuoteDetailModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Quote
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Domain;
    using UBind.Domain.Dto;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.Product.Component;
    using UBind.Domain.ReadModel;
    using UBind.Web.Helpers;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// Resource model for serving the details of a particular quote.
    /// </summary>
    public class QuoteDetailModel : IAdditionalPropertyValues
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteDetailModel"/> class.
        /// </summary>
        /// <param name="formDataPrettifier">The form data prettifier.</param>
        /// <param name="quoteDetails">The quote.</param>
        /// <param name="fieldDto">The list of displayable fields.</param>
        /// <param name="formDataSchema">The formdata schema.</param>
        /// <param name="additionalPropertyValueDtos">List of additional property value dto.</param>
        public QuoteDetailModel(
            IFormDataPrettifier formDataPrettifier,
            IQuoteReadModelDetails quoteDetails,
            DisplayableFieldDto fieldDto,
            FormDataSchema formDataSchema,
            List<AdditionalPropertyValueDto> additionalPropertyValueDtos,
            List<Trigger> triggerConfig = null)
        {
            this.Id = quoteDetails.QuoteId;
            this.OrganisationId = quoteDetails.OrganisationId;
            this.OrganisationName = quoteDetails.OrganisationName;
            this.PolicyId = quoteDetails.PolicyId;

            if (quoteDetails.CustomerId.HasValue)
            {
                this.CustomerDetails = new CustomerSimpleModel(
                    quoteDetails.CustomerId.Value, quoteDetails.CustomerFullName, quoteDetails.CustomerOwnerUserId);
            }

            this.PolicyOwnerUserId = quoteDetails.PolicyOwnerUserId;
            this.ProductName = quoteDetails.ProductName;
            this.ProductAlias = quoteDetails.ProductAlias;
            this.ProductId = quoteDetails.ProductId;
            this.TenantId = quoteDetails.TenantId;
            this.CreatedDateTime = quoteDetails.CreatedTimestamp.ToString();
            this.PolicyNumber = quoteDetails.PolicyNumber;
            this.QuoteNumber = quoteDetails.QuoteNumber;
            this.Status = quoteDetails.QuoteState;
            this.IsDiscarded = quoteDetails.IsDiscarded;

            var calculationResult = quoteDetails.LatestCalculationResult?.CalculationResult;
            var calculationJson = quoteDetails.LatestCalculationResult?.CalculationResult?.Json != null
                ? quoteDetails.LatestCalculationResult?.CalculationResult.Json
                : "{}";
            this.LatestCalculationData = new CalculationResultDetailModel(
                quoteDetails.LatestCalculationResultId,
                quoteDetails.LatestCalculationResultFormDataId,
                calculationJson,
                calculationResult,
                triggerConfig);

            this.Owner = quoteDetails.OwnerUserId.HasValue ? new UserSummaryModel(
                quoteDetails.OwnerUserId.Value, quoteDetails.OwnerFullName) : null;
            if (fieldDto != null)
            {
                this.DisplayableFieldsModel = new DisplayableFieldsModel(fieldDto);
            }

            this.LastModifiedDateTime = quoteDetails.LastModifiedTimestamp.ToExtendedIso8601String();
            if (quoteDetails.PolicyInceptionTimestamp.HasValue)
            {
                this.PolicyInceptionDateTime = quoteDetails.PolicyInceptionTimestamp.Value.ToExtendedIso8601String();
            }

            if (quoteDetails.PolicyExpiryTimestamp.HasValue)
            {
                this.PolicyExpiryDateTime = quoteDetails.PolicyExpiryTimestamp.Value.ToExtendedIso8601String();
            }

            if (quoteDetails.PolicyTransactionEffectiveTimestamp.HasValue)
            {
                this.PolicyTransactionEffectiveDateTime = quoteDetails.PolicyTransactionEffectiveTimestamp.Value.ToExtendedIso8601String();
            }

            if (quoteDetails.LastModifiedByUserTimestamp != null)
            {
                this.LastModifiedByUserDateTime = quoteDetails.LastModifiedByUserTimestamp.Value.ToExtendedIso8601String();
            }

            this.QuoteType = (int)quoteDetails.QuoteType;
            this.IsTestData = quoteDetails.IsTestData;

            dynamic formData = formDataSchema != null && quoteDetails.LatestFormData != null
                ? formDataPrettifier.GetPrettifiedFormData(
                    formDataSchema.GetQuestionMetaData(),
                    quoteDetails.LatestFormData,
                    quoteDetails.QuoteId,
                    "Quote",
                    new ProductContext(quoteDetails.TenantId, quoteDetails.ProductId, quoteDetails.Environment))
                : quoteDetails.LatestFormData != null
                ? (dynamic)quoteDetails.LatestFormData
                : null;

            dynamic formDataObject = formData ?? new JObject();

            this.FormData = formDataObject?.formModel;
            dynamic calculationResultObject = JsonConvert.DeserializeObject(calculationJson ?? string.Empty, CustomSerializerSetting.JsonSerializerSettings);
            this.QuestionData = calculationResultObject?.questions;
            this.PremiumData = new PremiumResult(quoteDetails.LatestCalculationResult.PayablePrice);
            this.ExpiryDateTime = quoteDetails?.ExpiryTimestamp?.ToString();
            this.QuestionAttachmentKeys = FormDataHelper.GetQuestionAttachmentFieldPaths(formDataSchema, this.FormData);
            if (quoteDetails.Documents != null)
            {
                // Dev Note: This is just a temporary fix and should have a proper document management in the future.
                // For more information, please see GetGroupedDocumentsFromQuoteDetailsInTicksDifference's remarks.
                this.Documents
                    = this.GetGroupedDocumentsFromQuoteDetailsInTicksDifference(quoteDetails, TimeSpan.TicksPerMinute);
            }

            this.AdditionalPropertyValues = additionalPropertyValueDtos != null && additionalPropertyValueDtos.Any() ?
                additionalPropertyValueDtos.Select(apv => new AdditionalPropertyValueModel(apv)).ToList() :
                new List<AdditionalPropertyValueModel>();

            this.ProductReleaseId = quoteDetails.ProductReleaseId;
            if (quoteDetails.ProductReleaseMajorNumber != null && quoteDetails.ProductReleaseMinorNumber != null)
            {
                this.ProductReleaseNumber = quoteDetails.ProductReleaseMajorNumber + "." + quoteDetails.ProductReleaseMinorNumber;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteDetailModel"/> class.
        /// </summary>
        [JsonConstructor]
        public QuoteDetailModel()
        {
        }

        /// <summary>
        /// Gets or sets the reference Id for the quote.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the policy the quote relates to.
        /// </summary>
        /// <remarks>
        /// This is also the ID of the aggregate the quote belongs to.
        /// .</remarks>
        public Guid? PolicyId { get; protected set; }

        /// <summary>
        /// Gets or sets the quote number.
        /// </summary>
        public string QuoteNumber { get; set; }

        /// <summary>
        /// Gets or sets the details of the quote customer.
        /// </summary>
        [JsonProperty]
        public CustomerSimpleModel CustomerDetails { get; set; }

        /// <summary>
        /// Gets or sets the name of the product the quote is for.
        /// </summary>
        public string ProductName { get; set; }

        public string ProductAlias { get; set; }

        /// <summary>
        /// Gets or sets the Product Id the quote is created from.
        /// </summary>
        public Guid ProductId { get; protected set; }

        /// <summary>
        /// Gets or sets the tenant ID.
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Gets or sets the current status of the quote.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the policy number assigned to quote, if any, otherwise null.
        /// </summary>
        public string PolicyNumber { get; set; }

        /// <summary>
        /// Gets or sets the polcies owner user id.
        /// </summary>
        public Guid? PolicyOwnerUserId { get; set; }

        /// <summary>
        /// Gets or sets the latest calculation data for the quote, if any, otherwise null.
        /// </summary>
        public CalculationResultDetailModel LatestCalculationData { get; set; }

        /// <summary>
        /// Gets or sets the latest payment calculation data for the quote.
        /// </summary>
        public PremiumResult PremiumData { get; set; }

        /// <summary>
        /// Gets or sets the created date and time in ISO8601 format.
        /// </summary>
        public string CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the expiry date and time in ISO8601 format.
        /// </summary>
        public string ExpiryDateTime { get; set; }

        /// <summary>
        /// Gets or sets the last modified date and time in ISO8601 format.
        /// </summary>
        public string LastModifiedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the policy expiry date and time in ISO8601 format.
        /// </summary>
        public string PolicyInceptionDateTime { get; set; }

        /// <summary>
        /// Gets or sets the policy expiry date and time in ISO8601 format.
        /// </summary>
        public string PolicyExpiryDateTime { get; set; }

        /// <summary>
        /// Gets or sets the policy transaction effective date and time in ISO8601 format.
        /// </summary>
        public string PolicyTransactionEffectiveDateTime { get; set; }

        /// <summary>
        /// Gets or sets the last modified date and time by user in ISO8601 format.
        /// </summary>
        public string LastModifiedByUserDateTime { get; set; }

        /// <summary>
        /// Gets or sets the summary details of the owner of the quote.
        /// </summary>
        public UserSummaryModel Owner { get; set; }

        /// <summary>
        /// Gets or sets the quote type.
        /// </summary>
        public int QuoteType { get; set; }

        /// <summary>
        /// Gets or sets Displayable Field model.
        /// </summary>
        public DisplayableFieldsModel DisplayableFieldsModel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the data is for testing.
        /// </summary>
        public bool IsTestData { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the quote is discarded.
        /// </summary>
        public bool IsDiscarded { get; set; }

        /// <summary>
        /// Gets or sets formData.
        /// </summary>
        [JsonProperty]
        public dynamic FormData { get; set; }

        /// <summary>
        /// Gets or sets the questionData.
        /// </summary>
        public object QuestionData { get; set; }

        /// <summary>
        /// Gets or sets the current documents collection for the policy.
        /// </summary>
        [JsonProperty]
        public IEnumerable<DocumentSetModel> Documents { get; protected set; } = new List<DocumentSetModel>();

        /// <summary>
        /// Gets or sets the question attachment key names that have the attachment data type.
        /// </summary>
        public IEnumerable<string> QuestionAttachmentKeys { get; set; }

        /// <summary>
        /// Gets or sets the ID of the organisation this quote was created under.
        /// </summary>
        public Guid OrganisationId { get; protected set; }

        /// <summary>
        /// Gets or sets the name of the organisation this quote was created under.
        /// </summary>
        public string OrganisationName { get; protected set; }

        /// <inheritdoc/>
        [JsonProperty]
        public List<AdditionalPropertyValueModel> AdditionalPropertyValues { get; protected set; }

        public Guid? ProductReleaseId { get; protected set; }

        public string ProductReleaseNumber { get; protected set; }

        /// <summary>
        /// Group the documents that have the same file name, file size, and just the latest within the given time
        /// difference (one minute by default).
        /// </summary>
        /// <remarks>
        /// First of all, this method was just created for a temporary fix.
        ///
        /// What does it fix?
        ///         We have automation behaviour in document generation that is causing us problems. It creates a new
        ///     record every time a file is attached to an entity. With this, it is possible to make a duplicate set
        ///     of records in a single action.
        ///         In some of our products, it resulted in duplicate displays in the portal app that is confusing
        ///     because they have the same content generated in a single operation. So the goal of this fix is just to
        ///     make it look good in the portal, that’s all. This is reported in UB-7156 and to know more about the
        ///     base of this problem, see UB-5987.
        ///
        /// What's the temporary fix?
        ///         To avoid duplicate documents displayed in the application, the quick solution is just to group the
        ///     quote detail documents by their file name, file size and should be the latest within the time range.
        ///
        /// What's the proposed solution?
        ///         We might have a document aggregate soon to have a centralised file attachments and implement
        ///     versioning and relationships. The goal for this is to have a single document to handle everything
        ///     related to quotes just like the email attachments. Next, is to have a large migration that will clean
        ///     up the existing file attachments. This way, we can just reference the created documents to these
        ///     automation actions.
        /// </remarks>
        /// <param name="quoteDetails">The quote read model details.</param>
        /// <param name="ticks">The time difference.</param>
        /// <returns>The list of document set model.</returns>
        private List<DocumentSetModel> GetGroupedDocumentsFromQuoteDetailsInTicksDifference(
            IQuoteReadModelDetails quoteDetails, long ticks = TimeSpan.TicksPerMinute)
        {
            var groupedDocuments = quoteDetails.Documents.Select(d => new DocumentSetModel(d))
                .GroupBy(doc => new { doc.FileName, doc.FileSize });

            var documents = new List<DocumentSetModel>();
            foreach (var documentSet in groupedDocuments)
            {
                DocumentSetModel previousDocument = null;
                foreach (var document in documentSet.OrderByDescending(d => d.CreatedDateTime))
                {
                    if (previousDocument == null)
                    {
                        documents.Add(document);
                    }
                    else
                    {
                        var createdDateDifferenceInTicks = Math.Abs(
                            Convert.ToDateTime(document.CreatedDateTime).Ticks
                            - Convert.ToDateTime(previousDocument.CreatedDateTime).Ticks);
                        if (createdDateDifferenceInTicks > ticks)
                        {
                            documents.Add(document);
                        }
                    }

                    previousDocument = document;
                }
            }

            return documents;
        }
    }
}
