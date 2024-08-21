// <copyright file="IDataTableContentDbFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.DataTable
{
    using System.Data.SqlClient;

    /// <summary>
    /// Contract for data table creator class.
    /// </summary>
    public interface IDataTableContentDbFactory
    {
        /// <summary>
        /// Get a new instance of SqlBulkCopy using the given parameters.
        /// </summary>
        /// <param name="sqlBulkCopyOptions">The bulk copy options.</param>
        /// <param name="bulkCopyBatchSize">The default batch size.</param>
        /// <param name="bulkCopyConnectionTimeout">The default connection timeout.</param>
        /// <param name="destinationTableName">The destination table name.</param>
        /// <param name="enableStreaming">Option to enable streaming.</param>
        /// <returns>Instance of SqlBulkCopy.</returns>
        SqlBulkCopy GetBulkCopy(
            SqlBulkCopyOptions sqlBulkCopyOptions,
            int bulkCopyBatchSize,
            int bulkCopyConnectionTimeout,
            string destinationTableName,
            bool enableStreaming);

        /// <summary>
        /// Get a new instance of sql connection.
        /// </summary>
        /// <param name="connectTimeout">The connection timeout.</param>
        /// <returns>Instance of SQL connection.</returns>
        SqlConnection GetConnection(int connectTimeout);
    }
}
