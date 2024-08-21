// <copyright file="DownloadFilesCommandHandlerTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.ThirdPartyDataSets.Commands;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentFTP;
using Flurl.Http.Testing;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UBind.Application.Commands.ThirdPartyDataSets;
using UBind.Application.DataDownloader;
using UBind.Application.ThirdPartyDataSets;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Repositories.FileSystem;
using UBind.Domain.Tests;
using UBind.Domain.ThirdPartyDataSets;
using UBind.Persistence.Services.Filesystem;
using Xunit;

public class DownloadFilesCommandHandlerTest : IClassFixture<ThirdPartyDataSetsTestFixture>
{
    private readonly ThirdPartyDataSetsTestFixture thirdPartyDataSetsTestFixture;
    private readonly ServiceCollection serviceCollection;

    public DownloadFilesCommandHandlerTest(ThirdPartyDataSetsTestFixture thirdPartyDataSetsTestFixture)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ICommandHandler<DownloadFilesCommand, IReadOnlyList<(string fileName, string fileHash)>>, DownloadFilesCommandHandler>();
        services.AddSingleton(new Mock<IFtpClient>().Object);

        var mockFilesystemFileCompressionService = new Mock<IFileSystemFileCompressionService>();
        services.AddSingleton(mockFilesystemFileCompressionService.Object);
        services.AddSingleton<IFileSystemService, FileService>();
        services.AddSingleton<IDataDownloaderService, DataDownloaderService>();
        services.AddSingleton<IFtpClientFactory, FtpClientFactory>();
        services.AddSingleton<IFileSystem, FileSystem>();

        var mockThirdPartyDataSetsConfiguration = new Mock<IThirdPartyDataSetsConfiguration>();

        mockThirdPartyDataSetsConfiguration.Setup(m => m.UpdaterJobsPath).Returns(() => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "unittest"));
        mockThirdPartyDataSetsConfiguration.Setup(m => m.FileHashesPath).Returns(() => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "unittest", "filehash"));
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
                    DefaultRemoteDirectory = string.Empty,
                }
            },
        };
        mockFtpConnections.Setup(m => m.Connections).Returns(() => ftpConnections);
        services.AddSingleton(mockFtpConnections.Object);

        var mockFtpGlassConnections = new Mock<IFtpConfiguration>();
        var ftpGlassConnections = new Dictionary<string, FtpConnectionConfiguration>
        {
            {
                UpdaterJobType.GlassGuide.Humanize(),
                new FtpConnectionConfiguration()
                {
                    Host = string.Empty,
                    Username = string.Empty,
                    Password = string.Empty,
                    DefaultRemoteDirectory = string.Empty,
                }
            },
        };
        mockFtpGlassConnections.Setup(m => m.Connections).Returns(() => ftpGlassConnections);
        services.AddSingleton(mockFtpGlassConnections.Object);

        this.thirdPartyDataSetsTestFixture = thirdPartyDataSetsTestFixture;
        this.serviceCollection = services;
    }

    [Fact]
    [Trait("TestCategory", TestCategory.Slow)]
    public async Task Handle_ReturnsDownloadLocationFolder_WhenFileIsDownloadedViaHttp()
    {
        using (var httpTest = new HttpTest())
        {
            //// Arrange

            var downloadUrls = new (string Url, string FileHash, string fileName)[] { (@"http://foo/foo.zip", string.Empty, string.Empty) };
            var updaterJobManifest = new UpdaterJobManifest(DataDownloaderProtocol.Http, downloadUrls, true);

            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<ICommandHandler<DownloadFilesCommand, IReadOnlyList<(string fileName, string fileHash)>>>();
            httpTest.RespondWith("FOO");

            //// Act
            var result = await sut.Handle(new DownloadFilesCommand(updaterJobManifest, this.thirdPartyDataSetsTestFixture.JobId, UpdaterJobType.Gnaf), CancellationToken.None);

            //// Assert
            result.Should().NotBeNull();
            httpTest.ShouldHaveCalled("http://foo/foo.zip");
            result.FirstOrDefault().fileName.Should().Contain($@"{this.thirdPartyDataSetsTestFixture.JobId.ToString()}\Downloaded");
        }
    }

    [Fact]
    public async Task Handle_ThrowsHttpException_WhenServerReturns500ErrorResponse()
    {
        using (var httpTest = new HttpTest())
        {
            //// Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<ICommandHandler<DownloadFilesCommand, IReadOnlyList<(string fileName, string fileHash)>>>();

            var downloadUrls = new (string Url, string FileHash, string fileName)[] { (@"http://foo/foo.zip", string.Empty, string.Empty) };
            var updaterJobManifest = new UpdaterJobManifest(DataDownloaderProtocol.Http, downloadUrls, false);
            httpTest.RespondWith("server error", 500);

            //// Act
            Func<Task> action = async () =>
                await sut.Handle(new DownloadFilesCommand(updaterJobManifest, this.thirdPartyDataSetsTestFixture.JobId, UpdaterJobType.Gnaf), CancellationToken.None);

            //// Assert
            await action.Should().ThrowAsync<Flurl.Http.FlurlHttpException>().WithMessage("Call failed with status code 500 (Internal Server Error): GET http://foo/foo.zip");
            httpTest.ShouldHaveCalled("http://foo/foo.zip");
        }
    }
}