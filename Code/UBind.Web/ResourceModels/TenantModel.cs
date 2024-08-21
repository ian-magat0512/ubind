// <copyright file="TenantModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Newtonsoft.Json;
    using UBind.Domain;
    using UBind.Domain.Dto;
    using UBind.Domain.Extensions;
    using UBind.Web.Validation;

    /// <summary>
    /// A view model for a tenant.
    /// </summary>
    public class TenantModel : IAdditionalPropertyValues
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TenantModel"/> class.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        public TenantModel(Tenant tenant)
        {
            if (tenant != null)
            {
                this.Id = tenant.Id;
                this.Name = tenant?.Details?.Name;
                this.Alias = tenant?.Details?.Alias;
                this.CustomDomain = tenant?.Details?.CustomDomain;
                this.Deleted = tenant?.Details?.Deleted ?? false;
                this.Disabled = tenant.Details?.Disabled ?? false;
                this.DefaultOrganisationId = tenant?.Details?.DefaultOrganisationId ?? default;
                this.CreatedDateTime = tenant.CreatedTimestamp.ToExtendedIso8601String();
                this.LastModifiedDateTime = tenant.Details?.CreatedTimestamp.ToExtendedIso8601String();
                this.MasterTenant = Tenant.MasterTenantAlias;
                this.IdleTimeoutPeriodType = tenant?.Details?.IdleTimeoutPeriodType;
                this.IdleTimeout = tenant?.Details?.IdleTimeoutByPeriod();
                this.FixLengthTimeoutInPeriodType
                    = tenant?.Details?.FixLengthTimeoutInPeriodType ?? TokenSessionPeriodType.Day;
                this.FixLengthTimeout = tenant?.Details?.FixLengthTimeoutByPeriod();
            }
        }

        public TenantModel(Tenant tenant, List<AdditionalPropertyValueDto> additionalPropertyValueDto)
            : this(tenant)
        {
            if (additionalPropertyValueDto != null && additionalPropertyValueDto.Any())
            {
                this.AdditionalPropertyValues = additionalPropertyValueDto.Select(
                    apv => new AdditionalPropertyValueModel(apv)).ToList();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantModel"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for JSON deserializer.
        /// .</remarks>
        [JsonConstructor]
        public TenantModel()
        {
        }

        /// <summary>
        /// Gets the tenant ID.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [JsonProperty]
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets or sets the tenant name.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [JsonProperty]
        [Required(ErrorMessage = "Tenant name is required.")]
        [EntityName]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the tenant's alias.
        /// </summary>
        /// <remarks>Public setter required for JSON serialization.</remarks>
        [JsonProperty]
        [Required(ErrorMessage = "Tenant alias is required.")]
        [RegularExpression(
            @ValidationExpressions.Alias,
            ErrorMessage = "Alias must only contain lowercase English letters, digits, and hyphens; and must not start or end with a hyphen.")]
        public string Alias { get; set; }

        /// <summary>
        /// Gets the custom domain.
        /// </summary>
        [JsonProperty]
        public string CustomDomain { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the tenant is deleted.
        /// </summary>
        /// <remarks>Public setter required for JSON serialization.</remarks>
        [JsonProperty]
        public bool Deleted { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the tenant is disabled.
        /// </summary>
        [JsonProperty]
        public bool Disabled { get; private set; }

        /// <summary>
        /// Gets the time the tenant was created.
        /// </summary>
        [JsonProperty]
        public string CreatedDateTime { get; private set; }

        /// <summary>
        /// Gets the time the tenant was last modified.
        /// </summary>
        [JsonProperty]
        public string LastModifiedDateTime { get; private set; }

        /// <summary>
        /// Gets the MasterTenant value.
        /// </summary>
        [JsonProperty]
        public string MasterTenant { get; private set; }

        /// <summary>
        /// Gets the Organisation Id.
        /// </summary>
        [JsonProperty]
        public Guid DefaultOrganisationId { get; private set; }

        /// <summary>
        /// Gets the period type of the idle timeout value.
        /// </summary>
        [JsonProperty]
        public string IdleTimeoutPeriodType { get; private set; }

        /// <summary>
        /// Gets the period type of the fix length timeout value.
        /// </summary>
        [JsonProperty]
        public string FixLengthTimeoutInPeriodType { get; private set; }

        /// <summary>
        /// Gets the idle timeout in milliseconds for the specific tenant for security purposes.
        /// </summary>
        [JsonProperty]
        public long? IdleTimeout { get; private set; }

        /// <summary>
        /// Gets the fix length timeout in milliseconds for the specific tenant for security purposes.
        /// </summary>
        [JsonProperty]
        public long? FixLengthTimeout { get; private set; }

        /// <summary>
        /// Gets or sets the additional property values.
        /// </summary>
        public List<AdditionalPropertyValueModel> AdditionalPropertyValues { get; set; }

        /// <summary>
        /// Gets or sets additional property values.
        /// </summary>
        [JsonProperty]
        public List<AdditionalPropertyValueUpsertModel> Properties { get; set; }
    }
}
