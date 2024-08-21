﻿// <copyright file="GetActiveProductsQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Product;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Product;

public class GetActiveProductsQueryHandler : IQueryHandler<GetActiveProductsQuery, List<Product>>
{
    private readonly IProductRepository productRepository;

    public GetActiveProductsQueryHandler(IProductRepository productRepository)
    {
        this.productRepository = productRepository;
    }

    public Task<List<Product>> Handle(GetActiveProductsQuery request, CancellationToken cancellationToken)
    {
        var activeProducts = this.productRepository.GetActiveProducts().ToList();
        return Task.FromResult(activeProducts);
    }
}
