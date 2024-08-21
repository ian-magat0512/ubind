// <copyright file="QuoteVersionReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel;

using System;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Extensions;

/// <summary>
/// Read model for quote versions.
/// </summary>
public class QuoteVersionReadModel : EntityReadModel<Guid>
{
    private Guid aggregateId;

    /// <summary>
    /// Initializes static properties.
    /// </summary>
    static QuoteVersionReadModel()
    {
        SupportsAdditionalProperties = true;
    }

    /// <summary>
    /// Gets or sets the ID of the quote version.
    /// </summary>
    public Guid QuoteVersionId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the aggregate the quote belongs to.
    /// </summary>
    public Guid AggregateId
    {
        get => (this.aggregateId != default) ? this.aggregateId : this.QuoteId;
        set => this.aggregateId = value;
    }

    /// <summary>
    /// Gets or sets the quote's ID.
    /// </summary>
    public Guid QuoteId { get; set; }

    /// <summary>
    /// Gets or sets the Version Number for the current quote.
    /// </summary>
    public int QuoteVersionNumber { get; set; }

    /// <summary>
    /// Gets or sets the latest form data for the quote.
    /// </summary>
    public string LatestFormData { get; set; }

    /// <summary>
    /// Gets or sets the ID of the customer the quote is assigned to if any, otherwise default.
    /// </summary>
    public Guid? CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the person who is the customer for the quote if any, otherwise default.
    /// </summary>
    public Guid? CustomerPersonId { get; set; }

    /// <summary>
    /// Gets or sets the Customer's full name.
    /// </summary>
    public string CustomerFullName { get; set; }

    /// <summary>
    /// Gets or sets the Customer's preferred name.
    /// </summary>
    public string CustomerPreferredName { get; set; }

    /// <summary>
    /// Gets or sets the customer's email address.
    /// </summary>
    public string CustomerEmail { get; set; }

    /// <summary>
    /// Gets or sets the customer's alternative email address.
    /// </summary>
    public string CustomerAlternativeEmail { get; set; }

    /// <summary>
    /// Gets or sets the customer's mobile phone number.
    /// </summary>
    public string CustomerMobilePhone { get; set; }

    /// <summary>
    /// Gets or sets the customer's home phone number.
    /// </summary>
    public string CustomerHomePhone { get; set; }

    /// <summary>
    /// Gets or sets the customer's work phone number.
    /// </summary>
    public string CustomerWorkPhone { get; set; }

    /// <summary>
    /// Gets or sets the quote number.
    /// </summary>
    public string QuoteNumber { get; set; }

    /// <summary>
    /// Gets or sets the user ID of the current owner of the quote.
    /// </summary>
    public Guid? OwnerUserId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the person who owns this quote.
    /// </summary>
    public Guid? OwnerPersonId { get; set; }

    /// <summary>
    /// Gets or sets the full name of the person who owns this quote.
    /// </summary>
    public string OwnerFullName { get; set; }

    /// <summary>
    /// Gets or sets the calculation result json string related to the quote version.
    /// </summary>
    /// <remarks>
    /// For old quotes that do not have the whole calculation result.
    /// </remarks>
    public string CalculationResultJson { get; set; }

    /// <summary>
    /// Gets or sets the serialized calculation result.
    /// </summary>
    public string SerializedCalculationResult { get; set; }

    /// <summary>
    /// Gets or sets the environment where the quote version is created.
    /// </summary>
    public DeploymentEnvironment Environment { get; set; }

    /// <summary>
    /// Gets or sets the type of quote version.
    /// </summary>
    public QuoteType Type { get; set; }

    /// <summary>
    /// Gets or sets the Id of product the quote version belongs to.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Gets or sets the Id of organisation the quote version belongs to.
    /// </summary>
    public Guid OrganisationId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to return a test data.
    /// </summary>
    public bool IsTestData { get; set; }

    /// <summary>
    /// Gets or sets the calculation result for the quote version.
    /// </summary>
    [NotMapped]
    public ReadWriteModel.CalculationResult CalculationResult
    {
        get => this.SerializedCalculationResult != null
            ? JsonConvert.DeserializeObject<ReadWriteModel.CalculationResult>(
                this.SerializedCalculationResult, CustomSerializerSetting.JsonSerializerSettings)
            : null;

        set => this.SerializedCalculationResult = JsonConvert.SerializeObject(value);
    }

    /// <summary>
    /// Gets or sets the quote state at the time this quote version was created.
    /// </summary>
    public string State { get; set; }

    /// <summary>
    /// Gets or sets the quote workflow step at the time this quote version was created.
    /// </summary>
    public string WorkflowStep { get; set; }

    /// <summary>
    /// Patch form data.
    /// </summary>
    /// <param name="patch">The patch to apply.</param>
    public void ApplyPatch(PolicyDataPatch patch)
    {
        if (patch.IsApplicable(this))
        {
            if (patch.Type == DataPatchType.FormData)
            {
                if (this.LatestFormData != null)
                {
                    var formData = new FormData(this.LatestFormData);
                    formData.PatchFormModelProperty(patch.Path, patch.Value);
                    this.LatestFormData = formData.Json;
                }
            }
            else if (patch.Type == DataPatchType.CalculationResult)
            {
                if (!string.IsNullOrEmpty(this.CalculationResultJson))
                {
                    JObject obj = JObject.Parse(this.CalculationResultJson);
                    obj.PatchProperty(patch.Path, patch.Value);
                    this.CalculationResultJson = obj.ToString();
                }

                if (!string.IsNullOrEmpty(this.SerializedCalculationResult))
                {
                    var calculationResult = JsonConvert.DeserializeObject<ReadWriteModel.CalculationResult>(
                        this.SerializedCalculationResult, CustomSerializerSetting.JsonSerializerSettings);
                    calculationResult.PatchProperty(patch.Path, patch.Value);
                    this.SerializedCalculationResult = JsonConvert.SerializeObject(calculationResult);
                }
            }
        }
    }
}
