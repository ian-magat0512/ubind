// <copyright file="UpdateOrganisationCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Organisation
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Services;

    public class UpdateOrganisationCommandHandler : ICommandHandler<UpdateOrganisationCommand, Unit>
    {
        private readonly IOrganisationService organisationService;

        public UpdateOrganisationCommandHandler(IOrganisationService organisationService)
        {
            this.organisationService = organisationService;
        }

        public async Task<Unit> Handle(UpdateOrganisationCommand command, CancellationToken cancellationToken)
        {
            await this.organisationService.UpdateOrganisation(
                command.TenantId,
                command.OrganisationId,
                command.Name,
                command.Alias,
                command.Properties,
                command.LinkedIdentities);
            return Unit.Value;
        }
    }
}
