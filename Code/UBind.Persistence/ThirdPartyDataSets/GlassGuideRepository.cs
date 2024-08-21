// <copyright file="GlassGuideRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Persistence.ThirdPartyDataSets;

using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using MediatR;
using UBind.Domain.Repositories;
using UBind.Domain.ThirdPartyDataSets.GlassGuide;

/// <inheritdoc/>
public class GlassGuideRepository : IGlassGuideRepository
{
    private const string VehiclesTable = "GG_Vehicle";
    private const string MakesTable = "GG_Make";
    private const string FamiliesTable = "GG_Family";
    private const string YearsTable = "GG_Year";

    private const string CreateIfNotExistSchema =
        "IF NOT EXISTS(SELECT 1 FROM    sys.schemas WHERE   name = N'{0}') EXEC('CREATE SCHEMA [{0}]'); ";

    private const string GetTableSchema = "SELECT top 0 * FROM [{0}].[{1}]";

    private const int DefaultConnectionTimeout = 1200;
    private const int DefaultLongRunningConnectionTimeout = 6600;
    private const int DefaultBulkCopyConnectionTimeout = 3600;
    private const int DefaultBulkCopyBatchSize = 100000;

    private const string Schema = "GlassGuide";

    private readonly IThirdPartyDataSetsDbObjectFactory thirdPartyDataSetsDbObjectFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlassGuideRepository"/> class.
    /// </summary>
    /// <param name="thirdPartyDataSetsDbObjectFactory">The third party data sets connection factory.</param>
    public GlassGuideRepository(IThirdPartyDataSetsDbObjectFactory thirdPartyDataSetsDbObjectFactory)
    {
        this.thirdPartyDataSetsDbObjectFactory = thirdPartyDataSetsDbObjectFactory;
    }

    public async Task<string> GetExistingTableIndex()
    {
        using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
        {
            string checkGlassGuideTableQuery = string.Format(CommonScriptRepository.GetExistingTableIndexBySchema, Schema);
            var index = (await connection.QueryAsync<string>(checkGlassGuideTableQuery)).SingleOrDefault();

            return index ?? string.Empty;
        }
    }

    /// <inheritdoc/>
    public async Task CreateGlassGuideTablesAndSchema(string index)
    {
        await this.CreateSchemaIfNotExists(Schema);

        using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
        {
            await connection.OpenAsync();
            using (var transaction = connection.BeginTransaction())
            {
                var sqlCreationTableScript = string.Format(GlassGuideRepositoryScripts.CreateGlassGuideTable, Schema, index);
                using (var sqlCommand = new SqlCommand(sqlCreationTableScript, connection, transaction))
                {
                    await sqlCommand.ExecuteNonQueryAsync();
                }

                transaction.Commit();
            }
        }
    }

    /// <inheritdoc/>
    public async Task CreateOrUpdateSchemaBoundView(string tableSuffix)
    {
        await this.CreateSchemaIfNotExists(Schema);

        using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
        {
            await connection.OpenAsync();
            using (var transaction = connection.BeginTransaction())
            {
                var createTableViewScript = string.Format(CommonScriptRepository.CreateOrAlterTableBoundedView, Schema, tableSuffix);
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
        var deleteSql = string.Format(CommonScriptRepository.DropTablesBySchemaAndIndex, Schema, tableSuffix);
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
            $"{Schema}.{dataTable.TableName}",
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
            string.Format(GetTableSchema, Schema, dataTableName),
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
    public DataTable GetVehiclesDataTable(string suffix)
    {
        string tableName = VehiclesTable + (string.IsNullOrEmpty(suffix) ? string.Empty : "_" + suffix);
        var dataTable = this.GetDataTableWithSchema(tableName);
        dataTable.PrimaryKey = new DataColumn[]
        {
            dataTable.Columns[(int)VehicleColumns.GlassCode],
            dataTable.Columns[(int)VehicleColumns.Nvic],
        };
        return dataTable;
    }

    /// <inheritdoc/>
    public DataTable CreateCodeDescriptionTable()
    {
        var dataTable = new DataTable();
        var colCode = dataTable.Columns.Add(CodeDescriptionColumns.Code.ToString(), typeof(string));
        dataTable.Columns.Add(CodeDescriptionColumns.Description.ToString(), typeof(string));
        var colSegment = dataTable.Columns.Add(CodeDescriptionColumns.Segment.ToString(), typeof(string));
        dataTable.PrimaryKey = new DataColumn[] { colCode, colSegment };
        return dataTable;
    }

    /// <inheritdoc/>
    public DataTable CreateRecodeTable()
    {
        var dataTable = new DataTable();
        var colCode = dataTable.Columns.Add(RecodeColumns.OldCode.ToString(), typeof(string));
        dataTable.Columns.Add(RecodeColumns.NewCode.ToString(), typeof(string));
        dataTable.Columns.Add(RecodeColumns.Date.ToString(), typeof(string));
        var colNvic = dataTable.Columns.Add(RecodeColumns.Nvic.ToString(), typeof(string));
        var colSegment = dataTable.Columns.Add(RecodeColumns.Segment.ToString(), typeof(string));
        dataTable.PrimaryKey = new DataColumn[] { colCode, colNvic, colSegment };
        return dataTable;
    }

    /// <inheritdoc/>
    public async Task<Unit> GenerateMakesFamiliesAndYearsTableFromVehicles(DataTable vehicles, string suffix)
    {
        var makeTableWithSchema = this.GetMakesDataTable(suffix);
        var familiesTableWithSchema = this.GetFamiliesDataTable(suffix);
        var yearsTableWithSchema = this.GetYearsDataTable(suffix);
        makeTableWithSchema.PrimaryKey = new DataColumn[]
        {
            makeTableWithSchema.Columns[(int)VehicleMakeColumns.MakeCode],
        };
        familiesTableWithSchema.PrimaryKey = new DataColumn[]
        {
            familiesTableWithSchema.Columns[(int)VehicleFamilyColumns.MakeCode],
            familiesTableWithSchema.Columns[(int)VehicleFamilyColumns.FamilyCode],
        };
        yearsTableWithSchema.PrimaryKey = new DataColumn[]
        {
            yearsTableWithSchema.Columns[(int)VehicleYearColumns.MakeCode],
            yearsTableWithSchema.Columns[(int)VehicleYearColumns.FamilyCode],
            yearsTableWithSchema.Columns[(int)VehicleYearColumns.Year],
        };
        makeTableWithSchema.BeginLoadData();
        familiesTableWithSchema.BeginLoadData();
        yearsTableWithSchema.BeginLoadData();
        foreach (DataRow row in vehicles.Rows)
        {
            makeTableWithSchema.LoadDataRow(this.CreateMakeRow(makeTableWithSchema, row), LoadOption.Upsert);
            familiesTableWithSchema.LoadDataRow(this.CreateFamilyRow(familiesTableWithSchema, row), LoadOption.Upsert);
            yearsTableWithSchema.LoadDataRow(this.CreateYearRow(row), LoadOption.Upsert);
        }
        makeTableWithSchema.EndLoadData();
        familiesTableWithSchema.EndLoadData();
        yearsTableWithSchema.EndLoadData();
        await this.BulkCopyAsync(makeTableWithSchema);
        await this.BulkCopyAsync(familiesTableWithSchema);
        await this.BulkCopyAsync(yearsTableWithSchema);
        return await Task.FromResult(Unit.Value);
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
        var sqlIndexCreationScriptWithSchema = string.Format(GlassGuideRepositoryScripts.GlassGuide_Add_Index_And_Fk_Constraints, Schema, index);

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
        using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
        {
            return await connection.QueryAsync<VehicleMake>(
                GlassGuideRepositoryScripts.GlassGuide_Dml_GetVehicleMakesByYear,
                new { @Year = year });
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<VehicleMake>> GetVehicleMakesAsync()
    {
        using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
        {
            return await connection.QueryAsync<VehicleMake>(GlassGuideRepositoryScripts.GlassGuide_Dml_GetVehicleMakes);
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<VehicleFamily>> GetVehicleFamiliesByMakeCodeAsync(string makeCode)
    {
        using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
        {
            return await connection.QueryAsync<VehicleFamily>(
                GlassGuideRepositoryScripts.GlassGuide_Dml_GetVehicleFamiliesByMakeCode,
                new
                {
                    MakeCode = makeCode,
                });
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<VehicleFamily>> GetVehicleFamiliesByMakeCodeAndYearGroupAsync(string makeCode, int year)
    {
        using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
        {
            return await connection.QueryAsync<VehicleFamily>(
                GlassGuideRepositoryScripts.GlassGuide_Dml_GetVehicleFamiliesByMakeCodeAndYear,
                new
                {
                    Year = year,
                    MakeCode = makeCode,
                });
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<int>> GetVehicleYearsByMakeCodeAsync(string makeCode)
    {
        using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
        {
            return await connection.QueryAsync<int>(
                GlassGuideRepositoryScripts.GlassGuide_Dml_GetVehicleYearsByMakeCode,
                new
                {
                    MakeCode = makeCode,
                });
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<int>> GetVehicleYearsByMakeCodeAndFamilyCodeAsync(string makeCode, string familyCode)
    {
        using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
        {
            return await connection.QueryAsync<int>(
                GlassGuideRepositoryScripts.GlassGuide_Dml_GetVehicleYearsByMakeCodeAndFamilyCode,
                new
                {
                    MakeCode = makeCode,
                    FamilyCode = familyCode,
                });
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Vehicle>> GetVehicleListByMakeCodeFamilyCodeAndYearAsync(string makeCode, string familyCode, int year)
    {
        using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
        {
            return await connection.QueryAsync<Vehicle>(
                GlassGuideRepositoryScripts.GlassGuide_Dml_GetVehicleListByMakeCodeFamilyCodeAndYear,
                new
                {
                    MakeCode = makeCode,
                    FamilyCode = familyCode,
                    Year = year,
                });
        }
    }

    /// <inheritdoc/>
    public async Task<VehicleDetails?> GetVehicleByVehicleKeyAsync(string vehicleKey)
    {
        using (var connection = this.thirdPartyDataSetsDbObjectFactory.GetNewConnection(DefaultConnectionTimeout))
        {
            var result = await connection.QueryFirstOrDefaultAsync<VehicleDetails>(
                GlassGuideRepositoryScripts.GlassGuide_Dml_GetVehicleByVehicleKey,
                new
                {
                    GlassCode = vehicleKey,
                });
            return result;
        }
    }

    private DataTable GetMakesDataTable(string suffix)
    {
        string tableName = MakesTable + (string.IsNullOrEmpty(suffix) ? string.Empty : "_" + suffix);
        return this.GetDataTableWithSchema(tableName);
    }

    /// <inheritdoc/>
    private DataTable GetFamiliesDataTable(string suffix)
    {
        string tableName = FamiliesTable + (string.IsNullOrEmpty(suffix) ? string.Empty : "_" + suffix);
        return this.GetDataTableWithSchema(tableName);
    }

    /// <inheritdoc/>
    private DataTable GetYearsDataTable(string suffix)
    {
        string tableName = YearsTable + (string.IsNullOrEmpty(suffix) ? string.Empty : "_" + suffix);
        return this.GetDataTableWithSchema(tableName);
    }

    private object?[] CreateMakeRow(DataTable makes, DataRow row)
    {
        object?[] addRow =
        {
            row[(int)VehicleColumns.MakeCode].ToString(),
            row[(int)VehicleColumns.MakeDescription].ToString(),
            Convert.ToInt32(row[(int)VehicleColumns.Year].ToString()),
            Convert.ToInt32(row[(int)VehicleColumns.Year].ToString()),
        };
        var oldRow = makes.Select(@$"
            {VehicleMakeColumns.MakeCode}='{row[(int)VehicleColumns.MakeCode]}'");
        if (oldRow != null && oldRow.Length > 0)
        {
            int col = (int)VehicleMakeColumns.StartYear;
            addRow[col] = Math.Min(Convert.ToInt32(addRow[col]), Convert.ToInt32(oldRow[0][col]));
            col = (int)VehicleMakeColumns.LatestYear;
            addRow[col] = Math.Max(Convert.ToInt32(addRow[col]), Convert.ToInt32(oldRow[0][col]));
        }
        return addRow;
    }

    private object?[] CreateFamilyRow(DataTable families, DataRow row)
    {
        object?[] addRow =
        {
            row[(int)VehicleColumns.MakeCode].ToString(),
            row[(int)VehicleColumns.FamilyCode].ToString(),
            row[(int)VehicleColumns.FamilyDescription].ToString(),
            row[(int)VehicleColumns.VehicleTypeCode].ToString(),
            Convert.ToInt32(row[(int)VehicleColumns.Year].ToString()),
            Convert.ToInt32(row[(int)VehicleColumns.Year].ToString()),
        };
        var oldRow = families.Select(@$"
            {VehicleFamilyColumns.MakeCode}='{row[(int)VehicleColumns.MakeCode]}' AND 
            {VehicleFamilyColumns.FamilyCode}='{row[(int)VehicleColumns.FamilyCode]}'");
        if (oldRow != null && oldRow.Length > 0)
        {
            int col = (int)VehicleFamilyColumns.StartYear;
            addRow[col] = Math.Min(Convert.ToInt32(addRow[col]), Convert.ToInt32(oldRow[0][col]));
            col = (int)VehicleFamilyColumns.LatestYear;
            addRow[col] = Math.Max(Convert.ToInt32(addRow[col]), Convert.ToInt32(oldRow[0][col]));
        }
        return addRow;
    }

    private object?[] CreateYearRow(DataRow row)
    {
        return new object?[]
        {
            row[(int)VehicleColumns.MakeCode].ToString(),
            row[(int)VehicleColumns.FamilyCode].ToString(),
            Convert.ToInt32(row[(int)VehicleColumns.Year].ToString()),
        };
    }
}