// <copyright file="UpdateDataTableFromCsvDataCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.DataTable
{
    using System;
    using UBind.Domain.Entities;
    using UBind.Domain.Models.DataTable;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command to update a data table definition entry, and then recreate the database table
    /// and load the contents of the CSV data into the database table.
    /// The first row of the CSV data will be used to determine the column names of the new database table,
    /// and the rest of the CSV data will be loaded in as the data table content.
    /// </summary>
    public class UpdateDataTableFromCsvDataCommand : ICommand<DataTableDefinition>
    {
        public UpdateDataTableFromCsvDataCommand(
            Guid tenantId,
            Guid definitionId,
            string dataTableName,
            string dataTableAlias,
            string csvData,
            DataTableSchema tableSchema,
            bool memoryCachingEnabled,
            int cacheExpiryInSeconds)
        {
            this.TenantId = tenantId;
            this.DefinitionId = definitionId;
            this.DataTableName = dataTableName;
            this.DataTableAlias = dataTableAlias;
            this.CsvData = csvData;
            this.TableSchema = tableSchema;
            this.MemoryCachingEnabled = memoryCachingEnabled;
            this.CacheExpiryInSeconds = cacheExpiryInSeconds;
        }

        public Guid TenantId { get; private set; }

        public Guid DefinitionId { get; private set; }

        public string DataTableName { get; private set; }

        public string DataTableAlias { get; private set; }

        public string CsvData { get; private set; }

        public DataTableSchema TableSchema { get; private set; }

        public int CacheExpiryInSeconds { get; }

        public bool MemoryCachingEnabled { get; }
    }
}
