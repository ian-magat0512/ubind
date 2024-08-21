// <copyright file="EventObserverHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class EventObserverHelper
    {
        public static List<Type> GetObserverTypesForDispatchFlags(
            bool dispatchToAllObservers,
            bool dispatchToReadModelWriters,
            bool dispatchToSystemEventEmitters)
        {
            if (dispatchToAllObservers)
            {
                return null;
            }

            List<Type> observerTypes = new List<Type>();
            if (dispatchToReadModelWriters)
            {
                observerTypes.Add(typeof(IReadModelWriter));
            }

            if (dispatchToSystemEventEmitters)
            {
                observerTypes.Add(typeof(ISystemEventEmitter));
            }

            return observerTypes;
        }

        public static bool IsInstanceOfOneOfTypes(object instance, IEnumerable<Type> types)
        {
            return types.Any(t => t.IsAssignableFrom(instance.GetType()));
        }
    }
}
