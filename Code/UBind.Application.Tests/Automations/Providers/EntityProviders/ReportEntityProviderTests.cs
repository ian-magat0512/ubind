// <copyright file="ReportEntityProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.EntityProviders
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Application.Services.Imports;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain;
    using UBind.Domain.Configuration;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Services;
    using Xunit;
    using SerialisedEntitySchemaObject = UBind.Domain.SerialisedEntitySchemaObject;

    public class ReportEntityProviderTests
    {
        [Fact]
        public async Task ReportEntityProvider_Should_Return_reportEntity_When_Pass_With_ReportId()
        {
            // Arrange
            var reportId = Guid.NewGuid();
            var json = @"{ 
                              ""reportId"" : """ + reportId.ToString() + @"""
                         }";

            var reportEntityProviderBuilder = JsonConvert.DeserializeObject<ReportEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockReportRepository = new Mock<IReportReadModelRepository>();

            var model = new FakeReportReadModel(reportId);
            mockReportRepository.Setup(c => c.SingleOrDefaultIncludeAllProperties(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(model);

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IReportReadModelRepository))).Returns(mockReportRepository.Object);
            var reportEntityProvider = reportEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await reportEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.Report));

            var reportEntity = entityObject.DataValue as SerialisedEntitySchemaObject.Report;
            reportEntity.Id.Should().Be(reportId.ToString());
        }

        private Mock<IServiceProvider> GetServiceProvider()
        {
            var mockUrlConfiguration = new Mock<IInternalUrlConfiguration>();
            mockUrlConfiguration.Setup(c => c.BaseApi).Returns("https://localhost:4366/api");

            var mockProductConfig = new DefaultProductConfiguration();
            var mockProductConfigProvider = new Mock<IProductConfigurationProvider>();
            var mockFormDataPrettifier = new Mock<IFormDataPrettifier>();
            var mockCachingResolver = new Mock<ICachingResolver>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockMediator = new Mock<ICqrsMediator>();
            mockServiceProvider.Setup(c => c.GetService(typeof(ISerialisedEntityFactory)))
                .Returns(new SerialisedEntityFactory(
                    mockUrlConfiguration.Object,
                    mockProductConfigProvider.Object,
                    mockFormDataPrettifier.Object,
                    mockCachingResolver.Object,
                    mockMediator.Object,
                    new DefaultPolicyTransactionTimeOfDayScheme()));
            mockServiceProvider.Setup(c => c.GetService(typeof(ICachingResolver)))
                .Returns(mockCachingResolver.Object);

            return mockServiceProvider;
        }
    }
}
