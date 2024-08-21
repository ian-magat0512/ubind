// <copyright file="GetProductOrganisationSettingQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.ProductOrganisation
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Handler for <see cref="GetProductOrganisationSettingQuery"/>.
    /// </summary>
    public class GetProductOrganisationSettingQueryHandler
        : IQueryHandler<GetProductOrganisationSettingQuery, ProductOrganisationSetting?>
    {
        private readonly IProductOrganisationSettingRepository productOrganisationSettingRepository;

        public GetProductOrganisationSettingQueryHandler(
            IProductOrganisationSettingRepository productOrganisationSettingRepository)
        {
            this.productOrganisationSettingRepository = productOrganisationSettingRepository;
        }

        public Task<ProductOrganisationSetting?> Handle(
            GetProductOrganisationSettingQuery request, CancellationToken cancellationToken)
        {
            var result = this.productOrganisationSettingRepository
                .GetProductOrganisationSetting(request.TenantId, request.OrganisationId, request.ProductId);
            return Task.FromResult(result);
        }
    }
}
