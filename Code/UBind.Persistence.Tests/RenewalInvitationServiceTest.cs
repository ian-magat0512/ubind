// <copyright file="RenewalInvitationServiceTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using NodaTime;
    using UBind.Application.Commands.Tenant;
    using UBind.Application.Queries.Tenant;
    using UBind.Application.Tests;
    using UBind.Application.User;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Authentication;
    using UBind.Domain.Entities;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Models;
    using UBind.Domain.Quote;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using UBind.Domain.Tests.Helpers;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class RenewalInvitationServiceTest
    {
        private readonly ApplicationStack stack;
        private readonly Guid? performingUserId = Guid.NewGuid();

        public RenewalInvitationServiceTest()
        {
            this.stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
        }

        public IClock Clock { get; } = SystemClock.Instance;

        [Fact]
        public async Task Test_SendPolicyRenewalInvitation_CreateEmail()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var tenantProductModel = await this.CreateProductAndTenant(productId);
            var tenant = tenantProductModel.Tenant;

            var organisation = Organisation.CreateNewOrganisation(
                 tenant.Id, tenant.Details.Alias, tenant.Details.Name, null, Guid.NewGuid(), this.Clock.GetCurrentInstant());
            await this.stack.OrganisationAggregateRepository.Save(organisation);

            tenant.SetDefaultOrganisation(
                organisation.Id, this.Clock.GetCurrentInstant().Plus(Duration.FromMinutes(1)));
            this.stack.TenantRepository.SaveChanges();
            var user = await this.CreateUserModelForTenant("test1-ris@ubind.io", tenant);
            var userAuthenticationData = new UserAuthenticationData(
                tenant.Id, tenant.Details.DefaultOrganisationId, UserType.Client, user.Id, Guid.Empty);

            var customerUser = await this.CreateUserModelForTenant("customer1@ubind.io", tenant);
            var customerUserAuthenticationData = new UserAuthenticationData(
                tenant.Id, tenant.Details.DefaultOrganisationId, UserType.Client, customerUser.Id, Guid.Empty);
            var productFeatureService = new Mock<IProductFeatureSettingService>();

            var customerPerson = PersonAggregate.CreatePerson(
                tenant.Id, tenant.Details.DefaultOrganisationId, this.performingUserId, this.Clock.Now());
            var personCommonProperties = new PersonCommonProperties
            {
                Email = "test1@test.com",
                FullName = "Monitoring Test",
            };

            customerPerson.Update(
                new PersonalDetails(tenant.Id, personCommonProperties), this.performingUserId, this.Clock.Now());
            await this.stack.PersonAggregateRepository.Save(customerPerson);

            var customerAggregate = await this.stack.CreateCustomerForExistingPerson(
                customerPerson, DeploymentEnvironment.Staging, customerUser.Id, null);
            var dateElevenMonthsAgo = this.Clock.Now().ToLocalDateInAet().PlusMonths(-11);
            var calculationResultJson = CalculationResultJsonFactory.Create(startDate: dateElevenMonthsAgo);
            var formDataJson = FormDataJsonFactory.GetSampleWithStartAndEndDates(inceptionDate: dateElevenMonthsAgo);
            var quote = QuoteFactory.CreateNewBusinessQuote(
                tenant.Id,
                productId,
                organisationId: organisation.Id);
            var quoteAggregate = quote.Aggregate
                    .WithCustomerDetails(quote.Id)
                    .WithCustomer(customerAggregate)
                    .WithQuoteNumber(quote.Id)
                    .WithCalculationResult(quote.Id, formDataJson, calculationResultJson)
                    .WithPolicy(quote.Id);
            var emailModel = this.GenerateEmailModel();

            await this.stack.QuoteAggregateRepository.Save(quoteAggregate);

            this.stack.MockMediator.GetTenantByIdOrAliasQuery(tenantProductModel.Tenant);
            this.stack.MockMediator.GetProductByIdOrAliasQuery(tenantProductModel.Product);

            // Act
            await this.stack.RenewalInvitationService.SendPolicyRenewalInvitation(
                userAuthenticationData.TenantId, DeploymentEnvironment.Staging, quoteAggregate.Id, (Guid)this.performingUserId, false, true);

            // Assert
            var quoteEmailReadModel
                = this.stack.DbContext.QuoteEmailReadModel.FirstOrDefault(rm => rm.PolicyId == quoteAggregate.Id);
            var emailAssociation = this.stack.DbContext.Relationships.FirstOrDefault(
                x => x.FromEntityType == Domain.EntityType.Policy && x.FromEntityId == quoteAggregate.Id);
            Assert.NotNull(emailAssociation);
        }

        [Fact]
        public async Task Test_SendPolicyRenewalInvitation_Create_User()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var tenantProductModel = await this.CreateProductAndTenant(productId);
            var tenant = tenantProductModel.Tenant;

            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id, tenant.Details.Alias, tenant.Details.Name, null, Guid.NewGuid(), this.Clock.GetCurrentInstant());
            await this.stack.OrganisationAggregateRepository.Save(organisation);

            tenant.SetDefaultOrganisation(
                organisation.Id, this.Clock.GetCurrentInstant().Plus(Duration.FromMinutes(1)));
            this.stack.TenantRepository.SaveChanges();

            var customerEmail = "customer2@ubind.io";

            var user = await this.CreateUserModelForTenant("user2@ubind.io", tenant);
            var userAuthenticationData = new UserAuthenticationData(
                tenant.Id, tenant.Details.DefaultOrganisationId, UserType.Client, user.Id, Guid.Empty);

            var customerUser = await this.CreateUserModelForTenant(customerEmail, tenant);
            var customerUserAuthenticationData = new UserAuthenticationData(
                tenant.Id, tenant.Details.DefaultOrganisationId, UserType.Client, customerUser.Id, Guid.Empty);

            var customerPerson = PersonAggregate.CreatePerson(
                 tenant.Id, tenant.Details.DefaultOrganisationId, this.performingUserId, this.Clock.Now());
            var personCommonProperties = new PersonCommonProperties
            {
                Email = "test2@test.com",
                FullName = "Monitoring Test",
            };

            customerPerson.Update(
                new PersonalDetails(tenant.Id, personCommonProperties), this.performingUserId, this.Clock.Now());
            await this.stack.PersonAggregateRepository.Save(customerPerson);

            var customerAggregate = await this.stack.CreateCustomerForExistingPerson(
                customerPerson, DeploymentEnvironment.Staging, customerUser.Id, null);

            var dateElevenMonthsAgo = this.Clock.Now().ToLocalDateInAet().PlusMonths(-11);
            var calculationResultJson = CalculationResultJsonFactory.Create(startDate: dateElevenMonthsAgo);
            var formDataJson = FormDataJsonFactory.GetSampleWithStartAndEndDates(inceptionDate: dateElevenMonthsAgo);
            var quote = QuoteFactory.CreateNewBusinessQuote(
                tenant.Id,
                productId,
                organisationId: organisation.Id);
            var quoteAggregate = quote.Aggregate
                    .WithCustomerDetails(quote.Id)
                    .WithCustomer(customerAggregate)
                    .WithQuoteNumber(quote.Id)
                    .WithCalculationResult(quote.Id, formDataJson, calculationResultJson)
                    .WithPolicy(quote.Id);
            var emailModel = this.GenerateEmailModel();

            await this.stack.QuoteAggregateRepository.Save(quoteAggregate);

            this.stack.MockMediator.GetTenantByIdOrAliasQuery(tenantProductModel.Tenant);
            this.stack.MockMediator.GetProductByIdOrAliasQuery(tenantProductModel.Product);

            // Act
            await this.stack.RenewalInvitationService.SendPolicyRenewalInvitation(
                userAuthenticationData.TenantId,
                DeploymentEnvironment.Staging,
                quoteAggregate.Id,
                (Guid)this.performingUserId,
                false,
                true);

            // Assert
            var userAggregate = this.stack.UserAggregateRepository.GetById(tenant.Id, customerUser.Id);
            userAggregate.Should().NotBeNull();
        }

        [Fact]
        public async Task Test_SendPolicRenewalInvitation_DoesNotCreateUser_WhenNotRequired()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var tenantProductModel = await this.CreateProductAndTenant(productId);
            var tenant = tenantProductModel.Tenant;
            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id, tenant.Details.Alias, tenant.Details.Name, null, Guid.NewGuid(), this.Clock.GetCurrentInstant());
            await this.stack.OrganisationAggregateRepository.Save(organisation);

            tenant.SetDefaultOrganisation(
                organisation.Id, this.Clock.GetCurrentInstant().Plus(Duration.FromMinutes(1)));
            this.stack.TenantRepository.SaveChanges();

            var customerEmail = "customer3@ubind.io";

            var user = await this.CreateUserModelForTenant("user2@ubind.io", tenant);
            var userAuthenticationData = new UserAuthenticationData(
                tenant.Id, tenant.Details.DefaultOrganisationId, UserType.Client, user.Id, Guid.Empty);

            var customerPerson = PersonAggregate.CreatePerson(
                 tenant.Id, tenant.Details.DefaultOrganisationId, this.performingUserId, this.Clock.Now());
            var personCommonProperties = new PersonCommonProperties
            {
                Email = "test2@test.com",
                FullName = "Monitoring Test",
            };

            customerPerson.Update(
                new PersonalDetails(tenant.Id, personCommonProperties), this.performingUserId, this.Clock.Now());
            await this.stack.PersonAggregateRepository.Save(customerPerson);

            var customerAggregate = await this.stack.CreateCustomerForExistingPerson(
                customerPerson, DeploymentEnvironment.Staging, null, null);

            var dateElevenMonthsAgo = this.Clock.Now().ToLocalDateInAet().PlusMonths(-11);
            var calculationResultJson = CalculationResultJsonFactory.Create(startDate: dateElevenMonthsAgo);
            var formDataJson = FormDataJsonFactory.GetSampleWithStartAndEndDates(inceptionDate: dateElevenMonthsAgo);
            var quote = QuoteFactory.CreateNewBusinessQuote(
                tenant.Id,
                productId,
                organisationId: organisation.Id);
            var quoteAggregate = quote.Aggregate
                    .WithCustomerDetails(quote.Id)
                    .WithCustomer(customerAggregate)
                    .WithQuoteNumber(quote.Id)
                    .WithCalculationResult(quote.Id, formDataJson, calculationResultJson)
                    .WithPolicy(quote.Id);

            await this.stack.QuoteAggregateRepository.Save(quoteAggregate);
            this.stack.MockMediator.GetTenantByIdOrAliasQuery(tenantProductModel.Tenant);
            this.stack.MockMediator.GetProductByIdOrAliasQuery(tenantProductModel.Product);

            // Act
            await this.stack.RenewalInvitationService.SendPolicyRenewalInvitation(
                userAuthenticationData.TenantId, DeploymentEnvironment.Staging, quoteAggregate.Id, (Guid)this.performingUserId, false, false);

            // Assert
            var userLogin = this.stack.UserLoginEmailRepository
                .GetUserLoginByEmail(tenant.Id, tenant.Details.DefaultOrganisationId, customerEmail);
            userLogin.Should().BeNull();
        }

        [Fact]
        public async Task SendPolicyRenewalInvitation_ShouldThrowAnErrorException_WhenRenewalInvitationEmailsSettingsIsDisabled()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var tenantProductModel = await this.CreateProductAndTenant(productId);
            var tenant = tenantProductModel.Tenant;

            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id, tenant.Details.Alias, tenant.Details.Name, null, Guid.NewGuid(), this.Clock.GetCurrentInstant());
            await this.stack.OrganisationAggregateRepository.Save(organisation);

            tenant.SetDefaultOrganisation(
                organisation.Id, this.Clock.GetCurrentInstant().Plus(Duration.FromMinutes(1)));
            this.stack.TenantRepository.SaveChanges();

            var customerEmail = "customer2@ubind.io";

            var user = await this.CreateUserModelForTenant("user2@ubind.io", tenant);
            var userAuthenticationData = new UserAuthenticationData(
                tenant.Id, tenant.Details.DefaultOrganisationId, UserType.Client, user.Id, Guid.Empty);

            var customerUser = await this.CreateUserModelForTenant(customerEmail, tenant);
            var customerUserAuthenticationData = new UserAuthenticationData(
                tenant.Id, tenant.Details.DefaultOrganisationId, UserType.Client, customerUser.Id, Guid.Empty);

            var customerPerson = PersonAggregate.CreatePerson(
                 tenant.Id, tenant.Details.DefaultOrganisationId, this.performingUserId, this.Clock.Now());
            var personCommonProperties = new PersonCommonProperties
            {
                Email = "test2@test.com",
                FullName = "Monitoring Test",
            };

            customerPerson.Update(
                new PersonalDetails(tenant.Id, personCommonProperties), this.performingUserId, this.Clock.Now());
            await this.stack.PersonAggregateRepository.Save(customerPerson);

            var customerAggregate = await this.stack.CreateCustomerForExistingPerson(
                customerPerson, DeploymentEnvironment.Staging, customerUser.Id, null);

            var dateElevenMonthsAgo = this.Clock.Now().ToLocalDateInAet().PlusMonths(-11);
            var calculationResultJson = CalculationResultJsonFactory.Create(startDate: dateElevenMonthsAgo);
            var formDataJson = FormDataJsonFactory.GetSampleWithStartAndEndDates(inceptionDate: dateElevenMonthsAgo);
            var quote = QuoteFactory.CreateNewBusinessQuote(
                tenant.Id,
                productId,
                organisationId: organisation.Id);
            var quoteAggregate = quote.Aggregate
                    .WithCustomerDetails(quote.Id)
                    .WithCustomer(customerAggregate)
                    .WithQuoteNumber(quote.Id)
                    .WithCalculationResult(quote.Id, formDataJson, calculationResultJson)
                    .WithPolicy(quote.Id);
            var emailModel = this.GenerateEmailModel();

            await this.stack.QuoteAggregateRepository.Save(quoteAggregate);

            this.stack.MockMediator.GetTenantByIdOrAliasQuery(tenantProductModel.Tenant);
            this.stack.MockMediator.GetProductByIdOrAliasQuery(tenantProductModel.Product);

            this.stack.EntitySettingsRepository.AddOrUpdateEntitySettings(
                tenant.Id,
                EntityType.Organisation,
                organisation.Id,
                new OrganisationEntitySettings
                {
                    AllowOrganisationRenewalInvitation = false,
                });

            // Act
            Func<Task> func = async () => await this.stack.RenewalInvitationService.SendPolicyRenewalInvitation(
                userAuthenticationData.TenantId,
                DeploymentEnvironment.Staging,
                quoteAggregate.Id,
                (Guid)this.performingUserId,
                false,
                true);

            // Assert that it throws exception
            (await func.Should().ThrowAsync<ErrorException>())
                .Which.Error.Code.Should().Be("renewal.invitation.emails.disabled");
        }

        [Fact(Skip = "This throws the required exception at the wrong point. The test needs rewriting.")]
        public async Task Test_SendPolicyRenewalInvitation_With_duplicate_email_Raise_Exception()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var tenantProductModel = await this.CreateProductAndTenant(productId);
            var tenant = tenantProductModel.Tenant;

            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id, tenant.Details.Alias, tenant.Details.Name, null, Guid.NewGuid(), this.Clock.GetCurrentInstant());
            await this.stack.OrganisationAggregateRepository.Save(organisation);

            tenant.SetDefaultOrganisation(
                organisation.Id, this.Clock.GetCurrentInstant().Plus(Duration.FromMinutes(1)));
            this.stack.TenantRepository.SaveChanges();

            var customerEmail = "customer3@ubind.io";

            // Create an existing user
            await this.CreateUserModelForTenant("customer3@ubind.io", tenant);

            var user = await this.CreateUserModelForTenant("user3@ubind.io", tenant);
            var userAuthenticationData = new UserAuthenticationData(
                tenant.Id, tenant.Details.DefaultOrganisationId, UserType.Client, user.Id, Guid.Empty);

            var customerUser = await this.CreateUserModelForTenant(customerEmail, tenant);
            var customerUserAuthenticationData = new UserAuthenticationData(
                tenant.Id, tenant.Details.DefaultOrganisationId, UserType.Client, customerUser.Id, Guid.Empty);

            var customerPerson = PersonAggregate.CreatePerson(
                tenant.Id, tenant.Details.DefaultOrganisationId, this.performingUserId, this.Clock.Now());
            var personCommonProperties = new PersonCommonProperties
            {
                Email = "test3@test.com",
                FullName = "Monitoring Test",
            };

            customerPerson.Update(
                new PersonalDetails(tenant.Id, personCommonProperties), this.performingUserId, this.Clock.Now());
            await this.stack.PersonAggregateRepository.Save(customerPerson);

            var customerAggregate = await this.stack.CreateCustomerForExistingPerson(
                customerPerson, DeploymentEnvironment.Staging, customerUser.Id, null);

            var dateElevenMonthsAgo = this.Clock.Now().ToLocalDateInAet().PlusMonths(-11);
            var calculationResultJson = CalculationResultJsonFactory.Create(startDate: dateElevenMonthsAgo);
            var quote = QuoteFactory.CreateNewBusinessQuote(tenant.Id, productId);
            var quoteAggregate = quote.Aggregate
                    .WithCustomerDetails(quote.Id)
                    .WithCustomer(customerAggregate)
                    .WithQuoteNumber(quote.Id)
                    .WithCalculationResult(quote.Id, calculationResultJson: calculationResultJson)
                    .WithPolicy(quote.Id);
            var emailModel = this.GenerateEmailModel();

            await this.stack.QuoteAggregateRepository.Save(quoteAggregate);

            // Act
            Func<Task> act = () => this.stack.RenewalInvitationService.SendPolicyRenewalInvitation(
                userAuthenticationData.TenantId, DeploymentEnvironment.Staging, quoteAggregate.Id, (Guid)this.performingUserId, true, false);

            // Assert
            var userAggregate = this.stack.UserAggregateRepository.GetById(tenant.Id, customerUser.Id);
            userAggregate.Should().NotBeNull();
            await act.Should().ThrowAsync<DuplicateUserEmailException>();
        }

        [Fact]
        public async Task Test_ExpiresNotLessOrEqualTo60Days_ThrowsException()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var tenantProductModel = await this.CreateProductAndTenant(productId);
            var tenant = tenantProductModel.Tenant;

            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id, tenant.Details.Alias, tenant.Details.Name, null, Guid.NewGuid(), this.Clock.GetCurrentInstant());
            await this.stack.OrganisationAggregateRepository.Save(organisation);

            tenant.SetDefaultOrganisation(
                organisation.Id, this.Clock.GetCurrentInstant().Plus(Duration.FromMinutes(1)));
            this.stack.TenantRepository.SaveChanges();

            var userModel = await this.CreateUserModelForTenant("ris-test4@ubind.io", tenant);
            var productFeatureService = new Mock<IProductFeatureSettingService>();

            var customerPerson = PersonAggregate.CreatePerson(
                 tenant.Id, tenant.Details.DefaultOrganisationId, this.performingUserId, this.Clock.Now());
            var personCommonProperties = new PersonCommonProperties
            {
                Email = userModel.LoginEmail,
                FullName = "Monitoring Test",
            };

            customerPerson.Update(
                new PersonalDetails(tenant.Id, personCommonProperties), this.performingUserId, this.Clock.Now());
            await this.stack.PersonAggregateRepository.Save(customerPerson);
            var customerAggregate = await this.stack.CreateCustomerForExistingPerson(
                customerPerson, DeploymentEnvironment.Staging, userModel.Id, null);

            var dateNineMonthsAgo = this.Clock.Now().ToLocalDateInAet().PlusMonths(-9);
            var calculationResultJson = CalculationResultJsonFactory.Create(startDate: dateNineMonthsAgo);
            var quote = QuoteFactory.CreateNewBusinessQuote(
                tenant.Id,
                productId,
                organisationId: organisation.Id);
            var quoteAggregate = quote.Aggregate
                    .WithCustomerDetails(quote.Id)
                    .WithCustomer(customerAggregate)
                    .WithQuoteNumber(quote.Id)
                    .WithCalculationResult(quote.Id, calculationResultJson: calculationResultJson)
                    .WithPolicy(quote.Id);
            var emailModel = this.GenerateEmailModel();

            await this.stack.QuoteAggregateRepository.Save(quoteAggregate);
            var userAuthenticationData = new UserAuthenticationData(
                tenant.Id, tenant.Details.DefaultOrganisationId, UserType.Client, userModel.Id, Guid.Empty);

            // Act
            Func<Task> func = async () => await this.stack.RenewalInvitationService.SendPolicyRenewalInvitation(
                userAuthenticationData.TenantId, DeploymentEnvironment.Staging, quoteAggregate.Policy.PolicyId, (Guid)this.performingUserId, false, true);

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be("policy.cannot.send.renewal.invitation");
        }

        [Theory]
        [InlineData("username1@domain.com", false, 0, -13)] // when policy is expired and allow renewal after expiry is not enabled
        [InlineData("username2@domain.com", true, 32, -13)] // allow renewal after expiry is enabled and expiry not with in the renewal period.
        public async Task SendPolicyRenewalInvitation_ShouldThrowNotAllowedErrorException_WhenSendingOfPolicyRenewalIsNotAllowed(
            string email,
            bool allowrenewalAfterExpiry,
            int expiredPolicyDuration,
            int monthsAfterExpiry)
        {
            // Arrange
            var tenantProductModel = await this.CreateProductAndTenant(Guid.NewGuid());
            var tenant = tenantProductModel.Tenant;
            var product = tenantProductModel.Product;

            var productFeature = new ProductFeatureSetting(tenant.Id, product.Id, this.Clock.Now());
            productFeature.UpdateProductFeatureRenewalSetting(allowrenewalAfterExpiry, Duration.FromDays(expiredPolicyDuration));
            var productFeatureService = new Mock<IProductFeatureSettingService>();
            productFeatureService.Setup(e => e.GetProductFeature(tenant.Id, product.Id)).Returns(productFeature);

            var organisation = Organisation.CreateNewOrganisation(
                 tenant.Id, tenant.Details.Alias, tenant.Details.Name, null, Guid.NewGuid(), this.Clock.GetCurrentInstant());
            await this.stack.OrganisationAggregateRepository.Save(organisation);

            tenant.SetDefaultOrganisation(
                organisation.Id, this.Clock.GetCurrentInstant().Plus(Duration.FromMinutes(1)));
            this.stack.TenantRepository.SaveChanges();

            // add roles
            this.CreateClientAdminAndCustomerRoles(tenant);

            var userModel = await this.CreateUserModelForTenant(email, tenant);
            var quoteAggregate = await this.CreateQuoteAsync(tenant, product.Id, email, userModel.Id, monthsAfterExpiry, organisation.Id);
            var userAuthenticationData = new UserAuthenticationData(
                tenant.Id, tenant.Details.DefaultOrganisationId, UserType.Client, userModel.Id, Guid.Empty);

            // Act
            Func<Task> func = async () => await this.stack.RenewalInvitationService.SendPolicyRenewalInvitation(
                userAuthenticationData.TenantId, DeploymentEnvironment.Staging, quoteAggregate.Policy.PolicyId, (Guid)this.performingUserId, false, true);

            // Assert that it throws exception
            (await func.Should().ThrowAsync<ErrorException>())
                .Which.Error.Code.Should().Be("expired.policy.not.allowed.for.sending.renewal");
        }

        [Theory]
        [InlineData("user1@domain.com", false, 0, 1)] // when policy is not expired
        [InlineData("user2@domain.com", true, 32, -11)] // when policy is expired and expiry is with in the renewal period.
        public async Task SendPolicyRenewalInvitation_ShouldNotThrowNotAllowedErrorException_WhenSendingOfPolicyRenewalIsAllowed(
            string email,
            bool allowrenewalAfterExpiry,
            int expiredPolicyDuration,
            int monthsAfterExpiry)
        {
            // Arrange
            var tenantProductModel = await this.CreateProductAndTenant(Guid.NewGuid());
            var tenant = tenantProductModel.Tenant;
            var product = tenantProductModel.Product;

            var productFeature = new ProductFeatureSetting(tenant.Id, product.Id, this.Clock.Now());
            productFeature.UpdateProductFeatureRenewalSetting(allowrenewalAfterExpiry, Duration.FromDays(expiredPolicyDuration));

            var productFeatureService = new Mock<IProductFeatureSettingService>();
            productFeatureService.Setup(e => e.GetProductFeature(tenant.Id, product.Id)).Returns(productFeature);

            // add roles
            this.CreateClientAdminAndCustomerRoles(tenant);

            this.stack.DbContext.SaveChanges();

            var organisation = Organisation.CreateNewOrganisation(
            tenant.Id, tenant.Details.Alias, tenant.Details.Name, null, Guid.NewGuid(), this.Clock.GetCurrentInstant());
            await this.stack.OrganisationAggregateRepository.Save(organisation);

            tenant.SetDefaultOrganisation(
                organisation.Id, this.Clock.GetCurrentInstant().Plus(Duration.FromMinutes(1)));
            this.stack.TenantRepository.SaveChanges();

            var userModel = await this.CreateUserModelForTenant(email, tenant);
            var quoteAggregate = await this.CreateQuoteAsync(tenant, product.Id, email, userModel.Id, monthsAfterExpiry, organisation.Id);

            var userAuthenticationData = new UserAuthenticationData(
                tenant.Id, tenant.Details.DefaultOrganisationId, UserType.Client, userModel.Id, Guid.Empty);

            // Act
            Func<Task> func = () => this.stack.RenewalInvitationService.SendPolicyRenewalInvitation(
                userAuthenticationData.TenantId, DeploymentEnvironment.Staging, quoteAggregate.Policy.PolicyId, (Guid)this.performingUserId, false, true);

            // Assert that it throws exception
            (await func.Should().ThrowAsync<ErrorException>())
                .Which.Error.Code.Should().NotBe("expired.policy.not.allowed.for.sending.renewal");
        }

        private async Task<UserAggregate> CreateUserModelForTenant(string email, Tenant tenant)
        {
            const string testNumber = "0412345678";
            var userSignupModel = new UserSignupModel()
            {
                AlternativeEmail = email,
                WorkPhoneNumber = testNumber,
                Email = email,
                Environment = DeploymentEnvironment.Staging,
                FullName = "test",
                HomePhoneNumber = testNumber,
                MobilePhoneNumber = testNumber,
                PreferredName = "test",
                UserType = UserType.Client,
                TenantId = tenant.Id,
                OrganisationId = tenant.Details.DefaultOrganisationId,
            };

            return await this.stack.UserService.CreateUser(userSignupModel);
        }

        private async Task<QuoteAggregate> CreateQuoteAsync(
            Tenant tenant, Guid productId, string email, Guid ownerId, int monthsAfterExpiry, Guid organizationId)
        {
            var customerPerson = PersonAggregate.CreatePerson(
                 tenant.Id, tenant.Details.DefaultOrganisationId, this.performingUserId, this.Clock.Now());
            var personCommonProperties = new PersonCommonProperties
            {
                Email = email,
                FullName = "Monitoring Test",
            };

            customerPerson.Update(
                new PersonalDetails(tenant.Id, personCommonProperties), this.performingUserId, this.Clock.Now());
            await this.stack.PersonAggregateRepository.Save(customerPerson);

            var customerAggregate = await this.stack.CreateCustomerForExistingPerson(
                customerPerson, DeploymentEnvironment.Staging, ownerId, null);

            var monthsAgo = this.Clock.Now().ToLocalDateInAet().PlusMonths(monthsAfterExpiry);
            var calculationResultJson = CalculationResultJsonFactory.Create(startDate: monthsAgo);
            var quote = QuoteFactory.CreateNewBusinessQuote(tenant.Id, productId, organisationId: organizationId);
            var formDataJson = FormDataJsonFactory.GetSampleWithStartAndEndDates(inceptionDate: monthsAgo);
            var quoteAggregate = quote.Aggregate
                .WithCustomerDetails(quote.Id)
                .WithCustomer(customerAggregate)
                .WithQuoteNumber(quote.Id)
                .WithCalculationResult(quote.Id, formDataJson, calculationResultJson)
                .WithPolicy(quote.Id);

            await this.stack.QuoteAggregateRepository.Save(quoteAggregate);
            return quoteAggregate;
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

        private EmailModel GenerateEmailModel()
        {
            return new EmailModel(
                Guid.NewGuid(), Guid.NewGuid(), null, null, "from", "to", "subject", "plainText", "html", "cc", "bcc");
        }

        private void CreateClientAdminAndCustomerRoles(Tenant tenant)
        {
            try
            {
                this.stack.RoleRepository.Insert(
                RoleHelper.CreateTenantAdminRole(
                    tenant.Id, tenant.Details.DefaultOrganisationId, this.stack.Clock.Now()));
                this.stack.RoleRepository.Insert(
                    RoleHelper.CreateCustomerRole(
                        tenant.Id, tenant.Details.DefaultOrganisationId, this.stack.Clock.Now()));
                this.stack.RoleRepository.SaveChanges();
            }
            catch (Exception)
            {
                Console.WriteLine("duplicate role");
            }
        }

        private Role CreateCustomerRole(IUserAuthenticationData userAuthData)
        {
            var role = RoleHelper.CreateCustomerRole(
                userAuthData.TenantId, userAuthData.OrganisationId, this.Clock.GetCurrentInstant());
            this.stack.DbContext.Roles.Add(role);
            this.stack.DbContext.SaveChanges();
            return role;
        }

        /// <summary>
        /// Tenant product model to return just for testing.
        /// </summary>
        private class TenantProductModel
        {
#pragma warning disable SA1401 // Fields should be private
            public Tenant Tenant;
            public Domain.Product.Product Product;
#pragma warning restore SA1401 // Fields should be private
        }
    }
}
