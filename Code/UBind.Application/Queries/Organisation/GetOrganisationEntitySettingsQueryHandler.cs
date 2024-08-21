// <copyright file="GetOrganisationEntitySettingsQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Organisation
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.Models;
    using UBind.Domain.Patterns.Cqrs;

    public class GetOrganisationEntitySettingsQueryHandler :
        IQueryHandler<GetOrganisationEntitySettingsQuery, OrganisationEntitySettings>
    {
        private readonly IEntitySettingsRepository entitySettingRepository;

        public GetOrganisationEntitySettingsQueryHandler(IEntitySettingsRepository entitySettingRepository)
        {
            this.entitySettingRepository = entitySettingRepository;
        }

        public Task<OrganisationEntitySettings> Handle(GetOrganisationEntitySettingsQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var organisationSetting = this.entitySettingRepository
                .GetEntitySettings<OrganisationEntitySettings>(request.TenantId, EntityType.Organisation, request.OrganisationId);
            return Task.FromResult(organisationSetting);
        }
    }
}
