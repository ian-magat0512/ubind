// <copyright file="IApplyAdditionalPropertyValueEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.AdditionalPropertyValue
{
    /// <summary>
    /// Contract that contains Apply method that accepts the generic AdditionalPropertyValueInitializedEvent.
    /// </summary>
    /// <typeparam name="TEvent">Aggregate event.</typeparam>
    public interface IApplyAdditionalPropertyValueEvent<TEvent>
    {
        /// <summary>
        /// Apply the event to the aggregate.
        /// </summary>
        /// <param name="aggregateEvent">The aggregate to apply the event to.</param>
        /// <param name="sequenceNumber">The sequence number of the event within the aggregate.</param>
        void Apply(
            TEvent aggregateEvent,
            int sequenceNumber);
    }
}
