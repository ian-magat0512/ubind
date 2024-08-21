// <copyright file="ImportDelimiterSeparatedValuesCommandHandlerTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.ThirdPartyDataSets.Commands.Gnaf
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using UBind.Application.Commands.ThirdPartyDataSets.Gnaf;
    using UBind.Application.Services.DelimiterSeparatedValues;
    using UBind.Application.ThirdPartyDataSets;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;
    using UBind.Domain.ThirdPartyDataSets;
    using Xunit;

    public class ImportDelimiterSeparatedValuesCommandHandlerTest : IClassFixture<ThirdPartyDataSetsTestFixture>
    {
        private readonly ThirdPartyDataSetsTestFixture thirdPartyDataSetsTestFixture;
        private readonly ServiceCollection serviceCollection;

        public ImportDelimiterSeparatedValuesCommandHandlerTest(ThirdPartyDataSetsTestFixture thirdPartyDataSetsTestFixture)
        {
            var services = new ServiceCollection();

            services.AddSingleton<ICommandHandler<ImportDelimiterSeparatedValuesCommand, Unit>, ImportDelimiterSeparatedValuesCommandHandler>();
            services.AddSingleton(new Mock<IDelimiterSeparatedValuesService>().Object);

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
        public async Task Handle_ReturnsUnitValue_WhenImportingDelimiterSeparatedValues()
        {
            //// Arrange
            var mockDelimiterSeparatedValuesService = new Mock<IDelimiterSeparatedValuesService>();
            mockDelimiterSeparatedValuesService.Setup(setup => setup
                .GetDsvFilesWithGroup(It.IsAny<DelimiterSeparatedValuesFileTypes>(), It.IsAny<string>(), It.IsAny<Func<string, string>>())).Returns(
                    new List<(string FileName, string GroupName)>()
                    {
                        ("foo", "foo"),
                    });

            mockDelimiterSeparatedValuesService.Setup(setup => setup.ConvertDelimiterSeparatedValuesToDataTable(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DataTable>())).Returns(new DataTable());

            this.serviceCollection.AddSingleton(mockDelimiterSeparatedValuesService.Object);

            var mockGnafRepository = new Mock<IGnafRepository>();

            mockGnafRepository.Setup(setup => setup.GetDataTableWithSchema(
                It.IsAny<string>()));

            this.serviceCollection.AddSingleton(mockGnafRepository.Object);

            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<ICommandHandler<ImportDelimiterSeparatedValuesCommand, Unit>>();
            var updaterJobManifest = service.GetService<UpdaterJobManifest>();

            //// Act
            var result = await sut.Handle(new ImportDelimiterSeparatedValuesCommand(updaterJobManifest, this.thirdPartyDataSetsTestFixture.JobId), CancellationToken.None);

            //// Assert
            result.Should().NotBeNull();
            result.Should().Be(Unit.Value);
        }
    }
}
