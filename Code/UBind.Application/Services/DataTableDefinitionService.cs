// <copyright file="DataTableDefinitionService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using CsvHelper;
    using Hangfire;
    using Humanizer;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Logging;
    using UBind.Domain;
    using UBind.Domain.Entities;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Models.DataTable;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using static UBind.Domain.Models.DataTable.DataTableModel;

    /// <inheritdoc/>
    public class DataTableDefinitionService : IDataTableDefinitionService
    {
        private readonly IDataTableDefinitionRepository dataTableDefinitionRepository;
        private readonly IDataTableContentRepository dataTableContentRepository;
        private readonly ITenantRepository tenantRepository;
        private readonly IOrganisationReadModelRepository organisationReadModelRepository;
        private readonly IProductRepository productRepository;
        private readonly ICachingResolver cachingResolver;
        private readonly JsonSchemaValidator jsonSchemaValidator;
        private readonly ILogger<DataTableDefinitionService> logger;
        private readonly string dataTableJsonSchemaVersion = "1.0.0";

        public DataTableDefinitionService(
            IDataTableDefinitionRepository dataTableDefinitionRepository,
            IDataTableContentRepository dataTableContentRepository,
            ITenantRepository tenantRepository,
            IOrganisationReadModelRepository organisationReadModelRepository,
            IProductRepository productRepository,
            ICachingResolver cachingResolver,
            IWebHostEnvironment hostingEnvironment,
            ILogger<DataTableDefinitionService> logger)
        {
            this.dataTableDefinitionRepository = dataTableDefinitionRepository;
            this.dataTableContentRepository = dataTableContentRepository;
            this.tenantRepository = tenantRepository;
            this.organisationReadModelRepository = organisationReadModelRepository;
            this.productRepository = productRepository;
            this.cachingResolver = cachingResolver;
            this.jsonSchemaValidator = new JsonSchemaValidator(Path.Combine(hostingEnvironment.ContentRootPath, "schemas", "data-table"));
            this.logger = logger;
        }

        public void ValidateTableSchema(string json)
        {
            this.jsonSchemaValidator.ValidateJsonAgainstSchema(
                    "data-table-table-schema-validation",
                    json,
                    "data-table-table-schema-validation",
                    this.dataTableJsonSchemaVersion);
        }

        /// <summary>
        /// This method will generate a database table name and add index if the name is already in use.
        /// Example patterns:
        ///     - Tenant data table pattern:
        ///         {{tenantAlias}}_{{dataTableAlias}}{{index}}
        ///         DataTables.AbcInsurance_EmbargoPostcodes0001
        ///     - Non tenant data table:
        ///         {{tenantAlias}}_{{EntityType}}_{{organisationAlias}}_{{dataTableAlias}}{{index}}
        ///         DataTables.AbcInsurance_Organisation_RiskSure_EmbargoPostcodes0002.
        /// </summary>
        public async Task<string> GenerateDatabaseTableName(DataTableDefinition dataTableDefinition)
        {
            var entityType = dataTableDefinition.EntityType;
            var baseDatabaseTablename = await this.GenerateDatabaseTableBaseName(dataTableDefinition);

            var existingDataTableDefinitionIndexes = this.dataTableDefinitionRepository
                .GetDataTableDefinitionsByDatabaseTableName(dataTableDefinition.TenantId, baseDatabaseTablename)
                .Select(dt =>
                {
                    var isNumeric = int.TryParse(
                        dt.DatabaseTableName.AsSpan(Math.Max(0, dt.DatabaseTableName.Length - 4)),
                        out int parseResult);
                    return isNumeric ? parseResult.ToString("D4") : "0000";
                }).ToList();

            if (existingDataTableDefinitionIndexes.Any())
            {
                return $"{baseDatabaseTablename}{this.FindAvailableSuffix(baseDatabaseTablename, existingDataTableDefinitionIndexes)}";
            }

            return baseDatabaseTablename;
        }

        /// <summary>
        /// Validates the index column aliases against the columns definition
        /// </summary>
        /// <param name="schema"></param>
        /// <exception cref="ErrorException"></exception>
        public void ValidateTableSchemaIndexes(DataTableSchema schema)
        {
            var columnsDefinition = schema.PascalizedColumnAliases.ToHashSet();
            this.ValidateClusteredIndex(schema, columnsDefinition);
            this.ValidateUnclusteredIndexes(schema, columnsDefinition);
        }

        public void ValidateColumns(DataTableSchema config)
        {
            var duplicateAliases = this.RetrieveDuplicateNonCaseSensitiveItems(config.Columns.Select(x => x.Alias).ToList());
            if (duplicateAliases.Any())
            {
                throw new ErrorException(Errors.DataTableDefinition.ColumnAliasesNotUnique(duplicateAliases.ToList()));
            }
            var duplicateNames = this.RetrieveDuplicateNonCaseSensitiveItems(config.ColumnNames);
            if (duplicateNames.Any())
            {
                throw new ErrorException(Errors.DataTableDefinition.ColumnNamesNotUnique(duplicateNames.ToList()));
            }
            this.ValidateUniqueColumns(config.Columns);
        }

        public System.Data.DataTable CreateDataTableFromCsv(DataTableSchema schema, string csvData)
        {
            try
            {
                var csvDataTable = new System.Data.DataTable();
                var expectedColumns = this.CreateDataColumnsFromConfiguration(schema);
                csvDataTable.Columns.AddRange(expectedColumns.ToArray());
                using (var reader = new StringReader(csvData))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                using (var csvDataReader = new CsvDataReader(csv))
                {
                    csv.ReadHeader();
                    var csvColumnHeaders = csv.HeaderRecord;
                    if (csvColumnHeaders == null)
                    {
                        throw new ErrorException(Errors.General.Unexpected(
                            "When trying to read the CSV Data, the CSV reader did not found any column headers. " +
                            "Please contact our support team for further assistance. We apologise for the inconvenience."));
                    }
                    this.ValidateCsvColumns(schema, csvColumnHeaders);
                    csvDataTable.Load(csvDataReader);
                }
                return csvDataTable;
            }
            catch (CsvHelper.MissingFieldException exception)
            {
                throw new ErrorException(Errors.DataTableDefinition.CsvDataColumnValueNotFound());
            }
        }

        [JobDisplayName("Rename Data Table Content Table Name Using New {1} Alias: {3}")]
        public async Task RenameDataTableDefinitionDataTableName(Guid tenantId, EntityType entityType, Guid entityId, string newEntityAlias)
        {
            this.logger.LogInformation(
                "\n" + "Entity Type: " + entityType.ToString() + "\n" +
                "New Entity Alias: " + newEntityAlias);

            List<DataTableDefinition> dataTableDefinitions = new List<DataTableDefinition>();

            if (entityType == EntityType.Tenant)
            {
                dataTableDefinitions = this.dataTableDefinitionRepository.GetDataTableDefinitionsByTenantId(tenantId);
            }

            if (entityType == EntityType.Product)
            {
                dataTableDefinitions = this.dataTableDefinitionRepository.GetDataTableDefinitionsByProductId(tenantId, entityId);
            }

            if (entityType == EntityType.Organisation)
            {
                dataTableDefinitions = this.dataTableDefinitionRepository.GetDataTableDefinitionsByOrganisationId(tenantId, entityId);
            }

            if (dataTableDefinitions.Any())
            {
                this.logger.LogInformation(
                $"Found {dataTableDefinitions.Count} Data Table Definitions");

                await this.UpdateDataTableDatabaseNames(dataTableDefinitions, entityType, newEntityAlias);
            }
        }

        private async Task UpdateDataTableDatabaseNames(
            List<DataTableDefinition> dataTableDefinitions,
            EntityType entityType,
            string newEntityAlias)
        {
            foreach (var dataTableDefinition in dataTableDefinitions)
            {
                this.logger.LogInformation(
                    "\n" + $"Data Table Name: {dataTableDefinition.Name}" + "\n"
                    + $"Data Table Alias: {dataTableDefinition.Alias}");

                var oldDatabaseTableName = dataTableDefinition.DatabaseTableName;
                var finalDatabaseTableName =
                    await this.GenerateDatabaseTableBaseNameForEntityAliasChangeEvent(
                        dataTableDefinition, entityType, newEntityAlias);

                this.logger.LogInformation(
                    "\n" + $"Old DB Name: {oldDatabaseTableName}" + "\n" +
                    $"Final DB Name: {finalDatabaseTableName}");

                await this.dataTableDefinitionRepository.UpdateDatabaseTableName(
                    dataTableDefinition, finalDatabaseTableName);
                await this.dataTableContentRepository.RenameDatabaseTableName(
                    dataTableDefinition, oldDatabaseTableName, dataTableDefinition.DatabaseTableName);
            }
        }

        private void ValidateClusteredIndex(DataTableSchema config, HashSet<string> columnsDefinition)
        {
            if (config.ClusteredIndex != null)
            {
                this.ValidateKeyColumns(config.Columns, config.ClusteredIndex.KeyColumns, columnsDefinition, true, config.ClusteredIndex.Alias);
            }
        }

        private void ValidateUnclusteredIndexes(DataTableSchema schema, HashSet<string> columnsDefinition)
        {
            if (schema.UnclusteredIndexes != null)
            {
                foreach (var index in schema.UnclusteredIndexes)
                {
                    this.ValidateKeyColumns(schema.Columns, index.KeyColumns, columnsDefinition, false, index.Alias);
                }
                var nonKeyColumnAliases = schema.UnclusteredIndexes
                    .Where(unclusteredIndex => unclusteredIndex.NormalizedNonKeyColumns != null
                        && unclusteredIndex.NormalizedNonKeyColumns.Any())
                    .SelectMany(unclusteredIndex => unclusteredIndex.NormalizedNonKeyColumns)
                    .Distinct().ToList();
                this.ValidateIndexColumnAliasesExistsInConfig(nonKeyColumnAliases, columnsDefinition, false);
            }
        }

        private bool IsLargeObjectDataType(DataTypeSqlProperties settings)
        {
            var invalidTypes = new string[] { "text", "ntext", "image" };
            return invalidTypes.Contains(settings.SqlDataType.ToLower()) || settings.SqlColumnSize.Contains("max");
        }

        private void ValidateUniqueColumns(IEnumerable<DataTableModel.Column> columns)
        {
            foreach (var column in columns)
            {
                if (column.Unique == true)
                {
                    var settings = column.DataType.GetDataTypeSqlSettings();
                    if (settings != null && this.IsLargeObjectDataType(settings))
                    {
                        throw new ErrorException(Errors.DataTableDefinition.IndexInvalidUniqueColumnLargeObjectType(
                            column.PascalizedAlias, column.DataType.ToString(), settings.SqlDataTypeAndSize));
                    }
                }
            }
        }

        private void ValidateKeyColumns(IEnumerable<DataTableModel.Column> columns, IEnumerable<KeyColumn> keyColumns, HashSet<string> columnsDefinition, bool isClustered, string indexAlias)
        {
            if (!keyColumns.Any())
            {
                throw new ErrorException(Errors.DataTableDefinition
                   .JsonIndexKeyColumnNotFound(true, indexAlias));
            }
            var keyColumnAliases = keyColumns
                    .Select(keyColumn => keyColumn.NormalizedColumnAlias).Distinct().ToList();
            this.ValidateIndexColumnAliasesExistsInConfig(keyColumnAliases, columnsDefinition, isClustered);

            foreach (var keyColumn in keyColumns)
            {
                var column = columns.Where(x => x.PascalizedAlias == keyColumn.NormalizedColumnAlias).FirstOrDefault();
                if (column == null)
                {
                    throw new ErrorException(Domain.Errors.General.Unexpected(
                        $"When trying to create index '{indexAlias}' with key column '{keyColumn.ColumnAlias}', " +
                        $"the assigned column was not found. " +
                        "Please contact our support team for further assistance. We apologise for the inconvenience."));
                }

                var settings = column.DataType.GetDataTypeSqlSettings();
                if (settings == null)
                {
                    throw new ErrorException(Domain.Errors.General.Unexpected(
                        $"When trying to validate index {indexAlias} key columns, " +
                        $"there was no column settings found for data type '{column.DataType}'. " +
                        "Please contact our support team for further assistance. We apologise for the inconvenience."));
                }

                if (settings.SqlColumnSize == "max"
                    || this.IsLargeObjectDataType(settings))
                {
                    throw new ErrorException(Domain.Errors.DataTableDefinition
                        .IndexInvalidKeyColumnLargeObjectType(
                        isClustered, indexAlias, keyColumn.ColumnAlias, column.DataType.ToString().Camelize(), settings.SqlDataTypeAndSize));
                }
            }
        }

        private void ValidateIndexColumnAliasesExistsInConfig(List<string> indexColumnAliases, HashSet<string> columnsDefinition, bool isClustered)
        {
            var notFoundColumnAliases = this.ReturnNotFoundColumnAiases(columnsDefinition, indexColumnAliases);
            if (notFoundColumnAliases.Count >= 1)
            {
                throw new ErrorException(Errors.DataTableDefinition
                    .IndexColumnAliasNotFound(isClustered, true, notFoundColumnAliases));
            }
        }

        private void ValidateCsvColumns(DataTableSchema schema, string[] csvColumnHeaders)
        {
            var columnAliases = schema.PascalizedColumnAliases;
            var csvColumnNameNotExisting = new List<string>();
            Regex spaceRegex = new Regex(@"\s");

            foreach (var csvHeaderName in csvColumnHeaders)
            {
                if (spaceRegex.IsMatch(csvHeaderName))
                {
                    csvColumnNameNotExisting.Add(csvHeaderName);
                }

                var isCsvColumnHeaderInConfig = columnAliases.Contains(DataTableModel.Column.GetNormalizedColumnAlias(csvHeaderName));
                if (!isCsvColumnHeaderInConfig)
                {
                    csvColumnNameNotExisting.Add(csvHeaderName);
                }
            }
            if (csvColumnNameNotExisting.Any())
            {
                throw new ErrorException(Errors.DataTableDefinition
                    .CsvColumnHeaderNameNotFoundInJson(csvColumnNameNotExisting));
            }

            // Check if required columns exists in csv data header columns
            var missingRequiredColumns = new List<string>();
            var normalizedCsvHeader = csvColumnHeaders.Select(x => x.ToLower()).ToArray();
            foreach (var columnConfig in schema.Columns)
            {
                if (columnConfig.Required == true)
                {
                    var isColumnExists = normalizedCsvHeader.Contains(columnConfig.Alias.ToLower());
                    if (!isColumnExists)
                    {
                        missingRequiredColumns.Add(columnConfig.Alias);
                    }
                }
            }
            if (missingRequiredColumns.Any())
            {
                throw new ErrorException(Errors.DataTableDefinition
                    .CsvDataRequiredColumnNotFound(missingRequiredColumns));
            }
        }

        private IEnumerable<System.Data.DataColumn> CreateDataColumnsFromConfiguration(DataTableSchema schema)
        {
            var dataColums = new List<System.Data.DataColumn>();
            foreach (var columnConfig in schema.Columns)
            {
                var settings = columnConfig.DataType.GetDataTypeSqlSettings();
                if (settings == null)
                {
                    throw new ErrorException(Domain.Errors.General.Unexpected(
                        $"When trying to create DataTable DataColumn for '{columnConfig.Name}', " +
                        $"there was no column settings found for data type '{columnConfig.DataType}'. " +
                        "Please contact our support team for further assistance. We apologise for the inconvenience."));
                }

                dataColums.Add(new System.Data.DataColumn(columnConfig.PascalizedAlias, settings.DataTableDataType));
            }
            return dataColums;
        }

        /// <summary>
        /// Retrieves duplicate case-insensitive items from an enumerable string object
        /// </summary>
        /// <param name="originalItems"></param>
        /// <returns></returns>
        private List<string> RetrieveDuplicateNonCaseSensitiveItems(IEnumerable<string> originalItems)
        {
            var uniqueItems = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var duplicates = new List<string>();
            foreach (var item in originalItems)
            {
                if (!uniqueItems.Add(item))
                {
                    duplicates.Add(item);
                }
            }
            return duplicates;
        }

        private List<string> ReturnNotFoundColumnAiases(HashSet<string> columnsDefinition, IEnumerable<string> columnAliases)
        {
            var notFoundColumnAiases = new List<string>();

            foreach (var item in columnAliases)
            {
                if (!columnsDefinition.Contains(item))
                {
                    notFoundColumnAiases.Add(item);
                }
            }
            return notFoundColumnAiases;
        }

        /// <summary>
        /// Generates a Data Table Database Table name based on the entity type and new entity alias
        /// </summary>
        /// <param name="dataTableDefinition"></param>
        /// <param name="entityType"></param>
        /// <param name="newEntityAlias"></param>
        /// <returns></returns>
        private async Task<string> GenerateDatabaseTableBaseNameForEntityAliasChangeEvent(
            Domain.Entities.DataTableDefinition dataTableDefinition,
            EntityType entityType,
            string newEntityAlias)
        {
            string pascaliseDataTableDefinitionAlias = this.PascalizeAlias(dataTableDefinition.Alias);
            string tenantAlias = string.Empty;
            string entityAlias = "";

            if (entityType == EntityType.Tenant)
            {
                tenantAlias = newEntityAlias;
            }
            else
            {
                tenantAlias = await this.tenantRepository.GetTenantAliasById(dataTableDefinition.TenantId) ?? string.Empty;
            }

            if (entityType == dataTableDefinition.EntityType)
            {
                entityAlias = newEntityAlias;
            }
            else
            {
                entityAlias = entityType == dataTableDefinition.EntityType ? newEntityAlias
                    : await this.GetEntityAlias(dataTableDefinition, dataTableDefinition.EntityType, entityAlias);
            }
            return this.CreateDataTableNameByEntityType(
                    pascaliseDataTableDefinitionAlias, tenantAlias, entityAlias, dataTableDefinition.EntityType);
        }

        private async Task<string> GenerateDatabaseTableBaseName(Domain.Entities.DataTableDefinition dataTableDefinition)
        {
            var databaseTablename = string.Empty;
            string tenantAlias = string.Empty;
            string organisationAlias = string.Empty;
            string productAlias = string.Empty;
            string pascaliseDataTableDefinitionAlias = this.PascalizeAlias(dataTableDefinition.Alias);
            var entityType = dataTableDefinition.EntityType;

            tenantAlias = await this.tenantRepository.GetTenantAliasById(dataTableDefinition.TenantId) ?? string.Empty;
            string entityAlias = tenantAlias;
            if (entityType != EntityType.Tenant)
            {
                entityAlias = await this.GetEntityAlias(dataTableDefinition, entityType, entityAlias);
            }
            return this.CreateDataTableNameByEntityType(
                pascaliseDataTableDefinitionAlias, tenantAlias, entityAlias, dataTableDefinition.EntityType);
        }

        private async Task<string> GetEntityAlias(DataTableDefinition dataTableDefinition, EntityType entityType, string entityAlias)
        {
            if (entityType == EntityType.Product)
            {
                entityAlias = await this.productRepository.GetProductAliasById(
                    dataTableDefinition.TenantId,
                    dataTableDefinition.EntityId) ?? string.Empty;
            }
            if (dataTableDefinition.EntityType == EntityType.Organisation)
            {
                entityAlias = await this.organisationReadModelRepository.GetOrganisationAliasById(
                         dataTableDefinition.TenantId,
                         dataTableDefinition.EntityId) ?? string.Empty;
            }

            return entityAlias;
        }

        /// <summary>
        /// This method will generate a database table base name.
        /// Example patterns:
        ///     - Tenant data table pattern:
        ///         {{tenantAlias}}_{{dataTableAlias}}
        ///         DataTables.AbcInsurance_EmbargoPostcodes
        ///     - Non tenant data table:
        ///         {{tenantAlias}}_{{EntityType}}_{{organisationAlias}}_{{dataTableAlias}}
        ///         DataTables.AbcInsurance_Organisation_RiskSure_EmbargoPostcodes.
        /// </summary>
        private string CreateDataTableNameByEntityType(
            string pascalizeDataTableDefinitionAlias,
            string tenantAlias,
            string entityAlias,
            EntityType entityType)
        {
            string pascalizeTenantAlias = this.PascalizeAlias(tenantAlias);
            string pascaliseEntityAlias = this.PascalizeAlias(entityAlias);

            if (entityType == EntityType.Tenant)
            {
                return $"{pascalizeTenantAlias}_{pascalizeDataTableDefinitionAlias}";
            }

            if (string.IsNullOrEmpty(entityAlias))
            {
                throw new ErrorException(Errors.General.Unexpected(
                    $"When trying to generate a datatable name for {pascalizeDataTableDefinitionAlias} " +
                    $"the {entityType} alias was not found."));
            }

            var databaseTablename = $"{pascalizeTenantAlias}_{entityType.ToString().ToTitleCase()}_{pascaliseEntityAlias}_{pascalizeDataTableDefinitionAlias}";

            if (databaseTablename.Length > 124)
            {
                // truncate up to 124 characters.
                databaseTablename = databaseTablename.Substring(0, 124);
            }

            return databaseTablename;
        }

        private string PascalizeAlias(string alias)
        {
            return alias.HyphenToUnderscore().Pascalize();
        }

        /// <summary>
        /// Find unused suffix range from 0001 to 9999.
        /// </summary>
        private string FindAvailableSuffix(string baseDatabaseTableName, List<string> usedIndexes)
        {
            HashSet<int> usedValuesSet = new HashSet<int>();
            foreach (string value in usedIndexes)
            {
                usedValuesSet.Add(int.Parse(value));
            }

            for (int i = 1; i < 10000; i++)
            {
                if (!usedValuesSet.Contains(i))
                {
                    return i.ToString("D4");
                }
            }

            throw new ErrorException(Errors.DataTableDefinition.OutOfIndex(baseDatabaseTableName));
        }
    }
}