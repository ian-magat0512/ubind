// <copyright file="IDataTableContentRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using UBind.Domain.Entities;
    using UBind.Domain.Models.DataTable;

    /// <summary>
    /// Repository for storing data table content.
    /// </summary>
    public interface IDataTableContentRepository
    {
        /// <summary>
        /// Creates a new table with the provided table name based on datatable configuration.
        /// </summary>
        /// <param name="dataTableDefinition"></param>
        /// <param name="dataTableSchema"></param>
        /// <param name="dbTransaction"></param>
        /// <returns></returns>
        Task CreateDataTableContentSchema(
             DataTableDefinition dataTableDefinition,
             DataTableSchema dataTableSchema,
             DbContextTransaction? dbTransaction = null);

        /// <summary>
        /// Creates a view for a datatable based on provided name and id
        /// </summary>
        /// <param name="datatableDefinitionId"></param>
        /// <param name="databaseTableName"></param>
        /// <param name="columns"></param>
        /// <param name="dbTransaction"></param>
        /// <returns></returns>
        Task CreateOrAlterSchemaBoundView(
           Guid datatableDefinitionId,
           string databaseTableName,
           IEnumerable<DataTableModel.Column> columns,
           DbContextTransaction? dbTransaction = null);

        /// <summary>
        /// Populate created datatable with content from CSV datatable.
        /// This will throw expected SQLExceptions for the validation of data.
        /// </summary>
        /// <param name="databaseTableName"></param>
        /// <param name="csvDataTable"></param>
        /// <returns></returns>
        Task PopulateDataTable(string databaseTableName, System.Data.DataTable csvDataTable);

        /// <summary>
        /// Drop the data table provided with the database table name.
        /// </summary>
        Task DropDataTableContent(Guid dataTableDefinitionId, string databaseTableName);

        /// <summary>
        /// Retrieve all data table content provided by the data table definition ID with added filtering.
        /// </summary>
        /// <param name="datatableDefinitionId">The data table definition Id.</param>
        /// <param name="resultCount">(optional) how many records to retrieve.</param>
        /// <param name="predicate">(optional) where clause string to be appended to the query. (Ex. "type == 1")</param>
        /// <returns></returns>
        IEnumerable<dynamic> GetDataTableContentWithFilter<TData>(
            Guid datatableDefinitionId,
            int? resultCount = 10000,
            Expression<Func<TData, bool>>? predicate = null)
            where TData : class;

        /// <summary>
        /// Reseeds the data table content to cache.
        /// </summary>
        /// /// <returns>Returns nothing.</returns>
        public Task ReseedDataTableContentToCache(DataTableDefinition dataTableDefinition);

        /// <summary>
        /// Retrieve all data table content provided by the data table definition ID.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<dynamic>> GetAllDataTableContent(
            Guid dataTableDefinitionId,
            bool useCache,
            int cacheDurationInSeconds = 60);

        /// <summary>
        /// Retrieve all data table content provided by the data table definition ID
        /// with a custom function to transform each row
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<dynamic>> GetAllDataTableContent(
            Guid dataTableDefinitionId,
            bool useCache,
            int cacheDurationInSeconds = 60,
            Func<dynamic, dynamic>? transformRow = null);

        /// <summary>
        /// Rename database table name
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task RenameDatabaseTableName(DataTableDefinition dataTableDefinition, string oldDatabaseTableName, string newDatabaseTableName);

        /// <summary>
        /// Drop a datatable database table name.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DropDataTableIfExists(string databaseTableName);
    }
}
