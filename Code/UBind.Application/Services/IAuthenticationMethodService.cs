// <copyright file="IAuthenticationMethodService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services;

using UBind.Application.Model.AuthenticationMethod;
using OrganisationAggregate = UBind.Domain.Aggregates.Organisation.Organisation;

public interface IAuthenticationMethodService
{
    Task<OrganisationAggregate> Create(OrganisationAggregate organisationAggregate, AuthenticationMethodUpsertModel upsertModel);

    Task<OrganisationAggregate> Update(
        OrganisationAggregate organisationAggregate,
        Guid authenticationMethodId,
        AuthenticationMethodUpsertModel upsertModel);

    Task<OrganisationAggregate> Enable(OrganisationAggregate organisationAggregate, Guid authenticationMethodId);

    Task<OrganisationAggregate> Disable(OrganisationAggregate organisationAggregate, Guid authenticationMethodId);
}
