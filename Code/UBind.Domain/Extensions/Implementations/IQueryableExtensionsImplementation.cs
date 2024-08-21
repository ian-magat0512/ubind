// <copyright file="IQueryableExtensionsImplementation.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Extensions.Implementations
{
    using System.Linq;
    using UBind.Domain.Enums;
    using UBind.Domain.ReadModel;

    public interface IQueryableExtensionsImplementation
    {
        IQueryable<T> Paginate<T>(IQueryable<T> list, EntityListFilters filter);

        IOrderedQueryable<T> Order<T>(IQueryable<T> list, string propertyName, SortDirection sortDirection);
    }
}
