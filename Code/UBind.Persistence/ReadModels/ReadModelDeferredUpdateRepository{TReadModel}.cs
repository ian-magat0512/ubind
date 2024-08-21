// <copyright file="ReadModelDeferredUpdateRepository{TReadModel}.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using CSharpFunctionalExtensions;
    using UBind.Domain;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Repository for writing updated read models, which deferrs adding them to the dbContext until there's a dbContext save event.
    /// </summary>
    /// <typeparam name="TReadModel">The type of the read model.</typeparam>
    public class ReadModelDeferredUpdateRepository<TReadModel> : ReadModelUpdateRepository<TReadModel>, IWritableReadModelRepository<TReadModel>
        where TReadModel : class, IReadModel<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadModelDeferredUpdateRepository{TReadModel}"/> class.
        /// </summary>
        /// <param name="dbContext">The UBind database context.</param>
        public ReadModelDeferredUpdateRepository(IUBindDbContext dbContext)
            : base(dbContext)
        {
        }

        /// <inheritdoc/>
        public override void Add(TReadModel readModel)
        {
            this.NewReadModels.Add(readModel);
        }

        /// <inheritdoc/>
        public override void AddOrUpdate(Guid tenantId, TReadModel readModel, Expression<Func<TReadModel, bool>> predicate)
        {
            predicate = this.AppendTenantIdToPredicate(tenantId, predicate);
            var readModelMaybe = Maybe<TReadModel>.From(this.NewReadModels.AsQueryable().SingleOrDefault(predicate));
            if (readModelMaybe.HasValue)
            {
                this.NewReadModels.Remove(readModelMaybe.Value);
            }

            this.NewReadModels.Add(readModel);
        }

        /// <summary>
        /// Event handler called when the DbContext is about to save.
        /// </summary>
        /// <param name="sender">the DbContext.</param>
        /// <param name="e">the args (empty).</param>
        public override void SavingChangesHandler(object sender, EventArgs e)
        {
            this.AddUnsavedToDbContext();
        }

        /// <summary>
        /// Adds the new read models to the db context.
        /// </summary>
        public void AddUnsavedToDbContext()
        {
            foreach (TReadModel readModel in this.NewReadModels)
            {
                this.DbContext.Set<TReadModel>().Add(readModel);
            }

            this.NewReadModels.Clear();
        }
    }
}
