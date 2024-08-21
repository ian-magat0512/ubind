// <copyright file="ClaimCalculationFormDataUpdateModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Claim
{
    using System;
    using Newtonsoft.Json;
    using UBind.Application;

    /// <summary>
    /// Form and and calculation data for an application POSTed from client.
    /// </summary>
    public class ClaimCalculationFormDataUpdateModel : IClaimResourceModel
    {
        /// <inheritdoc/>
        public Guid ClaimId { get; set; }

        /// <summary>
        /// Gets or sets form and calculation data.
        /// </summary>
        [JsonProperty(PropertyName = "formDataJson")]
        public SpreadsheetCalculationDataModel FormDataJson { get; set; }
    }
}
