// <copyright file="IAuthenticationMethodReadModelRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Organisation
{
    public interface IAuthenticationMethodReadModelRepository
    {
        IList<AuthenticationMethodReadModelSummary> Get(Guid tenantId, EntityListFilters filters);

        AuthenticationMethodReadModelSummary? Get(Guid tenantId, Guid authenticationMethodId);

        IList<IAuthenticationMethodReadModelSummary> GetSummaries(Guid tenantId, EntityListFilters filters);

        /// <summary>
        /// Returns true if there is a the local account authentication method stored for the given organisation,
        /// otherwise false.
        /// </summary>
        bool HasLocalAccountRecordForOrganisation(Guid tenantId, Guid organisationId);

        LocalAccountAuthenticationMethodReadModel? GetLocalAccountRecordForOrganisation(
            Guid tenantId, Guid organisationId);

        IList<AuthenticationMethodReadModelSummary> GetMany(Guid tenantId, IEnumerable<Guid> authenticationMethodIds);

        IList<AuthenticationMethodReadModelSummary> GetUserLinked(Guid tenantId, Guid userId);
    }
}
