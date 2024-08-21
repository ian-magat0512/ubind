// <copyright file="EmailEntityProviderTests.cs" company="uBind">
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
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using Xunit;
    using SerialisedEntitySchemaObject = UBind.Domain.SerialisedEntitySchemaObject;

    public class EmailEntityProviderTests
    {
        [Fact]
        public async Task EmailEntityProvider_Should_Return_EmailEntity_When_Pass_With_EmailId()
        {
            // Arrange
            var emailId = Guid.NewGuid();
            var e = "xxx+1@email.com";
            var el = new List<string>() { e };
            var json = @"{ 
                              ""emailId"" : """ + emailId.ToString() + @"""
                         }";

            var emailEntityProviderBuilder = JsonConvert.DeserializeObject<EmailMessageEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockEmailRepository = new Mock<IEmailRepository>();

            var email = new UBind.Domain.ReadWriteModel.Email.Email(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                DeploymentEnvironment.Development,
                emailId,
                el,
                e,
                el,
                el,
                el,
                "test",
                "test",
                "test",
                null,
                new TestClock().Timestamp);
            var model = new EmailReadModelWithRelatedEntities() { Email = email };
            mockEmailRepository
                .Setup(c => c.GetEmailWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(model);

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
            mockServiceProvider.Setup(c => c.GetService(typeof(IEmailRepository))).Returns(mockEmailRepository.Object);
            var emailEntityProvider = emailEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await emailEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.EmailMessage));

            var quoteEntity = entityObject.DataValue as SerialisedEntitySchemaObject.EmailMessage;
            quoteEntity.Id.Should().Be(emailId.ToString());
        }

        [Fact]
        public async Task EmailEntityProvider_Should_Throw_When_EmailId_DoesNot_Exists()
        {
            // Arrange
            var emailId = Guid.NewGuid();
            var json = @"{ 
                              ""emailId"" : """ + emailId.ToString() + @"""
                         }";

            var emailEntityProviderBuilder = JsonConvert.DeserializeObject<EmailMessageEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockEmailRepository = new Mock<IEmailRepository>();

            mockEmailRepository.Setup(c => c.GetById(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(default(UBind.Domain.ReadWriteModel.Email.Email));

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(c => c.GetService(typeof(IEmailRepository))).Returns(mockEmailRepository.Object);
            var mockUrlConfiguration = new Mock<IInternalUrlConfiguration>();
            mockUrlConfiguration.Setup(c => c.BaseApi).Returns("https://localhost:4366/api");
            var mockProductConfig = new DefaultProductConfiguration();
            var mockProductConfigProvider = new Mock<IProductConfigurationProvider>();
            var mockFormDataPrettifier = new Mock<IFormDataPrettifier>();
            var mockCachingResolver = new Mock<ICachingResolver>();
            var mockMediator = new Mock<ICqrsMediator>();
            mockServiceProvider.Setup(c => c.GetService(typeof(ISerialisedEntityFactory)))
                .Returns(new SerialisedEntityFactory(
                    mockUrlConfiguration.Object,
                    mockProductConfigProvider.Object,
                    mockFormDataPrettifier.Object,
                    mockCachingResolver.Object,
                    mockMediator.Object,
                    new DefaultPolicyTransactionTimeOfDayScheme()));
            var emailEntityProvider = emailEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> func = async () => await emailEntityProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Title.Should().Be("Cannot resolve entity reference");
            exception.Which.Error.AdditionalDetails.Should().Contain("Entity Type: Email");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Email ID: {emailId.ToString()}");
        }
    }
}
