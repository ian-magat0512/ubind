// <copyright file="AdjustmentQuoteCreatedEventTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Aggregates.Quote.Events
{
    using System;
    using FluentAssertions;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Extensions;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class AdjustmentQuoteCreatedEventTests
    {
        private readonly Guid? performingUserId = Guid.NewGuid();

        [Fact]
        public void AdjustmentQuoteCreateEvent_RoundTripsToJsonAndBackCorrectly()
        {
            // Arrange
            var aggregateId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var quoteNumber = "MYQUOTE";
            var customerDetails = new FakePersonalDetails();
            var formDataJson = "{}";
            var parentQuoteId = Guid.NewGuid();
            var timestamp = SystemClock.Instance.Now();
            var sut = new QuoteAggregate.AdjustmentQuoteCreatedEvent(
                customerDetails.TenantId, aggregateId, organisationId, quoteNumber, formDataJson, parentQuoteId, this.performingUserId, timestamp, Guid.NewGuid());
            var serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
            };

            // Act
            var json = JsonConvert.SerializeObject(sut, serializerSettings);
            var deserializedEvent = JsonConvert.DeserializeObject<QuoteAggregate.AdjustmentQuoteCreatedEvent>(json, serializerSettings);

            // Assert
            deserializedEvent.QuoteNumber.Should().Be(sut.QuoteNumber);
        }
    }
}
