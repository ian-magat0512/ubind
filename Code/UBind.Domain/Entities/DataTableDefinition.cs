// <copyright file="DataTableDefinition.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>
#pragma warning disable CS8618

namespace UBind.Domain.Entities
{
    using System;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Models.DataTable;

    /// <summary>
    /// Data table entity.
    /// </summary>
    public class DataTableDefinition : MutableEntity<Guid>
    {
        private DataTableDefinition(
            Guid tenantId,
            EntityType entityType,
            Guid entityId,
            string name,
            string alias,
            bool memoryCachingEnabled,
            int cacheExpiryInSeconds,
            string tableSchemaJson,
            int columnCount,
            long recordCount,
            Instant modifiedTime)
            : base(Guid.NewGuid(), modifiedTime)
        {
            this.TenantId = tenantId;
            this.EntityType = entityType;
            this.EntityId = entityId;
            this.Name = name;
            this.Alias = alias;
            this.RecordCount = recordCount;
            this.LastModifiedTimestamp = modifiedTime;
            this.ColumnCount = columnCount;
            this.TableSchemaJson = tableSchemaJson;
            this.MemoryCachingEnabled = memoryCachingEnabled;
            this.CacheExpiryInSeconds = cacheExpiryInSeconds;
            this.DatabaseTableName = name;
        }

        private DataTableDefinition()
        {
        }

        public Guid TenantId { get; private set; }

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

        public string Name { get; private set; }

        public string Alias { get; private set; }

        public string TableSchemaJson { get; private set; }

        public string DatabaseTableName { get; private set; }

        public bool IsDeleted { get; private set; }

        public int ColumnCount { get; private set; }

        /// <summary>
        /// Gets or sets the number of rows of the data table content.
        /// </summary>
        public long RecordCount { get; private set; }

        /// <summary>
        /// Gets or sets the property that indicates that the data table is going to be cached to memory.
        /// </summary>
        public bool MemoryCachingEnabled { get; private set; }

        /// <summary>
        /// Gets or sets the property that if data table is cached, set its cache duration to this value.
        /// </summary>
        public int CacheExpiryInSeconds { get; private set; }

        private DataTableSchema? TableSchema { get; set; }

        public static DataTableDefinition Create(
            Guid tenantId,
            EntityType entityType,
            Guid entityId,
            string name,
            string alias,
            bool cacheInMemory,
            int cacheExpiryInSeconds,
            string tableSchemaJson,
            int columnCount,
            long recordCount,
            Instant modifiedTime)
        {
            if (!IsSupportedEntity(entityType))
            {
                throw new NotSupportedException($"Entity \'{entityType}\' is not supported for creating data table.");
            }

            return new DataTableDefinition(
                tenantId,
                entityType,
                entityId,
                name,
                alias,
                cacheInMemory,
                cacheExpiryInSeconds,
                tableSchemaJson,
                columnCount,
                recordCount,
                modifiedTime);
        }

        public static bool IsSupportedEntity(EntityType entityType)
        {
            return entityType == EntityType.Tenant || entityType == EntityType.Product || entityType == EntityType.Organisation;
        }

        public void UpdateName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ErrorException(Errors.DataTableDefinition.NameIsEmpty());
            }

            this.Name = name;
        }

        public void UpdateAlias(string alias)
        {
            if (string.IsNullOrEmpty(alias))
            {
                throw new ErrorException(Errors.DataTableDefinition.AliasIsEmpty());
            }

            this.Alias = alias;
        }

        public void UpdateRecordAndColumnCount(long recordCount, int columnCount)
        {
            this.RecordCount = recordCount;
            this.ColumnCount = columnCount;
        }

        public void UpdateTableSchemaJson(string tableSchemaJson)
        {
            if (string.IsNullOrEmpty(tableSchemaJson))
            {
                throw new ErrorException(Errors.DataTableDefinition.TableSchemaIsEmpty());
            }

            this.TableSchemaJson = tableSchemaJson;
        }

        public void Delete()
        {
            if (this.IsDeleted == true)
            {
                throw new ErrorException(Errors.DataTableDefinition.AlreadyDeleted());
            }

            this.IsDeleted = true;
        }

        public void UpdateDatabaseTableName(string databaseTableName)
        {
            // Pattern: {{tenantAlias}}_{{dataTableAlias}}
            var tenantPattern = new Regex(@"^[A-Z][a-z]*(?:[A-Z][a-z]*)*_[A-Z][a-z]*(?:[A-Z][a-z]*)*$");

            // Pattern: {{tenantAlias}}_{{EntityType}}_{{organisationAlias}}_{{dataTableAlias}}
            var nonTenantPattern = new Regex(@"^[A-Z][a-z]*(?:[A-Z][a-z]*)*_[A-Z][a-z]*(?:[A-Z][a-z]*)*_[A-Z][a-z]*(?:[A-Z][a-z]*)*_[A-Z][a-z]*(?:[A-Z][a-z]*)*$");

            if (!tenantPattern.Match(databaseTableName).Success
                && !nonTenantPattern.Match(databaseTableName).Success
                && databaseTableName.Length > 128)
            {
                throw new ErrorException(Errors.DataTableDefinition.InvalidDatabaseTableName(databaseTableName));
            }

            this.DatabaseTableName = databaseTableName;
        }

        public DataTableSchema? GetTableSchema()
        {
            if (this.TableSchema == null && this.TableSchemaJson != null)
            {
                this.TableSchema = JsonConvert.DeserializeObject<DataTableSchema>(this.TableSchemaJson);
            }

            return this.TableSchema;
        }

        public void UpdateMemoryCachingStatus(bool memoryCachingEnabled, int cacheExpiryInSeconds)
        {
            this.MemoryCachingEnabled = memoryCachingEnabled;
            this.CacheExpiryInSeconds = cacheExpiryInSeconds;
        }
    }
}
