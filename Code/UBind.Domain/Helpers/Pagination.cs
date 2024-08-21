// <copyright file="Pagination.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Helpers;

using UBind.Domain.ReadModel;

public class Pagination
{
    public Pagination(int currentSkip, int pageSize)
    {
        this.NumberOfRecordsToSkip = currentSkip;
        this.PageSize = pageSize;
    }

    /// <summary>
    /// Gets or sets the number of records to skip.
    /// </summary>
    public int NumberOfRecordsToSkip { get; set; }

    /// <summary>
    /// Gets or sets the number of records to take.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets the pagination values from the filter
    /// if there is otherwise uses a default pagination
    /// </summary>
    /// <param name="filter"></param>
    /// <returns>An object that contains values for how many rows to skip and how many rows to retrieve in a query</returns>
    public static Pagination FromFilter(EntityListFilters? filter = null)
    {
        int pageSizeNormal = (int)Domain.Enums.PageSize.Normal;
        int pageSizeDefault = (int)Domain.Enums.PageSize.Default;
        int recordsToSkip = 0;
        if (filter == null || !filter.PageSize.HasValue && !filter.Page.HasValue)
        {
            return new Pagination(recordsToSkip, pageSizeDefault);
        }

        if (filter.PageSize.HasValue)
        {
            if (filter.PageSize < 1)
            {
                throw new ArgumentException(string.Format("Invalid page size : {0}", filter.PageSize));
            }

            pageSizeNormal = filter.PageSize.Value;

            if (pageSizeNormal > (int)Domain.Enums.PageSize.Default)
            {
                pageSizeNormal = (int)Domain.Enums.PageSize.Default;
            }
        }

        if (pageSizeNormal < 1)
        {
            throw new ArgumentException(string.Format("Invalid page size : {0}", pageSizeNormal));
        }

        if (!filter.Page.HasValue)
        {
            return new Pagination(recordsToSkip, pageSizeNormal);
        }

        int page = filter.Page.HasValue ? filter.Page.Value : 1;
        int currentSkip = (page - 1) * pageSizeNormal;

        if (page < 1)
        {
            throw new ArgumentException(string.Format("Invalid page value : {0}", page));
        }

        return new Pagination(currentSkip, pageSizeNormal);
    }
}
