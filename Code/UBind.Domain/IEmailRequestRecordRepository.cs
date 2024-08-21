// <copyright file="IEmailRequestRecordRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain.Entities;

    /// <summary>
    /// interface for email request record repository.
    /// </summary>
    /// <typeparam name="TEmailRequestRecord">the type of record.</typeparam>
    public interface IEmailRequestRecordRepository<TEmailRequestRecord>
        where TEmailRequestRecord : class, IEmailRequestRecord
    {
        /// <summary>
        /// Retrieve latest records.
        /// </summary>
        /// <param name="organisationId">The Id of the organisation.</param>
        /// <param name="emailAddress">The email address.</param>
        /// <param name="max">The max records to retrieve.</param>
        /// <returns>the records.</returns>
        IEnumerable<TEmailRequestRecord> GetLatestRecords(Guid tenantId, Guid organisationId, string emailAddress, int max = 5);

        /// <summary>
        /// insert the record.
        /// </summary>
        /// <param name="record">the record.</param>
        void Insert(TEmailRequestRecord record);

        /// <summary>
        /// commit changes to the database.
        /// </summary>
        void SaveChanges();
    }
}
