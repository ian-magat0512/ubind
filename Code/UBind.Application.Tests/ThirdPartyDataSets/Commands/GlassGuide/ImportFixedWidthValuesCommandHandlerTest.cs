// <copyright file="ImportFixedWidthValuesCommandHandlerTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.ThirdPartyDataSets.Commands.GlassGuide;

using System;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UBind.Application.Commands.ThirdPartyDataSets.GlassGuide;
using UBind.Application.ThirdPartyDataSets;
using UBind.Application.ThirdPartyDataSets.GlassGuideUpdaterJob;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Repositories;
using UBind.Domain.ThirdPartyDataSets;
using Xunit;

public class ImportFixedWidthValuesCommandHandlerTest : IClassFixture<ThirdPartyDataSetsTestFixture>
{
    private readonly ThirdPartyDataSetsTestFixture thirdPartyDataSetsTestFixture;
    private readonly ServiceCollection serviceCollection;

    public ImportFixedWidthValuesCommandHandlerTest(ThirdPartyDataSetsTestFixture thirdPartyDataSetsTestFixture)
    {
        var services = new ServiceCollection();

        services.AddSingleton<
            ICommandHandler<ImportDelimiterSeparatedValuesCommand, Unit>,
            ImportDelimiterSeparatedValuesCommandHandler>();

        var mockThirdPartyDataSetsConfiguration = new Mock<IThirdPartyDataSetsConfiguration>();
        mockThirdPartyDataSetsConfiguration.Setup(m => m.UpdaterJobsPath).Returns(() => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "unittest"));
        mockThirdPartyDataSetsConfiguration.Setup(m => m.DownloadBufferSize).Returns(() => 4096);
        mockThirdPartyDataSetsConfiguration.Setup(m => m.DownloadedFolder).Returns(() => "Downloaded");
        mockThirdPartyDataSetsConfiguration.Setup(m => m.ExtractedFolder).Returns(() => "Extracted");
        services.AddSingleton(config => mockThirdPartyDataSetsConfiguration.Object);

        var mockGlassGuideRepository = new Mock<IGlassGuideRepository>();
        mockGlassGuideRepository.Setup(repo => repo.CreateCodeDescriptionTable()).Returns(() => new DataTable());
        mockGlassGuideRepository.Setup(repo => repo.CreateRecodeTable()).Returns(() => new DataTable());
        mockGlassGuideRepository.Setup(repo => repo.GetVehiclesDataTable(It.IsAny<string>())).Returns(() => new DataTable());
        services.AddSingleton(mockGlassGuideRepository.Object);

        this.thirdPartyDataSetsTestFixture = thirdPartyDataSetsTestFixture;
        this.serviceCollection = services;
    }

    [Fact]
    public async Task Handle_ReturnsUnitValue_WhenImportingFixedWidthValues()
    {
        //// Arrange
        var service = this.serviceCollection.BuildServiceProvider();
        var sut = service.GetService<ICommandHandler<ImportDelimiterSeparatedValuesCommand, Unit>>();
        var updaterJobManifest = service.GetService<UpdaterJobManifest>();

        Assert.NotNull(sut);

        //// Act
        var result = await sut.Handle(
            new ImportDelimiterSeparatedValuesCommand(
                this.thirdPartyDataSetsTestFixture.JobId),
            CancellationToken.None);

        //// Assert
        result.Should().NotBeNull();
        result.Should().Be(Unit.Value);
    }

    [Fact]
    public void Handle_VehicleValidatedValueShouldNotBeNull()
    {
        //// Arrange
        var dataImport = new DataImportVehicle(
            new DataTable(),
            string.Empty,
            new DataTable(),
            new DataTable(),
            new DataTable(),
            new DataTable(),
            new DataTable());

        //// Assert
        string? result = dataImport.ValidateValue(
            It.IsAny<DataComponentType>(),
            new DataSegment(string.Empty),
            -1,
            0,
            new string[] { string.Empty });
        result.Should().NotBeNull();
    }
}