// <copyright file="PolicyTransactionData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Policy;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Extensions;
using UBind.Domain.Json;

/// <summary>
/// Policy data that is stored for policy upsert transactions.
/// </summary>
public class PolicyTransactionData
{
    private CalculationResultReadModel calculationResult;

    /// <summary>
    /// Initializes a new instance of the <see cref="PolicyTransactionData"/> class.
    /// </summary>
    /// <param name="policyData">The policy data.</param>
    public PolicyTransactionData(PolicyData policyData)
        : this(policyData.QuoteDataSnapshot)
    {
    }

    public PolicyTransactionData(QuoteDataSnapshot dataSnapshot)
    {
        this.SerializedCalculationResult = JsonConvert.SerializeObject(dataSnapshot.CalculationResult.Data);
        this.FormData = dataSnapshot.FormData.Data.Json;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PolicyTransactionData"/> class.
    /// </summary>
    /// <remarks>Parameterless constructor for EF.</remarks>
    protected PolicyTransactionData()
    {
    }

    /// <summary>
    /// Gets the policy's form data.
    /// </summary>
    public string FormData { get; private set; }

    /// <summary>
    /// Gets the policy's calculation result.
    /// </summary>
    public string SerializedCalculationResult { get; private set; }

    /// <summary>
    /// Gets the calculation result for the transaction.
    /// </summary>
    public CalculationResultReadModel CalculationResult
    {
        get
        {
            if (this.calculationResult == null)
            {
                this.calculationResult = new CalculationResultReadModel(this.SerializedCalculationResult);
            }

            return this.calculationResult;
        }
    }

    /// <summary>
    /// Patch a property in the form data.
    /// </summary>
    /// <param name="path">The property path.</param>
    /// <param name="value">The new property Value.</param>
    public void PatchFormDataPropertyValue(JsonPath path, JToken value)
    {
        JObject obj = JObject.Parse(this.FormData);
        var formModel = obj.SelectToken("formModel") as JObject;
        formModel.PatchProperty(path, value);
        this.FormData = JsonConvert.SerializeObject(obj);
    }

    /// <summary>
    /// Creates a new copy of the calculation result with updated data.
    /// </summary>
    /// <param name="path">The property path.</param>
    /// <param name="value">The new property Value.</param>
    public void PatchCalculationResultPropertyValue(JsonPath path, JToken value)
    {
        var calculationResult = JsonConvert.DeserializeObject<ReadWriteModel.CalculationResult>(
            this.SerializedCalculationResult, CustomSerializerSetting.JsonSerializerSettings);
        calculationResult.PatchProperty(path, value);
        this.SerializedCalculationResult = JsonConvert.SerializeObject(calculationResult);
    }
}
