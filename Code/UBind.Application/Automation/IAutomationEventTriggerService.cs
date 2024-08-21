// <copyright file="IAutomationEventTriggerService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation
{
    using System.Threading.Tasks;
    using UBind.Domain.Events;

    /// <summary>
    /// Event triggers automation service used for triggering events by system events.
    /// </summary>
    public interface IAutomationEventTriggerService
    {
        /// <summary>
        /// Handles the system event to trigger an automation.
        /// </summary>
        /// <param name="systemEvent">The system event.</param>
        Task HandleSystemEvent(SystemEvent systemEvent, CancellationToken cancellationToken);
    }
}
