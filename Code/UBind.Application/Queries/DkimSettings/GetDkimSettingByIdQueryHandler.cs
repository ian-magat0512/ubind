// <copyright file="GetDkimSettingByIdQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.DkimSettings
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.Entities;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;

    public class GetDkimSettingByIdQueryHandler :
        IQueryHandler<GetDkimSettingByIdQuery, DkimSettings>
    {
        private readonly IDkimSettingRepository dkimSettingRepository;

        public GetDkimSettingByIdQueryHandler(
            IDkimSettingRepository dkimSettingRepository)
        {
            this.dkimSettingRepository = dkimSettingRepository;
        }

        public async Task<DkimSettings?> Handle(
            GetDkimSettingByIdQuery request, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var dkimSetting = this.dkimSettingRepository
                .GetDkimSettingById(request.TenantId, request.OrganisationId, request.DkimSettingsId);
            if (dkimSetting == null)
            {
                throw new ErrorException(Errors.General.NotFound("DKIM setting", request.DkimSettingsId));
            }

            return await Task.FromResult(dkimSetting);
        }
    }
}
