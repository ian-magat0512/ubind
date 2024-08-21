// <copyright file="DataTableContentRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Entity;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Dapper;
    using UBind.Domain.Entities;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Models.DataTable;
    using UBind.Domain.Repositories;
    using UBind.Persistence.DataTable;
    using UBind.Persistence.Helpers;
    using static UBind.Domain.Models.DataTable.DataTableModel;

    /// <inheritdoc/>
    public class DataTableContentRepository : IDataTableContentRepository
    {
        private const string CreateIfNotExistSchema =
            "IF NOT EXISTS(SELECT 1 FROM sys.schemas WHERE name = N'{0}') EXEC('CREATE SCHEMA [{0}]'); ";

        private const int DefaultConnectionTimeout = 1200;
        private const string CreateTableWithSchema = "CREATE TABLE [{0}].";
        private const string CreateTable = "CREATE TABLE ";
        private const string Schema = "DataTables";
        private const int DefaultBulkCopyConnectionTimeout = 3600;
        private const int DefaultBulkCopyBatchSize = 100000;
        private const int DefaultPageSize = 10000;
        private readonly IUBindDbContext context;
        private readonly IDataTableContentDbFactory dataTableDbFactory;

        public DataTableContentRepository(
            IUBindDbContext context,
            IDataTableContentDbFactory dataTableDbFactory)
        {
            this.context = context;
            this.dataTableDbFactory = dataTableDbFactory;
        }

        /// <inheritdoc />
        public async Task CreateDataTableContentSchema(
            Domain.Entities.DataTableDefinition dataTableDefinition,
            DataTableSchema dataTableSchema,
            DbContextTransaction? dbTransaction = null)
        {
            try
            {
                await this.CreateSchemaIfNotExists(Schema, dbTransaction);
                await this.CreateTableAndIndexes(dataTableDefinition.DatabaseTableName, dataTableSchema, dbTransaction);
                await this.CreateOrAlterSchemaBoundView(
                    dataTableDefinition.Id,
                    dataTableDefinition.DatabaseTableName,
                    dataTableSchema.Columns,
                    dbTransaction);
            }
            catch
            {
                throw;
            }
        }

        public async Task PopulateDataTable(string databaseTableName, System.Data.DataTable csvDataTable)
        {
            try
            {
                using var bulkCopy = this.SetupBulkCopy(databaseTableName);
                this.MapBulkCopyColumns(csvDataTable, bulkCopy);
                await bulkCopy.WriteToServerAsync(csvDataTable);
            }
            catch (SqlException ex)
            {
                this.HandleSqlException(ex);
            }
        }

        public async Task DropDataTableIfExists(string databaseTableName)
        {
            var script = $"DROP TABLE IF EXISTS [{Schema}].[{databaseTableName}];";
            await this.context.Database.Connection.ExecuteAsync(script);
        }

        /// <inheritdoc />
        public async Task DropDataTableContent(Guid datatableDefintionId, string databaseTableName)
        {
            await this.DropDataTableViewIfExists(datatableDefintionId);
            await this.DropDataTableIfExists(databaseTableName);
        }

        public async Task<IEnumerable<dynamic>> GetAllDataTableContent(
            Guid dataTableDefinitionId,
            bool useCache,
            int cacheDurationInSeconds = 60)
        {
            return await this.GetDataTableContent(dataTableDefinitionId, false, useCache, cacheDurationInSeconds, DapperHelper.ToReadOnlyDictionary);
        }

        public async Task<IEnumerable<dynamic>> GetAllDataTableContent(
            Guid dataTableDefinitionId,
            bool useCache,
            int cacheDurationInSeconds = 60,
            Func<dynamic, dynamic>? transformRow = null)
        {
            return await this.GetDataTableContent(dataTableDefinitionId, false, useCache, cacheDurationInSeconds, transformRow);
        }

        /// <inheritdoc />
        public async Task RenameDatabaseTableName(DataTableDefinition dataTableDefinition, string oldDatabaseTableName, string newDatabaseTableName)
        {
            if (oldDatabaseTableName == newDatabaseTableName)
            {
                return;
            }
            var dataTableSchema = dataTableDefinition.GetTableSchema();
            if (dataTableSchema == null)
            {
                // Rename datatable the previous way for definitions without table schema
                // This should be removed once all previous datatables in production are already using table schema
                using (var connection = this.dataTableDbFactory.GetConnection(DefaultConnectionTimeout))
                {
                    await this.CreateACopyTable(oldDatabaseTableName, newDatabaseTableName);
                    await this.DropAndRecreateSchemaBoundView(dataTableDefinition.Id, newDatabaseTableName);
                }
            }
            else
            {
                // Rename datatable with its table schema
                await this.CreateTableAndIndexes(newDatabaseTableName, dataTableSchema);
                await this.CreateOrAlterSchemaBoundView(
                    dataTableDefinition.Id,
                    dataTableDefinition.DatabaseTableName,
                    dataTableSchema.Columns);
                await this.PopulateRenamedTable(dataTableSchema, oldDatabaseTableName, newDatabaseTableName);
            }
            await this.DropDataTableIfExists(oldDatabaseTableName);
        }

        /// <inheritdoc/>
        public async Task CreateOrAlterSchemaBoundView(
            Guid datatableDefinitionId,
            string databaseTableName,
            IEnumerable<DataTableModel.Column> columns,
            DbContextTransaction? dbTransaction = null)
        {
            await this.CreateSchemaIfNotExists(Schema, dbTransaction);
            try
            {
                var columnNames = string.Join(", ", columns.Select(x => x.PascalizedAlias).ToList());
                var script = $"CREATE OR ALTER VIEW [{Schema}].[DataTableView_{datatableDefinitionId.ToString().HyphenToUnderscore()}] " +
                    $" WITH SCHEMABINDING AS " +
                    $" SELECT {columnNames} FROM [{Schema}].[{databaseTableName}]; ";
                await this.context.Database.Connection.ExecuteAsync(script, transaction: dbTransaction?.UnderlyingTransaction);
            }
            catch
            {
                throw;
            }
        }

        public async Task DropSchemaBoundViewIfExists(Guid dataTableDefinitionId, IDbTransaction? dbTransaction = null)
        {
            var script = $"DROP VIEW IF EXISTS [{Schema}].[DataTableView_{dataTableDefinitionId.ToString().HyphenToUnderscore()}];";
            await this.context.Database.Connection.ExecuteAsync(script, transaction: dbTransaction);
        }

        /// <inheritdoc />
        public IEnumerable<dynamic> GetDataTableContentWithFilter<TData>(
            Guid datatableDefinitionId,
            int? resultCount = DefaultPageSize,
            Expression<Func<TData, bool>>? predicate = null)
            where TData : class
        {
            var tableName = this.GetTableName(datatableDefinitionId);
            var columnDataTypeDictionary = this.GetColumnAndTypeFromTable(tableName);
            var schemaTableName = $"[{Schema}].[{tableName}]";
            var sqlQueryBuilder = new SqlQueryBuilder(schemaTableName)
                                .SetResultCount(resultCount)
                                .SetColumnDataTypes(columnDataTypeDictionary);
            var query = sqlQueryBuilder.Build(predicate);
            using (var connection = this.dataTableDbFactory.GetConnection(DefaultConnectionTimeout))
            {
                var result = connection.Query(query);
                result = result.Select(d => DapperHelper.ToReadOnlyDictionary(d));
                return result;
            }
        }

        public async Task ReseedDataTableContentToCache(DataTableDefinition dataTableDefinition)
        {
            var cacheKey = this.GetDataTableContentCacheKey(dataTableDefinition.Id);
            MemoryCachingHelper.Remove(cacheKey);
            if (dataTableDefinition.MemoryCachingEnabled)
            {
                await MemoryCachingHelper.AddOrGetAsync(
                    cacheKey,
                    async () => { return await this.GetDataTableContent(dataTableDefinition.Id, false, false, 0); },
                    DateTimeOffset.Now.AddSeconds(dataTableDefinition.CacheExpiryInSeconds));
            }
        }

        private string GetTableName(Guid dataTableDefinitionId)
        {
            return $"DataTableView_{dataTableDefinitionId.ToString().HyphenToUnderscore()}";
        }

        private async Task<IEnumerable<dynamic>> GetDataTableContent(
            Guid dataTableDefinitionId,
            bool isLimited,
            bool useCache,
            int cacheDurationInSeconds = 60,
            Func<dynamic, dynamic>? transformRow = null)
        {
            var executeQuery = async () =>
            {
                using (var connection = this.dataTableDbFactory.GetConnection(DefaultConnectionTimeout))
                {
                    var limit = isLimited ? $"TOP {DefaultPageSize}" : string.Empty;
                    var script = $"SELECT {limit} * FROM [{Schema}].[DataTableView_{dataTableDefinitionId.ToString().HyphenToUnderscore()}];";

                    var result = await connection.QueryAsync(script);
                    if (transformRow != null)
                    {
                        result = result.Select(d => transformRow(d));
                    }
                    return result;
                }
            };

            if (useCache)
            {
                var result = await MemoryCachingHelper.AddOrGetAsync(
                    this.GetDataTableContentCacheKey(dataTableDefinitionId),
                    executeQuery,
                    DateTimeOffset.Now.AddSeconds(cacheDurationInSeconds));
                return result;
            }
            else
            {
                return await executeQuery();
            }
        }

        private SqlBulkCopy SetupBulkCopy(string databaseTableName)
        {
            return this.dataTableDbFactory.GetBulkCopy(
                       SqlBulkCopyOptions.KeepIdentity,
                       DefaultBulkCopyBatchSize,
                       DefaultBulkCopyConnectionTimeout,
                       $"{Schema}.{databaseTableName}",
                       false);
        }

        private void MapBulkCopyColumns(System.Data.DataTable csvDataTable, SqlBulkCopy bulkCopy)
        {
            foreach (DataColumn column in csvDataTable.Columns)
            {
                bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
            }
        }

        private void HandleSqlException(SqlException ex)
        {
            if (ex.Message.ToLower().Contains("Violation of UNIQUE KEY constraint".ToLower()))
            {
                throw new ErrorException(Domain.Errors.DataTableDefinition
                    .CsvDataColumnValueNotUnique(ex.Message));
            }
            throw ex;
        }

        private async Task<bool> TableColumnsHaveChanged(string oldDatabaseTableName, string newDatabaseTableName)
        {
            var oldTableColumns = await this.GetColumnsFromTable(oldDatabaseTableName);
            var newTableColumns = await this.GetColumnsFromTable(newDatabaseTableName);

            var orderedOldTableColumns = oldTableColumns.OrderBy(x => x).ToList();
            var orderedNewTableColumns = newTableColumns.OrderBy(x => x).ToList();

            return !orderedOldTableColumns.SequenceEqual(orderedNewTableColumns);
        }

        private async Task DropDataTableViewIfExists(Guid datatableDefinitionId)
        {
            using (var connection = this.dataTableDbFactory.GetConnection(DefaultConnectionTimeout))
            {
                var script = $"DROP VIEW IF EXISTS [{Schema}].[DataTableView_{datatableDefinitionId.ToString().HyphenToUnderscore()}];";
                await connection.ExecuteAsync(script);
            }
        }

        private async Task CreateDataTablesAndSchema(string databaseTableName, System.Data.DataTable csvDataTable)
        {
            await this.CreateSchemaIfNotExists(Schema);
            var columnsScriptText = string.Empty;

            foreach (var item in csvDataTable.Columns)
            {
                columnsScriptText += $"[{item}][nvarchar](max) NULL,";
            }

            using (var connection = this.dataTableDbFactory.GetConnection(DefaultConnectionTimeout))
            {
                var script = $"CREATE TABLE {databaseTableName} ({columnsScriptText});";
                var sqlCreationTableScript = script.Replace(CreateTable, string.Format(CreateTableWithSchema, Schema));
                await connection.ExecuteAsync(sqlCreationTableScript);
            }
        }

        private async Task CreateOrUpdateSchemaBoundView(Guid datatableDefinitionId, string databaseTableName, System.Data.DataTable csvDataTable)
        {
            await this.CreateSchemaIfNotExists(Schema);
            var columnTextArray = new List<string>();

            foreach (var item in csvDataTable.Columns)
            {
                columnTextArray.Add(item.ToString());
            }

            using (var connection = this.dataTableDbFactory.GetConnection(DefaultConnectionTimeout))
            {
                var script = $"CREATE OR ALTER VIEW [{Schema}].[DataTableView_{datatableDefinitionId.ToString().HyphenToUnderscore()}] WITH SCHEMABINDING AS " +
                   $"SELECT {string.Join(", ", columnTextArray.Select(t => $"[{t}]"))} FROM [{Schema}].[{databaseTableName}];";
                await connection.ExecuteAsync(script);
            }
        }

        /// <summary>
        /// Drop and recreate the schema-bound view to point to new source table within a transaction.
        /// </summary>
        private async Task DropAndRecreateSchemaBoundView(Guid dataTableDefinitionId, string sourceTable)
        {
            List<string> columns = await this.GetColumnsFromTable(sourceTable);

            using (var connection = this.dataTableDbFactory.GetConnection(DefaultConnectionTimeout))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    var dropViewSql = $"DROP VIEW IF EXISTS [{Schema}].[DataTableView_{dataTableDefinitionId.ToString().HyphenToUnderscore()}];";
                    using (var sqlCommand = new SqlCommand(dropViewSql, connection, transaction))
                    {
                        sqlCommand.ExecuteNonQuery();
                    }

                    var createViewSql = $"CREATE VIEW [{Schema}].[DataTableView_{dataTableDefinitionId.ToString().HyphenToUnderscore()}] WITH SCHEMABINDING AS " +
                       $"SELECT {string.Join(", ", columns.Select(t => $"[{t}]"))} FROM [{Schema}].[{sourceTable}];";

                    using (var sqlCommand = new SqlCommand(createViewSql, connection, transaction))
                    {
                        sqlCommand.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
        }

        private List<(string ColumnName, string DataType)> GetColumnAndTypeFromTable(string sourceTable)
        {
            var columnsQuery = $"SELECT COLUMN_NAME,DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{sourceTable}'";

            // Using the database connection directly for a synchronous operation
            var connection = this.context.Database.Connection;
            var command = connection.CreateCommand();
            command.CommandText = columnsQuery;

            var columns = new List<(string ColumnName, string DataType)>();

            try
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var columnName = reader["COLUMN_NAME"].ToString();
                        var dataType = reader["DATA_TYPE"].ToString();
                        columns.Add((columnName, dataType));
                    }
                }
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
                }
            }

            return columns;
        }

        private async Task<List<string>> GetColumnsFromTable(string sourceTable)
        {
            var columnsQuery = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{sourceTable}';";
            var columns = await this.context.Database.SqlQuery<string>(columnsQuery).ToListAsync();
            return columns;
        }

        private async Task CreateACopyTable(string fromDatabaseTableName, string targetDataBaseTableName)
        {
            using (var connection = this.dataTableDbFactory.GetConnection(DefaultConnectionTimeout))
            {
                // Create a copy of old database table with the new name
                var copyScript = $"SELECT * INTO [{Schema}].[{targetDataBaseTableName}]" +
                   $"FROM [{Schema}].[{fromDatabaseTableName}];";
                await connection.ExecuteAsync(copyScript);
            }
        }

        private async Task PopulateRenamedTable(DataTableSchema tableSchema, string fromDatabaseTableName, string targetDataBaseTableName)
        {
            var columns = tableSchema.PascalizedColumnAliases.Select(x => $"[{x}]").ToList();
            var sqlColumns = string.Join(", ", columns);
            var insertIntoScript = $"INSERT INTO [{Schema}].[{targetDataBaseTableName}] ({sqlColumns}) " +
                $"SELECT {sqlColumns} FROM [{Schema}].[{fromDatabaseTableName}]";
            await this.context.Database.Connection.ExecuteAsync(insertIntoScript);
        }

        private async Task CreateTableAndIndexes(
            string databaseTableName,
            DataTableSchema dataTableSchema,
            DbContextTransaction? transaction = null)
        {
            string sqlCreateTableScript = this.CreateSqlCreateTableScriptText(databaseTableName, dataTableSchema.Columns);
            string sqlCreateClusteredIndexScript = string.Empty;
            string sqlCreateUnclusteredIndexScript = string.Empty;
            var dbTransaction = transaction?.UnderlyingTransaction;

            if (dataTableSchema.ClusteredIndex != null)
            {
                sqlCreateClusteredIndexScript = this.CreateSqlClusteredIndexScriptText(
                   dataTableSchema.ClusteredIndex,
                   databaseTableName,
                   dataTableSchema.Columns);
            }

            if (dataTableSchema.UnclusteredIndexes != null && dataTableSchema.UnclusteredIndexes.Any())
            {
                var unclusteredIndexesScripts = new List<string>();
                foreach (var unclusteredIndex in dataTableSchema.UnclusteredIndexes)
                {
                    unclusteredIndexesScripts.Add(this.CreateSqlUnclusteredIndexScriptText(unclusteredIndex, databaseTableName, dataTableSchema.Columns).Trim());
                }
                sqlCreateUnclusteredIndexScript = string.Join("; ", unclusteredIndexesScripts);
            }

            await this.context.Database.Connection.ExecuteAsync(sqlCreateTableScript, transaction: dbTransaction);
            if (!string.IsNullOrEmpty(sqlCreateClusteredIndexScript))
            {
                await this.context.Database.Connection.ExecuteAsync(sqlCreateClusteredIndexScript, transaction: dbTransaction);
            }
            if (!string.IsNullOrEmpty(sqlCreateUnclusteredIndexScript))
            {
                await this.context.Database.Connection.ExecuteAsync(sqlCreateUnclusteredIndexScript, transaction: dbTransaction);
            }
        }

        private async Task CreateSchemaIfNotExists(string schemaName, DbContextTransaction? dbTransaction = null)
        {
            var schemaCreation = string.Format(CreateIfNotExistSchema, schemaName);
            await this.context.Database.Connection.ExecuteAsync(schemaCreation, transaction: dbTransaction?.UnderlyingTransaction);
        }

        private string CreateSqlCreateTableScriptText(string databaseTableName, IEnumerable<DataTableModel.Column> columns)
        {
            var columnsScripts = new List<string>();
            foreach (var column in columns)
            {
                columnsScripts.Add(this.CreateSqlColumnScriptText(column));
            }
            var columnScriptText = string.Join(", ", columnsScripts);
            var script = $"CREATE TABLE {databaseTableName} ({columnScriptText})";
            return script.Replace(CreateTable, string.Format(CreateTableWithSchema, Schema));
        }

        private string CreateSqlColumnScriptText(DataTableModel.Column column)
        {
            string columnName = column.PascalizedAlias;
            var columnSettings = column.DataType.GetDataTypeSqlSettings();
            if (columnSettings == null)
            {
                throw new ErrorException(Domain.Errors.General.Unexpected(
                    $"When trying to create column '{column.Name}', " +
                    $"there was no column settings found for data type '{column.DataType}'"));
            }

            string requiredConfig = column.Required != null && column.Required == true ? "NOT NULL" : "NULL";
            string uniqueConfig = column.Unique != null && column.Unique == true ? "UNIQUE" : string.Empty;
            string defaultValue = column.DefaultValue != null ? $"DEFAULT {columnSettings.GetSqlDefaultValue(column.DefaultValue)}" : string.Empty;
            string columnScript = $" [{columnName}][{columnSettings.SqlDataType}]{columnSettings.SqlColumnSize} {requiredConfig} {uniqueConfig} {defaultValue} ";

            return columnScript;
        }

        private string CreateSqlClusteredIndexScriptText(
            DataTableModel.ClusteredIndex clusteredIndex,
            string databaseTableName,
            IEnumerable<DataTableModel.Column> columns)
        {
            var keyColumnScripts = new List<string>();

            foreach (var keyColumn in clusteredIndex.KeyColumns)
            {
                keyColumnScripts.Add(this.CreateSqlIndexKeyColumnScript(columns, keyColumn, clusteredIndex.SqlIndexName));
            }

            string keyColumnsScriptText = string.Join(", ", keyColumnScripts);
            string indexScriptText = $"CREATE CLUSTERED INDEX {clusteredIndex.SqlIndexName} ON [{Schema}].[{databaseTableName}] ({keyColumnsScriptText}) ";

            return indexScriptText;
        }

        private string CreateSqlIndexKeyColumnScript(IEnumerable<DataTableModel.Column> columns, KeyColumn keyColumn, string indexName)
        {
            var column = columns.Where(x => x.PascalizedAlias == keyColumn.NormalizedColumnAlias).FirstOrDefault();
            string keyColumnSortOrder = keyColumn.SortOrder != null ? keyColumn.SortOrder.ToString().ToUpper() : string.Empty;
            return $" {column.PascalizedAlias} {keyColumnSortOrder} ".Trim();
        }

        private string CreateSqlUnclusteredIndexScriptText(
            DataTableModel.UnclusteredIndex unclusteredIndex,
            string databaseTableName,
            IEnumerable<DataTableModel.Column> columns)
        {
            try
            {
                var keyColumnScripts = new List<string>();
                foreach (var keyColumn in unclusteredIndex.KeyColumns)
                {
                    keyColumnScripts.Add(this.CreateSqlIndexKeyColumnScript(columns, keyColumn, unclusteredIndex.SqlIndexName));
                }

                string keyColumnsScriptText = string.Join(", ", keyColumnScripts);
                string indexScriptText = $" CREATE NONCLUSTERED INDEX {unclusteredIndex.SqlIndexName} ON [{Schema}].[{databaseTableName}] ({keyColumnsScriptText}) ";

                if (unclusteredIndex.NonKeyColumns != null && unclusteredIndex.NonKeyColumns.Any())
                {
                    var nonKeyColumnScripts = new List<string>();
                    foreach (var nonKeyColumnAlias in unclusteredIndex.NormalizedNonKeyColumns)
                    {
                        var column = columns.Where(x => x.PascalizedAlias == nonKeyColumnAlias).FirstOrDefault();
                        nonKeyColumnScripts.Add(column.PascalizedAlias);
                    }

                    string nonkeyColumnsScriptText = string.Join(", ", nonKeyColumnScripts);

                    indexScriptText += $" INCLUDE ({nonkeyColumnsScriptText}) ";
                }

                return indexScriptText;
            }
            catch
            {
                throw;
            }
        }

        private string GetDataTableContentCacheKey(Guid id)
        {
            return Schema + ":" + id.ToString();
        }
    }
}
