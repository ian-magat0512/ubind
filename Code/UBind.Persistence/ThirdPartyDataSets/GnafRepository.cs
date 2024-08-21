// <copyright file="GnafRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ThirdPartyDataSets
{
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Dapper;
    using UBind.Domain.Repositories;
    using UBind.Domain.ThirdPartyDataSets.Gnaf;

    /// <inheritdoc/>
    public class GnafRepository : IGnafRepository
    {
        private const string CreateIfNotExistSchema =
            "IF NOT EXISTS(SELECT 1 FROM    sys.schemas WHERE   name = N'{0}') EXEC('CREATE SCHEMA [{0}]'); ";

        private const string CreateTableWithSchema = "CREATE TABLE [{0}].";
        private const string CreateTable = "CREATE TABLE ";

        private const string ConstraintWithSchema = "REFERENCES [{0}].";
        private const string Constraint = "REFERENCES ";

        private const string AlterTableWithSchema = "ALTER TABLE [{0}].";
        private const string AlterTable = "ALTER TABLE ";

        private const string DropViewAddressView = "DROP VIEW IF EXISTS[{0}].ADDRESS_VIEW";

        private const string ForEachTableDelete =
            "sp_MSforeachtable  @command1='DROP TABLE ? ' , @whereand =' AND SCHEMA_NAME(schema_id) =''{0}'' '";

        private const string ForEachTableNoConstraint =
            "sp_MSforeachtable  @command1='ALTER TABLE ? NOCHECK CONSTRAINT ALL ' , @whereand =' AND SCHEMA_NAME(schema_id) =''{0}'' '";

        private const string GetTableSchema = "SELECT top 0 * FROM [{0}].[{1}]";

        private const int DefaultConnectionTimeout = 1200;
        private const int DefaultLongRunningConnectionTimeout = 6600;
        private const int DefaultBulkCopyConnectionTimeout = 3600;
        private const int DefaultBulkCopyBatchSize = 100000;

        private const string SCHEMA = "Gnaf";

        private readonly IThirdPartyDataSetsDbObjectFactory thirdPartyDataSetsDbObjectFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="GnafRepository "/> class.
        /// </summary>
        /// <param name="thirdPartyDataSetsDbObjectFactory">The third party data sets connection factory.</param>
        public GnafRepository(IThirdPartyDataSetsDbObjectFactory thirdPartyDataSetsDbObjectFactory)
        {
            this.thirdPartyDataSetsDbObjectFactory = thirdPartyDataSetsDbObjectFactory;
        }

        /// <inheritdoc/>
        public async Task CreateTablesAndSchemaFromScript(string sqlScripts, string tableSuffix)
        {
            await this.CreateSchemaIfNotExist(SCHEMA);

            using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    string sqlCreateTableScript = this.CustomizeGnafCreateTableScripts(sqlScripts, tableSuffix);
                    using (var sqlCommand = new SqlCommand(sqlCreateTableScript, connection, transaction))
                    {
                        await sqlCommand.ExecuteNonQueryAsync();
                    }

                    transaction.Commit();
                }
            }
        }

        public async Task CreateOrUpdateSchemaBoundView(string tableSuffix)
        {
            await this.CreateSchemaIfNotExist(SCHEMA);
            using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    var sqlCreationTableScript = string.Format(CommonScriptRepository.CreateOrAlterTableBoundedView, SCHEMA, tableSuffix);
                    using (var sqlCommand = new SqlCommand(sqlCreationTableScript, connection, transaction))
                    {
                        await sqlCommand.ExecuteNonQueryAsync();
                    }

                    transaction.Commit();
                }
            }
        }

        public async Task DropAllTablesByIndex(string tableSuffix)
        {
            await this.CreateSchemaIfNotExist(SCHEMA);
            using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
            {
                var droptTableScript = string.Format(CommonScriptRepository.DropTablesBySchemaAndIndex, SCHEMA, tableSuffix);
                await connection.ExecuteAsync(droptTableScript);
            }
        }

        public async Task<string> GetExistingTableIndex()
        {
            await this.CreateSchemaIfNotExist(SCHEMA);
            using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
            {
                string checkGnafTableQuery = string.Format(CommonScriptRepository.GetExistingTableIndexBySchema, SCHEMA);
                var index = (await connection.QueryAsync<string>(checkGnafTableQuery)).SingleOrDefault();

                return index ?? string.Empty;
            }
        }

        /// <inheritdoc/>
        public async Task DropAllTablesBySchema(string schema)
        {
            var sqlEachTableDelete = string.Format(ForEachTableDelete, schema);
            var sqlEachTableNoConstraint = string.Format(ForEachTableNoConstraint, schema);

            using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
            {
                await connection.ExecuteAsync(sqlEachTableNoConstraint);
                await connection.ExecuteAsync(sqlEachTableDelete);
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
        public async Task CreateSchemaIfNotExist(string schemaName)
        {
            var schemaCreation = string.Format(CreateIfNotExistSchema, schemaName);

            using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
            {
                await connection.ExecuteAsync(schemaCreation);
            }
        }

        /// <inheritdoc/>
        public async Task CreateOrUpdateAddressView()
        {
            using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
            {
                try
                {
                    var alterAddressView = string.Format(GnafRepositoryScripts.AlterAddressView, SCHEMA);
                    await connection.ExecuteAsync(alterAddressView);
                }
                catch (SqlException)
                {
                    var dropViewAddressView = string.Format(DropViewAddressView, SCHEMA);
                    await connection.ExecuteAsync(dropViewAddressView);

                    var createAddressView = string.Format(GnafRepositoryScripts.CreateAddressView, SCHEMA);
                    await connection.ExecuteAsync(createAddressView);
                }
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<MaterializedAddressView>> GetGnafMaterializedAddressView(
            int pageNumber,
            int pageSize)
        {
            var getAddressPerPageScript = string.Format(GnafRepositoryScripts.SelectAddressViewPerPage, SCHEMA, pageSize, pageNumber);
            using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultBulkCopyConnectionTimeout))
            {
                var addressViews = connection.Query<MaterializedAddressView>(getAddressPerPageScript, commandTimeout: connection.ConnectionTimeout);
                return await Task.FromResult(addressViews);
            }
        }

        /// <inheritdoc/>
        public async Task CreateForeignKeyAndIndexes(string sqlAddFkConstrainstScript, string tableSuffix)
        {
            if (!string.IsNullOrEmpty(tableSuffix))
            {
                tableSuffix = "_" + tableSuffix;
            }
            var sqlAddFkConstraintsScript = this.CustomizeGnafAddFkConstraintsScript(sqlAddFkConstrainstScript, tableSuffix);
            var sqlCreateAddressViewIndexScript = string.Format(GnafRepositoryScripts.CreateAddressViewIndex, SCHEMA, tableSuffix);

            using (var dbConnection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultLongRunningConnectionTimeout))
            {
                await dbConnection.OpenAsync();

                using (var command = new SqlCommand(sqlCreateAddressViewIndexScript, dbConnection))
                {
                    await command.ExecuteNonQueryAsync();
                }

                using (var command = new SqlCommand(sqlAddFkConstraintsScript, dbConnection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        private string CustomizeGnafCreateTableScripts(string sqlScripts, string tableSuffix)
        {
            if (!string.IsNullOrEmpty(tableSuffix))
            {
                tableSuffix = "_" + tableSuffix;
            }
            string pattern = @"CREATE TABLE (\w+)\s*\(";
            string sqlCreationScriptWithIndex = Regex.Replace(sqlScripts, pattern, match =>
            {
                string originalTableName = match.Groups[1].Value;
                string updatedTableName = originalTableName + tableSuffix;
                return $"CREATE TABLE {updatedTableName} (";
            });
            var sqlCreationTableScript =
                sqlCreationScriptWithIndex.Replace(CreateTable, string.Format(CreateTableWithSchema, SCHEMA));
            return sqlCreationTableScript;
        }

        private string CustomizeGnafAddFkConstraintsScript(string sqlAddFkConstrainstScript, string tableSuffix)
        {
            string alterTablePattern = @"ALTER TABLE\s+(\w+)\s+ADD";
            string constraintPattern = @"(CONSTRAINT (\w+)_\w+)";
            string referencesPattern = @"REFERENCES\s+(\w+)\s+\((\w+)\);";

            string modifiedAddFkConstrainstScript = Regex.Replace(sqlAddFkConstrainstScript, alterTablePattern, match =>
            {
                string originalTableName = match.Groups[1].Value;
                string updatedTableName = originalTableName + tableSuffix;
                return $"ALTER TABLE {updatedTableName} ADD";
            });

            modifiedAddFkConstrainstScript = Regex.Replace(modifiedAddFkConstrainstScript, referencesPattern, match =>
            {
                string originalReferenceTable = match.Groups[1].Value;
                string updatedReferenceTable = originalReferenceTable + tableSuffix;
                return $"REFERENCES {updatedReferenceTable} ({match.Groups[2].Value});";
            });

            modifiedAddFkConstrainstScript = Regex.Replace(modifiedAddFkConstrainstScript, constraintPattern, match =>
            {
                string originalConstraint = match.Groups[1].Value;
                string originalConstraintName = match.Groups[2].Value;
                return originalConstraint.Replace(originalConstraintName, originalConstraintName + tableSuffix);
            });

            var sqlIndexCreationScriptWithSchema = modifiedAddFkConstrainstScript
                .Replace(AlterTable, string.Format(AlterTableWithSchema, SCHEMA))
                .Replace(Constraint, string.Format(ConstraintWithSchema, SCHEMA));
            return sqlIndexCreationScriptWithSchema;
        }
    }
}
