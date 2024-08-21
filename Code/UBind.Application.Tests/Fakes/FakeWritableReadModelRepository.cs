// <copyright file="FakeWritableReadModelRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Fakes;

using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UBind.Domain;
using UBind.Domain.Repositories;
using UBind.Persistence.Extensions;

/// <summary>
/// This fake of the IWritableReadModelRepository interface is used for testing purposes, and allows
/// us to simulate the behaviour of a repository without actually hitting the database.
/// </summary>
/// <typeparam name="TReadModel"></typeparam>
public class FakeWritableReadModelRepository<TReadModel> : IWritableReadModelRepository<TReadModel>
    where TReadModel : class, IReadModel<Guid>
{
    private List<TReadModel> NewReadModels { get; set; } = new List<TReadModel>();

    public void Add(TReadModel readModel)
    {
        this.NewReadModels.Add(readModel);
    }

    public void AddOrUpdate(Guid tenantId, TReadModel readModel, Expression<Func<TReadModel, bool>> predicate)
    {
        predicate = this.AppendTenantIdToPredicate(tenantId, predicate);
        var readModelMaybe = Maybe<TReadModel>.From(this.NewReadModels.AsQueryable().SingleOrDefault(predicate));
        if (readModelMaybe.HasValue)
        {
            this.NewReadModels.Remove(readModelMaybe.Value);
        }

        this.NewReadModels.Add(readModel);
    }

    public void Delete(Guid tenantId, Expression<Func<TReadModel, bool>> predicate)
    {
        predicate = this.AppendTenantIdToPredicate(tenantId, predicate);

        // delete from the local list
        this.NewReadModels = this.NewReadModels.AsQueryable().Where(predicate.Not()).ToList();
    }

    public void DeleteById(Guid tenantId, Guid id)
    {
        var predicate = this.AppendTenantIdToIdPredicate(tenantId, id);

        // delete from the local list
        this.NewReadModels = this.NewReadModels.AsQueryable().Where(predicate.Not()).ToList();
    }

    public TReadModel GetById(Guid tenantId, Guid id)
    {
        Expression<Func<TReadModel, bool>> predicate = this.AppendTenantIdToIdPredicate(tenantId, id);
        var result = this.NewReadModels.AsQueryable().FirstOrDefault(predicate);
        return result;
    }

    public Maybe<TReadModel> GetByIdMaybe(Guid tenantId, Guid id)
    {
        Expression<Func<TReadModel, bool>> predicate = this.AppendTenantIdToIdPredicate(tenantId, id);
        var result = this.NewReadModels.AsQueryable().FirstOrDefault(predicate);
        return Maybe<TReadModel>.From(result);
    }

    public TReadModel GetByIdWithInclude(Guid tenantId, Guid id, Expression<Func<TReadModel, object>> includeExpression)
    {
        Expression<Func<TReadModel, bool>> predicate = this.AppendTenantIdToIdPredicate(tenantId, id);
        return this.NewReadModels.AsQueryable().SingleOrDefault(predicate);
    }

    public TReadModel Single(Guid tenantId, Expression<Func<TReadModel, bool>> predicate)
    {
        predicate = this.AppendTenantIdToPredicate(tenantId, predicate);
        return this.NewReadModels.AsQueryable().SingleOrDefault(predicate);
    }

    public Maybe<TReadModel> SingleMaybe(Guid tenantId, Expression<Func<TReadModel, bool>> predicate)
    {
        Expression<Func<TReadModel, bool>> newPredicate = this.AppendTenantIdToPredicate(tenantId, predicate);
        var result = this.NewReadModels.AsQueryable().FirstOrDefault(newPredicate);
        return Maybe<TReadModel>.From(result);
    }

    public TReadModel SingleWithInclude(
        Guid tenantId,
        Expression<Func<TReadModel, object>> includeExpression,
        Expression<Func<TReadModel, bool>> singlePredicate)
    {
        singlePredicate = this.AppendTenantIdToPredicate(tenantId, singlePredicate);
        return this.NewReadModels.AsQueryable().SingleOrDefault(singlePredicate);
    }

    public TReadModel SingleWithIncludes(
        Guid tenantId,
        List<Expression<Func<TReadModel, object>>> includeExpressions,
        Expression<Func<TReadModel, bool>> singlePredicate)
    {
        singlePredicate = this.AppendTenantIdToPredicate(tenantId, singlePredicate);
        var queryableReadModel = this.NewReadModels.AsQueryable();
        TReadModel readModel = null;
        readModel = queryableReadModel.SingleOrDefault(singlePredicate);
        return readModel;
    }

    public IEnumerable<TReadModel> Where(Guid tenantId, Expression<Func<TReadModel, bool>> predicate)
    {
        predicate = this.AppendTenantIdToPredicate(tenantId, predicate);
        return this.NewReadModels.AsQueryable().Where(predicate).ToList();
    }

    public IEnumerable<TReadModel> WhereWithIncludes(Guid tenantId, List<Expression<Func<TReadModel, object>>> includeExpressions, Expression<Func<TReadModel, bool>> predicate)
    {
        predicate = this.AppendTenantIdToPredicate(tenantId, predicate);
        var queryableReadModel = this.NewReadModels.AsQueryable();
        IEnumerable<TReadModel> readModels = null;
        readModels = queryableReadModel.Where(predicate).ToList();
        return readModels;
    }

    protected Expression<Func<TReadModel, bool>> AppendTenantIdToPredicate(Guid tenantId, Expression<Func<TReadModel, bool>> predicate)
    {
        Expression<Func<TReadModel, bool>> newPredicate = c => c.TenantId == tenantId;

        return ExpressionHelper.AndExpression(newPredicate, predicate);
    }

    protected Expression<Func<TReadModel, bool>> AppendTenantIdToIdPredicate(Guid tenantId, Guid id)
    {
        return c => c.TenantId == tenantId && c.Id == id;
    }
}
