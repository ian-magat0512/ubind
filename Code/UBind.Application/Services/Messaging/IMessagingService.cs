// <copyright file="IMessagingService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services
{
    using System;
    using System.Threading.Tasks;
    using MimeKit;

    /// <summary>
    /// Service for messaging.
    /// </summary>
    public interface IMessagingService
    {
        /// <summary>
        /// Sends the mime message.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="mailMessage">Represents a message that can be sent using the SmtpClient.</param>
        /// <param name="organisationId">The organisation Id.</param>
        void Send(Guid tenantId, MimeMessage mailMessage, Guid organisationId);

        /// <summary>
        /// Send an awaitable Email message.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="organisationId">The organisation Id.</param>
        /// <param name="mailMessage">The Mail Message to send.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SendAsync(Guid tenantId, Guid organisationId, MimeMessage mailMessage);
    }
}
