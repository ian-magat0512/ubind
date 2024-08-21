// <copyright file="ProductRepositoryTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests
{
    using System;
    using FluentAssertions;
    using UBind.Domain.Extensions;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class ProductRepositoryTests
    {
        [Fact]
        public void GetProductById_RecordsHasNewIds_WhenCreatingProduct()
        {
            using (ApplicationStack stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var tenantId = Guid.NewGuid();
                var productId = Guid.NewGuid();

                // Arrange
                var tenant = new Domain.Tenant(tenantId, "test", tenantId.ToString(), null, default, default, stack.Clock.Now());
                stack.TenantRepository.Insert(tenant);
                stack.TenantRepository.SaveChanges();
                var product = new Domain.Product.Product(tenant.Id, productId, "test", "alias", stack.Clock.Now());
                stack.ProductRepository.Insert(product);
                stack.ProductRepository.SaveChanges();

                // Act
                var retrievedProduct = stack.ProductRepository.GetProductById(tenant.Id, product.Id);

                // Assert
                retrievedProduct.Should().NotBeNull();
                retrievedProduct.Id.Should().Be(product.Id);
                retrievedProduct.Id.Should().Be(product.Id);
                retrievedProduct.TenantId.Should().Be(tenant.Id);
                retrievedProduct.DetailsCollection.Should().NotBeNull();
            }
        }
    }
}
