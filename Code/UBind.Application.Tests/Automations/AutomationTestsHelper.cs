// <copyright file="AutomationTestsHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations
{
    using System;
    using Moq;
    using UBind.Application.Releases;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.Repositories;

    public static class AutomationTestsHelper
    {
        public static IServiceProvider CreateMockServiceProvider(
            IReleaseQueryService releaseQueryService,
            ITenantRepository tenantRepository,
            IProductRepository productRepository,
            ICachingResolver cachingResolver,
            ICqrsMediator mediator)
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(c => c.GetService(typeof(IReleaseQueryService))).Returns(releaseQueryService);
            mockServiceProvider.Setup(c => c.GetService(typeof(ITenantRepository))).Returns(tenantRepository);
            mockServiceProvider.Setup(c => c.GetService(typeof(IProductRepository))).Returns(productRepository);
            mockServiceProvider.Setup(c => c.GetService(typeof(ICachingResolver))).Returns(cachingResolver);
            mockServiceProvider.Setup(c => c.GetService(typeof(ICqrsMediator))).Returns(mediator);
            mockServiceProvider.AddLoggers();
            return mockServiceProvider.Object;
        }
    }
}
