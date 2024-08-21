// <copyright file="RedBookRepository.cs" company="uBind">
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
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Repositories;
    using UBind.Domain.ThirdPartyDataSets.RedBook;

    /// <inheritdoc/>
    public class RedBookRepository : IRedBookRepository
    {
        private const string CreateIfNotExistSchema =
            "IF NOT EXISTS(SELECT 1 FROM    sys.schemas WHERE   name = N'{0}') EXEC('CREATE SCHEMA [{0}]'); ";

        private const string GetTableSchema = "SELECT top 0 * FROM [{0}].[{1}]";

        private const int DefaultConnectionTimeout = 1200;
        private const int DefaultLongRunningConnectionTimeout = 6600;
        private const int DefaultBulkCopyConnectionTimeout = 3600;
        private const int DefaultBulkCopyBatchSize = 100000;

        private const string SCHEMA = "RedBook";

        private readonly IThirdPartyDataSetsDbObjectFactory thirdPartyDataSetsDbObjectFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedBookRepository"/> class.
        /// </summary>
        /// <param name="thirdPartyDataSetsDbObjectFactory">The third party data sets connection factory.</param>
        public RedBookRepository(IThirdPartyDataSetsDbObjectFactory thirdPartyDataSetsDbObjectFactory)
        {
            this.thirdPartyDataSetsDbObjectFactory = thirdPartyDataSetsDbObjectFactory;
        }

        public async Task<string> GetExistingTableIndex()
        {
            using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
            {
                string checkRedBookTableQuery = string.Format(CommonScriptRepository.GetExistingTableIndexBySchema, SCHEMA);
                var index = (await connection.QueryAsync<string>(checkRedBookTableQuery)).SingleOrDefault();

                return index ?? string.Empty;
            }
        }

        /// <inheritdoc/>
        public async Task CreateTablesAndSchema(string tableSuffix)
        {
            await this.CreateSchemaIfNotExists(SCHEMA);

            using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    var sqlCreationTableScript = string.Format(RedBookRepositoryScripts.CreateTable, SCHEMA, tableSuffix);
                    using (var sqlCommand = new SqlCommand(sqlCreationTableScript, connection, transaction))
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
                    var createTableViewScript = string.Format(CommonScriptRepository.CreateOrAlterTableBoundedView, SCHEMA, tableSuffix);
                    using (var sqlCommand = new SqlCommand(createTableViewScript, connection, transaction))
                    {
                        await sqlCommand.ExecuteNonQueryAsync();
                    }

                    transaction.Commit();
                }
            }
        }

        /// <inheritdoc/>
        public async Task DropAllTablesByIndex(string tableSuffix)
        {
            var deleteSql = string.Format(CommonScriptRepository.DropTablesBySchemaAndIndex, SCHEMA, tableSuffix);
            using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
            {
                await connection.ExecuteAsync(deleteSql);
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
        public async Task CreateSchemaIfNotExists(string schemaName)
        {
            var schemaCreation = string.Format(CreateIfNotExistSchema, schemaName);

            using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
            {
                await connection.ExecuteAsync(schemaCreation);
            }
        }

        /// <inheritdoc/>
        public async Task CreateForeignKeysAndIndexes(string index)
        {
            var sqlIndexCreationScriptWithSchema = string.Format(RedBookRepositoryScripts.Add_Index_And_Fk_Constraints, SCHEMA, index);

            using (var dbConnection =
                this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultLongRunningConnectionTimeout))
            {
                await dbConnection.OpenAsync();
                using (var command = new SqlCommand(sqlIndexCreationScriptWithSchema, dbConnection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<VehicleMake>> GetVehicleMakesByYearGroupAsync(int year)
        {
            if (!(await this.VehicleMakeYearGroupExistsAsync(year)))
            {
                throw new ErrorException(Errors.ThirdPartyDataSets.RedBook.YearGroupNotFound(year.ToString()));
            }

            using (MiniProfiler.Current.Step("RedBookRepository." + nameof(this.GetVehicleMakesByYearGroupAsync)))
            using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
            {
                return await connection.QueryAsync<VehicleMake>(
                    RedBookRepositoryScripts.Dml_GetVehicleMakesByYear,
                    new { @Year = year });
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<VehicleMake>> GetVehicleMakesAsync()
        {
            using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
            {
                return await connection.QueryAsync<VehicleMake>(RedBookRepositoryScripts.Dml_GetVehicleMakes);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<VehicleFamily>> GetVehicleFamiliesByMakeCodeAsync(string makeCode)
        {
            if (!(await this.VehicleFamilyMakeCodeExistAsync(makeCode)))
            {
                throw new ErrorException(Errors.ThirdPartyDataSets.RedBook.MakeCodeNotFound(makeCode));
            }

            using (MiniProfiler.Current.Step("RedBookRepository." + nameof(this.GetVehicleFamiliesByMakeCodeAsync)))
            using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
            {
                return await connection.QueryAsync<VehicleFamily>(
                    RedBookRepositoryScripts.Dml_GetVehicleFamiliesByMakeCode,
                    new
                    {
                        MakeCode = makeCode,
                    });
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<VehicleFamily>> GetVehicleFamiliesByMakeCodeAndYearGroupAsync(string makeCode, int year)
        {
            if (!(await this.VehicleFamilyMakeCodeExistAsync(makeCode)))
            {
                throw new ErrorException(Errors.ThirdPartyDataSets.RedBook.MakeCodeNotFound(makeCode));
            }

            if (!(await this.VehicleFamilyYearGroupExistsAsync(year)))
            {
                throw new ErrorException(Errors.ThirdPartyDataSets.RedBook.YearGroupNotFound(year.ToString()));
            }

            using (MiniProfiler.Current.Step("RedBookRepository." + nameof(this.GetVehicleFamiliesByMakeCodeAndYearGroupAsync)))
            using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
            {
                return await connection.QueryAsync<VehicleFamily>(
                    RedBookRepositoryScripts.Dml_GetVehicleFamiliesByMakeCodeAndYear,
                    new
                    {
                        Year = year,
                        MakeCode = makeCode,
                    });
            }
        }

        /// <inheritdoc/>
        public async Task<bool> VehicleMakeYearGroupExistsAsync(int year)
        {
            using (MiniProfiler.Current.Step("RedBookRepository." + nameof(this.VehicleMakeYearGroupExistsAsync)))
            using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
            {
                return await connection.ExecuteScalarAsync<bool>(
                    RedBookRepositoryScripts.Dml_IsVehicleMakeYearGroupExists,
                    new { @Year = year });
            }
        }

        /// <inheritdoc/>
        public async Task<bool> VehicleFamilyYearGroupExistsAsync(int yearGroup)
        {
            using (MiniProfiler.Current.Step("RedBookRepository." + nameof(this.VehicleFamilyYearGroupExistsAsync)))
            using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
            {
                return await connection.ExecuteScalarAsync<bool>(
                    RedBookRepositoryScripts.Dml_IsVehicleFamilyYearGroupExists,
                    new { YearGroup = yearGroup });
            }
        }

        /// <inheritdoc/>
        public async Task<bool> VehicleFamilyMakeCodeExistAsync(string makeCode)
        {
            using (MiniProfiler.Current.Step("RedBookRepository." + nameof(this.VehicleFamilyMakeCodeExistAsync)))
            using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
            {
                return await connection.ExecuteScalarAsync<bool>(
                    RedBookRepositoryScripts.Dml_IsVehicleFamilyMakeCodeExists,
                    new { MakeCode = makeCode });
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<int>> GetVehicleYearsByMakeCodeAsync(string makeCode)
        {
            if (!await this.VehicleFamilyMakeCodeExistAsync(makeCode))
            {
                throw new ErrorException(Errors.ThirdPartyDataSets.RedBook.MakeCodeNotFound(makeCode));
            }

            using (MiniProfiler.Current.Step("RedBookRepository." + nameof(this.GetVehicleYearsByMakeCodeAsync)))
            using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
            {
                return await connection.QueryAsync<int>(
                    RedBookRepositoryScripts.Dml_GetVehicleYearsByMakeCode,
                    new
                    {
                        MakeCode = makeCode,
                    });
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<int>> GetVehicleYearsByMakeCodeAndFamilyCodeAsync(string makeCode, string familyCode)
        {
            if (!(await this.VehicleYearsMakeCodeExistsAsync(makeCode)))
            {
                throw new ErrorException(Errors.ThirdPartyDataSets.RedBook.MakeCodeNotFound(makeCode));
            }

            if (!(await this.VehicleYearsByMakeCodeAndFamilyCodeExistsAsync(makeCode, familyCode)))
            {
                throw new ErrorException(Errors.ThirdPartyDataSets.RedBook.MakeCodeAndFamilyCodeNotFound(makeCode, familyCode));
            }

            using (MiniProfiler.Current.Step("RedBookRepository." + nameof(this.GetVehicleYearsByMakeCodeAndFamilyCodeAsync)))
            using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
            {
                return await connection.QueryAsync<int>(
                    RedBookRepositoryScripts.Dml_GetVehicleYearsByMakeCodeAndFamilyCode,
                    new
                    {
                        MakeCode = makeCode,
                        FamilyCode = familyCode,
                    });
            }
        }

        /// <inheritdoc/>
        public async Task<bool> VehicleYearsByMakeCodeAndFamilyCodeExistsAsync(string makeCode, string familyCode)
        {
            using (MiniProfiler.Current.Step("RedBookRepository." + nameof(this.VehicleYearsByMakeCodeAndFamilyCodeExistsAsync)))
            using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
            {
                return await connection.ExecuteScalarAsync<bool>(
                    RedBookRepositoryScripts.Dml_VehicleYearsByMakeCodeAndFamilyCodeExists,
                    new
                    {
                        MakeCode = makeCode,
                        FamilyCode = familyCode,
                    });
            }
        }

        /// <inheritdoc/>
        public async Task<bool> VehicleYearsMakeCodeExistsAsync(string makeCode)
        {
            using (MiniProfiler.Current.Step("RedBookRepository." + nameof(this.VehicleYearsMakeCodeExistsAsync)))
            using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
            {
                return await connection.ExecuteScalarAsync<bool>(
                    RedBookRepositoryScripts.Dml_VehicleYearsByMakeCodeExists,
                    new { MakeCode = makeCode });
            }
        }

        public async Task<IEnumerable<VehicleBadge>> GetVehicleBadgesByMakeCodeFamilyCodeYearBodyStyleAndGearType(string makeCode, string familyCode, int year, string? bodyStyle, string? gearType)
        {
            using (MiniProfiler.Current.Step("RedBookRepository." + nameof(this.GetVehicleBadgesByMakeCodeFamilyCodeYearBodyStyleAndGearType)))
            using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
            {
                var parameters = new
                {
                    MakeCode = makeCode,
                    FamilyCode = familyCode,
                    Year = year,
                    BodyStyle = bodyStyle ?? string.Empty,
                    GearType = gearType ?? string.Empty,
                };
                return await connection.QueryAsync<VehicleBadge>(
                    RedBookRepositoryScripts.Dml_GetVehicleBadgesByMakeCodeFamilyCodeYearBodyStyleAndGearType,
                    parameters);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> GetVehicleBodyStyleByMakeCodeFamilyCodeYearBadgeAndGearType(string makeCode, string familyCode, int year, string? badge, string? gearType)
        {
            using (MiniProfiler.Current.Step("RedBookRepository." + nameof(this.GetVehicleBodyStyleByMakeCodeFamilyCodeYearBadgeAndGearType)))
            using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
            {
                var parameters = new
                {
                    MakeCode = makeCode,
                    FamilyCode = familyCode,
                    Year = year,
                    Badge = badge ?? string.Empty,
                    GearType = gearType ?? string.Empty,
                };
                return await connection.QueryAsync<string>(
                    RedBookRepositoryScripts.Dml_GetVehicleBodyStyleByMakeCodeFamilyCodeYearBadgeAndGearType,
                    parameters);
            }
        }

        public async Task<IEnumerable<string>> GetVehicleGearTypeByMakeCodeFamilyCodeYearBadgeAndBodyStyle(string makeCode, string familyCode, int year, string? badge, string? bodyStyle)
        {
            using (MiniProfiler.Current.Step("RedBookRepository." + nameof(this.GetVehicleGearTypeByMakeCodeFamilyCodeYearBadgeAndBodyStyle)))
            using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
            {
                var parameters = new
                {
                    MakeCode = makeCode,
                    FamilyCode = familyCode,
                    Year = year,
                    Badge = badge ?? string.Empty,
                    BodyStyle = bodyStyle ?? string.Empty,
                };
                return await connection.QueryAsync<string>(
                    RedBookRepositoryScripts.Dml_GetVehicleGearTypeByMakeCodeFamilyCodeYearBadgeAndBodyStyle,
                    parameters);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Vehicle>> GetVehicleListAsync(string makeCode, string familyCode, int year, string badge, string bodyStyle, string gearType)
        {
            using (MiniProfiler.Current.Step("RedBookRepository." + nameof(this.GetVehicleListAsync)))
            using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
            {
                var parameters = new
                {
                    MakeCode = makeCode,
                    FamilyCode = familyCode,
                    Year = year,
                    Badge = badge ?? string.Empty,
                    BodyStyle = bodyStyle ?? string.Empty,
                    GearType = gearType ?? string.Empty,
                };
                return await connection.QueryAsync<Vehicle>(
                    RedBookRepositoryScripts.Dml_GetVehicleList,
                    parameters);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> VehicleByMakeCodeAndFamilyCodeExistsAsync(string makeCode, string familyCode)
        {
            using (MiniProfiler.Current.Step("RedBookRepository." + nameof(this.VehicleByMakeCodeAndFamilyCodeExistsAsync)))
            using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
            {
                return await connection.ExecuteScalarAsync<bool>(
                    RedBookRepositoryScripts.Dml_VehicleByMakeCodeAndFamilyCodeExists,
                    new
                    {
                        MakeCode = makeCode,
                        FamilyCode = familyCode,
                    });
            }
        }

        /// <inheritdoc/>
        public async Task<bool> VehicleByMakeCodeFamilyCodeAndYearExistsAsync(string makeCode, string familyCode, int year)
        {
            using (MiniProfiler.Current.Step("RedBookRepository." + nameof(this.VehicleByMakeCodeFamilyCodeAndYearExistsAsync)))
            using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
            {
                return await connection.ExecuteScalarAsync<bool>(
                    RedBookRepositoryScripts.Dml_VehicleByMakeCodeFamilyCodeAndYearExists,
                    new
                    {
                        MakeCode = makeCode,
                        FamilyCode = familyCode,
                        Year = year,
                    });
            }
        }

        /// <inheritdoc/>
        public async Task<VehicleDetails> GetVehicleByVehicleKeyAsync(string vehicleKey)
        {
            using (MiniProfiler.Current.Step("RedBookRepository." + nameof(this.GetVehicleByVehicleKeyAsync)))
            using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
            {
                var result = await connection.QueryFirstOrDefaultAsync<VehicleDetails>(
                    RedBookRepositoryScripts.Dml_GetVehicleByVehicleKey,
                    new
                    {
                        VehicleKey = vehicleKey,
                    });
                return result;
            }
        }
    }
}
