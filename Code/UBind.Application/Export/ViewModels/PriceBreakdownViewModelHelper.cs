// <copyright file="PriceBreakdownViewModelHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export.ViewModels
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// Helper methods for generatingn different representations of price breakdowns.
    /// </summary>
    public static class PriceBreakdownViewModelHelper
    {
        /// <summary>
        /// Generate a view model for a price breakdown as a JObject.
        /// </summary>
        /// <param name="priceBreakdown">The price breakdown.</param>
        /// <returns>A new instance of <see cref="JObject"/> with the price breakdown as transfored by <see cref="PriceBreakdownViewModel"/>.</returns>
        public static JObject GenerateViewModelAsJObject(PriceBreakdown priceBreakdown) => JObject.FromObject(
            new PriceBreakdownViewModel(priceBreakdown),
            JsonSerializer.Create(new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }));
    }
}
