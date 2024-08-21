// <copyright file="ReplayAggregateEntityEventCommandTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.Commands.Entity
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NodaTime;
    using UBind.Application.Commands.QuoteAggregate;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Aggregates;
    using Xunit;

    public class ReplayAggregateEntityEventCommandTests
    {
        private readonly ServiceCollection serviceCollection;
        private readonly Mock<IQuoteAggregateRepository> quoteAggregateRepository = new Mock<IQuoteAggregateRepository>();
        private readonly Mock<IHttpContextPropertiesResolver> httpContextPropertiesResolver = new Mock<IHttpContextPropertiesResolver>();
        private readonly Mock<IUBindDbContext> ubindDbContextMock = new Mock<IUBindDbContext>();

        public ReplayAggregateEntityEventCommandTests()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IClock>(SystemClock.Instance);
            services.AddSingleton<
                ICommandHandler<ReplayAggregateEntityEventCommand, Unit>,
                ReplayAggregateEntityEventCommandHandler>();
            services.AddSingleton(this.quoteAggregateRepository.Object);
            services.AddSingleton(this.httpContextPropertiesResolver.Object);
            services.AddSingleton<IAggregateRepositoryResolver, AggregateRepositoryResolver>();
            services.AddSingleton(this.ubindDbContextMock.Object);
            this.serviceCollection = services;
        }

        [Fact]
        public async Task ReplayEvent_WithDispatchToSystemEventEmitter_ShouldPassCorrectObserverTypes()
        {
            // Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetRequiredService<ICommandHandler<ReplayAggregateEntityEventCommand, Unit>>();
            var quoteAggregate = QuoteFactory.CreateNewPolicy(TenantFactory.DefaultId);
            this.quoteAggregateRepository
                .Setup(q => q.GetById(quoteAggregate.TenantId, quoteAggregate.Id))
                .Returns(quoteAggregate);
            this.quoteAggregateRepository.Setup(s => s.ReplayEventByAggregateId(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<IEnumerable<Type>>()));

            // Act
            await sut.Handle(
                new ReplayAggregateEntityEventCommand(
                    quoteAggregate.TenantId,
                    quoteAggregate.Id,
                    AggregateEntityType.Quote,
                    5,
                    false,
                    false,
                    true),
                CancellationToken.None);

            // Assert
            this.quoteAggregateRepository.Verify(v => v.ReplayEventByAggregateId(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.Is<IEnumerable<Type>>(x => (x as IEnumerable<Type>).ToList().First().Equals(typeof(ISystemEventEmitter)))));
        }
    }
}
