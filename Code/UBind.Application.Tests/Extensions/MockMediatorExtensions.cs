// <copyright file="MockMediatorExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests
{
    using System.Threading;
    using Moq;
    using UBind.Application.Queries.Portal;
    using UBind.Application.Queries.Product;
    using UBind.Application.Queries.Tenant;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.Portal;

    public static class MockMediatorExtensions
    {
        public static void GetTenantByIdOrAliasQuery(this Mock<ICqrsMediator> mock, Domain.Tenant tenant)
        {
            mock.Setup(x => x.Send(It.IsAny<GetTenantByIdQuery>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(tenant);
            mock.Setup(x => x.Send(It.IsAny<GetTenantByAliasQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(tenant);
        }

        public static void GetProductByIdOrAliasQuery(this Mock<ICqrsMediator> mock, Domain.Product.Product product)
        {
            mock.Setup(x => x.Send(It.IsAny<GetProductByAliasQuery>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(product);
            mock.Setup(x => x.Send(It.IsAny<GetProductByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);
        }

        public static void GetPortalByIdOrAliasQuery(this Mock<ICqrsMediator> mock, PortalReadModel portal)
        {
            mock.Setup(x => x.Send(It.IsAny<GetPortalByAliasQuery>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(portal);
            mock.Setup(x => x.Send(It.IsAny<GetPortalByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(portal);
        }
    }
}
