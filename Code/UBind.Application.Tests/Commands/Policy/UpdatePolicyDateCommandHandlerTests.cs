// <copyright file="UpdatePolicyDateCommandHandlerTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Commands.Policy
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using MoreLinq;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Application.Commands.Policy;
    using UBind.Application.Releases;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Configuration;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Json;
    using UBind.Domain.Product;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using Xunit;
    using static UBind.Domain.Aggregates.Quote.QuoteAggregate;

    public class UpdatePolicyDateCommandHandlerTests
    {
        private readonly Mock<IQuoteAggregateRepository> quoteAggregateRepositoryMock
            = new Mock<IQuoteAggregateRepository>();

        private readonly Mock<IEventRecordRepository> eventRecordRepositoryMock
            = new Mock<IEventRecordRepository>();

        private readonly Mock<IProductConfigurationProvider> productConfigurationProviderMock;

        private readonly Mock<IReleaseQueryService> releaseQueryServiceMock
            = new Mock<IReleaseQueryService>();

        private readonly Mock<IAggregateSnapshotService<QuoteAggregate>> aggregateSnapshotService
            = new Mock<IAggregateSnapshotService<QuoteAggregate>>();

        private readonly TestClock clock = new TestClock();
        private readonly Guid performingUserId;
        private readonly IProductConfiguration config;

        public UpdatePolicyDateCommandHandlerTests()
        {
            var releaseContext = new ReleaseContext(TenantFactory.DefaultId, ProductFactory.DefaultId, DeploymentEnvironment.Development, Guid.NewGuid());
            this.releaseQueryServiceMock
                .Setup(s => s.GetDefaultReleaseContextOrNull(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>()))
                .Returns(releaseContext);
            this.config = new DefaultProductConfiguration();
            this.productConfigurationProviderMock = new Mock<IProductConfigurationProvider>();
            this.productConfigurationProviderMock
                .Setup(a => a.GetProductConfiguration(It.IsAny<ReleaseContext>(), WebFormAppType.Quote))
                .Returns(Task.FromResult(this.config));

            this.performingUserId = Guid.NewGuid();
            var productFeatureSettings = new ProductFeatureSetting(
                TenantFactory.DefaultId,
                ProductFactory.DefaultId,
                this.clock.Now());
        }

        [Fact]
        public async Task Handle_ThrowsAnError_WhenQuoteAggregateIsNotFound()
        {
            // Arrange
            var policy = QuoteFactory.CreateNewPolicy();

            var command = new UpdatePolicyDateCommand(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                this.performingUserId,
                PolicyDateType.InceptionDate,
                LocalDate.FromDateTime(DateTime.Today),
                null);

            var handler = this.CreateCommandHandler();

            // Act
            Func<Task> act = () => handler.Handle(command, CancellationToken.None);

            // Assert
            (await act.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be("aggregate.not.found");
        }

        [Fact]
        public async Task Handle_ThrowsAnError_WhenQuoteTheAggregateDoesNotContainAnyIPolicyUpsertEvent()
        {
            // Arrange
            var quote = QuoteFactory.CreateNewBusinessQuote();
            var aggregate = quote.Aggregate;

            this.quoteAggregateRepositoryMock
                .Setup(a => a.GetByIdWithoutUsingSnapshot(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(aggregate));

            var command = new UpdatePolicyDateCommand(
                aggregate.TenantId,
                Guid.NewGuid(),
                Guid.NewGuid(),
                this.performingUserId,
                PolicyDateType.InceptionDate,
                LocalDate.FromDateTime(DateTime.Today),
                null);

            var handler = this.CreateCommandHandler();

            // Act
            Func<Task> act = () => handler.Handle(command, CancellationToken.None);

            // Assert
            (await act.Should().ThrowAsync<ErrorException>())
                .Which.Error.Code.Should().Be("policy.date.patching.upsert.event.not.found");
        }

        [Fact]
        public async Task Handle_ThrowsAnError_WhenNothingWasPatchedInTheFormDataAndCalculationResult()
        {
            // Arrange
            var aggregate = QuoteFactory.CreateNewPolicy();

            var @event = aggregate.UnsavedEvents.OfType<PolicyIssuedEvent>().FirstOrDefault();
            this.quoteAggregateRepositoryMock
                .Setup(a => a.GetByIdWithoutUsingSnapshot(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(aggregate));

            // Ensure that there wouldn't be any patching in the form data and calculation result
            // because there wouldn't be any properties to patch
            @event.DataSnapshot.FormData.Data.PatchProperty(new Domain.Json.JsonPath("formModel"), JObject.Parse(" { \"data\": \"dummy data\" }"));
            foreach (var path in @event.DataSnapshot.CalculationResult.Data.JObject.Properties())
            {
                @event.DataSnapshot.CalculationResult.Data.PatchProperty(new JsonPath(path.Name),
                    new JObject() { { "property", "dummy" } });
            }

            this.productConfigurationProviderMock
                .Setup(a => a.GetProductConfiguration(It.IsAny<ReleaseContext>(), WebFormAppType.Quote)
                    .Result
                    .QuoteDataLocations
                    .InceptionDate)
                .Returns(new Domain.Aggregates.Quote.QuoteDatumLocation(
                    Domain.Aggregates.Quote.QuoteDataLocationObject.FormData, "dummyPath"));
            var command = new UpdatePolicyDateCommand(
                aggregate.TenantId,
                Guid.NewGuid(),
                Guid.NewGuid(),
                this.performingUserId,
                PolicyDateType.InceptionDate,
                LocalDate.FromDateTime(DateTime.Today),
                null);

            var handler = this.CreateCommandHandler();

            // Act
            Func<Task> act = () => handler.Handle(command, CancellationToken.None);

            // Assert
            (await act.Should().ThrowAsync<ErrorException>())
                .Which.Error.Code.Should().Be("policy.date.patching.property.not.found");
        }

        [Fact]
        public async Task Handle_Succeeds_WhenDatePropertiesArePatched()
        {
            // Arrange
            var aggregate = QuoteFactory.CreateNewPolicy();
            var @event = aggregate.UnsavedEvents.OfType<PolicyIssuedEvent>().FirstOrDefault();

            this.quoteAggregateRepositoryMock
                .Setup(a => a.GetByIdWithoutUsingSnapshot(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(aggregate));

            var command = new UpdatePolicyDateCommand(
                aggregate.TenantId,
                Guid.NewGuid(),
                Guid.NewGuid(),
                this.performingUserId,
                PolicyDateType.InceptionDate,
                LocalDate.FromDateTime(DateTime.Today),
                null);

            var handler = this.CreateCommandHandler();

            // Act
            Func<Task> act = () => handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task Handle_ReturnsThePatchedDate_WhenThePatchingOfDatesSucceeds()
        {
            // Arrange
            var asiaTimeZone = Timezones.GetTimeZoneByIdOrThrow("Asia/Singapore");
            var aggregate = QuoteFactory.CreateNewPolicy(timeZone: asiaTimeZone);
            var policy = aggregate.Policy;
            var @event = aggregate.UnsavedEvents.OfType<PolicyIssuedEvent>().FirstOrDefault();
            var test = aggregate.GetLatestQuote().TimeZone;
            this.quoteAggregateRepositoryMock
                .Setup(a => a.GetByIdWithoutUsingSnapshot(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(aggregate));

            // the date and time to be patched is in SGT
            var dateTimeInSGT = new LocalDateTime(2023, 12, 29, 9, 30, 56);
            var expectedDateInUTC = "December 29, 2023 01:30:56"; // SGT is UTC+8 so the expected date in UTC is 8 hours behind
            var expectedDateInAET = "December 29, 2023 12:30:56"; // AET is UTC+11 so the expected date in AET is 11 hours ahead of UTC
            var command = new UpdatePolicyDateCommand(
                aggregate.TenantId,
                Guid.NewGuid(),
                Guid.NewGuid(),
                this.performingUserId,
                PolicyDateType.InceptionDate,
                dateTimeInSGT.Date,
                dateTimeInSGT.TimeOfDay);

            var handler = this.CreateCommandHandler();

            // Act
            Func<Task<LocalDateTime>> act = () => handler.Handle(command, CancellationToken.None);

            // Assert
            var newDateTime = await act();
            newDateTime.ToString("MMMM dd, yyyy HH:mm:ss", null).Should().Be(expectedDateInUTC);
            newDateTime.ToTargetTimeZone(DateTimeZone.Utc, Timezones.AET).ToString("MMMM dd, yyyy HH:mm:ss", null).Should().Be(expectedDateInAET);
            newDateTime.ToTargetTimeZone(DateTimeZone.Utc, asiaTimeZone).Should().Be(dateTimeInSGT);
        }

        private UpdatePolicyDateCommandHandler CreateCommandHandler()
        {
            return new UpdatePolicyDateCommandHandler(
                this.quoteAggregateRepositoryMock.Object,
                this.eventRecordRepositoryMock.Object,
                this.productConfigurationProviderMock.Object,
                this.releaseQueryServiceMock.Object,
                this.aggregateSnapshotService.Object);
        }
    }
}
