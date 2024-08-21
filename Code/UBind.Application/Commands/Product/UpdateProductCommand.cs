// <copyright file="UpdateProductCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Product;

using UBind.Domain;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Product;

/// <summary>
/// Represents the command to be used when updating a product.
/// </summary>
public class UpdateProductCommand : ICommand<Product>
{
    public UpdateProductCommand(
        Guid tenantId,
        Guid productId,
        string newName,
        string newAlias,
        bool disabled,
        bool deleted,
        QuoteExpirySettings? productExpirySettings,
        ProductQuoteExpirySettingUpdateType expirySettingUpdateType)
    {
        this.TenantId = tenantId;
        this.ProductId = productId;
        this.NewName = newName;
        this.NewAlias = newAlias;
        this.Disabled = disabled;
        this.Deleted = deleted;
        this.ProductExpirySettings = productExpirySettings;
        this.ExpirySettingUpdateType = expirySettingUpdateType;
    }

    public Guid TenantId { get; private set; }

    public Guid ProductId { get; private set; }

    public string NewName { get; private set; }

    public string NewAlias { get; private set; }

    public bool Disabled { get; private set; }

    public bool Deleted { get; private set; }

    public QuoteExpirySettings? ProductExpirySettings { get; private set; }

    public ProductQuoteExpirySettingUpdateType ExpirySettingUpdateType { get; private set; }
}
