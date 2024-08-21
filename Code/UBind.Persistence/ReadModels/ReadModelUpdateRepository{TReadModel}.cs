// <copyright file="ReadModelUpdateRepository{TReadModel}.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;
    using CSharpFunctionalExtensions;
    using UBind.Domain;
    using UBind.Domain.Repositories;
    using UBind.Persistence.Extensions;

    /// <summary>
    /// Repository for fetching read models.
    /// </summary>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    public class ReadModelUpdateRepository<TReadModel> : IWritableReadModelRepository<TReadModel>
        where TReadModel : class, IReadModel<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadModelUpdateRepository{TReadModel}"/> class.
        /// </summary>
        /// <param name="dbContext">The UBind database context.</param>
        public ReadModelUpdateRepository(IUBindDbContext dbContext)
        {
            this.DbContext = dbContext;
            this.DbContext.SavingChanges += this.SavingChangesHandler;
        }

        /// <summary>
        /// Gets the UBind database context.
        /// </summary>
        protected IUBindDbContext DbContext { get; }

        /// <summary>
        /// Gets or sets the list of new read models that have been added to the repo in-memory,
        /// but not saved to the database yet.
        /// </summary>
        protected List<TReadModel> NewReadModels { get; set; } = new List<TReadModel>();

        /// <inheritdoc/>
        public TReadModel GetById(Guid tenantId, Guid id)
        {
            Expression<Func<TReadModel, bool>> predicate = this.AppendTenantIdToIdPredicate(tenantId, id);
            var result = this.NewReadModels.AsQueryable().FirstOrDefault(predicate) ??
                this.DbContext.Set<TReadModel>().FirstOrDefault(predicate);
            return result;
        }

        /// <inheritdoc/>
        public Maybe<TReadModel> GetByIdMaybe(Guid tenantId, Guid id)
        {
            Expression<Func<TReadModel, bool>> predicate = this.AppendTenantIdToIdPredicate(tenantId, id);
            var result = this.NewReadModels.AsQueryable().FirstOrDefault(predicate) ??
                this.DbContext.Set<TReadModel>().FirstOrDefault(predicate);
            return Maybe<TReadModel>.From(result);
        }

        /// <inheritdoc/>
        public virtual TReadModel GetByIdWithInclude(Guid tenantId, Guid id, Expression<Func<TReadModel, object>> includeExpression)
        {
            Expression<Func<TReadModel, bool>> predicate = this.AppendTenantIdToIdPredicate(tenantId, id);
            return this.NewReadModels.AsQueryable().Include(includeExpression).SingleOrDefault(predicate) ??
                this.DbContext.Set<TReadModel>().Include(includeExpression).SingleOrDefault(predicate);
        }

        /// <inheritdoc/>
        public Maybe<TReadModel> SingleMaybe(Guid tenantId, Expression<Func<TReadModel, bool>> predicate)
        {
            Expression<Func<TReadModel, bool>> newPredicate = this.AppendTenantIdToPredicate(tenantId, predicate);
            var result = this.NewReadModels.AsQueryable().FirstOrDefault(newPredicate) ??
                this.DbContext.Set<TReadModel>().FirstOrDefault(newPredicate);
            return Maybe<TReadModel>.From(result);
        }

        /// <inheritdoc/>
        public virtual TReadModel Single(Guid tenantId, Expression<Func<TReadModel, bool>> predicate)
        {
            predicate = this.AppendTenantIdToPredicate(tenantId, predicate);
            return this.NewReadModels.AsQueryable().SingleOrDefault(predicate) ??
                this.DbContext.Set<TReadModel>().Single(predicate);
        }

        /// <inheritdoc/>
        public virtual TReadModel SingleWithInclude(
            Guid tenantId,
            Expression<Func<TReadModel, object>> includeExpression,
            Expression<Func<TReadModel, bool>> singlePredicate)
        {
            singlePredicate = this.AppendTenantIdToPredicate(tenantId, singlePredicate);
            return this.NewReadModels.AsQueryable().Include(includeExpression).SingleOrDefault(singlePredicate) ??
                this.DbContext.Set<TReadModel>().Include(includeExpression).SingleOrDefault(singlePredicate);
        }

        /// <inheritdoc/>
        public virtual TReadModel SingleWithIncludes(
            Guid tenantId,
            List<Expression<Func<TReadModel, object>>> includeExpressions,
            Expression<Func<TReadModel, bool>> singlePredicate)
        {
            singlePredicate = this.AppendTenantIdToPredicate(tenantId, singlePredicate);
            var queryableReadModel = this.NewReadModels.AsQueryable();
            TReadModel readModel = null;
            foreach (var expression in includeExpressions)
            {
                queryableReadModel = queryableReadModel.Include(expression);
            }

            readModel = queryableReadModel.SingleOrDefault(singlePredicate);

            if (readModel == null)
            {
                queryableReadModel = this.DbContext.Set<TReadModel>();
                foreach (var expression in includeExpressions)
                {
                    queryableReadModel = queryableReadModel.Include(expression);
                }

                readModel = queryableReadModel.SingleOrDefault(singlePredicate);
            }

            return readModel;
        }

        /// <summary>
        /// filter records.
        /// </summary>
        /// <param name="predicate">predicate.</param>
        /// <returns>list.</returns>
        public IEnumerable<TReadModel> Where(Guid tenantId, Expression<Func<TReadModel, bool>> predicate)
        {
            predicate = this.AppendTenantIdToPredicate(tenantId, predicate);
            return this.NewReadModels.AsQueryable().Where(predicate).ToList()
                .Concat(this.DbContext.Set<TReadModel>().Select(s => s).Where(predicate));
        }

        /// <summary>
        /// filter records.
        /// </summary>
        /// <param name="includeExpressions">include expressions.</param>
        /// <param name="predicate">predicate.</param>
        /// <returns>list.</returns>
        public IEnumerable<TReadModel> WhereWithIncludes(Guid tenantId, List<Expression<Func<TReadModel, object>>> includeExpressions, Expression<Func<TReadModel, bool>> predicate)
        {
            predicate = this.AppendTenantIdToPredicate(tenantId, predicate);
            var queryableReadModel = this.NewReadModels.AsQueryable();
            IEnumerable<TReadModel> readModel = null;
            foreach (var expression in includeExpressions)
            {
                queryableReadModel = queryableReadModel.Include(expression);
            }

            readModel = queryableReadModel.Where(predicate).ToList();

            if (!readModel.Any())
            {
                queryableReadModel = this.DbContext.Set<TReadModel>();
                foreach (var expression in includeExpressions)
                {
                    queryableReadModel = queryableReadModel.Include(expression);
                }

                readModel = queryableReadModel.Where(predicate);
            }

            return readModel.ToList();
        }

        /// <inheritdoc/>
        public virtual void Add(TReadModel readModel)
        {
            this.NewReadModels.Add(readModel);
            this.DbContext.Set<TReadModel>().Add(readModel);
        }

        /// <inheritdoc/>
        public virtual void DeleteById(Guid tenantId, Guid id)
        {
            var predicate = this.AppendTenantIdToIdPredicate(tenantId, id);

            // delete from the local list
            this.NewReadModels = this.NewReadModels.AsQueryable().Where(predicate.Not()).ToList();

            // delete from the db
            var readModelsToDelete = this.DbContext.Set<TReadModel>().SingleOrDefault(predicate);
            if (readModelsToDelete != null)
            {
                this.DbContext.Set<TReadModel>().Remove(readModelsToDelete);
            }
        }

        /// <inheritdoc/>
        public virtual void Delete(Guid tenantId, Expression<Func<TReadModel, bool>> predicate)
        {
            predicate = this.AppendTenantIdToPredicate(tenantId, predicate);

            // delete from the local list
            this.NewReadModels = this.NewReadModels.AsQueryable().Where(predicate.Not()).ToList();

            // delete from the db
            var readModelsToDelete = this.DbContext.Set<TReadModel>().Where(predicate);
            foreach (TReadModel readModel in readModelsToDelete)
            {
                this.DbContext.Set<TReadModel>().Remove(readModel);
            }
        }

        /// <inheritdoc/>
        public virtual void AddOrUpdate(Guid tenantId, TReadModel readModel, Expression<Func<TReadModel, bool>> predicate)
        {
            predicate = this.AppendTenantIdToPredicate(tenantId, predicate);
            var readModelMaybe = Maybe<TReadModel>.From(this.NewReadModels.AsQueryable().SingleOrDefault(predicate));
            if (readModelMaybe.HasValue)
            {
                this.NewReadModels.Remove(readModelMaybe.Value);
                this.DbContext.Set<TReadModel>().Local.Remove(readModelMaybe.Value);
            }

            this.NewReadModels.Add(readModel);
            this.DbContext.Set<TReadModel>().Add(readModel);
        }

        /// <summary>
        /// Event handler called when the DbContext is about to save.
        /// </summary>
        /// <param name="sender">the DbContext.</param>
        /// <param name="e">the args (empty).</param>
        public virtual void SavingChangesHandler(object sender, EventArgs e)
        {
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
}
