// <copyright file="GetDkimSettingsByTenantIdAndOrganisationIdQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.DkimSettings
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.Entities;
    using UBind.Domain.Patterns.Cqrs;

    public class GetDkimSettingsByTenantIdAndOrganisationIdQueryHandler :
        IQueryHandler<GetDkimSettingsByTenantIdAndOrganisationIdQuery, IEnumerable<DkimSettings>>
    {
        private readonly IDkimSettingRepository dkimSettingRepository;

        public GetDkimSettingsByTenantIdAndOrganisationIdQueryHandler(
            IDkimSettingRepository dkimSettingRepository)
        {
            this.dkimSettingRepository = dkimSettingRepository;
        }

        public async Task<IEnumerable<DkimSettings>> Handle(
            GetDkimSettingsByTenantIdAndOrganisationIdQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var dkimSettingsModel = this.dkimSettingRepository
                .GetDkimSettingsByTenantIdAndOrganisationId(request.TenantId, request.OrganisationId)
                .ToList();

            return await Task.FromResult(dkimSettingsModel);
        }
    }
}
