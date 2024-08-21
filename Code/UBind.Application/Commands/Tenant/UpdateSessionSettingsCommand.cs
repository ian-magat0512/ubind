// <copyright file="UpdateSessionSettingsCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Tenant;

using UBind.Domain;
using UBind.Domain.Patterns.Cqrs;

public class UpdateSessionSettingsCommand : ICommand<Tenant>
{
    public UpdateSessionSettingsCommand(
        Guid tenantId,
        SessionExpiryMode sessionExpiryMode,
        string idleTimeoutPeriodType,
        string fixedLengthTimeoutInPeriodType,
        long idleTimeout,
        long fixedLengthTimeout)
    {
        this.TenantId = tenantId;
        this.SessionExpiryMode = sessionExpiryMode;
        this.IdleTimeoutPeriodType = idleTimeoutPeriodType;
        this.FixedLengthTimeoutInPeriodType = fixedLengthTimeoutInPeriodType;
        this.IdleTimeout = idleTimeout;
        this.FixedLengthTimeout = fixedLengthTimeout;
    }

    public Guid TenantId { get; }

    public SessionExpiryMode SessionExpiryMode { get; }

    public string IdleTimeoutPeriodType { get; }

    public string FixedLengthTimeoutInPeriodType { get; }

    public long IdleTimeout { get; }

    public long FixedLengthTimeout { get; }
}
