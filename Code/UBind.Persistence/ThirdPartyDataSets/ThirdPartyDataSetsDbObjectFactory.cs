// <copyright file="ThirdPartyDataSetsDbObjectFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ThirdPartyDataSets
{
    using System.Data;
    using System.Data.SqlClient;

    /// <inheritdoc />
    public class ThirdPartyDataSetsDbObjectFactory : IThirdPartyDataSetsDbObjectFactory
    {
        private readonly IThirdPartyDataSetsDbConfiguration thirdPartyDataSetsDbConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThirdPartyDataSetsDbObjectFactory"/> class.
        /// </summary>
        /// <param name="thirdPartyDataSetsDbConfiguration">The third party data sets database configuration.</param>
        public ThirdPartyDataSetsDbObjectFactory(IThirdPartyDataSetsDbConfiguration thirdPartyDataSetsDbConfiguration)
        {
            this.thirdPartyDataSetsDbConfiguration = thirdPartyDataSetsDbConfiguration;
        }

        /// <inheritdoc/>
        public SqlBulkCopy GetNewBulkCopy(SqlBulkCopyOptions bulkCopyOptions, int batchSize, int bulkCopyTimeout, string destinationTableName, bool enableStreaming)
        {
            return new SqlBulkCopy(this.thirdPartyDataSetsDbConfiguration.ConnectionString, bulkCopyOptions)
            {
                BatchSize = batchSize,
                BulkCopyTimeout = bulkCopyTimeout,
                DestinationTableName = destinationTableName,
                EnableStreaming = enableStreaming,
            };
        }

        /// <inheritdoc/>
        public SqlCommand GetNewDbCommand(CommandType commandType, string commandText, int connectTimeout)
        {
            var cmd = this.GetNewConnection(connectTimeout).CreateCommand();
            cmd.CommandType = commandType;
            cmd.CommandText = commandText;

            return cmd;
        }

        /// <inheritdoc/>
        public SqlConnection GetNewConnection(int connectTimeout)
        {
            var builder = new SqlConnectionStringBuilder(this.thirdPartyDataSetsDbConfiguration.ConnectionString)
            {
                ConnectTimeout = connectTimeout,
            };

            return new SqlConnection(builder.ConnectionString);
        }
    }
}
