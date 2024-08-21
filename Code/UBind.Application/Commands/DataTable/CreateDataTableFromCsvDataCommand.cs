// <copyright file="CreateDataTableFromCsvDataCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.DataTable
{
    using System;
    using UBind.Domain;
    using UBind.Domain.Entities;
    using UBind.Domain.Models.DataTable;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Creates a data table definition entry, and then create a new database table
    /// and load the contents of the CSV data into the new database table.
    /// The first row of the CSV data will be used to determine the column names of the new database table,
    /// and the rest of the CSV data will be loaded in as the data table content.
    /// </summary>
    public class CreateDataTableFromCsvDataCommand : ICommand<DataTableDefinition>
    {
        public CreateDataTableFromCsvDataCommand(
            Guid tenantId,
            EntityType entityType,
            Guid entityId,
            string dataTableName,
            string dataTableAlias,
            string csvData,
            DataTableSchema tableSchema,
            bool memoryCachingEnabled,
            int cacheExpiryInSeconds)
        {
            this.TenantId = tenantId;
            this.EntityType = entityType;
            this.EntityId = entityId;
            this.DataTableName = dataTableName;
            this.DataTableAlias = dataTableAlias;
            this.CsvData = csvData;
            this.TableSchema = tableSchema;
            this.MemoryCachingEnabled = memoryCachingEnabled;
            this.CacheExpiryInSeconds = cacheExpiryInSeconds;
        }

        /// <summary>
        /// Gets the entity type that the data table is associated with,
        /// for example a tenant, organisation or product.
        /// </summary>
        public EntityType EntityType { get; private set; }

        /// <summary>
        /// Gets the entity ID of the entity which the data table
        /// is associated with, for example the id of a tenant, product or organisation.
        /// </summary>
        public Guid EntityId { get; private set; }

        public Guid TenantId { get; private set; }

        public string DataTableName { get; private set; }

        public string DataTableAlias { get; private set; }

        public string CsvData { get; private set; }

        public DataTableSchema TableSchema { get; private set; }

        public bool MemoryCachingEnabled { get; private set; }

        public int CacheExpiryInSeconds { get; private set; }
    }
}
