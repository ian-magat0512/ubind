// <copyright file="ClaimVersionEntityProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.EntityProviders
{
    using System;
    using System.Collections.Generic;
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
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.Services;
    using Xunit;
    using SerialisedEntitySchemaObject = UBind.Domain.SerialisedEntitySchemaObject;

    public class ClaimVersionEntityProviderTests
    {
        [Fact]
        public async Task ClaimVersionEntityProvider_Should_Return_ClaimVersionEntity_When_Pass_With_ClaimId_And_VersionNumber()
        {
            // Arrange
            var claimId = Guid.NewGuid();
            var claimVersionNo = 1;
            var expectedclaimVersionNoId = Guid.NewGuid();
            var json = @"{ 
                              ""claimId"" : """ + claimId.ToString() + @""",
                              ""versionNumber"" : " + claimVersionNo + @"
                         }";

            var claimVersionEntityProviderBuilder = JsonConvert.DeserializeObject<ClaimVersionEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockClaimVersionRepository = new Mock<IClaimVersionReadModelRepository>();

            var model = new ClaimVersionReadModelWithRelatedEntities() { ClaimVersion = new ClaimVersionReadModel() { Id = expectedclaimVersionNoId } };
            mockClaimVersionRepository
                .Setup(c => c.GetClaimVersionWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IClaimVersionReadModelRepository))).Returns(mockClaimVersionRepository.Object);
            var claimVersionEntityProvider = claimVersionEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await claimVersionEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.ClaimVersion));

            var claimVersionEntity = entityObject.DataValue as SerialisedEntitySchemaObject.ClaimVersion;
            claimVersionEntity.Id.Should().Be(expectedclaimVersionNoId.ToString());
        }

        [Fact]
        public async Task ClaimVersionEntityProvider_Should_Return_ClaimVersionEntity_When_Pass_With_ClaimReference_And_VersionNumber()
        {
            // Arrange
            var claimReference = "NKHSA";
            var claimVersionNo = 1;
            var expectedclaimVersionNoId = Guid.NewGuid();

            var json = @"{ 
                              ""claimReference"" : """ + claimReference.ToString() + @""",
                              ""versionNumber"" : """ + claimVersionNo + @"""
                         }";

            var claimVersionEntityProviderBuilder = JsonConvert.DeserializeObject<ClaimVersionEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockClaimVersionRepository = new Mock<IClaimVersionReadModelRepository>();

            var model = new ClaimVersionReadModelWithRelatedEntities() { ClaimVersion = new ClaimVersionReadModel() { Id = expectedclaimVersionNoId } };
            mockClaimVersionRepository
                .Setup(c => c.GetClaimVersionWithRelatedEntitiesByReference(
                    It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<int>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IClaimVersionReadModelRepository))).Returns(mockClaimVersionRepository.Object);
            var claimVersionEntityProvider = claimVersionEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger(environment: DeploymentEnvironment.Production);

            // Act
            var entityObject = (await claimVersionEntityProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.ClaimVersion));

            var claimVersionEntity = entityObject.DataValue as SerialisedEntitySchemaObject.ClaimVersion;
            claimVersionEntity.Id.Should().Be(expectedclaimVersionNoId.ToString());

            // Check if the mock quote repository uses product context environment.
            mockClaimVersionRepository.Verify(
                c => c.GetClaimVersionWithRelatedEntitiesByReference(
                    It.IsAny<Guid>(), claimReference, DeploymentEnvironment.Production, claimVersionNo, It.IsAny<List<string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ClaimVersionEntityProvider_Should_Return_ClaimVersionEntity_When_Pass_With_ClaimReference_And_Environment_And_VersionNumber()
        {
            // Arrange
            var claimReference = "NKHSA";
            var claimVersionNo = 1;
            var expectedclaimVersionNoId = Guid.NewGuid();

            var json = @"{ 
                              ""claimReference"" : """ + claimReference.ToString() + @""",
                              ""versionNumber"" : """ + claimVersionNo + @""",
                              ""environment"" : ""Development""
                         }";

            var claimVersionEntityProviderBuilder = JsonConvert.DeserializeObject<ClaimVersionEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockClaimVersionRepository = new Mock<IClaimVersionReadModelRepository>();

            var model = new ClaimVersionReadModelWithRelatedEntities() { ClaimVersion = new ClaimVersionReadModel() { Id = expectedclaimVersionNoId } };
            mockClaimVersionRepository
                .Setup(c => c.GetClaimVersionWithRelatedEntitiesByReference(
                    It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<int>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IClaimVersionReadModelRepository))).Returns(mockClaimVersionRepository.Object);
            var claimVersionEntityProvider = claimVersionEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await claimVersionEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.ClaimVersion));

            var claimVersionEntity = entityObject.DataValue as SerialisedEntitySchemaObject.ClaimVersion;
            claimVersionEntity.Id.Should().Be(expectedclaimVersionNoId.ToString());

            // Check if the mock quote repository uses the passed environment.
            mockClaimVersionRepository.Verify(
                c => c.GetClaimVersionWithRelatedEntitiesByReference(
                    It.IsAny<Guid>(), claimReference, DeploymentEnvironment.Development, claimVersionNo, It.IsAny<List<string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ClaimVersionEntityProvider_Should_Return_ClaimVersionEntity_When_Pass_With_ClaimNumber_And_VersionNumber()
        {
            // Arrange
            var claimNumber = "C-0000";
            var claimVersionNo = 1;
            var expectedclaimVersionNoId = Guid.NewGuid();

            var json = @"{ 
                              ""claimNumber"" : """ + claimNumber.ToString() + @""",
                              ""versionNumber"" : """ + claimVersionNo + @"""
                         }";

            var claimVersionEntityProviderBuilder = JsonConvert.DeserializeObject<ClaimVersionEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockClaimVersionRepository = new Mock<IClaimVersionReadModelRepository>();

            var model = new ClaimVersionReadModelWithRelatedEntities() { ClaimVersion = new ClaimVersionReadModel() { Id = expectedclaimVersionNoId } };
            mockClaimVersionRepository
                .Setup(c => c.GetClaimVersionWithRelatedEntitiesByNumber(
                    It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<int>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IClaimVersionReadModelRepository))).Returns(mockClaimVersionRepository.Object);
            var claimVersionEntityProvider = claimVersionEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger(environment: DeploymentEnvironment.Production);

            // Act
            var entityObject = (await claimVersionEntityProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.ClaimVersion));

            var claimVersionEntity = entityObject.DataValue as SerialisedEntitySchemaObject.ClaimVersion;
            claimVersionEntity.Id.Should().Be(expectedclaimVersionNoId.ToString());

            // Check if the mock quote repository uses product context environment.
            mockClaimVersionRepository
                .Verify(c => c.GetClaimVersionWithRelatedEntitiesByNumber(It.IsAny<Guid>(), claimNumber, DeploymentEnvironment.Production, claimVersionNo, It.IsAny<List<string>>()), Times.Once);
        }

        [Fact]
        public async Task ClaimVersionEntityProvider_Should_Return_ClaimVersionEntity_When_Pass_With_ClaimNumber_And_Environment_And_VersionNumber()
        {
            // Arrange
            var claimNumber = "C-0000";
            var claimVersionNo = 1;
            var expectedclaimVersionNoId = Guid.NewGuid();

            var json = @"{ 
                              ""claimNumber"" : """ + claimNumber.ToString() + @""",
                              ""versionNumber"" : """ + claimVersionNo + @""",
                              ""environment"" : ""Development""
                         }";

            var claimVersionEntityProviderBuilder = JsonConvert.DeserializeObject<ClaimVersionEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockClaimVersionRepository = new Mock<IClaimVersionReadModelRepository>();

            var model = new ClaimVersionReadModelWithRelatedEntities() { ClaimVersion = new ClaimVersionReadModel() { Id = expectedclaimVersionNoId } };
            mockClaimVersionRepository
                .Setup(c => c.GetClaimVersionWithRelatedEntitiesByNumber(
                    It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<int>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IClaimVersionReadModelRepository))).Returns(mockClaimVersionRepository.Object);
            var claimVersionEntityProvider = claimVersionEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            var entityObject = (await claimVersionEntityProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.ClaimVersion));

            var claimVersionEntity = entityObject.DataValue as SerialisedEntitySchemaObject.ClaimVersion;
            claimVersionEntity.Id.Should().Be(expectedclaimVersionNoId.ToString());

            // Check if the mock quote repository uses the passed environment.
            mockClaimVersionRepository
                .Verify(c => c.GetClaimVersionWithRelatedEntitiesByNumber(It.IsAny<Guid>(), claimNumber, DeploymentEnvironment.Development, claimVersionNo, It.IsAny<List<string>>()), Times.Once);
        }

        [Fact]
        public async Task ClaimVersionEntityProvider_Should_Throw_When_ClaimId_DoesNot_Exists()
        {
            // Arrange
            var claimId = Guid.NewGuid();
            var claimVersionNo = 1;
            var json = @"{ 
                              ""claimId"" : """ + claimId.ToString() + @""",
                              ""versionNumber"" : """ + claimVersionNo + @"""
                         }";

            var claimVersionEntityProviderBuilder = JsonConvert.DeserializeObject<ClaimVersionEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockClaimVersionRepository = new Mock<IClaimVersionReadModelRepository>();

            mockClaimVersionRepository
                .Setup(c => c.GetClaimVersionWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<List<string>>()))
                .Returns(default(IClaimVersionReadModelWithRelatedEntities));

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IClaimVersionReadModelRepository))).Returns(mockClaimVersionRepository.Object);
            var claimVersionEntityProvider = claimVersionEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> func = async () => await claimVersionEntityProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Title.Should().Be("Cannot resolve entity reference");
            exception.Which.Error.AdditionalDetails.Should().Contain("Entity Type: Claim Version");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Claim ID: {claimId}");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Version Number: 1");
        }

        [Fact]
        public async Task ClaimVersionEntityProvider_Should_Throw_When_ClaimReference_DoesNot_Exists()
        {
            // Arrange
            var claimReference = "NKHSA";
            var claimVersionNo = 1;
            var json = @"{ 
                              ""claimReference"" : """ + claimReference.ToString() + @""",
                              ""versionNumber"" : """ + claimVersionNo + @"""
                         }";

            var claimVersionEntityProviderBuilder = JsonConvert.DeserializeObject<ClaimVersionEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockClaimVersionRepository = new Mock<IClaimVersionReadModelRepository>();

            mockClaimVersionRepository.Setup(
                c => c.GetClaimVersionWithRelatedEntitiesByReference(
                    It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<int>(), It.IsAny<List<string>>()))
                .Returns(default(IClaimVersionReadModelWithRelatedEntities));

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IClaimVersionReadModelRepository))).Returns(mockClaimVersionRepository.Object);
            var claimVersionEntityProvider = claimVersionEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> func = async () => await claimVersionEntityProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Title.Should().Be("Cannot resolve entity reference");
            exception.Which.Error.AdditionalDetails.Should().Contain("Entity Type: Claim Version");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Claim Reference: {claimReference}");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Version Number: 1");
        }

        [Fact]
        public async Task ClaimVersionEntityProvider_Should_Throw_When_ClaimNumber_DoesNot_Exists()
        {
            // Arrange
            var claimNumber = "C-0000";
            var claimVersionNo = 1;
            var json = @"{ 
                              ""claimNumber"" : """ + claimNumber.ToString() + @""",
                              ""versionNumber"" : """ + claimVersionNo + @"""
                         }";

            var claimVersionEntityProviderBuilder = JsonConvert.DeserializeObject<ClaimVersionEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockClaimVersionRepository = new Mock<IClaimVersionReadModelRepository>();

            mockClaimVersionRepository
                .Setup(c => c.GetClaimVersionWithRelatedEntitiesByReference(
                    It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<int>(), It.IsAny<List<string>>()))
                .Returns(default(IClaimVersionReadModelWithRelatedEntities));

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IClaimVersionReadModelRepository))).Returns(mockClaimVersionRepository.Object);
            var claimVersionEntityProvider = claimVersionEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> func = async () => await claimVersionEntityProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Title.Should().Be("Cannot resolve entity reference");
            exception.Which.Error.AdditionalDetails.Should().Contain("Entity Type: Claim Version");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Claim Number: {claimNumber}");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Version Number: 1");
        }

        private Mock<IServiceProvider> GetServiceProvider()
        {
            var mockUrlConfiguration = new Mock<IInternalUrlConfiguration>();
            mockUrlConfiguration.Setup(c => c.BaseApi).Returns("https://localhost:4366/api");
            var mockProductConfig = new DefaultProductConfiguration();
            var mockCachingProvider = new Mock<ICachingResolver>();
            var mockProductConfigProvider = new Mock<IProductConfigurationProvider>();
            var mockFormDataPrettifier = new Mock<IFormDataPrettifier>();
            var mockMediator = new Mock<ICqrsMediator>();

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(c => c.GetService(typeof(ISerialisedEntityFactory)))
                .Returns(new SerialisedEntityFactory(
                    mockUrlConfiguration.Object,
                    mockProductConfigProvider.Object,
                    mockFormDataPrettifier.Object,
                    mockCachingProvider.Object,
                    mockMediator.Object,
                    new DefaultPolicyTransactionTimeOfDayScheme()));

            return mockServiceProvider;
        }
    }
}
