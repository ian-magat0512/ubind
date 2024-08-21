// <copyright file="IAutomationExtensionPointService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services
{
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Configuration;
    using UBind.Domain.Product;

    /// <summary>
    /// Service for building and validating automation request data for extension points.
    /// </summary>
    public interface IAutomationExtensionPointService
    {
        /// <summary>
        /// Executes before quote calculation to modify the source input data.
        /// </summary>
        /// <returns>The modified source input data, or null if the input data was not changed.</returns>
        Task<JObject> TriggerBeforeQuoteCalculation(
           UBind.Domain.Aggregates.Quote.Quote? quote,
           ReleaseContext releaseContext,
           IProductConfiguration productConfiguration,
           JObject sourceInputData,
           Guid? organisationId,
           CancellationToken cancellationToken);

        /// <summary>
        /// Executes after quote calculation to modify the source input data.
        /// </summary>
        /// <returns>The modified source input data and the calculation result.</returns>
        Task<AfterQuoteCalculationExtensionResultModel> TriggerAfterQuoteCalculation(
           UBind.Domain.Aggregates.Quote.Quote? quote,
           ReleaseContext releaseContext,
           IProductConfiguration productConfiguration,
           JObject sourceInputData,
           JObject sourceCalculationResult,
           Guid? organisationId,
           CancellationToken cancellationToken);
    }
}
