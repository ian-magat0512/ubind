// <copyright file="IThirdPartyDataSetsDbObjectFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ThirdPartyDataSets
{
    using System.Data;
    using System.Data.SqlClient;

    /// <summary>
    /// Provide a contract for the class factory to be used to create common database objects and operations.
    /// </summary>
    public interface IThirdPartyDataSetsDbObjectFactory
    {
        /// <summary>
        /// Get a new instance of SqlBulkCopy using the given parameters.
        /// </summary>
        /// <param name="bulkCopyOptions">The bulk copy options.</param>
        /// <param name="batchSize">The number of rows of each batch.</param>
        /// <param name="bulkCopyTimeout">The bulk copy timeout.</param>
        /// <param name="destinationTableName">The destination table name.</param>
        /// <param name="enableStreaming">The options to enable or disable streaming.</param>
        /// <returns>A new instance of BulkCopy using the configuration from the parameters.</returns>
        SqlBulkCopy GetNewBulkCopy(SqlBulkCopyOptions bulkCopyOptions, int batchSize, int bulkCopyTimeout, string destinationTableName, bool enableStreaming);

        /// <summary>
        /// Get a new instance of SqlCommand using the given parameters.
        /// </summary>
        /// <param name="commandType">The command type.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="connectTimeout">The connection timeout.</param>
        /// <returns>A new instance of DbCommand using the configuration from the parameters.</returns>
        SqlCommand GetNewDbCommand(CommandType commandType, string commandText, int connectTimeout);

        /// <summary>
        /// Get a new instance of SqlConnection using the given parameters..
        /// </summary>
        /// <param name="connectTimeout">The connection timeout.</param>
        /// <returns>A new instance of SqlConnection using the configuration from the parameters.</returns>
        SqlConnection GetNewConnection(int connectTimeout);
    }
}
