// <copyright file="PolicyTransactionModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Dto;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Policy;

    /// <summary>
    /// For serving policy transaction details.
    /// </summary>
    public class PolicyTransactionModel : PolicyTransactionDetailsModel, IAdditionalPropertyValues
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyTransactionModel"/> class.
        /// </summary>
        /// <param name="policyDetails">The details of the policy the transaction belongs to.</param>
        /// <param name="transaction">The upsert transaction.</param>
        /// <param name="quote">The transactions quote.</param>
        /// <param name="displayableFields">Specification of displayable fields for the questions.</param>
        /// <param name="time">The current time.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <param name="formDataSchema">The form data schema.</param>
        /// <param name="formDataPrettifier">The form data prettifier.</param>
        /// <param name="additionalPropertyValueDtos">The additional property values.</param>
        public PolicyTransactionModel(
            IPolicyReadModelDetails policyDetails,
            PolicyTransaction transaction,
            NewQuoteReadModel quote,
            DisplayableFieldDto displayableFields,
            Instant time,
            DateTimeZone timeZone,
            IFormDataSchema formDataSchema,
            IFormDataPrettifier formDataPrettifier,
            List<AdditionalPropertyValueDto> additionalPropertyValueDtos,
            string productAlias,
            ReleaseBase? release)
             : base(policyDetails, transaction, quote, time, timeZone, productAlias, release)
        {
            this.ProductId = policyDetails.ProductId;
            this.OrganisationId = policyDetails.OrganisationId;
            this.OrganisationName = policyDetails.OrganisationName;
            this.PolicyId = transaction.PolicyId;
            this.ExpiryDateTime = transaction.ExpiryTimestamp.ToString();

            // Prettify Questions
            var productContext = new ProductContext(policyDetails.TenantId, policyDetails.ProductId, policyDetails.Environment);

            // Obtain questions from transaction policy data, otherwise, from policy detail latest form data.
            var questionData = formDataSchema != null && transaction.PolicyData.FormData != null
                ? formDataPrettifier.GetPrettifiedFormData(
                    formDataSchema.GetQuestionMetaData(),
                    transaction.PolicyData.FormData,
                    policyDetails.PolicyId,
                    "Policy",
                    productContext)
                : JObject.Parse(policyDetails.LatestFormData);

            this.Questions = questionData;

            this.Premium = new PremiumResult(transaction.PolicyData.CalculationResult.PayablePrice);
            this.DisplayableFields = new DisplayableFieldsModel(displayableFields);
            this.FormData = questionData;

            this.CancellationEffectiveDateTime = policyDetails.CancellationEffectiveTimestamp.ToString();

            this.AdditionalPropertyValues =
                (additionalPropertyValueDtos != null && additionalPropertyValueDtos.Any()) ?
                additionalPropertyValueDtos.Select(
                    apv => new AdditionalPropertyValueModel(apv)).ToList() :
                new List<AdditionalPropertyValueModel>();
        }

        [JsonProperty]
        public Guid ProductId { get; }

        /// <summary>
        /// Gets or sets the organisation ID.
        /// </summary>
        [JsonProperty]
        public Guid OrganisationId { get; set; }

        /// <summary>
        /// Gets or sets the organisation name.
        /// </summary>
        [JsonProperty]
        public string OrganisationName { get; set; }

        /// <summary>
        /// Gets the cancellation date of the policy specified in the transaction.
        /// </summary>
        [JsonProperty]
        public string CancellationEffectiveDateTime { get; private set; }

        /// <summary>
        /// Gets the questions from the calculation json.
        /// </summary>
        [JsonProperty]
        public object Questions { get; private set; }

        /// <summary>
        /// Gets or sets the displayable fields.
        /// </summary>
        [JsonProperty]
        public DisplayableFieldsModel DisplayableFields { get; set; }

        /// <summary>
        /// Gets the payable price for the transaction.
        /// </summary>
        [JsonProperty]
        public PremiumResult Premium { get; private set; }

        /// <summary>
        /// Gets the formdata of the transaction.
        /// </summary>
        [JsonProperty]
        public object FormData { get; private set; }

        /// <summary>
        /// Gets or sets the question key names that have the attachment data type.
        /// </summary>
        public string[] QuestionAttachmentKeys { get; set; }

        /// <inheritdoc/>
        [JsonProperty]
        public List<AdditionalPropertyValueModel> AdditionalPropertyValues { get; private set; }
    }
}
