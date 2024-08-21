// <copyright file="ProductRelease.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public class ProductRelease : BaseEntity<ReleaseBase>
{
    public ProductRelease(Guid id)
        : base(id)
    {
    }

    public ProductRelease(ReleaseBase release)
    {
        if (release is DevRelease)
        {
            this.ReleaseType = ReleaseType.Development;
        }
        else if (release is Release fullRelease)
        {
            this.ReleaseType = fullRelease.Type;
            this.MajorNumber = fullRelease.Number;
            this.MinorNumber = fullRelease.MinorNumber;
            this.Description = fullRelease.Description;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductRelease"/> class.
    /// </summary>
    [JsonConstructor]
    private ProductRelease()
    {
    }

    /// <summary>
    /// Gets or sets the tenant id of the product.
    /// </summary>
    [JsonProperty(PropertyName = "tenantId", Order = 21)]
    public string TenantId { get; set; }

    /// <summary>
    /// Gets or sets the tenant of the product.
    /// </summary>
    [JsonProperty(PropertyName = "tenant", Order = 23)]
    public Tenant Tenant { get; set; }

    /// <summary>
    /// Gets or sets the tenant id of the product.
    /// </summary>
    [JsonProperty(PropertyName = "productId", Order = 24)]
    public string ProductId { get; set; }

    /// <summary>
    /// Gets or sets the tenant of the product.
    /// </summary>
    [JsonProperty(PropertyName = "product", Order = 25)]
    public Product Product { get; set; }

    [JsonProperty(PropertyName = "majorNumber", Order = 25)]
    public int? MajorNumber { get; set; }

    [JsonProperty(PropertyName = "minorNumber", Order = 25)]
    public int? MinorNumber { get; set; }

    [JsonProperty(PropertyName = "patchNumber", Order = 25)]
    public int? PatchNumber { get; set; }

    /// <summary>
    /// Gets the release type.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public ReleaseType ReleaseType { get; set; }

    /// <summary>
    /// Gets or sets the description for the release.
    /// </summary>
    public string? Description { get; set; }
}
