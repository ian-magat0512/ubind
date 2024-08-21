// <copyright file="IClaimDataMapper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Claim
{
    using Newtonsoft.Json.Linq;
    using NodaTime;

    /// <summary>
    /// For mapping between claim data and claim form data.
    /// </summary>
    public interface IClaimDataMapper
    {
        /// <summary>
        /// Extract well-known claim data items from the form data.
        /// </summary>
        /// <param name="formDataJson">Form data json.</param>
        /// <param name="calculationJson">Calculation result json.</param>
        /// <returns>The well known claim data.</returns>
        IClaimData ExtractClaimData(string formDataJson, string calculationJson, DateTimeZone timeZone);

        /// <summary>
        /// Insert well-known claim data into form data.
        /// </summary>
        /// <param name="formModel">The form model.</param>
        /// <param name="claimData">The well-known claim data.</param>
        /// <returns>A new jObject with data locations holding well-known data overwritten by the provided claim data.</returns>
        JObject SyncFormdata(JObject formModel, IClaimData claimData);
    }
}
