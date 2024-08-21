// <copyright file="INfidRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Repositories
{
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using UBind.Domain.ThirdPartyDataSets.Nfid;

    /// <summary>
    /// Contract to be use for the NFID repository.
    /// </summary>
    public interface INfidRepository
    {
        Task<string> GetExistingTableIndex();

        Task CreateOrUpdateSchemaBoundView(string tableSuffix);

        /// <summary>
        /// Create the NFID tables and view.
        /// </summary>
        Task CreateTablesByIndex(string tableSuffix);

        /// <summary>
        /// Remove all NFID tables from the target schema.
        /// </summary>
        Task DropAllTablesByIndex(string tableSuffix);

        /// <summary>
        /// Create the schema if not yet created.
        /// </summary>
        /// <param name="schema">The schema name.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CreateSchemaIfNotExists(string schema);

        /// <summary>
        /// Get the data table and schema from a given table name.
        /// </summary>
        /// <param name="tableName">The table name.</param>
        /// <returns>The result returned by the database after executing the command.</returns>
        DataTable GetDataTableWithSchema(string tableName);

        /// <summary>
        /// Perform bulk copy operations from the supplied DataTable to a destination table and schema.
        /// </summary>
        /// <returns>The result returned by the database after executing the command.</returns>
        Task BulkCopyAsync(DataTable dataTable);

        /// <summary>
        /// Create the NFID indexes.
        /// </summary>
        Task CreateIndexes(string index);

        /// <summary>
        /// Gets the NFID stage 6 details by GNAF address id.
        /// </summary>
        /// <param name="gnafId">The GNAF address id.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<Detail> GetStage6ByGnafId(string gnafId);

        /// <summary>
        /// Get paginated nfid data.
        /// </summary>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <returns>Nfid data.</returns>
        Task<IEnumerable<Detail>> GetPaginatedNfid(int pageNumber, int pageSize);
    }
}
