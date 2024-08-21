// <copyright file="AdditionalPropertyValueCollectionHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;

    /// <summary>
    /// Helper class in adding or updating additional property values.
    /// This is being utilized in the aggregate to avoid code repetition.
    /// </summary>
    public static class AdditionalPropertyValueCollectionHelper
    {
        /// <summary>
        /// Adds the additional property event details to an initial additional property value collection.
        /// </summary>
        /// <typeparam name="TEvent">Additional property aggregate event type.</typeparam>
        /// <param name="initialPropertyValues">Initial list of <see cref="AdditionalPropertyValue"/>.</param>
        /// <param name="event">Additional property aggregate event.</param>
        public static void Add<TEvent>(IList<AdditionalPropertyValue> initialPropertyValues, TEvent @event)
            where TEvent : IAdditionalPropertyValueEventDetails
        {
            if (initialPropertyValues == null)
            {
                initialPropertyValues = new List<AdditionalPropertyValue>();
            }

            // just making sure that only those with valid ids are included in the collection.
            if (@event.AdditionalPropertyDefinitionId == Guid.Empty || @event.EntityId == Guid.Empty)
            {
                return;
            }

            initialPropertyValues.Add(new AdditionalPropertyValue(
                @event.TenantId, @event.EntityId, @event.AdditionalPropertyDefinitionId, @event.Value));
        }

        /// <summary>
        /// Adds or updates the additional property event details to an initial additional property value collection.
        /// </summary>
        /// <typeparam name="TEvent">Additional property aggregate event type.</typeparam>
        /// <param name="initialPropertyValues">Initial list of <see cref="AdditionalPropertyValue"/>.</param>
        /// <param name="event">Additional property aggregate event.</param>
        public static void AddOrUpdate<TEvent>(IList<AdditionalPropertyValue> initialPropertyValues, TEvent @event)
            where TEvent : IAdditionalPropertyValueEventDetails
        {
            if (initialPropertyValues == null)
            {
                initialPropertyValues = new List<AdditionalPropertyValue>();
            }

            var initialPropertyValue = initialPropertyValues.FirstOrDefault(
                ipv => ipv.AdditionalPropertyDefinitionId == @event.AdditionalPropertyDefinitionId &&
                ipv.EntityId == @event.EntityId);

            if (initialPropertyValue == default)
            {
                Add(initialPropertyValues, @event);
            }
            else
            {
                initialPropertyValue.Value = @event.Value;
            }
        }
    }
}
