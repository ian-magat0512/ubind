// <copyright file="DataTableContentDbFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.DataTable
{
    using System.Data.SqlClient;

    /// <summary>
    /// Factory class for data table.
    /// </summary>
    public class DataTableContentDbFactory : IDataTableContentDbFactory
    {
        private readonly IDataTableContentDbConfiguration dataTableDbConfiguration;

        public DataTableContentDbFactory(IDataTableContentDbConfiguration dataTableDbConfiguration)
        {
            this.dataTableDbConfiguration = dataTableDbConfiguration;
        }

        /// <inheritdoc/>
        public SqlBulkCopy GetBulkCopy(SqlBulkCopyOptions bulkCopyOptions, int batchSize, int bulkCopyTimeout, string destinationTableName, bool enableStreaming)
        {
            return new SqlBulkCopy(this.dataTableDbConfiguration.ConnectionString, bulkCopyOptions)
            {
                BatchSize = batchSize,
                BulkCopyTimeout = bulkCopyTimeout,
                DestinationTableName = destinationTableName,
                EnableStreaming = enableStreaming,
            };
        }

        /// <inheritdoc/>
        public SqlConnection GetConnection(int connectTimeout)
        {
            var builder = new SqlConnectionStringBuilder(this.dataTableDbConfiguration.ConnectionString)
            {
                ConnectTimeout = connectTimeout,
            };

            return new SqlConnection(builder.ConnectionString);
        }
    }
}
