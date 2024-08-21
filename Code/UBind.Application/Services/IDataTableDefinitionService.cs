// <copyright file="IDataTableDefinitionService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Services
{
    using System.Data;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.Entities;
    using UBind.Domain.Models.DataTable;

    /// <summary>
    /// Sevice for data table definition entity.
    /// </summary>
    public interface IDataTableDefinitionService
    {
        /// <summary>
        /// This method will generate a database table name.
        /// Example patterns:
        ///     - Tenant data table pattern:
        ///         {{tenantAlias}}_{{dataTableAlias}}
        ///         DataTables.AbcInsurance_EmbargoPostcodes
        ///     - Non tenant data table:
        ///         {{tenantAlias}}_{{EntityType}}_{{organisationAlias}}_{{dataTableAlias}}
        ///         DataTables.AbcInsurance_Organisation_RiskSure_EmbargoPostcodes.
        /// </summary>
        Task<string> GenerateDatabaseTableName(DataTableDefinition dataTableDefinition);

        /// <summary>
        /// Validates table schema's list of indexes and its index properties.
        /// Throws an error on invalid.
        /// </summary>
        /// <param name="schema"></param>
        void ValidateTableSchemaIndexes(DataTableSchema schema);

        /// <summary>
        /// Validates table schema's list of column names and aliases.
        /// Throws an error on invalid.
        /// </summary>
        /// <param name="schema"></param>
        void ValidateColumns(DataTableSchema schema);

        /// <summary>
        /// Updates the database table name of the list of database definition
        /// </summary>
        /// <param name="dataTableDefinitions"></param>
        /// <returns></returns>
        Task RenameDataTableDefinitionDataTableName(Guid tenantId, EntityType entityType, Guid entityId, string newProductAlias);

        /// <summary>
        /// Creates a DataTable object from the csv data and the provided table schema
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="csvData"></param>
        /// <returns></returns>
        DataTable CreateDataTableFromCsv(DataTableSchema schema, string csvData);

        void ValidateTableSchema(string json);
    }
}
