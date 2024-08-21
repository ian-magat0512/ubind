// <copyright file="NfidRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ThirdPartyDataSets
{
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using Dapper;
    using StackExchange.Profiling;
    using UBind.Domain.Repositories;
    using UBind.Domain.ThirdPartyDataSets.Nfid;

    public class NfidRepository : INfidRepository
    {
        private const string CreateIfNotExistSchema =
        "IF NOT EXISTS(SELECT 1 FROM sys.schemas WHERE name = N'{0}') EXEC('CREATE SCHEMA [{0}]'); ";

        private const string GetTableSchema = "SELECT top 0 * FROM [{0}].[{1}]";

        private const string SCHEMA = "Nfid";

        private const int DefaultConnectionTimeout = 1200;
        private const int DefaultLongRunningConnectionTimeout = 6600;
        private const int DefaultBulkCopyConnectionTimeout = 3600;
        private const int DefaultBulkCopyBatchSize = 100000;

        private readonly IThirdPartyDataSetsDbObjectFactory thirdPartyDataSetsDbObjectFactory;

        public NfidRepository(IThirdPartyDataSetsDbObjectFactory thirdPartyDataSetsDbObjectFactory)
        {
            this.thirdPartyDataSetsDbObjectFactory = thirdPartyDataSetsDbObjectFactory;
        }

        public async Task<string> GetExistingTableIndex()
        {
            await this.CreateSchemaIfNotExists(SCHEMA);
            using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
            {
                string checkNfidTableQuery = string.Format(CommonScriptRepository.GetExistingTableIndexBySchema, SCHEMA);
                var index = (await connection.QueryAsync<string>(checkNfidTableQuery)).SingleOrDefault();

                return index ?? string.Empty;
            }
        }

        /// <inheritdoc/>
        public async Task CreateTablesByIndex(string tableSuffix)
        {
            await this.CreateSchemaIfNotExists(SCHEMA);
            using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    var sqlCreateTablesByIndexScript = string.Format(NfidRepositoryScripts.CreateNfidTable, SCHEMA, tableSuffix);
                    using (var sqlCommand = new SqlCommand(sqlCreateTablesByIndexScript, connection, transaction))
                    {
                        await sqlCommand.ExecuteNonQueryAsync();
                    }

                    transaction.Commit();
                }
            }
        }

        public async Task CreateOrUpdateSchemaBoundView(string tableSuffix)
        {
            await this.CreateSchemaIfNotExists(SCHEMA);
            using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    var sqlCreateOrUpdateSchemaBoundViewScript = string.Format(CommonScriptRepository.CreateOrAlterTableBoundedView, SCHEMA, tableSuffix);
                    using (var sqlCommand = new SqlCommand(sqlCreateOrUpdateSchemaBoundViewScript, connection, transaction))
                    {
                        await sqlCommand.ExecuteNonQueryAsync();
                    }

                    transaction.Commit();
                }
            }
        }

        /// <inheritdoc/>
        public async Task DropAllTablesByIndex(string index)
        {
            await this.CreateSchemaIfNotExists(SCHEMA);
            using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
            {
                var dropTableScript = string.Format(CommonScriptRepository.DropTablesBySchemaAndIndex, SCHEMA, index);
                await connection.ExecuteAsync(dropTableScript);
            }
        }

        /// <inheritdoc/>
        public async Task CreateSchemaIfNotExists(string schemaName)
        {
            var schemaCreation = string.Format(CreateIfNotExistSchema, schemaName);

            using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
            {
                await connection.ExecuteAsync(schemaCreation);
            }
        }

        /// <inheritdoc/>
        public DataTable GetDataTableWithSchema(string dataTableName)
        {
            using (var cmd = this.thirdPartyDataSetsDbObjectFactory.GetNewDbCommand(
                CommandType.Text,
                string.Format(GetTableSchema, SCHEMA, dataTableName),
                DefaultConnectionTimeout))
            {
                var dataTable = new DataTable(dataTableName);
                using (var adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(dataTable);
                }

                if (cmd.Connection.State == ConnectionState.Open)
                {
                    cmd.Connection.Close();
                }

                return dataTable;
            }
        }

        /// <inheritdoc/>
        public async Task BulkCopyAsync(DataTable dataTable)
        {
            using (var bulkCopy = this.thirdPartyDataSetsDbObjectFactory.GetNewBulkCopy(
            SqlBulkCopyOptions.KeepIdentity,
            DefaultBulkCopyBatchSize,
            DefaultBulkCopyConnectionTimeout,
            $"{SCHEMA}.{dataTable.TableName}",
            false))
            {
                await bulkCopy.WriteToServerAsync(dataTable);
            }
        }

        /// <inheritdoc/>
        public async Task CreateIndexes(string index)
        {
            var sqlIndexCreationScript = string.Format(NfidRepositoryScripts.Nfid_Add_Index, SCHEMA, index);

            using (var dbConnection =
            this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultLongRunningConnectionTimeout))
            {
                await dbConnection.OpenAsync();
                using (var command = new SqlCommand(sqlIndexCreationScript, dbConnection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        /// <inheritdoc/>
        public async Task<Detail> GetStage6ByGnafId(string gnafId)
        {
            using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
            {
                using (MiniProfiler.Current.Step(nameof(NfidRepository) + "." + nameof(this.GetStage6ByGnafId)))
                {
                    // using string inside query to improve performance
                    var result = await connection.QueryFirstOrDefaultAsync<Detail>(
                        @"SELECT[Gnaf_Pid] AS GnafAddressId
                          , [Elev] AS Elevation
                          , [Flood_Depth_20] AS FloodDepth20
                          , [Flood_Depth_50] AS FloodDepth50
                          , [Flood_Depth_100] AS FloodDepth100
                          , [Flood_Depth_Extreme] AS FloodDepthExtreme
                          , [Flood_Ari_Gl] AS FloodAriGl
                          , [Flood_Ari_Gl1M] AS FloodAriGl1M
                          , [Flood_Ari_Gl2M] AS FloodAriGl2M
                          , [Notes_Id] AS NotesId
                          , [Level_Nfid_Id] AS LevelNfidId
                          , [Level_Fez_Id] AS LevelFezId
                          , [Flood_Code] AS floodCode
                          , [Floor_Height] AS floodCode
                          , [Latitude] AS latitude
                          , [Longitude] AS longitude
                        FROM[Nfid].[Stage6_View]
                        WHERE[Gnaf_Pid] = @GnafId", new { GnafId = gnafId });

                    return result;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Detail>> GetPaginatedNfid(int pageNumber, int pageSize)
        {
            using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
            {
                var paginatedNfidScript = string.Format(
                    @"SELECT[Gnaf_Pid] AS GnafAddressId
                          , [Elev] AS Elevation
                          , [Flood_Depth_20] AS FloodDepth20
                          , [Flood_Depth_50] AS FloodDepth50
                          , [Flood_Depth_100] AS FloodDepth100
                          , [Flood_Depth_Extreme] AS FloodDepthExtreme
                          , [Flood_Ari_Gl] AS FloodAriGl
                          , [Flood_Ari_Gl1M] AS FloodAriGl1M
                          , [Flood_Ari_Gl2M] AS FloodAriGl2M
                          , [Notes_Id] AS NotesId
                          , [Level_Nfid_Id] AS LevelNfidId
                          , [Level_Fez_Id] AS LevelFezId
                          , [Flood_Code] AS floodCode
                          , [Floor_Height] AS floodCode
                          , [Latitude] AS latitude
                          , [Longitude] AS longitude
                        FROM [{0}].[Stage6_View]
                        ORDER BY GnafAddressId
                        OFFSET {1} * ({2}-1) ROWS
                        FETCH NEXT {1} ROWS ONLY ",
                    SCHEMA,
                    pageSize,
                    pageNumber);
                var result = await connection.QueryAsync<Detail>(paginatedNfidScript, commandTimeout: connection.ConnectionTimeout);

                return result;
            }
        }
    }
}
