// <copyright file="IGnafRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Repositories
{
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using UBind.Domain.ThirdPartyDataSets.Gnaf;

    /// <summary>
    /// Provides the contract to be use for the Gnaf repository.
    /// </summary>
    public interface IGnafRepository
    {
        Task<string> GetExistingTableIndex();

        Task CreateOrUpdateSchemaBoundView(string tableSuffix);

        Task DropAllTablesByIndex(string tableSuffix);

        /// <summary>
        /// Create the Gnaf tables and view.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CreateTablesAndSchemaFromScript(string sqlCommand, string tableSuffix);

        /// <summary>
        /// Create the schema if not yet created.
        /// </summary>
        /// <param name="schemaName">The schema name.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CreateSchemaIfNotExist(string schemaName);

        /// <summary>
        /// Remove all Gnaf tables from the target schema.
        /// </summary>
        /// <param name="schema">The target schema.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DropAllTablesBySchema(string schema);

        /// <summary>
        /// Perform bulk copy operations from the supplied DataTable to a destination table and schema.
        /// </summary>
        /// <returns>The result returned by the database after executing the command.</returns>
        Task BulkCopyAsync(DataTable dataTable);

        /// <summary>
        /// Get the data table and schema from a given table name.
        /// </summary>
        /// <returns>The result returned by the database after executing the command.</returns>
        DataTable GetDataTableWithSchema(string dataTableName);

        /// <summary>
        /// Create or update the address view from a given schema name.
        /// </summary>
        /// <returns>The result returned by the database after executing the command.</returns>
        Task CreateOrUpdateAddressView();

        /// <summary>
        /// Get the Gnaf materialized address view .
        /// </summary>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <returns>MaterializedAddressView.</returns>
        Task<IEnumerable<MaterializedAddressView>> GetGnafMaterializedAddressView(int pageNumber, int pageSize);

        /// <summary>
        /// Create the Gnaf foreign keys and index.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CreateForeignKeyAndIndexes(string sqlIndexCreationScript, string index);
    }
}
