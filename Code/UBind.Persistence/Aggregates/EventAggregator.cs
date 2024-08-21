// <copyright file="EventAggregator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MoreLinq;
    using UBind.Domain;
    using UBind.Domain.Aggregates;

    /// <summary>
    /// Aggregator for multiple dispatch of events from user aggregates.
    /// </summary>
    /// <typeparam name="TAggregate">The type of the root entity of the aggregate to handle events for.</typeparam>
    /// <typeparam name="TAggregateId">The type of the ID of root entity of the aggregate.</typeparam>
    public class EventAggregator<TAggregate, TAggregateId> : IAggregateEventObserver<TAggregate, IEvent<TAggregate, TAggregateId>>
    {
        private readonly List<IAggregateEventObserver<TAggregate, IEvent<TAggregate, TAggregateId>>> observers = new List<IAggregateEventObserver<TAggregate, IEvent<TAggregate, TAggregateId>>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="EventAggregator{TAggregate, TId}"/> class.
        /// </summary>
        /// <param name="observers">Observers to pass events on to.</param>
        public EventAggregator(params IAggregateEventObserver<TAggregate, IEvent<TAggregate, TAggregateId>>[] observers)
        {
            foreach (var observer in observers)
            {
                this.observers.Add(observer);
            }
        }

        public void Dispatch(
            TAggregate aggregate,
            IEvent<TAggregate, TAggregateId> @event,
            int sequenceNumber,
            IEnumerable<Type> observerTypes = null)
        {
            if (observerTypes == null)
            {
                this.observers.ForEach(o =>
                {
                    o.Dispatch(aggregate, @event, sequenceNumber);
                });
            }
            else
            {
                this.observers.Where(o => EventObserverHelper.IsInstanceOfOneOfTypes(o, observerTypes))
                    .ForEach(o => o.Dispatch(aggregate, @event, sequenceNumber));
            }
        }
    }
}
