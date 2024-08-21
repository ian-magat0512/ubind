// <copyright file="LocalIntegrationTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.ThirdPartyDataSets.Gnaf
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Humanizer;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using UBind.Application.Commands.ThirdPartyDataSets;
    using UBind.Application.Commands.ThirdPartyDataSets.Gnaf;
    using UBind.Application.DataDownloader;
    using UBind.Application.Queries.ThirdPartyDataSets.Gnaf;
    using UBind.Application.Services.DelimiterSeparatedValues;
    using UBind.Application.Tests.Helpers;
    using UBind.Application.ThirdPartyDataSets;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;
    using UBind.Domain.Repositories.FileSystem;
    using UBind.Domain.Search.ThirdPartyDataSets;
    using UBind.Domain.Tests;
    using UBind.Domain.ThirdPartyDataSets;
    using UBind.Domain.ThirdPartyDataSets.Gnaf;
    using UBind.Persistence.Search.ThirdPartyDataSets;
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
            services.AddSingleton<ICommandHandler<CreateTablesAndSchemaCommand, Unit>, CreateTablesAndSchemaCommandHandler>();

            services.AddSingleton<ICommandHandler<DownloadFilesCommand, IReadOnlyList<(string fileName, string fileHash)>>, DownloadFilesCommandHandler>();
            services.AddSingleton<ICommandHandler<ExtractArchivesCommand, string>, ExtractArchivesCommandHandler>();
            services.AddSingleton<ICommandHandler<CreateTablesAndSchemaCommand, Unit>, CreateTablesAndSchemaCommandHandler>();
            services.AddSingleton<ICommandHandler<ImportDelimiterSeparatedValuesCommand, Unit>, ImportDelimiterSeparatedValuesCommandHandler>();
            services.AddSingleton<ICommandHandler<CreateForeignKeysAndIndexesCommand, Unit>, CreateForeignKeysAndIndexesCommandHandler>();
            services.AddSingleton<ICommandHandler<BuildAddressSearchIndexCommand, Unit>, BuildAddressSearchIndexCommandHandler>();
            services.AddSingleton<IQueryHandler<AddressSearchByAddressIdQuery, MaterializedAddressView>, AddressSearchByAddressIdQueryHandler>();

            services.AddSingleton<IThirdPartyDataSetsSearchService, ThirdPartyDataSetsSearchService>();
            services.AddSingleton<IFileSystemService, FileService>();
            services.AddSingleton<IFileSystemFileCompressionService, FilesystemFileCompressionService>();
            services.AddSingleton<IDataDownloaderService, DataDownloaderService>();
            services.AddSingleton<IFtpClientFactory, FtpClientFactory>();

            //// var downloadUrls = new (string Url, string FileHash, string filename)[] { (@"https://data.gov.au/data/dataset/19432f89-dc3a-4ef3-b943-5326ef1dbecc/resource/fdce090a-b356-4afe-91bb-c78fbf88082a/download/feb21_gnaf_pipeseparatedvalue_gda2020.zip", string.Empty, string.Empty) };
            var downloadUrls = new (string Url, string FileHash, string filename)[] { (@"http://localhost:8081/feb21_gnaf_pipeseparatedvalue_gda2020.zip", string.Empty, string.Empty) };

            var updaterJobManifest = new UpdaterJobManifest(DataDownloaderProtocol.Http, downloadUrls, false);

            services.AddSingleton(updaterJobManifest);

            var mockThirdPartyDataSetsConfiguration = new Mock<IThirdPartyDataSetsConfiguration>();
            mockThirdPartyDataSetsConfiguration.Setup(m => m.UpdaterJobsPath).Returns(() => @"C:\ThirdPartyDataSets\UpdaterJobs");
            mockThirdPartyDataSetsConfiguration.Setup(m => m.FileHashesPath).Returns(() => @"C:\ThirdPartyDataSets\FileHashes");
            mockThirdPartyDataSetsConfiguration.Setup(m => m.IndexBasePath).Returns(() => @"C:\ThirdPartyDataSets\SearchIndexes");
            mockThirdPartyDataSetsConfiguration.Setup(m => m.IndexTemporaryPath).Returns(() => @"C:\ThirdPartyDataSets\SearchIndexesTemp");
            mockThirdPartyDataSetsConfiguration.Setup(m => m.DownloadBufferSize).Returns(() => 4096);
            mockThirdPartyDataSetsConfiguration.Setup(m => m.DownloadedFolder).Returns(() => "Downloaded");
            mockThirdPartyDataSetsConfiguration.Setup(m => m.ExtractedFolder).Returns(() => "Extracted");
            services.AddSingleton(config => mockThirdPartyDataSetsConfiguration.Object);

            var appSettingsConfig = new ConfigurationBuilder().AddJsonFile("appsettings.localtest.json").Build();
            var connectionString = appSettingsConfig.GetConnectionString("ThirdPartyDataSets");
            services.AddSingleton<IThirdPartyDataSetsDbConfiguration>(config => new ThirdPartyDataSetsDbConfiguration(connectionString));
            services.AddScoped(ctx => new ThirdPartyDataSetsDbContext(connectionString));

            services.AddSingleton<IThirdPartyDataSetsDbObjectFactory, ThirdPartyDataSetsDbObjectFactory>();
            services.AddSingleton<IGnafRepository, GnafRepository>();
            services.AddSingleton<IDelimiterSeparatedValuesService, DelimiterSeparatedValuesService>();
            services.AddSingleton<IDelimiterSeparatedValuesFileProvider, DelimiterSeparatedValuesFileProvider>();
            services.AddSingleton<IFileSystem, FileSystem>();

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

            this.thirdPartyDataSetsTestFixture = thirdPartyDataSetsTestFixture;
            this.serviceCollection = services;
        }

        [Fact(Skip = "VerySlowRequiresFullSqlFeature")]
        [TestPriority(1)]
        [Trait("TestCategory", TestCategory.VerySlowRequiresFullSqlFeature)]
        public async Task Handle_ReturnsDownloadLocationFolder_WhenFileIsDownloaded()
        {
            //////////// Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<ICommandHandler<DownloadFilesCommand, IReadOnlyList<(string fileName, string fileHash)>>>();
            var updaterJobManifest = service.GetService<UpdaterJobManifest>();

            /////////// Act
            var result = await sut.Handle(new DownloadFilesCommand(updaterJobManifest, this.thirdPartyDataSetsTestFixture.JobId, UpdaterJobType.Gnaf), CancellationToken.None);
            /////////// Assert
            result.Should().NotBeNull();
            result.FirstOrDefault().fileName.Should().Contain($@"{this.thirdPartyDataSetsTestFixture.JobId.ToString()}\Downloaded");
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

            var gnafRepository = service.GetService<IGnafRepository>();
            var schema = "Gnaf";
            await gnafRepository.DropAllTablesBySchema(schema);
            await gnafRepository.DropAllTablesBySchema(schema);

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
        public async Task Handle_ReturnsUnitValue_WhenForeignKeysAndIndexesAreCreated()
        {
            //////////// Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<ICommandHandler<CreateForeignKeysAndIndexesCommand, Unit>>();
            var updaterJobManifest = service.GetService<UpdaterJobManifest>();

            /////////// Act
            var result = await sut.Handle(new CreateForeignKeysAndIndexesCommand(updaterJobManifest, this.thirdPartyDataSetsTestFixture.JobId), CancellationToken.None);

            /////////// Assert
            result.Should().NotBeNull();
            result.Should().Be(Unit.Value);
        }

        [Fact(Skip = "VerySlowRequiresFullSqlFeature")]
        [TestPriority(6)]
        [Trait("TestCategory", TestCategory.VerySlowRequiresFullSqlFeature)]
        public async Task Handle_ShouldBuildLuceneIndex_WhenBuildingAddressSearchIndex()
        {
            //// Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<ICommandHandler<BuildAddressSearchIndexCommand, Unit>>();

            //// Act
            var result = await sut.Handle(new BuildAddressSearchIndexCommand(this.thirdPartyDataSetsTestFixture.JobId, 1, 1000000), CancellationToken.None);

            //// Assert
            result.Should().NotBeNull();
        }

        [Fact(Skip = "VerySlowRequiresFullSqlFeature")]
        [TestPriority(7)]
        [Trait("TestCategory", TestCategory.VerySlowRequiresFullSqlFeature)]
        public async Task Handle_ShouldReturnAddress_WhenPerformingAddressSearch()
        {
            //// Arrange
            var addressId = "GAACT714845933";
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<IQueryHandler<AddressSearchByAddressIdQuery, MaterializedAddressView>>();

            //// Act
            var result = await sut.Handle(new AddressSearchByAddressIdQuery(addressId), CancellationToken.None);

            //// Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(addressId);
        }
    }
}
