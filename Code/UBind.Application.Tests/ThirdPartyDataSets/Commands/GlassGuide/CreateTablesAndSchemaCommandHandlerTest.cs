// <copyright file="CreateTablesAndSchemaCommandHandlerTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.ThirdPartyDataSets.Commands.GlassGuide;

using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UBind.Application.Commands.ThirdPartyDataSets.GlassGuide;
using UBind.Application.ThirdPartyDataSets.GlassGuideUpdaterJob;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Repositories;
using UBind.Domain.Repositories.FileSystem;
using UBind.Domain.ThirdPartyDataSets;
using UBind.Persistence.Services.Filesystem;
using Xunit;

public class CreateTablesAndSchemaCommandHandlerTest : IClassFixture<ThirdPartyDataSetsTestFixture>
{
    private readonly ServiceCollection serviceCollection;

    public CreateTablesAndSchemaCommandHandlerTest()
    {
        var services = new ServiceCollection();

        services.AddSingleton<ICommandHandler<CreateTablesAndSchemaCommand, Unit>, CreateTablesAndSchemaCommandHandler>();
        services.AddSingleton<IFileSystemService, FileService>();
        services.AddSingleton<IFileSystemFileCompressionService, FilesystemFileCompressionService>();
        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton(new Mock<IGlassGuideRepository>().Object);

        var mockThirdPartyDataSetsConfiguration = new Mock<IThirdPartyDataSetsConfiguration>();
        mockThirdPartyDataSetsConfiguration.Setup(m => m.UpdaterJobsPath).Returns(() => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "unittest"));
        mockThirdPartyDataSetsConfiguration.Setup(m => m.DownloadBufferSize).Returns(() => 4096);
        mockThirdPartyDataSetsConfiguration.Setup(m => m.DownloadedFolder).Returns(() => "Downloaded");
        mockThirdPartyDataSetsConfiguration.Setup(m => m.ExtractedFolder).Returns(() => "Extracted");
        services.AddSingleton(config => mockThirdPartyDataSetsConfiguration.Object);

        this.serviceCollection = services;
    }

    [Fact]
    public async Task Handle_ReturnsUnitValue_WhenTablesAndSchemasAreCreated()
    {
        //// Arrange
        var service = this.serviceCollection.BuildServiceProvider();
        var sut = service.GetService<ICommandHandler<CreateTablesAndSchemaCommand, Unit>>();

        Assert.NotNull(sut);

        ///// Act
        var result = await sut.Handle(
            new CreateTablesAndSchemaCommand(
                Schema.GlassGuideStaging),
            CancellationToken.None);

        ///// Assert
        result.Should().NotBeNull();
        result.Should().Be(Unit.Value);
    }
}