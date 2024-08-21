// <copyright file="ExtractArchivesCommandHandlerTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.ThirdPartyDataSets.Commands
{
    using System;
    using System.IO;
    using System.IO.Abstractions;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using UBind.Application.Commands.ThirdPartyDataSets;
    using UBind.Application.ThirdPartyDataSets;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories.FileSystem;
    using UBind.Domain.ThirdPartyDataSets;
    using UBind.Persistence.Services.Filesystem;
    using Xunit;

    public class ExtractArchivesCommandHandlerTest : IClassFixture<ThirdPartyDataSetsTestFixture>
    {
        private readonly ThirdPartyDataSetsTestFixture thirdPartyDataSetsTestFixture;
        private readonly ServiceCollection serviceCollection;

        public ExtractArchivesCommandHandlerTest(ThirdPartyDataSetsTestFixture thirdPartyDataSetsTestFixture)
        {
            var services = new ServiceCollection();

            services.AddSingleton<ICommandHandler<ExtractArchivesCommand, string>, ExtractArchivesCommandHandler>();
            var mockThirdPartyDataSetsConfiguration = new Mock<IThirdPartyDataSetsConfiguration>();
            mockThirdPartyDataSetsConfiguration.Setup(m => m.UpdaterJobsPath).Returns(() =>
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"unittest-{Path.GetRandomFileName()}"));
            mockThirdPartyDataSetsConfiguration.Setup(m => m.DownloadBufferSize).Returns(() => 4096);
            mockThirdPartyDataSetsConfiguration.Setup(m => m.DownloadedFolder).Returns(() => "Downloaded");
            mockThirdPartyDataSetsConfiguration.Setup(m => m.ExtractedFolder).Returns(() => "Extracted");
            services.AddSingleton(config => mockThirdPartyDataSetsConfiguration.Object);

            this.thirdPartyDataSetsTestFixture = thirdPartyDataSetsTestFixture;
            this.serviceCollection = services;
        }

        [Fact]
        public async Task Handle_ReturnsExtractedLocationFolder_WhenFileIsExtracted()
        {
            //// Arrange
            var filesystemProvider = new Mock<IFileSystem>();
            filesystemProvider.Setup(setup => setup.Directory.GetFiles(It.IsAny<string>(), It.IsAny<string>())).Returns(
                new[]
                {
                    "c://foo1.zip",
                    "c://foo2.zip",
                });

            this.serviceCollection.AddSingleton(filesystemProvider.Object);

            this.serviceCollection.AddSingleton<IFileSystemService, FileService>();

            var mockFilesystemFileCompressionService = new Mock<IFileSystemFileCompressionService>();
            mockFilesystemFileCompressionService.Setup(setup =>
                setup.ExtractToDirectory(It.IsAny<string>(), It.IsAny<string>()));
            this.serviceCollection.AddSingleton(mockFilesystemFileCompressionService.Object);

            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<ICommandHandler<ExtractArchivesCommand, string>>();
            var updaterJobManifest = service.GetService<UpdaterJobManifest>();

            //// Act
            var result = await sut.Handle(
                new ExtractArchivesCommand(updaterJobManifest, this.thirdPartyDataSetsTestFixture.JobId),
                CancellationToken.None);

            //// Assert
            result.Should().NotBeNull();
            result.Should().EndWith($@"{this.thirdPartyDataSetsTestFixture.JobId.ToString()}\Extracted");
        }

        [Fact]
        public async Task Handle_ThrowUnauthorizedAccessException_WhenFileIsExtractedToNonPermittedFolder()
        {
            //// Arrange
            var fileSystem = new Mock<IFileSystem>();
            fileSystem.Setup(setup => setup.Directory.GetFiles(It.IsAny<string>(), It.IsAny<string>())).Returns(
                new[]
                {
                    "c://foo1.zip",
                    "c://foo2.zip",
                });

            this.serviceCollection.AddSingleton(fileSystem.Object);

            var mockFilesystemFileCompressionService = new Mock<IFileSystemFileCompressionService>();
            mockFilesystemFileCompressionService.Setup(setup => setup.ExtractToDirectory(It.IsAny<string>(), It.IsAny<string>())).Throws<UnauthorizedAccessException>();
            this.serviceCollection.AddSingleton(mockFilesystemFileCompressionService.Object);

            this.serviceCollection.AddSingleton<IFileSystemService, FileService>();

            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<ICommandHandler<ExtractArchivesCommand, string>>();
            var updaterJobManifest = service.GetService<UpdaterJobManifest>();

            //// Act
            Func<Task> action = async () =>
                await sut.Handle(
                    new ExtractArchivesCommand(
                        updaterJobManifest,
                        this.thirdPartyDataSetsTestFixture.JobId), CancellationToken.None);

            //// Assert
            await action.Should().ThrowAsync<UnauthorizedAccessException>();
        }
    }
}
