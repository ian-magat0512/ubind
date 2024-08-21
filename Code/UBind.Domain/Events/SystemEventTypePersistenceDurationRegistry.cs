// <copyright file="SystemEventTypePersistenceDurationRegistry.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Events;

using System.Reflection;

/// <summary>
/// Contains the persistence for each system event type, for fast lookup.
/// This is gathered using Reflection and is done on first use and kept in memory
/// so that subsequent lookups are fast.
/// </summary>
public class SystemEventTypePersistenceDurationRegistry : ISystemEventTypePersistenceDurationRegistry
{
    private object mapLock = new object();
    private Dictionary<SystemEventType, int?> persistenceMap;

    public SystemEventTypePersistenceDurationRegistry()
    {
        SystemEventTypeExtensions.SetPersistenceDurationRegistry(this);
    }

    private Dictionary<SystemEventType, int?> PersistenceDurationMap
    {
        get
        {
            lock (this.mapLock)
            {
                if (this.persistenceMap == null)
                {
                    this.PopulatePersistenceDurationMap();
                }
            }
            return this.persistenceMap;
        }
    }

    public int? GetPersistenceDurationForSystemEventType(SystemEventType systemEventType)
    {
        this.PersistenceDurationMap.TryGetValue(systemEventType, out int? hours);
        return hours;
    }

    public List<int> GetSystemEventTypeValuesToPersistIndefinitely()
    {
        return this.PersistenceDurationMap.Where(p => p.Value == null).Select(p => (int)p.Key).ToList();
    }

    public List<SystemEventType> GetSystemEventTypesWithExpiry()
    {
        return this.PersistenceDurationMap.Where(p => p.Value != null).Select(p => p.Key).ToList();
    }

    private void PopulatePersistenceDurationMap()
    {
        this.persistenceMap = new Dictionary<SystemEventType, int?>();
        IEnumerable<FieldInfo> fieldInfos = typeof(SystemEventType).GetFields();
        foreach (var fieldInfo in fieldInfos)
        {
            if (!Enum.TryParse<SystemEventType>(fieldInfo.Name, true, out var systemEventType))
            {
                continue;
            }

            var persistenceAttr = fieldInfo
                .GetCustomAttributes(typeof(PersistForAttribute), false)
                .FirstOrDefault() as PersistForAttribute;
            if (!this.persistenceMap.ContainsKey(systemEventType))
            {
                this.persistenceMap.Add(systemEventType, persistenceAttr?.Hours);
            }
        }
    }
}