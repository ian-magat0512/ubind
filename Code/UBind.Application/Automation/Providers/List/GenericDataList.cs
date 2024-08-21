// <copyright file="GenericDataList.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

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

    /// <summary>
    /// Represents a collection of generic IData.
    /// </summary>
    /// <typeparam name="TData">The type of data in the list.</typeparam>
    public class GenericDataList<TData> : IDataList<TData>
    {
        public GenericDataList(IEnumerable<TData> dataList)
        {
            this.Query = dataList.AsQueryable();
        }

        public Type DataType => this.Query.ElementType;

        public IQueryable<TData> Query { get; }

        public Type GetInnerType()
        {
            return typeof(TData);
        }

        public Data<int> Count()
        {
            return this.Query.Count();
        }

        public async ITask<TData?> FirstOrDefault(string itemAlias, IFilterProvider? filterProvider, IProviderContext providerContext)
        {
            if (filterProvider == null)
            {
                return this.Query.FirstOrDefault();
            }

            var predicate = await filterProvider.Resolve<TData>(providerContext);
            return this.Query.FirstOrDefault(predicate);
        }

        public IEnumerator<TData> GetEnumerator()
        {
            return this.Query.GetEnumerator();
        }

        public dynamic GetValueFromGeneric()
        {
            return this.Query;
        }

        public async ITask<IDataList<TData>> Where(string? itemAlias, IFilterProvider filterProvider, IProviderContext providerContext)
        {
            var alias = itemAlias ?? AliasFactory.Generate(this.DataType);
            var predicate = await filterProvider.Resolve<TData>(providerContext, this.CreateScope(alias));

            return new GenericDataList<TData>(this.Query.Where(predicate));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private ExpressionScope CreateScope(string itemAlias) =>
            new ExpressionScope(itemAlias, Expression.Parameter(typeof(TData)));
    }
}
