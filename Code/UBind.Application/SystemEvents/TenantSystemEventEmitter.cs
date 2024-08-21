// <copyright file="TenantSystemEventEmitter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.SystemEvents;

using NodaTime;
using UBind.Domain;
using UBind.Domain.Events;
using UBind.Domain.Services.SystemEvents;

/// <summary>
/// This class raises SystemEvents so that automations can listen
/// and respond to when something changes on a tenant.
/// </summary>
public class TenantSystemEventEmitter : ITenantSystemEventEmitter
{
    private readonly ISystemEventService systemEventService;
    private readonly IClock clock;
    private readonly IEventPayloadFactory payloadFactory;

    public TenantSystemEventEmitter(
        ISystemEventService systemEventService,
        IEventPayloadFactory payloadFactory,
        IClock clock)
    {
        this.systemEventService = systemEventService;
        this.payloadFactory = payloadFactory;
        this.clock = clock;
    }

    public async Task CreateAndEmitSystemEvent(Guid tenantId, SystemEventType eventType)
    {
        var payload = await this.payloadFactory.CreateTenantEventPayload(tenantId);
        var performingUserId = payload.PerformingUser?.Id;
        var systemEvent = SystemEvent.CreateWithPayload(
            tenantId,
            default,
            Domain.DeploymentEnvironment.None,
            eventType,
            payload,
            this.clock.GetCurrentInstant());
        if (performingUserId.HasValue)
        {
            systemEvent.AddRelationshipToEntity(
                    RelationshipType.EventPerformingUser, EntityType.User, performingUserId.Value);
        }

        this.systemEventService.BackgroundPersistAndEmit(new List<SystemEvent> { systemEvent });
    }
}
