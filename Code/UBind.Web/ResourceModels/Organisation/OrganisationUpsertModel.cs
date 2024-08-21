// <copyright file="OrganisationUpsertModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Organisation
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Web.ResourceModels;
    using UBind.Web.Validation;

    /// <summary>
    /// The create organisation request model.
    /// </summary>
    public class OrganisationUpsertModel : IOrganisationDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrganisationUpsertModel"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for JSON deserializer.
        /// </remarks>
        [JsonConstructor]
        public OrganisationUpsertModel()
        {
        }

        /// <summary>
        /// Gets or sets the customer's tenant Id or Alias.
        /// </summary>
        [JsonProperty]
        public string Tenant { get; set; }

        /// <summary>
        /// Gets or sets the name of the organisation.
        /// </summary>
        [JsonProperty]
        [Required(ErrorMessage = "Organisation name is required.")]
        [EntityName]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the alias of the organisation.
        /// </summary>
        [JsonProperty]
        [Required(ErrorMessage = "Organisation alias is required.")]
        [RegularExpression(
            @ValidationExpressions.Alias,
            ErrorMessage = "Alias must only contain lowercase English letters, digits, and hyphens; and must not start or end with a hyphen.")]
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the managing organisation ID or alias (optional).
        /// </summary>
        [JsonProperty]
        public string ManagingOrganisation { get; set; }

        /// <summary>
        /// Gets or sets additional properties.
        /// </summary>
        [JsonProperty(PropertyName = "properties")]
        public List<AdditionalPropertyValueUpsertModel> Properties { get; set; }

        /// <summary>
        /// Gets or sets linked identities, which represent the organisation within an external identity provider.
        /// </summary>
        [JsonProperty(PropertyName = "linkedIdentities")]
        public List<UBind.Domain.Aggregates.Organisation.LinkedIdentity> LinkedIdentities { get; set; }
    }
}
