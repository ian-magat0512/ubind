// <copyright file="SystemEventTypeExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Events;

using System.Collections.Generic;
using NodaTime;

public static class SystemEventTypeExtensions
{
    private static ISystemEventTypePersistenceDurationRegistry? persistenceDurationRegistry;

    public static void SetPersistenceDurationRegistry(ISystemEventTypePersistenceDurationRegistry registry)
    {
        persistenceDurationRegistry = registry;
    }

    public static int? GetPersistenceInHoursOrNull(this SystemEventType systemEventType)
    {
        return persistenceDurationRegistry?.GetPersistenceDurationForSystemEventType(systemEventType);
    }

    public static Instant? GetExpiryTimeStamp(this SystemEventType systemEventType, Instant timestamp)
    {
        var expiryTimeHours = GetPersistenceInHoursOrNull(systemEventType);
        if (expiryTimeHours == null)
        {
            return null;
        }

        return timestamp.Plus(Duration.FromHours(expiryTimeHours.Value));
    }

    /// <summary>
    /// Gets the System event types that are to be persisted indefinitely.
    /// </summary>
    public static List<int> GetSystemEventTypeValuesToPersistIndefinitely()
    {
        return persistenceDurationRegistry?.GetSystemEventTypeValuesToPersistIndefinitely() ?? new List<int>();
    }

    /// <summary>
    /// Gets the System event types that has duration for persistence.
    /// </summary>
    public static List<SystemEventType> GetSystemEventTypesWithExpiry()
    {
        return persistenceDurationRegistry?.GetSystemEventTypesWithExpiry() ?? new List<SystemEventType>();
    }
}
