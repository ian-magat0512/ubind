// <copyright file="AdditionalPropertyDefinitionQueryModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain.Enums;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Query model used by the <see cref="AdditionalPropertyDefinitionQueryModel"/> controller in searching for the list of
    /// additional properties.
    /// </summary>
    public class AdditionalPropertyDefinitionQueryModel
    {
        /// <summary>
        /// Gets or sets the tenant ID or alias which the additional property is defined within.
        /// </summary>
        public string Tenant { get; set; }

        /// <summary>
        /// Gets or sets the context <see cref="AdditionalPropertyDefinitionContextType"/> being searched for.
        /// </summary>
        public AdditionalPropertyDefinitionContextType? ContextType { get; set; }

        /// <summary>
        /// Gets or sets the entity <see cref="AdditionalPropertyEntityType"/> being searched for.
        /// </summary>
        public AdditionalPropertyEntityType? Entity { get; set; }

        public Guid? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the context id being searched for.
        /// </summary>
        public string ContextId { get; set; }

        /// <summary>
        /// Gets or sets the parent context id (usually tenant id if your context is either product or organisation)
        /// being searched for.
        /// </summary>
        public string ParentContextId { get; set; }

        /// <summary>
        /// Gets or sets the property name being searched for.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the property alias being searched for.
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the search result should be merged dataset.
        /// 1) If the parent context is not null then the result should be the combine result of the child and parent
        /// context.
        /// 2) If the parent context is null then whatever value you assign in this property will be irrelevant.
        /// 3) Then parent context is always with respect to the tenant context.
        /// </summary>
        public bool MergeResult { get; set; }

        public AdditionalPropertyDefinitionReadModelFilters ToFilters()
        {
            var modelFilter = new AdditionalPropertyDefinitionReadModelFilters
            {
                Aliases = new List<string> { this.Alias },
                ContextType = this.ContextType,
                Entity = this.Entity,
                Name = this.Name,
                MergeResult = this.MergeResult,
            };
            return modelFilter;
        }
    }
}
