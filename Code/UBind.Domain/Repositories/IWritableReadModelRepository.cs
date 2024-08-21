// <copyright file="IWritableReadModelRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Repositories
{
    using System;
    using System.Linq.Expressions;
    using UBind.Domain;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Interface for a read model repository.
    /// </summary>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    public interface IWritableReadModelRepository<TReadModel> : IReadModelRepository<TReadModel>
        where TReadModel : class, IReadModel<Guid>
    {
        /// <summary>
        /// Add a new read model to the repository.
        /// </summary>
        /// <param name="readModel">The read model to add.</param>
        void Add(TReadModel readModel);

        /// <summary>
        /// Deletes read models which match the tenantId and id.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="id">The Id to be deleted.</param>
        void DeleteById(Guid tenantId, Guid id);

        /// <summary>
        /// Deletes read models which match the predicate.
        /// </summary>
        /// <param name="tenantId">Append the tenantId query to the predicate.</param>
        /// <param name="predicate">Used to test whether there is a matching read model to delete.</param>
        void Delete(Guid tenantId, Expression<Func<TReadModel, bool>> predicate);

        /// <summary>
        /// Adds or updates a new read model to the repository. A match in the NewReadModels sequence is tested using the passed predicate.
        /// This causes an extra query to the database, so should only be used in special circumstances.
        /// </summary>
        /// <param name="tenantId">Append the tenantId query to the predicate.</param>
        /// <param name="readModel">The read model to add.</param>
        /// <param name="predicate">Used to test whether there is a matching read model, which if there is means the operation will be an update operation.</param>
        void AddOrUpdate(Guid tenantId, TReadModel readModel, Expression<Func<TReadModel, bool>> predicate);
    }
}
