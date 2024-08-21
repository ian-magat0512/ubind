// <copyright file="FormDataPatchedEventTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Aggregates.Quote.Events
{
    using System;
    using FluentAssertions;
    using Microsoft.AspNetCore.JsonPatch;
    using Microsoft.AspNetCore.JsonPatch.Operations;
    using Newtonsoft.Json.Serialization;
    using NodaTime;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Extensions;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class FormDataPatchedEventTests
    {
        private readonly IClock clock = SystemClock.Instance;
        private readonly Guid? performingUserId = Guid.NewGuid();

        [Fact]
        public void FormDataPatchedEvent_UpdatesContactNameSuccessfully_InQuoteAggregate()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var environment = DeploymentEnvironment.Staging;
            var timestamp = SystemClock.Instance.Now();
            var quote = QuoteAggregate.CreateNewBusinessQuote(
                TenantFactory.DefaultId,
                organisationId,
                ProductFactory.DefaultId,
                environment,
                QuoteExpirySettings.Default,
                this.performingUserId,
                timestamp,
                Guid.NewGuid(),
                Timezones.AET);
            var formData = new FormData("{ \"formModel\": { \"contactName\": \"before\" } }");
            quote.UpdateFormData(formData, this.performingUserId, this.clock.GetCurrentInstant());

            var operations = new List<Operation> { new Operation("add", "formModel/contactName", null, "after") };
            var formDataPatch = new JsonPatchDocument(operations, new DefaultContractResolver());
            var patchedEvent = new QuoteAggregate.FormDataPatchedEvent(
                quote.Aggregate.TenantId, quote.Aggregate.Id, quote.Id, formDataPatch, this.performingUserId, timestamp);

            // Act
            quote.Apply(patchedEvent, quote.EventSequenceNumber);

            // Assert
            quote.LatestFormData.Data.FormModel["contactName"].ToString().Should().Be("after");
        }

        [Fact]
        public void FormDataPatchedEvent_UpdatesMutipleContactFieldsSuccessfully_InQuoteAggregate()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var environment = DeploymentEnvironment.Staging;
            var timestamp = SystemClock.Instance.Now();
            var quote = QuoteAggregate.CreateNewBusinessQuote(
                TenantFactory.DefaultId,
                organisationId,
                ProductFactory.DefaultId,
                environment,
                QuoteExpirySettings.Default,
                this.performingUserId,
                timestamp,
                Guid.NewGuid(),
                Timezones.AET);
            var formData = new FormData("{ \"formModel\": { \"contactName\": \"beforeName\", \"contactEmail\": \"beforeEmail\" } }");
            quote.UpdateFormData(formData, this.performingUserId, this.clock.GetCurrentInstant());

            var operations = new List<Operation>
            {
                new Operation("add", "formModel/contactName", null, "afterName"),
                new Operation("add", "formModel/contactEmail", null, "afterEmail"),
            };
            var formDataPatch = new JsonPatchDocument(operations, new DefaultContractResolver());

            var patchedEvent = new QuoteAggregate.FormDataPatchedEvent(
                quote.Aggregate.TenantId, quote.Aggregate.Id, quote.Id, formDataPatch, this.performingUserId, timestamp);

            // Act
            quote.Apply(patchedEvent, quote.EventSequenceNumber);

            // Assert
            quote.LatestFormData.Data.FormModel["contactName"].ToString().Should().Be("afterName");
            quote.LatestFormData.Data.FormModel["contactEmail"].ToString().Should().Be("afterEmail");
        }

        [Fact]
        public void FormDataPatchedEvent_CreatesContactNameDatumIfDoestNotExists_InQuoteAggregate()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var environment = DeploymentEnvironment.Staging;
            var timestamp = SystemClock.Instance.Now();
            var quote = QuoteAggregate.CreateNewBusinessQuote(
                TenantFactory.DefaultId,
                organisationId,
                ProductFactory.DefaultId,
                environment,
                QuoteExpirySettings.Default,
                this.performingUserId,
                timestamp,
                Guid.NewGuid(),
                Timezones.AET);
            var formData = new FormData("{ \"formModel\": { \"contactEmail\": \"beforeEmail\" } }");
            quote.UpdateFormData(formData, this.performingUserId, this.clock.GetCurrentInstant());

            var operations = new List<Operation> { new Operation("add", "formModel/contactName", null, "new") };
            var formDataPatch = new JsonPatchDocument(operations, new DefaultContractResolver());

            var patchedEvent = new QuoteAggregate.FormDataPatchedEvent(
                TenantFactory.DefaultId, quote.Aggregate.Id, quote.Id, formDataPatch, this.performingUserId, timestamp);

            // Act
            quote.Apply(patchedEvent, quote.EventSequenceNumber);

            // Assert
            quote.LatestFormData.Data.FormModel["contactName"].ToString().Should().Be("new");
        }
    }
}
