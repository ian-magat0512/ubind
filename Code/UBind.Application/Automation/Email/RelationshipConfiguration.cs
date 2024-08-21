// <copyright file="RelationshipConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Email
{
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Providers;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.SerialisedEntitySchemaObject;

    /// <summary>
    /// This class is needed because we need a provider that will resolve relationship json string to relationship object.
    /// </summary>
    public class RelationshipConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipConfiguration"/> class.
        /// </summary>
        /// <param name="relationshipType">The relationship type.</param>
        /// <param name="sourceEntityProvider">The source entity provider.</param>
        /// <param name="targetEntityProvider">The target entity provider.</param>
        public RelationshipConfiguration(RelationshipType relationshipType, IProvider<Data<IEntity>> sourceEntityProvider, IProvider<Data<IEntity>> targetEntityProvider)
        {
            this.ThrowIfRelationshipIsInvalid(relationshipType, sourceEntityProvider, targetEntityProvider);

            this.RelationshipType = relationshipType;
            this.SourceEntity = sourceEntityProvider;
            this.TargetEntity = targetEntityProvider;
        }

        /// <summary>
        /// Gets the alias of the relationship type that should be added to the entity's relationship.
        /// </summary>
        public RelationshipType RelationshipType { get; }

        /// <summary>
        /// Gets the source entity provider.
        /// </summary>
        public IProvider<Data<IEntity>> SourceEntity { get; }

        /// <summary>
        /// Gets the target entity provider.
        /// </summary>
        public IProvider<Data<IEntity>> TargetEntity { get; }

        private void ThrowIfRelationshipIsInvalid(RelationshipType relationshipType, IProvider<Data<IEntity>> sourceEntity, IProvider<Data<IEntity>> targetEntity)
        {
            var relationShipTypeInfo = relationshipType.GetAttributeOfType<RelationshipTypeInformationAttribute>();
            var data = new JObject()
            {
                { ErrorDataKey.Feature, "automations" },
            };

            // when message is source entity, a target entity is required.
            if (relationShipTypeInfo.SourceEntityType == EntityType.Message
                && targetEntity == null)
            {
                data.Add(ErrorDataKey.ErrorMessage, $"Relationship type \"{relationshipType}\" requires target entity.");
                throw new ErrorException(
                    Errors.Automation.AutomationConfigurationNotSupported(
                        $"Relationship type \"{relationshipType}\" requires target entity.", data));
            }

            // when message is target entity, a source entity is required.
            if (relationShipTypeInfo.TargetEntityTypes[0] == EntityType.Message
                && sourceEntity == null)
            {
                data.Add(ErrorDataKey.ErrorMessage, $"Relationship type \"{relationshipType}\" requires source entity.");
                throw new ErrorException(
                    Errors.Automation.AutomationConfigurationNotSupported(
                        $"Relationship type \"{relationshipType}\" requires source entity.", data));
            }
        }
    }
}
