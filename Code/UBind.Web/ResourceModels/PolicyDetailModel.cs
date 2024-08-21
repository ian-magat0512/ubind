// <copyright file="PolicyDetailModel.cs" company="uBind">
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
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Policy;

    /// <summary>
    /// Resource model for serving the details of a policy.
    /// </summary>
    public class PolicyDetailModel : IAdditionalPropertyValues
    {
        private PolicyDetailsType type;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyDetailModel"/> class.
        /// </summary>
        /// <param name="policyDetails">The details of the policy.</param>
        /// <param name="clock">The clock use when calculating the policy status.</param>
        /// <param name="fieldDto">The list of displayable fields.</param>
        /// <param name="type">The details type.</param>
        /// <param name="formDataSchema">The form data schema.</param>
        /// <param name="formDataPrettifier">The form data prettifier.</param>
        /// <param name="additionalPropertyValueDtos">Additional property value dtos.</param>
        public PolicyDetailModel(
            IPolicyReadModelDetails policyDetails,
            IClock clock,
            DisplayableFieldDto fieldDto,
            PolicyDetailsType type,
            IFormDataSchema formDataSchema,
            IFormDataPrettifier formDataPrettifier,
            List<AdditionalPropertyValueDto> additionalPropertyValueDtos)
        {
            this.Id = policyDetails.PolicyId;
            this.IsTestData = policyDetails.IsTestData;
            this.OrganisationId = policyDetails.OrganisationId;
            this.OrganisationName = policyDetails.OrganisationName;
            this.type = type;
            this.ProductName = policyDetails.ProductName;
            this.ProductId = policyDetails.ProductId;
            this.QuoteId = policyDetails.QuoteId;
            this.TenantId = policyDetails.TenantId;
            this.QuoteNumber = policyDetails.QuoteNumber;
            this.QuoteOwnerUserId = policyDetails.QuoteOwnerUserId;
            this.PolicyNumber = policyDetails.PolicyNumber;
            this.CreatedDateTime = policyDetails.IssuedTimestamp.ToExtendedIso8601String();
            this.LastModifiedDateTime = policyDetails.LastModifiedTimestamp.ToExtendedIso8601String();
            this.Owner = policyDetails.OwnerUserId.HasValue ? new UserSummaryModel(policyDetails.OwnerUserId.Value, policyDetails.OwnerFullName) : null;
            if (!string.IsNullOrEmpty(policyDetails.SerializedCalculationResult))
            {
                var calculationJson = JsonConvert.DeserializeObject<Domain.ReadWriteModel.CalculationResult>(
                    policyDetails.SerializedCalculationResult, CustomSerializerSetting.JsonSerializerSettings).Json;

                this.CalculationResult = new CalculationResultDetailModel(default, default, calculationJson);
            }

            if (policyDetails.CustomerId.HasValue)
            {
                this.Customer = new CustomerSimpleModel(policyDetails.CustomerId.Value, policyDetails.CustomerFullName, policyDetails.CustomerOwnerUserId);
            }

            var productContext = new ProductContext(
                policyDetails.TenantId,
                policyDetails.ProductId,
                policyDetails.Environment);
            var formData = formDataSchema != null && policyDetails.LatestFormData != null
                ? formDataPrettifier.GetPrettifiedFormData(
                    formDataSchema.GetQuestionMetaData(),
                    policyDetails.LatestFormData,
                    policyDetails.PolicyId,
                    "Policy",
                    productContext)
                : JObject.Parse(policyDetails.LatestFormData);

            this.FormData = formData ?? new JObject();
            this.NumberOfDaysToExpire = policyDetails.IsTermBased ? policyDetails.GetDaysToExpire(clock.Today()) : (int?)null;
            this.InceptionDateTime = policyDetails.InceptionTimestamp.ToExtendedIso8601String();
            this.CancellationEffectiveDateTime = policyDetails.CancellationEffectiveTimestamp?.ToExtendedIso8601String();
            this.ExpiryDateTime = policyDetails.ExpiryTimestamp?.ToExtendedIso8601String();
            this.Status = policyDetails.GetDetailStatus(
                policyDetails.AreTimestampsAuthoritative, policyDetails.TimeZone, clock.Now());
            var displayTransaction = policyDetails.GetDisplayTransaction(
                policyDetails.AreTimestampsAuthoritative, policyDetails.TimeZone, clock.Now());
            var currentTransaction = policyDetails.GetCurrentTransaction(
                policyDetails.AreTimestampsAuthoritative, policyDetails.TimeZone, clock.Now());
            this.EventTypeSummary = displayTransaction.GetEventTypeSummary();
            if (displayTransaction.PolicyData != null)
            {
                if (type == PolicyDetailsType.Premium)
                {
                    this.PremiumData = new PremiumResult(displayTransaction.PolicyData.CalculationResult.PayablePrice);
                }

                if (type == PolicyDetailsType.Questions)
                {
                    this.Questions = JsonConvert.DeserializeObject(displayTransaction.PolicyData.CalculationResult.Questions, CustomSerializerSetting.JsonSerializerSettings);
                    this.DisplayableFieldsModel = new DisplayableFieldsModel(fieldDto);
                }
            }

            if (type == PolicyDetailsType.Documents)
            {
                this.Documents = policyDetails.Documents
                    .Select(document => new DocumentSetModel(document));
            }

            var transactions = policyDetails.Transactions.Select(x => x.PolicyTransaction);
            PolicyTransaction pendingTransaction = transactions
                .OfType<PolicyTransaction>()
                .Where(t => t.EffectiveTimestamp > clock.Now())
                .Where(t => t.GetType().Name != typeof(NewBusinessTransaction).Name)
                .OrderBy(t => t.EffectiveTimestamp)
                .FirstOrDefault();

            this.HasFutureTransaction = pendingTransaction != null;
            this.FutureTransactionDateTime = pendingTransaction?.EffectiveTimestamp.ToExtendedIso8601String() ?? string.Empty;
            this.FutureTransactionType = pendingTransaction?.GetType().Name ?? string.Empty;
            this.FutureTransactionId = pendingTransaction?.Id ?? default;
            this.HasClaimConfiguration = false;
            this.AdditionalPropertyValues = additionalPropertyValueDtos != null && additionalPropertyValueDtos.Any() ?
                additionalPropertyValueDtos.Select(apv => new AdditionalPropertyValueModel(apv)).ToList() :
                new List<AdditionalPropertyValueModel>();
        }

        /// <summary>
        /// Gets or sets the policy ID.
        /// </summary>
        [JsonProperty]
        public Guid Id { get; set; }

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
        /// Gets or sets the reference ID of the quote the policy is issued for.
        /// </summary>
        [JsonProperty]
        public Guid? QuoteId { get; set; }

        /// <summary>
        /// Gets or sets quotes owner user id.
        /// </summary>
        [JsonProperty]
        public Guid? QuoteOwnerUserId { get; set; }

        /// <summary>
        /// Gets or sets the unique policy number.
        /// </summary>
        [JsonProperty]
        public string PolicyNumber { get; set; }

        /// <summary>
        /// Gets or sets the name of the product the policy is issued for.
        /// </summary>
        [JsonProperty]
        public string ProductName { get; set; }

        /// <summary>
        /// Gets or sets the id of the product the policy is issued for.
        /// </summary>
        [JsonProperty]
        public Guid ProductId { get; set; }

        /// <summary>
        /// Gets or sets the current status of the policy.
        /// </summary>
        [JsonProperty]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the event summary type.
        /// </summary>
        [JsonProperty]
        public string EventTypeSummary { get; set; }

        /// <summary>
        /// Gets or sets the Tenant Id.
        /// </summary>
        [JsonProperty]
        public Guid TenantId { get; set; }

        /// <summary>
        /// Gets or sets the number of days to expire.
        /// </summary>
        [JsonProperty]
        public int? NumberOfDaysToExpire { get; set; }

        /// <summary>
        /// Gets or sets the quote number the policy is for.
        /// </summary>
        [JsonProperty]
        public string QuoteNumber { get; set; }

        /// <summary>
        /// Gets or sets the date the policy is issued/created.
        /// </summary>
        [JsonProperty]
        public string CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date the policy was modified.
        /// </summary>
        [JsonProperty]
        public string LastModifiedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the effectivity date for the policy.
        /// </summary>
        [JsonProperty]
        public string InceptionDateTime { get; set; }

        /// <summary>
        /// Gets or sets the cancellation date for the policy.
        /// </summary>
        [JsonProperty]
        public string CancellationEffectiveDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date of expiry for the policy.
        /// </summary>
        [JsonProperty]
        public string ExpiryDateTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is test data flag is true or false.
        /// </summary>
        [JsonProperty]
        public bool IsTestData { get; set; }

        /// <summary>
        /// Gets or sets the summary details of the referrer/owner of the quote/policy.
        /// </summary>
        [JsonProperty]
        public UserSummaryModel Owner { get; set; }

        /// <summary>
        /// Gets or sets the calculation result the policy is based on.
        /// </summary>
        [JsonIgnore]
        [JsonProperty]
        public CalculationResultDetailModel CalculationResult { get; set; }

        /// <summary>
        /// Gets or sets the customer for whom the policy is for.
        /// </summary>
        [JsonProperty]
        public CustomerSimpleModel Customer { get; set; }

        /// <summary>
        /// Gets or sets the latest payment calculation data for the quite.
        /// </summary>
        [JsonProperty]
        public PremiumResult PremiumData { get; set; }

        /// <summary>
        /// Gets the current documents collection for the policy.
        /// </summary>
        [JsonProperty]
        public IEnumerable<DocumentSetModel> Documents { get; private set; }

        /// <summary>
        /// Gets the formData.
        /// </summary>
        [JsonProperty]
        public object FormData { get; private set; }

        /// <summary>
        /// Gets or sets Displayable Field model.
        /// </summary>
        [JsonProperty]
        public DisplayableFieldsModel DisplayableFieldsModel { get; set; }

        /// <summary>
        /// Gets or sets Displayable Field model.
        /// </summary>
        [JsonProperty]
        public object Questions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this Policy has a future transaction.
        /// </summary>
        [JsonProperty]
        public bool HasFutureTransaction { get; set; }

        /// <summary>
        /// Gets or sets the date of a future transaction.
        /// </summary>
        [JsonProperty]
        public string FutureTransactionDateTime { get; set; }

        /// <summary>
        /// Gets or sets the quote ID of a future transaction.
        /// </summary>
        [JsonProperty]
        public Guid FutureTransactionId { get; set; }

        /// <summary>
        /// Gets or sets the type of the future transaction.
        /// </summary>
        [JsonProperty]
        public string FutureTransactionType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a claim configuration exists or not.
        /// </summary>
        public bool HasClaimConfiguration { get; set; }

        /// <inheritdoc/>
        [JsonProperty]
        public List<AdditionalPropertyValueModel> AdditionalPropertyValues { get; }

        /// <summary>
        /// Include PremiumResult if type is 'Premium'.
        /// </summary>
        /// <returns>if included.</returns>
        public bool ShouldSerializePremiumData()
        {
            return this.type == PolicyDetailsType.Premium;
        }

        /// <summary>
        /// Include PremiumResult if type is 'Claims'.
        /// </summary>
        /// <returns>if included.</returns>
        public bool ShouldSerializeClaims()
        {
            return this.type == PolicyDetailsType.Claims;
        }

        /// <summary>
        /// Include PremiumResult if type is 'Questions'.
        /// </summary>
        /// <returns>if included.</returns>
        public bool ShouldSerializeQuestions()
        {
            return this.type == PolicyDetailsType.Questions;
        }

        /// <summary>
        /// Include PremiumResult if type is 'Questions'.
        /// </summary>
        /// <returns>if included.</returns>
        public bool ShouldSerializeDisplayableFieldsModel()
        {
            return this.type == PolicyDetailsType.Questions;
        }

        /// <summary>
        /// Include PremiumResult if type is 'Questions'.
        /// </summary>
        /// <returns>if included.</returns>
        public bool ShouldSerializeDocuments()
        {
            return this.type == PolicyDetailsType.Documents;
        }
    }
}
