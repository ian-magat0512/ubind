// <copyright file="IEventRecordRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Repositories
{

    /// <summary>
    /// Repository for managing event records.
    /// </summary>
    public interface IEventRecordRepository
    {
        /// <summary>
        /// Updates a policy event record in the dbContext.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="aggregateId">The aggregate id.</param>
        /// <param name="aggregateType">The aggregate type.</param>
        /// <param name="sequenceNo">The sequence no of the event.</param>
        /// <param name="event">The new value of the event.</param>
        void UpdateEventRecord<TEvent>(
            Guid tenantId,
            Guid aggregateId,
            AggregateType aggregateType,
            int sequenceNo,
            TEvent @event);

        IEnumerable<TEventRecord> GetEventRecords<TEventRecord, TId>(
            Guid tenantId,
            TId aggregateId);

        Task<IEnumerable<TEventRecord>> GetEventRecordsAsync<TEventRecord, TId>(
            Guid tenantId,
            TId aggregateId);

        IEnumerable<TEventRecord> GetEventRecordsAfterSequence<TEventRecord, TId>(
            Guid tenantId,
            TId aggregateId,
            int sequenceNumber);

        Task<IEnumerable<TEventRecord>> GetEventRecordsAfterSequenceAsync<TEventRecord, TId>(
            Guid tenantId,
            TId aggregateId,
            int sequenceNumber);

        Task<IEnumerable<TEventRecord>> GetEventRecordsAtSequence<TEventRecord, TId>(
            Guid tenantId,
            TId aggregateId,
            int sequenceNumber,
            int? version);

        /// <summary>
        /// Saves the changes to the database.
        /// </summary>
        void SaveChanges();
    }
}
