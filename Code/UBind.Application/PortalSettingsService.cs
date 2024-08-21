// <copyright file="PortalSettingsService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.Repositories;

    /// <inheritdoc/>
    public class PortalSettingsService : IPortalSettingsService
    {
        private readonly IPortalReadModelRepository portalRepository;
        private readonly ITenantRepository tenantRepository;
        private readonly IPortalSettingRepository portalSettingRepository;
        private readonly IClock clock;
        private readonly ICachingResolver cachingResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="PortalSettingsService"/> class.
        /// </summary>
        /// <param name="portalRepository">The portal repository.</param>
        /// <param name="tenantRepository">The tenant repository.</param>
        /// <param name="portalSettingRepository">The portal setting repository.</param>
        /// <param name="clock">Clock for obtaining current time.</param>
        public PortalSettingsService(
            IPortalReadModelRepository portalRepository,
            ITenantRepository tenantRepository,
            IPortalSettingRepository portalSettingRepository,
            ICachingResolver cachingResolver,
            IClock clock)
        {
            Contract.Assert(portalRepository != null);

            this.portalRepository = portalRepository;
            this.tenantRepository = tenantRepository;
            this.portalSettingRepository = portalSettingRepository;
            this.clock = clock;
            this.cachingResolver = cachingResolver;
        }

        /// <inheritdoc/>
        public PortalReadModel UpdatePortalSettings(Guid tenantId, Guid portalId, Guid settingId, bool value)
        {
            var portal = this.portalRepository.GetPortalById(tenantId, portalId);
            if (portal == null)
            {
                throw new ErrorException(Errors.General.NotFound("portal", portalId));
            }

            var portalSettings = new PortalSettingDetails(value, portal, this.clock.GetCurrentInstant());
            this.portalSettingRepository.Update(settingId, portalSettings);
            this.portalSettingRepository.SaveChanges();
            return portal;
        }

        /// <inheritdoc/>
        public IReadOnlyList<PortalSettings> GetPortalSettings(Guid tenantId, Guid portalId)
        {
            var portal = this.portalRepository.GetPortalById(tenantId, portalId);
            if (portal == null)
            {
                throw new ErrorException(Errors.General.NotFound("portal", portalId));
            }

            // TODO: This has not been implemented. It should allow filtering on tenantId and portal Id.
            return this.portalSettingRepository.GetAllPortalSettings();
        }
    }
}
