// <copyright file="ISystemEventTypePersistenceDurationRegistry.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Events;

/// <summary>
/// Contains the persistence in hours available per system event type, for fast lookup.
/// This is gathered using Reflection and is done on first use and kept in memory
/// so that subsequent lookups are fast.
/// </summary>
public interface ISystemEventTypePersistenceDurationRegistry
{
    int? GetPersistenceDurationForSystemEventType(SystemEventType systemEventType);

    List<int> GetSystemEventTypeValuesToPersistIndefinitely();

    List<SystemEventType> GetSystemEventTypesWithExpiry();
}