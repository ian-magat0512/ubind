// <copyright file="EmailRequestRecordRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using UBind.Domain;
    using UBind.Domain.Entities;
    using UBind.Domain.Repositories;

    /// <summary>
    /// repository for email request record.
    /// </summary>
    /// <typeparam name="TEmailRequestRecord">the type of record.</typeparam>
    public class EmailRequestRecordRepository<TEmailRequestRecord> : IEmailRequestRecordRepository<TEmailRequestRecord>
        where TEmailRequestRecord : class, IEmailRequestRecord
    {
        private readonly IUBindDbContext dbContext;
        private readonly DbSet<TEmailRequestRecord> dbSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailRequestRecordRepository{T}"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public EmailRequestRecordRepository(IUBindDbContext dbContext)
        {
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<TEmailRequestRecord>();
        }

        /// <inheritdoc/>
        public IEnumerable<TEmailRequestRecord> GetLatestRecords(
            Guid tenantId, Guid organisationId, string emailAddress, int max = 5)
        {
            return this.dbSet
                .Where(r => r.TenantId == tenantId && r.OrganisationId == organisationId && r.EmailAddress == emailAddress)
                .OrderByDescending(r => r.CreatedTicksSinceEpoch)
                .Take(max)
                .ToList();
        }

        /// <inheritdoc/>
        public void Insert(TEmailRequestRecord loginAttemptResult)
        {
            this.dbSet.Add(loginAttemptResult);
        }

        /// <inheritdoc/>
        public void SaveChanges() => this.dbContext.SaveChanges();
    }
}
