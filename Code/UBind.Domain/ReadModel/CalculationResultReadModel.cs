// <copyright file="CalculationResultReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UBind.Domain.ReadWriteModel;

/// <summary>
/// This class is for parsing serialized calculation results and exposing data for read queries.
/// It will work with both full calculation results including payable amounts etc., and also with
/// migrated calculation results, which only include the calculation json.
/// </summary>
public class CalculationResultReadModel
{
    private const string QuestionsPropertyKey = "questions";
    private readonly string serializedCalculationResult;
    private PriceBreakdown payablePrice;
    private string questions;
    private CalculationResult calculationResult;

    /// <summary>
    /// Initializes a new instance of the <see cref="CalculationResultReadModel"/> class.
    /// </summary>
    /// <param name="serializedCalculationResult">A string containing a serialized calculation result.</param>
    public CalculationResultReadModel(string serializedCalculationResult)
    {
        this.serializedCalculationResult = serializedCalculationResult;
        if (serializedCalculationResult == null)
        {
            this.payablePrice = PriceBreakdown.Zero(PriceBreakdown.DefaultCurrencyCode);
            this.questions = "{}";
        }

        // If serialized calculation result is available, all other data is
        // lazily generated, so preventn unecessary deserialiazation, since
        // this class is used in summary sets.
    }

    /// <summary>
    /// Gets a breakdown of the payable price.
    /// </summary>
    public PriceBreakdown PayablePrice
    {
        get
        {
            if (this.payablePrice == null)
            {
                if (this.CalculationResult == null)
                {
                    this.payablePrice = PriceBreakdown.Zero(this.payablePrice.CurrencyCode);
                }
                else
                {
                    this.payablePrice = this.CalculationResult.PayablePrice
                        ?? PriceBreakdown.CreateFromCalculationResultData(this.CalculationResult);
                }
            }

            return this.payablePrice;
        }
    }

    /// <summary>
    /// Gets json containing questions taken from the calculation result.
    /// </summary>
    public string Questions
    {
        get
        {
            if (this.questions == null)
            {
                if (this.CalculationResult == null)
                {
                    this.questions = "{}";
                }
                else
                {
                    var calculationJObject = JObject.Parse(this.CalculationResult.Json);
                    this.questions = JsonConvert.SerializeObject(calculationJObject?[QuestionsPropertyKey]) ?? string.Empty;
                }
            }

            return this.questions;
        }
    }

    /// <summary>
    /// Gets the calculation result.
    /// </summary>
    public CalculationResult CalculationResult
    {
        get
        {
            if (this.calculationResult == null && (!string.IsNullOrEmpty(this.serializedCalculationResult)))
            {
                this.calculationResult = JsonConvert.DeserializeObject<CalculationResult>(
                    this.serializedCalculationResult, CustomSerializerSetting.JsonSerializerSettings);
            }

            return this.calculationResult;
        }
    }
}
