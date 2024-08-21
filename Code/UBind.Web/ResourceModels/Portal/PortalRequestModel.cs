// <copyright file="PortalRequestModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Portal
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;
    using UBind.Domain;
    using UBind.Web.Validation;

    /// <summary>
    /// A request model for creating or editing portal.
    /// </summary>
    public class PortalRequestModel : IAdditionalPropertyValues
    {
        /// <summary>
        /// Gets the portal ID.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [JsonProperty]
        public Guid? Id { get; private set; }

        /// <summary>
        /// Gets the portal name.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [JsonProperty]
        [Required(ErrorMessage = "Portal Name is required.")]
        [EntityName]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the portal alias.
        /// </summary>
        /// <remarks>Public setter required for JSON serialization.</remarks>
        [JsonProperty]
        [Required(ErrorMessage = "Portal alias is required.")]
        [RegularExpression(
            @ValidationExpressions.Alias,
            ErrorMessage = "Alias must only contain lowercase, alphabetic characters and hyphens. It must not start or end in hyphen.")]
        public string Alias { get; private set; }

        /// <summary>
        /// Gets the portal title.
        /// </summary>
        /// <remarks>Public setter required for JSON serialization.</remarks>
        [JsonProperty]
        [Required(ErrorMessage = "Portal Title is required.")]
        [RegularExpression(
            @ValidationExpressions.CustomLabel,
            ErrorMessage = "Portal Title contain only letters, numbers, dashes and spaces")]
        public string Title { get; private set; }

        /// <summary>
        /// Gets or sets the portal tenant Id or Alias.
        /// </summary>
        [JsonProperty]
        public string Tenant { get; set; }

        [JsonProperty]
        public PortalUserType UserType { get; set; }

        /// <summary>
        /// Gets or sets the organisation Id or Alias.
        /// </summary>
        [JsonProperty]
        public string Organisation { get; set; }

        /// <inheritdoc/>
        public List<AdditionalPropertyValueModel> AdditionalPropertyValues { get; set; }

        /// <summary>
        /// Gets or sets additional property values.
        /// </summary>
        [JsonProperty]
        public List<AdditionalPropertyValueUpsertModel> Properties { get; set; }
    }
}
