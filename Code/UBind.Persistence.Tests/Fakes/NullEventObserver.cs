// <copyright file="NullEventObserver.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.Fakes
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain;

    /// <summary>
    /// Event observer that takes no action.
    /// </summary>
    /// <typeparam name="TAggregate">The type of aggregate.</typeparam>
    /// <typeparam name="TEvent">The type of event being observed.</typeparam>
    public class NullEventObserver<TAggregate, TEvent> : IAggregateEventObserver<TAggregate, TEvent>
    {
        public void Dispatch(
            TAggregate aggregate,
            TEvent @event,
            int sequenceNumber,
            IEnumerable<Type> observerTypes = null)
        {
            // NOP
        }
    }
}
