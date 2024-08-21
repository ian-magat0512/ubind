// <copyright file="PolicyTransactionReadModelRepositoryIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.ReadModels
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.Tests.Attributes;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;
    using static UBind.Domain.Aggregates.Quote.QuoteAggregate;

    [Collection(DatabaseCollection.Name)]
    public class PolicyTransactionReadModelRepositoryIntegrationTests
    {
        private Guid tenantId;
        private Guid productId;
        private Guid organisationId;
        private Guid policyTransactionId;

        public PolicyTransactionReadModelRepositoryIntegrationTests()
        {
            this.productId = Guid.NewGuid();
            this.tenantId = Guid.NewGuid();
            this.organisationId = Guid.NewGuid();
            this.policyTransactionId = Guid.NewGuid();
        }

        [SkipDuringLeapDay]
        public async Task QueryPolicyTransactions_IncludesAllTransactionType_WhenTransactionFilterContainsAllType()
        {
            // Arrange
            var transactionFilter = new TransactionType[]
            {
                TransactionType.NewBusiness,
                TransactionType.Adjustment,
                TransactionType.Renewal,
                TransactionType.Cancellation,
            };

            await this.CreatePolicyTransactions();

            // Act
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var tenant = stack1.TenantRepository.GetTenantById(this.tenantId);
                var output = stack1.PolicyTransactionReadModelRepository.GetPolicyTransactions(
                    tenant.Id,
                    tenant.Details.DefaultOrganisationId,
                    new Guid[] { this.productId },
                    QuoteFactory.DefaultEnvironment,
                    "2000-10-10".ToLocalDateFromIso8601().GetInstantAtStartOfDayInZone(Timezones.AET),
                    "2040-10-10".ToLocalDateFromIso8601().GetInstantAtStartOfDayInZone(Timezones.AET),
                    true,
                    transactionFilter).ToList();

                // Assert
                output.Should().Contain(p => p.TransactionType == "NewBusiness");
                output.Should().Contain(p => p.TransactionType == "Adjustment");
                output.Should().Contain(p => p.TransactionType == "Renewal");
                output.Should().Contain(p => p.TransactionType == "Cancellation");
            }
        }

        [SkipDuringLeapDay]
        public async Task QueryPolicyTransactions_IncludesNewBusinessOnly_WhenTransactionFilterContainsNewBusiness()
        {
            // Arrange
            var transactionFilter = new TransactionType[]
            {
                TransactionType.NewBusiness,
            };

            await this.CreatePolicyTransactions();

            // Act
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var tenant = stack1.TenantRepository.GetTenantById(this.tenantId);
                var output = stack1.PolicyTransactionReadModelRepository.GetPolicyTransactions(
                    tenant.Id,
                    tenant.Details.DefaultOrganisationId,
                    new Guid[] { this.productId },
                    QuoteFactory.DefaultEnvironment,
                    "2000-10-10".ToLocalDateFromIso8601().GetInstantAtStartOfDayInZone(Timezones.AET),
                    "2040-10-10".ToLocalDateFromIso8601().GetInstantAtStartOfDayInZone(Timezones.AET),
                    true,
                    transactionFilter).ToList();

                // Assert
                output.Should().Contain(p => p.TransactionType == "NewBusiness");
                output.Should().NotContain(p => p.TransactionType == "Adjustment");
                output.Should().NotContain(p => p.TransactionType == "Renewal");
                output.Should().NotContain(p => p.TransactionType == "Cancellation");
            }
        }

        [SkipDuringLeapDay]
        public async Task QueryPolicyTransactions_IncludesAdjustmentOnly_WhenTransactionFilterContainsAdjustment()
        {
            // Arrange
            var transactionFilter = new TransactionType[]
            {
                TransactionType.Adjustment,
            };

            await this.CreatePolicyTransactions();

            // Act
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var tenant = stack1.TenantRepository.GetTenantById(this.tenantId);
                var output = stack1.PolicyTransactionReadModelRepository.GetPolicyTransactions(
                    tenant.Id,
                    tenant.Details.DefaultOrganisationId,
                    new Guid[] { this.productId },
                    QuoteFactory.DefaultEnvironment,
                    "2000-10-10".ToLocalDateFromIso8601().GetInstantAtStartOfDayInZone(Timezones.AET),
                    "2040-10-10".ToLocalDateFromIso8601().GetInstantAtStartOfDayInZone(Timezones.AET),
                    true,
                    transactionFilter).ToList();

                // Assert
                output.Should().NotContain(p => p.TransactionType == "NewBusiness");
                output.Should().Contain(p => p.TransactionType == "Adjustment");
                output.Should().NotContain(p => p.TransactionType == "Renewal");
                output.Should().NotContain(p => p.TransactionType == "Cancellation");
            }
        }

        [SkipDuringLeapDay]
        public async Task QueryPolicyTransactions_IncludesRenewalOnly_WhenTransactionFilterContainsRenewal()
        {
            // Arrange
            var transactionFilter = new TransactionType[]
            {
                TransactionType.Renewal,
            };

            await this.CreatePolicyTransactions();

            // Act
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var tenant = stack1.TenantRepository.GetTenantById(this.tenantId);
                var output = stack1.PolicyTransactionReadModelRepository.GetPolicyTransactions(
                    tenant.Id,
                    tenant.Details.DefaultOrganisationId,
                    new Guid[] { this.productId },
                    QuoteFactory.DefaultEnvironment,
                    "2000-10-10".ToLocalDateFromIso8601().GetInstantAtStartOfDayInZone(Timezones.AET),
                    "2040-10-10".ToLocalDateFromIso8601().GetInstantAtStartOfDayInZone(Timezones.AET),
                    true,
                    transactionFilter).ToList();

                // Assert
                output.Should().NotContain(p => p.TransactionType == "NewBusiness");
                output.Should().NotContain(p => p.TransactionType == "Adjustment");
                output.Should().Contain(p => p.TransactionType == "Renewal");
                output.Should().NotContain(p => p.TransactionType == "Cancellation");
            }
        }

        [SkipDuringLeapDay]
        public async Task QueryPolicyTransactions_IncludesCancellationOnly_WhenTransactionFilterContainsCancellation()
        {
            // Arrange
            var transactionFilter = new TransactionType[]
            {
                TransactionType.Cancellation,
            };

            await this.CreatePolicyTransactions();

            // Act
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var tenant = stack1.TenantRepository.GetTenantById(this.tenantId);
                var output = stack1.PolicyTransactionReadModelRepository.GetPolicyTransactions(
                    tenant.Id,
                    tenant.Details.DefaultOrganisationId,
                    new Guid[] { this.productId },
                    QuoteFactory.DefaultEnvironment,
                    "2000-10-10".ToLocalDateFromIso8601().GetInstantAtStartOfDayInZone(Timezones.AET),
                    "2040-10-10".ToLocalDateFromIso8601().GetInstantAtStartOfDayInZone(Timezones.AET),
                    true,
                    transactionFilter).ToList();

                // Assert
                output.Should().NotContain(p => p.TransactionType == "NewBusiness");
                output.Should().NotContain(p => p.TransactionType == "Adjustment");
                output.Should().NotContain(p => p.TransactionType == "Renewal");
                output.Should().Contain(p => p.TransactionType == "Cancellation");
            }
        }

        [SkipDuringLeapDay]
        public async Task GetPolicyTransactionWithRelatedEntities_ShouldReturnThePolicyTransaction_WhenQuoteIsListedAsRelatedEntityButTransactionHasNoQuote()
        {
            // Arrange
            await this.CreatePolicyTransactions();

            // Act
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var tenant = stack1.TenantRepository.GetTenantById(this.tenantId);
                var output = stack1.PolicyTransactionReadModelRepository.GetPolicyTransactionWithRelatedEntities(
                    tenant.Id,
                    QuoteFactory.DefaultEnvironment,
                    this.policyTransactionId,
                    new List<string> { "quote" });

                // Assert
                output.Should().NotBeNull();
            }
        }

        private async Task CreatePolicyTransactions()
        {
            // to ensure we don't trigger failures due to times of day, we'll use midday as the time to make the changes
            LocalDateTime middayTodayDateTime = SystemClock.Instance.Now().ToLocalDateInAet()
                .At(new LocalTime(12, 0));
            var middayTodayTimestamp = SystemClock.Instance.Now().ToLocalDateInAet()
                .At(new LocalTime(12, 0)).InZoneLeniently(Timezones.AET).ToInstant();
            var startOfDateTodayTimestamp = middayTodayDateTime.Date.AtStartOfDayInZone(Timezones.AET).ToInstant();

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                Tenant tenant = TenantFactory.Create(this.tenantId);
                var organisation = Organisation.CreateNewOrganisation(tenant.Id, "alias", default, null, default, SystemClock.Instance.Now());
                await stack.CreateOrganisation(organisation);
                tenant.SetDefaultOrganisation(organisation.Id, middayTodayTimestamp);
                stack.CreateTenant(tenant);
                stack.TenantRepository.SaveChanges();

                Product product = ProductFactory.Create(this.tenantId, this.productId);
                stack.CreateProduct(product);

                // create a new policy starting 12 months ago
                var twelveMonthsAgoDate = middayTodayDateTime.PlusMonths(-12).Date;
                var newBusinessAndRenewalAndAdjustmentPolicy = QuoteFactory.CreateNewPolicy(
                    formDataJson: FormDataJsonFactory.GetSampleWithStartAndEndDates(
                        inceptionDate: twelveMonthsAgoDate,
                        durationInMonths: 12),
                    calculationResultJson: CalculationResultJsonFactory.Create(startDate: twelveMonthsAgoDate),
                    tenantId: this.tenantId,
                    productId: this.productId,
                    organisationId: organisation.Id);

                // create a renewal quote for today
                var renewQuote = newBusinessAndRenewalAndAdjustmentPolicy
                    .WithRenewalQuote(middayTodayTimestamp);
                newBusinessAndRenewalAndAdjustmentPolicy = renewQuote.Aggregate;
                newBusinessAndRenewalAndAdjustmentPolicy
                    .WithCustomer()
                    .WithCustomerDetails(renewQuote.Id)
                    .WithCalculationResult(
                        renewQuote.Id,
                        FormDataJsonFactory.GetSampleWithStartAndEndDates(
                            inceptionDate: middayTodayDateTime.Date))
                    .WithPolicyRenewalTransaction(renewQuote.Id);

                // create an adjustment quote for 6 months from now
                var adjustmentQuote = newBusinessAndRenewalAndAdjustmentPolicy
                  .WithAdjustmentQuote(middayTodayTimestamp);
                newBusinessAndRenewalAndAdjustmentPolicy = adjustmentQuote.Aggregate;
                newBusinessAndRenewalAndAdjustmentPolicy
                    .WithCustomer()
                    .WithCustomerDetails(adjustmentQuote.Id)
                    .WithCalculationResult(
                        adjustmentQuote.Id,
                        FormDataJsonFactory.GetSampleWithStartEndAndEffectiveDates())
                    .WithPolicyAdjustmentTransaction(adjustmentQuote.Id, middayTodayTimestamp);
                await stack.QuoteAggregateRepository.Save(newBusinessAndRenewalAndAdjustmentPolicy);

                // create a new policy
                var newBusinessAndCancelPolicy = QuoteFactory
                    .CreateNewPolicy(
                    tenantId: this.tenantId,
                    productId: this.productId,
                    organisationId: organisation.Id);

                // create a cancellation quote
                var cancelQuote = newBusinessAndCancelPolicy.WithCancellationQuote(middayTodayTimestamp);
                newBusinessAndCancelPolicy
                    .WithCalculationResult(
                        cancelQuote.Id,
                        FormDataJsonFactory.GetSampleWithStartEndAndEffectiveAndCancellationDatesInDays())
                    .WithPolicyCancellationTransaction(cancelQuote.Id);
                await stack.QuoteAggregateRepository.Save(newBusinessAndCancelPolicy);

                // create an imported policy
                var importedPolicy = QuoteFactory
                    .CreateImportedPolicy(
                    tenantId: this.tenantId,
                    productId: this.productId,
                    organisationId: organisation.Id);
                var importEvent = importedPolicy.UnsavedEvents.FirstOrDefault() as PolicyImportedEvent;
                this.policyTransactionId = importEvent != null ? importEvent.NewBusinessTransactionId : this.policyTransactionId;
                await stack.QuoteAggregateRepository.Save(importedPolicy);
            }
        }
    }
}
