// <copyright file="QueryableExtensionsImplementation.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Extensions.Implementations
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;
    using UBind.Domain.Attributes;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.ReadModel;

    public class QueryableExtensionsImplementation : IQueryableExtensionsImplementation
    {
        public IQueryable<T> Paginate<T>(IQueryable<T> list, EntityListFilters filter)
        {
            Pagination pagination = Pagination.FromFilter(filter);
            if (pagination.NumberOfRecordsToSkip == 0)
            {
                return list.Take(() => pagination.PageSize);
            }

            return list.Skip(() => pagination.NumberOfRecordsToSkip).Take(() => pagination.PageSize);
        }

        public IOrderedQueryable<T> Order<T>(IQueryable<T> list, string propertyName, SortDirection sortDirection)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException($"Invalid property name: {propertyName}");
            }

            Type type = typeof(T);
            Type propertyType = typeof(T);
            ParameterExpression paramExpression = Expression.Parameter(type, "sortBy");
            MemberExpression? memberExpression = null;
            foreach (string property in propertyName.Split('.'))
            {
                if (memberExpression == null)
                {
                    memberExpression = Expression.Property(paramExpression, property);
                }
                else
                {
                    memberExpression = Expression.Property(memberExpression, property);
                }

                propertyType = memberExpression.Type;
            }

            Expression expression = Expression.Convert(memberExpression!, propertyType);
            Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), propertyType);
            var lamdaExpression = Expression.Lambda(delegateType, expression, paramExpression);

            MethodCallExpression sortByExpression = Expression.Call(
                  typeof(Queryable),
                  sortDirection.GetAttributeOfType<QueryOrderMethodAttribute>().OrderMethod,
                  new Type[] { type, propertyType },
                  list.Expression,
                  lamdaExpression);
            return (IOrderedQueryable<T>)list.Provider.CreateQuery<T>(sortByExpression);
        }
    }
}
