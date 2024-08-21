// <copyright file="IAggregateRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates;

using System;
using System.Threading.Tasks;

/// <summary>
/// Repository for loading and saving aggregates.
/// </summary>
/// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
/// <typeparam name="TId">The type of the aggregate ID.</typeparam>
public interface IAggregateRepository<TAggregate, TId>
{
    /// <summary>
    /// Gets an aggregate by ID.
    /// </summary>
    /// <param name="tenantId">The Id of the tenant.</param>
    /// <param name="id">The ID.</param>
    /// <returns>The aggregate with matching ID, or null.</returns>
    TAggregate? GetById(Guid tenantId, TId id);

    Task<TAggregate?> GetByIdAsync(Guid tenantId, TId id);

    Task<TAggregate?> GetByIdWithoutUsingSnapshot(Guid tenantId, TId id);

    /// <summary>
    /// Gets an aggregate by ID as at the given sequence number.
    /// </summary>
    /// <param name="tenantId">The Id of the tenant.</param>
    /// <param name="id">The ID.</param>
    /// <param name="sequenceNumber">The sequence number.</param>
    /// <returns>The aggregate with matching ID, at that given sequence number, or null.</returns>
    Task<TAggregate?> GetByIdAtSequenceNumber(Guid tenantId, TId id, int sequenceNumber);

    /// <summary>
    /// Deletes an aggregate by ID.
    /// </summary>
    /// <param name="tenantId">The Id of the tenant.</param>
    /// <param name="id">The ID.</param>
    /// <returns>An awaitable task.</returns>
    Task DeleteById(Guid tenantId, TId id);

    /// <summary>
    /// Applies the changes to the DB Context, and if there is no existing transaction, then it will also persist
    /// the aggregate (and its events) in the repository. If there is an existing transaction under way, then it
    /// will be the responsibility of the code managing that transaction to commit it and save the changes that
    /// are in the DB Context to the database.
    /// </summary>
    /// <param name="aggregate">The aggregate to save.</param>
    /// <returns>An awaitable task.</returns>
    Task Save(TAggregate aggregate);

    /// <summary>
    /// Sets the changes on the DbContext but does not commit the change to the database yet.
    /// This can be used when you are managing a transaction with multiple changes and want to write the changes to the
    /// database only once all changes have been applied to the DB Context.
    /// You can call this and also make any other changes to the DbContext inside a transaction scope, and then
    /// manually call dbContext.SaveChanges() at the point you want to commit the changes to the database.
    /// Note: if you try to call this and there is no transaction under way, it will throw an exception.
    /// </summary>
    /// <param name="aggregate">The aggregate to persist.</param>
    /// <returns>An awaitable task.</returns>
    Task ApplyChangesToDbContext(TAggregate aggregate);

    /// <summary>
    /// Gets the snapshot save interval.
    /// </summary>
    /// <returns>Return the snapshot save interval.</returns>
    int GetSnapshotSaveInterval();
}
