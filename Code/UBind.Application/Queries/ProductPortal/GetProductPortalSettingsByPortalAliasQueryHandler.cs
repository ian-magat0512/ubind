// <copyright file="GetProductPortalSettingsByPortalAliasQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.ProductPortal
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;

    public class GetProductPortalSettingsByPortalAliasQueryHandler
        : IQueryHandler<GetProductPortalSettingsByPortalAliasQuery, IEnumerable<ProductPortalSettingModel>>
    {
        private readonly IProductPortalSettingRepository productPortalSettingRepository;

        public GetProductPortalSettingsByPortalAliasQueryHandler(
            IProductPortalSettingRepository productPortalSettingRepository)
        {
            this.productPortalSettingRepository = productPortalSettingRepository;
        }

        public Task<IEnumerable<ProductPortalSettingModel>> Handle(
            GetProductPortalSettingsByPortalAliasQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result =
                this.productPortalSettingRepository.GetProductPortalSettings(request.TenantId, request.PortalAlias)
                .ToList();

            return Task.FromResult(result.AsEnumerable());
        }
    }
}
