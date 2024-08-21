// <copyright file="QuoteImportedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using FluentAssertions;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever;
    using UBind.Domain.Extensions;
    using UBind.Domain.Imports;
    using UBind.Domain.Json;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.ValueTypes;

    public partial class QuoteAggregate
    {
        public class QuoteImportedEvent : Event<QuoteAggregate, Guid>, IQuoteCreatedEvent
        {
            public QuoteImportedEvent(
                Guid tenantId,
                Guid aggregateId,
                Guid quoteId,
                Guid organisationId,
                Guid productId,
                DeploymentEnvironment environment,
                Guid? performingUserId,
                Instant createdTimestamp,
                DateTimeZone timeZone,
                Guid? customerId,
                IPersonalDetails personalDetails,
                QuoteImportData data,
                Guid? productReleaseId)
                : base(tenantId, aggregateId, performingUserId, createdTimestamp)
            {
                timeZone.Should().NotBeNull();

                this.QuoteId = quoteId;
                this.ProductId = productId;
                this.OrganisationId = organisationId;
                this.Environment = environment;
                this.Type = QuoteType.NewBusiness;
                this.CustomerId = customerId;
                this.CustomerDetails = new PersonalDetails(personalDetails);
                this.QuoteState = data.QuoteState.IsNullOrEmpty() ? StandardQuoteStates.Incomplete : data.QuoteState;
                this.TimeZoneId = timeZone.ToString();
                this.FormDataJson = data.FormData;
                this.QuoteNumber = data.QuoteNumber;

                var quoteDataRetriever = new StandardQuoteDataRetriever(new CachingJObjectWrapper(data.FormData), new CachingJObjectWrapper(data.CalculationResult));
                var calculationResult = CalculationResult.CreateForNewPolicy(
                    new Json.CachingJObjectWrapper(data.CalculationResult), quoteDataRetriever);
                calculationResult.FormDataId = this.QuoteId; // seeding a quote with formdata uses the id of the quote to define id of the formdata update.

                this.CalculationResultId = Guid.NewGuid();
                this.CalculationResult = calculationResult;
                this.AreTimestampsAuthoritative = false;
                this.ProductReleaseId = productReleaseId;
            }

            [JsonConstructor]
            private QuoteImportedEvent()
            {
            }

            /// <summary>
            /// Gets the ID of the quote.
            /// </summary>
            [JsonProperty]
            public Guid QuoteId { get; private set; }

            [JsonProperty]
            public Guid ProductId { get; private set; }

            [JsonProperty]
            public Guid OrganisationId { get; private set; }

            [JsonProperty]
            public DeploymentEnvironment Environment { get; private set; }

            [JsonProperty]
            public Guid? CustomerId { get; private set; }

            [JsonProperty]
            public Guid? PersonId { get; private set; }

            [JsonProperty]
            public PersonalDetails CustomerDetails { get; private set; }

            [JsonProperty]
            public QuoteType Type { get; private set; }

            [JsonProperty]
            public string FormDataJson { get; private set; }

            [JsonProperty]
            public string QuoteState { get; private set; }

            [JsonProperty]
            public string QuoteNumber { get; private set; }

            [JsonProperty]
            public string TimeZoneId { get; private set; }

            [JsonProperty]
            public bool AreTimestampsAuthoritative { get; }

            [JsonProperty]
            public Guid CalculationResultId { get; private set; }

            [JsonProperty]
            public CalculationResult CalculationResult { get; private set; }

            [JsonProperty]
            public Guid? ProductReleaseId { get; private set; }
        }
    }
}
