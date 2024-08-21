// <copyright file="AuthenticationMethodService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services;

using UBind.Application.Mappers;
using UBind.Application.Model.AuthenticationMethod;
using UBind.Domain.Aggregates.Organisation;
using UBind.Domain;
using UBind.Domain.Aggregates.Organisation.AuthenticationMethod;
using NodaTime;
using UBind.Domain.Extensions;
using OrganisationAggregate = UBind.Domain.Aggregates.Organisation.Organisation;

public class AuthenticationMethodService : IAuthenticationMethodService
{
    private readonly IOrganisationAggregateRepository organisationAggregateRepository;
    private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
    private readonly IClock clock;

    public AuthenticationMethodService(
        IOrganisationAggregateRepository organisationAggregateRepository,
        IHttpContextPropertiesResolver httpContextPropertiesResolver,
        IClock clock)
    {
        this.organisationAggregateRepository = organisationAggregateRepository;
        this.httpContextPropertiesResolver = httpContextPropertiesResolver;
        this.clock = clock;
    }

    public async Task<OrganisationAggregate> Create(OrganisationAggregate organisationAggregate, AuthenticationMethodUpsertModel upsertModel)
    {
        var now = this.clock.Now();
        var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;

        // map the upsert model to the domain model using Mapperly
        var mapper = new AuthenticationMethodMapper();
        AuthenticationMethodBase model = mapper.MapUpsertModelToDomainModel(upsertModel, Guid.NewGuid(), now);
        organisationAggregate.AddAuthenticationMethod(model, performingUserId, now);
        await this.organisationAggregateRepository.ApplyChangesToDbContext(organisationAggregate);
        return organisationAggregate;
    }

    public async Task<OrganisationAggregate> Update(OrganisationAggregate organisationAggregate, Guid authenticationMethodId, AuthenticationMethodUpsertModel upsertModel)
    {
        var now = this.clock.Now();
        var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;

        // map the upsert model to the domain model using Mapperly
        var mapper = new AuthenticationMethodMapper();
        AuthenticationMethodBase model = mapper.MapUpsertModelToDomainModel(upsertModel, authenticationMethodId, now);
        organisationAggregate.UpdateAuthenticationMethod(model, performingUserId, now);
        await this.organisationAggregateRepository.ApplyChangesToDbContext(organisationAggregate);
        return organisationAggregate;
    }

    public async Task<OrganisationAggregate> Enable(OrganisationAggregate organisationAggregate, Guid authenticationMethodId)
    {
        var now = this.clock.Now();
        var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;
        organisationAggregate.EnableAuthenticationMethod(authenticationMethodId, performingUserId, now);
        await this.organisationAggregateRepository.ApplyChangesToDbContext(organisationAggregate);
        return organisationAggregate;
    }

    public async Task<OrganisationAggregate> Disable(OrganisationAggregate organisationAggregate, Guid authenticationMethodId)
    {
        var now = this.clock.Now();
        var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;
        organisationAggregate.DisableAuthenticationMethod(authenticationMethodId, performingUserId, now);
        await this.organisationAggregateRepository.ApplyChangesToDbContext(organisationAggregate);
        return organisationAggregate;
    }
}
