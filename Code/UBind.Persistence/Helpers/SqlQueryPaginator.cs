// <copyright file="SqlQueryPaginator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Persistence.Helpers;

using Dapper;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using UBind.Domain.Enums;
using UBind.Domain.Helpers;
using UBind.Domain.ReadModel;

/// <summary>
/// This is a helper class to apply pagination and order to queries
/// </summary>
public static class SqlQueryPaginator
{
    private static readonly Regex RegexColumnNamePattern = new Regex(
        @"^[a-zA-Z_][a-zA-Z0-9_]*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Creates a paginated query
    /// </summary>
    /// <param name="selectStatement">the select statement</param>
    /// <param name="filter">the filters to apply that includes paging and order conditions.</param>
    /// <param name="dynamicParameters">the parameters to be used in the query</param>
    /// <returns>SQL query string</returns>
    public static string BuildPaginatedSqlQuery(
            string selectStatement,
            DynamicParameters dynamicParameters,
            EntityListFilters? filter = null)
    {
        if (string.IsNullOrEmpty(selectStatement))
        {
            throw new ArgumentNullException(nameof(selectStatement), "SELECT statement cannot be null or empty");
        }

        StringBuilder query = new StringBuilder();
        Pagination pagination = Pagination.FromFilter(filter);
        dynamicParameters.Add("@PageSize", pagination.PageSize, DbType.Int32);
        string orderBy = string.Empty;
        if (filter != null)
        {
            orderBy = GetOrderClause(filter);
        }

        if (pagination.NumberOfRecordsToSkip == 0)
        {
            // replace only the outermost select statement
            var sqlQueryBody = selectStatement.Substring("SELECT".Length);
            var selectStatementWithPageSize = $"SELECT TOP (@PageSize) {sqlQueryBody}";
            query.Append(selectStatementWithPageSize);
            query.Append(orderBy);
        }
        else
        {
            dynamicParameters.Add("@Offset", pagination.NumberOfRecordsToSkip, DbType.Int32);
            query.Append(selectStatement);
            query.Append(orderBy);
            query.Append($" OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY");
        }

        return query.ToString();
    }

    private static string GetOrderClause(EntityListFilters filter)
    {
        if (string.IsNullOrEmpty(filter.SortBy))
        {
            return string.Empty;
        }

        var isValidColumnName = RegexColumnNamePattern.IsMatch(filter.SortBy);
        if (!isValidColumnName)
        {
            throw new ArgumentException(string.Format("Invalid column name : {0}", filter.SortBy));
        }

        string orderBy = $" ORDER BY {filter.SortBy} ";
        if (filter.SortOrder == SortDirection.Descending)
        {
            orderBy += "DESC";
        }
        else
        {
            orderBy += "ASC";
        }
        return orderBy;
    }
}
