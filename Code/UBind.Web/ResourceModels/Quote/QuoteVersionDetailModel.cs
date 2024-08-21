// <copyright file="QuoteVersionDetailModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Quote;

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UBind.Domain;
using UBind.Domain.Dto;
using UBind.Domain.Extensions;
using UBind.Domain.Product;
using UBind.Domain.ReadModel;
using UBind.Web.Helpers;
using UBind.Web.ResourceModels;

/// <summary>
/// Resource model for serving the details of a particular quote version.
/// </summary>
public class QuoteVersionDetailModel : QuoteDetailModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QuoteVersionDetailModel"/> class.
    /// </summary>
    /// <param name="formDataPrettifier">The form data prettifier.</param>
    /// <param name="quoteDetails">The quote details.</param>
    /// <param name="quoteVersionDetails">The quote version details.</param>
    /// <param name="fieldDto">The list of displayable fields.</param>
    /// <param name="formDataSchema">The formdata schema.</param>
    /// <param name="additionalPropertyValueDtos">Additional property value dtos.</param>
    public QuoteVersionDetailModel(
        IFormDataPrettifier formDataPrettifier,
        IQuoteReadModelDetails quoteDetails,
        IQuoteVersionReadModelDetails quoteVersionDetails,
        DisplayableFieldDto fieldDto,
        FormDataSchema formDataSchema,
        List<AdditionalPropertyValueDto> additionalPropertyValueDtos)
    {
        this.Id = quoteVersionDetails.QuoteVersionId;
        this.OrganisationId = quoteDetails.OrganisationId;
        this.OrganisationName = quoteDetails.OrganisationName;
        this.QuoteId = quoteDetails.QuoteId;
        this.PolicyId = quoteDetails.PolicyId;

        if (quoteVersionDetails.CustomerId.HasValue)
        {
            this.CustomerDetails = new CustomerSimpleModel(quoteVersionDetails.CustomerId.Value, quoteVersionDetails.CustomerFullName);
        }

        this.ProductName = quoteDetails.ProductName;
        this.ProductId = quoteDetails.ProductId;
        this.TenantId = quoteDetails.TenantId;
        this.CreatedDateTime = quoteVersionDetails.CreatedTimestamp.ToString();
        this.PolicyNumber = quoteDetails.PolicyNumber;
        this.QuoteNumber = quoteDetails.QuoteNumber;
        this.QuoteStatus = quoteDetails.QuoteState;
        this.QuoteVersionNumber = quoteVersionDetails.QuoteVersionNumber;

        this.QuoteVersionState = quoteVersionDetails.State ?? "Not available";
        this.WorkflowStep = quoteVersionDetails.WorkflowStep;

        // Store into a variable so we only deserialize once.
        var calculationResult = quoteVersionDetails.CalculationResult;
        var calculationJson = calculationResult?.Json ?? "{}";

        // TODO: we do not have access to the latest calculation result id, so we're setting it to default here
        // It's not available in the read model table, so if this is really needed we need to persist it from the start.
        this.LatestCalculationData = new CalculationResultDetailModel(
            default,
            calculationResult.FormDataId.Value,
            calculationJson);

        this.Owner = quoteVersionDetails.OwnerUserId.HasValue ? new UserSummaryModel(quoteVersionDetails.OwnerUserId.Value, quoteVersionDetails.OwnerFullName) : null;
        if (fieldDto != null)
        {
            this.DisplayableFieldsModel = new DisplayableFieldsModel(fieldDto);
        }

        this.LastModifiedDateTime = quoteVersionDetails.LastModifiedTimestamp.ToExtendedIso8601String();
        this.QuoteType = (int)quoteDetails.QuoteType;
        this.IsTestData = quoteDetails.IsTestData;

        dynamic formData = formDataSchema != null && quoteVersionDetails.LatestFormData != null
            ? formDataPrettifier.GetPrettifiedFormData(
                formDataSchema.GetQuestionMetaData(),
                quoteVersionDetails.LatestFormData,
                quoteVersionDetails.QuoteId,
                "Quote",
                new ProductContext(quoteDetails.TenantId, quoteDetails.ProductId, quoteDetails.Environment))
            : (dynamic)quoteVersionDetails.LatestFormData;

        dynamic formDataObject = formData ?? new JObject();
        this.FormData = formDataObject?.formModel;

        dynamic calculationResultObject = JsonConvert.DeserializeObject(calculationJson ?? string.Empty);
        this.QuestionData = calculationResultObject?.questions;
        this.PremiumData = calculationResult?.PayablePrice != null
            ? new PremiumResult(calculationResult.PayablePrice)
            : null;
        this.ExpiryDateTime = quoteDetails?.ExpiryTimestamp?.ToString();
        this.QuestionAttachmentKeys = FormDataHelper.GetQuestionAttachmentFieldPaths(formDataSchema, this.FormData);
        if (quoteVersionDetails.Documents != null)
        {
            this.Documents = quoteVersionDetails.Documents.Select(d => new DocumentSetModel(d));
        }

        this.AdditionalPropertyValues = additionalPropertyValueDtos != null && additionalPropertyValueDtos.Any() ?
            additionalPropertyValueDtos.Select(apv => new AdditionalPropertyValueModel(apv)).ToList() :
            new List<AdditionalPropertyValueModel>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QuoteVersionDetailModel"/> class.
    /// </summary>
    [JsonConstructor]
    public QuoteVersionDetailModel()
    {
    }

    /// <summary>
    /// Gets or sets the reference Id for the quote version.
    /// </summary>
    public new Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the reference Id for the quote.
    /// </summary>
    public Guid QuoteId { get; set; }

    /// <summary>
    /// Gets or sets the current state of this quote version.
    /// </summary>
    public string QuoteVersionState { get; set; }

    /// <summary>
    /// Gets or sets the current workflow step of this quote version.
    /// </summary>
    public string WorkflowStep { get; set; }

    /// <summary>
    /// Gets or sets the current status of the parent quote.
    /// </summary>
    public string QuoteStatus { get; set; }

    /// <summary>
    /// Gets the version number of the quote version.
    /// </summary>
    public int QuoteVersionNumber { get; private set; }
}
