// <copyright file="AdditionalPropertyDefinitionReadModelWriter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.AdditionalPropertyDefinition;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using static UBind.Domain.Aggregates.AdditionalPropertyDefinition.AdditionalPropertyDefinition;

    /// <summary>
    /// A read model writer for additional property definition.
    /// </summary>
    public class AdditionalPropertyDefinitionReadModelWriter : IAdditionalPropertyDefinitionReadModelWriter
    {
        private readonly IWritableReadModelRepository<AdditionalPropertyDefinitionReadModel>
            additionalPropertyDefinitionWritableReadmodelRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdditionalPropertyDefinitionReadModelWriter"/> class.
        /// </summary>
        /// <param name="additionalPropertyDefinitionWritableReadModelRepository">A writable read model repository of additional property definition.</param>
        public AdditionalPropertyDefinitionReadModelWriter(
            IWritableReadModelRepository<AdditionalPropertyDefinitionReadModel> additionalPropertyDefinitionWritableReadModelRepository)
        {
            this.additionalPropertyDefinitionWritableReadmodelRepository = additionalPropertyDefinitionWritableReadModelRepository;
        }

        public void Dispatch(
            AdditionalPropertyDefinition aggregate,
            IEvent<AdditionalPropertyDefinition, Guid> @event,
            int sequenceNumber,
            IEnumerable<Type> observerTypes = null)
        {
            this.DispatchIfHandlerExists(aggregate, @event, sequenceNumber);
        }

        /// <summary>
        /// Handles event when fired during the inital creation of additional property definition.
        /// </summary>
        /// <param name="event">An initialization event for additional property definition.</param>
        /// <param name="sequenceNumber">The number in order.</param>
        public void Handle(
            AdditionalPropertyDefinition aggregate,
            AdditionalPropertyDefinitionInitializedEvent @event,
            int sequenceNumber)
        {
            if (aggregate.IsBeingReplayed)
            {
                this.additionalPropertyDefinitionWritableReadmodelRepository.DeleteById(@event.TenantId, @event.AggregateId);
            }

            this.additionalPropertyDefinitionWritableReadmodelRepository.Add(
                new AdditionalPropertyDefinitionReadModel(
                    @event.TenantId,
                    @event.AggregateId,
                    @event.Timestamp,
                    @event.Alias,
                    @event.Name,
                    @event.EntityType,
                    @event.ContextType,
                    @event.ContextId,
                    @event.IsRequired,
                    @event.IsUnique,
                    false,
                    @event.DefaultValue,
                    @event.PropertyType,
                    @event.SchemaType,
                    @event.ParentContextId,
                    @event.CustomSchema));
        }

        /// <summary>
        /// Handles event when fired during the update of an existing additiona property definition.
        /// </summary>
        /// <param name="event">An event instance.</param>
        /// <param name="sequenceNumber">The number in order.</param>
        public void Handle(
            AdditionalPropertyDefinition aggregate,
            AdditionalPropertyDetailsUpdatedEvent @event,
            int sequenceNumber)
        {
            var additionalPropertyDefinition = this.additionalPropertyDefinitionWritableReadmodelRepository.
                GetById(@event.TenantId, @event.AggregateId);
            additionalPropertyDefinition.Name = @event.Value.Name;
            additionalPropertyDefinition.Alias = @event.Value.Alias;
            additionalPropertyDefinition.IsRequired = @event.Value.IsRequired;
            additionalPropertyDefinition.IsUnique = @event.Value.IsUnique;
            additionalPropertyDefinition.DefaultValue = @event.Value.DefaultValue;
            additionalPropertyDefinition.LastModifiedTimestamp = @event.Timestamp;
            additionalPropertyDefinition.PropertyType = @event.Value.Type;
            additionalPropertyDefinition.SchemaType = @event.Value.SchemaType;
            additionalPropertyDefinition.CustomSchema = @event.Value.CustomSchema;
        }

        public void Handle(
            AdditionalPropertyDefinition aggregate,
            AdditionalPropertyMarkAsDeletedEvent @event,
            int sequenceNumber)
        {
            var additionalPropertyDefinition = this.additionalPropertyDefinitionWritableReadmodelRepository.
                GetById(@event.TenantId, @event.AggregateId);
            additionalPropertyDefinition.IsDeleted = true;
            additionalPropertyDefinition.LastModifiedTimestamp = @event.Timestamp;
        }
    }
}
