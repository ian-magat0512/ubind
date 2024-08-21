// <copyright file="TinyUrl.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Models;

using System;
using NodaTime;

public class TinyUrl : Entity<Guid>
{
    public TinyUrl(
        Guid tenantId,
        DeploymentEnvironment environment,
        string redirectUrl,
        string token,
        long sequenceNumber,
        Instant createdTimestamp)
        : base(Guid.NewGuid(), createdTimestamp)
    {
        this.TenantId = tenantId;
        this.Environment = environment;
        this.RedirectUrl = redirectUrl;
        this.Token = token;
        this.SequenceNumber = sequenceNumber;
    }

    /// <summary>
    /// Parameterless constructor for Entity Framework.
    /// </summary>
    private TinyUrl()
        : base(default, default)
    {
    }

    public long SequenceNumber { get; private set; }

    public string Token { get; private set; }

    public string RedirectUrl { get; private set; }

    public Guid TenantId { get; private set; }

    public DeploymentEnvironment Environment { get; private set; }
}
