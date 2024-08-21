// <copyright file="OrganisationControllerTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Tests.Controllers.Portal
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Humanizer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using NodaTime.Text;
    using UBind.Application.Authorisation;
    using UBind.Application.Commands.Organisation;
    using UBind.Application.Infrastructure;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using UBind.Domain.Tests.Fakes;
    using UBind.Web.Controllers;
    using UBind.Web.ResourceModels.Organisation;
    using Xunit;

    public class OrganisationControllerTests
    {
        private readonly Guid tenantId = Guid.NewGuid();
        private readonly NodaTime.IClock clock = NodaTime.SystemClock.Instance;
        private readonly Mock<IAuthorisationService> authorisationService;
        private readonly Mock<IOrganisationAggregateRepository> organisationAggregateRepo;
        private readonly Mock<IOrganisationReadModelRepository> organisationReadModelRepository;
        private readonly Mock<ITenantRepository> tenantRepository;
        private readonly Mock<IHttpContextPropertiesResolver> httpContextPropertiesResolver;
        private readonly Mock<ICqrsMediator> mediator;
        private readonly IOrganisationService organisationService;
        private readonly OrganisationController sut;
        private readonly Mock<ICachingResolver> cachingResolver;
        private readonly Mock<IAdditionalPropertyValueService> additionalPropertyValueServiceMock;
        private readonly Mock<IOrganisationAuthorisationService> organisationAuthorisationService;

        public OrganisationControllerTests()
        {
            this.cachingResolver = new Mock<ICachingResolver>();
            this.authorisationService = new Mock<IAuthorisationService>();
            this.organisationAggregateRepo = new Mock<IOrganisationAggregateRepository>();
            this.organisationReadModelRepository = new Mock<IOrganisationReadModelRepository>();
            this.tenantRepository = new Mock<ITenantRepository>();
            this.mediator = new Mock<ICqrsMediator>();
            this.httpContextPropertiesResolver = new Mock<IHttpContextPropertiesResolver>();
            this.additionalPropertyValueServiceMock = new Mock<IAdditionalPropertyValueService>();
            this.organisationAuthorisationService = new Mock<IOrganisationAuthorisationService>();

            this.organisationService = new OrganisationService(
                this.organisationAggregateRepo.Object,
                this.organisationReadModelRepository.Object,
                this.tenantRepository.Object,
                this.additionalPropertyValueServiceMock.Object,
                this.httpContextPropertiesResolver.Object,
                this.mediator.Object,
                this.cachingResolver.Object,
                this.clock,
                Mock.Of<IProductRepository>(),
                Mock.Of<IProductFeatureSettingRepository>(),
                Mock.Of<IProductOrganisationSettingRepository>());
            this.sut = new OrganisationController(
                this.authorisationService.Object,
                this.organisationService,
                this.mediator.Object,
                this.cachingResolver.Object,
                this.additionalPropertyValueServiceMock.Object,
                this.organisationAuthorisationService.Object);
            this.sut.ControllerContext = this.ControllerHttpContext;
        }

        private ControllerContext ControllerHttpContext
        {
            get
            {
                var context = new ControllerContext();
                var defaultContext = new DefaultHttpContext();
                var mockedUser = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()
                {
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new Claim(ClaimNames.TenantId, this.tenantId.ToString()),
                    new Claim(ClaimNames.OrganisationId, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
                    new Claim(ClaimNames.CustomerId, Guid.NewGuid().ToString()),
                }));

                defaultContext.User = mockedUser;

                context.HttpContext = defaultContext;

                return context;
            }
        }

        [Fact]
        public void GetOrganisationSummary_ShouldContain_AllowAnonymousAttribute()
        {
            // Arrange
            var type = this.sut.GetType();

            // Act
#pragma warning disable SA1118 // Parameter should not span multiple lines
            var methodInfo = type.GetMethod(
                "GetOrganisationSummary",
                new Type[]
                {
                    typeof(string),
                    typeof(string),
                    typeof(string),
                    typeof(Guid),
                });
#pragma warning restore SA1118 // Parameter should not span multiple lines
            var attributes = methodInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), true);

            // Assert
            attributes.Should().NotBeEmpty();
        }

        [Fact]
        public async Task ActivateOrganisation_ReturnedModel_Should_Include_LastModifiedDateTime_ThatIs_GreaterThan_OrganisationCreatedTimestampAsync()
        {
            // Arrange
            var orgReadModel = this.CreateMockOrganisationReadModelWithOptionalDifferentLastModifiedDateTime(null);
            var orgCreatedTimestamp = orgReadModel.CreatedTimestamp;
            var tenant = this.CreateMockTenant();
            var orgAggr = Organisation.CreateNewOrganisation(
                orgReadModel.TenantId,
                orgReadModel.Alias,
                orgReadModel.Name,
                null, Guid.NewGuid(),
                orgCreatedTimestamp);

            this.cachingResolver.Setup(_ => _.GetOrganisationOrThrow(It.IsAny<Guid>(), It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(orgReadModel));
            this.cachingResolver.Setup(_ => _.GetOrganisationOrThrow(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(orgReadModel));
            this.cachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            orgAggr.Disable(It.IsAny<Guid>(), new TestClock().Timestamp);

            this.organisationAggregateRepo.Setup(_ => _.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Callback((Guid tenantId, Guid id) => Thread.Sleep(2000))
                .Returns(orgAggr);

            this.tenantRepository.Setup(_ => _.GetTenantById(It.IsAny<Guid>())).Returns(tenant);

            this.organisationAggregateRepo
                .Setup(_ => _.Save(It.IsAny<Organisation>()))
                .Callback((Action<Organisation>)((Organisation organisation) =>
                {
                    orgReadModel = new OrganisationReadModel(
                        organisation.TenantId,
                        organisation.Id,
                        organisation.Alias,
                        organisation.Name,
                        null, organisation.IsActive,
                        organisation.IsDeleted,
                        new TestClock().Timestamp);
                }));

            this.organisationReadModelRepository.Setup(_ => _.Get(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(() =>
            {
                return orgReadModel;
            });

            // Act
            var actionResult = await this.sut.Enable(It.IsAny<string>(), It.IsAny<string>()) as OkObjectResult;

            // Assert
            var objectContent = actionResult.Value as OrganisationModel;

            objectContent.LastModifiedDateTime.Should().NotBeNullOrEmpty();
            objectContent.IsActive.Should().BeTrue();
            InstantPattern.ExtendedIso.Parse(objectContent.LastModifiedDateTime).Value
                .Should().BeGreaterThan(orgCreatedTimestamp);
        }

        [Fact]
        public async Task DisableOrganisation_ReturnedModel_Should_Include_LastModifiedDateTime_ThatIs_GreaterThan_OrganisationCreatedTimestampAsync()
        {
            // Arrange
            var orgReadModel = this.CreateMockOrganisationReadModelWithOptionalDifferentLastModifiedDateTime(null);
            var orgCreatedTimestamp = orgReadModel.CreatedTimestamp;
            var tenant = this.CreateMockTenant();
            var orgAggr = Organisation.CreateNewOrganisation(
                orgReadModel.TenantId,
                orgReadModel.Alias,
                orgReadModel.Name,
                null, Guid.NewGuid(),
                orgReadModel.CreatedTimestamp);

            this.cachingResolver.Setup(_ => _.GetOrganisationOrThrow(It.IsAny<Guid>(), It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(orgReadModel));
            this.cachingResolver.Setup(_ => _.GetOrganisationOrThrow(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(orgReadModel));
            this.cachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            this.organisationAggregateRepo.Setup(_ => _.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Callback((Guid tenantId, Guid id) => Thread.Sleep(2000))
                .Returns(orgAggr);

            this.tenantRepository.Setup(_ => _.GetTenantById(It.IsAny<Guid>())).Returns(tenant);

            this.organisationAggregateRepo
                .Setup(_ => _.Save(It.IsAny<Organisation>()))
                .Callback((Action<Organisation>)((Organisation organisation) =>
                {
                    orgReadModel = new OrganisationReadModel(
                        organisation.TenantId,
                        organisation.Id,
                        organisation.Alias,
                        organisation.Name,
                        null, organisation.IsActive,
                        organisation.IsDeleted,
                        new TestClock().Timestamp);
                }));

            this.organisationReadModelRepository.Setup(_ => _.Get(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(() =>
            {
                return orgReadModel;
            });

            // Act
            var actionResult = await this.sut.Disable(It.IsAny<string>(), It.IsAny<string>()) as OkObjectResult;

            // Assert
            var objectContent = actionResult.Value as OrganisationModel;

            objectContent.LastModifiedDateTime.Should().NotBeNullOrEmpty();
            objectContent.IsActive.Should().BeFalse();
            InstantPattern.ExtendedIso.Parse(objectContent.LastModifiedDateTime).Value
                .Should().BeGreaterThan(orgCreatedTimestamp);
        }

        [Fact]
        public async Task DeleteOrganisation_ReturnedModel_Should_Include_LastModifiedDateTime_ThatIs_GreaterThan_OrganisationCreatedTimestampAsync()
        {
            // Arrange
            var tenant = this.CreateMockTenant();
            var orgReadModel = this.CreateMockOrganisationReadModelWithOptionalDifferentLastModifiedDateTime(null);
            var orgCreatedTimestamp = orgReadModel.CreatedTimestamp;
            var orgAggr = Organisation.CreateNewOrganisation(
                orgReadModel.TenantId,
                orgReadModel.Alias,
                orgReadModel.Name,
                null, Guid.NewGuid(),
                orgReadModel.CreatedTimestamp);
            orgAggr.LastModifiedTimestamp = new TestClock().Now();

            this.cachingResolver.Setup(_ => _.GetOrganisationOrThrow(It.IsAny<Guid>(), It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(orgReadModel));
            this.cachingResolver.Setup(_ => _.GetOrganisationOrThrow(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(orgReadModel));
            this.cachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            this.organisationAggregateRepo.Setup(_ => _.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Callback((Guid tenantId, Guid id) => Thread.Sleep(2000))
                .Returns(orgAggr);
            this.cachingResolver.Setup(c => c.GetTenantOrThrow(It.IsAny<Guid>())).ReturnsAsync(tenant);

            this.organisationAggregateRepo
                .Setup(_ => _.Save(It.IsAny<Organisation>()))
                .Callback((Action<Organisation>)((Organisation organisation) =>
                {
                    orgReadModel = new OrganisationReadModel(
                        organisation.TenantId,
                        organisation.Id,
                        organisation.Alias,
                        organisation.Name,
                        null, organisation.IsActive,
                        organisation.IsDeleted,
                        new TestClock().Timestamp);
                }));

            this.organisationReadModelRepository.Setup(_ => _.Get(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(() =>
            {
                return orgReadModel;
            });

            var organisationSummary = this.CreateMockDeletedOrganisationSummaryReadModel();
            this.mediator.Setup(s => s.Send(It.IsAny<DeleteOrganisationAndAssociatedUsersCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(organisationSummary));

            // Act
            var actionResult = await this.sut.Delete(It.IsAny<string>(), It.IsAny<string>()) as OkObjectResult;

            // Assert
            var objectContent = actionResult.Value as OrganisationModel;

            objectContent.LastModifiedDateTime.Should().NotBeNullOrEmpty();
            objectContent.IsDeleted.Should().BeTrue();
            InstantPattern.ExtendedIso.Parse(objectContent.LastModifiedDateTime).Value
                .Should().BeGreaterThan(orgCreatedTimestamp);
        }

        [Fact]
        [Obsolete("To be removed in UB-9510")]
        public async void GetOrganisationSummary_ShouldReturnCorrectSummary_ForGivenTenantAlias()
        {
            // Arrange
            var tenant = this.CreateMockTenant();
            var organisation = this.CreateMockOrganisationReadModelWithOptionalDifferentLastModifiedDateTime(null);

            this.tenantRepository.Setup(_ => _.GetTenantByAlias(It.IsAny<string>(), It.IsAny<bool>())).Returns(tenant);
            this.cachingResolver.Setup(_ => _.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            this.cachingResolver.Setup(s => s.GetOrganisationOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(organisation));
            this.cachingResolver.Setup(s => s.GetOrganisationOrThrow(It.IsAny<Guid>(), It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(organisation));

            // Act
            var actionResult = await this.sut.GetOrganisationSummary(It.IsAny<string>()) as OkObjectResult;
            var objectContent = actionResult.Value as OrganisationSummaryModel;

            // Assert
            objectContent.Should().NotBeNull();
            objectContent.TenantId.Should().Be(tenant.Id);
            objectContent.TenantName.Should().Be(tenant.Details.Name);
            objectContent.OrganisationId.Should().Be(organisation.Id.ToString());
            objectContent.OrganisationName.Should().Be(organisation.Name);
            objectContent.OrganisationAlias.Should().Be(organisation.Alias);
        }

        [Fact]
        [Obsolete("To be removed in UB-9510")]
        public async void GetOrganisationSummary_ShouldReturnCorrectSummary_ForGivenTenantAliasAndOrganisationAlias()
        {
            // Arrange
            var tenant = this.CreateMockTenant();
            var organisation = this.CreateMockOrganisationReadModelWithOptionalDifferentLastModifiedDateTime(
                tenant, null);

            this.cachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            this.tenantRepository.Setup(_ => _.GetTenantByAlias(It.IsAny<string>(), It.IsAny<bool>())).Returns(tenant);
            this.cachingResolver.Setup(s => s.GetOrganisationOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(organisation));
            this.cachingResolver.Setup(s => s.GetOrganisationOrThrow(It.IsAny<Guid>(), It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(organisation));

            // Act
            var actionResult = await this.sut.GetOrganisationSummary(It.IsAny<string>(), It.IsAny<string>()) as OkObjectResult;
            var objectContent = actionResult.Value as OrganisationSummaryModel;

            // Assert
            objectContent.Should().NotBeNull();
            objectContent.TenantId.Should().Be(tenant.Id);
            objectContent.TenantName.Should().Be(tenant.Details.Name);
            objectContent.OrganisationId.Should().Be(organisation.Id.ToString());
            objectContent.OrganisationName.Should().Be(organisation.Name);
            objectContent.OrganisationAlias.Should().Be(organisation.Alias);
        }

        private OrganisationReadModel CreateMockOrganisationReadModelWithOptionalDifferentLastModifiedDateTime(
            Tenant tenant, NodaTime.Instant? lastModifiedDateTime)
        {
            var createdTimestamp = new TestClock().Timestamp;
            return new OrganisationReadModel(
                tenant.Id, Guid.NewGuid(), tenant.Details.Alias, tenant.Details.Name, null, true, false, createdTimestamp);
        }

        private Tenant CreateMockTenant()
        {
            return new Tenant(this.tenantId, this.tenantId.ToString(), this.tenantId.ToString(), null, default, default, new TestClock().Timestamp);
        }

        private OrganisationReadModel CreateMockOrganisationReadModelWithOptionalDifferentLastModifiedDateTime(NodaTime.Instant? lastModifiedDateTime)
        {
            var createdTimestamp = lastModifiedDateTime ?? new TestClock().Timestamp;
            return new OrganisationReadModel(this.tenantId, this.tenantId, "tenant", "name", null, true, false, createdTimestamp);
        }

        private IOrganisationReadModelSummary CreateMockDeletedOrganisationSummaryReadModel()
        {
            var createdTimestamp = new TestClock().Timestamp;
            return new OrganisationReadModelSummary
            {
                TenantId = this.tenantId,
                Id = Guid.NewGuid(),
                Alias = "organisation",
                Name = "organisation",
                IsActive = true,
                IsDeleted = true,
                IsDefault = false,
                CreatedTimestamp = createdTimestamp,
                LastModifiedTimestamp = createdTimestamp,
            };
        }
    }
}
