// <copyright file="ReplayAllEventsForAggregateEntityCommandTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.Commands.Entity
{
    using System;
    using System.Threading;
    using FluentAssertions;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NodaTime;
    using UBind.Application.Commands.Entity;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Aggregates;
    using Xunit;

    public class ReplayAllEventsForAggregateEntityCommandTests
    {
        private readonly ServiceCollection serviceCollection;
        private readonly Mock<IQuoteAggregateRepository> quoteAggregateRepository = new Mock<IQuoteAggregateRepository>();
        private readonly Mock<IHttpContextPropertiesResolver> httpContextPropertiesResolver = new Mock<IHttpContextPropertiesResolver>();
        private readonly Mock<IUBindDbContext> ubindDbContextMock = new Mock<IUBindDbContext>();

        public ReplayAllEventsForAggregateEntityCommandTests()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IClock>(SystemClock.Instance);
            services.AddSingleton<
                ICommandHandler<ReplayAllEventsForAggregateEntityCommand, Unit>,
                ReplayAllEventsForAggregateEntityCommandHandler>();
            services.AddSingleton(this.quoteAggregateRepository.Object);
            services.AddSingleton(this.httpContextPropertiesResolver.Object);
            services.AddSingleton<IAggregateRepositoryResolver, AggregateRepositoryResolver>();
            services.AddSingleton(this.ubindDbContextMock.Object);
            this.serviceCollection = services;
        }

        [Fact]
        public void ReplayAllEvents_ForExistingAggregate_ShouldSucceed()
        {
            // Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetRequiredService<ICommandHandler<ReplayAllEventsForAggregateEntityCommand, Unit>>();
            var quoteAggregate = QuoteFactory.CreateNewPolicy(TenantFactory.DefaultId);
            this.quoteAggregateRepository
                .Setup(q => q.GetById(quoteAggregate.TenantId, quoteAggregate.Id))
                .Returns(quoteAggregate);

            // Act
            Action act = () => sut.Handle(
                new ReplayAllEventsForAggregateEntityCommand(TenantFactory.DefaultId, quoteAggregate.Id, AggregateEntityType.Quote),
                CancellationToken.None);

            // Assert
            act.Should().NotThrow();
        }
    }
}