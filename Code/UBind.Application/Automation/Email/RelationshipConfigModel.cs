// <copyright file="RelationshipConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Email
{
    using System;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// Model for creating an <see cref="RelationshipConfiguration"/>.
    /// </summary>
    public class RelationshipConfigModel : IBuilder<RelationshipConfiguration>
    {
        /// <summary>
        /// Gets or sets the relationship type that should be added to the entity's relationship.
        /// </summary>
        [JsonProperty]
        public string RelationshipType { get; set; }

        /// <summary>
        /// Gets or sets the source entity.
        /// </summary>
        [JsonProperty]
        public IBuilder<BaseEntityProvider> SourceEntity { get; set; }

        /// <summary>
        /// Gets or sets the target entity.
        /// </summary>
        [JsonProperty]
        public IBuilder<BaseEntityProvider> TargetEntity { get; set; }

        /// <inheritdoc/>
        public RelationshipConfiguration Build(IServiceProvider dependencyProvider)
        {
            if (!Enum.TryParse(this.RelationshipType, true, out RelationshipType relationshipType))
            {
                throw new ErrorException(
                    Errors.Automation.RelationshipTypeNotValid(this.RelationshipType));
            }

            return new RelationshipConfiguration(
                relationshipType,
                this.SourceEntity?.Build(dependencyProvider),
                this.TargetEntity?.Build(dependencyProvider));
        }
    }
}
