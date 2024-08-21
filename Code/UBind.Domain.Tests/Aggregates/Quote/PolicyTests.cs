// <copyright file="PolicyTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Aggregates.Quote
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Extensions;
    using UBind.Domain.Json;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class PolicyTests
    {
        private readonly IClock clock = SystemClock.Instance;
        private readonly Guid? performingUserId = Guid.NewGuid();
        private readonly IQuoteWorkflow quoteWorkflow = new DefaultQuoteWorkflow();
        private readonly IPolicyTransactionTimeOfDayScheme timeOfDayScheme = new DefaultPolicyTransactionTimeOfDayScheme();

        [Fact]
        public void Constructor_SetsPropertiesFromPolicyIssuedEvent()
        {
            // Arrange
            var formDataJson = FormDataJsonFactory.GetSampleWithStartAndEndDates();
            var calculationResultJson = CalculationResultJsonFactory.Create();
            var quoteDataRetriever = QuoteFactory.QuoteDataRetriever(new CachingJObjectWrapper(formDataJson), new CachingJObjectWrapper(calculationResultJson));

            var inceptionDateTime = new LocalDate(2000, 1, 1).At(this.timeOfDayScheme.GetInceptionTime());
            var inceptionTimestamp = inceptionDateTime.InZoneLeniently(Timezones.AET).ToInstant();
            var expiryDateTime = inceptionDateTime.PlusYears(1);
            var expiryTimestamp = expiryDateTime.InZoneLeniently(Timezones.AET).ToInstant();
            var calculationResult = Domain.ReadWriteModel.CalculationResult.CreateForNewPolicy(
                new CachingJObjectWrapper(calculationResultJson), quoteDataRetriever);
            var dataSnapshot = new QuoteDataSnapshot(
                    new QuoteDataUpdate<Domain.Aggregates.Quote.FormData>(
                        Guid.NewGuid(), new Domain.Aggregates.Quote.FormData(formDataJson), this.clock.Now()),
                    new QuoteDataUpdate<Domain.ReadWriteModel.CalculationResult>(Guid.NewGuid(), calculationResult, this.clock.Now()),
                    new QuoteDataUpdate<Domain.Aggregates.Person.IPersonalDetails>(Guid.NewGuid(), new FakePersonalDetails(), this.clock.Now()));
            var aggregate = QuoteFactory.CreateNewPolicy();
            var policyIssuedEvent = new QuoteAggregate.PolicyIssuedEvent(
                aggregate,
                Guid.NewGuid(),
                "QUOTE-1",
                "POL-1",
                calculationResult,
                inceptionDateTime,
                inceptionTimestamp,
                expiryDateTime,
                expiryTimestamp,
                Timezones.AET,
                dataSnapshot,
                this.performingUserId,
                this.clock.Now(),
                Guid.NewGuid());

            // Act
            var policy = new Domain.Aggregates.Quote.Policy(policyIssuedEvent, 0, aggregate);

            // Assert
            Assert.Equal(inceptionDateTime, policy.InceptionDateTime);
            Assert.Equal(expiryDateTime, policy.ExpiryDateTime);
        }

        [Fact]
        public void CreateRenewalQuote_InsertsPastClaimsIntoFormData()
        {
            // Arrange
            var aggregate = QuoteFactory.CreateNewPolicy();

            var claims = new List<IClaimReadModel>
            {
                this.CreateClaimReadModel("First status", 1m, new LocalDateTime(2000, 1, 1, 0, 0, 0), "First description", "CLA001"),
                this.CreateClaimReadModel("Second status", 2m, new LocalDateTime(2000, 1, 2, 0, 0, 0), "Second description", "CLA002"),
            };

            // Act
            var adjustmentQuote = aggregate.Policy.CreateRenewalQuote(
                claims,
                SystemClock.Instance.Now(),
                "QUOTE001",
                Guid.NewGuid(),
                this.quoteWorkflow,
                QuoteExpirySettings.Default,
                QuoteFactory.ProductConfiguation,
                false,
                Guid.NewGuid());

            var quote = aggregate.GetQuoteOrThrow(adjustmentQuote.Id);

            // Assert
            ////var expectedTable = "<table class='summary-table'><tr><th>Status</th><th>Total Claim Insurer</th><th>Date of Claim</th><th>Description</th><th>Claim No.</th></tr><tr><td>First status</td><td>$1.00</td><td>1 Jan 2000</td><td>First description</td><td>CLA001</td></tr><tr><td>Second status</td><td>$2.00</td><td>2 Jan 2000</td><td>Second description</td><td>CLA002</td></tr></table>";
            var expectedDataJson = @"[
      {
        ""dateOfClaim"": ""1 Jan 2000"",
        ""claimNumber"": ""CLA001"",
        ""detailsOfLoss"": ""First description"",
        ""totalClaimInsurer"": 1.0,
        ""claimStatus"": ""First status""
      },
      {
        ""dateOfClaim"": ""2 Jan 2000"",
        ""claimNumber"": ""CLA002"",
        ""detailsOfLoss"": ""Second description"",
        ""totalClaimInsurer"": 2.0,
        ""claimStatus"": ""Second status""
      }
    ]";
            var formattedExpectedData = JArray.Parse(expectedDataJson).ToString();
            var formModel = quote.LatestFormData.Data.FormModel;
            formModel["pastClaims"].Should().NotBeNull();
            formModel["pastClaims"].ToString().Should().Be(formattedExpectedData);
        }

        [Fact]
        public void CreateAdjustmentQuote_InsertsPastClaimsIntoFormData()
        {
            // Arrange
            var aggregate = QuoteFactory.CreateNewPolicy();

            var claims = new List<IClaimReadModel>
            {
                this.CreateClaimReadModel("First status", 1m, new LocalDateTime(2000, 1, 1, 0, 0, 0), "First description", "CLA001"),
                this.CreateClaimReadModel("Second status", 2m, new LocalDateTime(2000, 1, 2, 0, 0, 0), "Second description", "CLA002"),
            };

            // Act
            var adjustmentQuote = aggregate.Policy.CreateAdjustmentQuote(
                SystemClock.Instance.Now(),
                "QUOTE001",
                claims,
                Guid.NewGuid(),
                this.quoteWorkflow,
                QuoteExpirySettings.Default,
                false,
                Guid.NewGuid());
            var quote = aggregate.GetQuoteOrThrow(adjustmentQuote.Id);

            // Assert
            ////var expectedTable = "<table class='summary-table'><tr><th>Status</th><th>Total Claim Insurer</th><th>Date of Claim</th><th>Description</th><th>Claim No.</th></tr><tr><td>First status</td><td>$1.00</td><td>1 Jan 2000</td><td>First description</td><td>CLA001</td></tr><tr><td>Second status</td><td>$2.00</td><td>2 Jan 2000</td><td>Second description</td><td>CLA002</td></tr></table>";
            var expectedDataJson = @"[
      {
        ""dateOfClaim"": ""1 Jan 2000"",
        ""claimNumber"": ""CLA001"",
        ""detailsOfLoss"": ""First description"",
        ""totalClaimInsurer"": 1.0,
        ""claimStatus"": ""First status""
      },
      {
        ""dateOfClaim"": ""2 Jan 2000"",
        ""claimNumber"": ""CLA002"",
        ""detailsOfLoss"": ""Second description"",
        ""totalClaimInsurer"": 2.0,
        ""claimStatus"": ""Second status""
      }
    ]";
            var formattedExpectedData = JArray.Parse(expectedDataJson).ToString();
            var formModel = quote.LatestFormData.Data.FormModel;
            formModel["pastClaims"].Should().NotBeNull();
            formModel["pastClaims"].ToString().Should().Be(formattedExpectedData);
        }

        private IClaimReadModel CreateClaimReadModel(
            string status,
            decimal amount,
            LocalDateTime? incidentDateTime,
            string description,
            string claimNumber)
        {
            var mock = new Mock<IClaimReadModel>();
            mock.SetupGet(c => c.Status).Returns(status);
            mock.SetupGet(c => c.Amount).Returns(amount);
            mock.SetupGet(c => c.IncidentDateTime).Returns(incidentDateTime);
            mock.SetupGet(c => c.Description).Returns(description);
            mock.SetupGet(c => c.ClaimNumber).Returns(claimNumber);
            return mock.Object;
        }
    }
}
