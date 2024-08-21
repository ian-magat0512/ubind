// <copyright file="CreateCustomerPersonUserAccountCommandHandlerTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Commands.Customer;

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Moq;
using NodaTime;
using UBind.Application.Commands.Customer;
using UBind.Application.Commands.Customer.Merge;
using UBind.Application.Person;
using UBind.Application.Tests.Automations.Fakes;
using UBind.Application.User;
using UBind.Domain;
using UBind.Domain.Aggregates.AdditionalPropertyValue;
using UBind.Domain.Aggregates.Customer;
using UBind.Domain.Aggregates.Person;
using UBind.Domain.Aggregates.User;
using UBind.Domain.Extensions;
using UBind.Domain.ReadModel;
using UBind.Domain.ReadModel.Customer;
using UBind.Domain.Services;
using UBind.Domain.Tests.Fakes;
using Xunit;

public class CreateCustomerPersonUserAccountCommandHandlerTests
{
    private readonly Mock<IPersonService> personServiceMock = new Mock<IPersonService>();
    private readonly Mock<ICustomerService> customerServiceMock = new Mock<ICustomerService>();
    private readonly Mock<Application.User.IUserService> userServiceMock = new Mock<Application.User.IUserService>();
    private readonly Mock<IPersonReadModelRepository> personReadModelRepositoryMock = new Mock<IPersonReadModelRepository>();
    private readonly Mock<IMediator> mediatorMock = new Mock<IMediator>();
    private readonly Mock<IPersonAggregateRepository> personAggregateRepositoryMock = new Mock<IPersonAggregateRepository>();
    private readonly Mock<IHttpContextPropertiesResolver> httpContextPropertiesResolverMock = new Mock<IHttpContextPropertiesResolver>();
    private readonly Mock<IClock> clockMock = new Mock<IClock>();
    private readonly CreateCustomerPersonUserAccountCommandHandler handler;
    private readonly Mock<IUserActivationInvitationService> userActivationInvitationServiceMock = new Mock<IUserActivationInvitationService>();

    public CreateCustomerPersonUserAccountCommandHandlerTests()
    {
        this.handler = new CreateCustomerPersonUserAccountCommandHandler(
            this.customerServiceMock.Object,
            this.personServiceMock.Object,
            this.userServiceMock.Object,
            this.userActivationInvitationServiceMock.Object,
            this.personReadModelRepositoryMock.Object,
            this.personAggregateRepositoryMock.Object,
            this.httpContextPropertiesResolverMock.Object,
            this.mediatorMock.Object,
            this.clockMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldQueryAllPersonsByEmailAndOrganisation_WhenPersonIdIsNull()
    {
        // Arrange
        var tenant = TenantFactory.Create();
        var organisation = new OrganisationReadModel(
            tenant.Id, Guid.NewGuid(), "test-org", "Test Org", null, true, false, SystemClock.Instance.Now());
        var fakePersonalDetails = new FakePersonalDetails();
        var command = new CreateCustomerPersonUserAccountCommand(
            tenant.Id, null, DeploymentEnvironment.Development, fakePersonalDetails, organisation.Id, null);
        this.SetUp();

        // Act
        var customer = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        this.personReadModelRepositoryMock.Verify(
            m => m.GetAllPersonsAssociatedWithCustomerByEmailAndOrganisationId(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DeploymentEnvironment>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldQueryAPersonByIdAndOrganisation_WhenPersonIdIsNotNull()
    {
        // Arrange
        var tenant = TenantFactory.Create();
        var organisation = new OrganisationReadModel(
            tenant.Id, Guid.NewGuid(), "test-org", "Test Org", null, true, false, SystemClock.Instance.Now());
        var fakePersonalDetails = new FakePersonalDetails();
        var command = new CreateCustomerPersonUserAccountCommand(
            tenant.Id,
            null,
            Domain.DeploymentEnvironment.Development,
            fakePersonalDetails,
            organisation.Id,
            Guid.NewGuid());
        this.SetUp();

        // Act
        var customer = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        this.personReadModelRepositoryMock.Verify(
            m => m.GetPersonByIdAndOrganisationId(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_ShouldCreateNewPersonAndCustomer_WhenPersonDoesNotExist()
    {
        // Arrange
        var performingUserId = Guid.NewGuid();
        var person = CreatePerson(performingUserId);
        var customer = CreateNewCustomer(performingUserId, person);
        var request = CreateCustomerPresonUserAccountCommand(person);

        this.personServiceMock
            .Setup(x => x.CreateNewPerson(It.IsAny<Guid>(), It.IsAny<IPersonalDetails>(), It.IsAny<bool>()))
            .ReturnsAsync(person);
        this.customerServiceMock
            .Setup(x => x.CreateCustomerForExistingPerson(
                It.IsAny<PersonAggregate>(),
                It.IsAny<DeploymentEnvironment>(),
                null,
                null,
                It.IsAny<bool>(),
                It.IsAny<List<AdditionalPropertyValueUpsertModel>>()))
            .ReturnsAsync(customer);

        var userAggregate = UserAggregate.CreateUser(
            person.TenantId,
            person.UserId.GetValueOrDefault(),
            UserType.Customer,
            person,
            performingUserId,
            null,
            SystemClock.Instance.Now());

        this.userServiceMock
            .Setup(x => x.CreateUserForPerson(It.IsAny<PersonAggregate>(), It.IsAny<CustomerAggregate>()))
            .ReturnsAsync(userAggregate);

        // Person does not exist
        this.personReadModelRepositoryMock
            .Setup(x => x.GetPersonByIdAndOrganisationId(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(null as PersonReadModel);

        // Act
        var result = await this.handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        this.personServiceMock.Verify(
            x => x.CreateNewPerson(It.IsAny<Guid>(), It.IsAny<PersonAggregate>(), It.IsAny<bool>()), Times.Once);
        this.customerServiceMock.Verify(
            x => x.CreateCustomerForExistingPerson(
                It.IsAny<PersonAggregate>(),
                It.IsAny<DeploymentEnvironment>(),
                null,
                null,
                It.IsAny<bool>(),
                It.IsAny<List<AdditionalPropertyValueUpsertModel>>()),
            Times.Once);
        this.mediatorMock.Verify(
            x => x.Send(
                It.IsAny<MergeCustomerIntoExistingInvitedOrActivatedCustomerCommand>(),
                It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCreateUserAndMergeCustomer_WhenPersonExistsAndHasNoUserId()
    {
        // Arrange
        var performingUserId = Guid.NewGuid();
        var portalId = Guid.NewGuid();
        var person = CreatePerson(performingUserId);
        var customer = CreateNewCustomer(performingUserId, person);
        var user = UserAggregate.CreateUser(
            person.TenantId,
            person.UserId.GetValueOrDefault(),
            UserType.Customer,
            person,
            performingUserId,
            null,
            SystemClock.Instance.Now());
        var request = CreateCustomerPresonUserAccountCommand(person);

        this.personServiceMock
            .Setup(x => x.CreateNewPerson(It.IsAny<Guid>(), It.IsAny<IPersonalDetails>(), It.IsAny<bool>()))
            .ReturnsAsync(person);
        this.customerServiceMock
            .Setup(x => x.CreateCustomerForExistingPerson(
                It.IsAny<PersonAggregate>(),
                It.IsAny<DeploymentEnvironment>(),
                null,
                null,
                It.IsAny<bool>(),
                It.IsAny<List<AdditionalPropertyValueUpsertModel>>()))
            .ReturnsAsync(customer);

        this.personAggregateRepositoryMock
            .Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(person);

        this.userServiceMock
            .Setup(x => x.CreateUserForPerson(It.IsAny<PersonAggregate>(), null))
            .ReturnsAsync(user);

        // Person has no user Id
        var personWithNoUserId = new PersonReadModel(person.Id);

        var userAggregate = UserAggregate.CreateUser(
            person.TenantId,
            person.UserId.GetValueOrDefault(),
            UserType.Customer,
            person,
            performingUserId,
            null,
            SystemClock.Instance.Now());
        this.userServiceMock
            .Setup(x => x.CreateUserForPerson(It.IsAny<PersonAggregate>(), It.IsAny<CustomerAggregate>()))
            .ReturnsAsync(userAggregate);

        this.personReadModelRepositoryMock
            .Setup(x => x.GetPersonByIdAndOrganisationId(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(personWithNoUserId);

        // Act
        var result = await this.handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().Be(request.PersonId.Value);
        this.personAggregateRepositoryMock.Verify(x => x.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);

        this.userServiceMock.Verify(x => x.CreateUserForPerson(person, It.IsAny<CustomerAggregate>()), Times.Once);
        this.mediatorMock.Verify(
            x => x.Send(
                It.IsAny<MergeCustomerIntoExistingInvitedOrActivatedCustomerCommand>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldMergeCustomer_WhenPersonExistsAndHasUserId()
    {
        // Arrange
        var performingUserId = Guid.NewGuid();
        var person = CreatePerson(performingUserId);
        var request = CreateCustomerPresonUserAccountCommand(person);

        // Person has user Id
        var personWithUserId = new PersonReadModel(person.Id)
        {
            UserId = Guid.NewGuid(),
        };

        this.personReadModelRepositoryMock
            .Setup(x => x.GetPersonByIdAndOrganisationId(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(personWithUserId);

        // Act
        var result = await this.handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().Be(request.PersonId.Value);
        this.mediatorMock.Verify(
            x => x.Send(
                It.IsAny<MergeCustomerIntoExistingInvitedOrActivatedCustomerCommand>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static CreateCustomerPersonUserAccountCommand CreateCustomerPresonUserAccountCommand(PersonAggregate person)
    {
        return new CreateCustomerPersonUserAccountCommand(
            TenantFactory.DefaultId,
            null,
            DeploymentEnvironment.Staging,
            person,
            person.OrganisationId,
            person.Id);
    }

    private static PersonAggregate CreatePerson(Guid performingUserId)
    {
        return PersonAggregate.CreatePerson(
            TenantFactory.DefaultId,
            Guid.NewGuid(),
            performingUserId,
            SystemClock.Instance.Now());
    }

    private static CustomerAggregate CreateNewCustomer(Guid performingUserId, PersonAggregate person)
    {
        return CustomerAggregate.CreateNewCustomer(
            TenantFactory.DefaultId,
            person,
            DeploymentEnvironment.Development,
            performingUserId,
            Guid.NewGuid(),
            SystemClock.Instance.Now());
    }

    private void SetUp()
    {
        var fakePersonalDetails = new FakePersonReadModel(Guid.NewGuid())
        {
            UserId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
        };
        this.personReadModelRepositoryMock
            .Setup(r => r.GetPersonByIdAndOrganisationId(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(fakePersonalDetails);
        var customerReadModelSummaryMock = new Mock<ICustomerReadModelSummary>();
        this.customerServiceMock.Setup(s => s.GetCustomerById(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(customerReadModelSummaryMock.Object);
        this.personReadModelRepositoryMock
            .Setup(r => r.GetAllPersonsAssociatedWithCustomerByEmailAndOrganisationId(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DeploymentEnvironment>()))
            .Returns(new List<PersonReadModel> { fakePersonalDetails });
    }
}
