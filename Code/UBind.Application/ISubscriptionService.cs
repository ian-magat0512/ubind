// <copyright file="ISubscriptionService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using System.Threading.Tasks;
    using Flurl;

    /// <summary>
    /// Service for subscribing to One Drive notifications.
    /// </summary>
    public interface ISubscriptionService
    {
        /// <summary>
        /// Subscribe to One Drive notifications.
        /// </summary>
        /// <param name="notificationUrl">The URL where notifications will be recieved.</param>
        /// <returns>An asynchronous task that can be awaited.</returns>
        Task SubscribeToNotifications(Url notificationUrl);
    }
}
