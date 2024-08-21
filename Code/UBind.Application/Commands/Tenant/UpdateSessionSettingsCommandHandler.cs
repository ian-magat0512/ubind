// <copyright file="UpdateSessionSettingsCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Tenant;

using NodaTime;
using UBind.Domain;
using UBind.Domain.Events;
using UBind.Domain.Helpers;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Repositories;

public class UpdateSessionSettingsCommandHandler : ICommandHandler<UpdateSessionSettingsCommand, Tenant>
{
    private readonly IClock clock;
    private readonly ITenantRepository tenantRepository;
    private readonly ICachingResolver cachingResolver;
    private readonly ITenantSystemEventEmitter tenantSystemEventEmitter;

    public UpdateSessionSettingsCommandHandler(
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

    public async Task<Tenant> Handle(UpdateSessionSettingsCommand command, CancellationToken cancellationToken)
    {
        Tenant tenant = this.tenantRepository.GetTenantById(command.TenantId);
        EntityHelper.ThrowIfNotFound(tenant, command.TenantId, "tenant");

        var idleTimeoutInMilliseconds
            = this.TimeoutToMilliseconds(command.IdleTimeoutPeriodType, command.IdleTimeout);
        var fixedLengthTimeoutInMilliseconds
            = this.TimeoutToMilliseconds(command.FixedLengthTimeoutInPeriodType, command.FixedLengthTimeout);

        // retain previous tenant details while updating it with session details
        var details = new TenantDetails(tenant.Details, this.clock.GetCurrentInstant());
        details.UpdateSession(
            command.SessionExpiryMode,
            command.IdleTimeoutPeriodType,
            idleTimeoutInMilliseconds,
            command.FixedLengthTimeoutInPeriodType,
            fixedLengthTimeoutInMilliseconds);
        tenant.Update(details);
        this.tenantRepository.SaveChanges();

        this.cachingResolver.RemoveCachedTenants(
            tenant.Id,
            new List<string> { tenant.Details.Alias });

        await this.tenantSystemEventEmitter.CreateAndEmitSystemEvent(tenant.Id, SystemEventType.TenantModified);
        return tenant;
    }

    private long TimeoutToMilliseconds(string period, long idleTimeOut)
    {
        switch (period)
        {
            case TokenSessionPeriodType.Day:
                return idleTimeOut * 86400000;
            case TokenSessionPeriodType.Hour:
                return idleTimeOut * 3600000;
            case TokenSessionPeriodType.Minute:
                return idleTimeOut * 60000;
            default:
                return 0;
        }
    }
}
