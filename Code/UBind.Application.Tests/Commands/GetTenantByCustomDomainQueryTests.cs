// <copyright file="GetTenantByCustomDomainQueryTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Queries
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NodaTime;
    using UBind.Application.Queries.Tenant;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;
    using Xunit;

    public class GetTenantByCustomDomainQueryTests
    {
        private readonly ServiceCollection serviceCollection;
        private readonly Mock<ITenantRepository> mockTenantRepository = new Mock<ITenantRepository>();
        private Guid tenantId = Guid.NewGuid();

        public GetTenantByCustomDomainQueryTests()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IClock>(SystemClock.Instance);
            services
                .AddSingleton<IQueryHandler<GetTenantByCustomDomainQuery, Tenant>,
                    GetTenantByCustomDomainQueryHandler>();

            services.AddSingleton(this.mockTenantRepository.Object);
            this.serviceCollection = services;
        }

        [Fact]
        public async Task GetTenantByCustomDomain_ReturnsTenant_WhenThereIsAMatchTenant()
        {
            //// Arrange

            var domainName = "test.com";

            var tenant = new Tenant(
                this.tenantId,
                "foo",
                this.tenantId.ToString(),
                domainName,
                default,
                default,
                SystemClock.Instance.GetCurrentInstant());

            this.mockTenantRepository.Setup(e => e.GetTenantByCustomDomain(domainName)).Returns(tenant);

            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<IQueryHandler<GetTenantByCustomDomainQuery, Tenant>>();

            //// Act
            var result = await sut.Handle(
                                       new GetTenantByCustomDomainQuery(
                                           domainName), CancellationToken.None);

            //// Assert
            this.mockTenantRepository.Verify(e => e.GetTenantByCustomDomain(domainName));
            result.Should().Be(tenant);
        }

        [Fact]
        public async Task GetTenantByCustomDomain_ReturnsNull_WhenThereIsNoMatchTenant()
        {
            //// Arrange

            var domainName = "test.com";
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<IQueryHandler<GetTenantByCustomDomainQuery, Tenant>>();

            //// Act
            var result = await sut.Handle(
                                       new GetTenantByCustomDomainQuery(
                                           domainName), CancellationToken.None);

            //// Assert
            this.mockTenantRepository.Verify(e => e.GetTenantByCustomDomain(domainName));
            result.Should().Be(null);
        }
    }
}
