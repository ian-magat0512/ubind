// <copyright file="EntityQueryList.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.List
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;
    using MorseCode.ITask;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Filters;
    using UBind.Application.Automation.Providers.Expression;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// Represents a collection of entities generated through a query.
    /// </summary>
    /// <typeparam name="TData">The type of the entity.</typeparam>
    public class EntityQueryList<TData> : IDataList<TData>
        where TData : class
    {
        private readonly Func<IQueryable<TData>, IOrderedQueryable<TData>> order;
        private readonly Queue<TData> queue = new Queue<TData>();
        private int batchesFetched;
        private bool sourceExhausted;
        private int? pageSize;
        private long? pageNumber;
        private int batchSize = 10;
        private CancellationToken cancellationToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityQueryList{TData}"/> class.
        /// </summary>
        /// <param name="source">A query used to generate the collection.</param>
        /// <param name="order">A function for ordering the query so it can be lazily evaluated in blocks.</param>
        public EntityQueryList(IQueryable<TData> source,
            Func<IQueryable<TData>,
            IOrderedQueryable<TData>> order,
            long? pageSize = null,
            long? pageNumber = null,
            CancellationToken cancellationToken = default)
        {
            this.Query = source;
            this.order = order;

            // Value should be from EntityQueryListConfigModel
            this.pageSize = (int?)pageSize;

            // Value should be from EntityQueryListConfigModel
            this.pageNumber = pageNumber;
            this.cancellationToken = cancellationToken;
            this.cancellationToken.ThrowIfCancellationRequested();
        }

        /// <inheritdoc/>
        public Type DataType => this.Query.ElementType;

        /// <summary>
        /// Gets the query used to generate the items in the collection.
        /// </summary>
        public IQueryable<TData> Query { get; }

        /// <inheritdoc/>
        public async ITask<TData?> FirstOrDefault(string itemAlias, IFilterProvider filterProvider, IProviderContext providerContext)
        {
            var predicate = await filterProvider.Resolve<TData>(providerContext, this.CreateScope(itemAlias));
            return this.Query.FirstOrDefault(predicate);
        }

        /// <inheritdoc/>
        public Data<int> Count()
        {
            return this.Query.Count();
        }

        /// <inheritdoc/>
        public async ITask<IDataList<TData>> Where(string? itemAlias, IFilterProvider filterProvider, IProviderContext providerContext)
        {
            var alias = itemAlias ?? AliasFactory.Generate(this.DataType);
            var predicate = await filterProvider.Resolve<TData>(providerContext, this.CreateScope(alias));
            return new EntityQueryList<TData>(this.Query.Where(predicate), this.order, this.pageSize, this.pageNumber, this.cancellationToken);
        }

        /// <inheritdoc/>
        public IEnumerator<TData> GetEnumerator()
        {
            int totalSize = 0, pageCount = 0, batchCallCount = 0;
            int? finalBatchSize = null;
            if (this.pageSize != null)
            {
                if (this.pageSize < this.batchSize)
                {
                    this.batchSize = (int)this.pageSize.Value;
                }

                batchCallCount = (int)Math.Ceiling((double)this.pageSize.Value / this.batchSize);
            }

            // TODO: This seems wrong. It should be 0 based page numbers
            // int pageSkipValue = (int)(((this.pageNumber ?? 0) * (this.pageSize ?? 0));
            int pageSkipValue = (int)(((this.pageNumber ?? 1) - 1) * (this.pageSize ?? 0));

            // TODO: Ensure that all use cases of consumption for EntityQueryList calls
            // GetEnumerator method. UB-11324
            this.ValidateQuery();
            while (true)
            {
                this.cancellationToken.ThrowIfCancellationRequested();
                if (this.queue.Count == 0 && !this.sourceExhausted)
                {
                    // This is to determine if this is the last batch call to the db.
                    if (batchCallCount - 1 == this.batchesFetched)
                    {
                        // This is to determine if the items left to fulfill the page is still equivalent
                        // to the default batch size and if not, we will give the query an adjusted size to take.
                        if (this.pageSize - totalSize != this.batchSize)
                        {
                            // If the last page is lower than the bath size, this will be used as a batch size instead.
                            finalBatchSize = this.pageSize - totalSize;
                        }
                    }

                    List<TData> newEntities = new List<TData>();
                    newEntities = this.order(this.Query)
                        .Skip(pageSkipValue) // Skip from the calculation of the pagination
                        .Skip(this.batchesFetched * this.batchSize) // Skip for the for the batch size
                        .Take(finalBatchSize ?? this.batchSize)
                        .ToList();
                    ++this.batchesFetched;
                    this.sourceExhausted = newEntities.Count < this.batchSize;
                    this.cancellationToken.ThrowIfCancellationRequested();
                    foreach (var entity in newEntities)
                    {
                        totalSize++;
                        this.queue.Enqueue(entity);
                        if (this.pageSize != null && this.pageSize != 0 && totalSize == this.pageSize)
                        {
                            // stop the fetch if the page size is achieved
                            this.sourceExhausted = true;
                            break;
                        }
                    }
                }

                if (this.queue.Count > 0)
                {
                    yield return this.queue.Dequeue();
                }
                else
                {
                    yield break;
                }
            }
        }

        public dynamic GetValueFromGeneric()
        {
            // TODO: Ensure that all use cases of consumption for EntityQueryList calls
            // GetEnumerator method. UB-11324
            this.ValidateQuery();
            return this.Query;
        }

        public Type GetInnerType()
        {
            return typeof(TData);
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private ExpressionScope CreateScope(string itemAlias) =>
            new ExpressionScope(itemAlias, Expression.Parameter(typeof(TData)));

        private void ValidateQuery()
        {
            // Define the maximum allowed row limit as a constant
            // The value was changed from 10k to 15k as temporary fix for UB-12317 while waiting for the completion of UB-11324.
            const int MaxRowLimit = 15000;
            if (this.pageSize == null || this.pageSize > MaxRowLimit)
            {
                var rowCount = this.Query.Count();
                if (rowCount > MaxRowLimit)
                {
                    throw new ErrorException(Errors.Application.CannotRetrieveRowsAboveLimit(rowCount, MaxRowLimit));
                }
            }
        }
    }
}
