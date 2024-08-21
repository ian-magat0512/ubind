// <copyright file="DataTableQueryList.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.List
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using MorseCode.ITask;
    using UBind.Application.Automation.Filters;
    using UBind.Application.Automation.Providers.Expression;

    /// <inheritdoc />
    public class DataTableQueryList<TData> : IDataList<TData>
        where TData : class
    {
        private IQueryable<TData> query;
        private Expression<Func<TData, bool>>? predicate;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataTableQueryList{TData}"/> class.
        /// </summary>
        /// <param name="queryCallback">The callback is a function that will return the results of the query.</param>
        public DataTableQueryList(Func<int?, Expression<Func<TData, bool>>?, IEnumerable<TData>> queryCallback)
        {
            this.QueryResultsCallback = queryCallback;
        }

        public DataTableQueryList(IEnumerable<TData> dataListQuery)
        {
            this.Query = dataListQuery.AsQueryable();
        }

        /// <inheritdoc />
        public Type DataType => this.Query.ElementType;

        /// <inheritdoc />
        public IQueryable<TData> Query
        {
            get
            {
                // if this is empty, call the query callback to get the data.
                if (this.query == null)
                {
                    this.query = this.QueryResultsCallback(null, this.predicate).AsQueryable();
                }

                return this.query;
            }

            set
            {
                this.query = value;
            }
        }

        /// <summary>
        /// Gets or sets the query callback, the function that will return the results of the query.
        /// int? is the top ( optional ), and string is the where clause ( optional ) that returns an IEnumerable of TData.
        /// </summary>
        public Func<int?, Expression<Func<TData, bool>>?, IEnumerable<TData>> QueryResultsCallback { get; }

        /// <inheritdoc />
        public Data<int> Count()
        {
            return this.Query.Count();
        }

        /// <inheritdoc />
        public async ITask<TData> FirstOrDefault(string? itemAlias, IFilterProvider filterProvider, IProviderContext providerContext)
        {
            var alias = itemAlias ?? AliasFactory.Generate(typeof(object));
            var predicate = await filterProvider.Resolve<TData>(providerContext, this.CreateScope(alias));
            this.SetQuery(1, predicate);
            return this.Query.FirstOrDefault();
        }

        /// <inheritdoc />
        public IEnumerator<TData> GetEnumerator()
        {
            return this.Query.GetEnumerator();
        }

        /// <inheritdoc />
        public Type GetInnerType()
        {
            return typeof(TData);
        }

        /// <inheritdoc />
        public dynamic GetValueFromGeneric()
        {
            return this.Query;
        }

        /// <inheritdoc />
        public async ITask<IDataList<TData>> Where(string? itemAlias, IFilterProvider filterProvider, IProviderContext providerContext)
        {
            var alias = itemAlias ?? AliasFactory.Generate(typeof(object));
            var predicate = await filterProvider.Resolve<TData>(providerContext, this.CreateScope(alias));
            this.SetQuery(null, predicate);
            return new DataTableQueryList<TData>(this.Query);
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private ExpressionScope CreateScope(string itemAlias) =>
            new ExpressionScope(itemAlias, Expression.Parameter(typeof(TData)));

        /// <summary>
        /// Set the query based on the top and predicate that will be translated into sql query.
        /// This will mostly be called when you filter the result using FirstOrDefault() or Where()
        /// </summary>
        /// <param name="top">How many records will be retrieved? ex. 1 - 10000</param>
        /// <param name="predicate">The filter coming from the filter provider.</param>
        private void SetQuery(int? top, Expression<Func<TData, bool>>? predicate)
        {
            this.predicate = predicate;

            // if the callback is present, pass in filters, call to get results.
            if (this.QueryResultsCallback != null)
            {
                this.Query = this.QueryResultsCallback(top, this.predicate).AsQueryable();
            }

            // if there is no callback but the query is present, filter the query in memory.
            else if (this.predicate != null && this.Query != null)
            {
                this.Query = top != null
                    ? this.Query.Take(top.Value).Where(this.predicate)
                    : this.Query = this.Query.Where(this.predicate);
            }
        }
    }
}
