// <copyright file="UpdatePasswordExpirySettingsCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Tenant
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Events;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    public class UpdatePasswordExpirySettingsCommandHandler : ICommandHandler<UpdatePasswordExpirySettingsCommand, Tenant>
    {
        private readonly IClock clock;
        private readonly ITenantRepository tenantRepository;
        private readonly ICachingResolver cachingResolver;
        private readonly ITenantSystemEventEmitter tenantSystemEventEmitter;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdatePasswordExpirySettingsCommandHandler"/> class.
        /// </summary>
        /// <param name="tenantRepository">The repository of the tenant.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        /// <param name="clock">Represents the clock which can return the time as <see cref="Instant"/>.</param>
        public UpdatePasswordExpirySettingsCommandHandler(
            ITenantRepository tenantRepository,
            ICachingResolver cachingResolver,
            ITenantSystemEventEmitter tenantSystemEventEmitter,
            IClock clock)
        {
            this.tenantRepository = tenantRepository;
            this.cachingResolver = cachingResolver;
            this.clock = clock;
            this.tenantSystemEventEmitter = tenantSystemEventEmitter;
        }

        /// <inheritdoc/>
        public async Task<Tenant> Handle(UpdatePasswordExpirySettingsCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var tenant = this.tenantRepository.GetTenantById(request.TenantId);
            tenant = EntityHelper.ThrowIfNotFound(tenant, request.TenantId);

            // retain previous tenant details while updating it with password expiry details
            var details = new TenantDetails(tenant.Details, this.clock.GetCurrentInstant());
            details.UpdatePasswordExpiry(request.PasswordExpiryEnabled, request.MaxPasswordAgeDays);
            tenant.Update(details);
            this.tenantRepository.SaveChanges();
            this.cachingResolver.RemoveCachedTenants(
                tenant.Id,
                new List<string> { tenant.Details.Alias });
            await this.tenantSystemEventEmitter.CreateAndEmitSystemEvent(tenant.Id, SystemEventType.TenantModified);
            return tenant;
        }
    }
}
