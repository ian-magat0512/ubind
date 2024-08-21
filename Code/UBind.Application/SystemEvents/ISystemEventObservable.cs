// <copyright file="ISystemEventObservable.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.SystemEvents
{
    using System;
    using UBind.Domain.Events;

    /// <summary>
    /// Service for system events to notify group of observers.
    /// </summary>
    public interface ISystemEventObservable : IObservable<SystemEvent>, IDisposable
    {
        /// <summary>
        /// Trigger the system event to notify to registered observers.
        /// </summary>
        /// <param name="systemEvent">The system event to notify observers.</param>
        void Trigger(SystemEvent systemEvent);
    }
}
