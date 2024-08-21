// <copyright file="RestructureReleaseDataCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Migration;

using MediatR;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Repositories;
using UBind.Domain.Services.Maintenance;

public class RestructureReleaseDataCommandHandler : ICommandHandler<RestructureReleaseDataCommand, Unit>
{
    private readonly IUBindDbContext dbContext;
    private readonly ILogger<RestructureReleaseDataCommandHandler> logger;
    private readonly IDbLogFileMaintenanceService dbLogFileMaintenanceService;

    public RestructureReleaseDataCommandHandler(
        IUBindDbContext dbContext,
        ILogger<RestructureReleaseDataCommandHandler> logger,
        IDbLogFileMaintenanceService dbLogFileMaintenanceService)
    {
        this.dbContext = dbContext;
        this.logger = logger;
        this.dbLogFileMaintenanceService = dbLogFileMaintenanceService;
    }

    public async Task<Unit> Handle(RestructureReleaseDataCommand request, CancellationToken cancellationToken)
    {
        // We're setting this to 90 seconds, however on SSD we're not expecting any query to take more than 5 seconds.
        // On the UAT database which runs on magnetic (which is orders of magnitude slower), we've seen some queries
        // take longer than 30 seconds. So for testing purposes, we need to increase this beyond that normal 30 second
        // timeout.
        this.dbContext.Database.CommandTimeout = 90;
        await this.dbLogFileMaintenanceService.ThrowIfUserDoesNotHaveRequiredPermissions();
        await this.dbLogFileMaintenanceService.ShrinkLogFileIfNeeded();
        await this.DeleteAllDevReleasesWhichAreNotTheLatest(cancellationToken);
        await this.PopulateReleaseDetailsPropertiesOnDevRelease();
        await this.PopulateReleaseDetailsPropertiesOnRelease();
        await this.DeleteAssetsOfDanglingReleaseDetails();
        await this.DeleteDanglingReleaseDetails();
        await this.DeleteDanglingAssets();
        await this.PopulateReleaseDescriptions();
        return Unit.Value;
    }

    private async Task DeleteAllDevReleasesWhichAreNotTheLatest(CancellationToken cancellationToken)
    {
        int deletedRows;
        int batchSize = 500;
        int totalDeletedRows = 0;
        this.logger.LogInformation($"Deleting DevReleases which are not the latest...");
        do
        {
            string sql = @"
                    WITH RankedDevReleases AS (
                        SELECT 
                            TOP (@batchSize)
                            ID,
                            ROW_NUMBER() OVER (PARTITION BY TenantId, ProductId ORDER BY CreatedTicksSinceEpoch DESC) AS rn
                        FROM 
                            DevReleases
                    )
                    DELETE
                    FROM DevReleases
                    WHERE ID IN (SELECT ID FROM RankedDevReleases WHERE rn != 1);
                ";

            deletedRows = await this.dbContext.Database.ExecuteSqlCommandAsync(
                sql,
                new SqlParameter("@batchSize", batchSize));

            totalDeletedRows += deletedRows;
            if (deletedRows > 0)
            {
                this.logger.LogInformation($"    Deleted {deletedRows} DevReleases in this batch. Total deleted rows: {totalDeletedRows}");
                await Task.Delay(500, cancellationToken);
            }
        }
        while (deletedRows > 0);
        this.logger.LogInformation($"Completed deleting {totalDeletedRows} older DevReleases.");
    }

    private async Task PopulateReleaseDetailsPropertiesOnDevRelease()
    {
        this.logger.LogInformation($"Populating ReleaseDetails properties on DevReleases...");
        string sql = @"
                WITH LatestEvents AS (
                    SELECT 
                        DevRelease_Id,
                        QuoteDetails_Id,
                        ClaimDetails_Id,
                        ROW_NUMBER() OVER (PARTITION BY DevRelease_Id ORDER BY CreatedTicksSinceEpoch DESC) AS rn
                    FROM 
                        ReleaseEvents
                    WHERE 
                        DevRelease_Id IS NOT NULL
                )
                UPDATE DR
                SET 
                    QuoteDetails_Id = LE.QuoteDetails_Id,
                    ClaimDetails_Id = LE.ClaimDetails_Id
                FROM 
                    DevReleases DR
                JOIN 
                    LatestEvents LE ON DR.ID = LE.DevRelease_Id
                WHERE 
                    LE.rn = 1;
            ";

        int affectedRows = await this.dbContext.Database.ExecuteSqlCommandAsync(sql);
        this.logger.LogInformation($"{affectedRows} DevReleases were updated with release details IDs.");
    }

    private async Task PopulateReleaseDetailsPropertiesOnRelease()
    {
        this.logger.LogInformation($"Populating ReleaseDetails properties on Releases...");

        int batchSize = 200;
        int offset = 0;
        int totalAffectedRows = 0;

        while (true)
        {
            // Get batch of Release_Id's
            var releaseIds = this.dbContext.Releases
                .OrderBy(r => r.Id)
                .Skip(offset)
                .Take(batchSize)
                .Select(r => r.Id)
                .ToList();

            if (!releaseIds.Any())
            {
                break;
            }

            // Create a list of Ids for the SQL IN clause
            string joinedIds = string.Join(",", releaseIds.Select(id => $"'{id}'"));

            // Your SQL query to update in batches
            string sql = $@"
                WITH LatestEventsForRelease AS (
                    SELECT 
                        Release_Id,
                        QuoteDetails_Id,
                        ClaimDetails_Id,
                        ROW_NUMBER() OVER (PARTITION BY Release_Id ORDER BY CreatedTicksSinceEpoch DESC) AS rn
                    FROM 
                        ReleaseEvents
                    WHERE 
                        Release_Id IN ({joinedIds})
                )
                UPDATE R
                SET 
                    QuoteDetails_Id = LER.QuoteDetails_Id,
                    ClaimDetails_Id = LER.ClaimDetails_Id
                FROM 
                    Releases R
                JOIN 
                    LatestEventsForRelease LER ON R.Id = LER.Release_Id
                WHERE 
                    LER.rn = 1;
            ";

            int affectedRows = await this.dbContext.Database.ExecuteSqlCommandAsync(sql);
            totalAffectedRows += affectedRows;

            this.logger.LogInformation($"    {affectedRows} Releases in batch were updated with release details IDs. Total: {totalAffectedRows}");

            if (releaseIds.Count < batchSize)
            {
                break;
            }

            offset += batchSize;
        }

        this.logger.LogInformation($"{totalAffectedRows} Releases were updated with release details IDs in total.");
    }

    private async Task DeleteAssetsOfDanglingReleaseDetails()
    {
        this.logger.LogInformation($"Deleting Assets associated with dangling ReleaseDetails...");

        // Step 1: Check if temp table exists, create if it doesn't
        string checkOrCreateTableSql = @"
            DECLARE @Result INT;

            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TempDanglingReleaseDetails_UB10236')
            BEGIN
                CREATE TABLE TempDanglingReleaseDetails_UB10236 (Id uniqueidentifier);
                SET @Result = -1;
            END
            ELSE
            BEGIN
                SELECT @Result = COUNT(*) FROM TempDanglingReleaseDetails_UB10236;
            END

            SELECT @Result;
        ";

        int result = this.dbContext.Database.SqlQuery<int>(checkOrCreateTableSql).First();
        if (result == -1)
        {
            this.logger.LogInformation("Table TempDanglingReleaseDetails_UB10236 was created.");
        }
        else
        {
            this.logger.LogInformation($"Table TempDanglingReleaseDetails_UB10236 already exists with {result} rows.");
        }

        await this.dbLogFileMaintenanceService.ShrinkLogFileIfNeeded();

        // Step 2: Batch insert IDs of dangling ReleaseDetails to Temp table
        Guid lastMaxId = Guid.Empty;
        int rowsAffected;
        int totalRowsInserted = result > 0 ? result : 0;
        int batchSize = 500;
        this.logger.LogInformation($"Adding IDs of dangling ReleaseDetails to the temp table:");

        do
        {
            string batchInsertSql = $@"
                INSERT INTO TempDanglingReleaseDetails_UB10236
                SELECT TOP (@batchSize) RD.Id
                FROM 
                    ReleaseDetails RD
                LEFT JOIN
                    DevReleases DR ON RD.Id = DR.QuoteDetails_Id OR RD.Id = DR.ClaimDetails_Id
                LEFT JOIN
                    Releases R ON RD.Id = R.QuoteDetails_Id OR RD.Id = R.ClaimDetails_Id
                WHERE 
                    (DR.ID IS NULL AND R.Id IS NULL)
                    AND RD.Id > @LastMaxId
                ORDER BY RD.Id;
            ";

            SqlParameter[] sqlParams = {
                new SqlParameter("@batchSize", batchSize),
                new SqlParameter("@LastMaxId", lastMaxId),
            };

            rowsAffected = await this.dbContext.Database.ExecuteSqlCommandAsync(batchInsertSql, sqlParams);

            if (rowsAffected > 0)
            {
                // Retrieve the max ID for this batch and use it as the lastMaxId for the next batch
                lastMaxId = this.dbContext.Database.SqlQuery<Guid>(
                    "SELECT MAX(Id) FROM TempDanglingReleaseDetails_UB10236"
                ).First();

                totalRowsInserted += rowsAffected;
                this.logger.LogInformation($"    Added IDs of {rowsAffected} dangling release details to temp table. Total: {totalRowsInserted}");
            }
            else
            {
                this.logger.LogInformation($"Finished adding IDs of dangling release details to temp table. Total: {totalRowsInserted}.");
            }
        }
        while (rowsAffected > 0);

        int deletedRows;
        int deleteBatchSize = 100;
        int totalDeletedRows = 0;
        this.logger.LogInformation($"Deleting Assets associated with dangling ReleaseDetails:");

        do
        {
            // Step 3: Delete Assets using IDs from the temp table
            string deleteAssetsSql = @"
                DELETE TOP (@batchSize)
                FROM Assets
                WHERE ReleaseDetails_Id IN (SELECT Id FROM TempDanglingReleaseDetails_UB10236) 
                OR ReleaseDetails_Id1 IN (SELECT Id FROM TempDanglingReleaseDetails_UB10236);
            ";

            deletedRows = await this.dbContext.Database.ExecuteSqlCommandAsync(
                deleteAssetsSql,
                new SqlParameter("@batchSize", deleteBatchSize)
            );

            totalDeletedRows += deletedRows;
            if (deletedRows > 0)
            {
                this.logger.LogInformation($"    Deleted {deletedRows} associated Assets in this batch. Total deleted: {totalDeletedRows}");
            }

            if (totalDeletedRows % (deleteBatchSize * 20) == 0)
            {
                await this.dbLogFileMaintenanceService.CleanLogFileIfNeeded();
            }

            if (totalDeletedRows % (deleteBatchSize * 40) == 0)
            {
                await this.dbLogFileMaintenanceService.ShrinkLogFileIfNeeded();
            }
        }
        while (deletedRows > 0);
        this.logger.LogInformation($"{totalDeletedRows} Assets associated with dangling ReleaseDetails were deleted.");

        // Step 4: Drop the temporary table
        this.logger.LogInformation($"Dropping temporary table TempDanglingReleaseDetails_UB10236.");
        string dropTempTableSql = "DROP TABLE TempDanglingReleaseDetails_UB10236;";
        await this.dbContext.Database.ExecuteSqlCommandAsync(dropTempTableSql);
    }

    private async Task DeleteDanglingReleaseDetails()
    {
        this.logger.LogInformation($"Deleting ReleaseDetails not referenced by a DevRelease or Release...");

        int deletedRows;
        int batchSize = 20;
        int totalDeletedRows = 0;

        do
        {
            string sql = @"
                DELETE TOP (@batchSize)
                FROM ReleaseDetails
                WHERE Id IN (
                    SELECT TOP (@batchSize) RD.Id
                    FROM 
                        ReleaseDetails RD
                    LEFT JOIN
                        DevReleases DR ON RD.Id = DR.QuoteDetails_Id OR RD.Id = DR.ClaimDetails_Id
                    LEFT JOIN
                        Releases R ON RD.Id = R.QuoteDetails_Id OR RD.Id = R.ClaimDetails_Id
                    WHERE 
                        DR.ID IS NULL AND R.Id IS NULL
                )
            ";

            deletedRows = await this.dbContext.Database.ExecuteSqlCommandAsync(
                sql,
                new SqlParameter("@batchSize", batchSize)
            );

            totalDeletedRows += deletedRows;
            if (deletedRows > 0)
            {
                this.logger.LogInformation($"    Deleted {deletedRows} ReleaseDetails in this batch. Total deleted: {totalDeletedRows}");
            }

            if (totalDeletedRows % (batchSize * 20) == 0)
            {
                await this.dbLogFileMaintenanceService.CleanLogFileIfNeeded();
            }

            if (totalDeletedRows % (batchSize * 40) == 0)
            {
                await this.dbLogFileMaintenanceService.ShrinkLogFileIfNeeded();
            }
        }
        while (deletedRows > 0);
        this.logger.LogInformation($"{totalDeletedRows} dangling ReleaseDetails were deleted.");
    }

    private async Task DeleteDanglingAssets()
    {
        this.logger.LogInformation($"Deleting Assets not referenced by a DevRelease or Release...");

        // Step 1: Check if temp table exists, create if it doesn't
        this.logger.LogInformation("Creating temporary table TempReleaseDetails_UB10236...");
        string checkOrCreateTableSql = @"
            DECLARE @Result INT;

            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TempReleaseDetails_UB10236')
            BEGIN
                CREATE TABLE TempReleaseDetails_UB10236 (Id uniqueidentifier);
                SET @Result = -1;
            END
            ELSE
            BEGIN
                SELECT @Result = COUNT(*) FROM TempReleaseDetails_UB10236;
            END

            SELECT @Result;
        ";

        int result = this.dbContext.Database.SqlQuery<int>(checkOrCreateTableSql).First();
        if (result == -1)
        {
            this.logger.LogInformation("Table TempReleaseDetails_UB10236 was created.");
        }
        else
        {
            this.logger.LogInformation($"Table TempReleaseDetails_UB10236 already exists with {result} rows.");
        }

        await this.dbLogFileMaintenanceService.ShrinkLogFileIfNeeded();

        // Step 2: Populate temp table with all ReleaseDetails IDs
        this.logger.LogInformation("Populating TempReleaseDetails_UB10236 with all IDs of all ReleaseDetails...");
        Guid lastMaxId = Guid.Empty;
        int rowsAffected;
        int totalRowsInserted = result > 0 ? result : 0;
        int batchSize = 500;

        do
        {
            string batchInsertSql = $@"
                INSERT INTO TempReleaseDetails_UB10236
                SELECT TOP (@batchSize) RD.Id
                FROM 
                    ReleaseDetails RD
                WHERE 
                    RD.Id > @LastMaxId
                ORDER BY RD.Id;
            ";

            SqlParameter[] sqlParams = {
                new SqlParameter("@batchSize", batchSize),
                new SqlParameter("@LastMaxId", lastMaxId),
            };

            rowsAffected = await this.dbContext.Database.ExecuteSqlCommandAsync(batchInsertSql, sqlParams);

            if (rowsAffected > 0)
            {
                // Retrieve the max ID for this batch and use it as the lastMaxId for the next batch
                lastMaxId = this.dbContext.Database.SqlQuery<Guid>(
                    "SELECT MAX(Id) FROM TempReleaseDetails_UB10236"
                ).First();

                totalRowsInserted += rowsAffected;
                this.logger.LogInformation($"    Added IDs of {rowsAffected} release details to temp table. Total: {totalRowsInserted}");
            }
            else
            {
                this.logger.LogInformation($"Finished adding IDs of release details to temp table. Total: {totalRowsInserted}.");
            }
        }
        while (rowsAffected > 0);

        // Step 3: Delete Assets using IDs from the temp table
        int deletedRows;
        batchSize = 100;
        int totalDeletedRows = 0;

        do
        {
            string sql = @"
                DELETE A
                FROM 
                    (SELECT TOP (@batchSize) * FROM Assets) A
                LEFT JOIN TempReleaseDetails_UB10236 T1 ON A.ReleaseDetails_Id = T1.Id
                LEFT JOIN TempReleaseDetails_UB10236 T2 ON A.ReleaseDetails_Id1 = T2.Id
                WHERE 
                    (A.ReleaseDetails_Id IS NOT NULL AND T1.Id IS NULL)
                    OR
                    (A.ReleaseDetails_Id1 IS NOT NULL AND T2.Id IS NULL)";

            deletedRows = await this.dbContext.Database.ExecuteSqlCommandAsync(
                sql,
                new SqlParameter("@batchSize", batchSize));

            totalDeletedRows += deletedRows;
            if (deletedRows > 0)
            {
                Console.WriteLine($"    Deleted {deletedRows} dangling Assets in this batch. Total deleted: {totalDeletedRows}");
            }

            if (totalDeletedRows % (batchSize * 20) == 0)
            {
                await this.dbLogFileMaintenanceService.CleanLogFileIfNeeded();
            }

            if (totalDeletedRows % (batchSize * 40) == 0)
            {
                await this.dbLogFileMaintenanceService.ShrinkLogFileIfNeeded();
            }
        }
        while (deletedRows > 0);
        this.logger.LogInformation($"{totalDeletedRows} dangling Assets were deleted.");

        // Step 4: Drop the temporary table
        this.logger.LogInformation($"Dropping temporary table TempReleaseDetails_UB10236.");
        string dropTempTableSql = "DROP TABLE TempReleaseDetails_UB10236;";
        await this.dbContext.Database.ExecuteSqlCommandAsync(dropTempTableSql);
    }

    private async Task PopulateReleaseDescriptions()
    {
        this.logger.LogInformation($"Updating dbo.Releases with the description from dbo.ReleaseDescriptions...");
        string sql = @"
                UPDATE R
                SET R.Description = RD.Description
                FROM Releases R
                INNER JOIN ReleaseDescriptions RD ON R.Id = RD.Release_Id
                WHERE RD.DevRelease_Id IS NULL;
            ";

        int updatedRows = await this.dbContext.Database.ExecuteSqlCommandAsync(sql);
        this.logger.LogInformation($"Updated {updatedRows} rows in the Releases table.");
    }
}
