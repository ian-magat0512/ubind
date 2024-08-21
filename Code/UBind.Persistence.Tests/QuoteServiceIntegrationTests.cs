// <copyright file="QuoteServiceIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using NodaTime;
    using UBind.Application.User;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Authentication;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Tests;
    using UBind.Domain.Tests.Fakes;
    using UBind.Domain.Tests.Helpers;
    using UBind.Domain.ValueTypes;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class QuoteServiceIntegrationTests
    {
        private const string QuoteNumber = "FXOWQ";
        private readonly Guid? performingUserId = Guid.NewGuid();

        // Factory for stack configured to use real quote expiry setting provider.
        private Func<ApplicationStack> stackFactory = () => new ApplicationStack(
            DatabaseFixture.TestConnectionStringName,
            ApplicationStackConfiguration.WithRealQuoteExpirySettingProvider());

        private int updateExpiryDateDelayInSeconds = 1;

        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public async Task GetQuotesForUser_ShouldIncludeQuotesFromSubOrganisations_WhenUserIsFromDefaultOrganisation()
        {
            // Arrange
            var environment = QuoteFactory.DefaultEnvironment;
            var tenant = TenantFactory.Create(Guid.NewGuid());
            var product = ProductFactory.Create(tenant.Id, Guid.NewGuid());
            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null, this.performingUserId,
                SystemClock.Instance.GetCurrentInstant());
            var organisation2 = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null, this.performingUserId,
                SystemClock.Instance.Now());

            using (var stack = this.stackFactory())
            {
                stack.TenantRepository.Insert(tenant);
                stack.ProductRepository.Insert(product);
                stack.TenantRepository.SaveChanges();

                await stack.OrganisationAggregateRepository.Save(organisation);
                tenant.SetDefaultOrganisation(
                    organisation.Id, SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromMinutes(1)));
                stack.TenantRepository.SaveChanges();

                await stack.OrganisationAggregateRepository.Save(organisation2);

                // Incomplete from organisation 1 (default)
                var quote1 = QuoteFactory
                    .CreateNewBusinessQuote(tenant.Id, product.Id, environment, null, organisation.Id);
                await stack.QuoteAggregateRepository.Save(
                    quote1
                        .Aggregate
                        .WithCustomer()
                        .WithQuoteNumber(quote1.Id)
                        .WithFormData(quote1.Id));

                // Incomplete from organisation 1 (default)
                var quote2 = QuoteFactory
                    .CreateNewBusinessQuote(tenant.Id, product.Id, environment, null, organisation.Id);
                await stack.QuoteAggregateRepository.Save(
                    quote2
                        .Aggregate
                        .WithCustomer()
                        .WithQuoteNumber(quote2.Id)
                        .WithFormData(quote2.Id));

                // Complete from organisation 1 (default)
                await stack.QuoteAggregateRepository.Save(
                    QuoteFactory.CreateNewPolicy(tenant.Id, product.Id, environment, organisationId: organisation.Id));

                // Incomplete from organisation 2 (sub)
                var quote3 = QuoteFactory
                    .CreateNewBusinessQuote(tenant.Id, product.Id, environment, null, organisation2.Id);
                await stack.QuoteAggregateRepository.Save(
                    quote3
                        .Aggregate
                        .WithCustomer()
                        .WithQuoteNumber(quote3.Id));

                // Complete from organisation 1 (default)
                await stack.QuoteAggregateRepository.Save(
                    QuoteFactory.CreateNewPolicy(tenant.Id, product.Id, organisationId: organisation.Id));
            }

            using (var stack = this.stackFactory())
            {
                var filters = new QuoteReadModelFilters
                {
                    Statuses = new string[] { StandardQuoteStates.Incomplete, StandardQuoteStates.Complete },
                    IncludeTestData = true,
                }
                .WithDateIsAfterFilter("2000-01-01".ToLocalDateFromIso8601())
                .WithDateIsBeforeFilter(stack.Clock.Today().PlusDays(1).ToIso8601().ToLocalDateFromIso8601());

                // Act
                var clientAdminAuthData = new UserAuthenticationData(
                    tenant.Id,
                    tenant.Details.DefaultOrganisationId,
                    UserType.Client,
                    Guid.NewGuid(),
                    default);
                var quotes = stack.QuoteService.GetQuotes(tenant.Id, (QuoteReadModelFilters)filters);

                // Assert
                quotes.Should().HaveCount(5);
                quotes.Count(quote => quote.OrganisationId == organisation.Id).Should().Be(4);
                quotes.Count(quote => quote.OrganisationId == organisation2.Id).Should().Be(1);
            }
        }

        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public async Task GetQuotesForUser_ShouldIncludeQuotesForSpecificOrganisation_WhenUserIsNotFromDefaultOrganisation()
        {
            // Arrange
            var environment = QuoteFactory.DefaultEnvironment;
            var tenant = TenantFactory.Create(Guid.NewGuid());
            var product = ProductFactory.Create(tenant.Id, Guid.NewGuid());
            var now = SystemClock.Instance.Now();
            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id, tenant.Details.Alias, tenant.Details.Name, null, this.performingUserId, now);
            var organisation2 = Organisation.CreateNewOrganisation(
                tenant.Id, tenant.Details.Alias, tenant.Details.Name, null, this.performingUserId, now);

            using (var stack = this.stackFactory())
            {
                stack.TenantRepository.Insert(tenant);
                stack.ProductRepository.Insert(product);
                stack.TenantRepository.SaveChanges();

                await stack.OrganisationAggregateRepository.Save(organisation);
                tenant.SetDefaultOrganisation(organisation.Id, stack.Clock.Now().Plus(Duration.FromMinutes(1)));
                stack.TenantRepository.SaveChanges();

                await stack.OrganisationAggregateRepository.Save(organisation2);

                // Incomplete from organisation 1 (default)
                var quote1 = QuoteFactory.CreateNewBusinessQuote(tenant.Id, product.Id);
                await stack.QuoteAggregateRepository.Save(
                    quote1
                        .Aggregate
                        .WithCustomer()
                        .WithQuoteNumber(quote1.Id)
                        .WithFormData(quote1.Id));

                // Incomplete from organisation 1 (default)
                var quote2 = QuoteFactory.CreateNewBusinessQuote(tenant.Id, product.Id);
                await stack.QuoteAggregateRepository.Save(
                    quote2
                        .Aggregate
                        .WithCustomer()
                        .WithQuoteNumber(quote2.Id)
                        .WithFormData(quote2.Id));

                // Complete from organisation 1 (default)
                await stack.QuoteAggregateRepository.Save(QuoteFactory.CreateNewPolicy(tenant.Id, product.Id));

                // Incomplete from organisation 2 (sub)
                var quote3 = QuoteFactory
                    .CreateNewBusinessQuote(tenant.Id, product.Id, environment, null, organisation2.Id);
                await stack.QuoteAggregateRepository.Save(quote3.Aggregate.WithCustomer().WithQuoteNumber(quote3.Id));

                // Complete from organisation 2 (sub)
                var quoteAggregate5 = QuoteFactory.CreateNewPolicy(
                    tenant.Id, product.Id, environment, null, null, "POLNUM1", organisation2.Id);
                await stack.QuoteAggregateRepository.Save(quoteAggregate5);
            }

            using (var stack = this.stackFactory())
            {
                var filters = new QuoteReadModelFilters
                {
                    Statuses = new string[] { StandardQuoteStates.Incomplete, StandardQuoteStates.Complete },
                    IncludeTestData = true,
                }
                .WithDateIsAfterFilter("2000-01-01".ToLocalDateFromIso8601())
                .WithDateIsBeforeFilter(stack.Clock.Today().PlusDays(1).ToIso8601().ToLocalDateFromIso8601());

                var userAuthData = new UserAuthenticationData(
                    tenant.Id, organisation2.Id, UserType.Client, Guid.NewGuid(), default);

                // Act
                var quotes = stack.QuoteService.GetQuotes(tenant.Id, (QuoteReadModelFilters)filters);

                // Assert
                quotes.Should().HaveCount(2);
                quotes.Where(policy => policy.OrganisationId == organisation2.Id).Should().HaveCount(2);
            }
        }

        [Fact]
        public async Task ListQuotes_FiltersOnStatusCorrectly_ForMultipleStatuses()
        {
            // Arrange
            var tenant = TenantFactory.Create(Guid.NewGuid());
            var product = ProductFactory.Create(tenant.Id, Guid.NewGuid());
            var now = SystemClock.Instance.Now();
            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id, tenant.Details.Alias, tenant.Details.Name, null, this.performingUserId, now);

            using (var stack = this.stackFactory())
            {
                stack.CreateTenant(tenant);
                stack.CreateProduct(product);

                await stack.OrganisationAggregateRepository.Save(organisation);
                tenant.SetDefaultOrganisation(organisation.Id, stack.Clock.Now().Plus(Duration.FromMinutes(1)));
                stack.TenantRepository.SaveChanges();

                // Incomplete
                var quote1
                    = QuoteFactory.CreateNewBusinessQuote(
                        tenant.Id, product.Id, organisationId: organisation.Id, initialQuoteState: StandardQuoteStates.Incomplete);
                await stack.QuoteAggregateRepository.Save(
                    quote1
                        .Aggregate
                        .WithCustomer()
                        .WithQuoteNumber(quote1.Id)
                        .WithFormData(quote1.Id));

                // Incomplete
                var quote2
                    = QuoteFactory.CreateNewBusinessQuote(
                        tenant.Id, product.Id, organisationId: organisation.Id, initialQuoteState: StandardQuoteStates.Incomplete);
                await stack.QuoteAggregateRepository.Save(
                    quote2
                        .Aggregate
                        .WithCustomer()
                        .WithQuoteNumber(quote2.Id)
                        .WithFormData(quote2.Id));

                // Complete
                await stack.QuoteAggregateRepository.Save(
                    QuoteFactory.CreateNewPolicy(tenant.Id, product.Id, organisationId: organisation.Id));

                // Incomplete
                var quote3
                    = QuoteFactory.CreateNewBusinessQuote(
                        tenant.Id, product.Id, organisationId: organisation.Id, initialQuoteState: StandardQuoteStates.Incomplete);
                await stack.QuoteAggregateRepository.Save(
                    quote3
                        .Aggregate
                        .WithCustomer()
                        .WithQuoteNumber(quote3.Id));

                // Complete
                await stack.QuoteAggregateRepository.Save(QuoteFactory.CreateNewPolicy(tenant.Id, product.Id, organisationId: organisation.Id));
            }

            using (var stack = this.stackFactory())
            {
                var filters = new QuoteReadModelFilters
                {
                    Statuses = new string[] { StandardQuoteStates.Incomplete, StandardQuoteStates.Complete },
                    IncludeTestData = true,
                    Environment = QuoteFactory.DefaultEnvironment,
                }
                .WithDateIsAfterFilter("2000-01-01".ToLocalDateFromIso8601())
                .WithDateIsBeforeFilter(stack.Clock.Today().PlusDays(1).ToIso8601().ToLocalDateFromIso8601());

                // Act
                var quotes = stack.QuoteService.GetQuotes(tenant.Id, (QuoteReadModelFilters)filters);

                // Assert
                quotes.Count().Should().Be(5);
            }
        }

        [Fact]
        public async Task ListQuotes_FiltersOnCreatedDateCorrectly_ExcludingEarlyAndLateQuotes()
        {
            // Arrange
            var testClock = new TestClock();
            testClock.SetToInstant(Instant.FromUtc(2018, 6, 1, 0, 0));
            QuoteFactory.Clock = testClock;
            var tenant = TenantFactory.Create(Guid.NewGuid());
            var product = ProductFactory.Create(tenant.Id, Guid.NewGuid());
            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null, this.performingUserId,
                SystemClock.Instance.Now());
            var includedTimestamp = Instant.FromUtc(2019, 6, 1, 0, 0);

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack.CreateTenant(tenant);
                stack.CreateProduct(product);

                await stack.OrganisationAggregateRepository.Save(organisation);
                tenant.SetDefaultOrganisation(organisation.Id, stack.Clock.Now().Plus(Duration.FromMinutes(1)));
                stack.TenantRepository.SaveChanges();

                var quote1
                    = QuoteFactory.CreateNewBusinessQuote(tenant.Id, product.Id, organisationId: organisation.Id);
                await stack.QuoteAggregateRepository.Save(
                    quote1
                        .Aggregate
                        .WithCustomer()
                        .WithQuoteNumber(quote1.Id));
                testClock.SetToInstant(includedTimestamp);

                var quote2
                    = QuoteFactory.CreateNewBusinessQuote(tenant.Id, product.Id, organisationId: organisation.Id);
                await stack.QuoteAggregateRepository.Save(
                    quote2
                        .Aggregate
                        .WithCustomer()
                        .WithQuoteNumber(quote2.Id));
                testClock.SetToInstant(Instant.FromUtc(2020, 6, 1, 0, 0));
                var quote3
                    = QuoteFactory.CreateNewBusinessQuote(tenant.Id, product.Id, organisationId: organisation.Id);
                await stack.QuoteAggregateRepository.Save(
                    quote3
                        .Aggregate
                        .WithCustomer()
                        .WithQuoteNumber(quote3.Id));
            }

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var filters = new QuoteReadModelFilters
                {
                    SearchTerms = Enumerable.Empty<string>(),
                    Statuses = Enumerable.Empty<string>(),
                    IncludeTestData = true,
                    Environment = QuoteFactory.DefaultEnvironment,
                };

                filters.DateFilteringPropertyName = nameof(NewQuoteReadModel.CreatedTicksSinceEpoch);
                filters.WithDateIsAfterFilter("2018-07-01".ToLocalDateFromIso8601());
                filters.WithDateIsBeforeFilter("2020-05-01".ToLocalDateFromIso8601());

                // Act
                var quotes = stack.QuoteService.GetQuotes(tenant.Id, (QuoteReadModelFilters)filters);

                // Assert
                quotes.Should().ContainSingle();
                includedTimestamp.Should().Be(quotes.Single().CreatedTimestamp);
            }
        }

        [Fact]
        public async Task ListQuotes_FiltersOnSearchTermsCorrectly_UsingQuoteNumber()
        {
            // Arrange
            var tenant = TenantFactory.Create(Guid.NewGuid());
            var product = ProductFactory.Create(tenant.Id, Guid.NewGuid());
            var now = SystemClock.Instance.Now();
            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id, tenant.Details.Alias, tenant.Details.Name, null, this.performingUserId, now);
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack.CreateTenant(tenant);
                stack.CreateProduct(product);

                await stack.OrganisationAggregateRepository.Save(organisation);
                tenant.SetDefaultOrganisation(organisation.Id, now.Plus(Duration.FromMinutes(1)));
                stack.TenantRepository.SaveChanges();

                var nascentQuote = QuoteFactory
                    .CreateNewBusinessQuote(
                    tenant.Id,
                    product.Id,
                    organisationId: organisation.Id).Aggregate;
                await stack.QuoteAggregateRepository.Save(nascentQuote);
                var incompleteQuote
                    = QuoteFactory.CreateNewBusinessQuote(
                        tenant.Id,
                        product.Id,
                        organisationId: organisation.Id);
                var incompleteQuoteAggregate = incompleteQuote
                    .Aggregate
                    .WithCustomer()
                    .WithQuoteNumber(incompleteQuote.Id, "BBBBBB")
                    .WithFormData(incompleteQuote.Id);
                await stack.QuoteAggregateRepository.Save(incompleteQuoteAggregate);
                var completeQuote = QuoteFactory.CreateNewPolicy(
                    tenant.Id,
                    product.Id,
                    organisationId: organisation.Id);
                await stack.QuoteAggregateRepository.Save(completeQuote);
            }

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var filters = new QuoteReadModelFilters
                {
                    SearchTerms = new List<string> { "BBBBBB" },
                    Statuses = new List<string> { "Incomplete", "Nascent" },
                    IncludeTestData = true,
                    Environment = QuoteFactory.DefaultEnvironment,
                }
                .WithDateIsAfterFilter("2001-11-19".ToLocalDateFromIso8601())
                .WithDateIsBeforeFilter(stack.Clock.Today().PlusDays(1).ToIso8601().ToLocalDateFromIso8601());

                // Act
                var quotes = stack.QuoteService.GetQuotes(tenant.Id, (QuoteReadModelFilters)filters);

                // Assert
                quotes.Should().ContainSingle();
                "BBBBBB".Should().Be(quotes.Single().QuoteNumber);
            }
        }

        [Fact(Skip = "Rewrite this test to use appropriate mocks so that it's quick, e.g. <50ms")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task QuoteService_CapsQuoteListsAt1000()
        {
            // Arrange
            var tenant = TenantFactory.Create(Guid.NewGuid());
            var product = ProductFactory.Create(tenant.Id, Guid.NewGuid());
            var now = SystemClock.Instance.Now();

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack.TenantRepository.Insert(tenant);
                stack.ProductRepository.Insert(product);
                stack.TenantRepository.SaveChanges();
                stack.ProductRepository.SaveChanges();

                // Create user
                var adminPerson = PersonAggregate.CreatePerson(
                    tenant.Id, tenant.Details.DefaultOrganisationId, this.performingUserId, now);
                var adminUser = UserAggregate.CreateUser(
                    adminPerson.TenantId, Guid.NewGuid(), UserType.Client, adminPerson, this.performingUserId, null, now);
                var userAuthenticationData = new UserAuthenticationData(
                    tenant.Id, tenant.Details.DefaultOrganisationId, UserType.Client, adminUser.Id, default);

                for (int i = 0; i < 1010; ++i)
                {
                    var quote = QuoteFactory.CreateNewBusinessQuote(tenant.Id, product.Id);
                    var quoteAggregate = quote.Aggregate
                        .WithCustomer()
                        .WithQuoteNumber(quote.Id);
                    await stack.QuoteAggregateRepository.Save(quoteAggregate);
                }
            }

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var filters = new QuoteReadModelFilters
                {
                    SearchTerms = Array.Empty<string>(),
                    Statuses = new string[] { "Nascent", "Incomplete" },
                    IncludeTestData = true,
                    Environment = QuoteFactory.DefaultEnvironment,
                }
                .WithDateIsAfterFilter("2000-01-01".ToLocalDateFromIso8601())
                .WithDateIsBeforeFilter(stack.Clock.Today().PlusDays(1).ToIso8601().ToLocalDateFromIso8601());

                // Act
                var userAuthData = new UserAuthenticationData(
                    tenant.Id, tenant.Details.DefaultOrganisationId, UserType.Client, Guid.NewGuid(), default);
                var quotes = stack.QuoteService.GetQuotes(tenant.Id, (QuoteReadModelFilters)filters);

                // Assert
                1000.Should().Be(quotes.Count());
            }
        }

        [Fact]
        public async Task ListQuotes_Adjustment_UsingUniqueQuoteNumber()
        {
            // Arrange
            var tenant = TenantFactory.Create(Guid.NewGuid());
            var product = ProductFactory.Create(tenant.Id, Guid.NewGuid());
            var now = SystemClock.Instance.Now();
            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id, tenant.Details.Alias, tenant.Details.Name, null, this.performingUserId, now);

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack.CreateTenant(tenant);
                stack.CreateProduct(product);

                await stack.OrganisationAggregateRepository.Save(organisation);
                tenant.SetDefaultOrganisation(organisation.Id, now.Plus(Duration.FromMinutes(1)));
                stack.TenantRepository.SaveChanges();

                var nascentQuote = QuoteFactory.CreateNewBusinessQuote(
                    tenant.Id,
                    product.Id,
                    organisationId: organisation.Id).Aggregate;

                await stack.QuoteAggregateRepository.Save(nascentQuote);
                var incompleteQuote = QuoteFactory.CreateNewBusinessQuote(
                    tenant.Id,
                    product.Id,
                    organisationId: organisation.Id);

                var incompleteQuoteAggregate = incompleteQuote
                    .Aggregate
                    .WithCustomer()
                    .WithQuoteNumber(incompleteQuote.Id, "CCCCCC")
                    .WithFormData(incompleteQuote.Id);
                await stack.QuoteAggregateRepository.Save(incompleteQuote.Aggregate);
                var completeQuote = QuoteFactory.CreateNewPolicy(
                    tenant.Id,
                    product.Id,
                    organisationId: organisation.Id);
                var adjustmentQuote = completeQuote
                    .WithAdjustmentQuote(SystemClock.Instance.GetCurrentInstant(), "DDDDDD");
                completeQuote = adjustmentQuote.Aggregate;
                await stack.QuoteAggregateRepository.Save(completeQuote);
            }

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var filters = new QuoteReadModelFilters
                {
                    IncludeTestData = true,
                    Environment = QuoteFactory.DefaultEnvironment,
                };

                // Act
                var userAuthData = new UserAuthenticationData(
                    tenant.Id, organisation.Id, UserType.Client, Guid.NewGuid(), default);
                var quotes = stack.QuoteService.GetQuotes(tenant.Id, filters);
                var quoteAdjustment = quotes.Where(w => w.QuoteType == QuoteType.Adjustment);

                // Assert
                quoteAdjustment.Should().ContainSingle();
                quoteAdjustment.Single().QuoteNumber.Contains('-').Should().BeFalse();
                "DDDDDD".Should().Be(quoteAdjustment.Single().QuoteNumber);
            }
        }

        [Fact]
        public async Task ListQuotes_Renewal_UsingUniqueQuoteNumber()
        {
            // Arrange
            var tenant = TenantFactory.Create(Guid.NewGuid());
            var product = ProductFactory.Create(tenant.Id, Guid.NewGuid());
            var now = SystemClock.Instance.Now();
            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id, tenant.Details.Alias, tenant.Details.Name, null, this.performingUserId, now);
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack.CreateTenant(tenant);
                stack.CreateProduct(product);

                await stack.OrganisationAggregateRepository.Save(organisation);
                tenant.SetDefaultOrganisation(organisation.Id, now.Plus(Duration.FromMinutes(1)));
                stack.TenantRepository.SaveChanges();

                var nascentQuote = QuoteFactory.CreateNewBusinessQuote(
                    tenant.Id, product.Id, organisationId: organisation.Id).Aggregate;
                await stack.QuoteAggregateRepository.Save(nascentQuote);
                var incompleteQuote = QuoteFactory.CreateNewBusinessQuote(
                    tenant.Id, product.Id, organisationId: organisation.Id);
                var incompleteQuoteAggregate = incompleteQuote
                    .Aggregate
                    .WithCustomer()
                    .WithQuoteNumber(incompleteQuote.Id, "CCCCCC")
                    .WithFormData(incompleteQuote.Id);
                await stack.QuoteAggregateRepository.Save(incompleteQuote.Aggregate);
                var completeQuote = QuoteFactory.CreateNewPolicy(
                    tenant.Id, product.Id, organisationId: organisation.Id);
                completeQuote
                    .WithRenewalQuote(SystemClock.Instance.GetCurrentInstant(), "DDDDDD");
                await stack.QuoteAggregateRepository.Save(completeQuote);
            }

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var filters = new QuoteReadModelFilters
                {
                    IncludeTestData = true,
                    Environment = QuoteFactory.DefaultEnvironment,
                };

                // Act
                var userAuthData = new UserAuthenticationData(
                    tenant.Id, tenant.Details.DefaultOrganisationId, UserType.Client, Guid.NewGuid(), default);
                var quotes = stack.QuoteService.GetQuotes(tenant.Id, filters);
                var quoteRenewal = quotes.Where(w => w.QuoteType == QuoteType.Renewal);

                // Assert
                quoteRenewal.Should().ContainSingle();
                quoteRenewal.Single().QuoteNumber.Contains('-').Should().BeFalse();
                "DDDDDD".Should().Be(quoteRenewal.Single().QuoteNumber);
            }
        }

        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public async Task GetQuoteDetailsForUser_NewQuotesHasExpiry_SetProductQuoteExpirySettings()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var environment = DeploymentEnvironment.Development;
            var quoteExpirySettings = new QuoteExpirySettings(20, true);
            var tenantProductModel = await this.SetupTenantAndProduct(tenantId, productId);
            var tenant = tenantProductModel.Tenant;
            var product = tenantProductModel.Product;
            await this.ProductUpdateExpiryDates(
                 tenantId,
                 productId,
                 quoteExpirySettings,
                 ProductQuoteExpirySettingUpdateType.UpdateNone);

            Quote quote = null;
            using (var stack = this.stackFactory())
            {
                quote = QuoteAggregate.CreateNewBusinessQuote(
                   tenantId,
                   organisationId,
                   productId,
                   environment,
                   QuoteExpirySettings.Default,
                   this.performingUserId,
                   stack.Clock.Now(),
                   Guid.NewGuid(),
                   Timezones.AET);

                quote.Aggregate.SetQuoteExpiryFromSettings(
                    quote.Id, this.performingUserId, stack.Clock.Now(), quoteExpirySettings);

                await stack.QuoteAggregateRepository.Save(quote.Aggregate);
            }

            // Act
            using (var stack = this.stackFactory())
            {
                var storedValue = stack.QuoteService.GetQuoteDetails(
                    tenant.Id, quote.Id);
                var expiryDate = Instant.Add(
                    quote.Aggregate.CreatedTimestamp, Duration.FromDays(quoteExpirySettings.ExpiryDays));

                // Assert
                expiryDate.ToRfc5322DateStringInAet().Should().Be(
                    storedValue.ExpiryTimestamp.Value.ToRfc5322DateStringInAet());
                StandardQuoteStates.Nascent.Should().Be(storedValue.QuoteState);
            }
        }

        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public async Task ListQuotesForUser_NewQuotesHasExpiry_SetProductQuoteExpirySettings()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var environment = DeploymentEnvironment.Development;
            var quoteExpirySettings = new QuoteExpirySettings(20, true);
            var tenantProductModel = await this.SetupTenantAndProduct(tenantId, productId);
            var tenant = tenantProductModel.Tenant;
            await this.ProductUpdateExpiryDates(
                tenantId,
                productId,
                quoteExpirySettings,
                ProductQuoteExpirySettingUpdateType.UpdateNone);

            // Create customer
            var customerPerson = this.GetPersonAggregate(tenant, default);
            CustomerAggregate customerAggregate = await this.CreateCustomer(
                tenantProductModel.Tenant, customerPerson, environment);

            // Create performing user
            UserAuthenticationData userAuthData = new UserAuthenticationData(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                UserType.Client,
                customerPerson.UserId.Value,
                default);

            QuoteAggregate quoteAggregate;
            using (var stack = this.stackFactory())
            {
                var quote = QuoteAggregate.CreateNewBusinessQuote(
                   tenantId,
                   organisationId,
                   productId,
                   environment,
                   QuoteExpirySettings.Default,
                   this.performingUserId,
                   stack.Clock.Now(),
                   Guid.NewGuid(),
                   Timezones.AET);
                quoteAggregate = quote.Aggregate;
                quoteAggregate.SetQuoteExpiryFromSettings(
                    quote.Id, this.performingUserId, stack.Clock.Now(), quoteExpirySettings);
                quoteAggregate.RecordAssociationWithCustomer(
                    customerAggregate, new PersonalDetails(customerPerson), this.performingUserId, stack.Clock.Now());
                quote.AssignQuoteNumber(
                    QuoteNumber,
                    this.performingUserId,
                    stack.Clock.Now());
                await stack.QuoteAggregateRepository.Save(quoteAggregate);
            }

            var filters = new QuoteReadModelFilters
            {
                IncludeTestData = false,
            };

            using (var stack = this.stackFactory())
            {
                // Act
                var storedValue = stack.QuoteService.GetQuotes(
                    tenant.Id, filters).FirstOrDefault();
                var expiryDate = Instant.Add(
                    quoteAggregate.CreatedTimestamp, Duration.FromDays(quoteExpirySettings.ExpiryDays));

                // Assert
                expiryDate.ToRfc5322DateStringInAet().Should().Be(
                    storedValue.ExpiryTimestamp.Value.ToRfc5322DateStringInAet());
                StandardQuoteStates.Incomplete.Should().Be(storedValue.QuoteState);
            }
        }

        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public async Task ListQuotesForUser_EmptyResult_QueryExpirednNewlySetQuote()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var environment = DeploymentEnvironment.Development;
            var quoteExpirySettings = new QuoteExpirySettings(20, true);
            var tenantProductModel = await this.SetupTenantAndProduct(tenantId, productId);
            await this.ProductUpdateExpiryDates(
                tenantId,
                productId,
                quoteExpirySettings,
                ProductQuoteExpirySettingUpdateType.UpdateNone);

            // Create customer
            var tenant = tenantProductModel.Tenant;
            var customerPerson = this.GetPersonAggregate(tenant, default);
            CustomerAggregate customerAggregate = await this.CreateCustomer(
                tenantProductModel.Tenant, customerPerson, environment);

            // Create performing user
            UserAuthenticationData userAuthData = new UserAuthenticationData(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                UserType.Client,
                customerPerson.UserId.Value,
                default);

            using (var stack = this.stackFactory())
            {
                var quote = QuoteAggregate.CreateNewBusinessQuote(
                   tenantId,
                   organisationId,
                   productId,
                   environment,
                   QuoteExpirySettings.Default,
                   this.performingUserId,
                   stack.Clock.Now(),
                   Guid.NewGuid(),
                   Timezones.AET);
                var quoteAggregate = quote.Aggregate;

                quoteAggregate.SetQuoteExpiryFromSettings(
                    quoteAggregate.Id, this.performingUserId, stack.Clock.Now(), quoteExpirySettings);
                quoteAggregate.RecordAssociationWithCustomer(
                    customerAggregate, new PersonalDetails(customerPerson), this.performingUserId, stack.Clock.Now());

                // Associate
                quote.AssignQuoteNumber(
                    QuoteNumber,
                    this.performingUserId,
                    stack.Clock.Now());
                await stack.QuoteAggregateRepository.Save(quoteAggregate);

                var filters = new QuoteReadModelFilters
                {
                    IncludeTestData = false,
                    Statuses = new List<string> { StandardQuoteStates.Expired },
                };

                // Act
                var storedValue = stack.QuoteService.GetQuotes(
                    tenant.Id, filters).FirstOrDefault();

                // Assert
                storedValue.Should().BeNull();
            }
        }

        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public async Task ListQuotesForUser_WithResult_DefaultExpiryDateForNewlySetQuote()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var environment = DeploymentEnvironment.Development;
            var quoteExpirySettings = new QuoteExpirySettings(20, true);
            var tenantProductModel = await this.SetupTenantAndProduct(tenantId, productId);
            await this.ProductUpdateExpiryDates(
                tenantId,
                productId,
                quoteExpirySettings,
                ProductQuoteExpirySettingUpdateType.UpdateNone);

            // Create customer
            var tenant = tenantProductModel.Tenant;
            var customerPerson = this.GetPersonAggregate(tenant, default);
            CustomerAggregate customerAggregate = await this.CreateCustomer(
                tenantProductModel.Tenant, customerPerson, environment);

            // Create performing user
            UserAuthenticationData userAuthData = new UserAuthenticationData(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                UserType.Client,
                customerPerson.UserId.Value,
                default);

            using (var stack = this.stackFactory())
            {
                var quote = QuoteAggregate.CreateNewBusinessQuote(
                   tenantId,
                   organisationId,
                   productId,
                   environment,
                   QuoteExpirySettings.Default,
                   this.performingUserId,
                   stack.Clock.Now(),
                   Guid.NewGuid(),
                   Timezones.AET);
                quote.Aggregate.RecordAssociationWithCustomer(
                    customerAggregate, new PersonalDetails(customerPerson), this.performingUserId, stack.Clock.Now());

                // Associate
                quote.AssignQuoteNumber(
                    QuoteNumber, this.performingUserId, stack.Clock.Now());
                await stack.QuoteAggregateRepository.Save(quote.Aggregate);

                var filters = new QuoteReadModelFilters
                {
                    IncludeTestData = false,
                    Statuses = new List<string> { StandardQuoteStates.Incomplete },
                };

                // Act
                var storedValue = stack.QuoteService.GetQuotes(tenant.Id, filters)
                    .FirstOrDefault();

                // Assert
                StandardQuoteStates.Incomplete.Should().Be(storedValue.QuoteState);
            }
        }

        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public async Task GetQuoteDetailsForUser_IncompleteStatus_DefaultExpiryDateForNewlySetQuote()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var environment = DeploymentEnvironment.Development;
            var quoteExpirySettings = new QuoteExpirySettings(20, true);
            var tenantProductModel = await this.SetupTenantAndProduct(tenantId, productId);
            await this.ProductUpdateExpiryDates(
                tenantId,
                productId,
                quoteExpirySettings,
                ProductQuoteExpirySettingUpdateType.UpdateNone);

            // Create customer
            var tenant = tenantProductModel.Tenant;
            var customerPerson = this.GetPersonAggregate(tenant, default);
            CustomerAggregate customerAggregate = await this.CreateCustomer(
                tenantProductModel.Tenant, customerPerson, environment);

            // Create performing user
            UserAuthenticationData userAuthData = new UserAuthenticationData(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                UserType.Client,
                customerPerson.UserId.Value,
                default);

            using (var stack = this.stackFactory())
            {
                var quote = QuoteAggregate.CreateNewBusinessQuote(
                   tenantId,
                   organisationId,
                   productId,
                   environment,
                   QuoteExpirySettings.Default,
                   this.performingUserId,
                   stack.Clock.Now(),
                   Guid.NewGuid(),
                   Timezones.AET);

                quote.Aggregate.RecordAssociationWithCustomer(
                    customerAggregate, new PersonalDetails(customerPerson), this.performingUserId, stack.Clock.Now());

                // Associate
                quote.AssignQuoteNumber(
                    QuoteNumber,
                    this.performingUserId,
                    stack.Clock.Now());
                await stack.QuoteAggregateRepository.Save(quote.Aggregate);

                // Act
                var storedValue = stack.QuoteService.GetQuoteDetails(tenant.Id, quote.Id);

                // Assert
                StandardQuoteStates.Incomplete.Should().Be(storedValue.QuoteState);
            }
        }

        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public async Task ListQuotesForUser_StatusIsExpired_WaitQuoteExpire()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var environment = DeploymentEnvironment.Development;
            var quoteExpirySettings = new QuoteExpirySettings(30, true);
            var tenantProductModel = await this.SetupTenantAndProduct(tenantId, tenantId);
            await this.ProductUpdateExpiryDates(
                tenantId,
                productId,
                quoteExpirySettings,
                ProductQuoteExpirySettingUpdateType.UpdateNone);

            // Create customer
            var tenant = tenantProductModel.Tenant;
            var customerPerson = this.GetPersonAggregate(tenant, default);
            CustomerAggregate customerAggregate = await this.CreateCustomer(
                tenantProductModel.Tenant, customerPerson, environment);

            // Create performing user
            UserAuthenticationData userAuthData = new UserAuthenticationData(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                UserType.Client,
                customerPerson.UserId.Value,
                default);

            using (var stack = this.stackFactory())
            {
                var now = stack.Clock.Now();
                var quote = QuoteAggregate.CreateNewBusinessQuote(
                    tenantId,
                    organisationId,
                    productId,
                    environment,
                    quoteExpirySettings,
                    this.performingUserId,
                    now,
                    Guid.NewGuid(),
                    Timezones.AET);
                quote.Aggregate.RecordAssociationWithCustomer(
                    customerAggregate, new PersonalDetails(customerPerson), this.performingUserId, now);

                // Associate
                quote.AssignQuoteNumber(QuoteNumber, this.performingUserId, now);
                await stack.QuoteAggregateRepository.Save(quote.Aggregate);
            }

            var filters = new QuoteReadModelFilters
            {
                IncludeTestData = false,
                Statuses = new List<string> { "Expired" },
            };

            using (var stack = this.stackFactory())
            {
                // Wait for it
                stack.Clock.Increment(Duration.FromDays(31));

                // Act
                IQuoteReadModelSummary storedValue = stack.QuoteService.GetQuotes(
                    tenant.Id, filters).FirstOrDefault();

                // Assert
                StandardQuoteStates.Expired.Should().Be(storedValue.QuoteState);
            }
        }

        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public async Task GetQuoteDetailsForUser_StatusIsExpired_WaitQuoteExpire()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var environment = DeploymentEnvironment.Development;
            var quoteExpirySettings = new QuoteExpirySettings(30, true);
            var tenantProductModel = await this.SetupTenantAndProduct(tenantId, productId);
            await this.ProductUpdateExpiryDates(
                tenantId,
                productId,
                quoteExpirySettings,
                ProductQuoteExpirySettingUpdateType.UpdateNone);

            // Create customer
            var tenant = tenantProductModel.Tenant;
            var customerPerson = this.GetPersonAggregate(tenant, default);
            CustomerAggregate customerAggregate = await this.CreateCustomer(
                tenantProductModel.Tenant, customerPerson, environment);

            // Create performing user
            UserAuthenticationData userAuthData = new UserAuthenticationData(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                UserType.Client,
                customerPerson.UserId.Value,
                default);

            Quote quote = null;
            using (var stack = this.stackFactory())
            {
                var now = stack.Clock.Now();
                quote = QuoteAggregate.CreateNewBusinessQuote(
                    tenantId,
                    organisationId,
                    productId,
                    environment,
                    quoteExpirySettings,
                    this.performingUserId,
                    now,
                    Guid.NewGuid(),
                    Timezones.AET);
                quote.Aggregate.RecordAssociationWithCustomer(
                    customerAggregate, new PersonalDetails(customerPerson), this.performingUserId, now);
                quote.AssignQuoteNumber(QuoteNumber, this.performingUserId, now);
                await stack.QuoteAggregateRepository.Save(quote.Aggregate);
            }

            using (var stack = this.stackFactory())
            {
                // Wait for it
                stack.Clock.Increment(Duration.FromDays(31));

                // Act
                var storedValue = stack.QuoteService.GetQuoteDetails(tenant.Id, quote.Id);

                // Assert
                StandardQuoteStates.Expired.Should().Be(storedValue.QuoteState);
            }
        }

        // todo
        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public async Task UpdateExpiryDates_StatusIsStillExpired_WithLowerExpiryDateUpdateAndAfterQuoteIsExpired()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var environment = DeploymentEnvironment.Development;
            var quoteExpirySettings = new QuoteExpirySettings(30, true);
            var quoteExpirySettings2 = new QuoteExpirySettings(20, true);
            var tenantProductModel = await this.SetupTenantAndProduct(tenantId, productId);
            await this.ProductUpdateExpiryDates(
                tenantId,
                productId,
                quoteExpirySettings,
                ProductQuoteExpirySettingUpdateType.UpdateNone);

            // Create customer
            var tenant = tenantProductModel.Tenant;
            var customerPerson = this.GetPersonAggregate(tenant, default);
            CustomerAggregate customerAggregate = await this.CreateCustomer(
                tenantProductModel.Tenant, customerPerson, environment);

            // Create performing user
            UserAuthenticationData userAuthData = new UserAuthenticationData(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                UserType.Client,
                customerPerson.UserId.Value,
                default);

            using (var stack = this.stackFactory())
            {
                var quote = QuoteAggregate.CreateNewBusinessQuote(
                   tenantId,
                   organisationId,
                   productId,
                   environment,
                   QuoteExpirySettings.Default,
                   this.performingUserId,
                   stack.Clock.Now(),
                   Guid.NewGuid(),
                   Timezones.AET);
                quote.Aggregate.RecordAssociationWithCustomer(
                    customerAggregate, new PersonalDetails(customerPerson), this.performingUserId, stack.Clock.Now());
                quote.AssignQuoteNumber(
                    QuoteNumber,
                    this.performingUserId,
                    stack.Clock.Now());
                await stack.QuoteAggregateRepository.Save(quote.Aggregate);

                stack.Clock.Increment(Duration.FromDays(31));

                // Update quote expiry to a lesser date
                await this.ProductUpdateExpiryDates(
                    tenantId,
                    productId,
                    quoteExpirySettings2,
                    ProductQuoteExpirySettingUpdateType.UpdateAllQuotes);
            }

            var filters = new QuoteReadModelFilters
            {
                Statuses = new List<string> { StandardQuoteStates.Expired },
            };

            using (var stack = this.stackFactory())
            {
                stack.Clock.Increment(Duration.FromDays(31));

                // Act
                var storedValue = stack.QuoteService.GetQuotes(tenant.Id, filters)
                    .FirstOrDefault();
                var expiryDate = Instant.Add(storedValue.CreatedTimestamp, Duration.FromDays(20));

                // Assert
                expiryDate.ToRfc5322DateStringInAet().Should().Be(
                    storedValue.ExpiryTimestamp.Value.ToRfc5322DateStringInAet());
                StandardQuoteStates.Expired.Should().Be(storedValue.QuoteState);
            }
        }

        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public async Task UpdateExpiryDates_StatusIsIncomplete_WithLowerExpiryDateUpdateAndQuoteNotYetExpired()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var environment = DeploymentEnvironment.Development;
            var quoteExpirySettings = new QuoteExpirySettings(30, true);
            var quoteExpirySettings2 = new QuoteExpirySettings(20, true);
            var tenantProductModel = await this.SetupTenantAndProduct(tenantId, productId);
            await this.ProductUpdateExpiryDates(
                tenantId,
                productId,
                quoteExpirySettings,
                ProductQuoteExpirySettingUpdateType.UpdateNone);

            // Create customer
            var tenant = tenantProductModel.Tenant;
            var customerPerson = this.GetPersonAggregate(tenant, default);
            CustomerAggregate customerAggregate = await this.CreateCustomer(
                tenantProductModel.Tenant, customerPerson, environment);

            // Create performing user
            UserAuthenticationData userAuthData = new UserAuthenticationData(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                UserType.Client,
                customerPerson.UserId.Value,
                default);
            using (var stack = this.stackFactory())
            {
                var quote = QuoteAggregate.CreateNewBusinessQuote(
                   tenantId,
                   organisationId,
                   productId,
                   environment,
                   QuoteExpirySettings.Default,
                   this.performingUserId,
                   stack.Clock.Now(),
                   Guid.NewGuid(),
                   Timezones.AET);
                quote.Aggregate.RecordAssociationWithCustomer(
                    customerAggregate, new PersonalDetails(customerPerson), this.performingUserId, stack.Clock.Now());
                quote.AssignQuoteNumber(
                    QuoteNumber,
                    this.performingUserId,
                    stack.Clock.Now());
                await stack.QuoteAggregateRepository.Save(quote.Aggregate);

                stack.Clock.Increment(Duration.FromDays(19));

                // Update quote expiry to a lesser date
                await this.ProductUpdateExpiryDates(
                    tenantId,
                    productId,
                    quoteExpirySettings2,
                    ProductQuoteExpirySettingUpdateType.UpdateAllQuotes);
            }

            var filters = new QuoteReadModelFilters
            {
                Statuses = new List<string> { StandardQuoteStates.Incomplete },
            };

            using (var stack = this.stackFactory())
            {
                // Act
                var storedValue = stack.QuoteService.GetQuotes(tenant.Id, filters)
                    .FirstOrDefault();
                var expiryDate = Instant.Add(storedValue.CreatedTimestamp, Duration.FromDays(20));

                // Assert
                expiryDate.ToRfc5322DateStringInAet().Should().Be(
                    storedValue.ExpiryTimestamp.Value.ToRfc5322DateStringInAet());
                StandardQuoteStates.Incomplete.Should().Be(storedValue.QuoteState);
            }
        }

        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public async Task UpdateExpiryDates_WillUpdate_UpdateAllWithoutExpiryOnly()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var environment = DeploymentEnvironment.Development;
            var quoteExpirySettings = new QuoteExpirySettings(30);
            var quoteExpirySettings2 = new QuoteExpirySettings(20, true);
            var tenantProductModel = await this.SetupTenantAndProduct(tenantId, productId);
            await this.ProductUpdateExpiryDates(
                tenantId,
                productId,
                quoteExpirySettings,
                ProductQuoteExpirySettingUpdateType.UpdateNone);

            // Create customer
            var tenant = tenantProductModel.Tenant;
            var customerPerson = this.GetPersonAggregate(tenant, default);
            CustomerAggregate customerAggregate = await this.CreateCustomer(
                tenantProductModel.Tenant, customerPerson, environment);

            // Create performing user
            UserAuthenticationData userAuthData = new UserAuthenticationData(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                UserType.Client,
                customerPerson.UserId.Value,
                default);

            using (var stack = this.stackFactory())
            {
                var quote = QuoteAggregate.CreateNewBusinessQuote(
                    tenantId,
                    organisationId,
                    productId,
                    environment,
                    QuoteExpirySettings.Default,
                    this.performingUserId,
                    stack.Clock.Now(),
                    Guid.NewGuid(),
                    Timezones.AET);
                quote.Aggregate.RecordAssociationWithCustomer(
                    customerAggregate, new PersonalDetails(customerPerson), this.performingUserId, stack.Clock.Now());

                // associate
                quote.AssignQuoteNumber(
                    QuoteNumber,
                    this.performingUserId,
                    stack.Clock.Now());
                await stack.QuoteAggregateRepository.Save(quote.Aggregate);

                // Update quote expiry to a lesser date
                await this.ProductUpdateExpiryDates(
                    tenantId,
                    productId,
                    quoteExpirySettings2,
                    ProductQuoteExpirySettingUpdateType.UpdateAllWithoutExpiryOnly);
            }

            var filters = new QuoteReadModelFilters
            {
                Statuses = new List<string> { StandardQuoteStates.Incomplete },
            };

            using (var stack = this.stackFactory())
            {
                // Act
                var storedValue = stack.QuoteService.GetQuotes(tenant.Id, filters)
                    .FirstOrDefault();
                var expiryDate = Instant.Add(storedValue.CreatedTimestamp, Duration.FromDays(20));

                // Assert
                expiryDate.ToRfc5322DateStringInAet().Should().Be(
                    storedValue.ExpiryTimestamp.Value.ToRfc5322DateStringInAet());
                StandardQuoteStates.Incomplete.Should().Be(storedValue.QuoteState);
            }
        }

        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public async Task UpdateExpiryDates_WillNotUpdateQuotesWithExpiry_UpdateAllWithoutExpiryOnly()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var environment = DeploymentEnvironment.Development;
            var quoteExpirySettings = new QuoteExpirySettings(30, true);
            var quoteExpirySettings2 = new QuoteExpirySettings(20, true);
            var tenantProductModel = await this.SetupTenantAndProduct(tenantId, productId);
            await this.ProductUpdateExpiryDates(
                tenantId,
                productId,
                quoteExpirySettings,
                ProductQuoteExpirySettingUpdateType.UpdateNone);

            // Create customer
            var tenant = tenantProductModel.Tenant;
            var customerPerson = this.GetPersonAggregate(tenant, default);
            CustomerAggregate customerAggregate = await this.CreateCustomer(
                tenantProductModel.Tenant, customerPerson, environment);

            // Create performing user
            UserAuthenticationData userAuthData = new UserAuthenticationData(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                UserType.Client,
                customerPerson.UserId.Value,
                default);
            using (var stack = this.stackFactory())
            {
                var now = stack.Clock.Now();
                var quote = QuoteAggregate.CreateNewBusinessQuote(
                    tenantId,
                    organisationId,
                    productId,
                    environment,
                    QuoteExpirySettings.Default,
                    this.performingUserId,
                    now,
                    Guid.NewGuid(),
                    Timezones.AET);
                quote.Aggregate.SetQuoteExpiryFromSettings(quote.Id, this.performingUserId, now, quoteExpirySettings);
                quote.Aggregate.RecordAssociationWithCustomer(
                    customerAggregate, new PersonalDetails(customerPerson), this.performingUserId, now);

                // Associate
                quote.AssignQuoteNumber(QuoteNumber, this.performingUserId, now);
                await stack.QuoteAggregateRepository.Save(quote.Aggregate);
            }

            // Update quote expiry to a lesser date
            await this.ProductUpdateExpiryDates(
                tenantId,
                productId,
                quoteExpirySettings2,
                ProductQuoteExpirySettingUpdateType.UpdateAllWithoutExpiryOnly);

            var filters = new QuoteReadModelFilters
            {
                Statuses = new List<string> { StandardQuoteStates.Incomplete },
            };

            using (var stack = this.stackFactory())
            {
                // Act
                var storedValue = stack.QuoteService.GetQuotes(tenant.Id, filters)
                    .FirstOrDefault();
                var expiryDate = Instant.Add(
                    storedValue.CreatedTimestamp, Duration.FromDays(20)).Plus(Duration.FromDays(1));

                // Assert
                expiryDate.Should().NotBe(storedValue.ExpiryTimestamp.Value);
                StandardQuoteStates.Incomplete.Should().Be(storedValue.QuoteState);
            }
        }

        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public async Task UpdateExpiryDates_WillUpdate_UpdateAllQuotes()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var environment = DeploymentEnvironment.Development;
            var quoteExpirySettings = new QuoteExpirySettings(30);
            var quoteExpirySettings2 = new QuoteExpirySettings(20, true);
            var tenantProductModel = await this.SetupTenantAndProduct(tenantId, productId);
            await this.ProductUpdateExpiryDates(
                tenantId,
                productId,
                quoteExpirySettings,
                ProductQuoteExpirySettingUpdateType.UpdateNone);

            // Create customer
            var tenant = tenantProductModel.Tenant;
            var customerPerson = this.GetPersonAggregate(tenant, default);
            CustomerAggregate customerAggregate = await this.CreateCustomer(
                tenantProductModel.Tenant, customerPerson, environment);

            // Create performing user
            UserAuthenticationData userAuthData = new UserAuthenticationData(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                UserType.Client,
                customerPerson.UserId.Value,
                default);
            using (var stack = this.stackFactory())
            {
                var quote = QuoteAggregate.CreateNewBusinessQuote(
                    tenantId,
                    organisationId,
                    productId,
                    environment,
                    QuoteExpirySettings.Default,
                    this.performingUserId,
                    stack.Clock.Now(),
                    Guid.NewGuid(),
                    Timezones.AET);
                quote.Aggregate.SetQuoteExpiryFromSettings(
                    quote.Id, this.performingUserId, stack.Clock.Now(), quoteExpirySettings);
                customerPerson = stack.PersonAggregateRepository.GetById(tenant.Id, customerPerson.Id);
                quote.Aggregate.RecordAssociationWithCustomer(
                    customerAggregate, new PersonalDetails(customerPerson), this.performingUserId, stack.Clock.Now());

                // Associate
                quote.AssignQuoteNumber(
                    QuoteNumber,
                    this.performingUserId,
                    stack.Clock.Now());
                await stack.QuoteAggregateRepository.Save(quote.Aggregate);

                // Update quote expiry to a lesser date
                await this.ProductUpdateExpiryDates(
                    tenantId,
                    productId,
                    quoteExpirySettings2,
                    ProductQuoteExpirySettingUpdateType.UpdateAllQuotes);
            }

            using (var stack = this.stackFactory())
            {
                var filters = new QuoteReadModelFilters
                {
                    Statuses = new List<string> { StandardQuoteStates.Incomplete },
                };

                // Act
                var storedValue = stack.QuoteService.GetQuotes(tenant.Id, filters)
                    .FirstOrDefault();
                var expiryDate = Instant.Add(storedValue.CreatedTimestamp, Duration.FromDays(20));

                // Assert
                expiryDate.ToRfc5322DateStringInAet().Should().Be(
                    storedValue.ExpiryTimestamp.Value.ToRfc5322DateStringInAet());
                StandardQuoteStates.Incomplete.Should().Be(storedValue.QuoteState);
            }
        }

        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public async Task UpdateExpiryDates_WillNotUpdateQuoteWithExpiryExplicitySet_UpdateAllExceptExplicitSet()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var environment = DeploymentEnvironment.Development;
            var quoteExpirySettings = new QuoteExpirySettings(30, true);
            var quoteExpirySettings2 = new QuoteExpirySettings(20, true);
            var tenantProductModel = await this.SetupTenantAndProduct(tenantId, productId);
            await this.ProductUpdateExpiryDates(
                tenantId,
                productId,
                quoteExpirySettings,
                ProductQuoteExpirySettingUpdateType.UpdateNone);

            // Create customer
            var tenant = tenantProductModel.Tenant;
            var customerPerson = this.GetPersonAggregate(tenant, default);
            CustomerAggregate customerAggregate = await this.CreateCustomer(
                tenantProductModel.Tenant, customerPerson, environment);

            // Create performing user
            UserAuthenticationData userAuthData = new UserAuthenticationData(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                UserType.Client,
                customerPerson.UserId.Value,
                default);

            using (var stack = this.stackFactory())
            {
                var quote = QuoteAggregate.CreateNewBusinessQuote(
                    tenantId,
                    organisationId,
                    productId,
                    environment,
                    QuoteExpirySettings.Default,
                    this.performingUserId,
                    stack.Clock.Now(),
                    Guid.NewGuid(),
                    Timezones.AET);

                DateTime date = new DateTime(2030, 1, 1);
                quote.Aggregate.SetExpiryDate(quote.Id, Instant.FromDateTimeUtc(date.ToUniversalTime()), this.performingUserId, stack.Clock.Now(), quoteExpirySettings);
                quote.Aggregate.RecordAssociationWithCustomer(customerAggregate, new PersonalDetails(customerPerson), this.performingUserId, stack.Clock.Now());

                // Associate
                quote.AssignQuoteNumber(QuoteNumber, this.performingUserId, stack.Clock.Now());
                await stack.QuoteAggregateRepository.Save(quote.Aggregate);

                // update quote expiry to a lesser date.
                await this.ProductUpdateExpiryDates(
                    tenantId,
                    productId,
                    quoteExpirySettings2,
                    ProductQuoteExpirySettingUpdateType.UpdateAllExceptExplicitSet);
            }

            var filters = new QuoteReadModelFilters
            {
                Statuses = new List<string> { StandardQuoteStates.Incomplete },
            };

            using (var stack = this.stackFactory())
            {
                // Act
                var storedValue = stack.QuoteService.GetQuotes(tenant.Id, filters)
                    .FirstOrDefault();
                var expiryDate = Instant.Add(
                    storedValue.CreatedTimestamp, Duration.FromDays(20)).Plus(Duration.FromDays(1));

                // Assert
                expiryDate.Should().NotBe(storedValue.ExpiryTimestamp.Value);
                StandardQuoteStates.Incomplete.Should().Be(storedValue.QuoteState);
            }
        }

        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public async Task UpdateExpiryDates_WillUpdate_UpdateAllExceptExplicitSet()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var environment = DeploymentEnvironment.Development;
            var quoteExpirySettings = new QuoteExpirySettings(30, true);
            var quoteExpirySettings2 = new QuoteExpirySettings(20, true);
            var tenantProductModel = await this.SetupTenantAndProduct(tenantId, productId);
            await this.ProductUpdateExpiryDates(
                tenantId,
                productId,
                quoteExpirySettings,
                ProductQuoteExpirySettingUpdateType.UpdateNone);

            // Create customer
            var tenant = tenantProductModel.Tenant;
            var customerPerson = this.GetPersonAggregate(tenant, default);
            CustomerAggregate customerAggregate = await this.CreateCustomer(
                tenantProductModel.Tenant, customerPerson, environment);

            // Create performing user
            UserAuthenticationData userAuthData = new UserAuthenticationData(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                UserType.Client,
                customerPerson.UserId.Value,
                default);
            using (var stack = this.stackFactory())
            {
                var quote = QuoteAggregate.CreateNewBusinessQuote(
                    tenantId,
                    organisationId,
                    productId,
                    environment,
                    QuoteExpirySettings.Default,
                    this.performingUserId,
                    stack.Clock.Now(),
                    Guid.NewGuid(),
                    Timezones.AET);
                var quoteAggregate = quote.Aggregate;

                quoteAggregate.SetQuoteExpiryFromSettings(
                    quoteAggregate.Id, this.performingUserId, stack.Clock.Now(), quoteExpirySettings);
                quoteAggregate.RecordAssociationWithCustomer(
                    customerAggregate, new PersonalDetails(customerPerson), this.performingUserId, stack.Clock.Now());

                // Associate
                quote.AssignQuoteNumber(QuoteNumber, this.performingUserId, stack.Clock.Now());
                await stack.QuoteAggregateRepository.Save(quoteAggregate);

                // Update quote expiry to a lesser date
                await this.ProductUpdateExpiryDates(
                    tenantId,
                    productId,
                    quoteExpirySettings2,
                    ProductQuoteExpirySettingUpdateType.UpdateAllExceptExplicitSet);
            }

            var filters = new QuoteReadModelFilters
            {
                Statuses = new List<string> { StandardQuoteStates.Incomplete },
            };

            using (var stack = this.stackFactory())
            {
                // Act
                var storedValue = stack.QuoteService.GetQuotes(tenant.Id, filters)
                    .FirstOrDefault();
                var expiryDate = Instant.Add(storedValue.CreatedTimestamp, Duration.FromDays(20));

                // Assert
                expiryDate.ToRfc5322DateStringInAet().Should().Be(
                    storedValue.ExpiryTimestamp.Value.ToRfc5322DateStringInAet());
                StandardQuoteStates.Incomplete.Should().Be(storedValue.QuoteState);
            }
        }

        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public async Task UpdateExpiryDates_DoesntUpdateGroupOfRecords_IfUpdateNone()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var environment = DeploymentEnvironment.Development;
            var quoteExpirySettings = new QuoteExpirySettings(30, true);
            var quoteExpirySettings2 = new QuoteExpirySettings(20, true);
            var tenantProductModel = await this.SetupTenantAndProduct(tenantId, productId);

            // Create customer
            var tenant = tenantProductModel.Tenant;
            var customerPerson = this.GetPersonAggregate(tenant, default);
            CustomerAggregate customerAggregate = await this.CreateCustomer(
                tenantProductModel.Tenant, customerPerson, environment);

            // Create performing user
            UserAuthenticationData userAuthData = new UserAuthenticationData(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                UserType.Client,
                customerPerson.UserId.Value,
                default);

            var sampleQuoteAggregates = await this.CreateExpirySampleQuoteAggregates(
                tenantId,
                organisationId,
                productId,
                environment,
                customerAggregate,
                customerPerson,
                quoteExpirySettings);

            // Act
            await this.ProductUpdateExpiryDates(
                tenantId,
                productId,
                quoteExpirySettings2,
                ProductQuoteExpirySettingUpdateType.UpdateNone);

            var filters = new QuoteReadModelFilters { Statuses = new List<string> { StandardQuoteStates.Incomplete } };

            // Assert
            using (var stack = this.stackFactory())
            {
                var quoteAggregateWithoutExpiry = stack.QuoteService.GetQuoteDetails(
                    tenant.Id,
                    sampleQuoteAggregates.QuoteAggregateWithoutExpiry.Id);
                quoteAggregateWithoutExpiry.ExpiryTimestamp.Should().BeNull();
                quoteAggregateWithoutExpiry.QuoteState.Should().Be("Incomplete");

                var quoteAggregateWithExpiryInSettings = stack.QuoteService.GetQuoteDetails(
                    tenant.Id,
                    sampleQuoteAggregates.QuoteAggregateWithExpiryInSettings.Id);
                var expiryDate = Instant.Add(
                    quoteAggregateWithExpiryInSettings.CreatedTimestamp,
                    Duration.FromDays(quoteExpirySettings.ExpiryDays));
                expiryDate.ToIso8601DateInAet().Should().Be(
                    quoteAggregateWithExpiryInSettings.ExpiryTimestamp.Value.ToIso8601DateInAet());
                quoteAggregateWithExpiryInSettings.QuoteState.Should().Be("Incomplete");

                var quoteAggregateWillExpireIn2Days = stack.QuoteService.GetQuoteDetails(
                    tenant.Id,
                    sampleQuoteAggregates.QuoteAggregateWillExpireIn2Days.Id);
                expiryDate = Instant.Add(quoteAggregateWillExpireIn2Days.CreatedTimestamp, Duration.FromDays(2));
                expiryDate.ToIso8601DateInAet().Should().Be(
                    quoteAggregateWillExpireIn2Days.ExpiryTimestamp.Value.ToIso8601DateInAet());
                quoteAggregateWillExpireIn2Days.QuoteState.Should().Be("Incomplete");

                var quoteAggregateExpired2DaysAgo = stack.QuoteService.GetQuoteDetails(
                    tenant.Id,
                    sampleQuoteAggregates.QuoteAggregateExpired2DaysAgo.Id);
                expiryDate = Instant.Add(quoteAggregateExpired2DaysAgo.CreatedTimestamp, Duration.FromDays(-2));
                expiryDate.ToIso8601DateInAet().Should().Be(
                    quoteAggregateExpired2DaysAgo.ExpiryTimestamp.Value.ToIso8601DateInAet());
                quoteAggregateExpired2DaysAgo.QuoteState.Should().Be("Expired");
            }
        }

        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public async Task UpdateExpiryDates_SuccessfullyUpdatesAllQuotes_IfUpdateAllQuotes()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var environment = DeploymentEnvironment.Development;
            var quoteExpirySettings = new QuoteExpirySettings(30, true);
            var quoteExpirySettings2 = new QuoteExpirySettings(20, true);
            var tenantProductModel = await this.SetupTenantAndProduct(tenantId, productId);

            // Create customer
            var tenant = tenantProductModel.Tenant;
            var customerPerson = this.GetPersonAggregate(tenant, default);
            CustomerAggregate customerAggregate = await this.CreateCustomer(
                tenantProductModel.Tenant, customerPerson, environment);

            // Create performing user
            UserAuthenticationData userAuthData = new UserAuthenticationData(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                UserType.Client,
                customerPerson.UserId.Value,
                default);

            var sampleQuoteAggregates = await this.CreateExpirySampleQuoteAggregates(
                tenantId,
                organisationId,
                productId,
                environment,
                customerAggregate,
                customerPerson,
                quoteExpirySettings);

            // Act
            await this.ProductUpdateExpiryDates(
                tenantId,
                productId,
                quoteExpirySettings2,
                ProductQuoteExpirySettingUpdateType.UpdateAllQuotes);

            var filters = new QuoteReadModelFilters { Statuses = new List<string> { StandardQuoteStates.Incomplete } };

            // Assert
            using (var stack = this.stackFactory())
            {
                var quoteAggregateWithoutExpiry = stack.QuoteService.GetQuoteDetails(
                    tenant.Id,
                    sampleQuoteAggregates.QuoteAggregateWithoutExpiry.Id);
                var expiryDate = Instant.Add(
                    quoteAggregateWithoutExpiry.CreatedTimestamp, Duration.FromDays(quoteExpirySettings2.ExpiryDays));
                expiryDate.ToIso8601DateInAet().Should().Be(
                    quoteAggregateWithoutExpiry.ExpiryTimestamp.Value.ToIso8601DateInAet());
                quoteAggregateWithoutExpiry.QuoteState.Should().Be("Incomplete");

                var quoteAggregateWithExpiryInSettings = stack.QuoteService.GetQuoteDetails(
                    tenant.Id,
                    sampleQuoteAggregates.QuoteAggregateWithExpiryInSettings.Id);
                expiryDate = Instant.Add(
                    quoteAggregateWithExpiryInSettings.CreatedTimestamp,
                    Duration.FromDays(quoteExpirySettings2.ExpiryDays));
                expiryDate.ToIso8601DateInAet().Should().Be(
                    quoteAggregateWithExpiryInSettings.ExpiryTimestamp.Value.ToIso8601DateInAet());
                quoteAggregateWithExpiryInSettings.QuoteState.Should().Be("Incomplete");

                var quoteAggregateWillExpireIn2Days = stack.QuoteService.GetQuoteDetails(
                    tenant.Id,
                    sampleQuoteAggregates.QuoteAggregateWillExpireIn2Days.Id);
                expiryDate = Instant.Add(
                    quoteAggregateWillExpireIn2Days.CreatedTimestamp,
                    Duration.FromDays(quoteExpirySettings2.ExpiryDays));
                expiryDate.ToIso8601DateInAet().Should().Be(
                    quoteAggregateWillExpireIn2Days.ExpiryTimestamp.Value.ToIso8601DateInAet());
                quoteAggregateWillExpireIn2Days.QuoteState.Should().Be("Incomplete");

                var quoteAggregateExpired2DaysAgo = stack.QuoteService.GetQuoteDetails(
                    tenant.Id,
                    sampleQuoteAggregates.QuoteAggregateExpired2DaysAgo.Id);
                expiryDate = Instant.Add(
                    quoteAggregateExpired2DaysAgo.CreatedTimestamp,
                    Duration.FromDays(quoteExpirySettings2.ExpiryDays));
                expiryDate.ToIso8601DateInAet().Should().Be(
                    quoteAggregateExpired2DaysAgo.ExpiryTimestamp.Value.ToIso8601DateInAet());
                quoteAggregateExpired2DaysAgo.QuoteState.Should().Be("Incomplete");
            }
        }

        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public async Task UpdateExpiryDates_SuccessfullyUpdatesSpecificQuotesThatWhereAutomaticallySet_IfUpdateAllExceptExplicitSet()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var environment = DeploymentEnvironment.Development;
            var quoteExpirySettings = new QuoteExpirySettings(30, true);
            var quoteExpirySettings2 = new QuoteExpirySettings(20, true);
            var tenantProductModel = await this.SetupTenantAndProduct(tenantId, productId);

            // Create customer
            var tenant = tenantProductModel.Tenant;
            var customerPerson = this.GetPersonAggregate(tenant, default);
            CustomerAggregate customerAggregate = await this.CreateCustomer(
                tenantProductModel.Tenant, customerPerson, environment);

            // Create performing user
            UserAuthenticationData userAuthData = new UserAuthenticationData(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                UserType.Client,
                customerPerson.UserId.Value,
                default);

            var sampleQuoteAggregates = await this.CreateExpirySampleQuoteAggregates(
                tenantId,
                organisationId,
                productId,
                environment,
                customerAggregate,
                customerPerson,
                quoteExpirySettings);

            // Act
            await this.ProductUpdateExpiryDates(
                tenantId,
                productId,
                quoteExpirySettings2,
                ProductQuoteExpirySettingUpdateType.UpdateAllExceptExplicitSet);

            var filters = new QuoteReadModelFilters { Statuses = new List<string> { StandardQuoteStates.Incomplete } };

            // Assert
            using (var stack = this.stackFactory())
            {
                var quoteAggregateWithoutExpiry = stack.QuoteService.GetQuoteDetails(
                    tenant.Id,
                    sampleQuoteAggregates.QuoteAggregateWithoutExpiry.Id);
                var expiryDate = Instant.Add(
                    quoteAggregateWithoutExpiry.CreatedTimestamp, Duration.FromDays(quoteExpirySettings2.ExpiryDays));
                expiryDate.ToIso8601DateInAet().Should().Be(
                    quoteAggregateWithoutExpiry.ExpiryTimestamp.Value.ToIso8601DateInAet());
                quoteAggregateWithoutExpiry.QuoteState.Should().Be("Incomplete");

                var quoteAggregateWithExpiryInSettings = stack.QuoteService.GetQuoteDetails(
                    tenant.Id,
                    sampleQuoteAggregates.QuoteAggregateWithExpiryInSettings.Id);
                expiryDate = Instant.Add(
                    quoteAggregateWithExpiryInSettings.CreatedTimestamp,
                    Duration.FromDays(quoteExpirySettings2.ExpiryDays));
                expiryDate.ToIso8601DateInAet().Should().Be(
                    quoteAggregateWithExpiryInSettings.ExpiryTimestamp.Value.ToIso8601DateInAet());
                quoteAggregateWithExpiryInSettings.QuoteState.Should().Be("Incomplete");

                var quoteAggregateWillExpireIn2Days = stack.QuoteService.GetQuoteDetails(
                    tenant.Id,
                    sampleQuoteAggregates.QuoteAggregateWillExpireIn2Days.Id);
                expiryDate = Instant.Add(
                    quoteAggregateWillExpireIn2Days.CreatedTimestamp,
                    Duration.FromDays(quoteExpirySettings2.ExpiryDays));
                expiryDate.ToIso8601DateInAet().Should().NotBe(
                    quoteAggregateWillExpireIn2Days.ExpiryTimestamp.Value.ToIso8601DateInAet());
                quoteAggregateWillExpireIn2Days.QuoteState.Should().Be("Incomplete");

                var quoteAggregateExpired2DaysAgo = stack.QuoteService.GetQuoteDetails(
                    tenant.Id,
                    sampleQuoteAggregates.QuoteAggregateExpired2DaysAgo.Id);
                expiryDate = Instant.Add(
                    quoteAggregateExpired2DaysAgo.CreatedTimestamp,
                    Duration.FromDays(quoteExpirySettings2.ExpiryDays));
                expiryDate.ToIso8601DateInAet().Should().NotBe(
                    quoteAggregateExpired2DaysAgo.ExpiryTimestamp.Value.ToIso8601DateInAet());
                quoteAggregateExpired2DaysAgo.QuoteState.Should().Be("Expired");
            }
        }

        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public async Task UpdateExpiryDates_SuccessfullyUpdatesAllWithoutQuotes_IfUpdateAllWithoutExpiryOnly()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var environment = DeploymentEnvironment.Development;
            var quoteExpirySettings = new QuoteExpirySettings(30, true);
            var quoteExpirySettings2 = new QuoteExpirySettings(20, true);
            var tenantProductModel = await this.SetupTenantAndProduct(tenantId, productId);

            // Create customer
            var tenant = tenantProductModel.Tenant;
            var customerPerson = this.GetPersonAggregate(tenant, default);
            CustomerAggregate customerAggregate = await this.CreateCustomer(
                tenantProductModel.Tenant, customerPerson, environment);

            // Create performing user
            UserAuthenticationData userAuthData = new UserAuthenticationData(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                UserType.Client,
                customerPerson.UserId.Value,
                default);

            var sampleQuoteAggregates = await this.CreateExpirySampleQuoteAggregates(
                tenantId,
                organisationId,
                productId,
                environment,
                customerAggregate,
                customerPerson,
                quoteExpirySettings);

            // Act
            await this.ProductUpdateExpiryDates(
                tenantId,
                productId,
                quoteExpirySettings2,
                ProductQuoteExpirySettingUpdateType.UpdateAllWithoutExpiryOnly);

            var filters = new QuoteReadModelFilters { Statuses = new List<string> { StandardQuoteStates.Incomplete } };

            // Assert
            using (var stack = this.stackFactory())
            {
                var quoteAggregateWithoutExpiry = stack.QuoteService.GetQuoteDetails(
                    tenant.Id,
                    sampleQuoteAggregates.QuoteAggregateWithoutExpiry.Id);
                var expiryDate = Instant.Add(
                    quoteAggregateWithoutExpiry.CreatedTimestamp, Duration.FromDays(quoteExpirySettings2.ExpiryDays));
                quoteAggregateWithoutExpiry.ExpiryTimestamp.Should().NotBeNull();
                expiryDate.ToIso8601DateInAet().Should().Be(
                    quoteAggregateWithoutExpiry.ExpiryTimestamp.Value.ToIso8601DateInAet());
                quoteAggregateWithoutExpiry.QuoteState.Should().Be("Incomplete");

                var quoteAggregateWithExpiryInSettings = stack.QuoteService.GetQuoteDetails(
                    tenant.Id,
                    sampleQuoteAggregates.QuoteAggregateWithExpiryInSettings.Id);
                expiryDate = Instant.Add(
                    quoteAggregateWithExpiryInSettings.CreatedTimestamp,
                    Duration.FromDays(quoteExpirySettings.ExpiryDays));
                expiryDate.ToIso8601DateInAet().Should().Be(
                    quoteAggregateWithExpiryInSettings.ExpiryTimestamp.Value.ToIso8601DateInAet());
                quoteAggregateWithExpiryInSettings.QuoteState.Should().Be("Incomplete");

                var quoteAggregateWillExpireIn2Days = stack.QuoteService.GetQuoteDetails(
                    tenant.Id,
                    sampleQuoteAggregates.QuoteAggregateWillExpireIn2Days.Id);
                expiryDate = Instant.Add(quoteAggregateWillExpireIn2Days.CreatedTimestamp, Duration.FromDays(2));
                expiryDate.ToIso8601DateInAet().Should().Be(
                    quoteAggregateWillExpireIn2Days.ExpiryTimestamp.Value.ToIso8601DateInAet());
                quoteAggregateWillExpireIn2Days.QuoteState.Should().Be("Incomplete");

                var quoteAggregateExpired2DaysAgo = stack.QuoteService.GetQuoteDetails(
                    tenant.Id,
                    sampleQuoteAggregates.QuoteAggregateExpired2DaysAgo.Id);
                expiryDate = Instant.Add(quoteAggregateExpired2DaysAgo.CreatedTimestamp, Duration.FromDays(-2));
                expiryDate.ToIso8601DateInAet().Should().Be(
                    quoteAggregateExpired2DaysAgo.ExpiryTimestamp.Value.ToIso8601DateInAet());
                quoteAggregateExpired2DaysAgo.QuoteState.Should().Be("Expired");
            }
        }

        private async Task<SampleExpiryQuoteAggregates> CreateExpirySampleQuoteAggregates(
            Guid tenantId,
            Guid organisationId,
            Guid productId,
            DeploymentEnvironment environment,
            CustomerAggregate customerAggregate,
            PersonAggregate personAggregate,
            QuoteExpirySettings quoteExpirySettings)
        {
            using (var stack = this.stackFactory())
            {
                // Incomplete quote without expiry.
                var quoteWithoutExpiry = QuoteAggregate.CreateNewBusinessQuote(
                    tenantId,
                    organisationId,
                    productId,
                    environment,
                    new QuoteExpirySettings(30, false),
                    this.performingUserId,
                    stack.Clock.Now(),
                    Guid.NewGuid(),
                    Timezones.AET);
                var quoteAggregateWithoutExpiry = quoteWithoutExpiry.Aggregate;
                quoteAggregateWithoutExpiry.RecordAssociationWithCustomer(
                    customerAggregate, new PersonalDetails(personAggregate), this.performingUserId, stack.Clock.Now());
                quoteWithoutExpiry.AssignQuoteNumber(
                    this.RandomString(5),
                    this.performingUserId,
                    stack.Clock.Now());
                await stack.QuoteAggregateRepository.Save(quoteAggregateWithoutExpiry);

                await this.ProductUpdateExpiryDates(
                    tenantId, productId, quoteExpirySettings, ProductQuoteExpirySettingUpdateType.UpdateNone);

                // Incomplete quote with expiry same with settings.
                var quoteWithExpiryInSettings = QuoteAggregate.CreateNewBusinessQuote(
                    tenantId,
                    organisationId,
                    productId,
                    environment,
                    quoteExpirySettings,
                    this.performingUserId,
                    stack.Clock.Now(),
                    Guid.NewGuid(),
                    Timezones.AET);
                var quoteAggregateWithExpiryInSettings = quoteWithExpiryInSettings.Aggregate;

                quoteAggregateWithExpiryInSettings.SetQuoteExpiryFromSettings(
                    quoteWithExpiryInSettings.Id, this.performingUserId, stack.Clock.Now(), quoteExpirySettings);
                quoteAggregateWithExpiryInSettings.RecordAssociationWithCustomer(
                    customerAggregate, new PersonalDetails(personAggregate), this.performingUserId, stack.Clock.Now());
                quoteWithExpiryInSettings.AssignQuoteNumber(
                    this.RandomString(5),
                    this.performingUserId,
                    stack.Clock.Now());
                await stack.QuoteAggregateRepository.Save(quoteAggregateWithExpiryInSettings);

                // Incomplete quote will expire in 2 days.
                var expireIn2Days = stack.Clock.Now().Plus(Duration.FromDays(2));
                var quoteWillExpireIn2Days = QuoteAggregate.CreateNewBusinessQuote(
                    tenantId,
                    organisationId,
                    productId,
                    environment,
                    quoteExpirySettings,
                    this.performingUserId,
                    stack.Clock.Now(),
                    Guid.NewGuid(),
                    Timezones.AET);
                var quoteAggregateWillExpireIn2Days = quoteWillExpireIn2Days.Aggregate;
                quoteAggregateWillExpireIn2Days.SetExpiryDate(
                    quoteWillExpireIn2Days.Id,
                    expireIn2Days,
                    this.performingUserId,
                    stack.Clock.Now(),
                    quoteExpirySettings);
                quoteAggregateWillExpireIn2Days.RecordAssociationWithCustomer(
                    customerAggregate, new PersonalDetails(personAggregate), this.performingUserId, stack.Clock.Now());
                quoteWillExpireIn2Days.AssignQuoteNumber(
                    this.RandomString(5), this.performingUserId, stack.Clock.Now());
                await stack.QuoteAggregateRepository.Save(quoteAggregateWillExpireIn2Days);

                // Incomplete quote expired 2 days ago.
                var expired2DaysAgo = stack.Clock.Now().Plus(Duration.FromDays(-2));
                var quoteExpired2DaysAgo = QuoteAggregate.CreateNewBusinessQuote(
                    tenantId,
                    organisationId,
                    productId,
                    environment,
                    QuoteExpirySettings.Default,
                    this.performingUserId,
                    stack.Clock.Now(),
                    Guid.NewGuid(),
                    Timezones.AET);
                var quoteAggregateExpired2DaysAgo = quoteExpired2DaysAgo.Aggregate;
                quoteAggregateExpired2DaysAgo.SetExpiryDate(
                    quoteExpired2DaysAgo.Id,
                    expired2DaysAgo,
                    this.performingUserId,
                    stack.Clock.Now(),
                    quoteExpirySettings);
                quoteAggregateExpired2DaysAgo.RecordAssociationWithCustomer(
                    customerAggregate, new PersonalDetails(personAggregate), this.performingUserId, stack.Clock.Now());
                quoteExpired2DaysAgo.AssignQuoteNumber(
                    this.RandomString(5),
                    this.performingUserId,
                    stack.Clock.Now());
                await stack.QuoteAggregateRepository.Save(quoteAggregateExpired2DaysAgo);

                return new SampleExpiryQuoteAggregates()
                {
                    QuoteAggregateWithoutExpiry = quoteWithoutExpiry,
                    QuoteAggregateWithExpiryInSettings = quoteWithExpiryInSettings,
                    QuoteAggregateWillExpireIn2Days = quoteWillExpireIn2Days,
                    QuoteAggregateExpired2DaysAgo = quoteExpired2DaysAgo,
                };
            }
        }

        private async Task ProductUpdateExpiryDates(
            Guid tenantId,
            Guid productId,
            QuoteExpirySettings quoteExpirySettings,
            ProductQuoteExpirySettingUpdateType updateNone)
        {
            using (var stack = this.stackFactory())
            {
                // Needed to have higher created time to pass this unit test.
                stack.Clock.Increment(Duration.FromSeconds(this.updateExpiryDateDelayInSeconds++));
                stack.ProductService.UpdateQuoteExpirySettings(tenantId, productId, quoteExpirySettings, updateNone, CancellationToken.None);
                await stack.QuoteService.UpdateExpiryDates(tenantId, productId, quoteExpirySettings, updateNone, CancellationToken.None);
            }
        }

        private async Task<TenantProductModel> SetupTenantAndProduct(Guid tenantId, Guid productId)
        {
            var instant = SystemClock.Instance.GetCurrentInstant();
            var tenant = new Tenant(tenantId, "test tenant", tenantId.ToString(), null, default, default, SystemClock.Instance.GetCurrentInstant());
            var product = new Product(tenant.Id, productId, "test product", "alias", SystemClock.Instance.GetCurrentInstant());

            using (var stack = this.stackFactory())
            {
                var clientAdminRole = RoleHelper.CreateTenantAdminRole(
                    tenant.Id, tenant.Details.DefaultOrganisationId, stack.Clock.Now());
                var customerRole = RoleHelper.CreateCustomerRole(
                    tenant.Id, tenant.Details.DefaultOrganisationId, stack.Clock.Now());

                stack.RoleRepository.Insert(clientAdminRole);
                stack.RoleRepository.Insert(customerRole);
                stack.TenantRepository.Insert(tenant);
                stack.ProductRepository.Insert(product);
                stack.TenantRepository.SaveChanges();
                stack.ProductRepository.SaveChanges();
                stack.RoleRepository.SaveChanges();

                var tenantDetails = tenant.Details;
                var organisation = Organisation.CreateNewOrganisation(
                    tenant.Id, tenantDetails.Alias, tenantDetails.Name, null, this.performingUserId, stack.Clock.Now());
                await stack.OrganisationAggregateRepository.Save(organisation);

                tenant.SetDefaultOrganisation(organisation.Id, stack.Clock.Now().Plus(Duration.FromMinutes(1)));
                stack.TenantRepository.SaveChanges();
            }

            return new TenantProductModel
            {
                Product = product,
                Tenant = tenant,
            };
        }

        private PersonAggregate GetPersonAggregate(Tenant tenant, Instant timestamp)
        {
            var personCommonProperties = new PersonCommonProperties
            {
                Email = "jeo@gmail.com",
                FullName = "Test",
            };

            var personDetails = new PersonalDetails(tenant.Id, personCommonProperties);
            var customerPerson = PersonAggregate.CreatePersonFromPersonalDetails(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                personDetails,
                this.performingUserId,
                timestamp);
            return customerPerson;
        }

        private string RandomString(int length)
        {
            Random random = new Random();
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private async Task<CustomerAggregate> CreateCustomer(
            Tenant tenant, PersonAggregate person, DeploymentEnvironment deployment, ApplicationStack stack = null)
        {
            if (stack == null)
            {
                stack = this.stackFactory();
            }

            await stack.PersonAggregateRepository.Save(person);
            UserSignupModel userSignupModel = this.CreateSignupModel(tenant, deployment);
            UserAggregate user = await stack.UserService.CreateUser(userSignupModel);
            var customerAggregate = await stack.CreateCustomerForExistingPerson(person, deployment, user.Id, null);

            return customerAggregate;
        }

        private UserSignupModel CreateSignupModel(Tenant tenant, DeploymentEnvironment deploymentEnvironment)
        {
            var testEmail = "test-quoter-deleter@ubind.io";
            var testNumber = "123";

            return new UserSignupModel()
            {
                AlternativeEmail = testEmail,
                WorkPhoneNumber = testNumber,
                Email = testEmail,
                Environment = deploymentEnvironment,
                FullName = testNumber,
                HomePhoneNumber = testNumber,
                MobilePhoneNumber = testNumber,
                PreferredName = testNumber,
                UserType = UserType.Client,
                TenantId = tenant.Id,
                OrganisationId = tenant.Details.DefaultOrganisationId,
            };
        }

        /// <summary>
        /// Testing return model when setting up tenand and product.
        /// </summary>
        private class TenantProductModel
        {
            public Tenant Tenant { get; set; }

            public Product Product { get; set; }
        }

        /// <summary>
        /// Sample quote aggregates.
        /// </summary>
        private class SampleExpiryQuoteAggregates
        {
            public Quote QuoteAggregateWithoutExpiry { get; set; }

            public Quote QuoteAggregateWithExpiryInSettings { get; set; }

            public Quote QuoteAggregateWillExpireIn2Days { get; set; }

            public Quote QuoteAggregateExpired2DaysAgo { get; set; }
        }
    }
}
