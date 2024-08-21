// <copyright file="IAggregateEventObserver.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Interface for observers.
    /// </summary>
    /// <typeparam name="TAggregate">The type of the aggregate the event is on.</typeparam>
    /// <typeparam name="TEvent">The type of event being observed.</typeparam>
    public interface IAggregateEventObserver<TAggregate, in TEvent>
    {
        /// <summary>
        /// Handle an event.
        /// </summary>
        /// <param name="aggregate">The aggregate the event as was on.</param>
        /// <param name="event">The event to handle.</param>
        /// <param name="sequenceNumber">The sequence number of the event within its aggregate.</param>
        /// <param name="observerTypes">A list of observer types that this event will be dispatched to,
        /// or null to dispatch it to all event observer types.</param>
        void Dispatch(TAggregate aggregate, TEvent @event, int sequenceNumber, IEnumerable<Type> observerTypes = null);
    }
}
