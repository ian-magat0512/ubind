// <copyright file="UpdateOrganisationRenewalInvitationEmailsSettingCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Organisation
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Domain;
    using UBind.Domain.Models;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command handler responsible for updating the renewal invitation emails setting
    /// </summary>
    public class UpdateOrganisationRenewalInvitationEmailsSettingCommandHandler
        : ICommandHandler<UpdateOrganisationRenewalInvitationEmailsSettingCommand, Unit>
    {
        private readonly IEntitySettingsRepository entitySettingRepository;

        public UpdateOrganisationRenewalInvitationEmailsSettingCommandHandler(
            IEntitySettingsRepository entitySettingRepository)
        {
            this.entitySettingRepository = entitySettingRepository;
        }

        public Task<Unit> Handle(UpdateOrganisationRenewalInvitationEmailsSettingCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var organisationSetting = this.entitySettingRepository
                .GetEntitySettings<OrganisationEntitySettings>(request.TenantId, EntityType.Organisation, request.OrganisationId);
            organisationSetting.AllowOrganisationRenewalInvitation = request.AllowOrganisationRenewalInvitation;
            this.entitySettingRepository.AddOrUpdateEntitySettings(
                request.TenantId, EntityType.Organisation, request.OrganisationId, organisationSetting);
            return Task.FromResult(Unit.Value);
        }
    }
}
