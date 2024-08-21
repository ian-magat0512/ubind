// <copyright file="IGlassGuideRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Repositories;

using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MediatR;
using UBind.Domain.ThirdPartyDataSets.GlassGuide;

/// <summary>
/// Provides the contract to be use for the Glass's Guide repository.
/// </summary>
public interface IGlassGuideRepository
{
    /// <summary>
    /// Gets the existing table index.
    /// </summary>
    /// <returns>The index.</returns>
    Task<string> GetExistingTableIndex();

    /// <summary>
    /// Create the Glass's Guide tables and view.
    /// </summary>
    /// <param name="defaultSchema">The default schema.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task CreateGlassGuideTablesAndSchema(string defaultSchema);

    /// <summary>
    /// Create or Update Glass's Guide schema bound view.
    /// </summary>
    /// <param name="index"></param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task CreateOrUpdateSchemaBoundView(string index);

    /// <summary>
    /// Create the schema if not yet created.
    /// </summary>
    /// <param name="schemaName">The schema name.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task CreateSchemaIfNotExists(string schemaName);

    /// <summary>
    /// Remove all Glass's Guide tables from the target schema.
    /// </summary>
    /// <param name="index">The target index.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task DropAllTablesByIndex(string index);

    /// <summary>
    /// Perform bulk copy operations from the supplied DataTable to a destination table and schema.
    /// </summary>
    /// <param name="dataTable">The data table.</param>
    /// <returns>The result returned by the database after executing the command.</returns>
    Task BulkCopyAsync(DataTable dataTable);

    /// <summary>
    /// Get the data table and schema from a given table name.
    /// </summary>
    /// <param name="dataTableName">The table name.</param>
    /// <returns>The result returned by the database after executing the command.</returns>
    DataTable GetDataTableWithSchema(string dataTableName);

    /// <summary>
    /// Get an instance of vehicles table.
    /// </summary>
    /// <param name="suffix">The index suffix.</param>
    /// <returns>The data table instance.</returns>
    public DataTable GetVehiclesDataTable(string suffix);

    /// <summary>
    /// Generate contents of Make, Family, and Year tables from Vehicles data table.
    /// </summary>
    /// <param name="vehicles">The Vehicles data table.</param>
    /// <param name="suffix">The index suffix.</param>
    /// <returns></returns>
    public Task<Unit> GenerateMakesFamiliesAndYearsTableFromVehicles(DataTable vehicles, string suffix);

    /// <summary>
    /// Create a temporary instance of data table with structure based on Glass's Guide MAK, BDY, ENG, and TRN data segment files.
    /// </summary>
    /// <returns>The data table instance.</returns>
    public DataTable CreateCodeDescriptionTable();

    /// <summary>
    /// Create a temporary instance of data table with structure based on Glass's Guide REC data segment file.
    /// </summary>
    /// <returns>The data table instance.</returns>
    public DataTable CreateRecodeTable();

    /// <summary>
    /// Create the Glass's Guide foreign keys and indexes.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task CreateForeignKeysAndIndexes(string index);

    /// <summary>
    /// Get the collection of vehicle makes by vehicle year group.
    /// </summary>
    /// <param name="year">The vehicle year .</param>
    /// <returns>A <see cref="Task"/> representing the result of the request to obtain the list of vehicle makes by year group.</returns>
    Task<IEnumerable<VehicleMake>> GetVehicleMakesByYearGroupAsync(int year);

    /// <summary>
    /// Get the collection of vehicle makes.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the request to obtain the list of vehicle makes.</returns>
    Task<IEnumerable<VehicleMake>> GetVehicleMakesAsync();

    /// <summary>
    /// Get the collection of vehicle families by make code.
    /// </summary>
    /// <param name="makeCode">The vehicle make code.</param>
    /// <returns>A <see cref="Task"/> representing the result of the request to obtain the list of vehicle families.</returns>
    Task<IEnumerable<VehicleFamily>> GetVehicleFamiliesByMakeCodeAsync(string makeCode);

    /// <summary>
    /// Get the collection of vehicle families by make code and vehicle makes year group.
    /// </summary>
    /// <param name="makeCode">The vehicle make code.</param>
    /// <param name="year">The vehicle year.</param>
    /// <returns>A <see cref="Task"/> representing the result of the request to obtain the list of vehicle families.</returns>
    Task<IEnumerable<VehicleFamily>> GetVehicleFamiliesByMakeCodeAndYearGroupAsync(string makeCode, int year);

    /// <summary>
    /// Get the collection of vehicle families by make code and vehicle makes year group.
    /// </summary>
    /// <param name="makeCode">The vehicle make code.</param>
    /// <param name="familyCode">The vehicle year family code.</param>
    /// <returns>A <see cref="Task"/> representing the result of the request to obtain the list of vehicle years.</returns>
    Task<IEnumerable<int>> GetVehicleYearsByMakeCodeAndFamilyCodeAsync(string makeCode, string familyCode);

    /// <summary>
    /// Get the collection of vehicle years by vehicle make code and vehicle family code.
    /// </summary>
    /// <param name="makeCode">The vehicle make code.</param>
    /// <returns>A <see cref="Task"/> representing the result of t1he request to obtain the list of vehicle years.</returns>
    Task<IEnumerable<int>> GetVehicleYearsByMakeCodeAsync(string makeCode);

    /// <summary>
    /// Get the collection of vehicle families by make code and vehicle makes year group.
    /// </summary>
    /// <param name="makeCode">The vehicle make code.</param>
    /// <param name="familyCode">The vehicle year family code.</param>
    /// <param name="year">The vehicle year.</param>
    /// <returns>A <see cref="Task"/> representing the result of the request to obtain the list of vehicles.</returns>
    Task<IEnumerable<Vehicle>> GetVehicleListByMakeCodeFamilyCodeAndYearAsync(string makeCode, string familyCode, int year);

    /// <summary>
    /// Get the vehicle details by vehicle key.
    /// </summary>
    /// <param name="vehicleKey">The vehicle key.</param>
    /// <returns>A <see cref="Task"/> representing the result of the request to obtain the vehicle details.</returns>
    Task<VehicleDetails?> GetVehicleByVehicleKeyAsync(string vehicleKey);
}