// <copyright file="OrganisationModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Organisation
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Newtonsoft.Json;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Dto;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Web.ResourceModels;
    using UBind.Web.Validation;

    /// <summary>
    /// A view model for an organisation.
    /// </summary>
    public class OrganisationModel : IOrganisationDetails, IAdditionalPropertyValues
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrganisationModel"/> class.
        /// </summary>
        /// <param name="organisation">The organisation read model summary to transfer.</param>
        public OrganisationModel(IOrganisationReadModelSummary organisation)
        {
            this.Id = organisation.Id;
            this.TenantId = organisation.TenantId;
            this.Name = organisation.Name;
            this.Alias = organisation.Alias;
            this.IsActive = organisation.IsActive;
            this.IsDeleted = organisation.IsDeleted;
            this.IsDefault = organisation.IsDefault;
            this.CreatedDateTime = organisation.CreatedTimestamp.ToExtendedIso8601String();
            this.CreatedTicksSinceEpoch = organisation.CreatedTimestamp.ToUnixTimeTicks();
            this.LastModifiedDateTime = organisation.LastModifiedTimestamp.ToExtendedIso8601String();
            this.LastModifiedTicksSinceEpoch = organisation.LastModifiedTimestamp.ToUnixTimeTicks();
            this.Status = this.IsDeleted ? "Deleted" : organisation.IsActive ? "Active" : "Disabled";
        }

        public OrganisationModel(
            IOrganisationReadModelSummary organisation, List<AdditionalPropertyValueDto> additionalPropertyValueDtos)
            : this(organisation)
        {
            if (additionalPropertyValueDtos != null && additionalPropertyValueDtos.Any())
            {
                this.AdditionalPropertyValues = additionalPropertyValueDtos.Select(
                    apv => new AdditionalPropertyValueModel(apv)).ToList();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganisationModel"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for JSON deserializer.
        /// </remarks>
        [JsonConstructor]
        public OrganisationModel()
        {
        }

        /// <summary>
        /// Gets the status of the organisation.
        /// </summary>
        [JsonProperty]
        public string Status { get; private set; }

        /// <summary>
        /// Gets the Id of the organisation.
        /// </summary>
        [JsonProperty]
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets or sets the customer's tenant ID.
        /// </summary>
        [JsonProperty]
        public Guid TenantId { get; set; }

        /// <summary>
        /// Gets a value indicating whether it is the default organisation.
        /// </summary>
        [JsonProperty]
        public bool IsDefault { get; private set; }

        /// <summary>
        /// Gets the name of the organisation.
        /// </summary>
        [JsonProperty]
        [Required(ErrorMessage = "Organisation name is required.")]
        [EntityName]
        public string Name { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the organisation is active or disabled.
        /// </summary>
        [JsonProperty]
        public bool IsActive { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the organisation is deleted.
        /// </summary>
        [JsonProperty]
        public bool IsDeleted { get; private set; }

        /// <summary>
        /// Gets the time the organisation was created.
        /// </summary>
        [JsonProperty]
        public string CreatedDateTime { get; private set; }

        /// <summary>
        /// Gets the time the organisation was created in ticks in the epoch.
        /// </summary>
        public long CreatedTicksSinceEpoch { get; private set; }

        /// <summary>
        /// Gets the time the organisation was last modified.
        /// </summary>
        [JsonProperty]
        public string LastModifiedDateTime { get; private set; }

        /// <summary>
        /// Gets the time the organisation was last modified in ticks in the epoch.
        /// </summary>
        public long LastModifiedTicksSinceEpoch { get; private set; }

        /// <summary>
        /// Gets the alias of the organisation.
        /// </summary>
        [JsonProperty]
        [Required(ErrorMessage = "Organisation alias is required.")]
        [RegularExpression(
            @ValidationExpressions.Alias,
            ErrorMessage = "Alias must only contain lowercase English letters, digits, and hyphens; and must not start or end with a hyphen.")]
        public string Alias { get; private set; }

        /// <summary>
        /// Gets or sets additional properties.
        /// </summary>
        [JsonProperty(PropertyName = "properties")]
        public List<AdditionalPropertyValueUpsertModel> Properties { get; set; }

        /// <inheritdoc/>
        public List<AdditionalPropertyValueModel> AdditionalPropertyValues { get; set; }
    }
}
