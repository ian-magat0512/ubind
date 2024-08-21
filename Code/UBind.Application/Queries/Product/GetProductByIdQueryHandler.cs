// <copyright file="GetProductByIdQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Product
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;

    public class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, Domain.Product.Product>
    {
        private readonly IProductRepository productRepository;

        public GetProductByIdQueryHandler(IProductRepository productRepository)
        {
            this.productRepository = productRepository;
        }

        public Task<Domain.Product.Product> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Domain.Product.Product product
                = this.productRepository.GetProductById(request.TenantId, request.ProductId);

            return Task.FromResult(product);
        }
    }
}
