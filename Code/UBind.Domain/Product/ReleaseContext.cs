// <copyright file="ReleaseContext.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public struct ReleaseContext : IProductContext, IEquatable<ReleaseContext>
{
    public ReleaseContext(Guid tenantId, Guid productId, DeploymentEnvironment environment, Guid productReleaseId)
    {
        this.TenantId = tenantId;
        this.ProductId = productId;
        this.Environment = environment;
        this.ProductReleaseId = productReleaseId;
    }

    public ReleaseContext(IProductContext productContext, Guid productReleaseId)
        : this(productContext.TenantId, productContext.ProductId, productContext.Environment, productReleaseId)
    {
    }

    [JsonProperty("tenantId")]
    public Guid TenantId { get; }

    [JsonProperty("productId")]
    public Guid ProductId { get; }

    [JsonProperty("environment")]
    [JsonConverter(typeof(StringEnumConverter))]
    public DeploymentEnvironment Environment { get; }

    [JsonProperty("productReleaseId")]
    public Guid ProductReleaseId { get; }

    public bool Equals(ReleaseContext other) =>
        other.TenantId == this.TenantId
        && other.ProductId == this.ProductId
        && other.Environment == this.Environment
        && other.ProductReleaseId == this.ProductReleaseId;

    public override bool Equals(object obj) =>
        obj is ReleaseContext context &&
        this.Equals(context);

    /// <inheritdoc/>
    public override int GetHashCode() =>
        HashCode.Combine(
            this.TenantId,
            this.ProductId,
            this.ProductReleaseId);

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{this.TenantId}/{this.ProductId}/{this.ProductReleaseId}";
    }
}
