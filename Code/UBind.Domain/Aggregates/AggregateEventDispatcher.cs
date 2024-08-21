// <copyright file="AggregateEventDispatcher.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;

    /// <summary>
    /// Dispatches events to the correct method of the aggregate event observer, if it exists.
    /// </summary>
    public static class AggregateEventDispatcher
    {
        /// <summary>
        /// Dispatches to a Handle(...) method if one exists within the given observer class,
        /// If no such Handle(...) method exists, nothing happens.
        /// </summary>
        /// <remarks>
        /// If you want to see if any Handle methods haven't had their first parameter "aggregate" added, you can use
        /// this regex:
        /// "void Handle\((?!(.*)aggregate,)(?:.*)\)"
        /// .
        /// </remarks>
        public static void DispatchIfHandlerExists<TAggregate, TId>(
            this IAggregateEventObserver<TAggregate, IEvent<TAggregate, TId>> observer,
            TAggregate aggregate,
            IEvent<TAggregate, TId> @event,
            int sequenceNumber)
        {
            var method = observer.GetType().GetMethod(
                "Handle",
                new Type[] { aggregate.GetType(), @event.GetType(), typeof(int) },
                null);
            if (method != null)
            {
                try
                {
                    var result = method.Invoke(observer, new object[] { aggregate, @event, sequenceNumber });
                    if (result is Task task)
                    {
                        task.Wait();
                    }
                }
                catch (TargetInvocationException ex) when (ex.InnerException != null)
                {
                    // Unwrap the inner exception and throw it
                    ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                }
                catch (AggregateException ex) when (ex.InnerException != null)
                {
                    // Unwrap the inner exception and throw it
                    ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                }
            }
        }
    }
}
