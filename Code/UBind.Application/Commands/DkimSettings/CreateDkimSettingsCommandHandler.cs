// <copyright file="CreateDkimSettingsCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.DkimSettings
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Entities;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Command handler for creating DKIM settings.
    /// </summary>
    public class CreateDkimSettingsCommandHandler : ICommandHandler<CreateDkimSettingsCommand, DkimSettings>
    {
        private readonly IDkimSettingRepository dkimSettingRepository;
        private readonly ITenantRepository tenantRepository;
        private readonly IOrganisationReadModelRepository organisationReadModelRepository;
        private readonly IOrganisationSystemEventEmitter organisationSystemEventEmitter;
        private readonly IClock clock;

        public CreateDkimSettingsCommandHandler(
            IDkimSettingRepository dkimSettingRepository,
            IOrganisationReadModelRepository organisationReadModelRepository,
            IOrganisationSystemEventEmitter organisationSystemEventEmitter,
            ITenantRepository tenantRepository,
            IClock clock)
        {
            this.dkimSettingRepository = dkimSettingRepository;
            this.tenantRepository = tenantRepository;
            this.organisationReadModelRepository = organisationReadModelRepository;
            this.organisationSystemEventEmitter = organisationSystemEventEmitter;
            this.clock = clock;
        }

        public async Task<DkimSettings> Handle(CreateDkimSettingsCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.Validate(request);

            var dkimSettings = new DkimSettings(
                request.TenantId,
                request.OrganisationId,
                request.DomainName,
                request.PrivateKey,
                request.DnsSelector,
                request.AgentOrUserIdentifier,
                request.ApplicableDomainNameList,
                this.clock.Now());

            var dkimSetting = this.dkimSettingRepository.Insert(dkimSettings);
            await this.dkimSettingRepository.SaveChangesAsync();
            await this.organisationSystemEventEmitter.CreateAndEmitModifiedSystemEvent(request.TenantId, request.OrganisationId);
            return dkimSetting;
        }

        private void Validate(CreateDkimSettingsCommand request)
        {
            var organisation = this.organisationReadModelRepository.Get(request.TenantId, request.OrganisationId);
            if (organisation == null)
            {
                throw new ErrorException(Errors.General.NotFound("Organisation", request.OrganisationId));
            }

            var tenant = this.tenantRepository.GetTenantById(request.TenantId);
            if (tenant == null)
            {
                throw new ErrorException(Errors.General.NotFound("Tenant", request.TenantId));
            }

            var settings = this.dkimSettingRepository.GetDkimSettingsbyOrganisationIdAndDomainName(request.TenantId, request.OrganisationId, request.DomainName);
            if (settings.Any())
            {
                throw new ErrorException(Errors.Organisation.DuplicateDomainNameWithInOrganisation(request.OrganisationId, organisation.Name, request.DomainName));
            }
        }
    }
}
