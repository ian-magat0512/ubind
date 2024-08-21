// <copyright file="ClaimDataLocator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Claim
{
    using Newtonsoft.Json;
    using UBind.Domain.Aggregates.Common;

    /// <summary>
    /// Specifies the location of claim data in claim form data and calculation result JSON.
    /// </summary>
    public class ClaimDataLocator : IDataLocator<IClaimData>
    {
        [JsonConstructor]
        private ClaimDataLocator()
        {
        }

        /// <summary>
        /// Gets the default quote data locator.
        /// </summary>
        public static IDataLocator<IClaimData> SharedInstance => new ClaimDataLocator();

        /// <inheritdoc />
        public IClaimData Invoke(string formDataJson, string calculationJson)
        {
            return new ClaimData();
        }
    }
}
