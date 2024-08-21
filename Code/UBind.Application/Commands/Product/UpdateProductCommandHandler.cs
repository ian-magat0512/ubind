// <copyright file="UpdateProductCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Product;

using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Product;

/// <summary>
/// Handles the command to update a product.
/// </summary>
public class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand, Product>
{
    private readonly IProductService productService;

    public UpdateProductCommandHandler(
        IProductService productService)
    {
        this.productService = productService;
    }

    public async Task<Product> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await this.productService.UpdateProduct(
            command.TenantId,
            command.ProductId,
            command.NewName,
            command.NewAlias,
            command.Disabled,
            command.Deleted,
            cancellationToken,
            command.ProductExpirySettings,
            command.ExpirySettingUpdateType);
    }
}
