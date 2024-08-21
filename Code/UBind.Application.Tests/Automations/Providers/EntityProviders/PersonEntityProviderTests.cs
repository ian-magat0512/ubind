// <copyright file="PersonEntityProviderTests.cs" company="uBind">
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
    using Humanizer;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
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
    using UBind.Domain.ReadModel;
    using UBind.Domain.SerialisedEntitySchemaObject;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class PersonEntityProviderTests
    {
        [Fact]
        public async Task PersonEntityProvider_Should_Return_PersonEntity_WhenPassedWithCustomerId()
        {
            // Arrange
            var personId = Guid.NewGuid();
            var model = new PersonReadModelWithRelatedEntities()
            {
                Person = new Mock<PersonReadModel>(personId).Object,
            };
            var json = @"{
                            ""personId"": """ + personId.ToString() + @"""
                        }";
            var personEntitiyProviderBuilder = JsonConvert.DeserializeObject<PersonEntityProviderConfigModel>(
                json, AutomationDeserializationConfiguration.ModelSettings);
            var mockPersonReadModelRepository = new Mock<IPersonReadModelRepository>();
            mockPersonReadModelRepository
                .Setup(
                    p => p.GetPersonWithRelatedEntities(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider.Setup(p => p.GetService(typeof(IPersonReadModelRepository)))
                .Returns(mockPersonReadModelRepository.Object);
            var personEntityProvider = personEntitiyProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entity = (await personEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entity.DataValue.GetType().Should().Be(typeof(Person));
            var person = entity.DataValue as Person;
            person.Id.Should().Be(personId.ToString());
        }

        [Fact]
        public async Task PersonEntityProvider_ShouldThrowException_WhenPersonDoesNotExist()
        {
            // Arrange
            var personId = Guid.NewGuid();
            var model = new PersonReadModelWithRelatedEntities()
            {
                Person = new Mock<PersonReadModel>(personId).Object,
            };
            var json = @"{
                            ""personId"": """ + personId.ToString() + @"""
                        }";
            var personEntitiyProviderBuilder = JsonConvert.DeserializeObject<PersonEntityProviderConfigModel>(
                json, AutomationDeserializationConfiguration.ModelSettings);
            var mockPersonReadModelRepository = new Mock<IPersonReadModelRepository>();

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider.Setup(p => p.GetService(typeof(IPersonReadModelRepository)))
                .Returns(mockPersonReadModelRepository.Object);
            var personEntityProvider = personEntitiyProviderBuilder.Build(mockServiceProvider.Object);
            var errorThrown = Errors.Automation.Provider.Entity.NotFound(
                EntityType.Person.Humanize(), "personId", personId.ToString(), new JObject());
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act + Assert
            Func<Task> func = async () => await personEntityProvider.Resolve(new ProviderContext(automationData));
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Title.Should().Be(errorThrown.Title);
            exception.Which.Error.AdditionalDetails.Should().Contain($"Person ID: {personId}");
            exception.Which.Error.Data.Should().NotBeNull();
        }

        private async Task<Mock<IServiceProvider>> GetServiceProvider()
        {
            var mockUrlConfig = new Mock<IInternalUrlConfiguration>();
            mockUrlConfig.Setup(c => c.BaseApi).Returns("https://localhost:4366/api");

            var mockProductConfig = new DefaultProductConfiguration();
            var mockProductConfigProvider = new Mock<IProductConfigurationProvider>();
            var mockFormDataPrettifier = new Mock<IFormDataPrettifier>();
            var mockMediator = new Mock<ICqrsMediator>();
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var cachingResolver = new Mock<ICachingResolver>();
            cachingResolver.Setup(x => x.GetTenantOrThrow(automationData.ContextManager.Tenant.Id))
                .Returns(Task.FromResult(TenantFactory.Create(automationData.ContextManager.Tenant.Id)));

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(p => p.GetService(typeof(ISerialisedEntityFactory)))
                .Returns(new SerialisedEntityFactory(
                    mockUrlConfig.Object,
                    mockProductConfigProvider.Object,
                    mockFormDataPrettifier.Object,
                    cachingResolver.Object,
                    mockMediator.Object,
                    new DefaultPolicyTransactionTimeOfDayScheme()));

            mockServiceProvider.Setup(c => c.GetService(typeof(ICachingResolver)))
                .Returns(cachingResolver.Object);
            return mockServiceProvider;
        }
    }
}
