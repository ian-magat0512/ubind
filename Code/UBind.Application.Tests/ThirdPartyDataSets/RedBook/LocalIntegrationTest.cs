// <copyright file="LocalIntegrationTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.ThirdPartyDataSets.RedBook
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using FluentFTP;
    using Humanizer;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using UBind.Application.Commands.ThirdPartyDataSets;
    using UBind.Application.Commands.ThirdPartyDataSets.RedBook;
    using UBind.Application.DataDownloader;
    using UBind.Application.Queries.ThirdPartyDataSets.RedBook;
    using UBind.Application.Services.DelimiterSeparatedValues;
    using UBind.Application.Tests.Helpers;
    using UBind.Application.ThirdPartyDataSets;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;
    using UBind.Domain.Repositories.FileSystem;
    using UBind.Domain.Tests;
    using UBind.Domain.ThirdPartyDataSets;
    using UBind.Domain.ThirdPartyDataSets.RedBook;
    using UBind.Persistence.Services.Filesystem;
    using UBind.Persistence.ThirdPartyDataSets;
    using Xunit;

    [TestCaseOrderer("UBind.Application.Tests.Helpers.PriorityOrderer", "UBind.Application.Tests")]
    public class LocalIntegrationTest : IClassFixture<ThirdPartyDataSetsTestFixture>
    {
        private readonly ThirdPartyDataSetsTestFixture thirdPartyDataSetsTestFixture;
        private readonly ServiceCollection serviceCollection;

        public LocalIntegrationTest(ThirdPartyDataSetsTestFixture thirdPartyDataSetsTestFixture)
        {
            var services = new ServiceCollection();
            services.AddSingleton<ICommandHandler<DownloadFilesCommand, IReadOnlyList<(string fileName, string fileHash)>>, DownloadFilesCommandHandler>();
            services.AddSingleton<ICommandHandler<ExtractArchivesCommand, string>, ExtractArchivesCommandHandler>();
            services.AddSingleton<ICommandHandler<CreateTablesAndSchemaCommand, Unit>, CreateTablesAndSchemaCommandHandler>();
            services.AddSingleton<ICommandHandler<ImportDelimiterSeparatedValuesCommand, Unit>, ImportDelimiterSeparatedValuesCommandHandler>();
            services.AddSingleton<ICommandHandler<ArchiveDownloadedFilesCommand, IReadOnlyList<string>>, ArchiveDownloadedFilesCommandHandler>();
            services.AddSingleton<ICommandHandler<CleanUpUpdaterJobCommand, Unit>, CleanUpUpdaterJobCommandHandler>();
            services.AddSingleton<IQueryHandler<GetVehicleMakesQuery, IEnumerable<VehicleMake>>, GetVehicleMakesQueryHandler>();

            services.AddSingleton<IFileSystemService, FileService>();
            services.AddSingleton<IFtpClient, FtpClient>();
            services.AddSingleton<IFileSystemFileCompressionService, FilesystemFileCompressionService>();
            services.AddSingleton<IDataDownloaderService, DataDownloaderService>();
            services.AddSingleton<IFtpClientFactory, FtpClientFactory>();

            var updaterJobManifest = new UpdaterJobManifest(DataDownloaderProtocol.Ftp, false);
            services.AddSingleton(updaterJobManifest);

            var appSettingsConfig = new ConfigurationBuilder().AddJsonFile("appsettings.localtest.json").Build();
            var connectionString = appSettingsConfig.GetConnectionString("ThirdPartyDataSets");
            services.AddSingleton<IThirdPartyDataSetsDbConfiguration>(config => new ThirdPartyDataSetsDbConfiguration(connectionString));
            services.AddScoped(ctx => new ThirdPartyDataSetsDbContext(connectionString));

            var mockThirdPartyDataSetsConfiguration = new Mock<IThirdPartyDataSetsConfiguration>();
            mockThirdPartyDataSetsConfiguration.Setup(m => m.UpdaterJobsPath).Returns(() => @"C:\ThirdPartyDataSets\UpdaterJobs");
            mockThirdPartyDataSetsConfiguration.Setup(m => m.FileHashesPath).Returns(() => @"C:\ThirdPartyDataSets\FileHashes");
            mockThirdPartyDataSetsConfiguration.Setup(m => m.DownloadBufferSize).Returns(() => 4096);
            mockThirdPartyDataSetsConfiguration.Setup(m => m.DownloadedFolder).Returns(() => "Downloaded");
            mockThirdPartyDataSetsConfiguration.Setup(m => m.ExtractedFolder).Returns(() => "Extracted");

            services.AddSingleton(config => mockThirdPartyDataSetsConfiguration.Object);

            var mockFtpConnections = new Mock<IFtpConfiguration>();
            var ftpConnections = new Dictionary<string, FtpConnectionConfiguration>
            {
                {
                    UpdaterJobType.RedBook.Humanize(),
                    new FtpConnectionConfiguration()
                        {
                            Host = string.Empty,
                            Username = string.Empty,
                            Password = string.Empty,
                            DefaultRemoteDirectory = "/",
                        }
                },
            };

            mockFtpConnections.Setup(m => m.Connections).Returns(() => ftpConnections);
            services.AddSingleton(mockFtpConnections.Object);

            services.AddSingleton<IDelimiterSeparatedValuesService, DelimiterSeparatedValuesService>();
            services.AddSingleton<IDelimiterSeparatedValuesFileProvider, DelimiterSeparatedValuesFileProvider>();
            services.AddSingleton<IFileSystem, FileSystem>();
            services.AddSingleton<IThirdPartyDataSetsDbObjectFactory, ThirdPartyDataSetsDbObjectFactory>();
            services.AddSingleton<IRedBookRepository, RedBookRepository>();

            this.thirdPartyDataSetsTestFixture = thirdPartyDataSetsTestFixture;
            this.serviceCollection = services;
        }

        [Fact(Skip = "VerySlowRequiresFullSqlFeature")]
        [TestPriority(1)]
        [Trait("TestCategory", TestCategory.VerySlowRequiresFullSqlFeature)]
        public async Task Handle_ReturnsDownloadLocationFolder_WhenFileIsDownloadedViaFTP()
        {
            //// Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<ICommandHandler<DownloadFilesCommand, IReadOnlyList<(string fileName, string fileHash)>>>();
            var updaterJobManifest = service.GetService<UpdaterJobManifest>();

            //// Act
            var result = await sut.Handle(new DownloadFilesCommand(updaterJobManifest, this.thirdPartyDataSetsTestFixture.JobId, UpdaterJobType.RedBook), CancellationToken.None);

            //// Assert

            result.Should().NotBeNull();
        }

        [Fact(Skip = "VerySlowRequiresFullSqlFeature")]
        [TestPriority(2)]
        [Trait("TestCategory", TestCategory.VerySlowRequiresFullSqlFeature)]
        public async Task Handle_ReturnsExtractedLocationFolder_WhenFileIsExtracted()
        {
            //////////// Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<ICommandHandler<ExtractArchivesCommand, string>>();
            var updaterJobManifest = service.GetService<UpdaterJobManifest>();

            /////////// Act
            var result = await sut.Handle(new ExtractArchivesCommand(updaterJobManifest, this.thirdPartyDataSetsTestFixture.JobId), CancellationToken.None);

            /////////// Assert
            result.Should().NotBeNull();
            result.Should().EndWith($@"{this.thirdPartyDataSetsTestFixture.JobId.ToString()}\Extracted");
        }

        [Fact(Skip = "VerySlowRequiresFullSqlFeature")]
        [TestPriority(3)]
        [Trait("TestCategory", TestCategory.VerySlowRequiresFullSqlFeature)]
        public async Task Handle_ReturnsUnitValue_WhenTablesAndSchemasAreCreated()
        {
            //////////// Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<ICommandHandler<CreateTablesAndSchemaCommand, Unit>>();
            var updaterJobManifest = service.GetService<UpdaterJobManifest>();

            var thirdPartyDataSetsDbContext = service.GetService<ThirdPartyDataSetsDbContext>();
            var thirdPartyDataSetsDbContextInitializer = new MigrateDatabaseToLatestVersion<ThirdPartyDataSetsDbContext, ThirdPartyDataSetsDbContextConfiguration>(true);
            thirdPartyDataSetsDbContextInitializer.InitializeDatabase(thirdPartyDataSetsDbContext);

            var redBookRepository = service.GetService<IRedBookRepository>();
            await redBookRepository.DropAllTablesByIndex("01");

            /////////// Act
            var result = await sut.Handle(new CreateTablesAndSchemaCommand(updaterJobManifest, this.thirdPartyDataSetsTestFixture.JobId), CancellationToken.None);

            /////////// Assert
            result.Should().NotBeNull();
            result.Should().Be(Unit.Value);
        }

        [Fact(Skip = "VerySlowRequiresFullSqlFeature")]
        [TestPriority(4)]
        [Trait("TestCategory", TestCategory.VerySlowRequiresFullSqlFeature)]
        public async Task Handle_ReturnsUnitValue_WhenImportingDelimiterSeparatedValues()
        {
            //////////// Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<ICommandHandler<ImportDelimiterSeparatedValuesCommand, Unit>>();
            var updaterJobManifest = service.GetService<UpdaterJobManifest>();

            /////////// Act
            var result = await sut.Handle(new ImportDelimiterSeparatedValuesCommand(updaterJobManifest, this.thirdPartyDataSetsTestFixture.JobId), CancellationToken.None);

            /////////// Assert
            result.Should().NotBeNull();
            result.Should().Be(Unit.Value);
        }

        [Fact(Skip = "VerySlowRequiresFullSqlFeature")]
        [TestPriority(5)]
        [Trait("TestCategory", TestCategory.VerySlowRequiresFullSqlFeature)]
        public async Task Handle_ReturnsListOfArchiveFiles_WhenArchivingDownloadedFiles()
        {
            //////////// Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<ICommandHandler<ArchiveDownloadedFilesCommand, IReadOnlyList<string>>>();
            var updaterJobManifest = service.GetService<UpdaterJobManifest>();

            /////////// Act
            var result = await sut.Handle(new ArchiveDownloadedFilesCommand(updaterJobManifest, this.thirdPartyDataSetsTestFixture.JobId), CancellationToken.None);

            /////////// Assert
            result.Should().NotBeNull();
        }

        [Fact(Skip = "VerySlowRequiresFullSqlFeature")]
        [TestPriority(6)]
        [Trait("TestCategory", TestCategory.VerySlowRequiresFullSqlFeature)]
        public async Task Handle_ReturnsUnit_WhenCleanUpUpdaterJob()
        {
            //////////// Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<ICommandHandler<CleanUpUpdaterJobCommand, Unit>>();
            var updaterJobManifest = service.GetService<UpdaterJobManifest>();

            /////////// Act
            var result = await sut.Handle(new CleanUpUpdaterJobCommand(updaterJobManifest, this.thirdPartyDataSetsTestFixture.JobId), CancellationToken.None);

            /////////// Assert
            result.Should().NotBeNull();
        }

        [Fact(Skip = "VerySlowRequiresFullSqlFeature")]
        [TestPriority(7)]
        [Trait("TestCategory", TestCategory.VerySlowRequiresFullSqlFeature)]
        public async Task Handle_ReturnsListOfVehicleMakes_GetVehicleMakesQuery()
        {
            //////////// Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<IQueryHandler<GetVehicleMakesQuery, IEnumerable<VehicleMake>>>();
            var updaterJobManifest = service.GetService<UpdaterJobManifest>();

            /////////// Act
            var result = await sut.Handle(new GetVehicleMakesQuery(2020), CancellationToken.None);

            /////////// Assert
            result.Should().NotBeNull();
            result.Count().Should().BeGreaterThan(0);
        }
    }
}
