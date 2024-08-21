// <copyright file="IReadModelRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using CSharpFunctionalExtensions;

    /// <summary>
    /// Interface for a read model repository.
    /// </summary>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    public interface IReadModelRepository<TReadModel>
        where TReadModel : class, IReadModel<Guid>
    {
        /// <summary>
        /// Find the record by Id.
        /// </summary>
        /// <param name="tenantId">The tenant Id of the record.</param>
        /// <param name="id">The Id of the record.</param>
        /// <returns>The matching read model (or none).</returns>
        TReadModel GetById(Guid tenantId, Guid id);

        /// <summary>
        /// Find the record by Id.
        /// </summary>
        /// <param name="tenantId">The tenant Id of the record.</param>
        /// <param name="id">The Id of the record.</param>
        /// <returns>A maybe monad, that holds the matching read model (or none).</returns>
        Maybe<TReadModel> GetByIdMaybe(Guid tenantId, Guid id);

        /// <summary>
        /// Find the single read model matching a predicate.
        /// </summary>
        /// <param name="tenantId">Append the tenantId query to the predicate.</param>
        /// <param name="predicate">The predicate for selecting the read model.</param>
        /// <returns>The single matching read model.</returns>
        TReadModel Single(Guid tenantId, Expression<Func<TReadModel, bool>> predicate);

        /// <summary>
        /// Find the read model matching with the id.
        /// </summary>
        /// <param name="tenantId">The tenant Id of the record.</param>
        /// <param name="id">The Id to retrieve.</param>
        /// <param name="includeExpression">The expression for selecting include path.</param>
        /// <returns>The read model.</returns>
        TReadModel GetByIdWithInclude(
            Guid tenantId,
            Guid id,
            Expression<Func<TReadModel, object>> includeExpression);

        /// <summary>
        /// Find the single read model matching a predicate with an include.
        /// </summary>
        /// <param name="tenantId">Append the tenantId query to the predicate.</param>
        /// <param name="includeExpression">The expression for selecting include path.</param>
        /// <param name="filterPredicate">The predicate for selecting the read model.</param>
        /// <returns>The single matching read model.</returns>
        TReadModel SingleWithInclude(
            Guid tenantId,
            Expression<Func<TReadModel, object>> includeExpression,
            Expression<Func<TReadModel, bool>> filterPredicate);

        /// <summary>
        /// Find the single read model matching a predicate with includes.
        /// </summary>
        /// <param name="tenantId">Append the tenantId query to the predicate.</param>
        /// <param name="includeExpressions">The expressions for selecting include path.</param>
        /// <param name="filterPredicate">The predicate for selecting the read model.</param>
        /// <returns>The single matching read model.</returns>
        TReadModel SingleWithIncludes(
            Guid tenantId,
            List<Expression<Func<TReadModel, object>>> includeExpressions,
            Expression<Func<TReadModel, bool>> filterPredicate);

        /// <summary>
        /// Find the single read model matching a predicate, or none.
        /// </summary>
        /// <param name="tenantId">Append the tenantId query to the predicate.</param>
        /// <param name="predicate">The predicate for selecting the read model.</param>
        /// <returns>A maybe monad, that holds the first matching read model (or none).</returns>
        Maybe<TReadModel> SingleMaybe(Guid tenantId, Expression<Func<TReadModel, bool>> predicate);

        /// <summary>
        /// Find all read models matching a predicate.
        /// </summary>
        /// <param name="tenantId">Append the tenantId query to the predicate.</param>
        /// <param name="predicate">The predicate for selecting the read models.</param>
        /// <returns>A enumerable collection of all matching read models.</returns>
        IEnumerable<TReadModel> Where(Guid tenantId, Expression<Func<TReadModel, bool>> predicate);

        /// <summary>
        /// Find the list of read models matching a predicate with includes.
        /// </summary>
        /// <param name="tenantId">Append the tenantId query to the predicate.</param>
        /// <param name="includeExpressions">The expression for selecting include path.</param>
        /// <param name="predicate">The predicate for selecting the read model.</param>
        /// <returns>The single matching read model.</returns>
        IEnumerable<TReadModel> WhereWithIncludes(
            Guid tenantId,
            List<Expression<Func<TReadModel, object>>> includeExpressions,
            Expression<Func<TReadModel, bool>> predicate);
    }
}
