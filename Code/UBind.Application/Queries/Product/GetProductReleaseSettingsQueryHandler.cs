// <copyright file="GetProductReleaseSettingsQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Product
{
    using UBind.Domain;
    using UBind.Domain.Models;
    using UBind.Domain.Patterns.Cqrs;

    internal class GetProductReleaseSettingsQueryHandler :
        IQueryHandler<GetProductReleaseSettingsQuery, ProductReleaseEntitySettings>
    {
        private readonly IEntitySettingsRepository entitySettingRepository;

        public GetProductReleaseSettingsQueryHandler(IEntitySettingsRepository entitySettingRepository)
        {
            this.entitySettingRepository = entitySettingRepository;
        }

        public Task<ProductReleaseEntitySettings> Handle(GetProductReleaseSettingsQuery request, CancellationToken cancellationToken)
        {
            var productReleaseSettings = this.entitySettingRepository
                .GetEntitySettings<ProductReleaseEntitySettings>(request.TenantId, EntityType.Product, request.ProductId);
            return Task.FromResult(productReleaseSettings);
        }
    }
}
