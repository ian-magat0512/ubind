// <copyright file="DbaSchedulerTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Scheduler;

using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using UBind.Application.Configuration;
using UBind.Application.Scheduler.Jobs;
using UBind.Application.Services.Email;
using UBind.Domain.ReadModels.Dba;
using UBind.Domain.Repositories;
using UBind.Persistence;
using UBind.Persistence.Clients.DVA.Migrations;
using UBind.Persistence.ThirdPartyDataSets;
using Xunit;

public class DbaSchedulerTest
{
    private List<SqlServerSysProcessViewModel> repoResult;

    public DbaSchedulerTest()
    {
        this.repoResult = new List<SqlServerSysProcessViewModel>();
    }

    [Theory]
    [InlineData(80, "Email warning threshold warning escalated.")]
    [InlineData(90, "connections has a normal threshold:")]
    public async void Verify_Dva_Threshold_Scheduler(int threshold, string expectedString)
    {
        // Mock repository
        this.GenerateSysViewData("dvaDb");
        var mockProvider = new Mock<IServiceProvider>();
        var mockDbaContext = new Mock<DvaDbContext>();

        var mockDbRepository = new Mock<IDbaRepository<DvaDbContext>>();
        mockDbRepository.Setup(mtd => mtd.GetActiveConnections()).Returns(this.repoResult);
        mockDbRepository.Setup(mtd => mtd.GetMaxConnectionPool()).Returns(100);

        var mockLogger = new Mock<ILogger<DvaDbaScheduler>>();
        var mockStorage = new Mock<IStorageConnection>();
        var dbConfiguration = new DbMonitoringConfiguration { SqlDatabaseConnectionCountNotificationThreshold = threshold };
        var mockErrorNotification = new Mock<IErrorNotificationService>();
        var mockRecurringJobManager = new Mock<IRecurringJobManager>();
        var dvaScheduler = new DvaDbaScheduler(
            mockLogger.Object,
            mockDbRepository.Object,
            mockStorage.Object,
            mockRecurringJobManager.Object,
            dbConfiguration,
            mockErrorNotification.Object);

        await dvaScheduler.ExecuteDbaMonitoring();

        // Assert
        mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(level => level == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(expectedString, StringComparison.OrdinalIgnoreCase)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
    }

    [Theory]
    [InlineData(80, "Email warning threshold warning escalated.")]
    [InlineData(90, "connections has a normal threshold:")]
    public async void Verify_UBindDb_Threshold_Scheduler(int threshold, string expectedString)
    {
        // Mock repository
        this.GenerateSysViewData("ubindDb");
        var mockProvider = new Mock<IServiceProvider>();
        var mockDbaContext = new Mock<UBindDbContext>();

        var mockDbRepository = new Mock<IDbaRepository<UBindDbContext>>();
        mockDbRepository.Setup(mtd => mtd.GetActiveConnections()).Returns(this.repoResult);
        mockDbRepository.Setup(mtd => mtd.GetMaxConnectionPool()).Returns(100);

        var mockLogger = new Mock<ILogger<UBindDbaScheduler>>();
        var mockStorage = new Mock<IStorageConnection>();
        var dbConfiguration = new DbMonitoringConfiguration { SqlDatabaseConnectionCountNotificationThreshold = threshold };
        var mockErrorNotification = new Mock<IErrorNotificationService>();
        var mockRecurringJobManager = new Mock<IRecurringJobManager>();
        var dvaScheduler = new UBindDbaScheduler(
            mockLogger.Object,
            mockDbRepository.Object,
            mockStorage.Object,
            mockRecurringJobManager.Object,
            dbConfiguration,
            mockErrorNotification.Object);

        await dvaScheduler.ExecuteDbaMonitoring();

        // Assert
        mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(level => level == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(expectedString, StringComparison.OrdinalIgnoreCase)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
    }

    [Theory]
    [InlineData(80, "Email warning threshold warning escalated.")]
    [InlineData(90, "connections has a normal threshold:")]
    public async void Verify_Third_Party_Data_Set_Threshold_Scheduler(int threshold, string expectedString)
    {
        // Mock repository
        this.GenerateSysViewData("ubindDb");
        var mockProvider = new Mock<IServiceProvider>();
        var mockDbaContext = new Mock<ThirdPartyDataSetsDbContext>();

        var mockDbRepository = new Mock<IDbaRepository<ThirdPartyDataSetsDbContext>>();
        mockDbRepository.Setup(mtd => mtd.GetActiveConnections()).Returns(this.repoResult);
        mockDbRepository.Setup(mtd => mtd.GetMaxConnectionPool()).Returns(100);

        var mockLogger = new Mock<ILogger<ThirdPartyDataSetsDbaScheduler>>();
        var mockStorage = new Mock<IStorageConnection>();
        var dbConfiguration = new DbMonitoringConfiguration { SqlDatabaseConnectionCountNotificationThreshold = threshold };
        var mockErrorNotification = new Mock<IErrorNotificationService>();
        var mockRecurringJobManager = new Mock<IRecurringJobManager>();
        var dvaScheduler = new ThirdPartyDataSetsDbaScheduler(
            mockLogger.Object,
            mockDbRepository.Object,
            mockStorage.Object,
            mockRecurringJobManager.Object,
            dbConfiguration,
            mockErrorNotification.Object);

        await dvaScheduler.ExecuteDbaMonitoring();

        // Assert
        mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(level => level == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(expectedString, StringComparison.OrdinalIgnoreCase)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
    }

    private void GenerateSysViewData(string dbName)
    {
        this.repoResult.Add(new SqlServerSysProcessViewModel
        {
            DbId = 0,
            DbName = dbName,
            LoginName = "server01",
            NumberOfConnections = 87,
        });
    }
}
