// <copyright file="ClaimEntityProviderTests.cs" company="uBind">
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

    public class ClaimEntityProviderTests
    {
        [Fact]
        public async Task ClaimEntityProvider_Should_Return_ClaimEntity_When_Pass_With_ClaimId()
        {
            // Arrange
            var claimId = Guid.NewGuid();
            var json = @"{ 
                              ""claimId"" : """ + claimId.ToString() + @"""
                         }";

            var claimEntityProviderBuilder = JsonConvert.DeserializeObject<ClaimEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockClaimRepository = new Mock<IClaimReadModelRepository>();

            var model = new ClaimReadModelWithRelatedEntities() { Claim = new Mock<ClaimReadModel>(claimId).Object };
            mockClaimRepository
                .Setup(c => c.GetClaimWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IClaimReadModelRepository))).Returns(mockClaimRepository.Object);
            var claimEntityProvider = claimEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await claimEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.Claim));

            var claimEntity = entityObject.DataValue as SerialisedEntitySchemaObject.Claim;
            claimEntity.Id.Should().Be(claimId.ToString());
        }

        [Fact]
        public async Task ClaimEntityProvider_Should_Return_ClaimEntity_When_Pass_With_ClaimReference()
        {
            // Arrange
            var claimReference = "NKHSA";
            var expectedClaimId = Guid.NewGuid();

            var json = @"{ 
                              ""claimReference"" : """ + claimReference.ToString() + @"""
                         }";

            var claimEntityProviderBuilder = JsonConvert.DeserializeObject<ClaimEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockClaimRepository = new Mock<IClaimReadModelRepository>();

            var model = new ClaimReadModelWithRelatedEntities() { Claim = new Mock<ClaimReadModel>(expectedClaimId).Object };
            mockClaimRepository
                .Setup(c => c.GetClaimWithRelatedEntitiesByReference(
                    It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IClaimReadModelRepository))).Returns(mockClaimRepository.Object);
            var claimEntityProvider = claimEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger(environment: DeploymentEnvironment.Production);

            // Act
            var entityObject = (await claimEntityProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.Claim));

            var claimEntity = entityObject.DataValue as SerialisedEntitySchemaObject.Claim;
            claimEntity.Id.Should().Be(expectedClaimId.ToString());

            // Check if the mock quote repository uses product context environment.
            mockClaimRepository
                .Verify(c => c.GetClaimWithRelatedEntitiesByReference(It.IsAny<Guid>(), claimReference, DeploymentEnvironment.Production, It.IsAny<List<string>>()), Times.Once);
        }

        [Fact]
        public async Task ClaimEntityProvider_Should_Return_ClaimEntity_When_Pass_With_ClaimReference_And_Environment()
        {
            // Arrange
            var claimReference = "NKHSA";
            var expectedClaimId = Guid.NewGuid();

            var json = @"{ 
                              ""claimReference"" : """ + claimReference.ToString() + @""",
                              ""environment"" : ""Development""
                         }";

            var claimEntityProviderBuilder = JsonConvert.DeserializeObject<ClaimEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockClaimRepository = new Mock<IClaimReadModelRepository>();

            var model = new ClaimReadModelWithRelatedEntities() { Claim = new Mock<ClaimReadModel>(expectedClaimId).Object };
            mockClaimRepository
                .Setup(c => c.GetClaimWithRelatedEntitiesByReference(
                    It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IClaimReadModelRepository))).Returns(mockClaimRepository.Object);
            var claimEntityProvider = claimEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await claimEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.Claim));

            var claimEntity = entityObject.DataValue as SerialisedEntitySchemaObject.Claim;
            claimEntity.Id.Should().Be(expectedClaimId.ToString());

            // Check if the mock quote repository uses the passed environment.
            mockClaimRepository.Verify(c => c.GetClaimWithRelatedEntitiesByReference(It.IsAny<Guid>(), claimReference, DeploymentEnvironment.Development, It.IsAny<List<string>>()), Times.Once);
        }

        [Fact]
        public async Task ClaimEntityProvider_Should_Return_ClaimEntity_When_Pass_With_ClaimNumber()
        {
            // Arrange
            var claimNumber = "C-0000";
            var expectedClaimId = Guid.NewGuid();

            var json = @"{ 
                              ""claimNumber"" : """ + claimNumber.ToString() + @"""
                         }";

            var claimEntityProviderBuilder = JsonConvert.DeserializeObject<ClaimEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockClaimRepository = new Mock<IClaimReadModelRepository>();

            var model = new ClaimReadModelWithRelatedEntities() { Claim = new Mock<ClaimReadModel>(expectedClaimId).Object };
            mockClaimRepository
                .Setup(c => c.GetClaimWithRelatedEntitiesByNumber(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IClaimReadModelRepository))).Returns(mockClaimRepository.Object);
            var claimEntityProvider = claimEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger(environment: DeploymentEnvironment.Production);

            // Act
            var entityObject = (await claimEntityProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.Claim));

            var claimEntity = entityObject.DataValue as SerialisedEntitySchemaObject.Claim;
            claimEntity.Id.Should().Be(expectedClaimId.ToString());

            // Check if the mock quote repository uses product context environment.
            mockClaimRepository
                .Verify(c => c.GetClaimWithRelatedEntitiesByNumber(It.IsAny<Guid>(), claimNumber, DeploymentEnvironment.Production, It.IsAny<List<string>>()), Times.Once);
        }

        [Fact]
        public async Task ClaimEntityProvider_Should_Return_ClaimEntity_When_Pass_With_ClaimNumber_And_Environment()
        {
            // Arrange
            var claimNumber = "C-0000";
            var expectedClaimId = Guid.NewGuid();

            var json = @"{ 
                              ""claimNumber"" : """ + claimNumber.ToString() + @""",
                              ""environment"" : ""Development""
                         }";

            var claimEntityProviderBuilder = JsonConvert.DeserializeObject<ClaimEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockClaimRepository = new Mock<IClaimReadModelRepository>();

            var model = new ClaimReadModelWithRelatedEntities() { Claim = new Mock<ClaimReadModel>(expectedClaimId).Object };
            mockClaimRepository
                .Setup(c => c.GetClaimWithRelatedEntitiesByNumber(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IClaimReadModelRepository))).Returns(mockClaimRepository.Object);
            var claimEntityProvider = claimEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await claimEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.Claim));

            var claimEntity = entityObject.DataValue as SerialisedEntitySchemaObject.Claim;
            claimEntity.Id.Should().Be(expectedClaimId.ToString());

            // Check if the mock quote repository uses the passed environment.
            mockClaimRepository
                .Verify(c => c.GetClaimWithRelatedEntitiesByNumber(It.IsAny<Guid>(), claimNumber, DeploymentEnvironment.Development, It.IsAny<List<string>>()), Times.Once);
        }

        [Fact]
        public async Task ClaimEntityProvider_Should_Throw_When_ClaimId_DoesNot_Exists()
        {
            // Arrange
            var claimId = Guid.NewGuid();
            var json = @"{ 
                              ""claimId"" : """ + claimId.ToString() + @"""
                         }";

            var claimEntityProviderBuilder = JsonConvert.DeserializeObject<ClaimEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockClaimRepository = new Mock<IClaimReadModelRepository>();

            mockClaimRepository
                .Setup(c => c.GetClaimWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(default(IClaimReadModelWithRelatedEntities));

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IClaimReadModelRepository))).Returns(mockClaimRepository.Object);
            var claimEntityProvider = claimEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> act = async () => await claimEntityProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Title.Should().Be("Cannot resolve entity reference");
            exception.Which.Error.AdditionalDetails.Should().Contain("Entity Type: Claim");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Claim ID: {claimId}");
        }

        [Fact]
        public async Task ClaimEntityProvider_Should_Throw_When_ClaimReference_DoesNot_Exists()
        {
            // Arrange
            var claimReference = "NKHSA";
            var json = @"{ 
                              ""claimReference"" : """ + claimReference.ToString() + @"""
                         }";

            var claimEntityProviderBuilder = JsonConvert.DeserializeObject<ClaimEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockClaimRepository = new Mock<IClaimReadModelRepository>();

            mockClaimRepository
                .Setup(c => c.GetClaimWithRelatedEntitiesByReference(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<List<string>>()))
                .Returns(default(IClaimReadModelWithRelatedEntities));

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IClaimReadModelRepository))).Returns(mockClaimRepository.Object);
            var claimEntityProvider = claimEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> func = async () => await claimEntityProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Title.Should().Be("Cannot resolve entity reference");
            exception.Which.Error.AdditionalDetails.Should().Contain("Entity Type: Claim");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Claim Reference: {claimReference}");
        }

        [Fact]
        public async Task ClaimEntityProvider_Should_Throw_When_ClaimNumber_DoesNot_Exists()
        {
            // Arrange
            var claimNumber = "C-0000";
            var json = @"{ 
                              ""claimNumber"" : """ + claimNumber.ToString() + @"""
                         }";

            var claimEntityProviderBuilder = JsonConvert.DeserializeObject<ClaimEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockClaimRepository = new Mock<IClaimReadModelRepository>();

            mockClaimRepository
                .Setup(c => c.GetClaimWithRelatedEntitiesByReference(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<List<string>>()))
                .Returns(default(IClaimReadModelWithRelatedEntities));

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IClaimReadModelRepository))).Returns(mockClaimRepository.Object);
            var claimEntityProvider = claimEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> func = async () => await claimEntityProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Title.Should().Be("Cannot resolve entity reference");
            exception.Which.Error.AdditionalDetails.Should().Contain("Entity Type: Claim");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Claim Number: {claimNumber}");
        }

        private Mock<IServiceProvider> GetServiceProvider()
        {
            var mockUrlConfiguration = new Mock<IInternalUrlConfiguration>();
            mockUrlConfiguration.Setup(c => c.BaseApi).Returns("https://localhost:4366/api");
            var mockCachingProvider = new Mock<ICachingResolver>();
            var mockProductConfig = new DefaultProductConfiguration();
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
