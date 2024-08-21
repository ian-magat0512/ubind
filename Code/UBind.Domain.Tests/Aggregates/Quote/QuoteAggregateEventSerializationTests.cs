// <copyright file="QuoteAggregateEventSerializationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

#pragma warning disable SA1600

namespace UBind.Domain.Tests.Aggregates.Quote
{
    using System;
    using Moq;
    using Newtonsoft.Json;
    using NodaTime;
    using NodaTime.Serialization.JsonNet;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Json;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class QuoteAggregateEventSerializationTests
    {
        private readonly Guid? performingUserId = Guid.NewGuid();
        private readonly IPolicyTransactionTimeOfDayScheme timeOfDayScheme = new DefaultPolicyTransactionTimeOfDayScheme();

        [Fact]
        public void PolicyIssuedTest_RoundtripsCorrectly_AsObject()
        {
            // Arrange
            var aggregateId = Guid.NewGuid();
            var quoteId = Guid.NewGuid();
            var quoteNumber = "ABCDEF";
            var policyNumber = "123456";
            var formDataJson = FormDataJsonFactory.GetSampleWithStartAndEndDates();
            var calculationResultJson = CalculationResultJsonFactory.Create();
            var quoteData = QuoteFactory.QuoteDataRetriever(new CachingJObjectWrapper(formDataJson), new CachingJObjectWrapper(calculationResultJson));
            var calculationResult = CalculationResult.CreateForNewPolicy(
                new CachingJObjectWrapper(calculationResultJson), quoteData);
            var inceptionDateTime = new LocalDate(2018, 1, 1).At(this.timeOfDayScheme.GetInceptionTime());
            var inceptionTimestamp = inceptionDateTime.InZoneLeniently(Timezones.AET).ToInstant();
            var effectiveDateTime = inceptionDateTime.PlusDays(2);
            var expiryDateTime = inceptionDateTime.PlusYears(1);
            var expiryTimestamp = expiryDateTime.InZoneLeniently(Timezones.AET).ToInstant();
            var quoteDataSnapshot = this.CreateSnapshot(formDataJson, calculationResult);
            var aggregate = QuoteFactory.CreateNewPolicy();
            var @event = new QuoteAggregate.PolicyIssuedEvent(
                aggregate,
                quoteId,
                quoteNumber,
                policyNumber,
                calculationResult,
                inceptionDateTime,
                inceptionTimestamp,
                expiryDateTime,
                expiryTimestamp,
                Timezones.AET,
                quoteDataSnapshot,
                this.performingUserId,
                Instant.MinValue,
                Guid.NewGuid());

            // Act
            var serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
            }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

            var json = JsonConvert.SerializeObject(@event, serializerSettings);
            var obj = JsonConvert.DeserializeObject(json, serializerSettings);

            // Assert
            var result = obj as QuoteAggregate.PolicyIssuedEvent;
            Assert.NotNull(result);
            //// Assert.Equal(aggregate.Id, result.AggregateId); // removed aggregate id. aggregate will have access to this when obtained from repository
            Assert.Equal(policyNumber, result.PolicyNumber);
            Assert.Equal(inceptionDateTime, result.InceptionDateTime);
            Assert.Equal(expiryDateTime, result.ExpiryDateTime);
            Assert.Equal(inceptionTimestamp, result.InceptionTimestamp);
            Assert.Equal(expiryTimestamp, result.ExpiryTimestamp);
            Assert.Equal(quoteDataSnapshot.FormData.Id, result.DataSnapshot.FormData.Id);
            Assert.Equal(quoteDataSnapshot.CalculationResult.Id, result.DataSnapshot.CalculationResult.Id);
            Assert.Equal(quoteDataSnapshot.CustomerDetails.Id, result.DataSnapshot.CustomerDetails.Id);
        }

        [Fact]
        public void PolicyIssuedTest_RoundtripsCorrectly_AsIEvent()
        {
            // Arrange
            var aggregateid = Guid.NewGuid();
            var quoteId = Guid.NewGuid();
            var quoteNumber = "ABCDEF";
            var policyNumber = "123456";
            var formDataJson = FormDataJsonFactory.GetSampleWithStartAndEndDates();
            var calculationResultJson = CalculationResultJsonFactory.Create();
            var quoteData = QuoteFactory.QuoteDataRetriever(new CachingJObjectWrapper(formDataJson), new CachingJObjectWrapper(calculationResultJson));
            var calculationResult = Domain.ReadWriteModel.CalculationResult.CreateForNewPolicy(
                new CachingJObjectWrapper(calculationResultJson), quoteData);
            var inceptionDateTime = new LocalDate(2018, 1, 1).At(this.timeOfDayScheme.GetInceptionTime());
            var effectiveDateTime = inceptionDateTime.PlusDays(2);
            var expiryDateTime = inceptionDateTime.PlusYears(1);
            var inceptionTimestamp = inceptionDateTime.InZoneLeniently(Timezones.AET).ToInstant();
            var expiryTimestamp = expiryDateTime.InZoneLeniently(Timezones.AET).ToInstant();
            var quoteDataSnapshot = this.CreateSnapshot(formDataJson, calculationResult);
            var aggregate = QuoteFactory.CreateNewPolicy();
            var @event = new QuoteAggregate.PolicyIssuedEvent(
                aggregate,
                quoteId,
                quoteNumber,
                policyNumber,
                calculationResult,
                inceptionDateTime,
                inceptionTimestamp,
                expiryDateTime,
                expiryTimestamp,
                Timezones.AET,
                quoteDataSnapshot,
                this.performingUserId,
                Instant.MinValue,
                Guid.NewGuid());

            // Act
            var serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
            }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

            var json = JsonConvert.SerializeObject(@event, serializerSettings);
            var obj = JsonConvert.DeserializeObject<IEvent<QuoteAggregate, Guid>>(json, serializerSettings);

            // Assert
            var result = obj as QuoteAggregate.PolicyIssuedEvent;
            Assert.NotNull(result);
            //// Assert.Equal(aggregate.Id, result.AggregateId); // removed aggregate id. aggregate will have access to this when obtained from repository
            Assert.Equal(policyNumber, result.PolicyNumber);
            Assert.Equal(inceptionDateTime, result.InceptionDateTime);
            Assert.Equal(expiryDateTime, result.ExpiryDateTime);
            Assert.Equal(inceptionTimestamp, result.InceptionTimestamp);
            Assert.Equal(expiryTimestamp, result.ExpiryTimestamp);
            Assert.Equal(quoteDataSnapshot.FormData.Id, result.DataSnapshot.FormData.Id);
            Assert.Equal(quoteDataSnapshot.CalculationResult.Id, result.DataSnapshot.CalculationResult.Id);
            Assert.Equal(quoteDataSnapshot.CustomerDetails.Id, result.DataSnapshot.CustomerDetails.Id);
        }

        private QuoteDataSnapshot CreateSnapshot(
            string formDataJson,
            CalculationResult calculationResult)
        {
            var mockPersonalDetails = new Mock<IPersonalDetails>();
            mockPersonalDetails.SetupAllProperties();
            var calculationResultUpdate = new QuoteDataUpdate<CalculationResult>(
                    Guid.NewGuid(),
                    calculationResult,
                    Instant.MinValue);
            var formDataUpdate = new QuoteDataUpdate<FormData>(
                    Guid.NewGuid(),
                    new FormData(formDataJson),
                    Instant.MinValue);
            var customerDetailsUpdate = new QuoteDataUpdate<IPersonalDetails>(
                    Guid.NewGuid(),
                    new PersonalDetails(mockPersonalDetails.Object),
                    Instant.MinValue);
            return new QuoteDataSnapshot(
                formDataUpdate,
                calculationResultUpdate,
                customerDetailsUpdate);
        }
    }
}
