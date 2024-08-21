// <copyright file="OrganisationServiceTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Tests.Services
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using NodaTime;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Authentication;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    /// <summary>
    /// Unit testing for organisation service.
    /// </summary>
    public class OrganisationServiceTests
    {
        private readonly Mock<IOrganisationAggregateRepository> organisationAggregateRepository
            = new Mock<IOrganisationAggregateRepository>();

        private readonly Mock<IOrganisationReadModelRepository> organisationReadModelRepository
            = new Mock<IOrganisationReadModelRepository>();

        private readonly Mock<ITenantRepository> tenantRepository = new Mock<ITenantRepository>();

        private readonly Mock<IHttpContextPropertiesResolver> httpContextPropertiesResolver = new Mock<IHttpContextPropertiesResolver>();

        private readonly Mock<IAdditionalPropertyValueService> additionalPropertyValueService =
            new Mock<IAdditionalPropertyValueService>();

        private readonly IOrganisationService organisationService;

        private TestClock clock = new TestClock();

        private Guid? performingUserId = Guid.NewGuid();

        private Mock<ICqrsMediator> mediator = new Mock<ICqrsMediator>();
        private Mock<ICachingResolver> cachingResolver = new Mock<ICachingResolver>();

        public OrganisationServiceTests()
        {
            this.organisationService = new OrganisationService(
                this.organisationAggregateRepository.Object,
                this.organisationReadModelRepository.Object,
                this.tenantRepository.Object,
                this.additionalPropertyValueService.Object,
                this.httpContextPropertiesResolver.Object,
                this.mediator.Object,
                this.cachingResolver.Object,
                this.clock,
                Mock.Of<IProductRepository>(),
                Mock.Of<IProductFeatureSettingRepository>(),
                Mock.Of<IProductOrganisationSettingRepository>());
        }

        [Fact]
        public void GetOrganisationByAlias_ShouldReturn_ValidOrganisationReadModel()
        {
            // Arrange
            var tenant = TenantFactory.Create(Guid.NewGuid());
            this.tenantRepository
                .Setup(t => t.GetTenantById(It.IsAny<Guid>()))
                .Returns(tenant);

            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null, this.performingUserId,
                this.clock.GetCurrentInstant());
            this.organisationAggregateRepository
                .Setup(o => o.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(organisation);

            var organisationReadModel = new OrganisationReadModel(
                tenant.Id,
                organisation.Id,
                organisation.Alias,
                organisation.Name,
                null, organisation.IsActive,
                organisation.IsDeleted,
                organisation.CreatedTimestamp);
            this.organisationReadModelRepository
               .Setup(o => o.Get(It.IsAny<Guid>(), It.IsAny<Guid>()))
               .Returns(organisationReadModel);

            // Action
            Action validationAction
                = () => this.organisationService.ValidateOrganisationBelongsToTenantAndIsActive(tenant.Id, organisation.Id);

            // Assert
            validationAction.Should().NotThrow();
        }

        [Fact]
        public async Task GetOrganisationByAlias_ThrowsErrorException_WhenOrganisationIsDisabled()
        {
            // Arrange
            var tenant = TenantFactory.Create(Guid.NewGuid());
            this.tenantRepository
                .Setup(t => t.GetTenantById(It.IsAny<Guid>()))
                .Returns(tenant);

            var disabledOrganisation = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null,
                this.performingUserId,
                this.clock.GetCurrentInstant());
            disabledOrganisation.Disable(this.performingUserId, this.clock.GetCurrentInstant());
            var disabledOrganisationReadModel = new OrganisationReadModel(
                tenant.Id,
                disabledOrganisation.Id,
                disabledOrganisation.Alias,
                disabledOrganisation.Name,
                null,
                disabledOrganisation.IsActive,
                disabledOrganisation.IsDeleted,
                disabledOrganisation.CreatedTimestamp);
            this.organisationReadModelRepository
               .Setup(o => o.Get(It.IsAny<Guid>(), It.IsAny<Guid>()))
               .Returns(disabledOrganisationReadModel);
            this.organisationAggregateRepository
                .Setup(o => o.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(disabledOrganisation);
            this.cachingResolver.Setup(s => s.GetTenantOrThrow(tenant.Id)).ReturnsAsync(tenant);
            this.cachingResolver.Setup(s => s.GetOrganisationOrThrow(It.IsAny<Guid>(), disabledOrganisation.Id))
                .ReturnsAsync(disabledOrganisationReadModel);

            // Act
            Func<Task> validateDisabledAction = async () => await this.organisationService.ValidateOrganisationBelongsToTenantAndIsActive(
                tenant.Id, disabledOrganisation.Id);

            // Assert
            await validateDisabledAction
                .Should()
                .ThrowAsync<ErrorException>()
                .WithMessage(Errors.Organisation.Login.Disabled(disabledOrganisation.Name).ToString());
        }

        [Fact]
        public async Task Get_ShouldDisplayOrganisationDetailCorrectly()
        {
            // Arrange
            var tenant = TenantFactory.Create(Guid.NewGuid());
            var userAuthData = new UserAuthenticationData(
                tenant.Id, tenant.Details.DefaultOrganisationId, UserType.Client, Guid.NewGuid(), default);
            this.tenantRepository
                .Setup(t => t.GetTenantById(It.IsAny<Guid>()))
                .Returns(tenant);
            this.cachingResolver.Setup(c => c.GetTenantOrThrow(It.IsAny<Guid>()))
                .ReturnsAsync(tenant);

            Guid organisationId = Guid.NewGuid();
            this.organisationReadModelRepository
                .Setup(o => o.Get(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new OrganisationReadModel(
                    tenant.Id,
                    organisationId,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null, true,
                    false,
                    this.clock.GetCurrentInstant()));

            var organisationAggregate = await this.organisationService
                .CreateActiveNonDefaultAsync(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                new List<Domain.Aggregates.AdditionalPropertyValue.AdditionalPropertyValueUpsertModel>());

            // Act
            var details = await this.organisationService.GetOrganisationById(userAuthData.TenantId, organisationId);

            // Assert
            details.Should().NotBeNull();
            details.Alias.Should().Be(organisationAggregate.Alias);
            details.Name.Should().Be(organisationAggregate.Name);
            details.IsActive.Should().Be(organisationAggregate.IsActive);
            details.IsDeleted.Should().Be(organisationAggregate.IsDeleted);
        }

        [Fact]
        public async Task CreateActiveNonDefaultAsync_NonDefaultOrganisation_ShouldInsert()
        {
            // Arrange
            var tenant = TenantFactory.Create(Guid.NewGuid());
            this.tenantRepository
                .Setup(t => t.GetTenantById(It.IsAny<Guid>()))
                .Returns(tenant);
            var userAuthData = new UserAuthenticationData(
                tenant.Id, tenant.Details.DefaultOrganisationId, UserType.Client, Guid.NewGuid(), default);

            Guid organisationId = Guid.NewGuid();
            this.organisationReadModelRepository
                .Setup(o => o.Get(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new OrganisationReadModel(
                    tenant.Id,
                    organisationId,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null, true,
                    false,
                    this.clock.GetCurrentInstant()));

            // Act
            var organisationAggregate = await this.organisationService
                .CreateActiveNonDefaultAsync(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                new List<Domain.Aggregates.AdditionalPropertyValue.AdditionalPropertyValueUpsertModel>());

            // Assert
            organisationAggregate.Should().NotBeNull();
            organisationAggregate.Alias.Should().Be(tenant.Details.Alias);
            organisationAggregate.Name.Should().Be(tenant.Details.Name);
            organisationAggregate.IsActive.Should().BeTrue();
            organisationAggregate.IsDeleted.Should().BeFalse();
        }

        [Fact]
        public async Task CreateDefaultAsync_ShouldSetDefaultOrganisation_ForTenantDetails()
        {
            // Arrange
            var tenant = TenantFactory.Create(Guid.NewGuid());
            var userAuthData = new UserAuthenticationData(
                tenant.Id, tenant.Details.DefaultOrganisationId, UserType.Client, Guid.NewGuid(), default);
            this.tenantRepository
                .Setup(t => t.GetTenantById(It.IsAny<Guid>()))
                .Returns(tenant);

            Guid organisationId = Guid.NewGuid();
            this.organisationReadModelRepository
                .Setup(o => o.Get(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new OrganisationReadModel(
                    tenant.Id,
                    organisationId,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null,
                    true,
                    false,
                    this.clock.GetCurrentInstant()));

            var organisationAggregate = await this.organisationService
                .CreateDefaultAsync(tenant.Id, tenant.Details.Alias, tenant.Details.Name);

            // Act
            tenant.SetDefaultOrganisation(
                organisationAggregate.Id, this.clock.GetCurrentInstant().Plus(Duration.FromSeconds(60)));

            // Assert
            tenant.Details.DefaultOrganisationId.Should().Be(organisationAggregate.Id);
        }

        [Fact]
        public async Task UpdateOrganisation_ShouldUpdateProperty()
        {
            // Arrange
            var tenant = TenantFactory.Create(Guid.NewGuid());
            this.tenantRepository
                .Setup(t => t.GetTenantById(It.IsAny<Guid>()))
                .Returns(tenant);

            var userAuthData = new UserAuthenticationData(
                tenant.Id, tenant.Details.DefaultOrganisationId, UserType.Client, Guid.NewGuid(), default);
            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null, this.performingUserId,
                this.clock.GetCurrentInstant());
            var organisationAggregateUpdate = Organisation.CreateNewOrganisation(
                tenant.Id, "new-alias", "new-name", null, this.performingUserId, this.clock.GetCurrentInstant());
            this.cachingResolver.Setup(x => x.GetTenantOrThrow(tenant.Id))
                .ReturnsAsync(tenant);
            this.organisationReadModelRepository
                .Setup(o => o.IsAliasInUse(tenant.Id, organisation.Alias, organisation.Id))
                .Returns(false);
            this.organisationReadModelRepository
                .Setup(o => o.IsNameInUse(tenant.Id, organisation.Name, organisation.Id))
                .Returns(false);
            this.organisationAggregateRepository
                .Setup(o => o.GetById(tenant.Id, organisation.Id))
                .Returns(organisation);
            this.organisationReadModelRepository
                .Setup(o => o.Get(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new OrganisationReadModel(
                    tenant.Id,
                    organisationAggregateUpdate.Id,
                    organisationAggregateUpdate.Alias,
                    organisationAggregateUpdate.Name,
                    null, organisationAggregateUpdate.IsActive,
                    organisationAggregateUpdate.IsDeleted,
                    this.clock.GetCurrentInstant()));

            // Act
            var updatedOrganisationAggregate = await this.organisationService
                .UpdateOrganisation(
                    userAuthData.TenantId,
                    organisation.Id,
                    organisationAggregateUpdate.Name,
                    organisationAggregateUpdate.Alias,
                    null);

            // Assert
            updatedOrganisationAggregate.Should().NotBeNull();
            updatedOrganisationAggregate.Alias.Should().Be(organisationAggregateUpdate.Alias);
            updatedOrganisationAggregate.Name.Should().Be(organisationAggregateUpdate.Name);
            updatedOrganisationAggregate.IsActive.Should().Be(organisationAggregateUpdate.IsActive);
            updatedOrganisationAggregate.IsDeleted.Should().Be(organisationAggregateUpdate.IsDeleted);

            tenant.Details.DefaultOrganisationId.Should().NotBe(organisation.Id);
        }

        [Fact]
        public async Task MarkAsDeleted_ShouldUpdateProperty_WhenOrganisationIsNonDefault()
        {
            // Arrange
            var tenant = TenantFactory.Create(Guid.NewGuid());
            this.cachingResolver
                .Setup(t => t.GetTenantOrThrow(It.IsAny<Guid>()))
                .ReturnsAsync(tenant);

            var userAuthData = new UserAuthenticationData(
                tenant.Id, tenant.Details.DefaultOrganisationId, UserType.Client, Guid.NewGuid(), default);

            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null, this.performingUserId,
                this.clock.GetCurrentInstant());
            this.organisationAggregateRepository
                .Setup(o => o.GetById(tenant.Id, organisation.Id))
                .Returns(organisation);
            this.organisationReadModelRepository
                .Setup(o => o.Get(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new OrganisationReadModel(
                    tenant.Id,
                    organisation.Id,
                    organisation.Alias,
                    organisation.Name,
                    null, organisation.IsActive,
                    true,
                    this.clock.GetCurrentInstant()));

            // Act
            var updatedOrganisationAggregate = await this.organisationService
                .MarkAsDeleted(userAuthData.TenantId, organisation.Id);

            // Assert
            updatedOrganisationAggregate.Should().NotBeNull();
            updatedOrganisationAggregate.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public async Task Activate_ShouldUpdateProperty_WhenOrganisationIsDisabled()
        {
            // Arrange
            var tenant = TenantFactory.Create(Guid.NewGuid());
            this.tenantRepository
                .Setup(t => t.GetTenantById(It.IsAny<Guid>()))
                .Returns(tenant);

            var userAuthData = new UserAuthenticationData(
                tenant.Id, tenant.Details.DefaultOrganisationId, UserType.Client, Guid.NewGuid(), default);
            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null, this.performingUserId,
                this.clock.GetCurrentInstant());
            this.organisationAggregateRepository
                .Setup(o => o.GetById(tenant.Id, organisation.Id))
                .Returns(organisation);

            this.organisationReadModelRepository
                .Setup(o => o.Get(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new OrganisationReadModel(
                    tenant.Id,
                    organisation.Id,
                    organisation.Alias,
                    organisation.Name,
                    null, false,
                    organisation.IsDeleted,
                    this.clock.GetCurrentInstant()));

            // Act
            var updatedOrganisationAggregate = await this.organisationService
                .Disable(userAuthData.TenantId, organisation.Id);

            // Assert
            updatedOrganisationAggregate.Should().NotBeNull();
            updatedOrganisationAggregate.IsActive.Should().BeFalse();

            // Act
            this.organisationReadModelRepository
                .Setup(o => o.Get(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new OrganisationReadModel(
                    tenant.Id,
                    organisation.Id,
                    organisation.Alias,
                    organisation.Name,
                    null, true,
                    organisation.IsDeleted,
                    this.clock.GetCurrentInstant()));
            updatedOrganisationAggregate = await this.organisationService
                .Activate(userAuthData.TenantId, organisation.Id);

            // Assert
            updatedOrganisationAggregate.Should().NotBeNull();
            updatedOrganisationAggregate.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task Disable_ShouldUpdateProperty_WhenOrganisationIsNonDefault()
        {
            // Arrange
            var tenant = TenantFactory.Create(Guid.NewGuid());
            this.tenantRepository
                .Setup(t => t.GetTenantById(It.IsAny<Guid>()))
                .Returns(tenant);

            var userAuthData = new UserAuthenticationData(
                tenant.Id, tenant.Details.DefaultOrganisationId, UserType.Client, Guid.NewGuid(), default);

            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null, this.performingUserId,
                this.clock.GetCurrentInstant());
            this.organisationAggregateRepository
                .Setup(o => o.GetById(tenant.Id, organisation.Id))
                .Returns(organisation);

            this.organisationReadModelRepository
                .Setup(o => o.Get(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new OrganisationReadModel(
                    tenant.Id,
                    organisation.Id,
                    organisation.Alias,
                    organisation.Name,
                    null, false,
                    false,
                    this.clock.GetCurrentInstant()));

            // Act
            var updatedOrganisationAggregate = await this.organisationService.Disable(userAuthData.TenantId, organisation.Id);

            // Assert
            updatedOrganisationAggregate.Should().NotBeNull();
            updatedOrganisationAggregate.IsActive.Should().BeFalse();
        }

        [Fact]
        public async Task GetOrganisationSummaryForTenantIdAndOrganisationId_ThrowsTenantNotFoundException_WhenTenantIdDoesNotExists()
        {
            // Arrange
            var tenant = TenantFactory.Create(Guid.NewGuid());
            this.cachingResolver.Setup(c => c.GetTenantOrThrow(It.IsAny<Guid>()))
                .Throws(new ErrorException(Errors.Tenant.NotFound(tenant.Id)));
            Guid organisationId = Guid.NewGuid();
            var organisationReadModel = new OrganisationReadModel(
               tenant.Id, Guid.NewGuid(), "org-test", "Org Test", null, true, false, this.clock.GetCurrentInstant());
            this.cachingResolver.Setup(c => c.GetOrganisationOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Throws(new ErrorException(Errors.Organisation.NotFound(organisationId)));

            // Act
            Func<Task<IOrganisationReadModelSummary>> action = async () => await this.organisationService
                .GetOrganisationSummaryForTenantIdAndOrganisationId(tenant.Id, organisationId);

            // Assert
            (await action.Should().ThrowAsync<ErrorException>())
                .And.Error.Code.Should().Be("tenant.with.id.not.found");
            (await action.Should().ThrowAsync<ErrorException>())
                .And.Error.Title.Should().Be($"We couldn't find tenant '{tenant.Id}'");
        }

        [Fact]
        public async Task GetOrganisationSummaryForTenantIdAndOrganisationId_ThrowsOrganisationNotFoundException_WhenOrganisationIdDoesNotExists()
        {
            // Arrange
            var tenant = TenantFactory.Create(Guid.NewGuid());
            this.tenantRepository
                .Setup(t => t.GetTenantById(It.IsAny<Guid>()))
                .Returns(tenant);
            this.cachingResolver.Setup(c => c.GetTenantOrThrow(It.IsAny<Guid>()))
                .ReturnsAsync(tenant);

            Guid organisationId = Guid.NewGuid();
            this.cachingResolver.Setup(c => c.GetOrganisationOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Throws(new ErrorException(Errors.Organisation.NotFound(organisationId)));

            // Act
            Func<Task<IOrganisationReadModelSummary>> action = async () => await this.organisationService
                .GetOrganisationSummaryForTenantIdAndOrganisationId(tenant.Id, organisationId);

            // Assert
            (await action.Should().ThrowAsync<ErrorException>())
                .And.Error.Code.Should().Be("organisation.with.id.not.found");
            (await action.Should().ThrowAsync<ErrorException>())
                .And.Error.Title.Should().Be($"We couldn't find organisation '{organisationId}'");
        }

        [Fact]
        public async Task GetOrganisationSummaryForTenantIdAndOrganisationId_ShouldReturnSummary_WhenTenantIdAndOrganisationIdDoesExists()
        {
            // Arrange
            var tenant = TenantFactory.Create(Guid.NewGuid());
            this.tenantRepository
                .Setup(t => t.GetTenantById(It.IsAny<Guid>()))
                .Returns(tenant);
            this.cachingResolver.Setup(c => c.GetTenantOrThrow(It.IsAny<Guid>()))
                .ReturnsAsync(tenant);

            Guid organisationId = Guid.NewGuid();
            this.organisationReadModelRepository
                .Setup(o => o.Get(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new OrganisationReadModel(
                    tenant.Id,
                    organisationId,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null, true,
                    false,
                    this.clock.GetCurrentInstant()));

            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null, this.performingUserId,
                this.clock.GetCurrentInstant());
            this.organisationAggregateRepository
                .Setup(o => o.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(organisation);
            var organisationReadModel = new OrganisationReadModel(
                tenant.Id, organisationId, organisation.Alias, organisation.Name, null, true, false, this.clock.GetCurrentInstant());
            this.cachingResolver.Setup(c => c.GetOrganisationOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(organisationReadModel);

            // Act
            var result = await this.organisationService
                .GetOrganisationSummaryForTenantIdAndOrganisationId(tenant.Id, organisationId);

            // Assert
            result.TenantId.Should().Be(tenant.Id);
            result.Id.Should().Be(organisationId);
            result.Alias.Should().Be(tenant.Details.Alias);
            result.Name.Should().Be(tenant.Details.Name);
            result.IsActive.Should().BeTrue();
            result.IsDeleted.Should().BeFalse();
        }

        [Fact]
        public void GetOrganisationSummaryForTenantAliasAndOrganisationAlias_ThrowsTenantNotFoundException_WhenTenantIdDoesNotExists()
        {
            // Arrange
            var tenant = TenantFactory.Create(Guid.NewGuid());
            var organisationAlias = "organisation-alias";

            // Act
            Func<IOrganisationReadModelSummary> action = () => this.organisationService
                .GetOrganisationSummaryForTenantAliasAndOrganisationAlias(tenant.Details.Alias, organisationAlias);

            // Assert
            action.Should().Throw<ErrorException>()
                .And.Error.Code.Should().Be("tenant.with.id.not.found");
            action.Should().Throw<ErrorException>()
                .And.Error.Title.Should().Be($"We couldn't find tenant '{tenant.Id}'");
        }

        [Fact]
        public void GetOrganisationSummaryForTenantAliasAndOrganisationAlias_ThrowsOrganisationNotFoundException_WhenOrganisationIdDoesNotExists()
        {
            // Arrange
            var tenant = TenantFactory.Create(Guid.NewGuid());
            this.tenantRepository
                .Setup(t => t.GetTenantByAlias(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(tenant);

            var organisationAlias = "organisation-alias";

            // Act
            Func<IOrganisationReadModelSummary> action = () => this.organisationService
                .GetOrganisationSummaryForTenantAliasAndOrganisationAlias(tenant.Details.Alias, organisationAlias);

            // Assert
            action.Should().Throw<ErrorException>()
                .And.Error.Code.Should().Be("organisation.with.alias.not.found");
            action.Should().Throw<ErrorException>()
                .And.Error.Title.Should().Be($"Organisation '{organisationAlias}' not found");
        }

        [Fact]
        public void GetOrganisationSummaryForTenantAliasAndOrganisationAlias_ShouldReturnSummary_WhenTenantAliasAndOrganisationAliasDoesExists()
        {
            // Arrange
            var tenant = TenantFactory.Create(Guid.NewGuid());
            this.tenantRepository
                .Setup(t => t.GetTenantByAlias(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(tenant);

            Guid organisationId = Guid.NewGuid();
            this.organisationReadModelRepository
                .Setup(o => o.GetByAlias(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(new OrganisationReadModel(
                    tenant.Id,
                    organisationId,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null, true,
                    false,
                    this.clock.GetCurrentInstant()));

            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null, this.performingUserId,
                this.clock.GetCurrentInstant());
            this.organisationAggregateRepository
                .Setup(o => o.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(organisation);

            // Act
            var result = this.organisationService
                .GetOrganisationSummaryForTenantAliasAndOrganisationAlias(tenant.Details.Alias, organisation.Alias);

            // Assert
            result.TenantId.Should().Be(tenant.Id);
            result.Id.Should().Be(organisationId);
            result.Alias.Should().Be(tenant.Details.Alias);
            result.Name.Should().Be(tenant.Details.Name);
            result.IsActive.Should().BeTrue();
            result.IsDeleted.Should().BeFalse();
        }

        [Fact]
        public async Task ThrowIfHasDuplicateLinkedIdentity_ShouldThrow_WhenUniqueIdIsAlreadyUsed()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var authenticationMethodId = Guid.NewGuid();
            var externalUniqueId = "000929";
            var organisation = new OrganisationReadModel(tenantId, Guid.NewGuid(), "alias", "name", null, true, false, this.clock.Now());
            this.organisationReadModelRepository.Setup(x => x.GetLinkedOrganisation(tenantId, authenticationMethodId, externalUniqueId, null))
                .Returns(organisation);
            this.tenantRepository.Setup(x => x.GetTenantAliasById(tenantId))
                .ReturnsAsync("alias");

            // Act
            Func<Task> act = async () => await this.organisationService.ThrowIfHasDuplicateLinkedIdentity(tenantId, authenticationMethodId, externalUniqueId, null);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Code.Should().Be(Errors.Organisation.LinkedIdentityAlreadyExists(externalUniqueId).Code);
        }
    }
}
