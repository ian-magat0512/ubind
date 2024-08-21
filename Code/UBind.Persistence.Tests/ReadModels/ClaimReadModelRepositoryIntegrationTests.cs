// <copyright file="ClaimReadModelRepositoryIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.ReadModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Humanizer;
    using NodaTime;
    using UBind.Application.Infrastructure;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class ClaimReadModelRepositoryIntegrationTests
    {
        private readonly Guid? performingUserId = Guid.NewGuid();

        [Fact]
        public async Task ListClaims_FiltersActiveClaimsCorrectly()
        {
            // Arrange
            var tenant = TenantFactory.Create(Guid.NewGuid());
            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null, this.performingUserId,
                SystemClock.Instance.GetCurrentInstant());

            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack1.CreateTenant(tenant);

                tenant.SetDefaultOrganisation(
                    organisation.Id, SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromMinutes(1)));
                await stack1.OrganisationAggregateRepository.Save(organisation);
                stack1.TenantRepository.SaveChanges();

                stack1.ClaimNumberRepository.Seed(
                    tenant.Id,
                    ProductFactory.DefaultId,
                    QuoteFactory.DefaultEnvironment);
                QuoteFactory.QuoteExpirySettingsProvider = stack1.QuoteExpirySettingsProvider;
                QuoteAggregate quote = QuoteFactory.CreateNewPolicy(tenant.Id, organisationId: organisation.Id);

                var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Role, UserType.Customer.Humanize()),
                    new Claim(ClaimNames.TenantId, tenant.Id.ToString()),
                    new Claim(ClaimNames.OrganisationId, organisation.Id.ToString()),
                    new Claim(ClaimNames.CustomerId, quote.CustomerId.ToString()),
                }));
                await stack1.QuoteAggregateRepository.Save(quote);

                var claim1 = ClaimAggregate.CreateForPolicy(
                    "AAAAAA",
                    quote,
                    Guid.NewGuid(),
                    "John Smith",
                    "Johnny",
                    this.performingUserId,
                    SystemClock.Instance.GetCurrentInstant());
                var claim2 = ClaimAggregate.CreateForPolicy(
                    "BBBBBB",
                    quote,
                    Guid.NewGuid(),
                    "John Smith",
                    "Johnny",
                    this.performingUserId,
                    SystemClock.Instance.GetCurrentInstant());
                var releaseContext = new ReleaseContext(claim1.ProductContext, Guid.NewGuid());
                var workflow = await stack1.ClaimWorkflowProvider.GetConfigurableClaimWorkflow(releaseContext);
                claim1.ChangeClaimState(ClaimActions.Settle, Guid.NewGuid(), SystemClock.Instance.GetCurrentInstant(), workflow);
                claim2.ChangeClaimState(ClaimActions.Actualise, Guid.NewGuid(), SystemClock.Instance.GetCurrentInstant(), workflow);
                await stack1.ClaimAggregateRepository.Save(claim1);
                await stack1.ClaimAggregateRepository.Save(claim2);
            }

            using (var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                //// Act
                IEnumerable<IClaimReadModel> claims = stack2.ClaimReadModelRepository.ListClaimsWithoutJoiningProducts(
                    tenant.Id,
                    new EntityListFilters
                    {
                        Statuses = new string[] { "Incomplete" },
                        IncludeTestData = true,
                        SortBy = nameof(ClaimReadModel.CreatedTicksSinceEpoch),
                        SortOrder = Domain.Enums.SortDirection.Descending,
                    });

                var claimCount = claims.Count();

                // Assert
                Assert.Equal(1, claimCount);
            }
        }

        [Fact]
        public async Task ListClaimsWithoutJoiningProducts_ShouldListClaimsBySearchingByCustomerName_WhenTheSearchValueHasMatchToTheRecords()
        {
            // Arrange
            Tenant tenant = await this.CreateTenantPolicyForClaims();

            using (var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                //// Act
                var claimsFilter = new EntityListFilters
                {
                    SearchTerms = new string[] { "Elvien" },
                    Statuses = new string[] { "Incomplete" },
                    IncludeTestData = true,
                };

                IEnumerable<IClaimReadModel> claims = stack2.ClaimReadModelRepository.ListClaimsWithoutJoiningProducts(
                    tenant.Id,
                    claimsFilter);

                var claimCount = claims.Count();

                // Assert
                claimCount.Should().Be(1);

                //// Act
                claimsFilter = new EntityListFilters
                {
                    SearchTerms = new string[] { "John", "Smith" },
                    Statuses = new string[] { "Incomplete" },
                    IncludeTestData = true,
                };

                claims = stack2.ClaimReadModelRepository.ListClaimsWithoutJoiningProducts(
                    tenant.Id,
                    claimsFilter);

                claimCount = claims.Count();

                // Assert
                claimCount.Should().Be(2);
            }
        }

        [Fact]
        public void GetClaimsByPolicyNumberInPastFiveYears_ReturnsEmptyCollection_WhenNoClaimsExist()
        {
            // Arrange
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // Act
                var claims = stack.ClaimReadModelRepository.GetClaimsByPolicyNumberInPastFiveYears(Guid.NewGuid(), Guid.NewGuid(), "policyNumber");

                // Assert
                claims.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task GetClaimsByPolicyNumberInPastFiveYears_OnlyReturnsClaimsFromCorrectPeriod()
        {
            // Arrange
            Guid tenantId = Guid.NewGuid();
            var tenant = TenantFactory.Create(tenantId);
            Guid productId = Guid.NewGuid();

            var quoteAggregateWithPolicy = QuoteFactory.CreateNewPolicy(
                tenantId,
                productId);

            var testTime = new LocalDateTime(2021, 5, 2, 12, 0);
            var tooEarlyTimestamp = new LocalDateTime(2016, 5, 1, 12, 0).InZoneLeniently(Timezones.AET).ToInstant();
            var okTimestamp = new LocalDateTime(2019, 3, 4, 12, 0).InZoneLeniently(Timezones.AET).ToInstant();

            var tooEarlyClaim = ClaimFactory
                .CreateClaim(quoteAggregateWithPolicy)
                .WithFormDatafromClaimData(new ClaimData(100m, "Excluded", tooEarlyTimestamp, Timezones.AET));

            var okClaim = ClaimFactory
                .CreateClaim(quoteAggregateWithPolicy)
                .WithFormDatafromClaimData(new ClaimData(100m, "Included", okTimestamp, Timezones.AET));

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack.CreateTenant(tenant);

                await stack.QuoteAggregateRepository.Save(quoteAggregateWithPolicy);
                await stack.ClaimAggregateRepository.Save(tooEarlyClaim);
                await stack.ClaimAggregateRepository.Save(okClaim);
            }

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack.Clock.SetAetTime(testTime);

                // Act
                var claims = stack.ClaimReadModelRepository.GetClaimsByPolicyNumberInPastFiveYears(tenantId, productId, quoteAggregateWithPolicy.Policy.PolicyNumber);

                // Assert
                claims.Should().HaveCount(1);
                claims.Single().Description.Should().Be("Included");
            }
        }

        [Fact]
        public async Task GetTotalClaimsAmountByPolicyNumberInPastFiveYears_ReturnsCorrectSum_IgnoringNullAmountsAndClaimsOlderThanFiveYears()
        {
            // Arrange
            Guid tenantId = Guid.NewGuid();
            var tenant = TenantFactory.Create(tenantId);
            Guid productId = Guid.NewGuid();
            var quoteAggregateWithPolicy = QuoteFactory.CreateNewPolicy(
                tenantId,
                productId);

            var testTime = new LocalDateTime(2021, 5, 2, 12, 0);
            var tooEarlyTimestamp = new LocalDateTime(2016, 5, 1, 12, 0).InZoneLeniently(Timezones.AET).ToInstant();
            var justIncludedTimestamp = new LocalDateTime(2016, 5, 2, 12, 0).InZoneLeniently(Timezones.AET).ToInstant();
            var safelyIncludedTimestamp = new LocalDateTime(2019, 3, 4, 12, 0).InZoneLeniently(Timezones.AET).ToInstant();

            var tooEarlyClaimWithoutAmount = ClaimFactory
                .CreateClaim(quoteAggregateWithPolicy)
                .WithFormDatafromClaimData(new ClaimData(null, "Excluded", tooEarlyTimestamp, Timezones.AET));

            var tooEarlyClaimWithAmount = ClaimFactory
                .CreateClaim(quoteAggregateWithPolicy)
                .WithFormDatafromClaimData(new ClaimData(1m, "Excluded", tooEarlyTimestamp, Timezones.AET));

            var justIncludedClaimWithAmount10 = ClaimFactory
                .CreateClaim(quoteAggregateWithPolicy)
                .WithFormDatafromClaimData(new ClaimData(10m, "Included", justIncludedTimestamp, Timezones.AET));

            var safelyIncludedClaimWithAmount100 = ClaimFactory
                .CreateClaim(quoteAggregateWithPolicy)
                .WithFormDatafromClaimData(new ClaimData(100m, "Included", safelyIncludedTimestamp, Timezones.AET));

            var safelyIncludedClaimWithoutAmount = ClaimFactory
                .CreateClaim(quoteAggregateWithPolicy)
                .WithFormDatafromClaimData(new ClaimData(null, "Included", safelyIncludedTimestamp, Timezones.AET));

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack.CreateTenant(tenant);

                await stack.QuoteAggregateRepository.Save(quoteAggregateWithPolicy);
                await stack.ClaimAggregateRepository.Save(tooEarlyClaimWithoutAmount);
                await stack.ClaimAggregateRepository.Save(tooEarlyClaimWithAmount);
                await stack.ClaimAggregateRepository.Save(justIncludedClaimWithAmount10);
                await stack.ClaimAggregateRepository.Save(safelyIncludedClaimWithAmount100);
                await stack.ClaimAggregateRepository.Save(safelyIncludedClaimWithoutAmount);
            }

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack.Clock.SetAetTime(testTime);

                // Act
                var claimsTotal = stack.ClaimReadModelRepository.GetTotalClaimsAmountByPolicyNumberInPastFiveYears(tenantId, productId, quoteAggregateWithPolicy.Policy.PolicyNumber);

                // Assert
                claimsTotal.Should().Be(110m);
            }
        }

        private async Task<Tenant> CreateTenantPolicyForClaims()
        {
            var tenant = TenantFactory.Create(Guid.NewGuid());
            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null, this.performingUserId,
                SystemClock.Instance.GetCurrentInstant());

            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack1.CreateTenant(tenant);

                tenant.SetDefaultOrganisation(
                    organisation.Id, SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromMinutes(1)));
                await stack1.OrganisationAggregateRepository.Save(organisation);
                stack1.TenantRepository.SaveChanges();

                stack1.ClaimNumberRepository.Seed(
                    tenant.Id,
                    ProductFactory.DefaultId,
                    QuoteFactory.DefaultEnvironment);
                QuoteFactory.QuoteExpirySettingsProvider = stack1.QuoteExpirySettingsProvider;
                QuoteAggregate quote = QuoteFactory.CreateNewPolicy(tenant.Id, organisationId: organisation.Id);

                var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Role, UserType.Customer.Humanize()),
                    new Claim(ClaimNames.TenantId, tenant.Id.ToString()),
                    new Claim(ClaimNames.OrganisationId, organisation.Id.ToString()),
                    new Claim(ClaimNames.CustomerId, quote.CustomerId.ToString()),
                }));
                await stack1.QuoteAggregateRepository.Save(quote);

                var claim1 = ClaimAggregate.CreateForPolicy(
                    "AAAAAA",
                    quote,
                    Guid.NewGuid(),
                    "John Smith",
                    "Johnny",
                    this.performingUserId,
                    SystemClock.Instance.GetCurrentInstant());
                var claim2 = ClaimAggregate.CreateForPolicy(
                    "BBBBBB",
                    quote,
                    Guid.NewGuid(),
                    "John James",
                    "James",
                    this.performingUserId,
                    SystemClock.Instance.GetCurrentInstant());

                var claim3 = ClaimAggregate.CreateForPolicy(
                    "CCCCCC",
                    quote,
                    Guid.NewGuid(),
                    "Elvien Bustamante",
                    "Ken",
                    this.performingUserId,
                    SystemClock.Instance.GetCurrentInstant());
                var releaseContext = new ReleaseContext(claim1.ProductContext, Guid.NewGuid());
                var workflow = await stack1.ClaimWorkflowProvider.GetConfigurableClaimWorkflow(releaseContext);
                claim1.ChangeClaimState(ClaimActions.Actualise, Guid.NewGuid(), SystemClock.Instance.GetCurrentInstant(), workflow);
                claim2.ChangeClaimState(ClaimActions.Actualise, Guid.NewGuid(), SystemClock.Instance.GetCurrentInstant(), workflow);
                claim3.ChangeClaimState(ClaimActions.Actualise, Guid.NewGuid(), SystemClock.Instance.GetCurrentInstant(), workflow);
                await stack1.ClaimAggregateRepository.Save(claim1);
                await stack1.ClaimAggregateRepository.Save(claim2);
                await stack1.ClaimAggregateRepository.Save(claim3);
            }

            return tenant;
        }
    }
}
