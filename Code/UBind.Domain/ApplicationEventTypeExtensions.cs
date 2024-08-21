// <copyright file="ApplicationEventTypeExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    /// <summary>
    /// Extension methods for <see cref="ApplicationEventType"/>.
    /// </summary>
    public static class ApplicationEventTypeExtensions
    {
        /// <summary>
        /// Returns a value indicating whether an event type relates to policy transaction creation.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <returns><c>true</c> if the event does relate to policy transaction creation, otherwise <c>false</c>.</returns>
        public static bool IsPolicyTransactionCreation(this ApplicationEventType eventType) =>
            eventType == ApplicationEventType.PolicyIssued ||
            eventType == ApplicationEventType.PolicyRenewed ||
            eventType == ApplicationEventType.PolicyAdjusted ||
            eventType == ApplicationEventType.PolicyCancelled;
    }
}
