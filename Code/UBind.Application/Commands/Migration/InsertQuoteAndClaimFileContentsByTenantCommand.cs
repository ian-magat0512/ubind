// <copyright file="InsertQuoteAndClaimFileContentsByTenantCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Migration
{
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command for inserting quote and claim file assets into file contents for every tenant.
    /// This is needed to ensure that each tenant has a unique file content row
    /// handling same file contents used by multiple tenants in migration.
    /// Create new rows for different tenants
    /// and updating the associated EventRecordWithGuidIds.EventJson with the new FileContentId.
    /// This should run automatically during startup.
    /// We can remove this from the codebase after it's been run, but the code is idempotent so there's no harm in retaining it.
    /// </summary>
    public class InsertQuoteAndClaimFileContentsByTenantCommand : ICommand
    {
    }
}
