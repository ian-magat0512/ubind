// <copyright file="ClaimDetailModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Claim
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using UBind.Domain;
    using UBind.Domain.Dto;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.ReadModel.User;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// Resource model for serving claim records.
    /// </summary>
    public class ClaimDetailModel : IAdditionalPropertyValues
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimDetailModel"/> class.
        /// </summary>
        /// <param name="formDataPrettifier">The form data prettifier.</param>
        /// <param name="claimDetail">The claim to be served.</param>
        /// <param name="formDataSchema">The formdata schema.</param>
        /// <param name="additionalPropertyValueDtos">Additional property value dtos.</param>
        /// <param name="policyOwner">The owner of the policy of the claim.</param>
        /// <param name="customer">The customer of the claim.</param>
        /// <param name="dto">The dto for displayable fields.</param>
        public ClaimDetailModel(
            IFormDataPrettifier formDataPrettifier,
            IClaimReadModelDetails claimDetail,
            FormDataSchema formDataSchema,
            List<AdditionalPropertyValueDto> additionalPropertyValueDtos,
            IUserReadModelSummary policyOwner = null,
            ICustomerReadModelSummary customer = null,
            DisplayableFieldDto dto = null)
        {
            this.OrganisationId = claimDetail.OrganisationId;
            this.OrganisationName = claimDetail.OrganisationName;
            this.ProductName = claimDetail.ProductName;
            this.Id = claimDetail.Id;
            if (claimDetail.CustomerId.HasValue)
            {
                this.CustomerDetails = new CustomerSimpleModel(claimDetail.CustomerId.Value, claimDetail.CustomerFullName, customer?.OwnerUserId);
            }

            this.ClaimReference = claimDetail.ClaimReference;
            this.PolicyNumber = claimDetail.PolicyNumber;
            this.PolicyOwnerUserId = policyOwner != null ? policyOwner.Id : default;
            this.Status = claimDetail.Status.ToString();
            this.Amount = claimDetail.Amount?.ToDollarsAndCents();
            this.Description = claimDetail.Description;
            this.IncidentDateTime = claimDetail.IncidentTimestamp?.ToExtendedIso8601String();
            this.CreatedDateTime = claimDetail.CreatedTimestamp.ToExtendedIso8601String();
            this.ClaimNumber = claimDetail.ClaimNumber;
            this.PolicyId = claimDetail.PolicyId;
            this.ProductId = claimDetail.ProductId;
            this.IsTestData = claimDetail.IsTestData;
            this.LastModifiedDateTime = claimDetail.LastModifiedTimestamp.ToExtendedIso8601String();
            var latestCalculationResultJson = claimDetail.LatestCalculationResult?.Json;
            this.CalculationResult = latestCalculationResultJson != null ? JsonConvert.DeserializeObject(latestCalculationResultJson, CustomSerializerSetting.JsonSerializerSettings) : null;

            var claimFormDataJson = claimDetail.GetFormData();
            var productContext = new ProductContext(claimDetail.TenantId, claimDetail.ProductId, claimDetail.Environment);
            dynamic formData = formDataSchema != null && claimFormDataJson != null
                ? formDataPrettifier.GetPrettifiedFormData(formDataSchema.GetQuestionMetaData(), claimFormDataJson, claimDetail.Id, "Claim", productContext)
                : (dynamic)claimFormDataJson;

            this.FormData = formData;
            this.OwnerUserId = policyOwner?.Id;
            this.OwnerName = policyOwner != null ? policyOwner.FullName : string.Empty;

            if (dto != null)
            {
                this.DisplayableFieldsModel = new DisplayableFieldsModel(dto);
            }

            this.Documents = claimDetail.Documents.Select(d => new DocumentSetModel(d)).ToList();

            this.AdditionalPropertyValues = additionalPropertyValueDtos != null && additionalPropertyValueDtos.Any() ?
                additionalPropertyValueDtos.Select(apv => new AdditionalPropertyValueModel(apv)).ToList() :
                new List<AdditionalPropertyValueModel>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimDetailModel"/> class.
        /// </summary>
        [JsonConstructor]
        public ClaimDetailModel()
        {
        }

        /// <summary>
        /// Gets the Id of the Claim record.
        /// </summary>
        [JsonProperty]
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the organisation ID.
        /// </summary>
        [JsonProperty]
        public Guid OrganisationId { get; private set; }

        /// <summary>
        /// Gets the organisation name.
        /// </summary>
        [JsonProperty]
        public string OrganisationName { get; private set; }

        /// <summary>
        /// Gets the string Id of product.
        /// </summary>
        [JsonProperty]
        public Guid ProductId { get; private set; }

        /// <summary>
        /// Gets the Id of the parent application.
        /// </summary>
        [JsonProperty]
        public Guid? PolicyId { get; private set; }

        /// <summary>
        /// Gets the details of the Claim customer.
        /// </summary>
        [JsonProperty]
        public CustomerSimpleModel CustomerDetails { get; private set; }

        /// <summary>
        /// Gets the name of the product the claim is for.
        /// </summary>
        [JsonProperty]
        public string ProductName { get; private set; }

        /// <summary>
        /// Gets the reference number of the claim.
        /// </summary>
        [JsonProperty]
        public string ClaimReference { get; private set; }

        /// <summary>
        /// Gets the claim number of the claim.
        /// </summary>
        [JsonProperty]
        public string ClaimNumber { get; private set; }

        /// <summary>
        /// Gets the policy number for the parent policy.
        /// </summary>
        public string PolicyNumber { get; private set; }

        /// <summary>
        /// Gets the policy owner user id.
        /// </summary>
        public Guid? PolicyOwnerUserId { get; private set; }

        /// <summary>
        /// Gets the current status of the claim.
        /// </summary>
        [JsonProperty]
        public string Status { get; private set; }

        /// <summary>
        /// Gets the current amount of the claim.
        /// </summary>
        [JsonProperty]
        public string Amount { get; private set; }

        /// <summary>
        /// Gets the current description of the claim.
        /// </summary>
        [JsonProperty]
        public string Description { get; private set; }

        /// <summary>
        /// Gets the current incident date of the claim.
        /// </summary>
        [JsonProperty]
        public string IncidentDateTime { get; private set; }

        /// <summary>
        /// Gets the date the claim is created.
        /// </summary>
        [JsonProperty]
        public string CreatedDateTime { get; private set; }

        /// <summary>
        /// Gets the date the claim is created.
        /// </summary>
        [JsonProperty]
        public string LastModifiedDateTime { get; private set; }

        /// <summary>
        /// Gets or sets the latest calculation data for the quote, if any, otherwise null.
        /// </summary>
        [JsonProperty]
        public object CalculationResult { get; set; }

        /// <summary>
        /// Gets or sets the owner's user id.
        /// </summary>
        [JsonProperty]
        public Guid? OwnerUserId { get; set; }

        /// <summary>
        /// Gets or sets the owner's name.
        /// </summary>
        [JsonProperty]
        public string OwnerName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether claim is a test data.
        /// </summary>
        [JsonProperty]
        public bool IsTestData { get; set; }

        /// <summary>
        /// Gets or sets formData.
        /// </summary>
        [JsonProperty]
        public object FormData { get; set; }

        /// <summary>
        /// Gets or sets Displayable Field model.
        /// </summary>
        public DisplayableFieldsModel DisplayableFieldsModel { get; set; }

        /// <summary>
        /// Gets or sets the question key names that have the attachment data type.
        /// </summary>
        public string[] QuestionAttachmentKeys { get; set; }

        /// <summary>
        /// Gets the documents associated with this quote version.
        /// </summary>
        public List<DocumentSetModel> Documents { get; private set; }

        /// <inheritdoc/>
        [JsonProperty]
        public List<AdditionalPropertyValueModel> AdditionalPropertyValues { get; private set; }
    }
}
