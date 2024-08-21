// <copyright file="SynchroniseProductComponentCommandTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Commands.Release;

using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Moq;
using NodaTime;
using UBind.Application.Automation;
using UBind.Application.Commands.Release;
using UBind.Application.FlexCel;
using UBind.Application.Releases;
using UBind.Domain;
using UBind.Domain.Extensions;
using UBind.Domain.Product;
using UBind.Domain.Repositories;
using UBind.Domain.Repositories.Redis;
using UBind.Domain.Tests.Fakes;
using Xunit;
using UBindFileInfo = UBind.Domain.Models.FileInfo;

public class SynchroniseProductComponentCommandTests
{
    private readonly Mock<IDevReleaseRepository> devReleaseRepositoryMock;
    private readonly Mock<IFilesystemFileRepository> fileRepositoryMock;
    private readonly Mock<IFilesystemStorageConfiguration> filesystemStorageConfigurationMock;
    private readonly IFilesystemStoragePathService pathService;
    private readonly Mock<IFormConfigurationGenerator> spreadsheetFormConfigurationReaderMock;
    private readonly Mock<IBackgroundJobClient> jobClient;
    private readonly Mock<ISpreadsheetPoolService> spreadsheetPoolServiceMock;
    private readonly Mock<IReleaseValidator> releaseValidatorMock;
    private readonly Mock<IGlobalReleaseCache> globalReleaseCacheMock;
    private readonly Mock<ICachingResolver> cachingResolverMock;
    private readonly IClock clock;
    private readonly Mock<IFileContentRepository> fileContentRepository;
    private readonly Mock<IFilesystemStorageConfiguration> filesystemStorageConfiguration;
    private readonly Mock<IProductFeatureSettingRepository> productFeatureSettingRepositoryMock;
    private readonly Mock<IAutomationPeriodicTriggerScheduler> periodicTriggerSchedulerMock;
    private readonly Mock<IProductReleaseSynchroniseRepository> productReleaseSync;

    private readonly string tenantAlias = "test-tenant";
    private readonly string productAlias = "test-product";
    private byte[] testBinaryWorkBook;

    public SynchroniseProductComponentCommandTests()
    {
        this.devReleaseRepositoryMock = new Mock<IDevReleaseRepository>();
        this.fileRepositoryMock = new Mock<IFilesystemFileRepository>();

        this.filesystemStorageConfigurationMock = new Mock<IFilesystemStorageConfiguration>();
        this.filesystemStorageConfigurationMock.Setup(s => s.UBindFolderName).Returns("uBind");

        this.pathService = new FilesystemStoragePathService(this.filesystemStorageConfigurationMock.Object);
        this.spreadsheetFormConfigurationReaderMock = new Mock<IFormConfigurationGenerator>();
        this.jobClient = new Mock<IBackgroundJobClient>();
        this.spreadsheetPoolServiceMock = new Mock<ISpreadsheetPoolService>();
        this.releaseValidatorMock = new Mock<IReleaseValidator>();
        this.globalReleaseCacheMock = new Mock<IGlobalReleaseCache>();

        this.cachingResolverMock = new Mock<ICachingResolver>();
        this.cachingResolverMock.Setup(s => s.GetTenantAliasOrThrowAsync(It.IsAny<Guid>())).ReturnsAsync(this.tenantAlias);
        this.cachingResolverMock.Setup(s => s.GetProductAliasOrThrowAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(this.productAlias);

        this.productFeatureSettingRepositoryMock = new Mock<IProductFeatureSettingRepository>();
        this.fileContentRepository = new Mock<IFileContentRepository>();
        this.filesystemStorageConfiguration = new Mock<IFilesystemStorageConfiguration>();
        this.clock = new TestClock();
        this.periodicTriggerSchedulerMock = new Mock<IAutomationPeriodicTriggerScheduler>();
        this.productReleaseSync = new Mock<IProductReleaseSynchroniseRepository>();

        var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().Location);
        var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
        var dirPath = Path.GetDirectoryName(codeBasePath);
        var filePath = Path.Combine(dirPath, "Commands\\Release\\testWorkbook.xlsx");
        this.testBinaryWorkBook = System.IO.File.ReadAllBytes(filePath);
    }

    [Fact]
    public async Task Handle_CreatesNewDevRelease_WhenOneDoesNotExist()
    {
        // Arrange
        var command = new SynchroniseProductComponentCommand(Guid.NewGuid(), Guid.NewGuid(), WebFormAppType.Quote);
        var sut = new SynchroniseProductComponentCommandHandler(
            this.devReleaseRepositoryMock.Object,
            this.fileRepositoryMock.Object,
            this.pathService,
            this.spreadsheetFormConfigurationReaderMock.Object,
            this.releaseValidatorMock.Object,
            this.globalReleaseCacheMock.Object,
            this.cachingResolverMock.Object,
            this.clock,
            this.periodicTriggerSchedulerMock.Object,
            this.spreadsheetPoolServiceMock.Object,
            this.productReleaseSync.Object,
            this.fileContentRepository.Object);
        this.fileRepositoryMock.Setup(s => s.GetFileInfo(
            this.pathService.GetDevWorkbookPath(this.tenantAlias, this.productAlias, WebFormAppType.Quote),
            It.IsAny<string>(),
            It.IsAny<TimeSpan>()))
                .ReturnsAsync(new UBindFileInfo
                {
                    Path = "asdf",
                    CreatedTimestamp = this.clock.Now(),
                    LastModifiedTimestamp = this.clock.Now(),
                });
        this.fileRepositoryMock.Setup(s => s.GetFileContents(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<TimeSpan>()))
                .ReturnsAsync(this.testBinaryWorkBook);
        this.fileRepositoryMock.Setup(s => s.FolderExists(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync(true);
        this.fileRepositoryMock.Setup(s => s.FileExists(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync(true);
        this.spreadsheetFormConfigurationReaderMock.Setup(s => s.Generate(
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            It.IsAny<WebFormAppType>(),
            It.IsAny<FlexCelWorkbook>(),
            It.IsAny<string>(),
            It.IsAny<string?>(),
            It.IsAny<string?>()))
                .ReturnsAsync(@"{ ""some"": ""json"" }");

        // Act
        var devRelease = await sut.Handle(command, CancellationToken.None);

        // Assert
        devRelease.Should().NotBeNull();
        devRelease.QuoteDetails.Should().NotBeNull();
        devRelease.QuoteDetails.ConfigurationJson.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_UpdatesFormConfigurationJson_WhenWorkbookChanges()
    {
        // Arrange
        var firstTimestamp = this.clock.Now();
        var secondTimestamp = firstTimestamp.PlusTicks(100000);
        var command = new SynchroniseProductComponentCommand(Guid.NewGuid(), Guid.NewGuid(), WebFormAppType.Quote);
        var sut = new SynchroniseProductComponentCommandHandler(
            this.devReleaseRepositoryMock.Object,
            this.fileRepositoryMock.Object,
            this.pathService,
            this.spreadsheetFormConfigurationReaderMock.Object,
            this.releaseValidatorMock.Object,
            this.globalReleaseCacheMock.Object,
            this.cachingResolverMock.Object,
            this.clock,
            this.periodicTriggerSchedulerMock.Object,
            this.spreadsheetPoolServiceMock.Object,
            this.productReleaseSync.Object,
            this.fileContentRepository.Object);
        this.fileRepositoryMock.Setup(s => s.GetFileInfo(
            this.pathService.GetDevWorkbookPath(this.tenantAlias, this.productAlias, WebFormAppType.Quote),
            It.IsAny<string>(),
            It.IsAny<TimeSpan>()))
                .ReturnsAsync(new UBindFileInfo
                {
                    Path = "asdf",
                    CreatedTimestamp = firstTimestamp,
                    LastModifiedTimestamp = firstTimestamp,
                });
        this.fileRepositoryMock.Setup(s => s.GetFileContents(
            this.pathService.GetDevWorkbookPath(this.tenantAlias, this.productAlias, WebFormAppType.Quote),
            It.IsAny<string>(),
            It.IsAny<TimeSpan>()))
                .ReturnsAsync(this.testBinaryWorkBook);
        this.fileRepositoryMock.Setup(s => s.GetFileInfo(
            this.pathService.GetDevWorkflowFilePath(this.tenantAlias, this.productAlias, WebFormAppType.Quote),
            It.IsAny<string>(),
            It.IsAny<TimeSpan>()))
                .ReturnsAsync(new UBindFileInfo
                {
                    Path = "workflow.json",
                    CreatedTimestamp = firstTimestamp,
                    LastModifiedTimestamp = firstTimestamp,
                });
        this.fileRepositoryMock.Setup(s => s.GetFileStringContents(
            this.pathService.GetDevWorkflowFilePath(this.tenantAlias, this.productAlias, WebFormAppType.Quote),
            It.IsAny<string>(),
            It.IsAny<TimeSpan>()))
                .ReturnsAsync("{}");
        this.fileRepositoryMock.Setup(s => s.FolderExists(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync(true);
        this.fileRepositoryMock.Setup(s => s.FileExists(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync(true);
        this.spreadsheetFormConfigurationReaderMock.Setup(s => s.Generate(
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            It.IsAny<WebFormAppType>(),
            It.IsAny<FlexCelWorkbook>(),
            It.IsAny<string>(),
            It.IsAny<string?>(),
            It.IsAny<string?>()))
                .ReturnsAsync(@"{ ""first"": ""json"" }");
        var devRelease = await sut.Handle(command, CancellationToken.None);
        this.devReleaseRepositoryMock.Setup(s => s.GetDevReleaseForProductWithoutAssetFileContents(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(devRelease);
        this.fileRepositoryMock.Setup(s => s.GetFileInfo(
            this.pathService.GetDevWorkbookPath(this.tenantAlias, this.productAlias, WebFormAppType.Quote),
            It.IsAny<string>(),
            It.IsAny<TimeSpan>()))
                .ReturnsAsync(new UBindFileInfo
                {
                    Path = "asdf",
                    CreatedTimestamp = secondTimestamp,
                    LastModifiedTimestamp = secondTimestamp,
                });
        this.spreadsheetFormConfigurationReaderMock.Setup(s => s.Generate(
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            It.IsAny<WebFormAppType>(),
            It.IsAny<FlexCelWorkbook>(),
            It.IsAny<string>(),
            It.IsAny<string?>(),
            It.IsAny<string?>()))
                .ReturnsAsync(@"{ ""second"": ""json"" }");

        // Act
        devRelease = await sut.Handle(command, CancellationToken.None);

        // Assert
        devRelease.QuoteDetails.FormConfigurationJsonLastModifiedTimestamp.Should().Be(secondTimestamp);
        devRelease.QuoteDetails.ConfigurationJson.Should().Be(@"{ ""second"": ""json"" }");
    }

    [Fact]
    public async Task Handle_DoesNotUpdateFormConfigurationJson_WhenDependenciesDoNotChange()
    {
        // Arrange
        var firstTimestamp = this.clock.Now();
        var secondTimestamp = firstTimestamp.PlusTicks(100000);
        var command = new SynchroniseProductComponentCommand(Guid.NewGuid(), Guid.NewGuid(), WebFormAppType.Quote);
        var sut = new SynchroniseProductComponentCommandHandler(
            this.devReleaseRepositoryMock.Object,
            this.fileRepositoryMock.Object,
            this.pathService,
            this.spreadsheetFormConfigurationReaderMock.Object,
            this.releaseValidatorMock.Object,
            this.globalReleaseCacheMock.Object,
            this.cachingResolverMock.Object,
            this.clock,
            this.periodicTriggerSchedulerMock.Object,
            this.spreadsheetPoolServiceMock.Object,
            this.productReleaseSync.Object,
            this.fileContentRepository.Object);
        this.fileRepositoryMock.Setup(s => s.GetFileInfo(
            this.pathService.GetDevWorkbookPath(this.tenantAlias, this.productAlias, WebFormAppType.Quote),
            It.IsAny<string>(),
            It.IsAny<TimeSpan>()))
                .ReturnsAsync(new UBindFileInfo
                {
                    Path = "asdf",
                    CreatedTimestamp = firstTimestamp,
                    LastModifiedTimestamp = firstTimestamp,
                });
        this.fileRepositoryMock.Setup(s => s.GetFileContents(
            this.pathService.GetDevWorkbookPath(this.tenantAlias, this.productAlias, WebFormAppType.Quote),
            It.IsAny<string>(),
            It.IsAny<TimeSpan>()))
                .ReturnsAsync(this.testBinaryWorkBook);
        this.fileRepositoryMock.Setup(s => s.GetFileInfo(
            this.pathService.GetDevWorkflowFilePath(this.tenantAlias, this.productAlias, WebFormAppType.Quote),
            It.IsAny<string>(),
            It.IsAny<TimeSpan>()))
                .ReturnsAsync(new UBindFileInfo
                {
                    Path = "workflow.json",
                    CreatedTimestamp = firstTimestamp,
                    LastModifiedTimestamp = firstTimestamp,
                });
        this.fileRepositoryMock.Setup(s => s.GetFileStringContents(
            this.pathService.GetDevWorkflowFilePath(this.tenantAlias, this.productAlias, WebFormAppType.Quote),
            It.IsAny<string>(),
            It.IsAny<TimeSpan>()))
                .ReturnsAsync("{}");
        this.fileRepositoryMock.Setup(s => s.FolderExists(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync(true);
        this.fileRepositoryMock.Setup(s => s.FileExists(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync(true);
        this.spreadsheetFormConfigurationReaderMock.Setup(s => s.Generate(
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            It.IsAny<WebFormAppType>(),
            It.IsAny<FlexCelWorkbook>(),
            It.IsAny<string>(),
            It.IsAny<string?>(),
            It.IsAny<string?>()))
                .ReturnsAsync(@"{ ""first"": ""json"" }");
        var devRelease = await sut.Handle(command, CancellationToken.None);
        this.devReleaseRepositoryMock.Setup(s => s.GetDevReleaseForProductWithoutAssetFileContents(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(devRelease);
        this.fileRepositoryMock.Setup(s => s.GetFileInfo(
            this.pathService.GetDevWorkbookPath(this.tenantAlias, this.productAlias, WebFormAppType.Quote),
            It.IsAny<string>(),
            It.IsAny<TimeSpan>()))
                .ReturnsAsync(new UBindFileInfo
                {
                    Path = "asdf",
                    CreatedTimestamp = firstTimestamp,
                    LastModifiedTimestamp = firstTimestamp,
                });
        this.spreadsheetFormConfigurationReaderMock.Setup(s => s.Generate(
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            It.IsAny<WebFormAppType>(),
            It.IsAny<FlexCelWorkbook>(),
            It.IsAny<string>(),
            It.IsAny<string?>(),
            It.IsAny<string?>()))
                .ReturnsAsync(@"{ ""second"": ""json"" }");

        // Act
        devRelease = await sut.Handle(command, CancellationToken.None);

        // Assert
        devRelease.QuoteDetails.FormConfigurationJsonLastModifiedTimestamp.Should().Be(firstTimestamp);
        devRelease.QuoteDetails.ConfigurationJson.Should().Be(@"{ ""first"": ""json"" }");
    }

    [Fact]
    public async Task Handle_UpdatesWorkflowJson_WhenFileChanges()
    {
        // Arrange
        var workflowJsonPath = Path.Combine(
            this.pathService.GetProductDevelopmentAppFolder(this.tenantAlias, this.productAlias, WebFormAppType.Quote),
            this.pathService.WorkflowFileName);
        var firstTimestamp = this.clock.Now();
        var secondTimestamp = firstTimestamp.PlusTicks(100000);
        var command = new SynchroniseProductComponentCommand(Guid.NewGuid(), Guid.NewGuid(), WebFormAppType.Quote);
        var sut = new SynchroniseProductComponentCommandHandler(
            this.devReleaseRepositoryMock.Object,
            this.fileRepositoryMock.Object,
            this.pathService,
            this.spreadsheetFormConfigurationReaderMock.Object,
            this.releaseValidatorMock.Object,
            this.globalReleaseCacheMock.Object,
            this.cachingResolverMock.Object,
            this.clock,
            this.periodicTriggerSchedulerMock.Object,
            this.spreadsheetPoolServiceMock.Object,
            this.productReleaseSync.Object,
            this.fileContentRepository.Object);
        this.fileRepositoryMock.Setup(s => s.GetFileInfo(
            workflowJsonPath,
            It.IsAny<string>(),
            It.IsAny<TimeSpan>()))
                .ReturnsAsync(new UBindFileInfo
                {
                    Path = "workflow.json",
                    CreatedTimestamp = firstTimestamp,
                    LastModifiedTimestamp = firstTimestamp,
                });
        this.fileRepositoryMock.Setup(s => s.GetFileStringContents(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<TimeSpan>()))
                .ReturnsAsync(@"{ ""first"": ""json"" }");
        this.fileRepositoryMock.Setup(s => s.FolderExists(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync(true);
        this.fileRepositoryMock.Setup(s => s.FileExists(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync(true);
        var devRelease = await sut.Handle(command, CancellationToken.None);
        this.devReleaseRepositoryMock.Setup(s => s.GetDevReleaseForProductWithoutAssetFileContents(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(devRelease);
        this.fileRepositoryMock.Setup(s => s.GetFileInfo(
            workflowJsonPath,
            It.IsAny<string>(),
            It.IsAny<TimeSpan>()))
                .ReturnsAsync(new UBindFileInfo
                {
                    Path = "workflow.json",
                    CreatedTimestamp = secondTimestamp,
                    LastModifiedTimestamp = secondTimestamp,
                });
        this.fileRepositoryMock.Setup(s => s.GetFileStringContents(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<TimeSpan>()))
                .ReturnsAsync(@"{ ""second"": ""json"" }");

        // Act
        devRelease = await sut.Handle(command, CancellationToken.None);

        // Assert
        devRelease.QuoteDetails.WorkflowJsonLastModifiedTimestamp.Should().Be(secondTimestamp);
        devRelease.QuoteDetails.WorkflowJson.Should().Be(@"{ ""second"": ""json"" }");
    }

    [Fact]
    public async Task Handle_DoesNotUpdateWorkflowJson_WhenFileDoesNotChange()
    {
        // Arrange
        var workflowJsonPath = Path.Combine(
            this.pathService.GetProductDevelopmentAppFolder(this.tenantAlias, this.productAlias, WebFormAppType.Quote),
            this.pathService.WorkflowFileName);
        var firstTimestamp = this.clock.Now();
        var secondTimestamp = firstTimestamp.PlusTicks(100000);
        var command = new SynchroniseProductComponentCommand(Guid.NewGuid(), Guid.NewGuid(), WebFormAppType.Quote);
        var sut = new SynchroniseProductComponentCommandHandler(
            this.devReleaseRepositoryMock.Object,
            this.fileRepositoryMock.Object,
            this.pathService,
            this.spreadsheetFormConfigurationReaderMock.Object,
            this.releaseValidatorMock.Object,
            this.globalReleaseCacheMock.Object,
            this.cachingResolverMock.Object,
            this.clock,
            this.periodicTriggerSchedulerMock.Object,
            this.spreadsheetPoolServiceMock.Object,
            this.productReleaseSync.Object,
            this.fileContentRepository.Object);
        this.fileRepositoryMock.Setup(s => s.GetFileInfo(
            workflowJsonPath,
            It.IsAny<string>(),
            It.IsAny<TimeSpan>()))
                .ReturnsAsync(new UBindFileInfo
                {
                    Path = "workflow.json",
                    CreatedTimestamp = firstTimestamp,
                    LastModifiedTimestamp = firstTimestamp,
                });
        this.fileRepositoryMock.Setup(s => s.GetFileStringContents(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<TimeSpan>()))
                .ReturnsAsync(@"{ ""first"": ""json"" }");
        this.fileRepositoryMock.Setup(s => s.GetFileContents(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<TimeSpan>()))
                .ReturnsAsync(this.testBinaryWorkBook);
        this.fileRepositoryMock.Setup(s => s.FolderExists(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync(true);
        this.fileRepositoryMock.Setup(s => s.FileExists(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync(true);
        var devRelease = await sut.Handle(command, CancellationToken.None);
        this.devReleaseRepositoryMock.Setup(s => s.GetDevReleaseForProductWithoutAssetFileContents(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(devRelease);
        this.fileRepositoryMock.Setup(s => s.GetFileInfo(
            this.pathService.GetDevWorkbookPath(this.tenantAlias, this.productAlias, WebFormAppType.Quote),
            It.IsAny<string>(),
            It.IsAny<TimeSpan>()))
                .ReturnsAsync(new UBindFileInfo
                {
                    Path = "asdf",
                    CreatedTimestamp = firstTimestamp,
                    LastModifiedTimestamp = firstTimestamp,
                });

        // Act
        devRelease = await sut.Handle(command, CancellationToken.None);

        // Assert
        devRelease.QuoteDetails.WorkflowJsonLastModifiedTimestamp.Should().Be(firstTimestamp);
        devRelease.QuoteDetails.WorkflowJson.Should().Be(@"{ ""first"": ""json"" }");
    }

    [Fact]
    public async Task Handle_AddsNewPublicAsset_WhenNewFileIsFound()
    {
        // Arrange
        var publicAssetsFolder = this.pathService.GetPublicAssetFolder(this.tenantAlias, this.productAlias, WebFormAppType.Quote);
        var testFilename = "test.txt";
        var testFilePath = Path.Combine(publicAssetsFolder, testFilename);
        var testFileContents = Encoding.UTF8.GetBytes(@"insurance is interesting");
        var firstTimestamp = this.clock.Now();
        var command = new SynchroniseProductComponentCommand(Guid.NewGuid(), Guid.NewGuid(), WebFormAppType.Quote);
        var sut = new SynchroniseProductComponentCommandHandler(
            this.devReleaseRepositoryMock.Object,
            this.fileRepositoryMock.Object,
            this.pathService,
            this.spreadsheetFormConfigurationReaderMock.Object,
            this.releaseValidatorMock.Object,
            this.globalReleaseCacheMock.Object,
            this.cachingResolverMock.Object,
            this.clock,
            this.periodicTriggerSchedulerMock.Object,
            this.spreadsheetPoolServiceMock.Object,
            this.productReleaseSync.Object,
            this.fileContentRepository.Object);
        this.fileRepositoryMock.Setup(s => s.ListFilesInfoInFolder(publicAssetsFolder, It.IsAny<string>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync(new UBindFileInfo[]
            {
                new UBindFileInfo { Path = testFilePath, CreatedTimestamp = firstTimestamp, LastModifiedTimestamp = firstTimestamp },
            });
        this.fileRepositoryMock.Setup(s => s.GetFileContents(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<TimeSpan>()))
                .ReturnsAsync(testFileContents);
        this.fileRepositoryMock.Setup(s => s.FolderExists(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync(true);
        this.fileRepositoryMock.Setup(s => s.FileExists(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync(true);

        // Act
        var devRelease = await sut.Handle(command, CancellationToken.None);

        // Assert
        devRelease.QuoteDetails.Assets.Should().ContainSingle(a => a.Name == testFilename);
        devRelease.QuoteDetails.Assets.Single(a => a.Name == testFilename).FileContent.Content.Should().BeEquivalentTo(testFileContents);
    }

    [Fact]
    public async Task Handle_ReplacesPublicAsset_WhenFileIsUpdated()
    {
        // Arrange
        var publicAssetsFolder = this.pathService.GetPublicAssetFolder(this.tenantAlias, this.productAlias, WebFormAppType.Quote);
        var testFilename = "test.txt";
        var testFilePath = Path.Combine(publicAssetsFolder, testFilename);
        var testFileContents = Encoding.UTF8.GetBytes(@"insurance is interesting");
        var updatedFileContents = Encoding.UTF8.GetBytes(@"insurance is boring");
        var firstTimestamp = this.clock.Now();
        var secondTimestamp = firstTimestamp.PlusTicks(100000);
        var command = new SynchroniseProductComponentCommand(Guid.NewGuid(), Guid.NewGuid(), WebFormAppType.Quote);
        var sut = new SynchroniseProductComponentCommandHandler(
            this.devReleaseRepositoryMock.Object,
            this.fileRepositoryMock.Object,
            this.pathService,
            this.spreadsheetFormConfigurationReaderMock.Object,
            this.releaseValidatorMock.Object,
            this.globalReleaseCacheMock.Object,
            this.cachingResolverMock.Object,
            this.clock,
            this.periodicTriggerSchedulerMock.Object,
            this.spreadsheetPoolServiceMock.Object,
            this.productReleaseSync.Object,
            this.fileContentRepository.Object);
        this.fileRepositoryMock.Setup(s => s.ListFilesInfoInFolder(publicAssetsFolder, It.IsAny<string>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync(new UBindFileInfo[]
            {
                new UBindFileInfo { Path = testFilePath, CreatedTimestamp = firstTimestamp, LastModifiedTimestamp = firstTimestamp },
            });
        this.fileRepositoryMock.Setup(s => s.GetFileContents(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<TimeSpan>()))
                .ReturnsAsync(testFileContents);
        this.fileRepositoryMock.Setup(s => s.FolderExists(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync(true);
        this.fileRepositoryMock.Setup(s => s.FileExists(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync(true);
        var devRelease = await sut.Handle(command, CancellationToken.None);
        this.fileRepositoryMock.Setup(s => s.ListFilesInfoInFolder(publicAssetsFolder, It.IsAny<string>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync(new UBindFileInfo[]
            {
                new UBindFileInfo { Path = testFilePath, CreatedTimestamp = secondTimestamp, LastModifiedTimestamp = secondTimestamp },
            });
        this.fileRepositoryMock.Setup(s => s.GetFileContents(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<TimeSpan>()))
                .ReturnsAsync(updatedFileContents);

        // Act
        devRelease = await sut.Handle(command, CancellationToken.None);

        // Assert
        devRelease.QuoteDetails.Assets.Should().ContainSingle(a => a.Name == testFilename);
        devRelease.QuoteDetails.Assets.Single(a => a.Name == testFilename).FileContent.Content.Should().BeEquivalentTo(updatedFileContents);
    }

    [Fact]
    public async Task Handle_RemovesPublicAsset_WhenFileIsNoLongerFound()
    {
        // Arrange
        var publicAssetsFolder = this.pathService.GetPublicAssetFolder(this.tenantAlias, this.productAlias, WebFormAppType.Quote);
        var testFilename = "test.txt";
        var testFilePath = Path.Combine(publicAssetsFolder, testFilename);
        var testFileContents = Encoding.UTF8.GetBytes(@"insurance is interesting");
        var firstTimestamp = this.clock.Now();
        var command = new SynchroniseProductComponentCommand(Guid.NewGuid(), Guid.NewGuid(), WebFormAppType.Quote);
        var sut = new SynchroniseProductComponentCommandHandler(
            this.devReleaseRepositoryMock.Object,
            this.fileRepositoryMock.Object,
            this.pathService,
            this.spreadsheetFormConfigurationReaderMock.Object,
            this.releaseValidatorMock.Object,
            this.globalReleaseCacheMock.Object,
            this.cachingResolverMock.Object,
            this.clock,
            this.periodicTriggerSchedulerMock.Object,
            this.spreadsheetPoolServiceMock.Object,
            this.productReleaseSync.Object,
            this.fileContentRepository.Object);
        this.fileRepositoryMock.Setup(s => s.ListFilesInfoInFolder(publicAssetsFolder, It.IsAny<string>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync(new UBindFileInfo[]
            {
                new UBindFileInfo { Path = testFilePath, CreatedTimestamp = firstTimestamp, LastModifiedTimestamp = firstTimestamp },
            });
        this.fileRepositoryMock.Setup(s => s.GetFileContents(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<TimeSpan>()))
                .ReturnsAsync(testFileContents);
        this.fileRepositoryMock.Setup(s => s.FolderExists(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync(true);
        this.fileRepositoryMock.Setup(s => s.FileExists(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync(true);
        var devRelease = await sut.Handle(command, CancellationToken.None);
        this.fileRepositoryMock.Setup(s => s.ListFilesInfoInFolder(publicAssetsFolder, It.IsAny<string>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync(Array.Empty<UBindFileInfo>());

        // Act
        devRelease = await sut.Handle(command, CancellationToken.None);

        // Assert
        devRelease.QuoteDetails.Assets.Should().NotContain(a => a.Name == testFilename);
    }
}
