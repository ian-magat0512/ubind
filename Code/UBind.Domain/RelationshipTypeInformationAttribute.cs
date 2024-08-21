// <copyright file="RelationshipTypeInformationAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// This class is needed because we need to attach additional information on relationship type enumeration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class RelationshipTypeInformationAttribute : Attribute
    {
        public RelationshipTypeInformationAttribute(
            string name,
            string alias,
            string sourceEntityPropertyName,
            string sourceEntityPropertyNamePlural,
            string sourceEntityPropertyAlias,
            string sourceEntityPropertyAliasPlural,
            string sourceEntityDescription,
            string targetEntityPropertyName,
            string targetEntityPropertyNamePlural,
            string targetEntityPropertyAlias,
            string targetEntityPropertyAliasPlural,
            string targetEntityDescription,
            EntityType sourceEntityType,
            EntityType[] targetEntityTypes)
        {
            this.Name = name;
            this.Alias = alias;
            this.SourceEntityPropertyName = sourceEntityPropertyName;
            this.SourceEntityPropertyNamePlural = sourceEntityPropertyNamePlural;
            this.SourceEntityPropertyAlias = sourceEntityPropertyAlias;
            this.SourceEntityPropertyAliasPlural = sourceEntityPropertyAliasPlural;
            this.SourceEntityDescription = sourceEntityDescription;
            this.TargetEntityPropertyName = targetEntityPropertyName;
            this.TargetEntityPropertyNamePlural = targetEntityPropertyNamePlural;
            this.TargetEntityPropertyAlias = targetEntityPropertyAlias;
            this.TargetEntityPropertyAliasPlural = targetEntityPropertyAliasPlural;
            this.TargetEntityDescription = targetEntityDescription;
            this.SourceEntityType = sourceEntityType;
            this.TargetEntityTypes = targetEntityTypes.ToList();
        }

        /// <summary>
        /// Gets a value the name of the relationship.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a value the alias of the relationship.
        /// </summary>
        public string Alias { get; }

        /// <summary>
        /// Gets the source entity property name.
        /// </summary>
        public string SourceEntityPropertyName { get; }

        /// <summary>
        /// Gets the source entity property name plural.
        /// </summary>
        public string SourceEntityPropertyNamePlural { get; }

        /// <summary>
        /// Gets the source entity property alias.
        /// </summary>
        public string SourceEntityPropertyAlias { get; }

        /// <summary>
        /// Gets the source entity property alias plural.
        /// </summary>
        public string SourceEntityPropertyAliasPlural { get; }

        /// <summary>
        /// Gets a value for relationship description.
        /// </summary>
        public string SourceEntityDescription { get; }

        /// <summary>
        /// Gets the source entity property name.
        /// </summary>
        public string TargetEntityPropertyName { get; }

        /// <summary>
        /// Gets the source entity property name plural.
        /// </summary>
        public string TargetEntityPropertyNamePlural { get; }

        /// <summary>
        /// Gets the source entity property alias.
        /// </summary>
        public string TargetEntityPropertyAlias { get; }

        /// <summary>
        /// Gets the source entity property alias plural.
        /// </summary>
        public string TargetEntityPropertyAliasPlural { get; }

        /// <summary>
        /// Gets a value for relationship description.
        /// </summary>
        public string TargetEntityDescription { get; }

        /// <summary>
        /// Gets a value for source entity type.
        /// </summary>
        public EntityType SourceEntityType { get; }

        /// <summary>
        /// Gets a value for target entity types.
        /// </summary>
        public List<EntityType> TargetEntityTypes { get; }
    }
}
