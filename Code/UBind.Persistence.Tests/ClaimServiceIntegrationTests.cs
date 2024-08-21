// <copyright file="ClaimServiceIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Authentication;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class ClaimServiceIntegrationTests
    {
        private readonly ApplicationStack stackFactory;
        private readonly Guid? performingUserId = Guid.NewGuid();

        public ClaimServiceIntegrationTests()
        {
            this.stackFactory = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
        }

        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public async Task ClaimService_GetList_CapsAt10()
        {
            // Arrange
            var tenant = TenantFactory.Create();
            var product = ProductFactory.Create(tenant.Id, Guid.NewGuid());
            this.stackFactory.CreateTenant(tenant);
            this.stackFactory.CreateProduct(product);

            List<Task> tasks = new List<Task>();
            for (int i = 0; i < 10; ++i)
            {
                Domain.Aggregates.Quote.QuoteAggregate quote = QuoteFactory.CreateNewPolicy(
                    tenant.Id,
                    product.Id,
                    DeploymentEnvironment.Staging);

                var claimAggregate = ClaimAggregate.CreateForPolicy(
                    "REF123",
                    quote,
                    Guid.NewGuid(),
                    "John Smith",
                    "Jonboy",
                    this.performingUserId,
                    SystemClock.Instance.Now());
                var workflow = await this.stackFactory.ClaimWorkflowProvider.GetConfigurableClaimWorkflow(It.IsAny<ReleaseContext>());
                claimAggregate.ChangeClaimState(ClaimActions.Acknowledge, this.performingUserId, default, workflow);
                tasks.Add(this.stackFactory.ClaimAggregateRepository.Save(claimAggregate));
            }

            await Task.WhenAll(tasks);

            var filters = new EntityListFilters
            {
                TenantId = tenant.Id,
                OrganisationIds = new List<Guid> { tenant.Details.DefaultOrganisationId },
                Environment = DeploymentEnvironment.Staging,
                SearchTerms = new string[] { "bad", "thing", "REF" },
                Statuses = new string[] { "New", "Processing", "Accepted", "Acknowledge", "Acknowledged" },
            }
                .WithDateIsAfterFilter(new LocalDate(2001, 1, 1))
                .WithDateIsBeforeFilter(new LocalDate(2040, 1, 1));

            // Act
            var claims = this.stackFactory.ClaimService.GetClaims(
                tenant.Id,
                filters).ToList();

            // Assert
            claims.Count.Should().Be(10);
        }

        [Fact(Skip = "Experiencing inconsistent result")]
        public async Task GetClaimsForUser_ShouldIncludeClaimsFromSubOrganisations_WhenUserIsFromDefaultOrganisation()
        {
            // Arrange
            var environment = QuoteFactory.DefaultEnvironment;
            var tenant = TenantFactory.Create();
            var product = ProductFactory.Create(tenant.Id, Guid.NewGuid());

            var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null, this.performingUserId,
                SystemClock.Instance.GetCurrentInstant());
            var anotherOrganisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null, this.performingUserId,
                SystemClock.Instance.GetCurrentInstant());

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack.TenantRepository.Insert(tenant);
                stack.TenantRepository.SaveChanges();
                stack.ProductRepository.Insert(product);
                stack.ProductRepository.SaveChanges();

                await stack.OrganisationAggregateRepository.Save(organisation);
                tenant.SetDefaultOrganisation(
                    organisation.Id, SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromMinutes(1)));
                stack.TenantRepository.SaveChanges();

                await stack.OrganisationAggregateRepository.Save(anotherOrganisation);

                // Quote and claims from default organisation
                Domain.Aggregates.Quote.QuoteAggregate quote1 = QuoteFactory
                    .CreateNewPolicy(tenant.Id, product.Id, environment, organisationId: organisation.Id);
                await stack.QuoteAggregateRepository.Save(quote1);

                var claimAggregate1 = ClaimAggregate.CreateForPolicy(
                    "REF1234",
                    quote1,
                    Guid.NewGuid(),
                    "John Smith",
                    "Jonboy",
                    this.performingUserId,
                    stack.Clock.GetCurrentInstant());
                var releaseContext1 = new ReleaseContext(claimAggregate1.ProductContext, Guid.NewGuid());
                var workflow1 = await stack.ClaimWorkflowProvider.GetConfigurableClaimWorkflow(releaseContext1);
                claimAggregate1.ChangeClaimState(
                    ClaimActions.Actualise, Guid.NewGuid(), SystemClock.Instance.GetCurrentInstant(), workflow1);
                await stack.ClaimAggregateRepository.Save(claimAggregate1);

                var claimAggregate2 = ClaimAggregate.CreateForPolicy(
                    "REF1235",
                    quote1,
                    Guid.NewGuid(),
                    "John Smith",
                    "Jonboy",
                    this.performingUserId,
                    SystemClock.Instance.GetCurrentInstant());
                var releaseContext2 = new ReleaseContext(claimAggregate2.ProductContext, Guid.NewGuid());
                var workflow2 = await stack.ClaimWorkflowProvider.GetConfigurableClaimWorkflow(releaseContext2);
                claimAggregate2.ChangeClaimState(
                    ClaimActions.Return, Guid.NewGuid(), SystemClock.Instance.GetCurrentInstant(), workflow2);
                await stack.ClaimAggregateRepository.Save(claimAggregate2);

                // Quote and claims from sub-organisation
                Domain.Aggregates.Quote.QuoteAggregate quote2 = QuoteFactory
                    .CreateNewPolicy(tenant.Id, product.Id, environment, organisationId: anotherOrganisation.Id);
                await stack.QuoteAggregateRepository.Save(quote2);

                var claimAggregate3 = ClaimAggregate.CreateForPolicy(
                    "REF1236",
                    quote2,
                    Guid.NewGuid(),
                    "John Smith",
                    "Jonboy",
                    this.performingUserId,
                    SystemClock.Instance.GetCurrentInstant());
                var releaseContext3 = new ReleaseContext(claimAggregate3.ProductContext, Guid.NewGuid());
                var workflow3 = await stack.ClaimWorkflowProvider.GetConfigurableClaimWorkflow(releaseContext3);
                claimAggregate3.ChangeClaimState(
                    ClaimActions.Settle, Guid.NewGuid(), SystemClock.Instance.GetCurrentInstant(), workflow3);
                await stack.ClaimAggregateRepository.Save(claimAggregate3);
            }

            var userAuthData = new UserAuthenticationData(
                tenant.Id, organisation.Id, UserType.Client, Guid.NewGuid(), default);

            var filters = new EntityListFilters
            {
                TenantId = tenant.Id,
                OrganisationIds = new List<Guid> { organisation.Id },
                Environment = environment,
                Statuses = new string[] { "Acknowledged", "Incomplete", "Complete", "Notified" },
            }
            .WithDateIsAfterFilter(new LocalDate(2001, 1, 1))
            .WithDateIsBeforeFilter(new LocalDate(2040, 1, 1));

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // Act
                var claims = stack.ClaimService.GetClaims(tenant.Id, filters).ToList();
                Debug.WriteLine(claims);

                // Assert
                claims.Should().HaveCount(3);
                claims.Where(claim => claim.OrganisationId == organisation.Id).Should().HaveCount(2);
                claims.Where(claim => claim.OrganisationId == anotherOrganisation.Id).Should().HaveCount(1);
            }
        }

        [Fact(Skip = "Experiencing inconsistent result")]
        public async Task GetClaimsForUser_ShouldIncludeClaimsForSpecificOrganisation_WhenUserIsNotFromDefaultOrganisation()
        {
            // Arrange
            var environment = QuoteFactory.DefaultEnvironment;
            var tenant = TenantFactory.Create();
            var product = ProductFactory.Create(tenant.Id, Guid.NewGuid());

            var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null, this.performingUserId,
                SystemClock.Instance.GetCurrentInstant());
            var anotherOrganisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null, this.performingUserId,
                SystemClock.Instance.GetCurrentInstant());

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack.TenantRepository.Insert(tenant);
                stack.ProductRepository.Insert(product);
                stack.TenantRepository.SaveChanges();
                stack.ProductRepository.SaveChanges();

                await stack.OrganisationAggregateRepository.Save(organisation);
                tenant.SetDefaultOrganisation(
                    organisation.Id, SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromMinutes(1)));
                stack.TenantRepository.SaveChanges();

                await stack.OrganisationAggregateRepository.Save(anotherOrganisation);

                // Quote and claims from default organisation
                Domain.Aggregates.Quote.QuoteAggregate quote1 = QuoteFactory
                    .CreateNewPolicy(tenant.Id, product.Id, environment, organisationId: organisation.Id);
                await stack.QuoteAggregateRepository.Save(quote1);

                var claimAggregate1 = ClaimAggregate.CreateForPolicy(
                    "REF1237",
                    quote1,
                    Guid.NewGuid(),
                    "John Smith",
                    "Jonboy",
                    this.performingUserId,
                    SystemClock.Instance.GetCurrentInstant());
                var releaseContext1 = new ReleaseContext(claimAggregate1.ProductContext, Guid.NewGuid());
                var workflow1 = await stack.ClaimWorkflowProvider.GetConfigurableClaimWorkflow(releaseContext1);
                claimAggregate1.ChangeClaimState(
                    ClaimActions.Actualise, Guid.NewGuid(), SystemClock.Instance.GetCurrentInstant(), workflow1);
                await stack.ClaimAggregateRepository.Save(claimAggregate1);

                // Quote and claims from sub-organisation
                Domain.Aggregates.Quote.QuoteAggregate quote2 = QuoteFactory
                    .CreateNewPolicy(tenant.Id, product.Id, environment, organisationId: anotherOrganisation.Id);
                await stack.QuoteAggregateRepository.Save(quote2);

                var claimAggregate3 = ClaimAggregate.CreateForPolicy(
                    "REF1239",
                    quote2,
                    Guid.NewGuid(),
                    "John Smith",
                    "Jonboy",
                    this.performingUserId,
                    SystemClock.Instance.GetCurrentInstant());
                var releaseContext3 = new ReleaseContext(claimAggregate3.ProductContext, Guid.NewGuid());
                var workflow3 = await stack.ClaimWorkflowProvider.GetConfigurableClaimWorkflow(releaseContext3);
                claimAggregate3.ChangeClaimState(
                    ClaimActions.Settle, Guid.NewGuid(), SystemClock.Instance.GetCurrentInstant(), workflow3);
                await stack.ClaimAggregateRepository.Save(claimAggregate3);
            }

            var userAuthData = new UserAuthenticationData(
                tenant.Id, anotherOrganisation.Id, UserType.Client, Guid.NewGuid(), default);

            var filters = new EntityListFilters
            {
                TenantId = tenant.Id,
                OrganisationIds = new List<Guid> { anotherOrganisation.Id },
                Environment = environment,
                Statuses = new string[] { "Acknowledged", "Incomplete", "Complete", "Notified" },
            }
            .WithDateIsAfterFilter(new LocalDate(2001, 1, 1))
            .WithDateIsBeforeFilter(new LocalDate(2040, 1, 1));

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // Act
                var claims = stack.ClaimService.GetClaims(tenant.Id, filters).ToList();

                Debug.WriteLine(claims);

                // Assert
                claims.Where(claim => claim.OrganisationId == anotherOrganisation.Id).Should().HaveCount(1);
            }
        }
    }
}
