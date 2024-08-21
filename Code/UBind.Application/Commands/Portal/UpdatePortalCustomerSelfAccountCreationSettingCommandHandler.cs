// <copyright file="UpdatePortalCustomerSelfAccountCreationSettingCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Organisation
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Domain;
    using UBind.Domain.Models;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command handler responsible for updating the account creation from login page setting.
    /// </summary>
    public class UpdatePortalCustomerSelfAccountCreationSettingCommandHandler
        : ICommandHandler<UpdatePortalCustomerSelfAccountCreationSettingCommand, Unit>
    {
        private readonly IEntitySettingsRepository entitySettingRepository;

        public UpdatePortalCustomerSelfAccountCreationSettingCommandHandler(
            IEntitySettingsRepository entitySettingRepository)
        {
            this.entitySettingRepository = entitySettingRepository;
        }

        public Task<Unit> Handle(UpdatePortalCustomerSelfAccountCreationSettingCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var portalSetting = this.entitySettingRepository
                .GetEntitySettings<PortalEntitySettings>(request.TenantId, EntityType.Portal, request.PortalId);
            portalSetting.AllowCustomerSelfAccountCreation = request.AllowCustomerSelfAccountCreation;
            this.entitySettingRepository.AddOrUpdateEntitySettings(
                request.TenantId, EntityType.Portal, request.PortalId, portalSetting);
            return Task.FromResult(Unit.Value);
        }
    }
}
