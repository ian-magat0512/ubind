﻿// <copyright file="CreateTablesAndSchemaCommandHandlerTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.ThirdPartyDataSets.Commands.RedBook
{
    using System;
    using System.IO;
    using System.IO.Abstractions;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using UBind.Application.Commands.ThirdPartyDataSets.RedBook;
    using UBind.Application.ThirdPartyDataSets;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;
    using UBind.Domain.Repositories.FileSystem;
    using UBind.Domain.ThirdPartyDataSets;
    using UBind.Persistence.Services.Filesystem;
    using Xunit;

    public class CreateTablesAndSchemaCommandHandlerTest : IClassFixture<ThirdPartyDataSetsTestFixture>
    {
        private readonly ThirdPartyDataSetsTestFixture thirdPartyDataSetsTestFixture;
        private readonly ServiceCollection serviceCollection;

        public CreateTablesAndSchemaCommandHandlerTest(ThirdPartyDataSetsTestFixture thirdPartyDataSetsTestFixture)
        {
            var services = new ServiceCollection();

            services.AddSingleton<ICommandHandler<CreateTablesAndSchemaCommand, Unit>, CreateTablesAndSchemaCommandHandler>();
            services.AddSingleton<IFileSystemService, FileService>();
            services.AddSingleton<IFileSystemFileCompressionService, FilesystemFileCompressionService>();
            services.AddSingleton<IFileSystem, FileSystem>();
            services.AddSingleton(new Mock<IRedBookRepository>().Object);

            var mockThirdPartyDataSetsConfiguration = new Mock<IThirdPartyDataSetsConfiguration>();
            mockThirdPartyDataSetsConfiguration.Setup(m => m.UpdaterJobsPath).Returns(() => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "unittest"));
            mockThirdPartyDataSetsConfiguration.Setup(m => m.DownloadBufferSize).Returns(() => 4096);
            mockThirdPartyDataSetsConfiguration.Setup(m => m.DownloadedFolder).Returns(() => "Downloaded");
            mockThirdPartyDataSetsConfiguration.Setup(m => m.ExtractedFolder).Returns(() => "Extracted");
            services.AddSingleton(config => mockThirdPartyDataSetsConfiguration.Object);

            this.thirdPartyDataSetsTestFixture = thirdPartyDataSetsTestFixture;
            this.serviceCollection = services;
        }

        [Fact]
        public async Task Handle_ReturnsUnitValue_WhenTablesAndSchemasAreCreated()
        {
            //// Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<ICommandHandler<CreateTablesAndSchemaCommand, Unit>>();
            var updaterJobManifest = service.GetService<UpdaterJobManifest>();

            ///// Act
            var result = await sut.Handle(
                new CreateTablesAndSchemaCommand(
                    updaterJobManifest,
                    this.thirdPartyDataSetsTestFixture.JobId),
                CancellationToken.None);

            ///// Assert
            result.Should().NotBeNull();
            result.Should().Be(Unit.Value);
        }
    }
}
