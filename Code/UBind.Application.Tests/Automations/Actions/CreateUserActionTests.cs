// <copyright file="CreateUserActionTests.cs" company="uBind">
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
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NodaTime;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Actions;
    using UBind.Application.Automation.ContactDetail;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Application.Tests.Helpers;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using UBind.Domain.Tests.Fakes;
    using Xunit;
    using Action = UBind.Application.Automation.Actions.Action;
    using CreateUserAction = UBind.Application.Automation.Actions.CreateUserAction;
    using IEntity = UBind.Domain.SerialisedEntitySchemaObject.IEntity;
    using Organisation = UBind.Domain.SerialisedEntitySchemaObject.Organisation;

    public class CreateUserActionTests
    {
        [Theory]
        [InlineData("jeo.talavera@gmail.com", "test")]
        [InlineData("asd@gmail.com", "juniper")]
        [InlineData("ubind@ubind.io", "calcium plus")]
        public async Task CreateUserAction_ShouldSucceed_CreateUser(string emailAddress, string personFirstName)
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            var automationData = await MockAutomationData.CreateWithHttpTrigger(null, null, tenantId);
            IProviderContext providerContext = new ProviderContext(automationData);
            var additionalPropertyDefinitionRepository = new Mock<IAdditionalPropertyDefinitionRepository>().Object;
            var accountEmailProvider = new StaticProvider<Data<string>>(emailAddress);
            var firstNameProvider = new StaticProvider<Data<string>>(personFirstName);
            OrganisationEntityProvider organisationProvider = this.GetOrganisationProvider(tenantId, organisationId);
            var userId = Guid.NewGuid();
            var mediator = this.GetCqrsMediatorWithUserService(tenantId, userId, organisationId);
            var createUserAction = new CreateUserAction(
                "Set variable",
                "setPropertyValue",
                string.Empty,
                false,
                null,
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<Action>(),
                accountEmailProvider,
                organisationProvider,
                null,
                null,
                null,
                new Mock<IAdditionalPropertyTransformHelper>().Object,
                new PersonConstructor(firstNameProvider),
                mediator,
                new Mock<IRoleRepository>().Object,
                new Mock<IClock>().Object);
            var actionData = createUserAction.CreateActionData();
            automationData.AddActionData(actionData);

            // Act
            await createUserAction.Execute(providerContext, actionData);

            // Assert
            (actionData as CreateUserActionData).UserId.Should().NotBeEmpty();
            (actionData as CreateUserActionData).AccountEmailAddress.Should().Be(emailAddress);
            (actionData as CreateUserActionData).OrganisationId.Should().Be(organisationId);
        }

        [Theory]
        [InlineData("jeo.talavera.gmail.com", "test")]
        [InlineData("asd@gmail.com@", "juniper")]
        [InlineData("ubind@ubind.io.@", "calcium plus")]
        public async Task CreateUserAction_ShouldFail_IfWrongEmailAddress(string emailAddress, string personFirstName)
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            var automationData = await MockAutomationData.CreateWithHttpTrigger(null, null, tenantId);
            IProviderContext providerContext = new ProviderContext(automationData);
            var additionalPropertyDefinitionRepository = new Mock<IAdditionalPropertyDefinitionRepository>().Object;
            var accountEmailProvider = new StaticProvider<Data<string>>(emailAddress);
            var firstNameProvider = new StaticProvider<Data<string>>(personFirstName);
            OrganisationEntityProvider organisationProvider = this.GetOrganisationProvider(tenantId, organisationId);
            var userId = Guid.NewGuid();
            var userService = this.GetCqrsMediatorWithUserService(tenantId, userId, organisationId);
            var createUserAction = new CreateUserAction(
                "Set variable",
                "setPropertyValue",
                string.Empty,
                false,
                null,
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<Action>(),
                accountEmailProvider,
                organisationProvider,
                null,
                null,
                null,
                new Mock<IAdditionalPropertyTransformHelper>().Object,
                new PersonConstructor(firstNameProvider),
                userService,
                new Mock<IRoleRepository>().Object,
                new Mock<IClock>().Object);
            var actionData = createUserAction.CreateActionData();
            automationData.AddActionData(actionData);

            // Act
            Func<Task> act = async () => await createUserAction.Execute(providerContext, actionData);

            // Assert
            (await act.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be($"automation.create.user.action.account.email.address.invalid");
        }

        [Fact]
        public async Task CreateUserAction_ShouldSucceed_IfHasPersonData()
        {
            // Arrange
            var accountEmailProvider = new StaticProvider<Data<string>>("test@gmail.com");
            var firstNameProvider = new StaticProvider<Data<string>>("jeo");
            var namePrefixProvider = new StaticProvider<Data<string>>("Jr.");
            var lastNameProvider = new StaticProvider<Data<string>>("talavera");
            var middleNamesProvider = new StaticProvider<Data<string>>("p");
            var nameSuffixProvider = new StaticProvider<Data<string>>("Mr.");
            var preferredNameProvider = new StaticProvider<Data<string>>("J");
            var titleProvider = new StaticProvider<Data<string>>("some title");
            var companyProvider = new StaticProvider<Data<string>>("company name");
            var phoneNumberProvider = new List<PhoneNumberConstructor>()
            {
                new PhoneNumberConstructor(
                    new StaticProvider<Data<string>>("0412345678"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var emailAddressProvider = new List<EmailAddressConstructor>()
            {
                new EmailAddressConstructor(
                    new StaticProvider<Data<string>>("jeo.talavera@gmail.com"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var streetAddressProvider = new List<StreetAddressConstructor>()
            {
                new StreetAddressConstructor(
                    new StaticProvider<Data<string>>("something address"),
                    new StaticProvider<Data<string>>("suburb"),
                    new StaticProvider<Data<string>>("ACT"),
                    new StaticProvider<Data<string>>("2601"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var websiteAddressProvider = new List<WebsiteAddressConstructor>()
            {
                new WebsiteAddressConstructor(
                    new StaticProvider<Data<string>>("www.google.com"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var messengerIdProvider = new List<MessengerIdConstructor>()
            {
                new MessengerIdConstructor(
                    new StaticProvider<Data<string>>("some title"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var socialMediaProvider = new List<SocialMediaIdConstructor>()
            {
                new SocialMediaIdConstructor(
                    new StaticProvider<Data<string>>("some title"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var organisationId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            var automationData = await MockAutomationData.CreateWithHttpTrigger(null, null, tenantId);
            IProviderContext providerContext = new ProviderContext(automationData);
            var additionalPropertyDefinitionRepository = new Mock<IAdditionalPropertyDefinitionRepository>().Object;
            OrganisationEntityProvider organisationProvider = this.GetOrganisationProvider(tenantId, organisationId);
            var userId = Guid.NewGuid();
            var userService = this.GetCqrsMediatorWithUserService(tenantId, userId, organisationId);
            var createUserAction = new CreateUserAction(
                "Set variable",
                "setPropertyValue",
                string.Empty,
                false,
                null,
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<Action>(),
                accountEmailProvider,
                organisationProvider,
                null,
                null,
                null,
                new Mock<IAdditionalPropertyTransformHelper>().Object,
                new PersonConstructor(
                    firstNameProvider,
                    namePrefixProvider,
                    lastNameProvider,
                    middleNamesProvider,
                    nameSuffixProvider,
                    preferredNameProvider,
                    companyProvider,
                    titleProvider,
                    phoneNumberProvider,
                    emailAddressProvider,
                    streetAddressProvider,
                    websiteAddressProvider,
                    messengerIdProvider,
                    socialMediaProvider),
                userService,
                new Mock<IRoleRepository>().Object,
                new Mock<IClock>().Object);
            var actionData = createUserAction.CreateActionData();
            automationData.AddActionData(actionData);

            // Act
            await createUserAction.Execute(providerContext, actionData);

            // Assert
            (actionData as CreateUserActionData).UserId.Should().NotBeEmpty();
            (actionData as CreateUserActionData).AccountEmailAddress.Should().Be("test@gmail.com");
            (actionData as CreateUserActionData).OrganisationId.Should().Be(organisationId);
        }

        [Theory]
        [InlineData("0412345678")]
        [InlineData("0488866323")]
        [InlineData("+61 2 5555 0000")]
        [InlineData("+61 2 2222 2222")]
        [InlineData("+61222222222")]
        [InlineData("+61223211222")]
        public async Task CreateUserAction_ShouldSucceed_IfHasRightPhoneNumber(string phoneNumber)
        {
            // Arrange
            var accountEmailProvider = new StaticProvider<Data<string>>("test@gmail.com");
            var firstNameProvider = new StaticProvider<Data<string>>("jeo");
            var namePrefixProvider = new StaticProvider<Data<string>>("Jr.");
            var lastNameProvider = new StaticProvider<Data<string>>("talavera");
            var middleNamesProvider = new StaticProvider<Data<string>>("p");
            var nameSuffixProvider = new StaticProvider<Data<string>>("Mr.");
            var preferredNameProvider = new StaticProvider<Data<string>>("J");
            var titleProvider = new StaticProvider<Data<string>>("some title");
            var companyProvider = new StaticProvider<Data<string>>("company name");
            var phoneNumberProvider = new List<PhoneNumberConstructor>()
            {
                new PhoneNumberConstructor(
                    new StaticProvider<Data<string>>(phoneNumber),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var emailAddressProvider = new List<EmailAddressConstructor>()
            {
                new EmailAddressConstructor(
                    new StaticProvider<Data<string>>("jeo.talavera@gmail.com"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var streetAddressProvider = new List<StreetAddressConstructor>()
            {
                new StreetAddressConstructor(
                    new StaticProvider<Data<string>>("something address"),
                    new StaticProvider<Data<string>>("suburb"),
                    new StaticProvider<Data<string>>("ACT"),
                    new StaticProvider<Data<string>>("2601"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var websiteAddressProvider = new List<WebsiteAddressConstructor>()
            {
                new WebsiteAddressConstructor(
                    new StaticProvider<Data<string>>("www.google.com"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var messengerIdProvider = new List<MessengerIdConstructor>()
            {
                new MessengerIdConstructor(
                    new StaticProvider<Data<string>>("some title"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var socialMediaProvider = new List<SocialMediaIdConstructor>()
            {
                new SocialMediaIdConstructor(
                    new StaticProvider<Data<string>>("some title"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var organisationId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            var automationData = await MockAutomationData.CreateWithHttpTrigger(null, null, tenantId);
            IProviderContext providerContext = new ProviderContext(automationData);
            var additionalPropertyDefinitionRepository = new Mock<IAdditionalPropertyDefinitionRepository>().Object;
            OrganisationEntityProvider organisationProvider = this.GetOrganisationProvider(tenantId, organisationId);
            var userId = Guid.NewGuid();
            var userService = this.GetCqrsMediatorWithUserService(tenantId, userId, organisationId);
            var createUserAction = new CreateUserAction(
                "Set variable",
                "setPropertyValue",
                string.Empty,
                false,
                null,
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<Action>(),
                accountEmailProvider,
                organisationProvider,
                null,
                null,
                null,
                new Mock<IAdditionalPropertyTransformHelper>().Object,
                new PersonConstructor(
                    firstNameProvider,
                    namePrefixProvider,
                    lastNameProvider,
                    middleNamesProvider,
                    nameSuffixProvider,
                    preferredNameProvider,
                    companyProvider,
                    titleProvider,
                    phoneNumberProvider,
                    emailAddressProvider,
                    streetAddressProvider,
                    websiteAddressProvider,
                    messengerIdProvider,
                    socialMediaProvider),
                userService,
                new Mock<IRoleRepository>().Object,
                new Mock<IClock>().Object);
            var actionData = createUserAction.CreateActionData();
            automationData.AddActionData(actionData);

            // Act
            await createUserAction.Execute(providerContext, actionData);

            // Assert
            (actionData as CreateUserActionData).UserId.Should().NotBeEmpty();
            (actionData as CreateUserActionData).AccountEmailAddress.Should().Be("test@gmail.com");
            (actionData as CreateUserActionData).OrganisationId.Should().Be(organisationId);
        }

        [Theory]
        [InlineData("email@gmail.com")]
        [InlineData("ewq@yahoo.com.ph")]
        [InlineData("wo.wo@gmail.co")]
        [InlineData("howdy@zroo.zx")]
        [InlineData("wo20@ppre.px")]
        [InlineData("welcome@one.zx")]
        public async Task CreateUserAction_ShouldSucceed_IfHasRightEmailAddress(string emailAddress)
        {
            // Arrange
            var accountEmailProvider = new StaticProvider<Data<string>>("test@gmail.com");
            var firstNameProvider = new StaticProvider<Data<string>>("jeo");
            var namePrefixProvider = new StaticProvider<Data<string>>("Jr.");
            var lastNameProvider = new StaticProvider<Data<string>>("talavera");
            var middleNamesProvider = new StaticProvider<Data<string>>("p");
            var nameSuffixProvider = new StaticProvider<Data<string>>("Mr.");
            var preferredNameProvider = new StaticProvider<Data<string>>("J");
            var titleProvider = new StaticProvider<Data<string>>("some title");
            var companyProvider = new StaticProvider<Data<string>>("company name");
            var phoneNumberProvider = new List<PhoneNumberConstructor>()
            {
                new PhoneNumberConstructor(
                    new StaticProvider<Data<string>>("0412345678"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var emailAddressProvider = new List<EmailAddressConstructor>()
            {
                new EmailAddressConstructor(
                    new StaticProvider<Data<string>>(emailAddress),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var streetAddressProvider = new List<StreetAddressConstructor>()
            {
                new StreetAddressConstructor(
                    new StaticProvider<Data<string>>("something address"),
                    new StaticProvider<Data<string>>("suburb"),
                    new StaticProvider<Data<string>>("ACT"),
                    new StaticProvider<Data<string>>("2601"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var websiteAddressProvider = new List<WebsiteAddressConstructor>()
            {
                new WebsiteAddressConstructor(
                    new StaticProvider<Data<string>>("www.google.com"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var messengerIdProvider = new List<MessengerIdConstructor>()
            {
                new MessengerIdConstructor(
                    new StaticProvider<Data<string>>("some title"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var socialMediaProvider = new List<SocialMediaIdConstructor>()
            {
                new SocialMediaIdConstructor(
                    new StaticProvider<Data<string>>("some title"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var organisationId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            var automationData = await MockAutomationData.CreateWithHttpTrigger(null, null, tenantId);
            IProviderContext providerContext = new ProviderContext(automationData);
            var additionalPropertyDefinitionRepository = new Mock<IAdditionalPropertyDefinitionRepository>().Object;
            OrganisationEntityProvider organisationProvider = this.GetOrganisationProvider(tenantId, organisationId);
            var userId = Guid.NewGuid();
            var userService = this.GetCqrsMediatorWithUserService(tenantId, userId, organisationId);
            var createUserAction = new CreateUserAction(
                "Set variable",
                "setPropertyValue",
                string.Empty,
                false,
                null,
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<Action>(),
                accountEmailProvider,
                organisationProvider,
                null,
                null,
                null,
                new Mock<IAdditionalPropertyTransformHelper>().Object,
                new PersonConstructor(
                    firstNameProvider,
                    namePrefixProvider,
                    lastNameProvider,
                    middleNamesProvider,
                    nameSuffixProvider,
                    preferredNameProvider,
                    companyProvider,
                    titleProvider,
                    phoneNumberProvider,
                    emailAddressProvider,
                    streetAddressProvider,
                    websiteAddressProvider,
                    messengerIdProvider,
                    socialMediaProvider),
                userService,
                new Mock<IRoleRepository>().Object,
                new Mock<IClock>().Object);
            var actionData = createUserAction.CreateActionData();
            automationData.AddActionData(actionData);

            // Act
            await createUserAction.Execute(providerContext, actionData);

            // Assert
            (actionData as CreateUserActionData).UserId.Should().NotBeEmpty();
            (actionData as CreateUserActionData).AccountEmailAddress.Should().Be("test@gmail.com");
            (actionData as CreateUserActionData).OrganisationId.Should().Be(organisationId);
        }

        [Theory]
        [InlineData("general luna", "borca", "ACT", "2601")]
        [InlineData("mega street", "coach sub", "VIC", "3999")]
        [InlineData("wensh", "rigthon", "QLD", "4999")]
        [InlineData("billz", "misissipy", "SA", "5999")]
        [InlineData("Victoria", "connecticut", "WA", "6001")]
        [InlineData("Vulimia", "quezon", "TAS", "7800")]
        public async Task CreateUserAction_ShouldSucceed_IfHasRightStreetAddress(string address, string suburb, string state, string postcode)
        {
            // Arrange
            var accountEmailProvider = new StaticProvider<Data<string>>("test@gmail.com");
            var firstNameProvider = new StaticProvider<Data<string>>("jeo");
            var namePrefixProvider = new StaticProvider<Data<string>>("Jr.");
            var lastNameProvider = new StaticProvider<Data<string>>("talavera");
            var middleNamesProvider = new StaticProvider<Data<string>>("p");
            var nameSuffixProvider = new StaticProvider<Data<string>>("Mr.");
            var preferredNameProvider = new StaticProvider<Data<string>>("J");
            var titleProvider = new StaticProvider<Data<string>>("some title");
            var companyProvider = new StaticProvider<Data<string>>("company name");
            var phoneNumberProvider = new List<PhoneNumberConstructor>()
            {
                new PhoneNumberConstructor(
                    new StaticProvider<Data<string>>("0412345678"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var emailAddressProvider = new List<EmailAddressConstructor>()
            {
                new EmailAddressConstructor(
                    new StaticProvider<Data<string>>("qwe@gmail.com"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var streetAddressProvider = new List<StreetAddressConstructor>()
            {
                new StreetAddressConstructor(
                    new StaticProvider<Data<string>>(address),
                    new StaticProvider<Data<string>>(suburb),
                    new StaticProvider<Data<string>>(state),
                    new StaticProvider<Data<string>>(postcode),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var websiteAddressProvider = new List<WebsiteAddressConstructor>()
            {
                new WebsiteAddressConstructor(
                    new StaticProvider<Data<string>>("www.google.com"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var messengerIdProvider = new List<MessengerIdConstructor>()
            {
                new MessengerIdConstructor(
                    new StaticProvider<Data<string>>("some title"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var socialMediaProvider = new List<SocialMediaIdConstructor>()
            {
                new SocialMediaIdConstructor(
                    new StaticProvider<Data<string>>("some title"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var organisationId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            var automationData = await MockAutomationData.CreateWithHttpTrigger(null, null, tenantId);
            IProviderContext providerContext = new ProviderContext(automationData);
            var additionalPropertyDefinitionRepository = new Mock<IAdditionalPropertyDefinitionRepository>().Object;
            OrganisationEntityProvider organisationProvider = this.GetOrganisationProvider(tenantId, organisationId);
            var userId = Guid.NewGuid();
            var userService = this.GetCqrsMediatorWithUserService(tenantId, userId, organisationId);
            var createUserAction = new CreateUserAction(
                "Set variable",
                "setPropertyValue",
                string.Empty,
                false,
                null,
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<Action>(),
                accountEmailProvider,
                organisationProvider,
                null,
                null,
                null,
                new Mock<IAdditionalPropertyTransformHelper>().Object,
                new PersonConstructor(
                    firstNameProvider,
                    namePrefixProvider,
                    lastNameProvider,
                    middleNamesProvider,
                    nameSuffixProvider,
                    preferredNameProvider,
                    companyProvider,
                    titleProvider,
                    phoneNumberProvider,
                    emailAddressProvider,
                    streetAddressProvider,
                    websiteAddressProvider,
                    messengerIdProvider,
                    socialMediaProvider),
                userService,
                new Mock<IRoleRepository>().Object,
                new Mock<IClock>().Object);
            var actionData = createUserAction.CreateActionData();
            automationData.AddActionData(actionData);

            // Act
            await createUserAction.Execute(providerContext, actionData);

            // Assert
            (actionData as CreateUserActionData).UserId.Should().NotBeEmpty();
            (actionData as CreateUserActionData).AccountEmailAddress.Should().Be("test@gmail.com");
            (actionData as CreateUserActionData).OrganisationId.Should().Be(organisationId);
        }

        [Theory]
        [InlineData("google.com")]
        [InlineData("https://google.com")]
        [InlineData("facebook.com")]
        [InlineData("yahoo.mail")]
        [InlineData("fix.core")]
        [InlineData("http://company.ca")]
        public async Task CreateUserAction_ShouldSucceed_IfHasRightWebsite(string website)
        {
            // Arrange
            var accountEmailProvider = new StaticProvider<Data<string>>("test@gmail.com");
            var firstNameProvider = new StaticProvider<Data<string>>("jeo");
            var namePrefixProvider = new StaticProvider<Data<string>>("Jr.");
            var lastNameProvider = new StaticProvider<Data<string>>("talavera");
            var middleNamesProvider = new StaticProvider<Data<string>>("p");
            var nameSuffixProvider = new StaticProvider<Data<string>>("Mr.");
            var preferredNameProvider = new StaticProvider<Data<string>>("J");
            var titleProvider = new StaticProvider<Data<string>>("some title");
            var companyProvider = new StaticProvider<Data<string>>("company name");
            var phoneNumberProvider = new List<PhoneNumberConstructor>()
            {
                new PhoneNumberConstructor(
                    new StaticProvider<Data<string>>("0412345678"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var emailAddressProvider = new List<EmailAddressConstructor>()
            {
                new EmailAddressConstructor(
                    new StaticProvider<Data<string>>("goog@gmail.com"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var streetAddressProvider = new List<StreetAddressConstructor>()
            {
                new StreetAddressConstructor(
                    new StaticProvider<Data<string>>("something address"),
                    new StaticProvider<Data<string>>("suburb"),
                    new StaticProvider<Data<string>>("ACT"),
                    new StaticProvider<Data<string>>("2601"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var websiteAddressProvider = new List<WebsiteAddressConstructor>()
            {
                new WebsiteAddressConstructor(
                    new StaticProvider<Data<string>>(website),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var messengerIdProvider = new List<MessengerIdConstructor>()
            {
                new MessengerIdConstructor(
                    new StaticProvider<Data<string>>("some title"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var socialMediaProvider = new List<SocialMediaIdConstructor>()
            {
                new SocialMediaIdConstructor(
                    new StaticProvider<Data<string>>("some title"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var organisationId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            var automationData = await MockAutomationData.CreateWithHttpTrigger(null, null, tenantId);
            IProviderContext providerContext = new ProviderContext(automationData);

            OrganisationEntityProvider organisationProvider = this.GetOrganisationProvider(tenantId, organisationId);
            var userId = Guid.NewGuid();
            var userService = this.GetCqrsMediatorWithUserService(tenantId, userId, organisationId);
            var createUserAction = new CreateUserAction(
                "Set variable",
                "setPropertyValue",
                string.Empty,
                false,
                null,
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<Action>(),
                accountEmailProvider,
                organisationProvider,
                null,
                null,
                null,
                new Mock<IAdditionalPropertyTransformHelper>().Object,
                new PersonConstructor(
                    firstNameProvider,
                    namePrefixProvider,
                    lastNameProvider,
                    middleNamesProvider,
                    nameSuffixProvider,
                    preferredNameProvider,
                    companyProvider,
                    titleProvider,
                    phoneNumberProvider,
                    emailAddressProvider,
                    streetAddressProvider,
                    websiteAddressProvider,
                    messengerIdProvider,
                    socialMediaProvider),
                userService,
                new Mock<IRoleRepository>().Object,
                new Mock<IClock>().Object);
            var actionData = createUserAction.CreateActionData();
            automationData.AddActionData(actionData);

            // Act
            await createUserAction.Execute(providerContext, actionData);

            // Assert
            (actionData as CreateUserActionData).UserId.Should().NotBeEmpty();
            (actionData as CreateUserActionData).AccountEmailAddress.Should().Be("test@gmail.com");
            (actionData as CreateUserActionData).OrganisationId.Should().Be(organisationId);
        }

        [Theory]
        [InlineData("jeo.talavera")]
        [InlineData("jeo180")]
        [InlineData("marianne101")]
        [InlineData("dzm20")]
        [InlineData("asd123")]
        [InlineData("datablitz_1")]
        public async Task CreateUserAction_ShouldSucceed_IfHasRightMessengerIdAndSocial(string messengerId)
        {
            // Arrange
            var accountEmailProvider = new StaticProvider<Data<string>>("test@gmail.com");
            var firstNameProvider = new StaticProvider<Data<string>>("jeo");
            var namePrefixProvider = new StaticProvider<Data<string>>("Jr.");
            var lastNameProvider = new StaticProvider<Data<string>>("talavera");
            var middleNamesProvider = new StaticProvider<Data<string>>("p");
            var nameSuffixProvider = new StaticProvider<Data<string>>("Mr.");
            var preferredNameProvider = new StaticProvider<Data<string>>("J");
            var titleProvider = new StaticProvider<Data<string>>("some title");
            var companyProvider = new StaticProvider<Data<string>>("company name");
            var phoneNumberProvider = new List<PhoneNumberConstructor>()
            {
                new PhoneNumberConstructor(
                    new StaticProvider<Data<string>>("0412345678"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var emailAddressProvider = new List<EmailAddressConstructor>()
            {
                new EmailAddressConstructor(
                    new StaticProvider<Data<string>>("goog@gmail.com"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var streetAddressProvider = new List<StreetAddressConstructor>()
            {
                new StreetAddressConstructor(
                    new StaticProvider<Data<string>>("something address"),
                    new StaticProvider<Data<string>>("suburb"),
                    new StaticProvider<Data<string>>("ACT"),
                    new StaticProvider<Data<string>>("2601"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var websiteAddressProvider = new List<WebsiteAddressConstructor>()
            {
                new WebsiteAddressConstructor(
                    new StaticProvider<Data<string>>("google.com"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var messengerIdProvider = new List<MessengerIdConstructor>()
            {
                new MessengerIdConstructor(
                    new StaticProvider<Data<string>>(messengerId),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var socialMediaProvider = new List<SocialMediaIdConstructor>()
            {
                new SocialMediaIdConstructor(
                    new StaticProvider<Data<string>>(messengerId),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };

            var organisationId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            var automationData = await MockAutomationData.CreateWithHttpTrigger(null, null, tenantId);
            IProviderContext providerContext = new ProviderContext(automationData);
            var additionalPropertyDefinitionRepository = new Mock<IAdditionalPropertyDefinitionRepository>().Object;
            OrganisationEntityProvider organisationProvider = this.GetOrganisationProvider(tenantId, organisationId);
            var userId = Guid.NewGuid();
            var userService = this.GetCqrsMediatorWithUserService(tenantId, userId, organisationId);
            var createUserAction = new CreateUserAction(
                "Set variable",
                "setPropertyValue",
                string.Empty,
                false,
                null,
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<Action>(),
                accountEmailProvider,
                organisationProvider,
                null,
                null,
                null,
                new Mock<IAdditionalPropertyTransformHelper>().Object,
                new PersonConstructor(
                    firstNameProvider,
                    namePrefixProvider,
                    lastNameProvider,
                    middleNamesProvider,
                    nameSuffixProvider,
                    preferredNameProvider,
                    companyProvider,
                    titleProvider,
                    phoneNumberProvider,
                    emailAddressProvider,
                    streetAddressProvider,
                    websiteAddressProvider,
                    messengerIdProvider,
                    socialMediaProvider),
                userService,
                new Mock<IRoleRepository>().Object,
                new Mock<IClock>().Object);
            var actionData = createUserAction.CreateActionData();
            automationData.AddActionData(actionData);

            // Act
            await createUserAction.Execute(providerContext, actionData);

            // Assert
            (actionData as CreateUserActionData).UserId.Should().NotBeEmpty();
            (actionData as CreateUserActionData).AccountEmailAddress.Should().Be("test@gmail.com");
            (actionData as CreateUserActionData).OrganisationId.Should().Be(organisationId);
        }

        [Theory]
        [InlineData("021")]
        [InlineData("02020202")]
        [InlineData("+61 1 5555 0000")]
        [InlineData("+61 1 2222 2222")]
        [InlineData("+61122222222")]
        [InlineData("+61123211222")]
        public async Task CreateUserAction_ShouldFail_IfHasWrongPhoneNumber(string phoneNumber)
        {
            // Arrange
            var accountEmailProvider = new StaticProvider<Data<string>>("test@gmail.com");
            var firstNameProvider = new StaticProvider<Data<string>>("jeo");
            var namePrefixProvider = new StaticProvider<Data<string>>("Jr.");
            var lastNameProvider = new StaticProvider<Data<string>>("talavera");
            var middleNamesProvider = new StaticProvider<Data<string>>("p");
            var nameSuffixProvider = new StaticProvider<Data<string>>("Mr.");
            var preferredNameProvider = new StaticProvider<Data<string>>("J");
            var titleProvider = new StaticProvider<Data<string>>("some title");
            var companyProvider = new StaticProvider<Data<string>>("company name");
            var phoneNumberProvider = new List<PhoneNumberConstructor>()
            {
                new PhoneNumberConstructor(
                    new StaticProvider<Data<string>>(phoneNumber),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var emailAddressProvider = new List<EmailAddressConstructor>()
            {
                new EmailAddressConstructor(
                    new StaticProvider<Data<string>>("jeo.talavera@gmail.com"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var streetAddressProvider = new List<StreetAddressConstructor>()
            {
                new StreetAddressConstructor(
                    new StaticProvider<Data<string>>("something address"),
                    new StaticProvider<Data<string>>("suburb"),
                    new StaticProvider<Data<string>>("ACT"),
                    new StaticProvider<Data<string>>("2601"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var websiteAddressProvider = new List<WebsiteAddressConstructor>()
            {
                new WebsiteAddressConstructor(
                    new StaticProvider<Data<string>>("www.google.com"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var messengerIdProvider = new List<MessengerIdConstructor>()
            {
                new MessengerIdConstructor(
                    new StaticProvider<Data<string>>("some title"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var socialMediaProvider = new List<SocialMediaIdConstructor>()
            {
                new SocialMediaIdConstructor(
                    new StaticProvider<Data<string>>("some title"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var organisationId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            var automationData = await MockAutomationData.CreateWithHttpTrigger(null, null, tenantId);
            IProviderContext providerContext = new ProviderContext(automationData);
            var additionalPropertyDefinitionRepository = new Mock<IAdditionalPropertyDefinitionRepository>().Object;
            OrganisationEntityProvider organisationProvider = this.GetOrganisationProvider(tenantId, organisationId);
            var userId = Guid.NewGuid();
            var userService = this.GetCqrsMediatorWithUserService(tenantId, userId, organisationId);
            var createUserAction = new CreateUserAction(
                "Set variable",
                "setPropertyValue",
                string.Empty,
                false,
                null,
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<Action>(),
                accountEmailProvider,
                organisationProvider,
                null,
                null,
                null,
                new Mock<IAdditionalPropertyTransformHelper>().Object,
                new PersonConstructor(
                    firstNameProvider,
                    namePrefixProvider,
                    lastNameProvider,
                    middleNamesProvider,
                    nameSuffixProvider,
                    preferredNameProvider,
                    companyProvider,
                    titleProvider,
                    phoneNumberProvider,
                    emailAddressProvider,
                    streetAddressProvider,
                    websiteAddressProvider,
                    messengerIdProvider,
                    socialMediaProvider),
                userService,
                new Mock<IRoleRepository>().Object,
                new Mock<IClock>().Object);
            var actionData = createUserAction.CreateActionData();
            automationData.AddActionData(actionData);

            // Act
            Func<Task> act = async () => await createUserAction.Execute(providerContext, actionData);

            // Assert
            (await act.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be($"person.phone.number.invalid");
        }

        [Theory]
        [InlineData("email@gmail.com@")]
        [InlineData("wo.wo@gmail@.co")]
        [InlineData("email")]
        [InlineData("wo20@p.p..11")]
        [InlineData("wel@come@one.z12")]
        public async Task CreateUserAction_ShouldFail_IfHasWrongEmailAddress(string emailAddress)
        {
            // Arrange
            var accountEmailProvider = new StaticProvider<Data<string>>("test@gmail.com");
            var firstNameProvider = new StaticProvider<Data<string>>("jeo");
            var namePrefixProvider = new StaticProvider<Data<string>>("Jr.");
            var lastNameProvider = new StaticProvider<Data<string>>("talavera");
            var middleNamesProvider = new StaticProvider<Data<string>>("p");
            var nameSuffixProvider = new StaticProvider<Data<string>>("Mr.");
            var preferredNameProvider = new StaticProvider<Data<string>>("J");
            var titleProvider = new StaticProvider<Data<string>>("some title");
            var companyProvider = new StaticProvider<Data<string>>("company name");
            var phoneNumberProvider = new List<PhoneNumberConstructor>()
            {
                new PhoneNumberConstructor(
                    new StaticProvider<Data<string>>("0412345678"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var emailAddressProvider = new List<EmailAddressConstructor>()
            {
                new EmailAddressConstructor(
                    new StaticProvider<Data<string>>(emailAddress),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var streetAddressProvider = new List<StreetAddressConstructor>()
            {
                new StreetAddressConstructor(
                    new StaticProvider<Data<string>>("something address"),
                    new StaticProvider<Data<string>>("suburb"),
                    new StaticProvider<Data<string>>("ACT"),
                    new StaticProvider<Data<string>>("2601"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var websiteAddressProvider = new List<WebsiteAddressConstructor>()
            {
                new WebsiteAddressConstructor(
                    new StaticProvider<Data<string>>("www.google.com"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var messengerIdProvider = new List<MessengerIdConstructor>()
            {
                new MessengerIdConstructor(
                    new StaticProvider<Data<string>>("some title"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var socialMediaProvider = new List<SocialMediaIdConstructor>()
            {
                new SocialMediaIdConstructor(
                    new StaticProvider<Data<string>>("some title"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var organisationId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            var automationData = await MockAutomationData.CreateWithHttpTrigger(null, null, tenantId);
            IProviderContext providerContext = new ProviderContext(automationData);
            OrganisationEntityProvider organisationProvider = this.GetOrganisationProvider(tenantId, organisationId);
            var userId = Guid.NewGuid();
            var userService = this.GetCqrsMediatorWithUserService(tenantId, userId, organisationId);
            var createUserAction = new CreateUserAction(
                "Set variable",
                "setPropertyValue",
                string.Empty,
                false,
                null,
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<Action>(),
                accountEmailProvider,
                organisationProvider,
                null,
                null,
                null,
                new Mock<IAdditionalPropertyTransformHelper>().Object,
                new PersonConstructor(
                    firstNameProvider,
                    namePrefixProvider,
                    lastNameProvider,
                    middleNamesProvider,
                    nameSuffixProvider,
                    preferredNameProvider,
                    companyProvider,
                    titleProvider,
                    phoneNumberProvider,
                    emailAddressProvider,
                    streetAddressProvider,
                    websiteAddressProvider,
                    messengerIdProvider,
                    socialMediaProvider),
                userService,
                new Mock<IRoleRepository>().Object,
                new Mock<IClock>().Object);
            var actionData = createUserAction.CreateActionData();
            automationData.AddActionData(actionData);

            // Act
            Func<Task> act = async () => await createUserAction.Execute(providerContext, actionData);

            // Assert
            (await act.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be($"person.email.address.invalid");
        }

        [Theory]
        [InlineData("general luna", "borca", "ACT", "26011")]
        [InlineData("mega street", "coach sub", "VIC", "4000")]
        [InlineData("wensh", "rigthon", "QLD", "5000")]
        [InlineData("billz", "misissipy", "SA", "6000")]
        [InlineData("Victoria", "connecticut", "WA", "5000")]
        [InlineData("Vulimia", "quezon", "TAS", "6999")]
        [InlineData("Vulimia", "quezon", "XZ", "7600")]
        public async Task CreateUserAction_ShouldFail_IfHasWrongStreetAddress(string address, string suburb, string state, string postcode)
        {
            // Arrange
            var accountEmailProvider = new StaticProvider<Data<string>>("test@gmail.com");
            var firstNameProvider = new StaticProvider<Data<string>>("jeo");
            var namePrefixProvider = new StaticProvider<Data<string>>("Jr.");
            var lastNameProvider = new StaticProvider<Data<string>>("talavera");
            var middleNamesProvider = new StaticProvider<Data<string>>("p");
            var nameSuffixProvider = new StaticProvider<Data<string>>("Mr.");
            var preferredNameProvider = new StaticProvider<Data<string>>("J");
            var titleProvider = new StaticProvider<Data<string>>("some title");
            var companyProvider = new StaticProvider<Data<string>>("company name");
            var phoneNumberProvider = new List<PhoneNumberConstructor>()
            {
                new PhoneNumberConstructor(
                    new StaticProvider<Data<string>>("0412345678"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var emailAddressProvider = new List<EmailAddressConstructor>()
            {
                new EmailAddressConstructor(
                    new StaticProvider<Data<string>>("qwe@gmail.com"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var streetAddressProvider = new List<StreetAddressConstructor>()
            {
                new StreetAddressConstructor(
                    new StaticProvider<Data<string>>(address),
                    new StaticProvider<Data<string>>(suburb),
                    new StaticProvider<Data<string>>(state),
                    new StaticProvider<Data<string>>(postcode),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var websiteAddressProvider = new List<WebsiteAddressConstructor>()
            {
                new WebsiteAddressConstructor(
                    new StaticProvider<Data<string>>("www.google.com"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var messengerIdProvider = new List<MessengerIdConstructor>()
            {
                new MessengerIdConstructor(
                    new StaticProvider<Data<string>>("some title"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var socialMediaProvider = new List<SocialMediaIdConstructor>()
            {
                new SocialMediaIdConstructor(
                    new StaticProvider<Data<string>>("some title"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var organisationId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            var automationData = await MockAutomationData.CreateWithHttpTrigger(null, null, tenantId);
            IProviderContext providerContext = new ProviderContext(automationData);
            var additionalPropertyDefinitionRepository = new Mock<IAdditionalPropertyDefinitionRepository>().Object;
            OrganisationEntityProvider organisationProvider = this.GetOrganisationProvider(tenantId, organisationId);
            var userId = Guid.NewGuid();
            var userService = this.GetCqrsMediatorWithUserService(tenantId, userId, organisationId);
            var createUserAction = new CreateUserAction(
                "Set variable",
                "setPropertyValue",
                string.Empty,
                false,
                null,
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<Action>(),
                accountEmailProvider,
                organisationProvider,
                null,
                null,
                null,
                new Mock<IAdditionalPropertyTransformHelper>().Object,
                new PersonConstructor(
                    firstNameProvider,
                    namePrefixProvider,
                    lastNameProvider,
                    middleNamesProvider,
                    nameSuffixProvider,
                    preferredNameProvider,
                    companyProvider,
                    titleProvider,
                    phoneNumberProvider,
                    emailAddressProvider,
                    streetAddressProvider,
                    websiteAddressProvider,
                    messengerIdProvider,
                    socialMediaProvider),
                userService,
                new Mock<IRoleRepository>().Object,
                new Mock<IClock>().Object);
            var actionData = createUserAction.CreateActionData();
            automationData.AddActionData(actionData);

            // Act
            Func<Task> act = async () => await createUserAction.Execute(new ProviderContext(automationData), actionData);

            // Assert
            var errorCode = (await act.Should().ThrowAsync<ErrorException>()).Which.Error.Code;
            if (errorCode.Contains("postcode"))
            {
                errorCode.Should().Be($"person.street.address.postcode.and.state.mismatch");
            }
            else
            {
                errorCode.Should().Be($"person.street.address.state.invalid");
            }
        }

        [Theory]
        [InlineData("htt:://invalid#example!1.com/abc%de&$^")]
        [InlineData("htt$ps://invalid_example5-url.com/<>?,./")]
        [InlineData("http:///example$invalid(4).com/][]{}|;':\"?")]
        [InlineData("preston smash ")]
        public async Task CreateUserAction_ShouldFail_IfHasWrongWebsite(string website)
        {
            // Arrange
            var accountEmailProvider = new StaticProvider<Data<string>>("test@gmail.com");
            var firstNameProvider = new StaticProvider<Data<string>>("jeo");
            var namePrefixProvider = new StaticProvider<Data<string>>("Jr.");
            var lastNameProvider = new StaticProvider<Data<string>>("talavera");
            var middleNamesProvider = new StaticProvider<Data<string>>("p");
            var nameSuffixProvider = new StaticProvider<Data<string>>("Mr.");
            var preferredNameProvider = new StaticProvider<Data<string>>("J");
            var titleProvider = new StaticProvider<Data<string>>("some title");
            var companyProvider = new StaticProvider<Data<string>>("company name");
            var phoneNumberProvider = new List<PhoneNumberConstructor>()
            {
                new PhoneNumberConstructor(
                    new StaticProvider<Data<string>>("0412345678"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var emailAddressProvider = new List<EmailAddressConstructor>()
            {
                new EmailAddressConstructor(
                    new StaticProvider<Data<string>>("goog@gmail.com"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var streetAddressProvider = new List<StreetAddressConstructor>()
            {
                new StreetAddressConstructor(
                    new StaticProvider<Data<string>>("something address"),
                    new StaticProvider<Data<string>>("suburb"),
                    new StaticProvider<Data<string>>("ACT"),
                    new StaticProvider<Data<string>>("2601"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var websiteAddressProvider = new List<WebsiteAddressConstructor>()
            {
                new WebsiteAddressConstructor(
                    new StaticProvider<Data<string>>(website),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var messengerIdProvider = new List<MessengerIdConstructor>()
            {
                new MessengerIdConstructor(
                    new StaticProvider<Data<string>>("some title"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var socialMediaProvider = new List<SocialMediaIdConstructor>()
            {
                new SocialMediaIdConstructor(
                    new StaticProvider<Data<string>>("some title"),
                    new StaticProvider<Data<string>>("test label"),
                    new StaticProvider<Data<bool>>(false)),
            };
            var organisationId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            var automationData = await MockAutomationData.CreateWithHttpTrigger(null, null, tenantId);
            IProviderContext providerContext = new ProviderContext(automationData);
            var additionalPropertyDefinitionRepository = new Mock<IAdditionalPropertyDefinitionRepository>().Object;
            OrganisationEntityProvider organisationProvider = this.GetOrganisationProvider(tenantId, organisationId);
            var userId = Guid.NewGuid();
            var userService = this.GetCqrsMediatorWithUserService(tenantId, userId, organisationId);
            var createUserAction = new CreateUserAction(
                "Set variable",
                "setPropertyValue",
                string.Empty,
                false,
                null,
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<Action>(),
                accountEmailProvider,
                organisationProvider,
                null,
                null,
                null,
                new Mock<IAdditionalPropertyTransformHelper>().Object,
                new PersonConstructor(
                    firstNameProvider,
                    namePrefixProvider,
                    lastNameProvider,
                    middleNamesProvider,
                    nameSuffixProvider,
                    preferredNameProvider,
                    companyProvider,
                    titleProvider,
                    phoneNumberProvider,
                    emailAddressProvider,
                    streetAddressProvider,
                    websiteAddressProvider,
                    messengerIdProvider,
                    socialMediaProvider),
                userService,
                new Mock<IRoleRepository>().Object,
                new Mock<IClock>().Object);
            var actionData = createUserAction.CreateActionData();
            automationData.AddActionData(actionData);

            // Act
            Func<Task> act = async () => await createUserAction.Execute(providerContext, actionData);

            // Assert
            (await act.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be($"person.website.address.invalid");
        }

        private IAdditionalPropertyValueService GetAdditionalPropertyValueService(Guid tenantId)
        {
            var additionalPropertyDefinitionRepositoryMock = new Mock<IAdditionalPropertyDefinitionRepository>();
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
            return new FakeAdditionalPropertyValueService(additionalPropertyDefinitionRepositoryMock.Object);
        }

        private OrganisationEntityProvider GetOrganisationProvider(Guid tenantId, Guid organisationId)
        {
            var organisation = new OrganisationReadModel(
                    tenantId,
                    organisationId,
                    "sampleAlias",
                    "sampleName",
                    null,
                    true,
                    false,
                    new TestClock(true).Timestamp);
            var organisationReadModelRepository = new Mock<IOrganisationReadModelRepository>();
            var organisationReadModelWithRelatedEntities = new OrganisationReadModelWithRelatedEntities()
            {
                Organisation = organisation,
                TextAdditionalPropertiesValues = new List<TextAdditionalPropertyValueReadModel>(),
                StructuredDataAdditionalPropertyValues = new List<StructuredDataAdditionalPropertyValueReadModel>(),
                Tenant = new Domain.Tenant(tenantId),
                TenantDetails = new List<TenantDetails>() { },
            };
            organisationReadModelRepository.Setup(x => x.GetOrganisationWithRelatedEntities(tenantId, organisationId, new List<string>()))
                .Returns(organisationReadModelWithRelatedEntities);
            organisationReadModelRepository.Setup(x => x.Get(tenantId, organisationId))
               .Returns(organisation);
            var entityFactory = new Mock<ISerialisedEntityFactory>();
            entityFactory.Setup(x => x.Create(organisationReadModelWithRelatedEntities, new List<string>()))
                .Returns(Task.FromResult(new Organisation(organisationReadModelWithRelatedEntities.Organisation) as IEntity));
            var organisationProvider = new OrganisationEntityProvider(
                new StaticProvider<Data<string>>(organisationId.ToString()),
                organisationReadModelRepository.Object,
                entityFactory.Object);

            return organisationProvider;
        }

        private ICqrsMediator GetCqrsMediatorWithUserService(Guid tenantId, Guid userId, Guid organisationId)
        {
            var services = UserHelper.GetServiceCollectionWithUserService(tenantId, userId, organisationId);
            var serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetRequiredService<ICqrsMediator>();
            return mediator;
        }
    }
}
