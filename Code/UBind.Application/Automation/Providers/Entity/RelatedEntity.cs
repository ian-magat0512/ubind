// <copyright file="RelatedEntity.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Entity
{
    using System.Collections.Generic;

    /// <summary>
    /// This class is needed because we need to have a object that will represents the related entity of an entity.
    /// </summary>
    public class RelatedEntity
    {
        /// <summary>
        /// Gets or sets the property name.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Gets or sets the entity provider that will generate the entity.
        /// </summary>
        public DynamicEntityProvider EntityProvider { get; set; }

        /// <summary>
        /// Gets or sets child entities of the entity.
        /// </summary>
        public List<RelatedEntity> ChildEntities { get; set; }
    }
}
