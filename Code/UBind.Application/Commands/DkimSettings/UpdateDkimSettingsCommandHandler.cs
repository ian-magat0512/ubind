// <copyright file="UpdateDkimSettingsCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.DkimSettings
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Entities;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Command handler for updating DKIM settings.
    /// </summary>
    public class UpdateDkimSettingsCommandHandler : ICommandHandler<UpdateDkimSettingsCommand, DkimSettings>
    {
        private readonly IDkimSettingRepository dkimSettingRepository;
        private readonly IOrganisationReadModelRepository organisationReadModelRepository;
        private readonly ITenantRepository tenantRepository;
        private readonly IOrganisationSystemEventEmitter organisationSystemEventEmitter;

        public UpdateDkimSettingsCommandHandler(
            IDkimSettingRepository dkimSettingRepository,
            IOrganisationReadModelRepository organisationReadModelRepository,
            IOrganisationSystemEventEmitter organisationSystemEventEmitter,
            ITenantRepository tenantRepository)
        {
            this.dkimSettingRepository = dkimSettingRepository;
            this.organisationReadModelRepository = organisationReadModelRepository;
            this.tenantRepository = tenantRepository;
            this.organisationSystemEventEmitter = organisationSystemEventEmitter;
        }

        public async Task<DkimSettings> Handle(UpdateDkimSettingsCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.Validate(request);
            var updatedDkimSetting = this.dkimSettingRepository.Update(request.TenantId, request.DkimSettingsId, request.OrganisationId, request.DomainName, request.PrivateKey, request.DnsSelector, request.AgentOrUserIdentifier, request.ApplicableDomainNameList);
            await this.dkimSettingRepository.SaveChangesAsync();
            await this.organisationSystemEventEmitter.CreateAndEmitModifiedSystemEvent(
                request.TenantId,
                request.OrganisationId);
            return updatedDkimSetting;
        }

        private void Validate(UpdateDkimSettingsCommand request)
        {
            var dkimSetting = this.dkimSettingRepository.GetDkimSettingById(request.TenantId, request.OrganisationId, request.DkimSettingsId);
            if (dkimSetting == null)
            {
                throw new ErrorException(Errors.General.NotFound("DKIM settings", request.DkimSettingsId));
            }

            var organisation = this.organisationReadModelRepository.Get(dkimSetting.OrganisationId, dkimSetting.TenantId);

            var tenant = this.tenantRepository.GetTenantById(dkimSetting.TenantId);
            if (tenant == null)
            {
                throw new ErrorException(Errors.General.NotFound("Tenant", dkimSetting.TenantId));
            }

            var settings = this.dkimSettingRepository.GetDkimSettingsbyOrganisationIdAndDomainName(request.TenantId, dkimSetting.OrganisationId, request.DomainName);
            if (settings.Count() > 1)
            {
                throw new ErrorException(Errors.Organisation.DuplicateDomainNameWithInOrganisation(organisation.Id, organisation.Name, request.DomainName));
            }
        }
    }
}
