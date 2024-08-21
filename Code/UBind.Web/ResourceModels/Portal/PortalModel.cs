// <copyright file="PortalModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Portal
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Newtonsoft.Json;
    using UBind.Domain;
    using UBind.Domain.Dto;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Web.Validation;

    /// <summary>
    /// A view model for a portal.
    /// </summary>
    public class PortalModel : IAdditionalPropertyValues
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PortalModel"/> class.
        /// </summary>
        /// <param name="portal">The tenant.</param>
        public PortalModel(PortalReadModel portal)
        {
            this.Id = portal.Id;
            this.StylesheetUrl = portal.StyleSheetUrl;
            this.Styles = portal.Styles;
            this.Name = portal.Name;
            this.Alias = portal.Alias;
            this.Title = portal.Title;
            this.Deleted = portal.Deleted;
            this.Disabled = portal.Disabled;
            this.IsDefault = portal.IsDefault;
            this.CreatedDateTime = portal.CreatedTimestamp.ToExtendedIso8601String();
            this.CreatedTicksSinceEpoch = portal.CreatedTimestamp.ToUnixTimeTicks();
            this.LastModifiedDateTime = portal.CreatedTimestamp.ToExtendedIso8601String();
            this.LastModifiedTicksSinceEpoch = portal.CreatedTicksSinceEpoch;
            this.TenantId = portal.TenantId;
            this.UserType = portal.UserType;
            this.OrganisationId = portal.OrganisationId;
            this.ProductionUrl = portal.ProductionUrl;
            this.StagingUrl = portal.StagingUrl;
            this.DevelopmentUrl = portal.DevelopmentUrl;
        }

        public PortalModel(PortalReadModel portal, List<AdditionalPropertyValueDto> additionalPropertyValueDtos)
            : this(portal)
        {
            if (additionalPropertyValueDtos != null && additionalPropertyValueDtos.Any())
            {
                this.AdditionalPropertyValues = additionalPropertyValueDtos.Select(
                    apv => new AdditionalPropertyValueModel(apv)).ToList();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PortalModel"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for JSON deserializer.
        /// .</remarks>
        [JsonConstructor]
        private PortalModel()
        {
        }

        /// <summary>
        /// Gets the portal ID.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [JsonProperty]
        public Guid? Id { get; private set; }

        /// <summary>
        /// Gets the portal stylesheet url.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [JsonProperty]
        [StylesheetUrl]
        public string StylesheetUrl { get; private set; }

        /// <summary>
        /// Gets the portal inline styles.
        /// </summary>
        [JsonProperty]
        [StylesheetUrl]
        public string Styles { get; private set; }

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
        /// Gets a value indicating whether the portal is deleted.
        /// </summary>
        /// <remarks>Public setter required for JSON serialization.</remarks>
        [JsonProperty]
        public bool Deleted { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the portal is disabled.
        /// </summary>
        [JsonProperty]
        public bool Disabled { get; private set; }

        [JsonProperty]
        public bool IsDefault { get; private set; }

        /// <summary>
        /// Gets the time the portal was created.
        /// </summary>
        [JsonProperty]
        public string CreatedDateTime { get; private set; }

        /// <summary>
        /// Gets the time the portal was created in ticks since epoch.
        /// </summary>
        [JsonProperty]
        public long CreatedTicksSinceEpoch { get; private set; }

        /// <summary>
        /// Gets the time the portal was last modified.
        /// </summary>
        [JsonProperty]
        public string LastModifiedDateTime { get; private set; }

        /// <summary>
        /// Gets the time the portal was last modified in ticks since epoch.
        /// </summary>
        [JsonProperty]
        public long LastModifiedTicksSinceEpoch { get; private set; }

        /// <summary>
        /// Gets or sets the owner tenant ID.
        /// </summary>
        [JsonProperty]
        public Guid? TenantId { get; set; }

        [JsonProperty]
        public PortalUserType UserType { get; set; }

        /// <summary>
        /// Gets or sets the organisation ID.
        /// </summary>
        [JsonProperty]
        public Guid? OrganisationId { get; set; }

        [JsonProperty]
        public string ProductionUrl { get; set; }

        [JsonProperty]
        public string StagingUrl { get; set; }

        [JsonProperty]
        public string DevelopmentUrl { get; set; }

        /// <inheritdoc/>
        public List<AdditionalPropertyValueModel> AdditionalPropertyValues { get; set; }

        /// <summary>
        /// Gets or sets additional property values.
        /// </summary>
        [JsonProperty]
        public List<AdditionalPropertyValueUpsertModel> Properties { get; set; }
    }
}
