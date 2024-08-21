// <copyright file="MergeSourceToTargetCustomerCommandHandlerTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Commands.Customer;

using DotLiquid.Util;
using FluentAssertions;
using Hangfire;
using Hangfire.States;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using System;
using System.Threading;
using System.Threading.Tasks;
using UBind.Application.Commands.Customer.Merge;
using UBind.Application.Queries.Email;
using UBind.Application.Queries.Sms;
using UBind.Application.User;
using UBind.Domain;
using UBind.Domain.Aggregates.Claim;
using UBind.Domain.Aggregates.Customer;
using UBind.Domain.Aggregates.Person;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Aggregates.User;
using UBind.Domain.Entities;
using UBind.Domain.Extensions;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Permissions;
using UBind.Domain.ReadModel;
using UBind.Domain.ReadModel.Claim;
using UBind.Domain.ReadModel.Customer;
using UBind.Domain.Repositories;
using UBind.Domain.Tests.Fakes;
using UBind.Persistence.ReadModels.Email;
using Xunit;

public class MergeSourceToTargetCustomerCommandHandlerTest
{
    private readonly Mock<IPersonReadModelRepository> personReadModelRepositoryMock = new Mock<IPersonReadModelRepository>();
    private readonly Mock<ICqrsMediator> cqrsMediatorMock = new Mock<ICqrsMediator>();
    private readonly Mock<IClaimAggregateRepository> claimAggregateRepositoryMock = new Mock<IClaimAggregateRepository>();
    private readonly Mock<IClaimReadModelRepository> claimReadModelRepositoryMock = new Mock<IClaimReadModelRepository>();
    private readonly Mock<IPersonAggregateRepository> personAggregateRepositoryMock = new Mock<IPersonAggregateRepository>();
    private readonly Mock<IHttpContextPropertiesResolver> httpContextPropertiesResolverMock = new Mock<IHttpContextPropertiesResolver>();
    private readonly Mock<IClock> clockMock = new Mock<IClock>();
    private readonly MergeCustomerIntoExistingInvitedOrActivatedCustomerCommandHandler handler2;
    private readonly MergeSourceToTargetCustomerCommandHandler handler;
    private readonly Mock<ICustomerAggregateRepository> customerAggregateRepositoryMock = new Mock<ICustomerAggregateRepository>();
    private readonly Mock<ICustomerReadModelRepository> customerReadModelRepositoryMock = new Mock<ICustomerReadModelRepository>();
    private readonly Mock<IEmailRepository> emailRepositoryMock = new Mock<IEmailRepository>();
    private readonly Mock<IQuoteAggregateRepository> quoteAggregateRepositoryMock = new Mock<IQuoteAggregateRepository>();
    private readonly Mock<IQuoteReadModelRepository> quoteReadModelRepositoryMock = new Mock<IQuoteReadModelRepository>();
    private readonly Mock<ISmsRepository> smsRepositoryMock = new Mock<ISmsRepository>();
    private readonly Mock<IUserReadModelRepository> userReadModelRepositoryMock = new Mock<IUserReadModelRepository>();
    private readonly Mock<ICachingResolver> cachingResolverMock = new Mock<ICachingResolver>();
    private readonly Mock<IUserService> userServiceMock = new Mock<IUserService>();
    private readonly Mock<IUserAggregateRepository> userAggregateRepositoryMock = new Mock<IUserAggregateRepository>();
    private readonly Mock<IBackgroundJobClient> backgroundJobClientMock = new Mock<IBackgroundJobClient>();
    private readonly Mock<ILogger<MergeSourceToTargetCustomerCommandHandler>> loggerMock =
        new Mock<ILogger<MergeSourceToTargetCustomerCommandHandler>>();
    private readonly Mock<ILogger<MergeCustomerIntoExistingInvitedOrActivatedCustomerCommandHandler>> logger2Mock =
        new Mock<ILogger<MergeCustomerIntoExistingInvitedOrActivatedCustomerCommandHandler>>();

    public MergeSourceToTargetCustomerCommandHandlerTest()
    {
        this.handler2 = new MergeCustomerIntoExistingInvitedOrActivatedCustomerCommandHandler(
            this.claimAggregateRepositoryMock.Object,
            this.claimReadModelRepositoryMock.Object,
            this.customerAggregateRepositoryMock.Object,
            this.customerReadModelRepositoryMock.Object,
            this.emailRepositoryMock.Object,
            this.personAggregateRepositoryMock.Object,
            this.personReadModelRepositoryMock.Object,
            this.userServiceMock.Object,
            this.quoteAggregateRepositoryMock.Object,
            this.quoteReadModelRepositoryMock.Object,
            this.smsRepositoryMock.Object,
            this.httpContextPropertiesResolverMock.Object,
            this.cqrsMediatorMock.Object,
            this.backgroundJobClientMock.Object,
            this.logger2Mock.Object,
            this.clockMock.Object,
            this.userAggregateRepositoryMock.Object);

        this.handler = new MergeSourceToTargetCustomerCommandHandler(
            this.claimAggregateRepositoryMock.Object,
            this.claimReadModelRepositoryMock.Object,
            this.customerAggregateRepositoryMock.Object,
            this.customerReadModelRepositoryMock.Object,
            this.emailRepositoryMock.Object,
            this.personAggregateRepositoryMock.Object,
            this.personReadModelRepositoryMock.Object,
            this.quoteAggregateRepositoryMock.Object,
            this.quoteReadModelRepositoryMock.Object,
            this.smsRepositoryMock.Object,
            this.userReadModelRepositoryMock.Object,
            this.httpContextPropertiesResolverMock.Object,
            this.cachingResolverMock.Object,
            this.cqrsMediatorMock.Object,
            this.loggerMock.Object,
            this.clockMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeleteSourceCustomer_OnProcess()
    {
        // Arrange
        var tenant = TenantFactory.Create();

        // setup customers and aggregates
        var paramSet = this.SetUp(tenant.Id);

        // Act
        var command = new MergeSourceToTargetCustomerCommand(tenant.Id, paramSet.SourceCustomerAggregate.Id, paramSet.TargetCustomerAggregate.Id);
        var customer = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        paramSet.SourceCustomerAggregate.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_QuoteShouldTransferCustomerAndOwner_OnProcess()
    {
        // Arrange
        var tenant = TenantFactory.Create();

        // setup customers and aggregates
        var paramSet = this.SetUp(tenant.Id);

        // Act
        var command = new MergeSourceToTargetCustomerCommand(tenant.Id, paramSet.SourceCustomerAggregate.Id, paramSet.TargetCustomerAggregate.Id);
        var customer = await this.handler.Handle(command, CancellationToken.None);

        // Assert
        paramSet.SourceCustomerAggregate.IsDeleted.Should().BeTrue();
        paramSet.SourceQuoteAggregate.CustomerId.Should().Be(paramSet.TargetCustomerAggregate.Id);
        paramSet.SourceQuoteAggregate.OwnerUserId.Should().Be(paramSet.TargetCustomerAggregate.OwnerUserId);
    }

    [Fact]
    public async Task Handle_ClaimShouldTransferCustomerAndOwner_OnProcess()
    {
        // Arrange
        var tenant = TenantFactory.Create();

        // setup customers and aggregates
        var paramSet = this.SetUp(tenant.Id);

        // Act
        var command = new MergeSourceToTargetCustomerCommand(tenant.Id, paramSet.SourceCustomerAggregate.Id, paramSet.TargetCustomerAggregate.Id);
        await this.handler.Handle(command, CancellationToken.None);

        // Assert
        paramSet.SourceCustomerAggregate.IsDeleted.Should().BeTrue();
        paramSet.SourceClaimAggregate.CustomerId.Should().Be(paramSet.TargetCustomerAggregate.Id);
        paramSet.SourceClaimAggregate.OwnerUserId.Should().Be(paramSet.TargetCustomerAggregate.OwnerUserId);
    }

    [Fact]
    public async Task Handle_ShouldTransferMessages_OnProcess()
    {
        // Arrange
        var tenant = TenantFactory.Create();

        // setup customers and aggregates
        var paramSet = this.SetUp(tenant.Id);

        // Act
        var command = new MergeSourceToTargetCustomerCommand(tenant.Id, paramSet.SourceCustomerAggregate.Id, paramSet.TargetCustomerAggregate.Id);
        await this.handler.Handle(command, CancellationToken.None);

        // Assert
        paramSet.SourceCustomerAggregate.IsDeleted.Should().BeTrue();
        this.smsRepositoryMock.Verify(
                m => m.RemoveSmsRelationship(It.IsAny<Domain.ReadWriteModel.Relationship>()),
                Times.Exactly(2));
        this.smsRepositoryMock.Verify(
               m => m.InsertSmsRelationship(It.IsAny<Domain.ReadWriteModel.Relationship>()),
               Times.Exactly(2));
        this.emailRepositoryMock.Verify(
                m => m.RemoveEmailRelationship(It.IsAny<Domain.ReadWriteModel.Relationship>()),
                Times.Exactly(2));
        this.emailRepositoryMock.Verify(
                m => m.RemoveEmailRelationship(It.IsAny<Domain.ReadWriteModel.Relationship>()),
                Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_ShouldTransferPersons_WhenHasSimilarEmailButBothUninvited()
    {
        // Arrange
        var tenant = TenantFactory.Create();

        // setup customers and aggregates
        var paramSet = this.SetUp(tenant.Id);
        var sourceAdditionalPerson = this.CreateAdditionalPerson(tenant.Id, paramSet.SourceCustomerAggregate, "test@gmail.com", false);
        var destinationAddtionalPerson = this.CreateAdditionalPerson(tenant.Id, paramSet.TargetCustomerAggregate, "test@gmail.com", false);

        // Act
        var command = new MergeSourceToTargetCustomerCommand(tenant.Id, paramSet.SourceCustomerAggregate.Id, paramSet.TargetCustomerAggregate.Id);
        await this.handler.Handle(command, CancellationToken.None);

        // Assert
        paramSet.SourceCustomerAggregate.IsDeleted.Should().BeTrue();
        sourceAdditionalPerson.CustomerId.Should().Be(paramSet.TargetCustomerAggregate.Id);
        destinationAddtionalPerson.CustomerId.Should().Be(paramSet.TargetCustomerAggregate.Id);
    }

    [Fact]
    public async Task Handle_ShouldTransferPersons_WhenHasSimilarEmailAndSourcePersonWasInvited()
    {
        // Arrange
        var tenant = TenantFactory.Create();

        // setup customers and aggregates
        var paramSet = this.SetUp(tenant.Id);
        var sourceAdditionalPerson = this.CreateAdditionalPerson(tenant.Id, paramSet.SourceCustomerAggregate, "test@gmail.com", true);
        var destinationAddtionalPerson = this.CreateAdditionalPerson(tenant.Id, paramSet.TargetCustomerAggregate, "test@gmail.com", false);

        // Act
        var command = new MergeSourceToTargetCustomerCommand(tenant.Id, paramSet.SourceCustomerAggregate.Id, paramSet.TargetCustomerAggregate.Id);
        await this.handler.Handle(command, CancellationToken.None);

        // Assert
        paramSet.SourceCustomerAggregate.IsDeleted.Should().BeTrue();
        sourceAdditionalPerson.CustomerId.Should().Be(paramSet.TargetCustomerAggregate.Id);
        destinationAddtionalPerson.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldTransferPersons_WhenHasSimilarEmailAndTargetPersonWasInvited()
    {
        // Arrange
        var tenant = TenantFactory.Create();

        // setup customers and aggregates
        var paramSet = this.SetUp(tenant.Id);
        var sourceAdditionalPerson = this.CreateAdditionalPerson(tenant.Id, paramSet.SourceCustomerAggregate, "test@gmail.com", false);
        var destinationAddtionalPerson = this.CreateAdditionalPerson(tenant.Id, paramSet.TargetCustomerAggregate, "test@gmail.com", true);

        // Act
        var command = new MergeSourceToTargetCustomerCommand(tenant.Id, paramSet.SourceCustomerAggregate.Id, paramSet.TargetCustomerAggregate.Id);
        await this.handler.Handle(command, CancellationToken.None);

        // Assert
        paramSet.SourceCustomerAggregate.IsDeleted.Should().BeTrue();
        sourceAdditionalPerson.IsDeleted.Should().BeTrue();
        destinationAddtionalPerson.CustomerId.Should().Be(paramSet.TargetCustomerAggregate.Id);
    }

    [Fact]
    public async Task Handle_ReplaceDestinationPerson_OnProcess()
    {
        // Arrange
        var tenant = TenantFactory.Create();

        // setup customers and aggregates
        var paramSet = this.SetUp(tenant.Id, "test@gmail.com");

        // Act
        var command = new MergeCustomerIntoExistingInvitedOrActivatedCustomerCommand(tenant.Id, DeploymentEnvironment.Development, paramSet.TargetPersonAggregate.Id);
        await this.handler2.Handle(command, CancellationToken.None);

        // Assert
        paramSet.SourceCustomerAggregate.IsDeleted.Should().IsFalsy();
        this.backgroundJobClientMock.Verify(
            x =>
                x.Create(
                It.IsAny<Hangfire.Common.Job>(),
                It.IsAny<IState>()),
                Times.Once);
    }

    private static PersonAggregate CreatePerson(Guid performingUserId, Guid organisationId)
    {
        return PersonAggregate.CreatePerson(
            TenantFactory.DefaultId,
            organisationId,
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

    private ParamSet SetUp(Guid tenantId, string email = null)
    {
        var paramSet = new ParamSet();

        // set source
        Guid sourcePerformingUserId = Guid.NewGuid();
        Guid organisationId = Guid.NewGuid();
        paramSet.SourcePersonAggregate = CreatePerson(sourcePerformingUserId, organisationId);
        paramSet.SourcePersonAggregate.UpdateEmail(email, sourcePerformingUserId, this.clockMock.Object.Now());
        paramSet.SourceCustomerAggregate = CreateNewCustomer(sourcePerformingUserId, paramSet.SourcePersonAggregate);
        paramSet.SourcePersonAggregate.AssociateWithCustomer(paramSet.SourceCustomerAggregate.Id, sourcePerformingUserId, this.clockMock.Object.Now());
        paramSet.SourceCustomerAggregate.AssignOwnership(sourcePerformingUserId, paramSet.SourcePersonAggregate, sourcePerformingUserId, this.clockMock.Object.Now());
        this.customerReadModelRepositoryMock.Setup(x => x.GetCustomerById(tenantId, paramSet.SourceCustomerAggregate.Id, true))
            .Returns(
                new CustomerReadModelDetail
                {
                    Id = paramSet.SourceCustomerAggregate.Id,
                    PrimaryPersonId = paramSet.SourceCustomerAggregate.PrimaryPersonId,
                    TenantId = paramSet.SourceCustomerAggregate.TenantId,
                    OwnerUserId = paramSet.SourceCustomerAggregate.OwnerUserId,
                    OwnerPersonId = paramSet.SourcePersonAggregate.Id,
                    OrganisationId = organisationId,
                });
        this.customerAggregateRepositoryMock.Setup(m => m.GetById(paramSet.SourcePersonAggregate.TenantId, paramSet.SourceCustomerAggregate.Id))
            .Returns(paramSet.SourceCustomerAggregate);
        this.personAggregateRepositoryMock.Setup(x => x.GetById(tenantId, paramSet.SourceCustomerAggregate.PrimaryPersonId)).Returns(paramSet.SourcePersonAggregate);
        this.personReadModelRepositoryMock.Setup(x => x.GetPersonById(tenantId, paramSet.SourcePersonAggregate.Id))
            .Returns(new PersonReadModel(paramSet.SourcePersonAggregate.Id)
            {
                UserId = paramSet.SourceCustomerAggregate.OwnerUserId,
                FullName = "sample",
            });

        var user = new Domain.ReadModel.User.UserReadModel(
            Guid.NewGuid(),
            new PersonData(paramSet.SourcePersonAggregate),
            null,
            null,
            this.clockMock.Object.Now(),
            UserType.Customer);
        var defaultRolePermissionsRegistry = new DefaultRolePermissionsRegistry();
        var defaultRoleNameRegistry = new DefaultRoleNameRegistry();
        Role.SetDefaultRolePermissionsRegistry(defaultRolePermissionsRegistry);
        Role.SetDefaultRoleNameRegistry(defaultRoleNameRegistry);
        var roles = new List<Role>();
        var role = new Role(Guid.NewGuid(), Guid.Empty, DefaultRole.TenantAdmin, this.clockMock.Object.Now());
        roles.Add(role);
        user.Roles = roles;
        this.userReadModelRepositoryMock.Setup(x => x.GetUserWithRoles(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(user);

        // setup related persons with same email.
        this.personReadModelRepositoryMock.Setup(x =>
            x.GetAllPersonsAssociatedWithCustomerByEmailAndOrganisationId(tenantId, It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DeploymentEnvironment>()))
            .Returns(
                new List<PersonReadModel>()
                {
                    new PersonReadModel(paramSet.SourcePersonAggregate.Id)
                    {
                        UserId = paramSet.SourcePersonAggregate.UserId,
                        CustomerId = paramSet.SourcePersonAggregate.CustomerId,
                        FullName = "sample",
                    },
                });
        this.CreateQuoteForSource(tenantId, paramSet);
        this.CreateMessagesForSource(tenantId, paramSet);
        this.CreateClaimForSource(tenantId, paramSet);

        // set target.
        Guid targetPerformingUserId = Guid.NewGuid();
        paramSet.TargetPersonAggregate = CreatePerson(targetPerformingUserId, organisationId);
        paramSet.TargetPersonAggregate.UpdateEmail(email, targetPerformingUserId, this.clockMock.Object.Now());
        paramSet.TargetCustomerAggregate = CreateNewCustomer(targetPerformingUserId, paramSet.TargetPersonAggregate);
        paramSet.TargetPersonAggregate.AssociateWithCustomer(paramSet.TargetCustomerAggregate.Id, targetPerformingUserId, this.clockMock.Object.Now());
        paramSet.TargetCustomerAggregate.AssignOwnership(targetPerformingUserId, paramSet.TargetPersonAggregate, targetPerformingUserId, this.clockMock.Object.Now());
        this.customerReadModelRepositoryMock.Setup(x => x.GetCustomerById(tenantId, paramSet.TargetCustomerAggregate.Id, true))
        .Returns(
            new CustomerReadModelDetail
            {
                Id = paramSet.TargetCustomerAggregate.Id,
                PrimaryPersonId = paramSet.TargetCustomerAggregate.PrimaryPersonId,
                TenantId = paramSet.TargetCustomerAggregate.TenantId,
                OwnerUserId = paramSet.TargetCustomerAggregate.OwnerUserId,
                OwnerPersonId = paramSet.TargetPersonAggregate.Id,
                OrganisationId = organisationId,
            });
        this.customerAggregateRepositoryMock.Setup(m => m.GetById(paramSet.TargetPersonAggregate.TenantId, paramSet.TargetCustomerAggregate.Id))
            .Returns(paramSet.TargetCustomerAggregate);
        this.personAggregateRepositoryMock.Setup(x => x.GetById(tenantId, paramSet.TargetCustomerAggregate.PrimaryPersonId)).Returns(paramSet.TargetPersonAggregate);
        this.personReadModelRepositoryMock.Setup(x => x.GetPersonById(tenantId, paramSet.TargetPersonAggregate.Id))
            .Returns(new PersonReadModel(paramSet.TargetPersonAggregate.Id)
            {
                UserId = paramSet.TargetCustomerAggregate.OwnerUserId,
                FullName = "sample",
            });

        return paramSet;
    }

    private PersonAggregate CreateAdditionalPerson(Guid tenantId, CustomerAggregate customerAggregate, string email, bool invited)
    {
        var personAggregate = PersonAggregate.CreatePerson(
            TenantFactory.DefaultId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            SystemClock.Instance.Now());
        personAggregate.UpdateEmail(email, null, this.clockMock.Object.Now());
        personAggregate.AssociateWithCustomer(customerAggregate.Id, Guid.NewGuid(), this.clockMock.Object.Now());
        if (invited)
        {
            personAggregate.AssociateWithUserAccount(Guid.NewGuid(), null, this.clockMock.Object.Now());
        }

        List<IPersonReadModelSummary> personReadModelSummary = new List<IPersonReadModelSummary>();
        personReadModelSummary.Add(
            new PersonReadModelSummary()
            {
                Id = personAggregate.Id,
                UserId = customerAggregate.OwnerUserId,
                FullName = "sample",
                Email = email,
            });

        this.personReadModelRepositoryMock.Setup(x => x.GetPersonsByCustomerId(tenantId, customerAggregate.Id, true))
            .Returns(personReadModelSummary);
        this.personAggregateRepositoryMock.Setup(x => x.GetById(tenantId, personAggregate.Id))
            .Returns(personAggregate);
        return personAggregate;
    }

    private void CreateClaimForSource(Guid tenantId, ParamSet paramSet)
    {
        var sourceClaimAggregate = ClaimAggregate.CreateWithoutPolicy(
            tenantId,
            paramSet.SourceCustomerAggregate.OrganisationId,
            Guid.NewGuid(),
            paramSet.SourceCustomerAggregate.Environment,
            "SampleClaim",
            false,
            paramSet.SourceCustomerAggregate.Id,
            paramSet.SourceCustomerAggregate.PrimaryPersonId,
            "full name",
            "preferred name",
            paramSet.SourceCustomerAggregate.OwnerUserId,
            this.clockMock.Object.Now(),
            DateTimeZone.Utc);
        paramSet.SourceClaimAggregate = sourceClaimAggregate;
        this.claimAggregateRepositoryMock.Setup(m => m.GetById(tenantId, sourceClaimAggregate.Id))
            .Returns(paramSet.SourceClaimAggregate);
        this.claimReadModelRepositoryMock.Setup(x => x.ListClaims(tenantId, It.IsAny<EntityListFilters>()))
            .Returns(new List<IClaimReadModelSummary>
            {
                new ClaimReadModelDetails()
                {
                    TenantId = tenantId,
                    Id = sourceClaimAggregate.Id,
                },
            });
    }

    private void CreateMessagesForSource(Guid tenantId, ParamSet paramSet)
    {
        this.cqrsMediatorMock.Setup(x => x.Send(It.IsAny<GetAllSmsByFilterQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                var list = new List<Domain.ReadWriteModel.Sms>();
                var sms = new Domain.ReadWriteModel.Sms(
                    tenantId,
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    "to",
                    "from",
                    "message",
                    this.clockMock.Object.Now());
                list.Add(sms);
                foreach (var item in list)
                {
                    List<Domain.ReadWriteModel.Relationship> relationships = new List<Domain.ReadWriteModel.Relationship>();
                    relationships.Add(
                        new Domain.ReadWriteModel.Relationship(
                            tenantId,
                            EntityType.Customer,
                            Guid.NewGuid(),
                            RelationshipType.CustomerMessage,
                            EntityType.Message,
                            sms.Id,
                            this.clockMock.Object.Now()));
                    relationships.Add(
                        new Domain.ReadWriteModel.Relationship(
                            tenantId,
                            EntityType.Person,
                            Guid.NewGuid(),
                            RelationshipType.MessageRecipient,
                            EntityType.Message,
                            sms.Id,
                            this.clockMock.Object.Now()));
                    this.smsRepositoryMock.Setup(x => x.GetSmsRelationships(tenantId, sms.Id, null))
                    .Returns(relationships);
                }

                return list;
            });

        this.cqrsMediatorMock.Setup(x => x.Send(It.IsAny<GetEmailSummariesByFilterQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                var list = new List<IEmailSummary>();
                var email = new EmailSummary()
                {
                    TenantId = tenantId,
                    Id = Guid.NewGuid(),
                };
                list.Add(email);
                foreach (var item in list)
                {
                    List<Domain.ReadWriteModel.Relationship> relationships = new List<Domain.ReadWriteModel.Relationship>();
                    relationships.Add(
                        new Domain.ReadWriteModel.Relationship(
                            tenantId,
                            EntityType.Customer,
                            Guid.NewGuid(),
                            RelationshipType.CustomerMessage,
                            EntityType.Message,
                            email.Id,
                            this.clockMock.Object.Now()));
                    relationships.Add(
                        new Domain.ReadWriteModel.Relationship(
                            tenantId,
                            EntityType.Person,
                            Guid.NewGuid(),
                            RelationshipType.MessageRecipient,
                            EntityType.Message,
                            email.Id,
                            this.clockMock.Object.Now()));
                    this.emailRepositoryMock.Setup(x => x.GetRelationships(tenantId, email.Id, null))
                    .Returns(relationships);
                }

                return list;
            });
    }

    private void CreateQuoteForSource(Guid tenantId, ParamSet paramSet)
    {
        var sourceQuote = QuoteAggregate.CreateNewBusinessQuote(
            tenantId,
            paramSet.SourceCustomerAggregate.OrganisationId,
            Guid.NewGuid(),
            paramSet.SourceCustomerAggregate.Environment,
            new QuoteExpirySettings(30, false),
            paramSet.SourceCustomerAggregate.OwnerUserId,
            this.clockMock.Object.Now(),
            null,
            null,
            false,
            paramSet.SourceCustomerAggregate.Id,
            false);
        paramSet.SourceQuoteAggregate = sourceQuote.Aggregate;
        this.quoteAggregateRepositoryMock.Setup(m => m.GetById(tenantId, sourceQuote.Aggregate.Id))
            .Returns(sourceQuote.Aggregate);
        this.quoteReadModelRepositoryMock.Setup(x => x.ListQuotes(tenantId, It.IsAny<QuoteReadModelFilters>()))
            .Returns(new List<IQuoteReadModelSummary>
            {
                new FakeQuoteReadModelSummary(tenantId, sourceQuote.Aggregate.Id, sourceQuote.Id)
            });
    }

    private class ParamSet
    {
        public PersonAggregate SourcePersonAggregate { get; set; }

        public CustomerAggregate SourceCustomerAggregate { get; set; }

        public QuoteAggregate SourceQuoteAggregate { get; set; }

        public ClaimAggregate SourceClaimAggregate { get; set; }

        public PersonAggregate TargetPersonAggregate { get; set; }

        public CustomerAggregate TargetCustomerAggregate { get; set; }
    }
}
