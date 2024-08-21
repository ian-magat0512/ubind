// <copyright file="OrganisationServiceIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Persistence.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Authentication;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Services;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using UBind.Domain.Tests;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.ReadModels;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class OrganisationServiceIntegrationTests
    {
        private Func<ApplicationStack> stackFactory = () => new ApplicationStack(
            DatabaseFixture.TestConnectionStringName, ApplicationStackConfiguration.Default);

        private Mock<DbSet<OrganisationReadModel>> mockOrganisationReadModelSet = new Mock<DbSet<OrganisationReadModel>>();
        private Mock<IAdditionalPropertyValueService> additionalPropertyValueServiceMock =
            new Mock<IAdditionalPropertyValueService>();

        [Fact]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task GetListOrganisationsForUser_ThatCapsAt1000()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var tenant = TenantFactory.Create(tenantId);
            var userAuthData = new UserAuthenticationData(
                tenant.Id, tenant.Details.DefaultOrganisationId, UserType.Client, Guid.NewGuid(), default);

            List<OrganisationReadModel> organisationReadModels = new List<OrganisationReadModel>();
            using (var stack = this.stackFactory())
            {
                stack.CreateTenant(tenant);

                for (var i = 0; i < 1000; i++)
                {
                    organisationReadModels.Add(new OrganisationReadModel(
                        tenant.Id,
                        Guid.NewGuid(),
                        $"{tenant.Details.Alias}" + i,
                        tenant.Details.Name + i,
                        null,
                        true,
                        false,
                        stack.Clock.Now()));
                }
            }

            this.mockOrganisationReadModelSet.As<IQueryable<OrganisationReadModel>>().Setup(m => m.Provider).Returns(organisationReadModels.AsQueryable().Provider);
            this.mockOrganisationReadModelSet.As<IQueryable<OrganisationReadModel>>().Setup(m => m.Expression).Returns(organisationReadModels.AsQueryable().Expression);
            this.mockOrganisationReadModelSet.As<IQueryable<OrganisationReadModel>>().Setup(m => m.ElementType).Returns(organisationReadModels.AsQueryable().ElementType);
            this.mockOrganisationReadModelSet.As<IQueryable<OrganisationReadModel>>().Setup(m => m.GetEnumerator()).Returns(organisationReadModels.AsQueryable().GetEnumerator());

            var mockContext = new Mock<UBindDbContext>();
            mockContext.Setup(c => c.OrganisationReadModel).Returns(this.mockOrganisationReadModelSet.Object);

            using (var stack = this.stackFactory())
            {
                var filters = new OrganisationReadModelFilters();
                IOrganisationReadModelRepository organisationReadModelRepository =
                    new OrganisationReadModelRepository(mockContext.Object);
                var organisationService = new OrganisationService(
                    stack.OrganisationAggregateRepository,
                    organisationReadModelRepository,
                    stack.TenantRepository,
                    this.additionalPropertyValueServiceMock.Object,
                    stack.HttpContextPropertiesResolver,
                    stack.MediatorMock.Object,
                    stack.CachingResolver,
                    stack.Clock,
                    stack.ProductRepository,
                    stack.ProductFeatureSettingRepository,
                    stack.ProductOrganisationSettingRepository);

                // Act
                var organisationCount = (await organisationService
                   .ListOrganisationsForUser(userAuthData.TenantId, filters))
                   .Count();

                // Assert
                organisationCount.Should().Be(1000);
            }
        }

        [Fact]
        public async Task CreateActiveNonDefaultAsync_CreatesNewOrganisation_ForExistingTenant()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var tenant = TenantFactory.Create(tenantId);
            var userAuthData = new UserAuthenticationData(
                tenant.Id, tenant.Details.DefaultOrganisationId, UserType.Client, Guid.NewGuid(), default);

            IOrganisationReadModelSummary organisationSummary;
            using (var stack = this.stackFactory())
            {
                stack.TenantRepository.Insert(tenant);
                stack.TenantRepository.SaveChanges();

                // Action
                organisationSummary = await stack.OrganisationService.CreateActiveNonDefaultAsync(
                    tenant.Id, $"{tenant.Details.Alias}0", tenant.Details.Name, null);
            }

            using (var stack = this.stackFactory())
            {
                var organisation = await stack.OrganisationService
                    .GetOrganisationById(userAuthData.TenantId, organisationSummary.Id);

                // Assert
                organisation.Should().NotBeNull();
                organisation.Id.Should().Be(organisationSummary.Id);
                organisation.Alias.Should().Be(organisationSummary.Alias);
                organisation.Name.Should().Be(organisationSummary.Name);
                organisation.IsActive.Should().Be(organisationSummary.IsActive);
                organisation.IsDeleted.Should().Be(organisationSummary.IsDeleted);
            }
        }

        [Fact]
        public async Task ListOrganisationsForUser_FiltersOnStatusCorrectly_ForMultipleStatuses()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var tenant = TenantFactory.Create(tenantId);
            var organisationA = "os-name-a1";
            var organisationB = "os-name-b1";
            var userAuthData = new UserAuthenticationData(
                tenant.Id, tenant.Details.DefaultOrganisationId, UserType.Client, Guid.NewGuid(), default);

            using (var stack = this.stackFactory())
            {
                stack.TenantRepository.Insert(tenant);
                stack.TenantRepository.SaveChanges();

                await stack.OrganisationService.CreateActiveNonDefaultAsync(
                    tenant.Id, organisationA, organisationA, null);

                var organisationAggregateB = await stack.OrganisationService.CreateActiveNonDefaultAsync(
                    tenant.Id, organisationB, organisationB, null);
                await stack.OrganisationService.Disable(userAuthData.TenantId, organisationAggregateB.Id);

                stack.DbContext.SaveChanges();
            }

            var filters = (OrganisationReadModelFilters)new OrganisationReadModelFilters
            {
                Statuses = new string[] { "active", "disabled" },
            }
            .WithDateIsAfterFilter(new LocalDate(2001, 1, 1))
            .WithDateIsBeforeFilter(new LocalDate(2040, 1, 1));

            using (var stack = this.stackFactory())
            {
                // Act
                var organisations = await stack.OrganisationService.ListOrganisationsForUser(userAuthData.TenantId, (OrganisationReadModelFilters)filters);

                // Assert
                organisations.Should().Contain(o => o.Name == organisationA);
                organisations.Should().Contain(o => o.Name == organisationB);
                organisations.Should().HaveCountGreaterThan(0);
            }
        }

        [Fact]
        public async Task ListOrganisationsForUser_FiltersOnStatusCorrectly_ForDisabledOnly()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var tenant = TenantFactory.Create(tenantId);
            var organisationA = "os-name-a1";
            var organisationB = "os-name-b1";
            var userAuthData = new UserAuthenticationData(
                tenant.Id, tenant.Details.DefaultOrganisationId, UserType.Client, Guid.NewGuid(), default);

            using (var stack = this.stackFactory())
            {
                stack.TenantRepository.Insert(tenant);
                stack.TenantRepository.SaveChanges();

                await stack.OrganisationService.CreateActiveNonDefaultAsync(
                    tenant.Id, organisationA, organisationA, null);

                var organisationAggregateB = await stack.OrganisationService.CreateActiveNonDefaultAsync(
                    tenant.Id, organisationB, organisationB, null);
                await stack.OrganisationService.Disable(tenant.Id, organisationAggregateB.Id);
            }

            var filters = (OrganisationReadModelFilters)new OrganisationReadModelFilters
            {
                Statuses = new string[] { "disabled" },
            }
            .WithDateIsAfterFilter(new LocalDate(2001, 1, 1))
            .WithDateIsBeforeFilter(new LocalDate(2040, 1, 1));

            using (var stack = this.stackFactory())
            {
                // Act
                var organisations = (await stack.OrganisationService.ListOrganisationsForUser(userAuthData.TenantId, (OrganisationReadModelFilters)filters)).ToList();

                // Assert
                organisations.Should().NotContain(o => o.Name == organisationA);
                organisations.Should().Contain(o => o.Name == organisationB);
                organisations.Should().HaveCountGreaterThan(0);
            }
        }
    }
}
