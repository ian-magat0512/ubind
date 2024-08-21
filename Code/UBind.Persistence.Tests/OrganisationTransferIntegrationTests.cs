// <copyright file="OrganisationTransferIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Hangfire;
    using Hangfire.Common;
    using Hangfire.States;
    using Moq;
    using NodaTime;
    using UBind.Application.Commands.Tenant;
    using UBind.Application.Commands.User;
    using UBind.Application.Queries.Tenant;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Factory;
    using UBind.Domain.Product;
    using UBind.Domain.Quote;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadWriteModel.Email;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class OrganisationTransferIntegrationTests
    {
        private readonly Guid? performingUserId;
        private readonly Func<ApplicationStack> applicationStack;
        private Random randomNumber = new Random();

        public OrganisationTransferIntegrationTests()
        {
            this.applicationStack = () => new ApplicationStack(DatabaseFixture.TestConnectionStringName, ApplicationStackConfiguration.Default);
            this.performingUserId = Guid.NewGuid();
        }

        [Fact]
        public async Task ValidateCustomerDataForTransfer_ShouldThrowErrorException_WhenTryingToTransferToTheSameOrganisation()
        {
            using (var stack = this.applicationStack())
            {
                // Arrange
                var tenant = await this.GenerateTenantFromApplicationStack(stack);
                var organisation = await this.GenerateOrganisationAggregateFromApplicationStack(stack, tenant.Id, "sourceOrg", "sourceOrg", stack.Clock.Now());

                var customerEmail = $"{Guid.NewGuid()}@email.com";
                var person = await this.GeneratePersonAggregateFromApplicationStack(tenant.Id, organisation.Id, stack, customerEmail, stack.Clock.Now());

                var customerAggregate = await stack.CreateCustomerForExistingPerson(
                    person, DeploymentEnvironment.Staging, null, null);
                customerAggregate.Should().NotBeNull();

                var customer = stack.CustomerService.GetCustomerById(customerAggregate.TenantId, customerAggregate.Id);

                // Assert
                customer.Should().NotBeNull();
                customer.OrganisationId.Should().Be(organisation.Id);

                // Act
                Action action = () => stack.OrganisationTransferService.ValidateCustomerDataForTransfer(tenant.Id, customer.Id, organisation.Id);

                // Assert
                action.Should().Throw<ErrorException>().And.Error.Message.Should().Be(Errors.Customer.BelongsToTheSameOrganisation(customer.Id, organisation.Id).Message);
            }
        }

        /// <summary>
        /// This was trying to validate the transfering of customer to another organisation
        /// without any existing email on the destination organisation.
        /// Should not throw exception since this is a valid use case.
        /// </summary>
        [Fact]
        public async Task ValidateCustomerDataForTransfer_ShouldNotThrowErrorException_WhenNoExistingEmailOnDestinationOrganisation()
        {
            using (var stack = this.applicationStack())
            {
                // Arrange
                var tenant = await this.GenerateTenantFromApplicationStack(stack);
                var organisation = await this.GenerateOrganisationAggregateFromApplicationStack(stack, tenant.Id, "sourceOrg", "sourceOrg", stack.Clock.Now());
                var anotherOrganisation = await this.GenerateOrganisationAggregateFromApplicationStack(stack, tenant.Id, "destinationOrg", "destinationOrg", stack.Clock.Now());

                var customerEmail = $"{Guid.NewGuid()}@email.com";
                var person = await this.GeneratePersonAggregateFromApplicationStack(tenant.Id, organisation.Id, stack, customerEmail, stack.Clock.Now());

                var customerAggregate = await stack.CreateCustomerForExistingPerson(
                    person, DeploymentEnvironment.Staging, null, null);

                // Assert
                customerAggregate.Should().NotBeNull();

                var customer = stack.CustomerService.GetCustomerById(customerAggregate.TenantId, customerAggregate.Id);

                // Assert
                customer.Should().NotBeNull();
                customer.OrganisationId.Should().Be(organisation.Id);

                // Act
                Action action = () => stack.OrganisationTransferService.ValidateCustomerDataForTransfer(tenant.Id, customer.Id, anotherOrganisation.Id);

                // Assert
                action.Should().NotThrow<ErrorException>();
            }
        }

        /// <summary>
        /// This was trying to validate the transfering customner to another organisation with have duplicate email
        /// on both organisations without user account. It should not throw exception because this is a valid use case.
        /// </summary>
        [Fact]
        public async Task ValidateCustomerDataForTransfer_ShouldNotThrowException_WhenPersonHaveDuplicateEmailBothOrganisationsWithoutUserAccount()
        {
            using (var stack = this.applicationStack())
            {
                // Arrange
                var tenant = await this.GenerateTenantFromApplicationStack(stack);
                var organisation = await this.GenerateOrganisationAggregateFromApplicationStack(stack, tenant.Id, "sourceOrg", "sourceOrg", stack.Clock.Now());
                var anotherOrganisation = await this.GenerateOrganisationAggregateFromApplicationStack(stack, tenant.Id, "destinationOrg", "destinationOrg", stack.Clock.Now());

                // First customer on first organisation
                var customerEmail = $"{Guid.NewGuid()}@email.com";
                var person = await this.GeneratePersonAggregateFromApplicationStack(tenant.Id, organisation.Id, stack, customerEmail, stack.Clock.Now());

                var customerAggregate = await stack.CreateCustomerForExistingPerson(
                    person, DeploymentEnvironment.Staging, null, null);

                // Assert
                customerAggregate.Should().NotBeNull();

                // First customer on second organisation
                var anotherPerson = await this.GeneratePersonAggregateFromApplicationStack(tenant.Id, anotherOrganisation.Id, stack, customerEmail, stack.Clock.Now());
                var anotherCustomerAggregate = await stack.CreateCustomerForExistingPerson(
                    anotherPerson, DeploymentEnvironment.Staging, null, null);

                // Assert
                customerAggregate.Should().NotBeNull();

                var customer = stack.CustomerService.GetCustomerById(customerAggregate.TenantId, customerAggregate.Id);

                // Assert
                customer.Should().NotBeNull();
                customer.OrganisationId.Should().Be(organisation.Id);
                person.Email.Should().Be(anotherPerson.Email);
                anotherPerson.Email.Should().Be(person.Email);

                // Act
                Action action = () => stack.OrganisationTransferService.ValidateCustomerDataForTransfer(tenant.Id, customer.Id, anotherOrganisation.Id);

                // Assert
                action.Should().NotThrow<ErrorException>();
            }
        }

        /// <summary>
        /// This was trying to validate the transfering of customer to another organisation
        /// should not throw exception when the person has user account on the source organisation.
        /// </summary>
        [Fact]
        public async Task ValidateCustomerDataForTransfer_ShouldNotThrowException_WhenPersonHasUserAccountOnSourceOrganisation()
        {
            using (var stack = this.applicationStack())
            {
                // Arrange
                var tenant = await this.GenerateTenantFromApplicationStack(stack);
                var organisation = await this.GenerateOrganisationAggregateFromApplicationStack(stack, tenant.Id, "sourceOrg", "sourceOrg", stack.Clock.Now());
                var anotherOrganisation = await this.GenerateOrganisationAggregateFromApplicationStack(stack, tenant.Id, "destinationOrg", "destinationOrg", stack.Clock.Now());

                // First customer on first organisation
                var customerEmail = $"{Guid.NewGuid()}@email.com";
                var person = await this.GeneratePersonAggregateFromApplicationStack(tenant.Id, organisation.Id, stack, customerEmail, stack.Clock.Now());

                var customerAggregate = await stack.CreateCustomerForExistingPerson(
                    person, DeploymentEnvironment.Staging, null, null);
                var userAccountSource = await stack.UserService.CreateCustomerUserForPersonAndSendActivationInvitation(organisation.TenantId, person.Id, customerAggregate.Environment);

                // Assert
                customerAggregate.Should().NotBeNull();
                userAccountSource.Should().NotBeNull();

                // First customer on second organisation
                var anotherPerson = await this.GeneratePersonAggregateFromApplicationStack(tenant.Id, anotherOrganisation.Id, stack, customerEmail, stack.Clock.Now());
                var anotherCustomerAggregate = await stack.CreateCustomerForExistingPerson(
                    anotherPerson, DeploymentEnvironment.Staging, null, null);

                // Assert
                customerAggregate.Should().NotBeNull();

                var customer = stack.CustomerService.GetCustomerById(customerAggregate.TenantId, customerAggregate.Id);

                // Assert
                customer.Should().NotBeNull();
                customer.OrganisationId.Should().Be(organisation.Id);

                // Act
                Action action = () => stack.OrganisationTransferService.ValidateCustomerDataForTransfer(tenant.Id, customer.Id, anotherOrganisation.Id);

                // Assert
                action.Should().NotThrow<ErrorException>();
            }
        }

        /// <summary>
        /// This was trying to validate the transfering of customer to another organisation
        /// should not throw exception when the person have same email both soure and destination,
        /// there no user account on source but there a user account on the destination organisation.
        /// </summary>
        [Fact]
        public async Task ValidateCustomerDataForTransfer_ShouldNotThrowException_WhenPersonHaveSameEmailBothOrganisationsButOnDestinationHasUserAccount()
        {
            using (var stack = this.applicationStack())
            {
                // Arrange
                var tenant = await this.GenerateTenantFromApplicationStack(stack);
                var organisation = await this.GenerateOrganisationAggregateFromApplicationStack(stack, tenant.Id, "sourceOrg", "sourceOrg", stack.Clock.Now());
                var anotherOrganisation = await this.GenerateOrganisationAggregateFromApplicationStack(stack, tenant.Id, "destinationOrg", "destinationOrg", stack.Clock.Now());

                // First customer on first organisation
                var customerEmail = $"{Guid.NewGuid()}@email.com";
                var person = await this.GeneratePersonAggregateFromApplicationStack(tenant.Id, organisation.Id, stack, customerEmail, stack.Clock.Now());

                var customerAggregate = await stack.CreateCustomerForExistingPerson(
                    person, DeploymentEnvironment.Staging, null, null);

                // Assert
                customerAggregate.Should().NotBeNull();

                // First customer on second organisation
                var anotherPerson = await this.GeneratePersonAggregateFromApplicationStack(tenant.Id, anotherOrganisation.Id, stack, customerEmail, stack.Clock.Now());

                var anotherCustomerAggregate = await stack.CreateCustomerForExistingPerson(
                    anotherPerson, DeploymentEnvironment.Staging, null, null);
                var userAccountDestination = await stack.UserService.CreateCustomerUserForPersonAndSendActivationInvitation(anotherCustomerAggregate.TenantId, anotherPerson.Id, anotherCustomerAggregate.Environment);

                // Assert
                customerAggregate.Should().NotBeNull();
                userAccountDestination.Should().NotBeNull();

                var customer = stack.CustomerService.GetCustomerById(customerAggregate.TenantId, customerAggregate.Id);

                // Assert
                customer.Should().NotBeNull();
                customer.OrganisationId.Should().Be(organisation.Id);

                // Act
                Action action = () => stack.OrganisationTransferService.ValidateCustomerDataForTransfer(tenant.Id, customer.Id, anotherOrganisation.Id);

                // Assert
                action.Should().NotThrow<ErrorException>();
            }
        }

        /// <summary>
        /// This was trying to validate the transfering of customer to another organisation
        /// that the person has user account both source and destination with conflicting email. It should throw an exception
        /// because we dont allow that two customer with same login email.
        /// </summary>
        [Fact]
        public async Task ValidateCustomerDataForTransfer_ShouldThrowErrorException_WhenPersonHasDuplicateUserAccountsBothOrganisationsWithConflictingEmail()
        {
            using (var stack = this.applicationStack())
            {
                // Arrange
                var tenant = await this.GenerateTenantFromApplicationStack(stack);
                var organisation = await this.GenerateOrganisationAggregateFromApplicationStack(stack, tenant.Id, "sourceOrg", "sourceOrg", stack.Clock.Now());
                var anotherOrganisation = await this.GenerateOrganisationAggregateFromApplicationStack(stack, tenant.Id, "destinationOrg", "destinationOrg", stack.Clock.Now());

                // First customer on first organisation
                var customerEmail = $"testing+{this.randomNumber.Next(1, 10)}@email.com";
                var person = await this.GeneratePersonAggregateFromApplicationStack(tenant.Id, organisation.Id, stack, customerEmail, stack.Clock.Now());
                var newPerson = await this.GeneratePersonAggregateFromApplicationStack(tenant.Id, organisation.Id, stack, $"myPerson+{this.randomNumber.Next(1, 10)}@email.com", stack.Clock.Now());
                var customerAggregate = await stack.CreateCustomerForExistingPerson(
                    person, DeploymentEnvironment.Staging, null, null);
                newPerson.AssociateWithCustomer(customerAggregate.Id, this.performingUserId, stack.Clock.Now());
                await stack.CustomerAggregateRepository.Save(customerAggregate);
                await stack.PersonAggregateRepository.Save(newPerson);
                var userAccountSource = await stack.UserService.CreateCustomerUserForPersonAndSendActivationInvitation(customerAggregate.TenantId, person.Id, customerAggregate.Environment);

                // Assert
                customerAggregate.Should().NotBeNull();
                userAccountSource.Should().NotBeNull();

                // First customer on second organisation
                var anotherPerson = await this.GeneratePersonAggregateFromApplicationStack(tenant.Id, anotherOrganisation.Id, stack, customerEmail, stack.Clock.Now());

                var anotherCustomerAggregate = await stack.CreateCustomerForExistingPerson(
                    anotherPerson, DeploymentEnvironment.Staging, null, null);
                var userAccountDestination = await stack.UserService.CreateCustomerUserForPersonAndSendActivationInvitation(anotherCustomerAggregate.TenantId, anotherPerson.Id, anotherCustomerAggregate.Environment);

                // Assert
                customerAggregate.Should().NotBeNull();
                userAccountDestination.Should().NotBeNull();

                var customer = stack.CustomerService.GetCustomerById(customerAggregate.TenantId, customerAggregate.Id);

                // Assert
                customer.Should().NotBeNull();
                customer.OrganisationId.Should().Be(organisation.Id);

                // Act
                Action action = () => stack.OrganisationTransferService.ValidateCustomerDataForTransfer(tenant.Id, customer.Id, anotherOrganisation.Id);

                // Assert
                action.Should().Throw<ErrorException>().And.Error.Code.Should()
                .Be(Errors.Organisation.DuplicateCustomerWithUserInBothOrganisations(null, null, null, null, null, null, null, null).Code);
            }
        }

        [Fact]
        public async Task TransferCustomerToAnotherOrganisation_ShouldChangeCustomerOrganisation_WhenTransferringToAnotherOrganisationWithTheSameTenancy()
        {
            using (var stack = this.applicationStack())
            {
                // Arrange
                var tenant = await this.GenerateTenantFromApplicationStack(stack);
                var product = this.GenerateProductFromApplicatioFromApplicationStack(tenant.Id, stack);
                var organisation = await this.GenerateOrganisationAggregateFromApplicationStack(stack, tenant.Id, "sourceOrg", "sourceOrg", stack.Clock.Now());
                var anotherOrganisation = await this.GenerateOrganisationAggregateFromApplicationStack(stack, tenant.Id, "destinationOrg", "destinationOrg", stack.Clock.Now());

                var customerEmail = $"{Guid.NewGuid()}@email.com";
                var person = await this.GeneratePersonAggregateFromApplicationStack(tenant.Id, organisation.Id, stack, customerEmail, stack.Clock.Now());

                var customerAggregate = await stack.CreateCustomerForExistingPerson(
                    person, DeploymentEnvironment.Staging, null, null);

                // Assert
                customerAggregate.Should().NotBeNull();

                var quote = QuoteFactory.CreateNewBusinessQuote(tenant.Id, product.Id, DeploymentEnvironment.Staging, organisationId: organisation.Id);
                var quoteAggregate = quote.Aggregate.WithCustomer(customerAggregate);
                await stack.QuoteAggregateRepository.Save(quoteAggregate);

                var customer = stack.CustomerService.GetCustomerById(customerAggregate.TenantId, customerAggregate.Id);

                var metadata = this.CreateEmailAndMetadata(quoteAggregate, customer.Id, stack.Clock.Now(), stack);
                stack.EmailService.InsertEmailAndMetadata(metadata);

                // Assert
                customer.Should().NotBeNull();
                customer.OrganisationId.Should().Be(organisation.Id);

                // Act
                await stack.OrganisationTransferService.TransferCustomerToAnotherOrganisation(tenant.Id, customer.Id, anotherOrganisation.Id, this.performingUserId, null);
                customer = stack.CustomerService.GetCustomerById(customerAggregate.TenantId, customerAggregate.Id);

                var quoteDetails = stack.QuoteService.GetQuoteDetails(tenant.Id, quote.Id);

                var filters = new EntityListFilters
                {
                    SearchTerms = new string[] { "example", "com" },
                    Statuses = new string[] { "Customer" },
                    Sources = new string[] { "Policy", "Quote" },
                    CustomerId = customer.Id,
                    OrganisationIds = new List<Guid> { anotherOrganisation.Id },
                };
                var emails = stack.EmailQueryService.GetAll(tenant.Id, filters);

                // Assert
                customer.Should().NotBeNull();
                customer.OrganisationId.Should().Be(anotherOrganisation.Id);
                quoteDetails.OrganisationId.Should().Be(anotherOrganisation.Id);
                emails.Should().HaveCount(1);
                emails.FirstOrDefault().OrganisationId.Should().Be(anotherOrganisation.Id);
            }
        }

        [Fact]
        public async Task ValidateUserDataForTransfer_ShouldThrowErrorException_WhenTryingToTransferToTheSameOrganisation()
        {
            using (var stack = this.applicationStack())
            {
                // Arrange
                var tenant = await this.GenerateTenantFromApplicationStack(stack);
                var organisation = await this.GenerateOrganisationAggregateFromApplicationStack(stack, tenant.Id, "sourceOrg", "sourceOrg", stack.Clock.Now());

                var customerEmail = $"{Guid.NewGuid()}@email.com";
                var person = await this.GeneratePersonAggregateFromApplicationStack(tenant.Id, organisation.Id, stack, customerEmail, stack.Clock.Now());

                var customerAggregate = await stack.CreateCustomerForExistingPerson(
                    person, DeploymentEnvironment.Staging, null, null);
                customerAggregate.Should().NotBeNull();

                var ownerEmail = $"{Guid.NewGuid()}@owner.com";
                var ownerPerson = await this.GeneratePersonAggregateFromApplicationStack(tenant.Id, organisation.Id, stack, ownerEmail, stack.Clock.Now());

                var userSignupModel = new Application.User.UserSignupModel()
                {
                    WorkPhoneNumber = "0412345678",
                    Email = ownerEmail,
                    Environment = DeploymentEnvironment.Staging,
                    FullName = "test",
                    HomePhoneNumber = "0412345678",
                    MobilePhoneNumber = "0412345678",
                    PreferredName = "test",
                    UserType = UserType.Client,
                    TenantId = tenant.Id,
                    OrganisationId = organisation.Id,
                };
                var userModel = await stack.UserService.CreateUser(userSignupModel);
                customerAggregate.AssignOwnership(userSignupModel.UserId, ownerPerson, this.performingUserId, stack.Clock.Now());

                // Act
                Func<Task> action = async () => await stack.OrganisationTransferService.ValidateUserDataForTransfer(tenant.Id, userModel.Id, organisation.Id);

                // Assert
                await action.Should().ThrowAsync<ErrorException>();
            }
        }

        [Fact]
        public async Task TransferUserToAnotherOrganisation_ShouldChangeOrganisation_WhenTransferringToAnotherOrganisationWithTheSameTenancy()
        {
            using (var stack = this.applicationStack())
            {
                // Arrange
                var tenant = await this.GenerateTenantFromApplicationStack(stack);
                var product = this.GenerateProductFromApplicatioFromApplicationStack(tenant.Id, stack);
                var organisation = await this.GenerateOrganisationAggregateFromApplicationStack(stack, tenant.Id, "sourceOrg", "sourceOrg", stack.Clock.Now());
                var anotherOrganisation = await this.GenerateOrganisationAggregateFromApplicationStack(stack, tenant.Id, "destinationOrg", "destinationOrg", stack.Clock.Now());

                var customerEmail = $"{Guid.NewGuid()}@email.com";
                var person = await this.GeneratePersonAggregateFromApplicationStack(tenant.Id, organisation.Id, stack, customerEmail, stack.Clock.Now());
                var customerAggregate = await stack.CreateCustomerForExistingPerson(
                    person, DeploymentEnvironment.Staging, null, null);

                // Assert
                customerAggregate.Should().NotBeNull();

                var quote = QuoteFactory.CreateNewBusinessQuote(tenant.Id, product.Id, DeploymentEnvironment.Staging, organisationId: organisation.Id);
                var quoteAggregate = quote.Aggregate.WithCustomer(customerAggregate);
                await stack.QuoteAggregateRepository.Save(quoteAggregate);

                var customer = stack.CustomerService.GetCustomerById(customerAggregate.TenantId, customerAggregate.Id);

                var metadata = this.CreateEmailAndMetadata(quoteAggregate, customer.Id, stack.Clock.Now(), stack);
                stack.EmailService.InsertEmailAndMetadata(metadata);

                var ownerEmail = $"{Guid.NewGuid()}@owner.com";
                var ownerPerson = await this.GeneratePersonAggregateFromApplicationStack(tenant.Id, organisation.Id, stack, ownerEmail, stack.Clock.Now());
                var userSignupModel = new Application.User.UserSignupModel()
                {
                    WorkPhoneNumber = "0412345678",
                    Email = ownerEmail,
                    Environment = DeploymentEnvironment.Staging,
                    FullName = "test",
                    HomePhoneNumber = "0412345678",
                    MobilePhoneNumber = "0412345678",
                    PreferredName = "test",
                    UserType = UserType.Client,
                    TenantId = tenant.Id,
                    OrganisationId = organisation.Id,
                };
                var userModel = await stack.UserService.CreateUser(userSignupModel);
                customerAggregate.AssignOwnership(userModel.Id, ownerPerson, this.performingUserId, stack.Clock.Now());
                await stack.CustomerAggregateRepository.Save(customerAggregate);

                bool includeCustomers = true;
                var backgroundJobClient = new Mock<IBackgroundJobClient>();

                // Act
                var command = new TransferUserToOtherOrganisationCommand(tenant.Id, userModel.Id, anotherOrganisation.Id, includeCustomers);
                var handler = new TransferUserToOtherOrganisationCommandHandler(
                    stack.UserReadModelRepository,
                    stack.CustomerReadModelRepository,
                    stack.RoleRepository,
                    stack.HttpContextPropertiesResolver,
                    stack.OrganisationTransferService,
                    stack.OrganisationService,
                    stack.DomainUserService,
                    backgroundJobClient.Object,
                    stack.UserSessionDeletionService);
                await handler.Handle(command, CancellationToken.None);

                customer = stack.CustomerService.GetCustomerById(customerAggregate.TenantId, customerAggregate.Id);

                var quoteDetails = stack.QuoteService.GetQuoteDetails(tenant.Id, quote.Id);
                var filters = new EntityListFilters
                {
                    SearchTerms = new string[] { "example", "com" },
                    Statuses = new string[] { "Customer" },
                    Sources = new string[] { "Policy", "Quote" },
                    CustomerId = customer.Id,
                    OrganisationIds = new List<Guid> { anotherOrganisation.Id },
                };
                var emails = stack.EmailQueryService.GetAll(tenant.Id, filters);
                var user = stack.UserService.GetUser(tenant.Id, userModel.Id);

                // Assert
                customer.Should().NotBeNull();

                backgroundJobClient
                    .Verify(x => x.Create(
                        It.Is<Job>(
                            job => job.Method.Name == "EnqueueTransferUserCustomersToAnotherOrganisation"
                            && job.Args[0].Equals(tenant.Id)
                            && job.Args[1].Equals(anotherOrganisation.Id)
                            && job.Args[2].Equals(userModel.Id)),
                        It.IsAny<EnqueuedState>()));

                backgroundJobClient
                    .Verify(x => x.Create(
                        It.Is<Job>(
                            job => job.Method.Name == "EnqueueTransferUserDetailsToAnotherOrganisation"
                            && job.Args[0].Equals(tenant.Id)
                            && job.Args[1].Equals(userModel.OrganisationId)
                            && job.Args[2].Equals(anotherOrganisation.Id)
                            && job.Args[3].Equals(userModel.Id)
                            && job.Args[4].Equals(includeCustomers)),
                        It.IsAny<EnqueuedState>()));
            }
        }

        private Product GenerateProductFromApplicatioFromApplicationStack(Guid tenantId, ApplicationStack stack)
        {
            var product = ProductFactory.Create(tenantId, Guid.NewGuid());
            stack.ProductRepository.Insert(product);
            stack.ProductRepository.SaveChanges();
            return product;
        }

        private async Task<Tenant> GenerateTenantFromApplicationStack(ApplicationStack stack)
        {
            var guid = Guid.NewGuid();
            var tenantAlias = "test-tenant-" + guid;
            var tenantName = "Test Tenant " + guid;
            var tenantId = await stack.Mediator.Send(new CreateTenantCommand(tenantName, tenantAlias, null));
            return await stack.Mediator.Send(new GetTenantByIdQuery(tenantId));
        }

        private async Task<Organisation> GenerateOrganisationAggregateFromApplicationStack(ApplicationStack stack, Guid tenantId, string name, string alias, Instant instant)
        {
            var organisation = Organisation.CreateNewOrganisation(tenantId, alias, name, null, this.performingUserId, instant);
            await stack.OrganisationAggregateRepository.Save(organisation);
            return organisation;
        }

        private async Task<PersonAggregate> GeneratePersonAggregateFromApplicationStack(Guid tenantId, Guid organisationId, ApplicationStack stack, string email, Instant instant)
        {
            var personAggregate = PersonAggregate.CreatePerson(tenantId, organisationId, this.performingUserId, instant);
            var personCommonProperties = new PersonCommonProperties()
            {
                Email = email,
                FullName = $"My Name {this.randomNumber.Next(1, 100)}",
            };
            personAggregate.Update(new PersonalDetails(tenantId, personCommonProperties), this.performingUserId, instant);
            await stack.PersonAggregateRepository.Save(personAggregate);
            return personAggregate;
        }

        private EmailAndMetadata CreateEmailAndMetadata(QuoteAggregate quoteAggregate, Guid customerId, Instant instant, ApplicationStack stack)
        {
            var emailModel = new EmailModel(quoteAggregate.TenantId, quoteAggregate.OrganisationId, quoteAggregate.ProductId, DeploymentEnvironment.Staging, "from@mail.com", "to@mail.com", "subject", "text", "html");
            return IntegrationEmailMetadataFactory.CreateForQuote(emailModel, quoteAggregate.Id, quoteAggregate.Id, default, Guid.NewGuid(), customerId, Guid.NewGuid(), EmailType.Customer, instant, stack.FileContentRepository);
        }
    }
}
