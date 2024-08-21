// <copyright file="IEmailAddressBlockingEventRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using UBind.Domain.Entities;

    /// <summary>
    /// Repository for email address blocking events.
    /// </summary>
    public interface IEmailAddressBlockingEventRepository
    {
        /// <summary>
        /// Get the latest email blocking event for a given email address in a given tenant.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="organisationId">The Id of the organisation.</param>
        /// <param name="emailAddress">The email address.</param>
        /// <returns>The latest email address blocking event if any, otherwise null.</returns>
        EmailAddressBlockingEvent GetLatestBlockingEvent(Guid tenantId, Guid organisationId, string emailAddress);

        /// <summary>
        /// Insert a new email address blocking event.
        /// </summary>
        /// <param name="blockingEvent">The email address blocking event.</param>
        void Insert(EmailAddressBlockingEvent blockingEvent);

        /// <summary>
        /// Save changes in the database.
        /// </summary>
        void SaveChanges();
    }
}
