// <copyright file="ISystemEventDeletionService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Services
{
    /// <summary>
    /// A service interface for deleting system events.
    /// </summary>
    public interface ISystemEventDeletionService
    {
        /// <summary>
        /// Deletes expired system events by batch.
        /// </summary>
        /// <param name="batchSize">The size of the batch to delete.</param>
        /// <returns></returns>
        Task ExecuteDeletionInBatches(int batchSize, CancellationToken cancellationToken);
    }
}