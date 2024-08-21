// <copyright file="ErrorDataObjectBuilderExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Extensions;

using Newtonsoft.Json.Linq;
using System.Linq;
using UBind.Domain.Aggregates.Claim;
using UBind.Domain.Aggregates.Claim.Entities;
public static class ErrorDataObjectBuilderExtensions
{

    /// <summary>
    /// Extension method to create and get a lightweight information object for a claim.
    /// </summary>
    /// <param name="claim">The current claim instance object</param>
    /// <returns></returns>
    public static List<string> CreateAndGetLightweightInformationDataForClaimErrorDetails(this Claim claim)
    {
        string triggers = claim.LatestCalculationResult.Data.Triggers.Any() ?
            string.Join(", ", claim.LatestCalculationResult.Data.Triggers.Select(trg => trg.Name)) :
            "0 calculation triggers found.";
        return new List<string>
        {
            $"Triggers: {triggers}",
        };
    }

    /// <summary>
    /// Extension method to create and get a lightweight information object for a claim.
    /// </summary>
    /// <param name="claim">The current claim instance object</param>
    /// <param name="operation">Current performed operation</param>
    /// <returns></returns>
    public static JObject CreateAndGetLightweightClaimInformationObject(this Claim claim, ClaimActions operation)
    {
        string triggers = claim.LatestCalculationResult.Data.Triggers.Any() ?
           string.Join(", ", claim.LatestCalculationResult.Data.Triggers.Select(trg => trg.Name)) :
           "0 calculation triggers found.";
        var lwData = new
        {
            ClaimId = claim.ClaimId,
            ProductId = claim.ProductId,
            TenantId = claim.TenantId,
            Triggers = triggers,
            ClaimAction = operation.ToString(),
            CalculationResultState = claim.LatestCalculationResult.Data.CalculationResultState,
        };
        return JObject.FromObject(lwData);
    }
}
