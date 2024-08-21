// <copyright file="DeleteAdditionalPropertyDefinitionCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.AdditionalPropertyDefinition;

using System;
using NodaTime;
using UBind.Domain.Patterns.Cqrs;

/// <summary>
/// Command in marking the additional property as deleted or not.
/// </summary>
public class DeleteAdditionalPropertyDefinitionCommand : ICommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteAdditionalPropertyDefinitionCommand"/> class.
    /// </summary>
    /// <param name="tenantId">The Id of the tenant.</param>
    /// <param name="additionalPropertyId">The Id of the additional property to be soft deleted.</param>
    /// <param name="performingUserId">The userid of the performing person.</param>
    /// <param name="instant">The timestamp.</param>
    public DeleteAdditionalPropertyDefinitionCommand(
        Guid tenantId,
        Guid additionalPropertyId,
        Guid? performingUserId,
        Instant instant)
    {
        this.TenantId = tenantId;
        this.AdditionalPropertyId = additionalPropertyId;
        this.PerformingUserId = performingUserId;
        this.Instant = instant;
    }

    public Guid TenantId { get; }

    /// <summary>
    /// Gets the value of additional property id.
    /// </summary>
    public Guid AdditionalPropertyId { get; }

    /// <summary>
    /// Gets the value of the user id of the performer.
    /// </summary>
    public Guid? PerformingUserId { get; }

    /// <summary>
    /// Gets the value of the timestamp.
    /// </summary>
    public Instant Instant { get; }
}
