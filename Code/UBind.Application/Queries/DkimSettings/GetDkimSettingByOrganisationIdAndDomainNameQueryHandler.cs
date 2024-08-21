﻿// <copyright file="GetDkimSettingByOrganisationIdAndDomainNameQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.DkimSettings
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.Entities;
    using UBind.Domain.Patterns.Cqrs;

    public class GetDkimSettingByOrganisationIdAndDomainNameQueryHandler :
        IQueryHandler<GetDkimSettingByOrganisationIdAndDomainNameQuery, DkimSettings?>
    {
        private readonly IDkimSettingRepository dkimSettingRepository;

        public GetDkimSettingByOrganisationIdAndDomainNameQueryHandler(
            IDkimSettingRepository dkimSettingRepository)
        {
            this.dkimSettingRepository = dkimSettingRepository;
        }

        public Task<DkimSettings?> Handle(
            GetDkimSettingByOrganisationIdAndDomainNameQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var dkimSettings = this.dkimSettingRepository
                .GetDkimSettingsbyOrganisationIdAndDomainName(request.TenantId, request.OrganisationId, request.Domain).FirstOrDefault();
            return Task.FromResult(dkimSettings);
        }
    }
}
