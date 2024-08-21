// <copyright file="DeleteDkimSettingsCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.DkimSettings
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command handler for deleting DKIM settings.
    /// </summary>
    public class DeleteDkimSettingsCommandHandler : ICommandHandler<DeleteDkimSettingsCommand, Unit>
    {
        private readonly IDkimSettingRepository dkimSettingRepository;
        private readonly IOrganisationSystemEventEmitter organisationSystemEventEmitter;

        public DeleteDkimSettingsCommandHandler(
            IDkimSettingRepository dkimSettingRepository,
            IOrganisationSystemEventEmitter organisationSystemEventEmitter)
        {
            this.dkimSettingRepository = dkimSettingRepository;
            this.organisationSystemEventEmitter = organisationSystemEventEmitter;
        }

        public async Task<Unit> Handle(DeleteDkimSettingsCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var dkimSettings = this.dkimSettingRepository.GetDkimSettingById(request.TenantId, request.OrganisationId, request.DkimSettingsId);
            if (dkimSettings == null)
            {
                throw new ErrorException(Errors.General.NotFound("DKIM setting", request.DkimSettingsId));
            }

            this.dkimSettingRepository.Delete(request.TenantId, request.DkimSettingsId, request.OrganisationId);
            await this.dkimSettingRepository.SaveChangesAsync();
            await this.organisationSystemEventEmitter.CreateAndEmitModifiedSystemEvent(request.TenantId, request.OrganisationId);

            return await Task.FromResult(Unit.Value);
        }
    }
}
