// <copyright file="CreateOrganisationActionTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Actions;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Application.Commands.Organisation;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Services;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using UBind.Domain.Tests.Fakes;
    using Xunit;
    using Action = UBind.Application.Automation.Actions.Action;

    public class CreateOrganisationActionTests
    {
        [Theory]
        [InlineData("test", "value")]
        [InlineData("test01", "test-1")]
        [InlineData("test welcomee", "true-1")]
        [InlineData("testerz", "westorm-4")]
        [InlineData("tqwe q", "jeo")]
        public async Task CreateOrganisationAction_ShouldSuceed_ProperNameAndAlias(string organisationName, string organisationAlias)
        {
            // Arrange
            var cachingResolver = new Mock<ICachingResolver>().Object;
            var organisationServiceMock = new Mock<IOrganisationService>();
            var mediatorMock = new Mock<ICqrsMediator>();
            var organisationReadModel = new OrganisationReadModel(
                Guid.NewGuid(),
                Guid.NewGuid(),
                organisationAlias,
                organisationName,
                null,
                true,
                false,
                new TestClock().GetCurrentInstant());
            mediatorMock.Setup(s => s.Send(It.IsAny<CreateOrganisationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(organisationReadModel);
            organisationServiceMock.Setup(x => x.CreateActiveNonDefaultAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<AdditionalPropertyValueUpsertModel>>()))
                .ReturnsAsync(organisationReadModel);
            var organisationService = organisationServiceMock.Object;
            var additionalPropertyDefinitionRepository = new Mock<IAdditionalPropertyDefinitionRepository>().Object;
            var organisationNameProvider = new StaticProvider<Data<string>>(organisationName);
            var organisationAliasProvider = new StaticProvider<Data<string>>(organisationAlias);
            var createOrganisationAction = new CreateOrganisationAction(
                "Set variable",
                "setPropertyValue",
                string.Empty,
                false,
                null,
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<Action>(),
                organisationNameProvider,
                organisationAliasProvider,
                null,
                null,
                new TestClock(),
                cachingResolver,
                new Mock<IAdditionalPropertyTransformHelper>().Object,
                mediatorMock.Object);
            var tenantId = Guid.NewGuid();
            var automationData = await MockAutomationData.CreateWithHttpTrigger(null, null, tenantId);
            var actionData = createOrganisationAction.CreateActionData();
            automationData.AddActionData(actionData);

            // Act
            await createOrganisationAction.Execute(new ProviderContext(automationData), actionData);

            // Assert
            (actionData as CreateOrganisationActionData).OrganisationName.Should().Be(organisationName);
            (actionData as CreateOrganisationActionData).OrganisationAlias.Should().Be(organisationAlias);
        }

        [Theory]
        [InlineData("testz !!@#")]
        [InlineData(" q ")]
        [InlineData("q")]
        [InlineData("$2")]
        [InlineData("@#@")]
        [InlineData("tyrty!@#")]
        [InlineData("123")]
        [InlineData("ro !!@")]
        [InlineData("qwe .q #$")]
        public async Task CreateOrganisationAction_ShouldFail_InvalidOrganisationName(string organisationName)
        {
            // Arrange
            var organisationAlias = "test";
            var cachingResolver = new Mock<ICachingResolver>().Object;
            var organisationServiceMock = new Mock<IOrganisationService>();
            var additionalPropertiesValue = new List<AdditionalPropertyValueUpsertModel>();
            var mediatorMock = new Mock<ICqrsMediator>();
            var organisationReadModel = new OrganisationReadModel(
                Guid.NewGuid(),
                Guid.NewGuid(),
                organisationAlias,
                organisationName,
                null,
                true,
                false,
                new TestClock().GetCurrentInstant());
            mediatorMock.Setup(s => s.Send(It.IsAny<CreateOrganisationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(organisationReadModel);
            organisationServiceMock.Setup(x => x.CreateActiveNonDefaultAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), additionalPropertiesValue))
                .ReturnsAsync(organisationReadModel);
            var organisationService = organisationServiceMock.Object;
            var additionalPropertyDefinitionRepository = new Mock<IAdditionalPropertyDefinitionRepository>().Object;
            var organisationNameProvider = new StaticProvider<Data<string>>(organisationName);
            var organisationAliasProvider = new StaticProvider<Data<string>>(organisationAlias);
            var createOrganisationAction = new CreateOrganisationAction(
                "Set variable",
                "setPropertyValue",
                string.Empty,
                false,
                null,
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<Action>(),
                organisationNameProvider,
                organisationAliasProvider,
                null,
                null,
                new TestClock(),
                cachingResolver,
                new Mock<IAdditionalPropertyTransformHelper>().Object,
                mediatorMock.Object);
            var tenantId = Guid.NewGuid();
            var automationData = await MockAutomationData.CreateWithHttpTrigger(null, null, tenantId);
            var actionData = createOrganisationAction.CreateActionData();
            automationData.AddActionData(actionData);

            // Act
            Func<Task> func = async () => await createOrganisationAction.Execute(new ProviderContext(automationData), actionData);

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be($"automation.create.organisation.action.organisation.name.invalid");
        }

        [Theory]
        [InlineData(" qwe")]
        [InlineData(" q ")]
        [InlineData("1q")]
        [InlineData("qe#")]
        [InlineData("@#@")]
        [InlineData("tyrty!@#")]
        [InlineData("123")]
        [InlineData("ZZ")]
        [InlineData("qwe .q #$")]
        public async Task CreateOrganisationAction_ShouldFail_InvalidOrganisationAlias(string organisationAlias)
        {
            // Arrange
            var organisationName = "test";
            var cachingResolver = new Mock<ICachingResolver>().Object;
            var organisationServiceMock = new Mock<IOrganisationService>();
            var additionalPropertiesValue = new List<AdditionalPropertyValueUpsertModel>();
            var mediatorMock = new Mock<ICqrsMediator>();
            var organisationReadModel = new OrganisationReadModel(
                Guid.NewGuid(),
                Guid.NewGuid(),
                organisationAlias,
                organisationName,
                null,
                true,
                false,
                new TestClock().GetCurrentInstant());
            mediatorMock.Setup(s => s.Send(It.IsAny<CreateOrganisationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(organisationReadModel);
            organisationServiceMock.Setup(x => x.CreateActiveNonDefaultAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), additionalPropertiesValue))
                .ReturnsAsync(organisationReadModel);
            var organisationService = organisationServiceMock.Object;
            var additionalPropertyDefinitionRepository = new Mock<IAdditionalPropertyDefinitionRepository>().Object;
            var organisationNameProvider = new StaticProvider<Data<string>>(organisationName);
            var organisationAliasProvider = new StaticProvider<Data<string>>(organisationAlias);
            var createOrganisationAction = new CreateOrganisationAction(
                "Set variable",
                "setPropertyValue",
                string.Empty,
                false,
                null,
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<Action>(),
                organisationNameProvider,
                organisationAliasProvider,
                null,
                null,
                new TestClock(),
                cachingResolver,
                new Mock<IAdditionalPropertyTransformHelper>().Object,
                mediatorMock.Object);
            var tenantId = Guid.NewGuid();
            var automationData = await MockAutomationData.CreateWithHttpTrigger(null, null, tenantId);
            var actionData = createOrganisationAction.CreateActionData();
            automationData.AddActionData(actionData);

            // Act
            Func<Task> func = async () => await createOrganisationAction.Execute(new ProviderContext(automationData), actionData);

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be($"automation.create.organisation.action.organisation.alias.invalid");
        }

        [Fact]
        public async Task CreateOrganisationAction_ShouldThrow_WhenCreatingWithAdditionalPropertiesThatDontExist()
        {
            // Arrange
            var organisationName = "qwe";
            var organisationAlias = "qwe";
            var additionalPropertiesValues = "[{ \"propertyName\": \"static\", \"value\": 123.34 }]";
            var tenantId = Guid.NewGuid();
            var cachingResolver = new Mock<ICachingResolver>().Object;
            var organisationServiceMock = new Mock<IOrganisationService>();
            var mediatorMock = new Mock<ICqrsMediator>();
            var organisationReadModel = new OrganisationReadModel(
                Guid.NewGuid(),
                Guid.NewGuid(),
                organisationAlias,
                organisationName,
                null,
                true,
                false,
                new TestClock().GetCurrentInstant());
            mediatorMock.Setup(s => s.Send(It.IsAny<CreateOrganisationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(organisationReadModel);
            organisationServiceMock.Setup(x => x.CreateActiveNonDefaultAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<AdditionalPropertyValueUpsertModel>>()))
                .ReturnsAsync(organisationReadModel);
            var providerModel = JsonConvert.DeserializeObject<IBuilder<IObjectProvider>>(additionalPropertiesValues, AutomationDeserializationConfiguration.ModelSettings);
            var serviceProvider = new Mock<IServiceProvider>().AddLoggers().Object;
            var dataProvider = providerModel.Build(serviceProvider);
            var organisationService = organisationServiceMock.Object;
            var additionalPropertyValueService = this.GetAdditionalPropertyValueService(tenantId);
            var organisationNameProvider = new StaticProvider<Data<string>>(organisationName);
            var organisationAliasProvider = new StaticProvider<Data<string>>(organisationAlias);
            var createOrganisationAction = new CreateOrganisationAction(
                "Set variable",
                "setPropertyValue",
                string.Empty,
                false,
                null,
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<Action>(),
                organisationNameProvider,
                organisationAliasProvider,
                null,
                dataProvider,
                new TestClock(),
                cachingResolver,
                additionalPropertyValueService,
                mediatorMock.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger(null, null, tenantId);
            var actionData = createOrganisationAction.CreateActionData();
            automationData.AddActionData(actionData);

            // Act
            Func<Task> func = async () => await createOrganisationAction.Execute(new ProviderContext(automationData), actionData);

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be($"additional.property.value.has.invalid.type");
        }

        private IAdditionalPropertyTransformHelper GetAdditionalPropertyValueService(Guid tenantId)
        {
            var additionalPropertyDefinitionRepositoryMock = new Mock<IAdditionalPropertyDefinitionRepository>();
            var mediatorMock = new Mock<ICqrsMediator>();
            additionalPropertyDefinitionRepositoryMock.Setup(x => x.GetByModelFilter(It.IsAny<Guid>(), It.IsAny<AdditionalPropertyDefinitionReadModelFilters>()))
                .ReturnsAsync(
                new List<AdditionalPropertyDefinitionReadModel>
                {
                    new AdditionalPropertyDefinitionReadModel(
                        tenantId,
                        Guid.NewGuid(),
                        Instant.MinValue,
                        "test",
                        "test",
                        Domain.Enums.AdditionalPropertyEntityType.Organisation,
                        Domain.Enums.AdditionalPropertyDefinitionContextType.Tenant,
                        tenantId,
                        true,
                        false,
                        false,
                        string.Empty,
                        Domain.Enums.AdditionalPropertyDefinitionType.Text,
                        Domain.Enums.AdditionalPropertyDefinitionSchemaType.None,
                        tenantId),
                });

            return new AdditionalPropertyTransformHelper(additionalPropertyDefinitionRepositoryMock.Object, mediatorMock.Object);
        }
    }
}
