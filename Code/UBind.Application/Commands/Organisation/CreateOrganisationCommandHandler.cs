// <copyright file="CreateOrganisationCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Organisation
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Services;

    public class CreateOrganisationCommandHandler : ICommandHandler<CreateOrganisationCommand, OrganisationReadModel>
    {
        private readonly IOrganisationService organisationService;

        public CreateOrganisationCommandHandler(
            IOrganisationService organisationService)
        {
            this.organisationService = organisationService;
        }

        public async Task<OrganisationReadModel> Handle(CreateOrganisationCommand command, CancellationToken cancellationToken)
        {
            var organisationAggregate = await this.organisationService.CreateOrganisation(
                command.TenantId,
                command.Alias,
                command.Name,
                command.ManagingOrganisationId,
                command.Properties,
                command.LinkedIdentities);
            return organisationAggregate.LatestProjectedReadModel;
        }
    }
}
