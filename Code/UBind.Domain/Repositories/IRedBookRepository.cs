// <copyright file="IRedBookRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Repositories
{
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using UBind.Domain.ThirdPartyDataSets.RedBook;

    /// <summary>
    /// Provides the contract to be use for the RedBook repository.
    /// </summary>
    public interface IRedBookRepository
    {
        /// <summary>
        /// Create the RedBook tables and view.
        /// </summary>
        Task CreateTablesAndSchema(string tableSuffix);

        Task<string> GetExistingTableIndex();

        Task CreateOrUpdateSchemaBoundView(string tableSuffix);

        /// <summary>
        /// Create the schema if not yet created.
        /// </summary>
        /// <param name="schemaName">The schema name.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CreateSchemaIfNotExists(string schemaName);

        /// <summary>
        /// Remove all RedBook tables from the target schema.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DropAllTablesByIndex(string tableSuffix);

        /// <summary>
        /// Perform bulk copy operations from the supplied DataTable to a destination table and schema.
        /// </summary>
        /// <returns>The result returned by the database after executing the command.</returns>
        Task BulkCopyAsync(DataTable dataTable);

        /// <summary>
        /// Get the data table and schema from a given table name.
        /// </summary>
        /// <returns>The result returned by the database after executing the command.</returns>
        DataTable GetDataTableWithSchema(string dataTableName);

        /// <summary>
        /// Create the RedBook foreign keys and indexes.
        /// </summary>
        /// <param name="defaultSchema">The default schema.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CreateForeignKeysAndIndexes(string defaultSchema);

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
        /// Validate if the vehicle makes year group exist in the RedBook database.
        /// </summary>
        /// <param name="year">The vehicle year .</param>
        /// <returns>A <see cref="Task"/> representing the result of the request to verify if the vehicle makes year group exists in the database.</returns>
        Task<bool> VehicleMakeYearGroupExistsAsync(int year);

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
        /// Validate if the vehicle make code exists in the RedBook database.
        /// </summary>
        /// <param name="makeCode">The vehicle make code.</param>
        /// <returns>A <see cref="Task"/> representing the result of the request to verify if the make code exists in the database.</returns>
        Task<bool> VehicleFamilyMakeCodeExistAsync(string makeCode);

        /// <summary>
        /// Validate if the vehicle family year group exists in the RedBook database.
        /// </summary>
        /// <param name="yearGroup">The vehicle year group.</param>
        /// <returns>A <see cref="Task"/> representing the result of the request to verify if the vehicle year group exists in the database.</returns>
        Task<bool> VehicleFamilyYearGroupExistsAsync(int yearGroup);

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
        /// Validate if the vehicle make code and vehicle year family code exists in the RedBook database.
        /// </summary>
        /// <param name="makeCode">The vehicle make code.</param>
        /// <param name="familyCode">The vehicle year family code.</param>
        /// <returns>A <see cref="Task"/> representing the result of the request to verify if the make code exists in the database.</returns>
        Task<bool> VehicleYearsByMakeCodeAndFamilyCodeExistsAsync(string makeCode, string familyCode);

        /// <summary>
        /// Validate if the vehicle years and make code exists in the RedBook database.
        /// </summary>
        /// <param name="makeCode">The vehicle make code.</param>
        /// <returns>A <see cref="Task"/> representing the result of the request to verify if the make code exists in the database.</returns>
        Task<bool> VehicleYearsMakeCodeExistsAsync(string makeCode);

        /// <summary>
        /// Get the collection of vehicle badges by make code, family code, year, body type and gear type.
        /// </summary>
        /// <param name="makeCode">The vehicle make code.</param>
        /// <param name="familyCode">The vehicle year family code.</param>
        /// <param name="year">The vehicle year.</param>
        /// <param name="bodyStyle">The vehicle body type.</param>
        /// <param name="gearType">The vehicle gear type.</param>
        /// <returns>A <see cref="Task"/> representing the result of the request to obtain the list of vehicle families.</returns>
        Task<IEnumerable<VehicleBadge>> GetVehicleBadgesByMakeCodeFamilyCodeYearBodyStyleAndGearType(string makeCode, string familyCode, int year, string? bodyStyle, string? gearType);

        /// <summary>
        /// Get the collection of vehicle badges by make code, family code, year, body type and gear type.
        /// </summary>
        /// <param name="makeCode">The vehicle make code.</param>
        /// <param name="familyCode">The vehicle year family code.</param>
        /// <param name="year">The vehicle year.</param>
        /// <param name="badge">The vehicle badge.</param>
        /// <param name="gearType">The vehicle gear type.</param>
        /// <returns>A <see cref="Task"/> representing the result of the request to obtain the list of vehicle families.</returns>
        Task<IEnumerable<string>> GetVehicleBodyStyleByMakeCodeFamilyCodeYearBadgeAndGearType(string makeCode, string familyCode, int year, string? badge, string? gearType);

        /// <summary>
        /// Get the collection of vehicle badges by make code, family code, year, body type and gear type.
        /// </summary>
        /// <param name="makeCode">The vehicle make code.</param>
        /// <param name="familyCode">The vehicle year family code.</param>
        /// <param name="year">The vehicle year.</param>
        /// <param name="badge">The vehicle badge.</param>
        /// <param name="bodyStyle">The vehicle body style.</param>
        /// <returns>A <see cref="Task"/> representing the result of the request to obtain the list of vehicle families.</returns>
        Task<IEnumerable<string>> GetVehicleGearTypeByMakeCodeFamilyCodeYearBadgeAndBodyStyle(string makeCode, string familyCode, int year, string? badge, string? bodyStyle);

        /// <summary>
        /// Get the collection of vehicle families by make code and vehicle makes year group.
        /// </summary>
        /// <param name="makeCode">The vehicle make code.</param>
        /// <param name="familyCode">The vehicle year family code.</param>
        /// <param name="year">The vehicle year.</param>
        /// <returns>A <see cref="Task"/> representing the result of the request to obtain the list of vehicles.</returns>
        Task<IEnumerable<Vehicle>> GetVehicleListAsync(string makeCode, string familyCode, int year, string? badge, string? bodyStyle, string? gearType);

        /// <summary>
        /// Validate if the vehicle make code and vehicle family code exists in the RedBook database.
        /// </summary>
        /// <param name="makeCode">The vehicle make code.</param>
        /// <param name="familyCode">The vehicle year family code.</param>
        /// <returns>A <see cref="Task"/> representing the result of the request to verify if the family code  doesn't match a RedBook family code for the specified MakeCode in the database.</returns>
        Task<bool> VehicleByMakeCodeAndFamilyCodeExistsAsync(string makeCode, string familyCode);

        /// <summary>
        /// Verify if year doesn't match the RedBook family code for the specified make code exists in the database.
        /// </summary>
        /// <param name="makeCode">The vehicle make code.</param>
        /// <param name="familyCode">The vehicle year family code.</param>
        /// <param name="year">The vehicle year.</param>
        /// <returns>A <see cref="Task"/> representing the result of the request to verify if year doesn't match the RedBook family code for the specified make code exists in the database.</returns>
        Task<bool> VehicleByMakeCodeFamilyCodeAndYearExistsAsync(string makeCode, string familyCode, int year);

        /// <summary>
        /// Get the vehicle details by vehicle key.
        /// </summary>
        /// <param name="vehicleKey">The vehicle key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the request to obtain the vehicle details.</returns>
        Task<VehicleDetails> GetVehicleByVehicleKeyAsync(string vehicleKey);
    }
}
