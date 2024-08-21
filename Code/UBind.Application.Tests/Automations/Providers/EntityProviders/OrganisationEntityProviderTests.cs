// <copyright file="OrganisationEntityProviderTests.cs" company="uBind">
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
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using Xunit;
    using SerialisedEntitySchemaObject = UBind.Domain.SerialisedEntitySchemaObject;

    public class OrganisationEntityProviderTests
    {
        [Fact]
        public async Task OrganisationEntityProvider_Should_Return_OrganisationEntity_When_Pass_With_OrganisationId()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var json = @"{ 
                              ""organisationId"" : """ + organisationId.ToString() + @"""
                         }";

            var organisationEntityProviderBuilder = JsonConvert.DeserializeObject<OrganisationEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockOrganisationRepository = new Mock<IOrganisationReadModelRepository>();

            var organisation = new OrganisationReadModel(TenantFactory.DefaultId, organisationId, "orgAlias", "orgName", null, true, false, new TestClock().Timestamp);
            var model = new OrganisationReadModelWithRelatedEntities() { Organisation = organisation };
            mockOrganisationRepository
                .Setup(c => c.GetOrganisationWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider
                .Setup(c => c.GetService(typeof(IOrganisationReadModelRepository)))
                .Returns(mockOrganisationRepository.Object);
            var organisationEntityProvider = organisationEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await organisationEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.Organisation));

            var organisationEntity = entityObject.DataValue as SerialisedEntitySchemaObject.Organisation;
            organisationEntity.Id.Should().Be(organisationId.ToString());
        }

        [Fact]
        public async Task OrganisationEntityProvider_Should_Throw_When_OrganisationId_DoesNot_Exists()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var json = @"{ 
                              ""organisationId"" : """ + organisationId.ToString() + @"""
                         }";

            var organisationEntityProviderBuilder = JsonConvert.DeserializeObject<OrganisationEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockOrganisationRepository = new Mock<IOrganisationReadModelRepository>();

            mockOrganisationRepository
                .Setup(c => c.GetOrganisationWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(default(OrganisationReadModelWithRelatedEntities));

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider
                .Setup(c => c.GetService(typeof(IOrganisationReadModelRepository)))
                .Returns(mockOrganisationRepository.Object);
            var organisationEntityProvider = organisationEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> func = async () => await organisationEntityProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Title.Should().Be("Cannot resolve entity reference");
            exception.Which.Error.AdditionalDetails.Should().Contain("Entity Type: Organisation");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Organisation ID: {organisationId}");
        }

        [Fact]
        public async Task OrganisationEntityProvider_Should_Return_OrganisationEntity_When_Pass_With_OrganisationAlias()
        {
            // Arrange
            var organisationAlias = "organisation_alias";
            var json = @"{ 
                              ""organisationAlias"" : """ + organisationAlias + @"""
                         }";

            var organisationEntityProviderBuilder = JsonConvert.DeserializeObject<OrganisationEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockOrganisationRepository = new Mock<IOrganisationReadModelRepository>();

            var organisationId = Guid.NewGuid();
            var organisation = new OrganisationReadModel(TenantFactory.DefaultId, organisationId, "orgAlias", "orgName", null, true, false, new TestClock().Timestamp);
            var model = new OrganisationReadModelWithRelatedEntities() { Organisation = organisation };
            mockOrganisationRepository
                .Setup(c => c.GetOrganisationWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider
                .Setup(c => c.GetService(typeof(IOrganisationReadModelRepository)))
                .Returns(mockOrganisationRepository.Object);
            var organisationEntityProvider = organisationEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await organisationEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.Organisation));

            var organisationEntity = entityObject.DataValue as SerialisedEntitySchemaObject.Organisation;
            organisationEntity.Id.Should().Be(organisationId.ToString());
        }

        [Fact]
        public async Task OrganisationEntityProvider_Should_Throw_When_OrganisationAlias_DoesNot_Exists()
        {
            // Arrange
            var organisationAlias = "invalid_organisation_alias";
            var json = @"{ 
                              ""organisationAlias"" : """ + organisationAlias + @"""
                         }";

            var organisationEntityProviderBuilder = JsonConvert.DeserializeObject<OrganisationEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockOrganisationRepository = new Mock<IOrganisationReadModelRepository>();

            mockOrganisationRepository
                .Setup(c => c.GetOrganisationWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(default(OrganisationReadModelWithRelatedEntities));

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider
                .Setup(c => c.GetService(typeof(IOrganisationReadModelRepository)))
                .Returns(mockOrganisationRepository.Object);
            var organisationEntityProvider = organisationEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> func = async () => await organisationEntityProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Title.Should().Be("Cannot resolve entity reference");
            exception.Which.Error.AdditionalDetails.Should().Contain("Entity Type: Organisation");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Organisation Alias: {organisationAlias}");
        }

        [Fact]
        public async Task OrganisationEntityProvider_Should_Return_OrganisationEntity_With_LastModifiedDateTime_Field()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var json = @"{ 
                              ""organisationId"" : """ + organisationId.ToString() + @"""
                         }";

            var organisationEntityProviderBuilder = JsonConvert.DeserializeObject<OrganisationEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockOrganisationRepository = new Mock<IOrganisationReadModelRepository>();

            var organisation = new OrganisationReadModel(TenantFactory.DefaultId, organisationId, "orgAlias", "orgName", null, true, false, new TestClock().Timestamp);
            var model = new OrganisationReadModelWithRelatedEntities() { Organisation = organisation };
            mockOrganisationRepository
                .Setup(c => c.GetOrganisationWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IOrganisationReadModelRepository))).Returns(mockOrganisationRepository.Object);
            var organisationEntityProvider = organisationEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await organisationEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.Organisation));

            var organisationEntity = entityObject.DataValue as SerialisedEntitySchemaObject.Organisation;
            organisationEntity.LastModifiedDateTime.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task OrganisationEntityProvider_Should_Return_OrganisationEntity_With_LastModifiedDate_Field()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var json = @"{ 
                              ""organisationId"" : """ + organisationId.ToString() + @"""
                         }";

            var organisationEntityProviderBuilder = JsonConvert.DeserializeObject<OrganisationEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockOrganisationRepository = new Mock<IOrganisationReadModelRepository>();

            var organisation = new OrganisationReadModel(TenantFactory.DefaultId, organisationId, "orgAlias", "orgName", null, true, false, new TestClock().Timestamp);
            var model = new OrganisationReadModelWithRelatedEntities() { Organisation = organisation };
            mockOrganisationRepository
                .Setup(c => c.GetOrganisationWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider
                .Setup(c => c.GetService(typeof(IOrganisationReadModelRepository)))
                .Returns(mockOrganisationRepository.Object);
            var organisationEntityProvider = organisationEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await organisationEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.Organisation));

            var organisationEntity = entityObject.DataValue as SerialisedEntitySchemaObject.Organisation;
            organisationEntity.LastModifiedDateTime.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task OrganisationEntityProvider_Should_Return_OrganisationEntity_With_LastModifiedTime_Field()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var json = @"{ 
                              ""organisationId"" : """ + organisationId.ToString() + @"""
                         }";

            var organisationEntityProviderBuilder = JsonConvert.DeserializeObject<OrganisationEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockOrganisationRepository = new Mock<IOrganisationReadModelRepository>();

            var organisation = new OrganisationReadModel(TenantFactory.DefaultId, organisationId, "orgAlias", "orgName", null, true, false, new TestClock().Timestamp);
            var model = new OrganisationReadModelWithRelatedEntities() { Organisation = organisation };
            mockOrganisationRepository
                .Setup(c => c.GetOrganisationWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IOrganisationReadModelRepository))).Returns(mockOrganisationRepository.Object);
            var organisationEntityProvider = organisationEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await organisationEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.Organisation));

            var organisationEntity = entityObject.DataValue as SerialisedEntitySchemaObject.Organisation;
            organisationEntity.LastModifiedDateTime.Should().NotBeNullOrWhiteSpace();
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
