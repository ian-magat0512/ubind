// <copyright file="QuoteVersionReadModelDetails.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels.Quote;

using System.Collections.Generic;
using Newtonsoft.Json;
using UBind.Domain;
using UBind.Domain.ReadModel;
using UBind.Domain.ReadWriteModel;

/// <summary>
/// For representing details of quote versions.
/// </summary>
public class QuoteVersionReadModelDetails : QuoteVersionReadModelSummary, IQuoteVersionReadModelDetails
{
    /// <summary>
    /// Gets or sets the documents associated with the quote version.
    /// </summary>
    public IEnumerable<QuoteDocumentReadModel> Documents { get; set; }

    /// <summary>
    /// Gets or sets a string containing the serialized calculation result.
    /// </summary>
    public string SerializedCalculationResult { get; set; }

    /// <summary>
    /// Gets the calculation result.
    /// </summary>
    public CalculationResult CalculationResult
    {
        get => this.SerializedCalculationResult != null
            ? JsonConvert.DeserializeObject<CalculationResult>(
                this.SerializedCalculationResult, CustomSerializerSetting.JsonSerializerSettings)
            : null;
    }
}
