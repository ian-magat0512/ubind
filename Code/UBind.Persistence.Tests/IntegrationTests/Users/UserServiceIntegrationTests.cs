// <copyright file="UserServiceIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.IntegrationTests.Users
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Humanizer;
    using NodaTime;
    using UBind.Application.Commands.Tenant;
    using UBind.Application.Queries.Tenant;
    using UBind.Application.User;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Person.Fields;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Tests;
    using UBind.Domain.Tests.Fakes;
    using UBind.Domain.ValueTypes;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class UserServiceIntegrationTests
    {
        private readonly ApplicationStack stack;
        private readonly Guid? performingUserId = Guid.NewGuid();

        private Guid tenantId;
        private Guid productId;

        public UserServiceIntegrationTests()
        {
            this.stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            this.productId = Guid.NewGuid();
            this.tenantId = Guid.NewGuid();
        }

        public static IClock Clock { get; set; } = SystemClock.Instance;

        [Fact(Skip = "Rewrite this test to use appropriate mocks so that it's quick, e.g. <50ms")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task UserService_CapsQuoteListsAt1000()
        {
            // Arrange
            var tenantProductModel = await this.CreateProductAndTenant(Guid.NewGuid());
            var tenant = tenantProductModel.Tenant;
            var filters = new UserReadModelFilters();

            var clock = SystemClock.Instance;
            var userType = UserType.Master;
            var defaultOrganisationId = tenant.Details.DefaultOrganisationId;
            for (var i = 0; i < 1010; i++)
            {
                var email = "jeo" + i.ToString() + "@email.com";
                var person = PersonAggregate.CreatePerson(
                    tenant.Id, defaultOrganisationId, this.performingUserId, clock.Now());
                var personCommonProperties = new PersonCommonProperties
                {
                    Email = email,
                };

                person.Update(new PersonalDetails(tenant.Id, personCommonProperties), this.performingUserId, this.stack.Clock.Now());
                await this.stack.PersonAggregateRepository.Save(person);
                var userId = Guid.NewGuid();
                var persistedUserAggregate = UserAggregate.CreateUser(
                    person.TenantId, userId, userType, person, this.performingUserId, null, clock.Now());
                await this.stack.UserAggregateRepository.Save(persistedUserAggregate);
            }

            filters.OrganisationIds = new List<Guid> { tenant.Details.DefaultOrganisationId };

            // Act
            var users = this.stack.UserReadModelRepository.GetUsers(
                tenant.Id,
                filters);

            // Assert
            users.Should().HaveCount(1000);
        }

        [Fact]
        public async Task UserService_GetUsers_ShouldReturnListUnderSpecificTenancyAndOrganisation()
        {
            // Arrange
            var tenantProductModel = await this.CreateProductAndTenant(Guid.NewGuid());
            var tenant = tenantProductModel.Tenant;
            var clock = SystemClock.Instance;
            var filters = new UserReadModelFilters();
            var userType = UserType.Master;
            var email = $"{Guid.NewGuid()}@email.com";
            var defaultOrganisationId = tenant.Details.DefaultOrganisationId;
            var person = PersonAggregate.CreatePerson(
                tenant.Id, defaultOrganisationId, this.performingUserId, clock.Now());
            person.UpdateEmail(email, this.performingUserId, clock.Now());
            await this.stack.PersonAggregateRepository.Save(person);
            var userId = Guid.NewGuid();
            var persistedUserAggregate = UserAggregate.CreateUser(
                person.TenantId, userId, userType, person, this.performingUserId, null, clock.Now());
            await this.stack.UserAggregateRepository.Save(persistedUserAggregate);
            filters.OrganisationIds = new List<Guid> { tenant.Details.DefaultOrganisationId };

            // Act
            var users = this.stack.UserReadModelRepository.GetUsers(tenant.Id, filters);

            users.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task UserService_GetUsers_ShouldReturnEmptyForDifferentTenancyAndOrganisation()
        {
            // Arrange
            var tenantProductModel = await this.CreateProductAndTenant(Guid.NewGuid());
            var tenant = tenantProductModel.Tenant;
            var clock = SystemClock.Instance;
            var filters = new UserReadModelFilters();
            var userType = UserType.Master;

            var differentTenant = TenantFactory.Create(Guid.NewGuid());
            this.stack.TenantRepository.Insert(differentTenant);

            this.stack.TenantRepository.SaveChanges();

            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id, tenant.Details.Alias, tenant.Details.Name, null, Guid.NewGuid(), clock.GetCurrentInstant());
            await this.stack.OrganisationAggregateRepository.Save(organisation);
            tenant.SetDefaultOrganisation(organisation.Id, clock.GetCurrentInstant().Plus(Duration.FromMinutes(1)));
            this.stack.TenantRepository.SaveChanges();

            var email = $"{Guid.NewGuid()}@email.com";
            var defaultOrganisationId = tenant.Details.DefaultOrganisationId;
            var person = PersonAggregate.CreatePerson(
                tenant.Id, defaultOrganisationId, this.performingUserId, clock.Now());
            person.UpdateEmail(email, this.performingUserId, clock.Now());
            await this.stack.PersonAggregateRepository.Save(person);
            var userId = Guid.NewGuid();
            var persistedUserAggregate = UserAggregate.CreateUser(
                person.TenantId, userId, userType, person, this.performingUserId, null, clock.Now());
            await this.stack.UserAggregateRepository.Save(persistedUserAggregate);
            filters.OrganisationIds = new List<Guid> { Guid.NewGuid() };

            // Act
            var users = this.stack.UserReadModelRepository.GetUsers(differentTenant.Id, filters);

            users.Should().BeNullOrEmpty();
        }

        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public async Task UserService_CreateUserForQuoteCustomer_CreateCustomerUser_With_Customer_in_Quote()
        {
            // Arrange
            var clock = SystemClock.Instance;
            var tenantProductModel = await this.CreateProductAndTenant(Guid.NewGuid());
            var tenant = tenantProductModel.Tenant;

            // link quote to person and customer
            var email = $"{Guid.NewGuid()}@email.com";
            var person = PersonAggregate.CreatePerson(tenant.Id, tenant.Details.DefaultOrganisationId, this.performingUserId, clock.Now());
            person.UpdateEmail(email, this.performingUserId, clock.Now());
            person.UpdateFullName("sample-fullname", this.performingUserId, clock.Now());
            var customerAggregate = CustomerAggregate.CreateNewCustomer(
                tenant.Id, person, QuoteFactory.DefaultEnvironment, this.performingUserId, null, Clock.Now());

            // edited to create quote with policy to avoid
            var quoteAggregate = QuoteFactory.CreateNewPolicy(
                this.tenantId,
                this.productId,
                policyNumber: "POLNUM2");
            var adjustmentQuote = quoteAggregate.WithAdjustmentQuote();
            var quote = quoteAggregate.GetQuoteOrThrow(adjustmentQuote.Id);
            quoteAggregate = adjustmentQuote.Aggregate;
            quoteAggregate.WithCalculationResult(adjustmentQuote.Id);
            quoteAggregate.RecordAssociationWithCustomer(customerAggregate, person, Guid.NewGuid(), Clock.Now());

            await this.stack.PersonAggregateRepository.Save(person);
            await this.stack.CustomerAggregateRepository.Save(customerAggregate);
            await this.stack.QuoteAggregateRepository.Save(quoteAggregate); // Complete
            var signUpModel = new UserSignupModel()
            {
                Environment = QuoteFactory.DefaultEnvironment,
                UserType = UserType.Customer,
                FullName = person.FullName,
                PreferredName = person.PreferredName ?? person.FullName,
                MobilePhoneNumber = person.MobilePhone,
                HomePhoneNumber = person.HomePhone,
                Email = person.Email,
                TenantId = tenant.Id,
                OrganisationId = tenant.Details.DefaultOrganisationId,
            };

            // Act
            var userDto = await this.stack.UserService.CreateUserForQuoteCustomer(
                    signUpModel,
                    quote.Id);

            // Assert
            userDto.UserType.Should().Be(UserType.Customer.Humanize());
        }

        [Fact]
        public async Task UserService_CreateUserForQuoteCustomer_CreateCustomerUser_With_Out_Customer_in_Quote_Throws_Exception()
        {
            // Arrange
            var tenantProductModel = await this.CreateProductAndTenant(Guid.NewGuid());
            var tenant = tenantProductModel.Tenant;
            var product = tenantProductModel.Product;
            var quote = QuoteFactory.CreateNewBusinessQuote(tenant.Id, product.Id);
            var quoteAggregate = quote.Aggregate
                .WithCalculationResult(quote.Id);

            // link quote to person and customer
            var person = QuoteFactory.CreatePersonAggregate(tenant.Id);
            await this.stack.QuoteAggregateRepository.Save(quoteAggregate); // Complete
            var signUpModel = new UserSignupModel()
            {
                Environment = QuoteFactory.DefaultEnvironment,
                UserType = UserType.Customer,
                FullName = person.FullName,
                PreferredName = person.PreferredName ?? person.FullName,
                MobilePhoneNumber = person.MobilePhone,
                HomePhoneNumber = person.HomePhone,
                Email = person.Email,
                TenantId = tenant.Id,
            };

            // Act
            Func<Task<UserModel>> func = async () => await this.stack.UserService.CreateUserForQuoteCustomer(
                    signUpModel,
                    quote.Id);

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Code.Should().Be("cannot.create.user.for.non.existent.customer");
        }

        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public async Task UserService_CreateUserForQuoteCustomer_CreateCustomerUser_With_CustomerInQuote_MustBeUpdated_WithPassedDetails()
        {
            // Arrange
            var clock = SystemClock.Instance;
            var tenantProductModel = await this.CreateProductAndTenant(Guid.NewGuid());
            var tenant = tenantProductModel.Tenant;
            var product = tenantProductModel.Product;

            // link quote to person and customer
            var person = PersonAggregate.CreatePerson(tenant.Id, tenant.Details.DefaultOrganisationId, this.performingUserId, clock.Now());
            var customerAggregate = CustomerAggregate.CreateNewCustomer(
                tenant.Id, person, QuoteFactory.DefaultEnvironment, this.performingUserId, null, Clock.Now());
            var quoteAggregate = QuoteFactory.CreateNewPolicy(
                this.tenantId,
                this.productId);
            var adjustmentQuote = quoteAggregate.WithAdjustmentQuote();
            quoteAggregate = adjustmentQuote.Aggregate;
            var quote = quoteAggregate.GetQuoteOrThrow(adjustmentQuote.Id);
            quoteAggregate.WithCalculationResult(quote.Id);
            quoteAggregate.RecordAssociationWithCustomer(customerAggregate, person, this.performingUserId, Clock.Now());
            await this.stack.PersonAggregateRepository.Save(person);
            await this.stack.CustomerAggregateRepository.Save(customerAggregate);
            await this.stack.QuoteAggregateRepository.Save(quoteAggregate); // Complete

            var signUpModel = new UserSignupModel()
            {
                Environment = QuoteFactory.DefaultEnvironment,
                UserType = UserType.Customer,
                FullName = "Minerva McGonagall",
                PreferredName = "McGonagall",
                MobilePhoneNumber = "04 8765 4321",
                HomePhoneNumber = "04 8765 4321",
                Email = "minerva.mcgonagall@test.com",
                TenantId = tenant.Id,
            };

            // Act
            var userDto = await this.stack.UserService.CreateUserForQuoteCustomer(
                    signUpModel,
                    quote.Id);

            // Assert
            userDto.UserType.Should().Be(UserType.Customer.Humanize());
            userDto.FullName.Should().Be("Minerva McGonagall");
            userDto.Email.Should().Be("minerva.mcgonagall@test.com");
        }

        [Fact]
        public async Task CreateUser_ShouldCreateInvitedUser_WhenSendActivationInvitationIsTrue()
        {
            // Arrange
            var tenantProductModel = await this.CreateProductAndTenant(Guid.NewGuid());
            var tenant = tenantProductModel.Tenant;
            var clock = SystemClock.Instance;
            var email = "chris.valmoria@ubind.io";
            var person = PersonAggregate.CreatePerson(
                tenant.Id, tenant.Details.DefaultOrganisationId, this.performingUserId, clock.Now());
            var personCommonProperties = new PersonCommonProperties
            {
                Email = email,
            };

            person.Update(new PersonalDetails(tenant.Id, personCommonProperties), this.performingUserId, this.stack.Clock.Now());
            await this.stack.PersonAggregateRepository.Save(person);
            var emailAddresses = new List<EmailAddressField>();
            emailAddresses.Add(new EmailAddressField("home", email, new EmailAddress(email)));

            UserSignupModel userSignupModel = new UserSignupModel
            {
                SendActivationInvitation = true,
                Password = null,
                Environment = QuoteFactory.DefaultEnvironment,
                UserType = UserType.Client,
                FullName = person.FullName,
                PreferredName = person.PreferredName ?? person.FullName,
                MobilePhoneNumber = person.MobilePhone,
                HomePhoneNumber = person.HomePhone,
                Email = person.Email,
                TenantId = tenant.Id,
                OrganisationId = tenant.Details.DefaultOrganisationId,
            };

            var userDto = await this.stack.UserService.CreateUser(userSignupModel);

            // Act
            var user = this.stack.UserReadModelRepository.GetUser(
                tenant.Id,
                userDto.Id);

            // Assert
            user.HasBeenInvitedToActivate.Should().BeTrue();
            user.HasBeenActivated.Should().BeFalse();
        }

        [Fact]
        public async Task CreateUser_ShouldCreateActivatedUser_WhenSendActivationInvitationIsFalseAndPasswordIsStrong()
        {
            // Arrange
            var tenantProductModel = await this.CreateProductAndTenant(Guid.NewGuid());
            var tenant = tenantProductModel.Tenant;
            var clock = SystemClock.Instance;

            var email = "foo@bar.com";
            var person = PersonAggregate.CreatePerson(
                tenant.Id, tenant.Details.DefaultOrganisationId, this.performingUserId, clock.Now());
            var personCommonProperties = new PersonCommonProperties
            {
                Email = email,
            };

            person.Update(new PersonalDetails(tenant.Id, personCommonProperties), this.performingUserId, this.stack.Clock.Now());
            await this.stack.PersonAggregateRepository.Save(person);
            var emailAddresses = new List<EmailAddressField>();
            emailAddresses.Add(new EmailAddressField("home", email, new EmailAddress(email)));

            UserSignupModel userSignupModel = new UserSignupModel
            {
                SendActivationInvitation = false,
                Password = "strongPass88*",
                Environment = QuoteFactory.DefaultEnvironment,
                UserType = UserType.Customer,
                FullName = person.FullName,
                PreferredName = person.PreferredName ?? person.FullName,
                MobilePhoneNumber = person.MobilePhone,
                HomePhoneNumber = person.HomePhone,
                Email = person.Email,
                TenantId = tenant.Id,
                OrganisationId = tenant.Details.DefaultOrganisationId,
            };

            var userDto = await this.stack.UserService.CreateUser(userSignupModel);

            // Act
            var user = this.stack.UserReadModelRepository.GetUser(
                tenant.Id,
                userDto.Id);

            // Assert
            user.HasBeenInvitedToActivate.Should().BeFalse();
            user.HasBeenActivated.Should().BeTrue();
        }

        [Fact]
        public async Task CreateUser_ShouldThowException_WhenSendActivationInvitationIsFalseAndPasswordIsWeak()
        {
            // Arrange
            var tenantProductModel = await this.CreateProductAndTenant(Guid.NewGuid());
            var tenant = tenantProductModel.Tenant;
            var product = tenantProductModel.Product;
            var clock = SystemClock.Instance;

            var email = "chris.valmoria@ubind.io";
            var person = PersonAggregate.CreatePerson(
                tenant.Id, tenant.Details.DefaultOrganisationId, this.performingUserId, clock.Now());
            var personCommonProperties = new PersonCommonProperties
            {
                Email = email,
            };

            person.Update(new PersonalDetails(tenant.Id, personCommonProperties), this.performingUserId, this.stack.Clock.Now());
            await this.stack.PersonAggregateRepository.Save(person);
            var emailAddresses = new List<EmailAddressField>();
            emailAddresses.Add(new EmailAddressField("home", email, new EmailAddress(email)));

            UserSignupModel userSignupModel = new UserSignupModel
            {
                SendActivationInvitation = false,
                Password = "weakPass",
                Environment = QuoteFactory.DefaultEnvironment,
                UserType = UserType.Customer,
                FullName = person.FullName,
                PreferredName = person.PreferredName ?? person.FullName,
                MobilePhoneNumber = person.MobilePhone,
                HomePhoneNumber = person.HomePhone,
                Email = person.Email,
                TenantId = tenant.Id,
                OrganisationId = tenant.Details.DefaultOrganisationId,
            };

            // Act
            Func<Task> act = async () => await this.stack.UserService.CreateUser(userSignupModel);

            // Assert
            await act.Should().ThrowAsync<ErrorException>();
        }

        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public async Task UserService_CreateUser_WithNewAndCustomFields_WithPassedDetails()
        {
            // Arrange
            var tenantProductModel = await this.CreateProductAndTenant(Guid.NewGuid());
            var tenant = tenantProductModel.Tenant;

            var signUpModel = new UserSignupModel()
            {
                Environment = QuoteFactory.DefaultEnvironment,
                UserType = UserType.Client,
                FullName = "Jose Rizal",
                NamePrefix = "Dr",
                FirstName = "Jose",
                MiddleNames = "Mercado Protacio",
                LastName = "Rizal",
                NameSuffix = "Jr",
                PreferredName = "jose",
                Company = "Microhard",
                Title = "QA Especialist",
                Email = "jose@test.com",
                TenantId = tenant.Id,
                OrganisationId = tenant.Details.DefaultOrganisationId,
                EmailAddresses = new List<EmailAddressField>
                {
                    new EmailAddressField("home", string.Empty, new EmailAddress("email001@test.com")),
                    new EmailAddressField("work", string.Empty, new EmailAddress("email002@test.com")),
                },
                PhoneNumbers = new List<PhoneNumberField>
                {
                    new PhoneNumberField("home", string.Empty, new PhoneNumber("0412341234")),
                    new PhoneNumberField("custom", "Home and Office", new PhoneNumber("0412341235")),
                },
                StreetAddresses = new List<StreetAddressField>
                {
                    new StreetAddressField("home", string.Empty, new Address
                    {
                        Line1 = "line1 test",
                        Suburb = "suburb1 test",
                        Postcode = "1000",
                        State = State.ACT,
                    }),
                    new StreetAddressField("office", string.Empty, new Address
                    {
                        Line1 = "line2 test",
                        Suburb = "suburb2 test",
                        Postcode = "4000",
                        State = State.QLD,
                    }),
                },
                WebsiteAddresses = new List<WebsiteAddressField>
                {
                    new WebsiteAddressField("personal", string.Empty, new WebAddress("www.web1.com")),
                    new WebsiteAddressField("custom", "company", new WebAddress("www.web2.com")),
                },
                MessengerIds = new List<MessengerIdField>
                {
                    new MessengerIdField("skype", string.Empty, "Messenger001"),
                    new MessengerIdField("viber", string.Empty, "Messenger002"),
                },
                SocialMediaIds = new List<SocialMediaIdField>
                {
                    new SocialMediaIdField("facebook", string.Empty, "Social1"),
                    new SocialMediaIdField("tweeter", string.Empty, "Social2"),
                },
            };

            // Act
            var userDto = await this.stack.UserService.CreateUser(signUpModel);

            // Assert
            var savedUserDto = this.stack.UserService.GetUser(userDto.TenantId, userDto.Id);
            var savedPerson = this.stack.PersonReadModelRepository.GetPersonSummaryById(savedUserDto.TenantId, savedUserDto.PersonId);
            var repeatingEmails = savedPerson.EmailAddresses.ToList();
            var repeatingPhoneNumbers = savedPerson.PhoneNumbers.ToList();
            var repeatingAddresses = savedPerson.StreetAddresses.ToList();
            var repeatingWebAddresses = savedPerson.WebsiteAddresses.ToList();
            var repeatingMessengers = savedPerson.MessengerIds.ToList();
            var repeatingSocials = savedPerson.SocialMediaIds.ToList();

            savedUserDto.FullName.Should().Be("Jose Rizal");
            savedUserDto.NamePrefix.Should().Be("Dr");
            savedUserDto.FirstName.Should().Be("Jose");
            savedUserDto.MiddleNames.Should().Be("Mercado Protacio");
            savedUserDto.LastName.Should().Be("Rizal");
            savedUserDto.NameSuffix.Should().Be("Jr");
            savedUserDto.Company.Should().Be("Microhard");
            savedUserDto.Title.Should().Be("QA Especialist");

            repeatingEmails.Should().HaveCount(2);
            repeatingEmails[0].EmailAddressValueObject.Should().Be(new EmailAddress("email001@test.com"));
            repeatingEmails[0].Label.Should().Be("home");
            repeatingEmails[1].EmailAddressValueObject.Should().Be(new EmailAddress("email002@test.com"));
            repeatingEmails[1].Label.Should().Be("work");

            repeatingPhoneNumbers.Should().HaveCount(2);
            repeatingPhoneNumbers[0].PhoneNumberValueObject.Should().Be(new PhoneNumber("0412341234"));
            repeatingPhoneNumbers[0].Label.Should().Be("home");
            repeatingPhoneNumbers[1].PhoneNumberValueObject.Should().Be(new PhoneNumber("0412341235"));
            repeatingPhoneNumbers[1].Label.Should().Be("custom");
            repeatingPhoneNumbers[1].CustomLabel.Should().Be("Home and Office");

            repeatingAddresses.Should().HaveCount(2);
            repeatingAddresses[0].StreetAddressValueObject.Should()
                .Equals(new Address
                {
                    Line1 = "line1 test",
                    Suburb = "suburb1 test",
                    Postcode = "1000",
                    State = State.ACT,
                });
            repeatingAddresses[0].Label.Should().Be("home");
            repeatingAddresses[1].StreetAddressValueObject.Should()
                 .Equals(new Address
                 {
                     Line1 = "line2 test",
                     Suburb = "suburb2 test",
                     Postcode = "4000",
                     State = State.QLD,
                 });
            repeatingAddresses[1].Label.Should().Be("office");

            repeatingWebAddresses.Should().HaveCount(2);
            repeatingWebAddresses[0].WebsiteAddressValueObject.Should().Be(new WebAddress("www.web1.com"));
            repeatingWebAddresses[0].Label.Should().Be("personal");
            repeatingWebAddresses[1].WebsiteAddressValueObject.Should().Be(new WebAddress("www.web2.com"));
            repeatingWebAddresses[1].Label.Should().Be("custom");
            repeatingWebAddresses[1].CustomLabel.Should().Be("company");

            repeatingMessengers.Should().HaveCount(2);
            repeatingMessengers[0].MessengerId.Should().Be("Messenger001");
            repeatingMessengers[0].Label.Should().Be("skype");
            repeatingMessengers[1].MessengerId.Should().Be("Messenger002");
            repeatingMessengers[1].Label.Should().Be("viber");

            repeatingSocials.Should().HaveCount(2);
            repeatingSocials[0].SocialMediaId.Should().Be("Social1");
            repeatingSocials[0].Label.Should().Be("facebook");
            repeatingSocials[1].SocialMediaId.Should().Be("Social2");
            repeatingSocials[1].Label.Should().Be("tweeter");
        }

        [Fact]
        public async Task CreateUser_ShouldNotCreateUserWhenInvalidPhoneNumber()
        {
            // Arrange
            var environment = QuoteFactory.DefaultEnvironment;
            var tenant = TenantFactory.Create(Guid.NewGuid());
            var product = ProductFactory.Create(tenant.Id, Guid.NewGuid());
            var currentInstance = SystemClock.Instance.GetCurrentInstant();

            var tenantAlias = tenant.Details.Alias;
            var tenantName = tenant.Details.Name;
            var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                tenant.Id, tenantAlias, tenantName, null, this.performingUserId, currentInstance);
            var anotherOrganisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                tenant.Id, tenantAlias, tenantName, null, this.performingUserId, currentInstance);

            this.stack.TenantRepository.Insert(tenant);
            this.stack.TenantRepository.SaveChanges();
            this.stack.ProductRepository.Insert(product);
            this.stack.ProductRepository.SaveChanges();

            await this.stack.OrganisationAggregateRepository.Save(organisation);
            tenant.SetDefaultOrganisation(organisation.Id, currentInstance.Plus(Duration.FromMinutes(1)));
            this.stack.TenantRepository.SaveChanges();

            await this.stack.OrganisationAggregateRepository.Save(anotherOrganisation);

            var email = "tester" + Guid.NewGuid() + "@ubind.io";

            var phoneNumbers = new List<PhoneNumberField>
            {
                new PhoneNumberField { PhoneNumber = "02 8503 8000", Label = "AU" }, // AU
                new PhoneNumberField { PhoneNumber = "+1 541-754-3010", Label = "US" }, // US
                new PhoneNumberField { PhoneNumber = "+44 20 1234 5678", Label = "UK" }, // UK
                new PhoneNumberField { PhoneNumber = "+1 416-555-1234", Label = "CA" }, // CA
                new PhoneNumberField { PhoneNumber = "+61 2 1234 5678", Label = "AU" }, // AU
                new PhoneNumberField { PhoneNumber = "+49 30 12345678", Label = "DE" }, // DE
                new PhoneNumberField { PhoneNumber = "+33 1 23 45 67 89", Label = "FR" } // FR
            };

            var userSignupModel = new UserSignupModel
            {
                AlternativeEmail = email,
                WorkPhoneNumber = "123",
                Email = email,
                Environment = environment,
                FullName = "john doe",
                HomePhoneNumber = "123",
                MobilePhoneNumber = "123",
                PhoneNumbers = phoneNumbers,
                PreferredName = "john",
                UserType = UserType.Client,
                TenantId = tenant.Id,
                OrganisationId = organisation.Id,
            };

            // Act
            Func<Task> act = async () => await this.stack.UserService.CreateUser(userSignupModel);

            // Assert
            var exception = await Record.ExceptionAsync(act);
            (exception as ErrorException)?.Error.Code.Should().NotBe("person.phone.number.invalid");
        }

        private async Task<TenantProductModel> CreateProductAndTenant(Guid productId)
        {
            var guid = Guid.NewGuid();
            var tenantAlias = "test-tenant-" + guid;
            var tenantName = "Test Tenant " + guid;
            var tenantId = await this.stack.Mediator.Send(new CreateTenantCommand(tenantName, tenantAlias, null));
            var tenant = await this.stack.Mediator.Send(new GetTenantByIdQuery(tenantId));
            var product = ProductFactory.Create(tenant.Id, productId);
            this.stack.CreateProduct(product);

            return new TenantProductModel
            {
                Tenant = tenant,
                Product = product,
            };
        }

        private class TenantProductModel
        {
#pragma warning disable SA1401 // Fields should be private
            public Tenant Tenant;
            public Domain.Product.Product Product;
#pragma warning restore SA1401 // Fields should be private
        }
    }
}
