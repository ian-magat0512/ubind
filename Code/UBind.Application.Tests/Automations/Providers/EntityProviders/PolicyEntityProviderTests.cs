// <copyright file="PolicyEntityProviderTests.cs" company="uBind">
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
    using UBind.Application.Tests.Automations.Providers.Fakes;
    using UBind.Domain;
    using UBind.Domain.Configuration;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using Xunit;
    using SerialisedEntitySchemaObject = UBind.Domain.SerialisedEntitySchemaObject;

    public class PolicyEntityProviderTests
    {
        [Fact]
        public async Task PolicyEntityProvider_Should_Return_PolicyEntity_When_Pass_With_PolicyId()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            var json = @"{ 
                              ""policyId"" : """ + policyId.ToString() + @"""
                         }";

            var policyEntityProviderBuilder = JsonConvert.DeserializeObject<PolicyEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockPolicyRepository = new Mock<IPolicyReadModelRepository>();

            var model = new PolicyReadModelWithRelatedEntities() { Policy = new FakePolicyReadModel(TenantFactory.DefaultId, policyId) };
            mockPolicyRepository
                .Setup(c => c.GetPolicyWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IPolicyReadModelRepository))).Returns(mockPolicyRepository.Object);
            var policyEntityProvider = policyEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await policyEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.Policy));

            var quoteEntity = entityObject.DataValue as SerialisedEntitySchemaObject.Policy;
            quoteEntity.Id.Should().Be(policyId.ToString());
        }

        [Fact]
        public async Task PolicyEntityProvider_Should_Return_PolicyEntity_When_Pass_With_PolicyNumber()
        {
            // Arrange
            var policyNumber = "P-0001";
            var expectedPolicyId = Guid.NewGuid();

            var json = @"{ 
                              ""policyNumber"" : """ + policyNumber.ToString() + @"""
                         }";

            var policyEntityProviderBuilder = JsonConvert.DeserializeObject<PolicyEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockPolicyRepository = new Mock<IPolicyReadModelRepository>();

            var model = new PolicyReadModelWithRelatedEntities() { Policy = new FakePolicyReadModel(TenantFactory.DefaultId, expectedPolicyId) };
            mockPolicyRepository.Setup(c => c.GetPolicyWithRelatedEntities(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<string>(), It.IsAny<List<string>>())).Returns(model);

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IPolicyReadModelRepository))).Returns(mockPolicyRepository.Object);
            var policyEntityProvider = policyEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger(environment: DeploymentEnvironment.Production);

            // Act
            var entityObject = (await policyEntityProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.Policy));

            var quoteEntity = entityObject.DataValue as SerialisedEntitySchemaObject.Policy;
            quoteEntity.Id.Should().Be(expectedPolicyId.ToString());

            // Check if the mock quote repository uses product context environment.
            mockPolicyRepository.Verify(c => c.GetPolicyWithRelatedEntities(It.IsAny<Guid>(), It.IsAny<Guid>(), DeploymentEnvironment.Production, policyNumber, It.IsAny<List<string>>()), Times.Once);
        }

        [Fact]
        public async Task PolicyEntityProvider_Should_Return_PolicyEntity_When_Pass_With_PolicyNumber_And_Environment()
        {
            // Arrange
            var policyNumber = "P-0001";
            var expectedPolicyId = Guid.NewGuid();

            var json = @"{ 
                              ""policyNumber"" : """ + policyNumber.ToString() + @""",
                              ""environment"" : ""Development""
                         }";

            var policyEntityProviderBuilder = JsonConvert.DeserializeObject<PolicyEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockPolicyRepository = new Mock<IPolicyReadModelRepository>();

            var model = new PolicyReadModelWithRelatedEntities() { Policy = new FakePolicyReadModel(TenantFactory.DefaultId, expectedPolicyId) };
            mockPolicyRepository.Setup(c => c.GetPolicyWithRelatedEntities(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<string>(), It.IsAny<List<string>>())).Returns(model);

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IPolicyReadModelRepository))).Returns(mockPolicyRepository.Object);
            var policyEntityProvider = policyEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await policyEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.Policy));

            var policyEntity = entityObject.DataValue as SerialisedEntitySchemaObject.Policy;
            policyEntity.Id.Should().Be(expectedPolicyId.ToString());

            // Check if the mock quote repository uses the passed environment.
            mockPolicyRepository.Verify(c => c.GetPolicyWithRelatedEntities(It.IsAny<Guid>(), It.IsAny<Guid>(), DeploymentEnvironment.Development, policyNumber, It.IsAny<List<string>>()), Times.Once);
        }

        [Fact]
        public async Task PolicyEntityProvider_Should_Throw_When_PolicyId_DoesNot_Exists()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            var json = @"{ 
                              ""policyId"" : """ + policyId.ToString() + @"""
                         }";

            var policyEntityProviderBuilder = JsonConvert.DeserializeObject<PolicyEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockPolicyRepository = new Mock<IPolicyReadModelRepository>();

            mockPolicyRepository
                .Setup(c => c.GetPolicyWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(default(IPolicyReadModelWithRelatedEntities));

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IPolicyReadModelRepository))).Returns(mockPolicyRepository.Object);
            var policyEntityProvider = policyEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> func = async () => await policyEntityProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Title.Should().Be("Cannot resolve entity reference");
            exception.Which.Error.AdditionalDetails.Should().Contain("Entity Type: Policy");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Policy ID: {policyId}");
        }

        [Fact]
        public async Task PolicyEntityProvider_Should_Throw_When_PolicyNumber_DoesNot_Exists()
        {
            // Arrange
            var policyNumber = "P-0001";
            var json = @"{ 
                              ""policyNumber"" : """ + policyNumber.ToString() + @"""
                         }";

            var policyEntityProviderBuilder = JsonConvert.DeserializeObject<PolicyEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockPolicyRepository = new Mock<IPolicyReadModelRepository>();

            mockPolicyRepository.Setup(c => c.GetPolicyWithRelatedEntities(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<string>(), It.IsAny<List<string>>())).Returns(default(IPolicyReadModelWithRelatedEntities));

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IPolicyReadModelRepository))).Returns(mockPolicyRepository.Object);
            var policyEntityProvider = policyEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> func = async () => await policyEntityProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Title.Should().Be("Cannot resolve entity reference");
            exception.Which.Error.AdditionalDetails.Should().Contain("Entity Type: Policy");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Policy Number: {policyNumber}");
        }

        private Mock<IServiceProvider> GetServiceProvider()
        {
            var mockUrlConfiguration = new Mock<IInternalUrlConfiguration>();
            mockUrlConfiguration.Setup(c => c.BaseApi).Returns("https://localhost:4366/api");

            var mockProductConfig = new DefaultProductConfiguration();
            var mockProductConfigProvider = new Mock<IProductConfigurationProvider>();
            var mockFormDataPrettifier = new Mock<IFormDataPrettifier>();
            var mockCachingResolver = new Mock<ICachingResolver>();
            var mockMediator = new Mock<ICqrsMediator>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(c => c.GetService(typeof(ISerialisedEntityFactory)))
                .Returns(new SerialisedEntityFactory(
                    mockUrlConfiguration.Object,
                    mockProductConfigProvider.Object,
                    mockFormDataPrettifier.Object,
                    mockCachingResolver.Object,
                    mockMediator.Object,
                    new DefaultPolicyTransactionTimeOfDayScheme()));

            return mockServiceProvider;
        }
    }
}
