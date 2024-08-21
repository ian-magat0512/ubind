// <copyright file="CustomerEntityProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.EntityProviders
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using NodaTime;
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
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using Xunit;
    using SerialisedEntitySchemaObject = UBind.Domain.SerialisedEntitySchemaObject;

    public class CustomerEntityProviderTests
    {
        [Fact]
        public async Task CustomerEntityProvider_Should_Return_CustomerEntity_When_Pass_With_CustomerId()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var json = @"{ 
                              ""customerId"" : """ + customerId.ToString() + @"""
                         }";

            var customerEntityProviderBuilder = JsonConvert.DeserializeObject<CustomerEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockCustomerReadModelRepository = new Mock<ICustomerReadModelRepository>();

            var personReadModel = new PersonReadModel(Guid.NewGuid());
            var deploymentEnvironment = DeploymentEnvironment.Staging;
            var currentInstant = SystemClock.Instance.GetCurrentInstant();
            var customer = new CustomerReadModel(customerId, personReadModel, deploymentEnvironment, null, currentInstant, false);
            customer.People = new Collection<PersonReadModel> { personReadModel };
            var model = new CustomerReadModelWithRelatedEntities() { Customer = customer };
            mockCustomerReadModelRepository.Setup(c => c.GetCustomerWithRelatedEntities(
                It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>())).Returns(model);

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(ICustomerReadModelRepository))).Returns(mockCustomerReadModelRepository.Object);
            var customerEntityProvider = customerEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await customerEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.Customer));

            var customerEntity = entityObject.DataValue as SerialisedEntitySchemaObject.Customer;
            customerEntity.Id.Should().Be(customerId.ToString());
        }

        [Fact]
        public async Task CustomerEntityProvider_Should_Throw_When_CustomerId_DoesNot_Exists()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var json = @"{ 
                              ""customerId"" : """ + customerId.ToString() + @"""
                         }";

            var customerEntityProviderBuilder = JsonConvert.DeserializeObject<CustomerEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockCustomerReadModelRepository = new Mock<ICustomerReadModelRepository>();

            mockCustomerReadModelRepository.Setup(c => c.GetCustomerWithRelatedEntities(
                It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(default(ICustomerReadModelWithRelatedEntities));

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(ICustomerReadModelRepository))).Returns(mockCustomerReadModelRepository.Object);
            var customerEntityProvider = customerEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> func = async () => await customerEntityProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Title.Should().Be("Cannot resolve entity reference");
            exception.Which.Error.AdditionalDetails.Should().Contain("Entity Type: customer");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Customer ID: {customerId}");
        }

        [Fact]
        public async Task CustomerEntityProvider_Should_Return_CustomerEntity_When_Pass_With_EmailAddress()
        {
            // Arrange
            var email = "xxxx+12@email.com";
            var expectedCustomerId = Guid.NewGuid();

            var json = @"{ 
                              ""customerAccountEmail"" : """ + email + @"""
                         }";

            var customerEntityProviderBuilder = JsonConvert.DeserializeObject<CustomerEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockCustomerReadModelRepository = new Mock<ICustomerReadModelRepository>();

            var model = new CustomerReadModelWithRelatedEntities() { Customer = new FakeCustomerReadModel(expectedCustomerId) };
            mockCustomerReadModelRepository
                .Setup(c => c.GetCustomerWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(ICustomerReadModelRepository))).Returns(mockCustomerReadModelRepository.Object);
            var customerEntityProvider = customerEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await customerEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.Customer));

            var customerEntity = entityObject.DataValue as SerialisedEntitySchemaObject.Customer;
            customerEntity.Id.Should().Be(expectedCustomerId.ToString());
        }

        [Fact]
        public async Task CustomerEntityProvider_Should_Throw_When_EmailAddress_DoesNot_Exists()
        {
            // Arrange
            var email = "xxxx+12@email.com";
            var json = @"{ 
                              ""customerAccountEmail"" : """ + email + @"""
                         }";

            var customerEntityProviderBuilder = JsonConvert.DeserializeObject<CustomerEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockCustomerReadModelRepository = new Mock<ICustomerReadModelRepository>();

            mockCustomerReadModelRepository
                .Setup(c => c.GetCustomerWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(default(ICustomerReadModelWithRelatedEntities));

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(ICustomerReadModelRepository))).Returns(mockCustomerReadModelRepository.Object);
            var customerEntityProvider = customerEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> func = async () => await customerEntityProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Title.Should().Be("Cannot resolve entity reference");
            exception.Which.Error.AdditionalDetails.Should().Contain("Entity Type: customer");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Customer Account Email: {email}");
        }

        private async Task<Mock<IServiceProvider>> GetServiceProvider()
        {
            var mockUrlConfiguration = new Mock<IInternalUrlConfiguration>();
            mockUrlConfiguration.Setup(c => c.BaseApi).Returns("https://localhost:4366/api");
            var mockProductConfig = new DefaultProductConfiguration();
            var mockProductConfigProvider = new Mock<IProductConfigurationProvider>();
            var mockFormDataPrettifier = new Mock<IFormDataPrettifier>();
            var mockMediator = new Mock<ICqrsMediator>();

            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var cachingResolver = new Mock<ICachingResolver>();
            cachingResolver.Setup(x => x.GetTenantOrThrow(automationData.ContextManager.Tenant.Id))
                .Returns(Task.FromResult(TenantFactory.Create(automationData.ContextManager.Tenant.Id)));

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(c => c.GetService(typeof(ISerialisedEntityFactory)))
                .Returns(new SerialisedEntityFactory(
                    mockUrlConfiguration.Object,
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
