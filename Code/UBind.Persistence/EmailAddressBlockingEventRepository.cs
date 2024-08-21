// <copyright file="EmailAddressBlockingEventRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Linq;
    using UBind.Domain;
    using UBind.Domain.Entities;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Repository for email blocking events.
    /// </summary>
    public class EmailAddressBlockingEventRepository : IEmailAddressBlockingEventRepository
    {
        private readonly IUBindDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailAddressBlockingEventRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public EmailAddressBlockingEventRepository(IUBindDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public EmailAddressBlockingEvent GetLatestBlockingEvent(
            Guid tenantId, Guid organisationId, string emailAddress)
        {
            return this.dbContext.EmailAddressBlockingEvents
                .Where(e => e.TenantId == tenantId)
                .Where(e => e.OrganisationId == organisationId)
                .Where(e => e.EmailAddress == emailAddress)
                .OrderByDescending(e => e.CreatedTicksSinceEpoch)
                .FirstOrDefault();
        }

        /// <inheritdoc/>
        public void Insert(EmailAddressBlockingEvent emailBlockingEvent)
        {
            this.dbContext.EmailAddressBlockingEvents.Add(emailBlockingEvent);
        }

        /// <inheritdoc/>
        public void SaveChanges() => this.dbContext.SaveChanges();
    }
}
