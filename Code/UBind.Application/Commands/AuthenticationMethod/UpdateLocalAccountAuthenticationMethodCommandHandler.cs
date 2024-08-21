﻿// <copyright file="UpdateLocalAccountAuthenticationMethodCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.AuthenticationMethod
{
    using Humanizer;
    using UBind.Application.Services;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.Organisation;

    public class UpdateLocalAccountAuthenticationMethodCommandHandler
        : ICommandHandler<UpdateLocalAccountAuthenticationMethodCommand, AuthenticationMethodReadModelSummary>
    {
        private readonly IOrganisationAggregateRepository organisationAggregateRepository;
        private readonly IAuthenticationMethodService authenticationMethodService;

        public UpdateLocalAccountAuthenticationMethodCommandHandler(
            IOrganisationAggregateRepository organisationAggregateRepository,
            IAuthenticationMethodService authenticationMethodService)
        {
            this.organisationAggregateRepository = organisationAggregateRepository;
            this.authenticationMethodService = authenticationMethodService;
        }

        public async Task<AuthenticationMethodReadModelSummary> Handle(UpdateLocalAccountAuthenticationMethodCommand command, CancellationToken cancellationToken)
        {
            var organisationAggregate
                = this.organisationAggregateRepository.GetById(command.TenantId, command.OrganisationId);
            EntityHelper.ThrowIfNotFound(organisationAggregate, command.OrganisationId, "Organisation");

            // check if it exists first
            string localAccountTypeName = AuthenticationMethodType.LocalAccount.Humanize();
            var record = organisationAggregate.AuthenticationMethods.FirstOrDefault(am => am.TypeName == localAccountTypeName);
            if (record == null)
            {
                // let's create it
                await this.authenticationMethodService.Create(organisationAggregate, command.AuthenticationMethod);
            }
            else
            {
                // update it
                await this.authenticationMethodService.Update(organisationAggregate, record.Id, command.AuthenticationMethod);
            }

            return organisationAggregate.LatestProjectedAuthenticationMethodReadModel;
        }
    }
}
