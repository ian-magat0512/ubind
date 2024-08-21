// <copyright file="ProductContext.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Specifies a product and environment to identify the context a quote belongs to.
    /// </summary>
    public struct ProductContext : IProductContext, IEquatable<ProductContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductContext"/> struct.
        /// </summary>
        /// <param name="tenantId">The string tenant ID.</param>
        /// <param name="productId">The string product ID.</param>
        /// <param name="environment">The deployment environment.</param>
        public ProductContext(Guid tenantId, Guid productId, DeploymentEnvironment environment)
        {
            this.TenantId = tenantId;
            this.ProductId = productId;
            this.Environment = environment;
        }

        /// <summary>
        /// Gets the ID of the tenant.
        /// </summary>
        [JsonProperty("tenantId")]
        public Guid TenantId { get; }

        /// <summary>
        /// Gets the ID of the product.
        /// </summary>
        [JsonProperty("productId")]
        public Guid ProductId { get; }

        /// <summary>
        /// Gets the deployment environment.
        /// </summary>
        [JsonProperty("environment")]
        [JsonConverter(typeof(StringEnumConverter))]
        public DeploymentEnvironment Environment { get; }

        /// <summary>
        /// Equality opertator for comparing two instance of <see cref="ProductContext"/>.
        /// </summary>
        /// <param name="lhs">The left hand side operand.</param>
        /// <param name="rhs">The right hand side operand.</param>
        /// <returns><c>true</c> if the product conttexts are identical, otherwise <c>false</c>.</returns>
        public static bool operator ==(ProductContext lhs, ProductContext rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(ProductContext lhs, ProductContext rhs)
        {
            return !lhs.Equals(rhs);
        }

        /// <inheritdoc/>
        public bool Equals(ProductContext other) =>
            other.TenantId == this.TenantId
            && other.ProductId == this.ProductId
            && other.Environment == this.Environment;

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            obj is ProductContext context &&
            this.Equals(context);

        /// <inheritdoc/>
        public override int GetHashCode() =>
            HashCode.Combine(
                this.TenantId,
                this.ProductId,
                this.Environment);

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{this.TenantId}/{this.ProductId}/{this.Environment}";
        }
    }
}
