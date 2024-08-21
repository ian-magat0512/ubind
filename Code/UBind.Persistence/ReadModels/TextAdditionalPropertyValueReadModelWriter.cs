// <copyright file="TextAdditionalPropertyValueReadModelWriter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using static UBind.Domain.Aggregates.AdditionalPropertyValue.TextAdditionalPropertyValue;

    /// <summary>
    /// A read model writer for additional property value.
    /// </summary>
    public class TextAdditionalPropertyValueReadModelWriter : ITextAdditionalPropertyValueReadModelWriter
    {
        private readonly IWritableReadModelRepository<TextAdditionalPropertyValueReadModel> writableReadModelRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextAdditionalPropertyValueReadModelWriter"/> class.
        /// </summary>
        /// <param name="writableReadModelRepository">A writable read model repository. </param>
        public TextAdditionalPropertyValueReadModelWriter(
            IWritableReadModelRepository<TextAdditionalPropertyValueReadModel> writableReadModelRepository)
        {
            this.writableReadModelRepository = writableReadModelRepository;
        }

        public void Dispatch(
            TextAdditionalPropertyValue aggregate,
            IEvent<TextAdditionalPropertyValue, Guid> @event,
            int sequenceNumber,
            IEnumerable<Type> observerTypes = null)
        {
            this.DispatchIfHandlerExists(aggregate, @event, sequenceNumber);
        }

        public void Dispatch(QuoteAggregate aggregate, IEvent<QuoteAggregate, Guid> @event, int sequenceNumber, IEnumerable<Type> observerTypes = null)
        {
            this.DispatchIfHandlerExists(aggregate, @event, sequenceNumber);
        }

        public void Handle(
            TextAdditionalPropertyValue aggregate,
            TextAdditionalPropertyValueInitializedEvent @event,
            int sequenceNumber)
        {
            if (aggregate.IsBeingReplayed)
            {
                this.writableReadModelRepository.DeleteById(@event.TenantId, @event.AggregateId);
            }

            this.writableReadModelRepository.Add(
                new TextAdditionalPropertyValueReadModel(
                    @event.TenantId,
                    @event.EntityId,
                    @event.AggregateId,
                    @event.AdditionalPropertyDefinitionId,
                    @event.Value,
                    @event.Timestamp));
        }

        public void Handle(
            TextAdditionalPropertyValue aggregate,
            TextAdditionalPropertyUpdateValueEvent @event,
            int sequenceNumber)
        {
            var readModel = this.writableReadModelRepository.GetById(@event.TenantId, @event.AggregateId);
            readModel.Value = @event.Value;
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteRollbackEvent @event, int sequenceNumber)
        {
            // We delete existing AVP values because these will be added back during rollback when it replays all the saved events
            this.writableReadModelRepository.Delete(@event.TenantId, q => q.EntityId == @event.QuoteId);
        }
    }
}
