// <copyright file="PersistForAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Events;

using NodaTime;
using System;

/// <summary>
/// System Event Persistence.
/// Indicates how long from event creation it is persisted before
/// being deleted by the system event deletion service.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class PersistForAttribute : Attribute
{
    public PersistForAttribute(PeriodUnits unit, int value)
    {
        var hours = 0;
        switch (unit)
        {
            case PeriodUnits.Hours:
                hours = value;
                break;
            case PeriodUnits.Days:
                hours = value * 24;
                break;
            case PeriodUnits.Weeks:
                hours = value * 24 * 7;
                break;
            case PeriodUnits.Months:
                hours = value * 24 * 30;
                break;
            case PeriodUnits.Years:
                hours = value * 24 * 365;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(unit), unit, null);
        }
        this.Hours = hours;
    }

    /// <summary>
    /// Gets a value for role tenancy.
    /// </summary>
    public int Hours { get; }
}
