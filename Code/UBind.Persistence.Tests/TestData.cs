// <copyright file="TestData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests
{
    using System;
    using System.Threading.Tasks;
    using UBind.Application.Commands.Tenant;
    using UBind.Application.Queries.Tenant;
    using UBind.Domain;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Tests.Fakes;

    public static class TestData
    {
        public static async Task<TenantProductModel> CreateProductAndTenant(
            Guid productId,
            ApplicationStack stack)
        {
            var guid = Guid.NewGuid();
            var tenantAlias = "test-tenant-" + guid;
            var tenantName = "Test Tenant " + guid;
            var tenantId = await stack.Mediator.Send(new CreateTenantCommand(tenantName, tenantAlias, null));
            var tenant = await stack.Mediator.Send(new GetTenantByIdQuery(tenantId));
            var product = ProductFactory.Create(tenant.Id, productId);
            stack.CreateProduct(product);

            return new TenantProductModel
            {
                Tenant = tenant,
                Product = product,
            };
        }

        /// <summary>
        /// Tenant product model to return just for testing.
        /// </summary>
        public class TenantProductModel
        {
#pragma warning disable SA1401 // Fields should be private
            public Tenant Tenant;
            public Domain.Product.Product Product;
#pragma warning restore SA1401 // Fields should be private
        }
    }
}
